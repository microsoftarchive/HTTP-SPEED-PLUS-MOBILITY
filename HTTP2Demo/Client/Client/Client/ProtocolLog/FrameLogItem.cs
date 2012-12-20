//-----------------------------------------------------------------------
// <copyright file="FrameLogItem.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Client.Benchmark
{
    using System;
    using System.ServiceModel.Http2Protocol.ProtocolFrames;

    /// <summary>
    /// Log item for frame.
    /// </summary>
    public struct FrameLogItem
    {
        /// <summary>
        /// Time stamp
        /// </summary>   
        public DateTime TimeStamp;

        /// <summary>
        /// Duration of frame
        /// </summary>   
        public TimeSpan Duration;

        /// <summary>
        /// Length of frame
        /// </summary>   
        public long Length;

        /// <summary>
        /// Frame headers
        /// </summary>   
        public string Headers;

        /// <summary>
        /// Stream Id
        /// </summary>   
        public int StreamId;

        /// <summary>
        /// Frame type
        /// </summary>   
        public FrameType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameLogItem"/> struct.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public FrameLogItem(BaseFrame frame)
        {
            this.Length = frame.Length;
            this.StreamId = frame.StreamId;

            if (frame is ControlFrame)
            {
                this.Type = ((ControlFrame)frame).Type;
                this.Headers = ((ControlFrame)frame).Headers.ToString();
                this.TimeStamp = DateTime.Now;
                this.Duration = TimeSpan.Zero;
            }
            else
            {
                this.Type = FrameType.Data;
                this.TimeStamp = DateTime.Now;
                this.Duration = TimeSpan.Zero;
                this.Headers = string.Empty;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = "--------------------\r\n";
            result += "Frame:\r\n";
            result += "Type: " + this.Type + "\r\n";
            result += "TimeStamp: " + this.TimeStamp + "\r\n";
            result += "Length: " + this.Length + "\r\n";
            result += "StreamId: " + this.StreamId + "\r\n";
            if (this.Duration.TotalMilliseconds > 0)
            {
                result += "Duration(ms): " + this.Duration.TotalMilliseconds + "\r\n";
            }

            result += "Headers: " + this.Headers + "\r\n";
            result += "--------------------";

            return result;
        }
    }
}
