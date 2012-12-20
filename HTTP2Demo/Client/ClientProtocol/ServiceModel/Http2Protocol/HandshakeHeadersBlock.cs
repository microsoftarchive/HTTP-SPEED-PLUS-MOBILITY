//-----------------------------------------------------------------------
// <copyright file="HandshakeHeadersBlock.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace System.ServiceModel.Http2Protocol
{
    /// <summary>
    /// Class that represents headers block for http2 handshake.
    /// </summary>
    internal class HandshakeHeadersBlock
    {
        #region Fields    
   
            private readonly System.ServiceModel.Http2Protocol.Http2Protocol _protocol;
            private Uri uri;
            private readonly Regex headerTemplate = new Regex(@"([^:]+): ?([^\r]+)", RegexOptions.Compiled);
            private readonly Regex responseTemplate = new Regex(@"HTTP/1\.1 (\d+) Switching Protocols", RegexOptions.Compiled);
       
        #endregion 
        
        #region Public Properties

            public List<string> HandshakeHeaders { get; private set; }

        #endregion

        #region Constructors

            public HandshakeHeadersBlock(Http2Protocol protocol, Uri uri)
            {
                this._protocol = protocol;
                this.uri = uri;

                HandshakeHeaders = new List<string>(10);
            }

        #endregion

        #region Internal Methods

            internal bool StartHandshake()
            {
                string request = string.Format(
                            "GET {0} HTTP/1.1\r\n"
                            + "Upgrade: HTTP/2.0\r\n"
                            + "Connection: Upgrade\r\n",
                            uri);

                request += "\r\n";

                byte[] requestInBytes = Encoding.UTF8.GetBytes(request);

                _protocol.SendMessage(requestInBytes);

                ReadHandshakeHeaders();

                return TotalHeadersCheck();
            }

        #endregion

        #region Private Methods

            private void ReadHandshakeHeaders()
            {
                byte[] lineBuffer = new byte[256];
                string header = String.Empty;
                int totalBytesCame = 0;
                bool gotException;
                int bytesOfLastHeader = 0;

                while (true)
                {
                    gotException = false;
                    byte[] bf = new byte[1];
                    int bytesCame = _protocol.Receive(bf);
                    if (bytesCame == 0) break;

                    Buffer.BlockCopy(bf, 0, lineBuffer, totalBytesCame, bytesCame);
                    totalBytesCame += bytesCame;
                    try
                    {
                        header = Encoding.UTF8.GetString(lineBuffer, bytesOfLastHeader, totalBytesCame - bytesOfLastHeader);
                    }
                    catch
                    {
                        gotException = true;
                    }

                    if (totalBytesCame != 0 && !gotException && header[header.Length - 1] == '\n')
                    {
                        HandshakeHeaders.Add(header.TrimEnd('\n', '\r'));
                        bytesOfLastHeader = totalBytesCame;
                    }

                    if (header.Length > 4 && HandshakeHeaders.Count == 0)
                    {
                        if (!header.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ProtocolExeption("Server responce is not recognized", StatusCode.ProtocolError);
                        }
                    }

                    // empty header means we got \r\n\r\n which was trimmed. This means end of headers block.
                    if (HandshakeHeaders.Count >= 2 && String.IsNullOrEmpty(HandshakeHeaders.LastOrDefault()))
                    {
                        byte[] eohMark = new byte[2];
                        break;
                    }
                }

                HandshakeHeaders.RemoveAll(s => String.IsNullOrEmpty(s));
            }

            private bool CheckStatus()
            {
                if (HandshakeHeaders.Count == 0) return false;

                string[] splittedFirstHeader = HandshakeHeaders[0].Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries );

                return splittedFirstHeader[1] == "101";
            }

            private bool CheckFirstHeader(string header)
            {
                Match match = responseTemplate.Match(header);
                return match.Success;
            }

            private bool CheckHeadersWithRegex()
            {
                if (HandshakeHeaders.Count == 0) return false;

                bool firstMatch = CheckFirstHeader(HandshakeHeaders[0]);
                bool headersOk = firstMatch;
                if (!headersOk)
                    return false;

                for(int i = 1 ; i < HandshakeHeaders.Count ; i++)
                    headersOk &= headerTemplate.Match(HandshakeHeaders[i]).Success;
                return headersOk;
            }

            private bool CheckCurrentHeaders()
            {
                return CheckConnection() & CheckUpgrade();
            }

            private bool CheckConnection()
            {
                for (int i = 1; i < HandshakeHeaders.Count; i++)
                {
                    if (HandshakeHeaders[i].IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) != -1
                        &&
                        HandshakeHeaders[i].IndexOf("connection", StringComparison.OrdinalIgnoreCase) != -1)
                        return true;
                }
                return false;
            }

            private bool CheckUpgrade()
            {
                for (int i = 1; i < HandshakeHeaders.Count; i++)
                {
                    if (HandshakeHeaders[i].IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) != -1
                        &&
                        HandshakeHeaders[i].IndexOf("http/2.0", StringComparison.OrdinalIgnoreCase) != -1)
                        return true;
                }
                return false;
            }

            private bool TotalHeadersCheck()
            {
                return CheckStatus() & CheckCurrentHeaders() & CheckHeadersWithRegex();
            }

        #endregion
    }
}
