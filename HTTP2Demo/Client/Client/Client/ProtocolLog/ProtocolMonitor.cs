//-----------------------------------------------------------------------
// <copyright file="ProtocolMonitor.cs" company="Microsoft Open Technologies, Inc.">
//
// The copyright in this software is being made available under the BSD License, included below. 
// This software may be subject to other third party and contributor rights, including patent rights, 
// and no such rights are granted under this license.
//
// Copyright (c) 2012, Microsoft Open Technologies, Inc. 
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice, 
//   this list of conditions and the following disclaimer in the documentation 
//   and/or other materials provided with the distribution.
// - Neither the name of Microsoft Open Technologies, Inc. nor the names of its contributors 
//   may be used to endorse or promote products derived from this software 
//   without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// </copyright>
//-----------------------------------------------------------------------
namespace Client.Benchmark
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Http2Protocol;
    using System.ServiceModel.Http2Protocol.ProtocolFrames;
    using System.Threading;
    using Client.HttpBenchmark;
    using Client.Utils;

    /// <summary>
    /// Monitor session log.
    /// </summary>
    public class ProtocolMonitor : IDisposable
    {
        #region Private fields

        /// <summary>
        /// Protocol session
        /// </summary>   
        private ProtocolSession session;

        /// <summary>
        /// frame totals
        /// </summary>   
        private readonly ProtocolMonitorLog totals;

        /// <summary>
        /// Exit event
        /// </summary>   
        private readonly ManualResetEvent exitEvent;

        /// <summary>
        /// Locking object to serialize inserts in dictionary
        /// </summary>
        [NonSerialized]
        private object exclusiveLock;

        /// <summary>
        /// Disposed flag
        /// </summary>   
        private bool disposed;

        /// <summary>
        /// Protocol session monitor
        /// </summary>   
        private ProtocolFramesMonitor sessionMonitor;

        /// <summary>
        /// Protocol stream monitor
        /// </summary>   
        private ProtocolFramesMonitor streamMonitor;

        /// <summary>
        /// Per stream statistics
        /// </summary>   
        private StreamStats statsByStream;

        /// <summary>
        /// Saved statistics
        /// </summary>   
        private Dictionary<int, StatisticsSnapshot> savedStats;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolMonitor"/> class.
        /// </summary>
        /// <param name="session">The protocol session instance.</param>
        public ProtocolMonitor()
        {
            this.exclusiveLock = new object();

            this.totals = new ProtocolMonitorLog();
            this.exitEvent = new ManualResetEvent(false);
            this.statsByStream = new StreamStats();
            this.State = MonitorState.MonitorOff;
            this.savedStats = new Dictionary<int, StatisticsSnapshot>();
            this.LastStartDate = DateTime.Now;
            this.LastEndDate = this.LastStartDate;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets Protocol Monitor state.
        /// </summary>
        public MonitorState State { get; private set; }

        /// <summary>
        /// Gets the totals.
        /// </summary>
        /// <returns>Benchmark totals.</returns>
        public ProtocolMonitorLog Totals
        {
            get { return this.totals; }
        }

        /// <summary>
        /// Gets or sets last HTTP log.
        /// </summary>
        public HttpTrafficLog LastHTTPLog { get; set; }

        /// <summary>
        /// Gets or sets date/time of start of operation
        /// </summary>
        public DateTime LastStartDate { get; set; }

        /// <summary>
        /// Gets or sets date/time of end of operation
        /// </summary>
        public DateTime LastEndDate { get; set; }

        /// <summary>
        /// Indicates whenever this monitor is attached to the session.
        /// </summary>
        public Boolean IsAttached { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts string to enum MonitorState 
        /// </summary>
        /// <param name="val">the string.</param>
        /// <returns>MonitorState enum.</returns>
        public static MonitorState StringToState(string val)
        {
            switch (val.ToUpper())
            {
                case "ON":
                    return MonitorState.MonitorOn;
                case "OFF":
                    return MonitorState.MonitorOff;
                case "RESET":
                    return MonitorState.MonitorReset;
                default:
                    return MonitorState.MonitorError;
            }
        }

        /// <summary>
        /// Gets the statistics per stream.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <returns>Stats per stream.</returns>
        public StreamInfo StreamStatistics(int streamId)
        {
            return this.statsByStream.GetStreamStatistics(streamId);
        }

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.exitEvent.Set();

                this.disposed = true;
                if (this.session != null)
                {
                    this.session.OnClose -= this.HandleCloseSession;
                }

                if (this.sessionMonitor != null)
                {
                    this.sessionMonitor.Dispose();
                }

                if (this.streamMonitor != null)
                {
                    this.streamMonitor.Dispose();
                }

                this.exitEvent.Dispose();
            }
        }

        /// <summary>
        ///  Execute benchmark.
        /// </summary>
        public void Attach(ProtocolSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            this.session = session;
            this.exitEvent.Reset();
            this.IsAttached = true;

            ThreadPool.QueueUserWorkItem(delegate
            {
                this.sessionMonitor = new ProtocolFramesMonitor(this.session, this.SessionFramesFilter);
                this.streamMonitor = new ProtocolFramesMonitor(this.session, f => !this.SessionFramesFilter(f)); 

                this.sessionMonitor.Attach();
                this.streamMonitor.Attach();

                this.session.OnClose += this.HandleCloseSession;
                this.session.OnStreamOpened += this.HandleStreamOpened;

                this.sessionMonitor.OnFrameReceived += this.SessionMonitorOnFrameReceived;
                this.sessionMonitor.OnFrameSent += this.SessionMonitorOnFrameSent;
                this.streamMonitor.OnFrameReceived += this.StreamMonitorOnFrameReceived;
                this.streamMonitor.OnFrameSent += this.StreamMonitorOnFrameSent;

                this.totals.BeginTime = DateTime.Now;

                this.State = MonitorState.MonitorOn;

                this.exitEvent.WaitOne();
            });
        }

        /// <summary>
        ///  Reset statistics for current session.
        /// </summary>
        public void Reset()
        {
            lock (this.exclusiveLock)
            {
                this.totals.Reset();
                this.statsByStream.Reset();
                this.LastHTTPLog = null;
                    this.LastStartDate = DateTime.Now;
                    this.LastEndDate = this.LastStartDate;
                    this.savedStats.Clear();
            }
        }

        /// <summary>
        ///  Saves current Protocol statistics into storage slot.
        /// </summary>
        /// <param name="slotId">The slot id.</param>
        /// <returns>True is operation succeeded.</returns>
        public bool SaveSlot(int slotId)
        {
            bool result = false;
            lock (this.exclusiveLock)
            {
                StatisticsSnapshot ss;
                if (!this.savedStats.TryGetValue(slotId, out ss))
                {
                    TimeSpan currentDuration = this.LastEndDate - this.LastStartDate;
                    if (this.LastHTTPLog != null)
                    {
                        ss = new StatisticsSnapshot(this.LastHTTPLog, currentDuration);
                        this.LastHTTPLog = null;
                    }
                    else
                    {
                        ss = new StatisticsSnapshot(this.totals, currentDuration);
                    }

                    this.savedStats.Add(slotId, ss);
                    this.totals.Reset();
                    this.statsByStream.Reset();
                    this.LastHTTPLog = null;
                    this.LastStartDate = DateTime.Now;
                    this.LastEndDate = this.LastStartDate;
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Create protocol monitoring report
        /// </summary>
        /// <param name="level">The verbosity level.</param>
        /// <returns>
        /// string representation of report
        /// </returns>
        public string GetMonitoringStats(Http2LoggerState level)
        {
            string result = string.Empty;

            // if we have zero saved result slots - just make totals
            if (this.savedStats.Count == 0)
            {
                // if HTTP log is avaialble, assume it is the last download operation
                if (this.LastHTTPLog != null)
                {
                    return this.LastHTTPLog.ToString();
                }
                else
                {
                    // output S+M operation
                    if (level < Http2LoggerState.VerboseLogging)
                    {
                        return this.totals.GetShortLog();
                    }
                    else
                    {
                        return this.totals.ToString();
                    }
                }
            }

            // if we have saved slots, we make new side-by-side format
            int maxLines = 0;
            foreach (KeyValuePair<int, StatisticsSnapshot> kvp in this.savedStats)
            {
                StatisticsSnapshot ss = kvp.Value;
                if (maxLines < ss.MaxTotalsLines)
                {
                    maxLines = ss.MaxTotalsLines;
                }

                ss.StartSxSOutput();
                result += string.Format("{0,-40}    ", ss.GetLogTitle());
            }

            result += "\r\n";

            for (int i = 0; i < maxLines; i++)
            {
                foreach (KeyValuePair<int, StatisticsSnapshot> kvp in this.savedStats)
                {
                    StatisticsSnapshot ss = kvp.Value;
                    result += string.Format("{0,-35}  ", ss.GetSxSLine());
                }

                result += "\r\n";
            }

            return result;
        }

        /// <summary>
        /// Selects frame based on type
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>
        /// TRUE if frame is Http2 control frame of SYNSTREAM or SYNREPLY type
        /// </returns>
        private bool SessionFramesFilter(BaseFrame frame)
        {
            var cntrlframe = frame as ControlFrame;
            return cntrlframe != null && (cntrlframe.Type == FrameType.SynStream || cntrlframe.Type == FrameType.SynReply);
        }

        /// <summary>
        /// Event handler for close session
        /// </summary>
        /// <param name="s">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleCloseSession(object s, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Event handler for open stream
        /// </summary>
        /// <param name="s">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleStreamOpened(object s, EventArgs e)
        {
            this.totals.TotalCountStreamsOpened++;
        }

        /// <summary>
        /// Event handler for frame sent
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void SessionMonitorOnFrameSent(object sender, FrameEventArgs e)
        {
            var frame = (ControlFrame)e.Frame;
            this.totals.TotalSizeFrameSent += frame.Length;
            this.totals.TotalCountControlFramesSent++;

            this.totals.FramesLog.Add(new FrameLogItem(frame));
            this.statsByStream.AddUpCount(frame.StreamId, frame.Length);
        }

        /// <summary>
        /// Event handler for frame received
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void SessionMonitorOnFrameReceived(object sender, FrameEventArgs e)
        {
            var frame = (ControlFrame)e.Frame;
            this.totals.TotalSizeControlFrameReceived += frame.Length;
            this.totals.TotalCountControlFramesReceived++;

            this.totals.FramesLog.Add(new FrameLogItem(frame));
            this.statsByStream.AddDownCount(frame.StreamId, frame.Length);
        }

        /// <summary>
        /// Event handler for stream frame sent
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void StreamMonitorOnFrameSent(object sender, FrameEventArgs e)
        {
            this.totals.TotalSizeFrameSent += e.Frame.Length;
            if (e.Frame is ControlFrame)
            {
                this.totals.TotalCountControlFramesSent++;
            }
            else
            {
                this.totals.TotalCountDataFramesSent++;                
            }

            this.totals.FramesLog.Add(new FrameLogItem(e.Frame));
            this.statsByStream.AddUpCount(e.Frame.StreamId, e.Frame.Length);
        }

        /// <summary>
        /// Event handler for stream frame received
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void StreamMonitorOnFrameReceived(object sender, FrameEventArgs e)
        {
            if (e.Frame is ControlFrame)
            {
                this.totals.TotalCountControlFramesReceived++;
                this.totals.TotalSizeControlFrameReceived += e.Frame.Length;
            }
            else
            {
                this.totals.TotalCountDataFramesReceived++;
                this.totals.TotalSizeDataFrameReceived += e.Frame.Length;
            }

            this.totals.FramesLog.Add(new FrameLogItem(e.Frame));
            this.statsByStream.AddDownCount(e.Frame.StreamId, e.Frame.Length);
        }

        #endregion
    }
}
