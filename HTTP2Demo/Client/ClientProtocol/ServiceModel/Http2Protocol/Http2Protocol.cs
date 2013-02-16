//-----------------------------------------------------------------------
// <copyright file="Http2Protocol.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.ServiceModel.Http2Protocol.ProtocolFrames;
    using System.Threading;

    using ClientProtocol.ServiceModel.Http2Protocol.MessageProcessing;

    using Org.Mentalis.Security.Ssl.Shared.Extensions;
    using Org.Mentalis.Security.Ssl;
    using Org.Mentalis;

    /// <summary>
    /// HTTP2 protocol.
    /// </summary>
    public sealed class Http2Protocol : IDisposable
    {
        /// <summary>
        /// Protocol version.
        /// </summary>
        public const int Version = 3;

        #region Fields

        /// <summary>
        /// Internal Frame serializer.
        /// </summary>
        private FrameSerializer serializer;

        /// <summary>
        /// Internal Frame builder.
        /// </summary>
        private readonly FrameBuilder builder;

        /// <summary>
        /// Internal Stream store.
        /// </summary>
        private readonly IStreamStore streamsStore;

        /// <summary>
        /// Uri for connection.
        /// </summary>
        private readonly Uri uri;

        /// <summary>
        /// Protocol options.
        /// </summary>
        private ProtocolOptions options = new ProtocolOptions();

        /// <summary>
        /// HTTP2 Protocol opened flag.
        /// </summary>
        private bool opened;

        /// <summary>
        /// Socket.
        /// </summary>
        private VirtualSocket socket;

        /// <summary>
        /// Receive buffer.
        /// </summary>
        private List<byte> receivedDataBuffer = new List<byte>(4096 * 3);

        /// <summary>
        /// Indicates whenever this instance is working in server mode.
        /// </summary>
        private bool isServer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Http2Protocol"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="streamsStore">The streams store.</param>
        internal Http2Protocol(Uri uri, IStreamStore streamsStore) :
            this(uri, streamsStore, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Http2Protocol"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="streamsStore">The streams store.</param>
        /// <param name="options">Protocol options</param>
        internal Http2Protocol(Uri uri, IStreamStore streamsStore, ProtocolOptions options)
        {
            this.options = options;
            this.streamsStore = streamsStore;
            this.serializer = new FrameSerializer(this.options);
            this.builder = new FrameBuilder();

            this.uri = uri;
            this.isServer = false;
        }

        internal Http2Protocol(VirtualSocket socket, IStreamStore streamsStore, ProtocolOptions options)
        {
            this.options = options;
            this.streamsStore = streamsStore;
            this.serializer = new FrameSerializer(this.options);
            this.builder = new FrameBuilder();

            this.socket = socket;
            this.isServer = true;
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
        public event EventHandler<ProtocolErrorEventArgs> OnError;

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
        /// Gets Protocol options.
        /// </summary>
        public ProtocolOptions Options
        {
            get { return this.options; }
        }

        /// <summary>
        /// Gets the URI for the session.
        /// </summary>
        public Uri Uri
        {
            get { return uri; }
        }

        #endregion

        #region Methods

        private SecureSocket CreateSocketByUri(Uri uri)
        {
            if (!String.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Unrecognized scheme: " + uri.Scheme);

            SecurityOptions options;
            var extensions = new ExtensionType[] { ExtensionType.Renegotiation, ExtensionType.ALPN };
            if (!String.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                options = new SecurityOptions(SecureProtocol.None, null, ConnectionEnd.Client);
            }
            else
            {
                options = new SecurityOptions(SecureProtocol.Tls1, extensions, ConnectionEnd.Client);
            }

            options.Entity = ConnectionEnd.Client;
            options.CommonName = uri.Host;
            options.VerificationType = CredentialVerification.None;
            options.Certificate = Org.Mentalis.Security.Certificates.Certificate.CreateFromCerFile(@"certificate.pfx");
            options.Flags = SecurityFlags.Default;
            options.AllowedAlgorithms = SslAlgorithms.RSA_AES_128_SHA | SslAlgorithms.NULL_COMPRESSION;

            SecureSocket s = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
            
            try
            {
                s.Connect(new Net.DnsEndPoint(uri.Host, uri.Port));
            }
            catch(Exception ex)
            {
                s.Close();
                // Emitting protocol error. It will emit session OnError
                if (OnError != null)
                    this.OnError(this, new ProtocolErrorEventArgs(ex));

                throw;
            }

            return s;
        }

        internal void SendMessage(byte[] message)
        {
            socket.Send(message);
        }

        internal int Receive(byte[] message)
        {
            return socket.Receive(message);
        }

        /// <summary>
        /// Sets the frames processors.
        /// </summary>
        /// <param name="processors">The processors.</param>
        public void SetProcessors(List<IMessageProcessor> processors)
        {
            this.serializer.SetProcessors(processors);
        }

        /// <summary>
        /// Initializes connection to the remote host.
        /// </summary>
        public bool Connect()
        {
            if (this.socket == null)
                this.socket = CreateSocketByUri(uri);

            this.opened = true;            

            if (!this.socket.IsClosed)
            {
                if (OnOpen != null)
                    OnOpen(this, null);

                ThreadPool.QueueUserWorkItem(ProcessMessages);
            }

            return !this.socket.IsClosed;
        }

        /// <summary>
        /// Fills the given buffer with bytes.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="filledBytesCount">Already filled bytes count in buffer.</param>
        private bool FillBuffer(byte[] buffer, int filledBytesCount)
        {
            int bytesFilledTotal = filledBytesCount;
            while (bytesFilledTotal < buffer.Length)
            {
                byte[] bf = new byte[buffer.Length - bytesFilledTotal];
                int bytesFilledAtOneStep = socket.Receive(bf);

                if (bytesFilledAtOneStep == -1)
                    return false; 

                Buffer.BlockCopy(bf, 0, buffer, bytesFilledTotal, bf.Length);
                bytesFilledTotal += bytesFilledAtOneStep;
            }

            return true;
        }

        /// <summary>
        /// Receives the data.
        /// </summary>
        /// <returns></returns>
        private byte[] ReceiveData()
        {
            byte[] frameHeader = new byte[8];

            if (!FillBuffer(frameHeader, 0))
            {
                if (this.OnError != null)
                {
                    var connWasLostEx = new Exception("Connection was lost!");
                    this.OnError(this, new ProtocolErrorEventArgs(connWasLostEx));
                }
            }
            byte[] frameBytes = new byte[0];

            int frameLenInBytes = 8 + BinaryHelper.Int32FromBytes(new ArraySegment<byte>(frameHeader, 5, 3));
            frameBytes = new byte[frameLenInBytes];
            Buffer.BlockCopy(frameHeader, 0, frameBytes, 0, frameHeader.Length);

            if (!FillBuffer(frameBytes, 8))
            {
                if (this.OnError != null)
                {
                    var connWasLostEx = new Exception("Connection was lost!");
                    this.OnError(this, new ProtocolErrorEventArgs(connWasLostEx));
                }
            }
            return frameBytes;
        }

        /// <summary>
        /// Processes the messages.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        private void ProcessMessages(Object stateInfo)
        {
            do
            {
                byte[] data = ReceiveData();

                SocketEventArgs args = new SocketEventArgs(data);
                OnSocketData(this, args);
            }
            while (this.opened);
        }

        /// <summary>
        /// Initiates the Http2 close handshake.
        /// </summary>
        /// <param name="reason">the status code.</param>
        /// <param name="lastSeenStreamId">the last stream id.</param>
        public void Close(StatusCode reason, int lastSeenStreamId)
        {
            if (this.opened)
            {
                //Server must not send goAway to already closed from client side socket
                CloseInternal(reason, lastSeenStreamId, !isServer);
            }
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "0#",
            Justification = "CloseInternal() calls _socket.Dispose()")]
        public void Dispose()
        {
            this.CloseInternal(StatusCode.Success, 0, false);
        }

        /// <summary>
        /// Sends the WindowUpdate frame.
        /// </summary>
        public void SendWindowUpdate(int deltaSize,int streamId)
        {
            this.SendFrame(builder.BuildWindowUpdateFrame(deltaSize, streamId));
        }

        /// <summary>
        /// Sends the ping.
        /// </summary>
        public void SendPing(int streamId)
        {
            this.SendFrame(builder.BuildPingFrame(streamId));
        }

        /// <summary>
        /// Sends the syn stream request.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">FIN flag.</param>
        public void SendSynStream(Http2Stream stream, ProtocolHeaders headers, bool isFin)
        {
            this.SendFrame(this.builder.BuildSynStreamFrame(stream, headers, isFin));
        }

        public void SendSynReply(Http2Stream stream)
        {
            var headers = new ProtocolHeaders();

            headers[ProtocolHeaders.ContentType] = "text/plain";
            headers[ProtocolHeaders.Status] = "200";
            headers[ProtocolHeaders.Version] = "spdy/3";

            this.SendFrame(this.builder.BuildSynReplyFrame(stream, headers));
        }

        public void SendSettings(Http2Stream stream)
        {
            this.SendFrame(this.builder.BuildSettingsFrame(stream));
        }

        /// <summary>
        /// Sends the headers.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">FIN flag.</param>
        public void SendHeaders(Http2Stream stream, ProtocolHeaders headers, bool isFin)
        {
            this.SendFrame(stream, this.builder.BuildHeadersFrame(stream, headers, isFin));
        }

        /// <summary>
        /// Sends the RST.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="reason">The reason for RST.</param>
        public void SendRST(Http2Stream stream, StatusCode reason)
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
        /// Sends the window update request.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="creditAddition">The window addition.</param>
        public void SendWindowUpdate(int streamId, Int64 windowAddition)
        {
            this.SendFrame(this.builder.BuildWindowUpdateFrame(streamId, windowAddition));
        }

        /// <summary>
        /// Sends the window update request.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="creditAddition">The credit addition.</param>
        public void SendWindowUpdate(Http2Stream stream, Int64 windowAddition)
        {
            this.SendFrame(this.builder.BuildWindowUpdateFrame(stream, windowAddition));
        }

        /// <summary>
        /// Sends the GoAway stream.
        /// </summary>
        /// <param name="lastSeenStreamId">The last seen stream id.</param>
        /// <param name="reason">The reason of GoAway.</param>
        public void SendGoAway(int lastSeenStreamId, StatusCode reason)
        {
            this.SendFrame(this.streamsStore.GetStreamById(lastSeenStreamId), this.builder.BuildGoAwayFrame(lastSeenStreamId, reason));
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <param name="isFin">FIN flag.</param>
        public void SendData(Http2Stream stream, ProtocolData data, bool isFin)
        {
            this.SendFrame(stream, this.builder.BuildDataFrame(stream, data, isFin));
        }

        /// <summary>
        /// Closes connection to the remote host.
        /// </summary>
        private void CloseInternal(StatusCode reason, int lastSeenStreamId, bool sendGoAway)
        {
            if (this.opened)
            {
                this.opened = false;

                if (sendGoAway)
                    SendGoAway(lastSeenStreamId, reason);

                if (this.OnClose != null)
                {
                    this.OnClose(this, new CloseFrameEventArgs(new CloseFrameExt() { LastGoodSessionId = lastSeenStreamId, StatusCode = (int)reason }));
                }

                this.socket.Close();
                this.serializer.Dispose();
            }
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="frame">The base frame.</param>
        private void SendFrame(Http2Stream stream, BaseFrame frame)
        {
            try
            {
                byte[] frameBinary = this.serializer.Serialize(frame);
                frame.Length = frameBinary.Length;

                SendMessage(frameBinary);

                if (frame.IsFinal)
                {
                    stream.State = Http2StreamState.HalfClosed;
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
                else if (this.OnError != null)
                {
                    this.OnError(this, new ProtocolErrorEventArgs(e)); 
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

            SendMessage(frameBinary);

            if (this.OnFrameSent != null)
            {
                this.OnFrameSent(this, new FrameEventArgs(frame));
            }
        }

        /// <summary>
        /// Event handler for Socket open.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSocketOpen(object sender, EventArgs e)
        {
            if (this.OnOpen != null)
            {
                this.OnOpen(this, e);
            }
        }

        /// <summary>
        /// Event handler for Protocol Ping.
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
        /// Event handler for Http2 data.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSocketData(object sender, SocketEventArgs e)
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
                    if (streamError is ProtocolExeption || this.OnStreamError == null)
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
                    this.OnError(this, new ProtocolErrorEventArgs(protocolError));
                }
            }

        }

        /// <summary>
        /// Process Protocol Data frame.
        /// </summary>
        /// <param name="frame">The data frame.</param>
        private void ProcessDataFrame(DataFrame frame)
        {
            if (this.OnStreamFrame != null)
            {
                Http2Stream stream = this.streamsStore.GetStreamById(frame.StreamId);
                if (stream == null)
                {
                    this.SendRST(frame.StreamId, StatusCode.InvalidStream);
                }
                else
                {
                    if (stream.Session.IsFlowControlEnabled)
                    {
                        //TODO: incomment this when server will be able to handle window update
                        //if (stream.CurrentWindowBalanceFromServer <= 0)
                            // this.SendWindowUpdate(stream, stream.Session.CurrentWindowBalanceToServer);
                        //stream.UpdateWindowBalance(-frame.Data.Length);
                    }
                    receivedDataBuffer.AddRange(frame.Data);
                    if (frame.IsFinal && this.OnStreamFrame != null)
                    {
                         this.OnStreamFrame(this, new StreamDataEventArgs(stream, new ProtocolData(receivedDataBuffer.ToArray()), frame.IsFinal));
                        this.receivedDataBuffer.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Process Protocol control frame.
        /// </summary>
        /// <param name="frame">The control frame.</param>
        private void ProcessControlFrame(ControlFrame frame)
        {
            if (frame.Version != Version)
            {
                throw new ProtocolExeption(StatusCode.UnsupportedVersion);
            }

            if (frame.Type == FrameType.GoAway)
            {
                GoAwayFrame goAway = (GoAwayFrame)frame;
                CloseInternal(goAway.Status, goAway.LastSeenGoodStreamId, false);
            }
            else
            {
                Http2Stream stream = this.streamsStore.GetStreamById(frame.StreamId);

                // if this is rst frame - don't send error or it will go in rst loop
                if (stream == null && frame.Type != FrameType.RTS && frame.Type != FrameType.Settings && frame.Type != FrameType.SynStream)
                {
                    this.SendRST(frame.StreamId, StatusCode.InvalidStream);
                    return;
                }

                switch (frame.Type)
                {
                    case FrameType.SynStream:
                        //TODO validate syn_stream and send syn_reply or rst
                        this.OnSessionFrame(this, new ControlFrameEventArgs(frame));
                        break;
                    case FrameType.SynReply:
                        this.OnSessionFrame(this, new ControlFrameEventArgs(frame));
                        break;
                    case FrameType.Headers:
                        this.OnStreamFrame(this, new HeadersEventArgs(this.streamsStore.GetStreamById(frame.StreamId), frame.Headers));
                        break;
                    case FrameType.RTS:
                        this.OnStreamFrame(this, new RSTEventArgs(this.streamsStore.GetStreamById(frame.StreamId), frame.StatusCode));
                        break;
                    case FrameType.Ping:
                        this.OnSocketPing(this, new PingEventArgs(frame.StreamId));
                        break;
                    case FrameType.Settings:
                        OnSessionFrame(this, new SettingsEventArgs(frame));
                        break;
                    case FrameType.WindowUpdate:
                        Http2Stream http2Stream = this.streamsStore.GetStreamById(frame.StreamId);
                        http2Stream.UpdateWindowBalance(frame.DeltaWindowSize);
                        break;
                }
            }
        }
        #endregion
    }
}
