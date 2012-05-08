//-----------------------------------------------------------------------
// <copyright file="ProtocolMonitor.cs" company="Microsoft Open Technologies, Inc.">
//
// ---------------------------------------
// HTTPbis
// Copyright Microsoft Open Technologies, Inc.
// ---------------------------------------
// Microsoft Reference Source License.
// 
// This license governs use of the accompanying software. 
// If you use the software, you accept this license. 
// If you do not accept the license, do not use the software.
// 
// 1. Definitions
// 
// The terms "reproduce," "reproduction," and "distribution" have the same meaning here 
// as under U.S. copyright law.
// "You" means the licensee of the software.
// "Your company" means the company you worked for when you downloaded the software.
// "Reference use" means use of the software within your company as a reference, in read // only form, 
// for the sole purposes of debugging your products, maintaining your products, 
// or enhancing the interoperability of your products with the software, 
// and specifically excludes the right to distribute the software outside of your company.
// "Licensed patents" means any Licensor patent claims which read directly on the software 
// as distributed by the Licensor under this license. 
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free copyright license to reproduce the software for reference use.
// (B) Patent Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free patent license under licensed patents for reference use. 
// 
// 3. Limitations
// (A) No Trademark License- This license does not grant you any rights 
// to use the Licensor’s name, logo, or trademarks.
// (B) If you begin patent litigation against the Licensor over patents that you think may apply 
// to the software (including a cross-claim or counterclaim in a lawsuit), your license 
// to the software ends automatically. 
// (C) The software is licensed "as-is." You bear the risk of using it. 
// The Licensor gives no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws 
// which this license cannot change. To the extent permitted under your local laws, 
// the Licensor excludes the implied warranties of merchantability, 
// fitness for a particular purpose and non-infringement. 
// 
// -----------------End of License---------
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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolMonitor"/> class.
        /// </summary>
        /// <param name="session">The sm session instance.</param>
        public ProtocolMonitor(SMSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            this.totals = new ProtocolMonitorLog();
            this.session = session;
            this.exitEvent = new ManualResetEvent(false);
            this.statsByStream = new StreamStats();
            this.State = MonitorState.MonitorOff;
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
        ///  Reset statistics.
        /// </summary>
        public void Reset()
        {
            this.totals.Reset();
            this.statsByStream.Reset();
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
