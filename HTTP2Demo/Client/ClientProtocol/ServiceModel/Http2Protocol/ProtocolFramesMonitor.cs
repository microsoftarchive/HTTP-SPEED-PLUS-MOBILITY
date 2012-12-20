//-----------------------------------------------------------------------
// <copyright file="ProtocolFramesMonitor.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.Http2Protocol
{
    using System.ServiceModel.Http2Protocol.ProtocolFrames;

    /// <summary>
    /// Class that can be used to monitor frames on session.
    /// </summary>
    public class ProtocolFramesMonitor : IDisposable
    {
        /// <summary>
        /// Protocol Session.
        /// </summary>
        private readonly ProtocolSession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolFramesMonitor"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        public ProtocolFramesMonitor(ProtocolSession session, Func<BaseFrame, bool> filter)
        {
            this.session = session;
            this.Filter = filter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolFramesMonitor"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public ProtocolFramesMonitor(ProtocolSession session)
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
