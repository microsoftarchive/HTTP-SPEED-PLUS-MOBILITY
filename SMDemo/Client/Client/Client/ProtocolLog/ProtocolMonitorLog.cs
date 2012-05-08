//-----------------------------------------------------------------------
// <copyright file="ProtocolMonitorLog.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Monitor session log.
    /// </summary>
    public class ProtocolMonitorLog
    {
        #region Fields

        /// <summary>
        /// Separator constant
        /// </summary>   
        private const string Separator = "______________________________________\n";

        /// <summary>
        /// list of frames
        /// </summary>   
        private readonly List<FrameLogItem> framesLog = new List<FrameLogItem>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets begin time
        /// </summary>   
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// Gets or sets total size of data frames
        /// </summary>   
        public long TotalSizeDataFrameReceived { get; set; }

        /// <summary>
        /// Gets or sets total size of control frames
        /// </summary>   
        public long TotalSizeControlFrameReceived { get; set; }

        /// <summary>
        /// Gets or sets total size of frames sent
        /// </summary>   
        public long TotalSizeFrameSent { get; set; }

        /// <summary>
        /// Gets or sets total size of control frames sent
        /// </summary>   
        public long TotalCountControlFramesSent { get; set; }

        /// <summary>
        /// Gets or sets total size of frames received
        /// </summary>   
        public long TotalCountControlFramesReceived { get; set; }

        /// <summary>
        /// Gets or sets total count of data frames sent
        /// </summary>   
        public long TotalCountDataFramesSent { get; set; }

        /// <summary>
        /// Gets or sets total count of data frames received
        /// </summary>   
        public long TotalCountDataFramesReceived { get; set; }

        /// <summary>
        /// Gets or sets total count of streams opened
        /// </summary>   
        public long TotalCountStreamsOpened { get; set; }

        /// <summary>
        /// Gets the frames log.
        /// </summary>
        public List<FrameLogItem> FramesLog
        {
            get { return this.framesLog; }
        }

        /// <summary>
        /// Gets the total count control frames.
        /// </summary>
        public long TotalCountControlFrames
        {
            get { return this.TotalCountControlFramesReceived + this.TotalCountControlFramesSent; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = this.FramesLog.Aggregate(string.Empty, (current, frameItem) => current + (frameItem.ToString() + "\r\n"));

            result += this.GetShortLog();
            return result;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents short version of log.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents short version of log.
        /// </returns>
        public string GetShortLog()
        {
            string result = Separator;
            result += "                TOTAL\n";
            result += string.Format("Begin time: {0}\n", this.BeginTime);
            result += string.Format("Total size of data frames received(bytes):    {0}\r\n", this.TotalSizeDataFrameReceived);
            result += string.Format("Total size of control frames received(bytes): {0}\r\n", this.TotalSizeControlFrameReceived);
            result += string.Format("Total size of frames received(bytes):         {0}\r\n", this.TotalSizeDataFrameReceived + this.TotalSizeControlFrameReceived);
            result += string.Format("Total size of frames sent(bytes):             {0}\r\n", this.TotalSizeFrameSent);
            result += string.Format("Total size of data exchanged(bytes):          {0}\r\n", this.TotalSizeDataFrameReceived + this.TotalSizeControlFrameReceived + this.TotalSizeFrameSent);
            result += string.Format("Total count of control frames sent:           {0}\r\n", this.TotalCountControlFramesSent);
            result += string.Format("Total count of control frames received:       {0}\r\n", this.TotalCountControlFramesReceived);
            result += string.Format("Total count of control frames:                {0}\r\n", this.TotalCountControlFrames);
            result += string.Format("Total count of data frames sent:              {0}\r\n", this.TotalCountDataFramesSent);
            result += string.Format("Total count of data frames received:          {0}\r\n", this.TotalCountDataFramesReceived);
            result += string.Format("Total count of frames exchanged:              {0}\r\n", this.TotalCountControlFrames + this.TotalCountDataFramesSent + this.TotalCountDataFramesReceived);
            result += string.Format("Total count of streams opened:                {0}\r\n", this.TotalCountStreamsOpened);
            result += string.Format("Total count of connections opened:            1\r\n");
            result += Separator;

            return result;
        }

        /// <summary>
        /// Reset all statistics and clear frame log.
        /// </summary>
        public void Reset()
        {
            this.TotalSizeDataFrameReceived = 0;
            this.TotalSizeControlFrameReceived = 0;
            this.TotalSizeFrameSent = 0;
            this.TotalCountControlFramesSent = 0;
            this.TotalCountControlFramesReceived = 0;
            this.TotalCountDataFramesSent = 0;
            this.TotalCountDataFramesReceived = 0;
            this.TotalCountStreamsOpened = 0;

            this.framesLog.Clear();
        }

        #endregion
    }
}
