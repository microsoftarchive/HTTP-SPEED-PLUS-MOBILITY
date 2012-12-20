//-----------------------------------------------------------------------
// <copyright file="FrameType.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.Http2Protocol.ProtocolFrames
{
    /// <summary>
    /// Frame type enum.
    /// </summary>
    public enum FrameType : short
    {
        /// <summary>
        /// Data Frame.
        /// </summary>
        Data = 0,

        /// <summary>
        /// The SYN_STREAM control frame allows the sender to asynchronously create a stream between the endpoints.
        /// </summary>
        SynStream = 1,

        /// <summary>
        /// SYN_REPLY indicates the acceptance of a stream creation by the recipient of a SYN_STREAM frame.
        /// </summary>
        SynReply = 2,

        /// <summary>
        /// The RST_STREAM frame allows for abnormal termination of a stream.
        /// </summary>
        RTS = 3,

        /// <summary>
        /// A SETTINGS frame contains a set of id/value pairs for communicating
        /// configuration data about how the two endpoints may communicate.
        /// </summary>
        Settings = 4,

        /// <summary>
        /// The PING control frame is a mechanism for measuring a minimal round-
        /// trip time from the sender.
        /// </summary>
        Ping = 6,

        /// <summary>
        /// The GOAWAY control frame is a mechanism to tell the remote side of
        /// the connection to stop creating streams on this session
        /// </summary>
        GoAway = 7, 

        /// <summary>
        /// The HEADERS frame augments a stream with additional headers.
        /// </summary>
        Headers = 8,

        /// <summary>
        /// The WINDOW_UPDATE control frame is used to implement per stream flow
        /// control in Http/2.0
        /// </summary>
        WindowUpdate = 9
    }
}
