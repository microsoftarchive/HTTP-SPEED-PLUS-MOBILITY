//-----------------------------------------------------------------------
// <copyright file="FrameType.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol.SMFrames
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
        /// The HEADERS frame augments a stream with additional headers.
        /// </summary>
        Headers = 4
    }
}
