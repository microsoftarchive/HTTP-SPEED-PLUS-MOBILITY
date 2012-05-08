//-----------------------------------------------------------------------
// <copyright file="FrameLogItem.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Client.Benchmark
{
    using System;
    using System.ServiceModel.SMProtocol.SMFrames;

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
