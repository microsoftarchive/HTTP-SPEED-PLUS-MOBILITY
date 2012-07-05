// <copyright file="IETFHyBiWebSocketPotocol.cs" company="Microsoft Open Technologies, Inc.">
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

namespace System.ServiceModel.WebSockets
{
    using Collections.Generic;
    using Globalization;
    using Linq;
    using Net;
    using Security.Cryptography;
    using Text;
    using Text.RegularExpressions;
    using Threading;

    /// <summary>
    /// Initializes a new instance of the <see cref="IETFHyBiWebSocketPotocol"/> class.
    /// WebSocket protocol draft at http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-07
    /// </summary>
    internal class IETFHyBiWebSocketPotocol : WebSocketProtocol
    {
        private const bool EnableServerPing = true;
        private static readonly TimeSpan ServerPingInterval = TimeSpan.FromSeconds(50);

        private const byte FrameFIN = 0x1;
        private const byte FrameMASK = 0x1;
        private const byte FrameOpCodeCont = 0x0;
        private const byte FrameOpCodeClose = 0x8;
        private const byte FrameOpCodePing = 0x9;
        private const byte FrameOpCodePong = 0xa;
        private const byte FrameOpCodeText = 0x1;
        private const byte FrameOpCodeBin = 0x2;
        private const int MaskingKeySize = 4;
        private const string WebSocketsGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private static readonly Regex responseTemplate = new Regex(@"HTTP/1\.1 (\d+) Switching Protocols");
        private static readonly Regex headerTemplate = new Regex(@"([^:]+): ?([^\r]+)\r");
        private readonly HashAlgorithm hasher = new SHA1Managed();
        private string clientNonce;
        private byte incomingContinuationOpCode = byte.MaxValue;
        private bool firstContinuationFrameSent;
        private Timer pingTimer;

        private Dictionary<string, string> handshakeResponseHeaders;

		public IETFHyBiWebSocketPotocol(string url, string origin, string protocol, bool noDelay)
			: base(url, origin, protocol, noDelay)
		{
			// empty
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "Request headers are usually lowercase.")]
        public override void StartWebSocketHandshake(Dictionary<string, string> customHeaders) // main entry point to the protocol
        {
            // Generate the client nonce
            this.GenerateClientNonce();

            string request = string.Format(
                CultureInfo.InvariantCulture,
                "GET {0} HTTP/1.1\r\n" +
                "Upgrade: WebSocket\r\n" +
                "Connection: Upgrade, http2-icb\r\n" +
                "Host: {1}{2}\r\n" +
                "Sec-WebSocket-Origin: {3}\r\n" +
                "Sec-WebSocket-Version: 8\r\n" +
                "{4}" + // web socket protocol, if specified by the user
                "Sec-WebSocket-Key: {5}\r\n" +
                "Sec-WebSocket-Extensions: http2\r\n",
                this.Uri.AbsolutePath + this.Uri.Query,
                this.Uri.DnsSafeHost.ToLowerInvariant(),
                this.Uri.Port == 80
                    ? string.Empty
                    : string.Format(CultureInfo.InvariantCulture, ":{0}", this.Uri.Port),
                this.Origin.ToLowerInvariant(),
                string.IsNullOrEmpty(this.Protocol)
                    ? string.Empty
                    : string.Format(CultureInfo.InvariantCulture, "Sec-WebSocket-Protocol: {0}\r\n", this.Protocol),
                this.clientNonce);
        	request = customHeaders.Aggregate(request, (current, header) => current + string.Format("{0}: {1}\r\n", header.Key, header.Value));
        	request += "\r\n";

            this.ReceiveMoreBytes(this.ProcessHandshakeResponseHeader);
            this.EnqueueForSending(new ArraySegment<byte>(Encoding.UTF8.GetBytes(request)));
        }

        public override void SendMessage(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            byte[] encodedData = Encoding.UTF8.GetBytes(data);
            this.SendRaw(FrameFIN << 7 | FrameOpCodeText, encodedData);
        }

        public override void SendMessage(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.SendRaw(FrameFIN << 7 | FrameOpCodeBin, data);
        }

        public override void SendFragment(bool final, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            byte[] encodedData = Encoding.UTF8.GetBytes(data);
            this.SendFragmentRaw(FrameOpCodeText, final, encodedData);
        }

        public override void SendFragment(bool final, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.SendFragmentRaw(FrameOpCodeBin, final, data);
        }

        // initiates WS close handshake
        public override void Close(byte[] data)
        {
            this.SendRaw(FrameFIN << 7 | FrameOpCodeClose, data);
        }

        public override void SendPing()
        {
            this.SendRaw(FrameFIN << 7 | FrameOpCodePing, new byte[0]);
        }

