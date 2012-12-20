//-----------------------------------------------------------------------
// <copyright file="ProtocolSession.cs" company="Microsoft Open Technologies, Inc.">
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

using ClientProtocol.ServiceModel.Http2Protocol.MessageProcessing;

namespace System.ServiceModel.Http2Protocol
{
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Http2Protocol.ProtocolFrames;

    /// <summary>
    /// Speed+Mobility Session
    /// </summary>
    public sealed class ProtocolSession : IDisposable, IStreamStore
    {
        /// <summary>
        /// Is a special value that designates "infinite".
        /// </summary>
        public readonly UInt32 MaxWindowBalance = 0xffffffff;

        #region Fields

        /// <summary>
        /// List of streams.
        /// </summary>
        private readonly List<Http2Stream> streams;

        /// <summary>
        /// list of closed stream.
        /// </summary>
        private readonly List<Http2Stream> closedStreams;

        /// <summary>
        /// WS protocol.
        /// </summary>
        private readonly Http2Protocol protocol;

        /// <summary>
        /// Flag server.
        /// </summary>
        private readonly bool isServer;

        /// <summary>
        /// Last good stream id.
        /// </summary>
        private int lastSeenStreamId;
        
        /// <summary>
        /// Indicates is flow control enabled.
        /// </summary>
        private readonly bool isFlowControlEnabled;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolSession"/> class.
        /// </summary>
        /// <param name="uri">the URI.</param>
        public ProtocolSession(Uri uri)
            : this(uri, false, null)
        {
        }

    	/// <summary>
    	/// Initializes a new instance of the <see cref="ProtocolSession"/> class.
    	/// </summary>
    	/// <param name="uri">the URI.</param>
    	/// <param name="isServer">the Server flag.</param>
    	/// <param name="options">Session options.</param>
    	public ProtocolSession(Uri uri, bool isServer, ProtocolOptions options)
        {
            this.isFlowControlEnabled = options.IsFlowControl;
    	    this.streams = new List<Http2Stream>();
			this.closedStreams = new List<Http2Stream>();
			this.isServer = isServer;

    	    this.CurrentWindowBalanceToServer = 512;
    	    this.CurrentWindowBalanceFromServer = 256;

            this.protocol = new Http2Protocol(uri, this, options);
            this.protocol.OnSessionFrame += this.OnSessionFrame;
            this.protocol.OnClose += this.OnProtocolClose;
            this.protocol.OnOpen += this.OnProtocolOpen;
            this.protocol.OnError += this.OnProtocolError;
            this.protocol.OnPing += this.OnProtocolPing;

			if (options.UseCompression)
			{
				this.Protocol.SetProcessors(new List<IMessageProcessor> { new CompressionProcessor() });				
			}
        }
        #endregion

        #region Events

        /// <summary>
        /// Occurs when session is opened.
        /// </summary>
        public event EventHandler<EventArgs> OnOpen;

        /// <summary>
        /// Occurs when session is closed.
        /// </summary>
        public event EventHandler<CloseFrameEventArgs> OnClose;

        /// <summary>
        /// Occurs when session errors.
        /// </summary>
        public event EventHandler<ProtocolErrorEventArgs> OnError;

        /// <summary>
        /// Occurs when ping is received.
        /// </summary>
        public event EventHandler<EventArgs> OnPing;

        /// <summary>
        /// Occurs when stream is opened.
        /// </summary>
        public event EventHandler<StreamEventArgs> OnStreamOpened;

        /// <summary>
        /// Occurs when stream is closed.
        /// </summary>
        public event EventHandler<RSTEventArgs> OnStreamClosed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the protocol.
        /// </summary>
		internal Http2Protocol Protocol 
        { 
            get 
            { 
                return this.protocol; 
            } 
        }

        /// <summary>
        /// Gets a value indicating whether this instance is server.
        /// </summary>
        public bool IsServer 
        { 
            get 
            { 
                return this.isServer; 
            } 
        }

        /// <summary>
        /// Gets the session state.
        /// </summary>
        public ProtocolSessionState State { get; private set; }

        /// <summary>
        /// Gets the streams collection.
        /// </summary>
        public ICollection<Http2Stream> Streams 
        { 
            get 
            { 
                return this.streams; 
            } 
        }

        /// <summary>
        /// Gets the URI.
        /// </summary>
		public Uri Uri
		{
			get
			{
				return this.protocol.Uri;
			}
		}

        /// <summary>
        /// Flag flow control.
        /// </summary>
        public bool IsFlowControlEnabled { get { return isFlowControlEnabled; } }


        /// <summary>
        /// Gets or sets the initial CurrentWindowBalanceFromServer for new streams
        /// </summary>
        /// <value>
        /// The current window balance from server.
        /// </value>
        public Int64 CurrentWindowBalanceFromServer { get; set; }

