//-----------------------------------------------------------------------
// <copyright file="ProtocolMonitor.cs" company="Microsoft Open Technologies, Inc.">
//
// Copyright 2012 Microsoft Open Technologies, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
namespace Client.Benchmark
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.SMProtocol;
    using System.ServiceModel.SMProtocol.SMFrames;
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
        /// SM session
        /// </summary>   
        private readonly SMSession session;

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
        /// SM session monitor
        /// </summary>   
        private SMFramesMonitor sessionMonitor;

        /// <summary>
        /// SM stream monitor
        /// </summary>   
        private SMFramesMonitor streamMonitor;

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
        /// <param name="session">The sm session instance.</param>
        public ProtocolMonitor(SMSession session)
        {
            this.exclusiveLock = new object();

            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            this.totals = new ProtocolMonitorLog();
            this.session = session;
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
                this.session.OnClose -= this.HandleCloseSession;

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
        public void Attach()
        {
            this.exitEvent.Reset();

            ThreadPool.QueueUserWorkItem(delegate
            {
                this.sessionMonitor = new SMFramesMonitor(this.session, this.SessionFramesFilter);
                this.streamMonitor = new SMFramesMonitor(this.session, f => !this.SessionFramesFilter(f)); 

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
        ///  Saves current SM statistics into storage slot.
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
        public string GetMonitoringStats(SMLoggerState level)
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
                    if (level < SMLoggerState.VerboseLogging)
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
        /// TRUE if frame is SM control frame of SYNSTREAM or SYNREPLY type
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
