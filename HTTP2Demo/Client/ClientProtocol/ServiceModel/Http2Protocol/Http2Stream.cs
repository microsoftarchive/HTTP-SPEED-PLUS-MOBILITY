//-----------------------------------------------------------------------
// <copyright file="Http2Stream.cs" company="Microsoft Open Technologies, Inc.">
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
    /// <summary>
    /// Http2 stream that runs inside Http2Protocol.
    /// </summary>
    public class Http2Stream
    {
        #region Fields

        /// <summary>
        /// internal Http2 protocol reference
        /// </summary>
        private readonly Http2Protocol protocol;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Http2Stream"/> class.
        /// </summary>
        /// <param name="id">The stream id.</param>
        /// <param name="session">The stream session.</param>
        public Http2Stream(int id, ProtocolSession session)
        {
            this.StreamId = id;
            this.Session = session;
            this.Headers = new ProtocolHeaders();
            this.protocol = session.Protocol;

            this.CurrentWindowBalanceFromServer = session.CurrentWindowBalanceFromServer;

            this.protocol.OnStreamFrame += this.OnProtocolData;
            this.protocol.OnStreamError += this.OnStreamError;
            this.protocol.OnSessionFrame += this.OnSessionFrame;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when data received on the stream.
        /// </summary>
        public event EventHandler<StreamDataEventArgs> OnDataReceived;

        /// <summary>
        /// Occurs when RST received on the stream.
        /// </summary>
        public event EventHandler<RSTEventArgs> OnRSTReceived;

        /// <summary>
        /// Occurs when stream is closed.
        /// </summary>
        public event EventHandler<RSTEventArgs> OnClose;

        /// <summary>
        /// Occurs when headers received on the stream.
        /// </summary>
        public event EventHandler<HeadersEventArgs> OnHeadersReceived;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the stream id.
        /// </summary>
        public int StreamId { get; private set; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        public ProtocolSession Session { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public Http2StreamState State { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the stream is closed.
        /// </summary>
        public bool IsClosed 
        { 
            get 
            { 
                return this.State == Http2StreamState.Closed; 
            } 
        }

        /// <summary>
        /// Gets the stream request headers.
        /// </summary>
        public ProtocolHeaders Headers { get; internal set; }

        /// <summary>
        /// Gets or sets the initial CurrentWindowBalanceFromServer for current stream
        /// </summary>
        /// <value>
        /// The current window balance from server.
        /// </value>
        public Int64 CurrentWindowBalanceFromServer { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the window balance.
        /// </summary>
        /// <param name="deltaWindowSize">Size of the delta window.</param>
        public void UpdateWindowBalance(Int64 deltaWindowSize)
        {
            this.CurrentWindowBalanceFromServer += deltaWindowSize;
        }

        /// <summary>
        /// Closes the stream with specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void Close(StatusCode reason)
        {
            this.State = Http2StreamState.Closed;

            this.protocol.OnStreamFrame -= this.OnProtocolData;
            this.protocol.OnStreamError -= this.OnStreamError;

            if (reason != StatusCode.Success)
            {
                this.SendRST(reason);
            }

            if (this.OnClose != null)
            {
                this.OnClose(this, new RSTEventArgs(this, reason));
            }
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            this.Close(StatusCode.Success);
        }

        /// <summary>
        /// Sends the data to the stream.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="isFin">if set to <c>true</c> than this stream will be half-closed.</param>
        public void SendData(ProtocolData data, bool isFin)
        {
            if (this.State == Http2StreamState.Opened)
            {
                ProtocolData dataFrame;
                if (Session.IsFlowControlEnabled)
                {
                    //Session.CurrentWindowBalanceToServer -= data.Data.Length;
                    dataFrame = new ProtocolData(data.Data);
                }
                else
                {
                    dataFrame = new ProtocolData(data.Data);
                }

                this.protocol.SendData(this, dataFrame, isFin);
            }
            else
            {
                throw new InvalidOperationException("Trying to send data into closed stream!");
            }
        }

        /// <summary>
        /// Sends the headers to the stream.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">if set to <c>true</c> than this stream will be half-closed.</param>
        public void SendHeaders(ProtocolHeaders headers, bool isFin)
        {
            this.Headers.Merge(headers);
            this.protocol.SendHeaders(this, headers, isFin);
        }

        /// <summary>
        /// Sends the RST to the stream.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void SendRST(StatusCode reason)
        {
            this.protocol.SendRST(this, reason);
        }

        /// <summary>
        /// Send the WindowUpdate frame to the stream.
        /// </summary>
        /// <param name="windowAddition"></param>
        public void SendWindowUpdate(Int64 windowAddition)
        {
            //Session.CurrentWindowBalanceFromServer += windowAddition;
            this.protocol.SendWindowUpdate(this, windowAddition);
        }

        /// <summary>
        /// Opens the stream.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">The Final flag.</param>
        internal void OpenClient(ProtocolHeaders headers, bool isFin)
        {
            this.Headers.Merge(headers);
            if (isFin)
            {
                this.State = Http2StreamState.HalfClosed;
            }

            this.protocol.SendSynStream(this, headers, isFin);
        }

        internal void OpenServer(ProtocolHeaders headers, bool isFin)
        {
            this.Headers.Merge(headers);
            this.State = Http2StreamState.Opened;
            //if (isFin)
            //{
            //    this.State = Http2StreamState.HalfClosed;
            //}
        }

        /// <summary>
        /// Event handler for Session frame.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSessionFrame(object sender, ControlFrameEventArgs e)
        {
            if (e.Frame.StreamId == this.StreamId
                && e.Frame.IsFinal
                && this.State == Http2StreamState.HalfClosed)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Event handler for Stream error.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnStreamError(object sender, StreamErrorEventArgs e)
        {
            if (e.Stream == this)
            {
                if (e.Exeption is ProtocolExeption)
                {
                    this.Close(((ProtocolExeption)e.Exeption).StatusCode);
                }
                else
                {
                    this.Close(StatusCode.InternalError);
                }
            }
        }

        /// <summary>
        /// Event handler for protocol data.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnProtocolData(object sender, StreamEventArgs e)
        {
            if (e.Stream == this)
            {
                if (Http2StreamState.Closed == e.Stream.State)
                {
                    this.Close(StatusCode.ProtocolError);
                    return;
                }

                if (e is HeadersEventArgs)
                {
                    var args = (HeadersEventArgs)e;
                    this.Headers.Merge(args.Headers);

                    if (this.OnHeadersReceived != null)
                    {
                        this.OnHeadersReceived(this, args);
                    }
                }
                else if (e is StreamDataEventArgs)
                {
                    StreamDataEventArgs arg = e as StreamDataEventArgs;
                    if (arg.IsFin && this.State == Http2StreamState.HalfClosed)
                    {
                        this.Close();
                    }

                    if (this.OnDataReceived != null)
                    {
                        StreamDataEventArgs args = (StreamDataEventArgs)e;
                        this.OnDataReceived(this, e as StreamDataEventArgs);

                        //TODO incomment when server will be able to handle windowUpdate
                        /*if (Session.IsFlowControlEnabled)
                        {
                            CurrentWindowBalanceFromServer -= args.Data.Data.Length;
                            if (CurrentWindowBalanceFromServer <= 0)
                                this.protocol.SendWindowUpdate(this, Session.CurrentWindowBalanceToServer);
                        }*/
                    }
                }
                else if (e is RSTEventArgs)
                {
                    if (this.OnRSTReceived != null)
                    {
                        this.OnRSTReceived(this, e as RSTEventArgs);
                    }
                }
            }
        }

        #endregion
    }
}
