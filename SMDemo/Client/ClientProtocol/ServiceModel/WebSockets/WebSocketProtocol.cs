// <copyright file="WebSocketProtocol.cs" company="Microsoft Open Technologies, Inc.">
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

namespace System.ServiceModel.WebSockets
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketProtocol"/> class.
    /// Opens a TCP connection to the server and delegates the rest of the protocol to the version-specific derived class.
    /// Provides socket read/write primitves, output queue and input buffer management to the derived class. 
    /// </summary>
    internal abstract class WebSocketProtocol : IDisposable
    {
        private const int DefaultMaxInputBufferSize = 1024 * 1024 * 4;

        // Establishing of the TCP connection and handling of buffered read/writes are encapsulated int this class
        private Collection<ArraySegment<byte>> outputQueue = new Collection<ArraySegment<byte>>();
        private bool isSending;
        private Socket socket;
        private SocketAsyncEventArgs sendArgs;
        private SocketAsyncEventArgs receiveArgs;
        private bool noDelay;

        public WebSocketProtocol(string url, string origin, string protocol, bool noDelay)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("Specified URL is malformed. Provide a valid absolute URL.", "url");
            }

            this.Uri = uri;
            if ("wss".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Secure web sockets are not supported. Specify 'ws' scheme instead of 'wss'.");
            }

            if (!"ws".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Unrecognized URL scheme. Specify 'ws' URL scheme.");
            }

#if !SILVERLIGHT
#else
            // Silverlight limits the sockets port range within 4502-4534
            if (uri.Port < 4502 || uri.Port > 4534)
            {
                throw new InvalidOperationException("Unsupported port number. Specify port number in the range 4502-4534.");
            }
