//-----------------------------------------------------------------------
// <copyright file="StatusCode.cs" company="Microsoft Open Technologies, Inc.">
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
    /// Protocol status codes.
    /// </summary>
    public enum StatusCode : int
    {
        /// <summary>
        /// Indicates no status. This is not a valid value to receive or send.
        /// </summary>
        None = 0,

        /// <summary>
        /// All is ok.
        /// </summary>
        Success = 1000,

        /// <summary>
        /// This is a generic error, and should only be used if a more specific error is not available.
        /// </summary>
        ProtocolError = 1002,

        /// <summary>
        /// This is returned when a frame is received for a stream which is not active
        /// </summary>
        InvalidStream = 2,

        /// <summary>
        /// Indicates that the stream was refused before any processing has been done on the stream.
        /// </summary>
        RefusedStream = 3,

        /// <summary>
        /// Indicates that the recipient of a stream does not support the Http2 version requested.
        /// </summary>
        UnsupportedVersion = 4,

        /// <summary>
        /// Used by the creator of a stream to indicate that the stream is no longer needed.
        /// </summary>
        Cancel = 5,

        /// <summary>
        /// This is a generic error which can be used when the implementation has internally failed, not due to anything in the protocol.
        /// </summary>
        InternalError = 6,

        /// <summary>
        /// The endpoint detected that its peer violated the flow control protocol.
        /// </summary>
        FlowControlError = 7,

        /// <summary>
        /// The endpoint received a SYN_REPLY for a stream already open.
        /// </summary>
        StreamInUse = 8,

        /// <summary>
        /// The endpoint received a data or SYN_REPLY frame for a stream which is half closed.
        /// </summary>
        StreamAlreadyClosed = 9,

        /// <summary>
        /// The server received a request for a resource whose origin does not have valid credentials in the client certificate vector.
        /// </summary>
        InvalidCredentials = 10,

        /// <summary>
        /// The endpoint received a frame which this implementation could not support.
        /// </summary>
        FrameTooLarge = 11
    }
}
