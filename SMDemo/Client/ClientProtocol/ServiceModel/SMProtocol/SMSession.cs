//-----------------------------------------------------------------------
// <copyright file="SMSession.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.SMProtocol.SMFrames;

    /// <summary>
    /// Speed+Mobility Session
    /// </summary>
    public sealed class SMSession : IDisposable, IStreamStore
    {
        #region Fields

        /// <summary>
        /// List of streams.
        /// </summary>
        private readonly List<SMStream> streams;

        /// <summary>
        /// list of closed stream.
        /// </summary>
        private readonly List<SMStream> closedStreams;

        /// <summary>
        /// WS protocol.
        /// </summary>
        private readonly SMProtocol protocol;

        /// <summary>
        /// Flag server.
        /// </summary>
        private readonly bool isServer;

        /// <summary>
        /// Last good stream id.
        /// </summary>
        private int lastSeenStreamId;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSession"/> class.
        /// </summary>
        /// <param name="uri">the URI.</param>
        public SMSession(Uri uri)
            : this(uri, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSession"/> class.
        /// </summary>
        /// <param name="uri">the URI.</param>
        /// <param name="isServer">the Server flag.</param>
        public SMSession(Uri uri, bool isServer)
        {
            this.streams = new List<SMStream>();
            this.closedStreams = new List<SMStream>();
            this.isServer = isServer;

            this.protocol = new SMProtocol(uri, this);
            this.protocol.OnSessionFrame += this.OnSessionFrame;
            this.protocol.OnClose += this.OnProtocolClose;
            this.protocol.OnOpen += this.OnProtocolOpen;
            this.protocol.OnError += this.OnProtocolError;
            this.protocol.OnPing += this.OnProtocolPing;
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
        public event EventHandler<SMProtocolErrorEventArgs> OnError;

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
        public event EventHandler<EventArgs> OnStreamClosed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        public SMProtocol Protocol 
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
        public SMSessionState State { get; private set; }

        /// <summary>
        /// Gets the streams collection.
        /// </summary>
        public ICollection<SMStream> Streams 
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

        #endregion

        #region Methods

        /// <summary>
        /// Opens the sessions.
        /// </summary>
        public void Open()
        {
            if (this.State == SMSessionState.Created)
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
            if (this.State != SMSessionState.Closed)
            {
                this.protocol.Close(reason, this.lastSeenStreamId);
            }
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
        public void Ping()
        {
            this.protocol.SendPing();
        }

        /// <summary>
        /// Opens the stream in current session.
        /// </summary>
        /// <param name="headers">The S+M headers.</param>
        /// <param name="isFinal">the final flag.</param>
        /// <returns>The Stream.</returns>
        public SMStream OpenStream(SMHeaders headers, bool isFinal)
        {
            return this.OpenStream(this.GenerateNewStreamId(), headers, isFinal);
        }

        /// <summary>
        /// returns stream by id.
        /// </summary>
        /// <param name="streamId">Stream Id.</param>
        /// <returns>The Stream.</returns>
        public SMStream GetStreamById(int streamId)
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
        private SMStream OpenStream(int id, SMHeaders headers, bool isFinal)
        {
            if (id <= this.lastSeenStreamId)
            {
                this.End(StatusCode.ProtocolError);
                return null;
            }

            this.lastSeenStreamId = id;

            // don't have to wait for stream opening
            SMStream stream = new SMStream(id, this);

            stream.OnClose += this.OnCloseStream;
            stream.Open(headers, isFinal);

            this.streams.Add(stream);

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
        private void OnProtocolError(object sender, SMProtocolErrorEventArgs e)
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
            this.State = SMSessionState.Opened;
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
            if (e.Frame.Type == FrameType.SynStream)
            {
                OpenStream(e.Frame.StreamId, e.Frame.Headers, false);
            }
            else if (e.Frame.Type == FrameType.SynReply)
            {
                SMStream stream = this.GetStreamById(e.Frame.StreamId);
                if (stream != null && stream.State != SMStreamState.Closed && stream.State != SMStreamState.HalfClosed)
                {
                    stream.State = SMStreamState.Accepted;
                }
            }
        }

        /// <summary>
        /// Event handler for close.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">The event args.</param>
        private void OnProtocolClose(object sender, CloseFrameEventArgs e)
        {
            this.State = SMSessionState.Closed;

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
            SMStream stream = sender as SMStream;
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
