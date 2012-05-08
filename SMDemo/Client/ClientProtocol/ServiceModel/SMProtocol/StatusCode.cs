//-----------------------------------------------------------------------
// <copyright file="StatusCode.cs" company="Microsoft Open Technologies, Inc.">
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
        /// Indicates that the recipient of a stream does not support the SM version requested.
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