        protected override void Close(Exception e, byte[] data)
        {
            if (this.pingTimer != null)
            {
                this.pingTimer.Dispose();
                this.pingTimer = null;
            }

            base.Close(e, data);
        }

        private static byte[] MaskPayload(byte[] payload, out byte[] maskingKey)
        {
            // Create masking-key
            maskingKey = new byte[MaskingKeySize];
            new Random((int)(DateTime.Now.Ticks % int.MaxValue)).NextBytes(maskingKey);

            // Mask data
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] ^= maskingKey[i % MaskingKeySize];
            }

            return payload;
        }

        private static byte[] UnmaskPayload(byte[] payload, byte[] maskingKey)
        {
            // Unmask data
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] ^= maskingKey[i % MaskingKeySize];
            }

            return payload;
        }

        private void SendPong(byte[] data)
        {
            this.SendRaw(FrameFIN << 7 | FrameOpCodePong, data);
        }

        private void SendFragmentRaw(byte opcode, bool final, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            // Set the updcode depending whether this is the first fragment of the message.
            if (!this.firstContinuationFrameSent)
            {
                this.firstContinuationFrameSent = true;
            }
            else
            {
                opcode = FrameOpCodeCont;
            }

            if (final)
            {
                opcode |= FrameFIN << 7;
                this.firstContinuationFrameSent = false;
            }

            this.SendRaw(opcode, data);
        }

        private void SendRaw(byte opcode, byte[] data)
        {
            int length = data.Length;
            int dataOffset;
            int maskingKeyOffset;
            byte[] payload;
            byte[] maskingKey;

            if (length < 126)
            {
                maskingKeyOffset = 2;
                dataOffset = 6;

                // Allocate for opcode + frame-length
                payload = new byte[dataOffset + length];

                // Fill length + data
                payload[1] = (byte)(0x7F & length);
            }
            else if (length < 65536)
            {
                maskingKeyOffset = 4;
                dataOffset = 8;

                // Allocate for opcode + frame-length + frame-length-16
                payload = new byte[dataOffset + length];

                // Fill length + data
                payload[1] = 126;
                payload[2] = (byte)((length & 0xFF00) >> 8);
                payload[3] = (byte)(length & 0x00FF);
            }
            else
            {
                maskingKeyOffset = 10;
                dataOffset = 14;

                // Allocate for opcode + frame-length + frame-length-63
                payload = new byte[dataOffset + length];

                // Fill length + data
                payload[1] = 127;
                payload[6] = (byte)((length & 0xFF000000) >> 24);
                payload[7] = (byte)((length & 0x00FF0000) >> 16);
                payload[8] = (byte)((length & 0x0000FF00) >> 8);
                payload[9] = (byte)(length & 0x000000FF);
            }

            // Fill the opcode
            payload[0] = opcode;

            // Fill the mask flag
            payload[1] |= FrameMASK << 7;

            // Mask the data
            data = MaskPayload(data, out maskingKey);
            data.CopyTo(payload, dataOffset);

            // Fill the masking key
            maskingKey.CopyTo(payload, maskingKeyOffset);

            this.EnqueueForSending(new ArraySegment<byte>(payload));
        }

        private void ProcessHandshakeResponseHeader()
        {
            string line;
            if (!this.TryReadLine(out line))
            {
                this.ReceiveMoreBytes(this.ProcessHandshakeResponseHeader);
                return;
            }

            Match match = responseTemplate.Match(line);
            if (!match.Success)
            {
                this.FailWebSocketConnection(new ProtocolViolationException("Server responded with unrecognized protocol. Header line of server handshake response is not recognized: " + line), false);
            }
            else if ("407".Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
            {
                this.FailWebSocketConnection(new NotSupportedException("Proxy authentication is required by the client implementation does not support it yet: " + line), false);
            }
            else if (!"101".Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
            {
                this.FailWebSocketConnection(new InvalidOperationException("Server status code of " + match.Groups[1].Value + " from the handshake response header does not match the expected 101 status code."), false);
            }
            else
            {
                this.handshakeResponseHeaders = new Dictionary<string, string>();
                this.ProcessHandshakeResponseHeaders();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "Request headers are usually lowercase.")]
        private void ProcessHandshakeResponseHeaders()
        {
            string line;
            bool readAllHeaders = false;
            while (this.TryReadLine(out line))
            {
                if ("\r".Equals(line, StringComparison.OrdinalIgnoreCase))
                {
                    readAllHeaders = true;
                    break;
                }
                else
                {
                    Match match = headerTemplate.Match(line);
                    if (!match.Success)
                    {
                        this.FailWebSocketConnection(new ProtocolViolationException("Server responded with unrecognized protocol. One of the header lines of server handshake response is malformed."), false);
                        return;
                    }
                    else
                    {
                        this.handshakeResponseHeaders[match.Groups[1].Value.ToLowerInvariant()] = match.Groups[2].Value;
                    }
                }
            }

            if (readAllHeaders)
            {
                this.ValidateHandshakeResponseHeaders();
            }
            else
            {
                this.ReceiveMoreBytes(this.ProcessHandshakeResponseHeaders);
            }
        }

        private bool ValidateHandshakeResponseHeader(string name, Func<string, bool> assert, Exception e)
        {
            if (!this.handshakeResponseHeaders.ContainsKey(name) || !assert(this.handshakeResponseHeaders[name]))
            {
                this.FailWebSocketConnection(e, false);
                return false;
            }

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Protocol terms are used here")]
        private void ValidateHandshakeResponseHeaders()
        {
            Func<bool>[] validators = new Func<bool>[] 
            {
                () => this.ValidateHandshakeResponseHeader(
                    "upgrade",
                    v => v.Equals("websocket", StringComparison.OrdinalIgnoreCase),
                    new ProtocolViolationException("Server protocol violation. Server did not respond with an Upgrade header with 'WebSocket' value.")),
                () => this.ValidateHandshakeResponseHeader(
                    "connection",
                    v => v.Equals("upgrade, http2-icb", StringComparison.OrdinalIgnoreCase),
                    new ProtocolViolationException("Server protocol violation. Server did not respond with an Connection header with 'Upgrade' value.")),
                () => this.ValidateHandshakeResponseHeader(
                    "sec-websocket-accept",
                    this.ValidateAccept,
                    new ProtocolViolationException("Server protocol violation. Server did not respond with sec-websocket-accept header that matches the sec-websocket-accept header sent on request.")),
                () => string.IsNullOrEmpty(this.Protocol) ? true : this.ValidateHandshakeResponseHeader(
                    "sec-websocket-protocol",
                    v => v.Equals(this.Protocol, StringComparison.Ordinal),
                    new ProtocolViolationException("Server protocol violation. Server did not respond with sec-websocket-protocol header that matches the protocol sent on request.")),
                () => this.ValidateHandshakeResponseHeader(
                        "sec-websocket-extensions",
                        v => v.Equals("http2", StringComparison.OrdinalIgnoreCase),
                        new ProtocolViolationException("Server protocol violation. Server did not respond with sec-websocket-extensions header that matches the protocol sent on request.")),
                () => 
                {
                    if (this.handshakeResponseHeaders.ContainsKey("set-cookie") || this.handshakeResponseHeaders.ContainsKey("set-cookie2"))
                    {
                        this.FailWebSocketConnection(new NotSupportedException("Server attempted to set cookies with handshake response. Cookies are not supported yet."), false);
                        return false;
                    }

                    return true;
                }
            };

            if (validators.All(v => v()))
            {
                this.Connected();

                if (EnableServerPing)
                {
                    // Setup ping timer to ping the server every ServerPingInterval.
                    this.pingTimer = new Timer(this.PingTimerCallback, null, TimeSpan.Zero, ServerPingInterval);
                }

                this.ProcessMessages();
            }
        }

        private void PingTimerCallback(object state)
        {
            // Send ping message to the server.
            this.SendPing();
        }

        private void ProcessPing(byte[] data)
        {
            // Reply to server's ping message with a pong message.
            this.SendPong(data);
            DispatchPing();
        }

        private void ProcessMessages()
        {
            do
            {
                if (this.UnreadDataCount < 2)
                {
                    this.ReceiveMoreBytes(this.ProcessMessages);
                    return;
                }

                byte opcode = (byte)(this.InputBuffer[this.UnreadDataOffset] & 0x0F);
                bool isFinal = ((this.InputBuffer[this.UnreadDataOffset] & 0xF0) >> 7) == FrameFIN;
                bool isFragment = false;
                int length = this.InputBuffer[this.UnreadDataOffset + 1];
                bool isMasked = ((length & 0x80) >> 7) == FrameMASK;
                byte[] maskingKey = null;
                int offset;

                if (length < 126)
                {
                    offset = 2;
                }
                else if (length == 126)
                {
                    if (this.UnreadDataCount < 4)
                    {
                        this.ReceiveMoreBytes(this.ProcessMessages);
                        return;
                    }

                    offset = 4;
                    length = (this.InputBuffer[this.UnreadDataOffset + 2] << 8) + this.InputBuffer[this.UnreadDataOffset + 3];
                }
                else if (length == 127)
                {
                    if (this.UnreadDataCount < 10)
                    {
                        this.ReceiveMoreBytes(this.ProcessMessages);
                        return;
                    }

                    offset = 10;
                    length = ((this.InputBuffer[this.UnreadDataOffset + 2] & 0x7F) << 56) + 
                        (this.InputBuffer[this.UnreadDataOffset + 3] << 48) +
                        (this.InputBuffer[this.UnreadDataOffset + 4] << 40) +
                        (this.InputBuffer[this.UnreadDataOffset + 5] << 32) +
                        (this.InputBuffer[this.UnreadDataOffset + 6] << 24) +
                        (this.InputBuffer[this.UnreadDataOffset + 7] << 16) +
                        (this.InputBuffer[this.UnreadDataOffset + 8] << 8)  +
                        this.InputBuffer[this.UnreadDataOffset + 9];
                }
                else
                {
                    this.FailWebSocketConnection(new NotSupportedException("Server sent a text frame with unknown frame size."), false);
                    return;
                }

                if (isMasked)
                {
                    // Ensure that we receive the masking key
                    if (this.UnreadDataCount < offset + 4)
                    {
                        this.ReceiveMoreBytes(this.ProcessMessages);
                        return;
                    }
                }

                if (this.UnreadDataCount < (length + offset))
                {
                    this.ReceiveMoreBytes(this.ProcessMessages);
                    return;
                }

                if (isMasked)
                {
                    // Read the masking key
                    maskingKey = new byte[MaskingKeySize];
                    Buffer.BlockCopy(this.InputBuffer, this.UnreadDataOffset + offset, maskingKey, 0, MaskingKeySize);
                    offset += 4;
                }

                if (opcode == FrameOpCodePing)
                {
                    byte[] buffer = new byte[length];
                    Buffer.BlockCopy(this.InputBuffer, this.UnreadDataOffset + offset, buffer, 0, length);
                    if (isMasked)
                    {
                        buffer = UnmaskPayload(buffer, maskingKey);
                    }

                    this.ProcessPing(buffer);
                }
                else if (opcode == FrameOpCodePong)
                {
                    // nop
                }
                else if (opcode == FrameOpCodeClose)
                {
                    byte[] buffer = new byte[length];
                    Buffer.BlockCopy(this.InputBuffer, this.UnreadDataOffset + offset, buffer, 0, length);
                    if (isMasked)
                    {
                        buffer = UnmaskPayload(buffer, maskingKey);
                    }

                    this.CloseConnection(buffer);
                }
                else
                {
                    // Ajust the opcode for continuation frames
                    if (opcode == FrameOpCodeCont)
                    {
                        opcode = this.incomingContinuationOpCode;
                        isFragment = true;
                    }
                    else
                    {
                        // Remember the opcode if the there are more frames to come
                        this.incomingContinuationOpCode = opcode;
                        isFragment = !isFinal;
                    }

                    if (opcode == FrameOpCodeText)
                    {
                        byte[] buffer = new byte[length];
                        Buffer.BlockCopy(this.InputBuffer, this.UnreadDataOffset + offset, buffer, 0, length);
                        if (isMasked)
                        {
                            buffer = UnmaskPayload(buffer, maskingKey);
                        }

                        string message = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        this.DispatchData(isFragment, isFinal, message);
                    }
                    else if (opcode == FrameOpCodeBin)
                    {
                        byte[] buffer = new byte[length];
                        Buffer.BlockCopy(this.InputBuffer, this.UnreadDataOffset + offset, buffer, 0, length);
                        if (isMasked)
                        {
                            buffer = UnmaskPayload(buffer, maskingKey);
                        }

                        this.DispatchData(isFragment, isFinal, buffer);
                    }
                }

                this.ConsumeInputBytes(length + offset);
            }
            while (!this.IsClosed);
        }

        private void GenerateClientNonce()
        {
            // Create random 16-byte value
            byte[] nonceBytes = new byte[16];
            new Random((int)(DateTime.Now.Ticks % int.MaxValue)).NextBytes(nonceBytes);

            // Get base64-encoded string
            this.clientNonce = Convert.ToBase64String(nonceBytes);
        }

        private bool ValidateAccept(string value)
        {
            // Create the concatination of Sec-WebSocket-Key and the predefined string
            string acceptString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.clientNonce, WebSocketsGuid);

            // Compute SHA1 hash
            byte[] hashedAccept = this.hasher.ComputeHash(Encoding.UTF8.GetBytes(acceptString));

            // Get base64-encoded string
            string encoded = Convert.ToBase64String(hashedAccept);

            return value.Equals(encoded, StringComparison.Ordinal);
        }
    }
}
