//-----------------------------------------------------------------------
// <copyright file="SMProtocol.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.ServiceModel.SMProtocol.SMFrames;
    using System.ServiceModel.WebSockets;

    /// <summary>
    /// SM protocol.
    /// </summary>
    public sealed class SMProtocol : IDisposable    
    {
        /// <summary>
        /// Protocol version.
        /// </summary>
        public const int Version = 1;

        #region Fields

        /// <summary>
        /// Internal WebSocket.
        /// </summary>
        private readonly WebSocket webSocket;

        /// <summary>
        /// Internal Frame serializer.
        /// </summary>
        private readonly FrameSerializer serializer;

        /// <summary>
        /// Internal Frame builder.
        /// </summary>
        private readonly FrameBuilder builder;

        /// <summary>
        /// Internal Stream store.
        /// </summary>
        private readonly IStreamStore streamsStore;

        /// <summary>
        /// SM Protocol opened flag.
        /// </summary>
        private bool opened;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SMProtocol"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="streamsStore">The streams store.</param>
        internal SMProtocol(Uri uri, IStreamStore streamsStore)
        {
            this.streamsStore = streamsStore;
            this.webSocket = new WebSocket(uri.ToString());
            this.webSocket.OnOpen += this.OnSocketOpen;
            this.webSocket.OnPing += this.OnSocketPing;
            this.webSocket.OnClose += this.OnSocketClose;

            this.serializer = new FrameSerializer();
            this.builder = new FrameBuilder();
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when frame is sent.
        /// </summary>
        public event EventHandler<FrameEventArgs> OnFrameSent;

        /// <summary>
        /// Occurs when frame is sent.
        /// </summary>
        public event EventHandler<FrameEventArgs> OnFrameReceived;

        /// <summary>
        /// Occurs when ping is received.
        /// </summary>
        public event EventHandler<EventArgs> OnPing;

        /// <summary>
        /// Occurs when protocol is opened.
        /// </summary>
        public event EventHandler<EventArgs> OnOpen;

        /// <summary>
        /// Occurs when protocol is closed.
        /// </summary>
        public event EventHandler<CloseFrameEventArgs> OnClose;

        /// <summary>
        /// Occurs when session errors.
        /// </summary>
        public event EventHandler<SMProtocolErrorEventArgs> OnError;

        /// <summary>
        /// Occurs when stream errors.
        /// </summary>
        public event EventHandler<StreamErrorEventArgs> OnStreamError;

        /// <summary>
        /// Occurs when frame arrives for the stream.
        /// </summary>
        public event EventHandler<StreamEventArgs> OnStreamFrame;

        /// <summary>
        /// Occurs when frame arrives for the session.
        /// </summary>
        public event EventHandler<ControlFrameEventArgs> OnSessionFrame;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the URI for the session.
        /// </summary>
        public Uri Uri 
        { 
            get 
            { 
                return new Uri(this.webSocket.Url); 
            } 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes connection to the remote host.
        /// </summary>
        public void Connect()
        {
            this.webSocket.Open();
            this.opened = true;
        }

        /// <summary>
        /// Initiates the WS close handshake.
        /// </summary>
        /// <param name="reason">the status code.</param>
        /// <param name="lastSeenStreamId">the last stream id.</param>
        public void Close(StatusCode reason, int lastSeenStreamId)
        {
            if (this.opened)
            {
                List<byte> byteList = new List<byte>();
                byteList.AddRange(BinaryHelper.Int16ToBytes((Int16)reason));
                byteList.AddRange(BinaryHelper.Int32ToBytes(lastSeenStreamId));
                this.webSocket.Close(byteList.ToArray());
            }
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "0#",
            Justification = "CloseInternal() calls webSocket.Dispose()")]
        public void Dispose()
        {
            this.CloseInternal();
        }

        /// <summary>
        /// Sends the ping.
        /// </summary>
        public void SendPing()
        {
            this.webSocket.SendPing();
        }

        /// <summary>
        /// Sends the syn stream request.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">FIN flag.</param>
        public void SendSynStream(SMStream stream, SMHeaders headers, bool isFin)
        {
            this.SendFrame(this.builder.BuildSynStreamFrame(stream, headers, isFin));
        }

        /// <summary>
        /// Sends the headers.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">FIN flag.</param>
        public void SendHeaders(SMStream stream, SMHeaders headers, bool isFin)
        {
            this.SendFrame(stream, this.builder.BuildHeadersFrame(stream, headers, isFin));
        }

        /// <summary>
        /// Sends the RST.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="reason">The reason for RST.</param>
        public void SendRST(SMStream stream, StatusCode reason)
        {
            this.SendFrame(stream, this.builder.BuildRSTFrame(stream, reason));
        }

        /// <summary>
        /// Sends the RST.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="reason">The reason for RST.</param>
        public void SendRST(int streamId, StatusCode reason)
        {
            this.SendFrame(this.builder.BuildRSTFrame(streamId, reason));
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <param name="isFin">FIN flag.</param>
        public void SendData(SMStream stream, SMData data, bool isFin)
        {
            this.SendFrame(stream, this.builder.BuildDataFrame(stream, data, isFin));
        }

        /// <summary>
        /// Closes connection to the remote host.
        /// </summary>
        private void CloseInternal()
        {
            if (this.opened)
            {
                this.opened = false;
                if (this.webSocket.ReadyState != WebSocketState.Closed)
                {
                    this.webSocket.CloseInternal();
                }

                if (this.OnClose != null)
                {
                    this.OnClose(this, null);
                }

                this.webSocket.Dispose();
            }
        }
        
        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="frame">The base frame.</param>
        private void SendFrame(SMStream stream, BaseFrame frame)
        {
            try
            {
                byte[] frameBinary = this.serializer.Serialize(frame);
                frame.Length = frameBinary.Length;

                this.webSocket.SendMessage(frameBinary);

                if (frame.IsFinal)
                {
                    stream.State = SMStreamState.HalfClosed;
                }

                if (this.OnFrameSent != null)
                {
                    this.OnFrameSent(this, new FrameEventArgs(frame));
                }
            }
            catch (Exception e)
            {
                if (this.OnStreamError != null)
                {
                    this.OnStreamError(this, new StreamErrorEventArgs(stream, e));
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sends the frame.
        /// </summary>
        /// <param name="frame">The control frame.</param>
        private void SendFrame(ControlFrame frame)
        {
            byte[] frameBinary = this.serializer.Serialize(frame);
            frame.Length = frameBinary.Length;

            this.webSocket.SendMessage(frameBinary);

            if (this.OnFrameSent != null)
            {
                this.OnFrameSent(this, new FrameEventArgs(frame));
            }
        }

        /// <summary>
        /// Event handler for WebSocket open.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSocketOpen(object sender, EventArgs e)
        {
            this.webSocket.OnData += this.OnSocketData;
            if (this.OnOpen != null)
            {
                this.OnOpen(this, e);
            }
        }

        /// <summary>
        /// callback from WebSocket after socket is closed
        /// clean up and error reporting
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSocketClose(object sender, WebSocketProtocolEventArgs e)
        {
            // if WebSocket initiated close by itself, SMProtocol is not aware yet
            // In this case this.opened will be true and SMProtocol is forced to close
            if (this.webSocket.LastError != null)
            {
                if (this.OnError != null)
                {
                    // General error state, do not attempt to communicate
                    this.OnError(this, new SMProtocolErrorEventArgs(this.webSocket.LastError));
                }
                else
                {
                    // fallback to notify user anyhow
                    Console.WriteLine("ERROR: WebSocket error {0}", this.webSocket.LastError);
                }
            }
            else
            {
                try
                {
                    if (e.BinaryData != null)
                    {
                        // We received WS close frame from server with extension data. 
                        // Attempt to unpack the data
                        CloseFrameExt closeData = this.serializer.DeserializeCloseFrameExt(e.BinaryData);

                        if (this.OnClose != null)
                        {
                            // Send unpacked extension data to event listener
                            this.OnClose(this, new CloseFrameEventArgs(closeData));
                        }
                    }
                    else
                    {
                        // We received WS close frame without extension data. If may be 
                        // response to our frame. Or it may be server initiated by server.
                        if (this.OnClose != null)
                        {
                            this.OnClose(this, null);
                        }
                    }

                    // this object is now closed
                    this.opened = false;
                }
                catch (Exception protocolError)
                {
                    if (this.OnError != null)
                    {
                        this.OnError(this, new SMProtocolErrorEventArgs(protocolError));
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for SM Ping.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSocketPing(object sender, EventArgs e)
        {
            if (this.OnPing != null)
            {
                this.OnPing(this, e);
            }
        }

        /// <summary>
        /// Event handler for WebSocket data.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSocketData(object sender, WebSocketEventArgs e)
        {
            try
            {
                BaseFrame frame = this.serializer.Deserialize(e.BinaryData);
                if (this.OnFrameReceived != null)
                {
                    this.OnFrameReceived(this, new FrameEventArgs(frame));
                }

                try
                {
                    if (frame is DataFrame)
                    {
                        this.ProcessDataFrame((DataFrame)frame);
                    }
                    else if (frame is ControlFrame)
                    {
                        this.ProcessControlFrame((ControlFrame)frame);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported frame type");
                    }
                }
                catch (Exception streamError)
                {
                    if (streamError is SMProtocolExeption || this.OnStreamError == null)
                    {
                        throw;
                    }

                    this.OnStreamError(this, new StreamErrorEventArgs(this.streamsStore.GetStreamById(frame.StreamId), streamError));
                }
            }
            catch (Exception protocolError)
            {
                if (this.OnError != null)
                {
                    this.OnError(this, new SMProtocolErrorEventArgs(protocolError));
                }
            }
        }

        /// <summary>
        /// Process SM Data frame.
        /// </summary>
        /// <param name="frame">The data frame.</param>
        private void ProcessDataFrame(DataFrame frame)
        {
            if (this.OnStreamFrame != null)
            {
                SMStream stream = this.streamsStore.GetStreamById(frame.StreamId);
                if (stream == null)
                {
                    this.SendRST(frame.StreamId, StatusCode.InvalidStream);
                }
                else
                {
                    this.OnStreamFrame(this, new StreamDataEventArgs(stream, new SMData(frame.Data), frame.IsFinal));
                }
            }
        }

        /// <summary>
        /// Process SM control frame.
        /// </summary>
        /// <param name="frame">The control frame.</param>
        private void ProcessControlFrame(ControlFrame frame)
        {
            if (frame.Version != Version)
            {
                throw new SMProtocolExeption(StatusCode.UnsupportedVersion);
            }

            SMStream stream = this.streamsStore.GetStreamById(frame.StreamId);

            // if this is rst frame - don't send error or it will go in rst loop
            if (stream == null && frame.Type != FrameType.RTS)
            {
                this.SendRST(frame.StreamId, StatusCode.InvalidStream);
                return;
            }

            switch (frame.Type)
            {
                case FrameType.SynStream:
                case FrameType.SynReply:
                    this.OnSessionFrame(this, new ControlFrameEventArgs(frame));
                    break;
                case FrameType.Headers:
                    this.OnStreamFrame(this, new HeadersEventArgs(this.streamsStore.GetStreamById(frame.StreamId), frame.Headers));
                    break;
                case FrameType.RTS:
                    this.OnStreamFrame(this, new RSTEventArgs(this.streamsStore.GetStreamById(frame.StreamId), frame.StatusCode));
                    break;
            }
        }

        #endregion
    }
}
