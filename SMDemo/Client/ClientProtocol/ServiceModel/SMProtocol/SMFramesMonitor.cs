//-----------------------------------------------------------------------
// <copyright file="SMFramesMonitor.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol
{
    using System.ServiceModel.SMProtocol.SMFrames;

    /// <summary>
    /// Class that can be used to monitor frames on session.
    /// </summary>
    public class SMFramesMonitor : IDisposable
    {
        /// <summary>
        /// SM Session.
        /// </summary>
        private readonly SMSession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="SMFramesMonitor"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        public SMFramesMonitor(SMSession session, Func<BaseFrame, bool> filter)
        {
            this.session = session;
            this.Filter = filter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMFramesMonitor"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public SMFramesMonitor(SMSession session)
            : this(session, null)
        {
        }

        /// <summary>
        /// Occurs when frame is sent.
        /// </summary>
        public event EventHandler<FrameEventArgs> OnFrameSent;

        /// <summary>
        /// Occurs when frame is received.
        /// </summary>
        public event EventHandler<FrameEventArgs> OnFrameReceived;

        /// <summary>
        /// Gets or sets the filter for frames.
        /// </summary>
        public Func<BaseFrame, bool> Filter { get; set; }

        /// <summary>
        /// Attaches monitr to session.
        /// </summary>
        public void Attach()
        {
            this.session.Protocol.OnFrameSent += this.ProtocolOnSendFrame;
            this.session.Protocol.OnFrameReceived += this.ProtocolOnFrameReceived;            
        }

        /// <summary>
        /// Detach monitor from session.
        /// </summary>
        public void Dispose()
        {
            this.session.Protocol.OnFrameSent -= this.ProtocolOnSendFrame;
            this.session.Protocol.OnFrameReceived -= this.ProtocolOnFrameReceived;
        }

        /// <summary>
        /// On send frame event handler.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void ProtocolOnSendFrame(object sender, FrameEventArgs e)
        {
            if (this.Filter == null || this.Filter(e.Frame))
            {
                if (this.OnFrameSent != null)
                {
                    this.OnFrameSent(this, e);
                }
            }
        }

        /// <summary>
        /// On receive frame event handler.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void ProtocolOnFrameReceived(object sender, FrameEventArgs e)
        {
            if (this.Filter == null || this.Filter(e.Frame))
            {
                if (this.OnFrameReceived != null)
                {
                    this.OnFrameReceived(this, e);
                }
            }
        }
    }
}