#endif

            this.Origin = origin;
            this.Protocol = protocol;
            this.InputBuffer = new byte[1024];
            this.MaxInputBufferSize = DefaultMaxInputBufferSize;
            this.noDelay = noDelay;
        }

        // Contract with WebSocket
        public event EventHandler<WebSocketProtocolEventArgs> OnClose;

        public event EventHandler<WebSocketProtocolEventArgs> OnData;

        public event EventHandler<EventArgs> OnPing;

        public event EventHandler<EventArgs> OnConnected;

        public int MaxInputBufferSize { get; set; }

        // Contract with version-specific derived types
        protected byte[] InputBuffer { get; private set; }

        protected int UnreadDataOffset { get; private set; }

        protected int UnreadDataCount { get; private set; }

        protected string Origin { get; private set; }

        protected string Protocol { get; private set; }

        protected Uri Uri { get; private set; }

        protected bool IsClosed { get; private set; }

        public abstract void StartWebSocketHandshake();

        public abstract void SendMessage(string data);

        public abstract void SendMessage(byte[] data);

        public abstract void SendFragment(bool final, string data);

        public abstract void SendFragment(bool final, byte[] data);

        public abstract void SendPing();

        // initiates WS close handshake
        public abstract void Close(byte[] data);

        public void Start()
        {
            this.sendArgs = new SocketAsyncEventArgs();
            this.sendArgs.RemoteEndPoint = new DnsEndPoint(this.Uri.DnsSafeHost, this.Uri.Port);
#if !SILVERLIGHT
#else
            this.sendArgs.SocketClientAccessPolicyProtocol = SocketClientAccessPolicyProtocol.Http;
#endif
            this.sendArgs.Completed += this.OnTcpConnectCompleted;
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.NoDelay = this.noDelay;

            if (!this.socket.ConnectAsync(this.sendArgs))
            {
                this.ProcessConnectCompleted(true);
            }
        }

        public void Dispose()
        {
            this.Close(new Exception(), null);
        }

        protected void Connected()
        {
            if (this.OnConnected != null)
            {
                this.OnConnected(this, null);
            }
        }

        protected void DispatchData(bool fragment, bool final, string data)
        {
            if (this.OnData != null)
            {
                this.OnData(this, new WebSocketProtocolEventArgs { TextData = data, IsFragment = fragment, IsFinal = final });
            }
        }

        /// <summary>
        /// Callback for ws close frame from server. Client need to close everything, this is last frame.
        /// </summary>
        /// <param name="data">the close frame extension data.</param>
        protected void CloseConnection(byte[] data)
        {
            // perform regular close on ws protocol and underlining socket
            Close(null, data);
        }

        protected void DispatchPing()
        {
            if (this.OnPing != null)
            {
                this.OnPing(this, null);
            }
        }

        protected void DispatchData(bool fragment, bool final, byte[] data)
        {
            if (this.OnData != null)
            {
                this.OnData(this, new WebSocketProtocolEventArgs { BinaryData = data, IsFragment = fragment, IsFinal = final });
            }
        }

        // Shuts down socket without WS close handshake
        protected virtual void Close(Exception e, byte[] data)
        {
            if (!this.IsClosed)
            {
                this.IsClosed = true;
                try
                {
                    if (this.socket != null)
                    {
                        this.socket.Shutdown(SocketShutdown.Both);
                        this.socket.Close();
                        this.socket.Dispose();
                    }
                }
                finally
                {
                    if (this.sendArgs != null)
                    {
                        this.sendArgs.Dispose();
                    }

                    if (this.receiveArgs != null)
                    {
                        this.receiveArgs.Dispose();
                    }

                    if (this.OnClose != null)
                    {
                        this.OnClose(this, new WebSocketProtocolEventArgs { Exception = e, BinaryData = data });
                    }
                }
            }
        }

        protected void FailWebSocketConnection(Exception exception, bool synchronous)
        {
            this.CloseNoThrow(exception);
            if (synchronous)
            {
                throw exception;
            }
        }

        protected void EnqueueForSending(ArraySegment<byte> data)
        {
            lock (this.outputQueue)
            {
                this.outputQueue.Add(data);
            }

            this.ScheduleNextSend();
        }

        protected void ConsumeInputBytes(int count)
        {
            Debug.Assert((count > 0 || count == -1) && count <= this.UnreadDataCount, "ConsumeInputBytes()|Attempt to consume more bytes from the input buffer that are available");
            this.UnreadDataOffset += count;
            this.UnreadDataCount -= count;
        }

        protected void ReceiveMoreBytes(Action continuation)
        {
            if (this.EnsureRoomInBuffer())
            {
                int startIndex = this.UnreadDataOffset + this.UnreadDataCount;
                this.receiveArgs.UserToken = continuation;
                this.receiveArgs.SetBuffer(this.InputBuffer, startIndex, this.InputBuffer.Length - startIndex);
                if (!this.socket.ReceiveAsync(this.receiveArgs))
                {
                    this.OnReceiveCompleted(null, null);
                }
            }
        }

        protected bool TryReadLine(out string line)
        {
            line = null;
            for (int i = 0; i < this.UnreadDataCount; i++)
            {
                if (this.InputBuffer[this.UnreadDataOffset + i] == 0x0a)
                {
                    line = Encoding.UTF8.GetString(this.InputBuffer, this.UnreadDataOffset, i);
                    this.ConsumeInputBytes(i + 1);
                    break;
                }
            }

            return line != null;
        }

        private void OnTcpConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            this.ProcessConnectCompleted(false);
        }

        private void ProcessConnectCompleted(bool synchronous)
        {
            this.sendArgs.Completed -= this.OnTcpConnectCompleted;
            if (this.sendArgs.SocketError == SocketError.Success)
            {
                this.receiveArgs = new SocketAsyncEventArgs();
                this.receiveArgs.Completed += this.OnReceiveCompleted;
                this.sendArgs.Completed -= this.OnTcpConnectCompleted;
                this.sendArgs.Completed += this.OnSendCompleted;
                this.StartWebSocketHandshake(); // this is the entry point to a version specific derived class 
            }
            else
            {
                this.FailWebSocketConnection(this.sendArgs.ConnectByNameError, synchronous);
            }
        }

        private void SendNext()
        {
            if (this.IsClosed)
            {
                return;
            }

            lock (this.outputQueue)
            {
                ArraySegment<byte> segment = this.outputQueue[0];
                this.sendArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
                this.outputQueue.RemoveAt(0);
                this.isSending = true;
            }

            if (!this.socket.SendAsync(this.sendArgs))
            {
                this.OnSendCompleted(null, null);
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            this.isSending = false;
            if (this.sendArgs.SocketError == SocketError.Success)
            {
                this.ScheduleNextSend();
            }
            else
            {
                this.FailWebSocketConnection(new SocketException((int)this.sendArgs.SocketError), false);
            }
        }

        private void ScheduleNextSend()
        {
            bool sendNext;
            lock (this.outputQueue)
            {
                sendNext = this.outputQueue.Count > 0 && !this.isSending;
            }

            if (sendNext)
            {
                this.SendNext();
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (this.receiveArgs.SocketError == SocketError.Success)
            {
                this.UnreadDataCount += this.receiveArgs.BytesTransferred;
                if (this.receiveArgs.BytesTransferred == 0)
                {
                    this.CloseNoThrow(null);
                }
                else
                {
                    // invoke continuation to process the received data 
                    ((Action)this.receiveArgs.UserToken)();
                }
            }
            else
            {
                this.FailWebSocketConnection(new SocketException((int)this.receiveArgs.SocketError), false);
            }
        }

        private bool EnsureRoomInBuffer()
        {
            bool result = true;
            if (this.UnreadDataCount == this.InputBuffer.Length)
            {
                // buffer is full with no data consumed yet; grow the buffer
                Debug.Assert(this.UnreadDataOffset == 0, "EnsureRoomInBuffer(): Unread data offset is non-zero.");
                if ((this.InputBuffer.Length * 2) > this.MaxInputBufferSize)
                {
                    this.FailWebSocketConnection(new InvalidOperationException("Server message exceeds the size limit. Increase MaxInputBufferSize to allow it."), false);
                    result = false;
                }
                else
                {
                    byte[] newBuffer = new byte[this.InputBuffer.Length * 2];
                    Buffer.BlockCopy(this.InputBuffer, 0, newBuffer, 0, this.InputBuffer.Length);
                    this.InputBuffer = newBuffer;
                }
            }
            else if (this.UnreadDataCount == 0)
            {
                // all data in the buffer has been consumed
                this.UnreadDataOffset = 0;
            }
            else if ((this.UnreadDataOffset + this.UnreadDataCount) == this.InputBuffer.Length)
            {
                // some unconsumed data remains at the very end of the buffer with some free space up front; 
                // move the unread data to the beginning of the buffer
                Buffer.BlockCopy(this.InputBuffer, this.UnreadDataOffset, this.InputBuffer, 0, this.UnreadDataCount);
                this.UnreadDataOffset = 0;
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The purpose of CloseNoThrow is to not throw.")]
        private void CloseNoThrow(Exception e)
        {
            try
            {
                this.Close(e, null);
            }
            catch
            {
            }
        }
    }
}