        /// <summary>
        /// Gets or sets the initial CurrentWindowBalanceToServer for new streams
        /// </summary>
        /// <value>
        /// The current window balance from server.
        /// </value>
        public Int64 CurrentWindowBalanceToServer { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Opens the sessions.
        /// </summary>
        public void Open()
        {
            if (this.State == ProtocolSessionState.Created)
            {
                this.protocol.Connect();
            }
            else
            {
                throw new InvalidOperationException("Session already " + this.State);
            }
        }

        /// <summary>
        /// Ends the sessions.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void End(StatusCode reason)
        {
            if (this.State != ProtocolSessionState.Closed)
            {
                this.protocol.Close(reason, this.lastSeenStreamId);
            }
            this.State = ProtocolSessionState.Closed;
        }

        /// <summary>
        /// Ends the session.
        /// </summary>
        public void End()
        {
            this.End(StatusCode.Success);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        public void Dispose()
        {
            this.End();
            this.protocol.Dispose();
        }

        /// <summary>
        /// Sends ping.
        /// </summary>
        public void Ping(int streamId)
        {
            this.protocol.SendPing(streamId);
        }

        /// <summary>
        /// Opens the stream in current session.
        /// </summary>
        /// <param name="headers">The S+M headers.</param>
        /// <param name="isFinal">the final flag.</param>
        /// <returns>The Stream.</returns>
        public Http2Stream OpenStream(ProtocolHeaders headers, bool isFinal)
        {
            int newId = this.GenerateNewStreamId();
            return this.OpenStream(newId, headers, isFinal);
        }

        /// <summary>
        /// returns stream by id.
        /// </summary>
        /// <param name="streamId">Stream Id.</param>
        /// <returns>The Stream.</returns>
        public Http2Stream GetStreamById(int streamId)
        {
            return this.streams.FirstOrDefault(s => s.StreamId == streamId);
        }

        /// <summary>
        /// Opens the stream in current session.
        /// </summary>
        /// <param name="id">the stream id.</param>
        /// <param name="headers">The S+M headers.</param>
        /// <param name="isFinal">the final flag.</param>
        /// <returns>The Stream.</returns>
        private Http2Stream OpenStream(int id, ProtocolHeaders headers, bool isFinal)
        {
            if (id <= this.lastSeenStreamId)
            {
                this.End(StatusCode.ProtocolError);
                return null;
            }

            this.lastSeenStreamId = id;

            // don't have to wait for stream opening
            Http2Stream stream = new Http2Stream(id, this);

            this.streams.Add(stream);

            stream.OnClose += this.OnCloseStream;
            stream.Open(headers, isFinal);

            if (this.OnStreamOpened != null)
            {
                this.OnStreamOpened(this, new StreamEventArgs(stream));
            }

            return stream;
        }

        /// <summary>
        /// Event handler for Ping.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnProtocolPing(object sender, EventArgs e)
        {
            if (this.OnPing != null)
            {
                this.OnPing(this, e);
            }
        }


        /// <summary>
        /// Event handler for error.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnProtocolError(object sender, ProtocolErrorEventArgs e)
        {
            this.End(StatusCode.ProtocolError);
            if (this.OnError != null)
            {
                this.OnError(this, e);
            }
        }

        /// <summary>
        /// Event handler for open.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnProtocolOpen(object sender, EventArgs e)
        {
            this.State = ProtocolSessionState.Opened;
            if (this.OnOpen != null)
            {
                this.OnOpen(this, null);
            }
        }

        /// <summary>
        /// Event handler for session frame.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnSessionFrame(object sender, ControlFrameEventArgs e)
        {
            switch (e.Frame.Type)
            {
                case FrameType.SynStream:
                    OpenStream(e.Frame.StreamId, e.Frame.Headers, e.Frame.IsFinal);                    
                    break;
                case FrameType.SynReply:
                    Http2Stream stream = this.GetStreamById(e.Frame.StreamId);
                    if (stream != null && stream.State != Http2StreamState.Closed && stream.State != Http2StreamState.HalfClosed)
                    {
                        stream.State = Http2StreamState.Accepted;
                    }
                    break;
                case FrameType.Settings:
                    if (e.Frame.SettingsHeaders != null)
                    {
                        if (e.Frame.SettingsHeaders.ContainsKey((int)(SettingsIds.SETTINGS_INITIAL_WINDOW_SIZE)))
                            CurrentWindowBalanceFromServer =
                                e.Frame.SettingsHeaders[(int)(SettingsIds.SETTINGS_INITIAL_WINDOW_SIZE)];
                    }
                    break;
            }
        }

        /// <summary>
        /// Event handler for close.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnProtocolClose(object sender, CloseFrameEventArgs e)
        {
            this.State = ProtocolSessionState.Closed;
            if (this.OnClose != null)
            {
                this.OnClose(this, e);
            }
        }

        /// <summary>
        /// Event handler for close stream.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnCloseStream(object sender, RSTEventArgs e)
        {
            Http2Stream stream = sender as Http2Stream;
            this.streams.Remove(stream);
            this.closedStreams.Add(stream);

            if (this.OnStreamClosed != null)
            {
                this.OnStreamClosed(this, e);
            }
        }

        /// <summary>
        /// returns new stream id.
        /// </summary>
        /// <returns>The Stream id.</returns>
        private int GenerateNewStreamId()
        {
            // streams initiated by client must be odd
            // streams initiated by server must be even
            int newStreamId = this.lastSeenStreamId + 1;
            if (this.isServer == (newStreamId % 2 == 1))
            {
                ++newStreamId;
            }

            return newStreamId;
        }

        #endregion
    }
}
