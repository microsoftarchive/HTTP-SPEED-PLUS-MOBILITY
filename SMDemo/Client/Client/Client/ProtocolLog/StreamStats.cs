//-----------------------------------------------------------------------
// <copyright file="StreamStats.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Runtime.Serialization;

    /// <summary>
    /// collection of stats for all streams.
    /// </summary>
    [Serializable]
    public class StreamStats : Dictionary<int, StreamInfo> 
    {
        /// <summary>
        /// Locking object to serialize inserts in dictionary
        /// </summary>
        [NonSerialized] 
        private object exclusiveLock = new object();

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamStats"/> class.
        /// </summary>
        public StreamStats() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamStats"/> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The serialization context.</param>
        protected StreamStats(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add upstream byte count.
        /// </summary>
        /// <param name="streamId">The stream if</param>
        /// <param name="length">The length in bytes</param>
        public void AddUpCount(int streamId, int length)
        {
            lock (this.exclusiveLock)
            {
                StreamInfo ss;
                if (!this.TryGetValue(streamId, out ss))
                {
                    ss = new StreamInfo();
                    this.Add(streamId, ss);
                }

                ss.UpCount += length;
                this[streamId] = ss;
            }
        }

        /// <summary>
        /// Add downstream byte count.
        /// </summary>
        /// <param name="streamId">The stream if</param>
        /// <param name="length">The length in bytes</param>
        public void AddDownCount(int streamId, int length)
        {
            lock (this.exclusiveLock)
            {
                StreamInfo ss;
                if (!this.TryGetValue(streamId, out ss))
                {
                    ss = new StreamInfo();
                    this.Add(streamId, ss);
                }

                ss.DownCount += length;
                this[streamId] = ss; 
            }
        }

        /// <summary>
        /// Reset statistics.
        /// </summary>
        public void Reset()
        {
            this.Clear();
        }

        /// <summary>
        /// Get stream statistics as string.
        /// </summary>
        /// <param name="streamId">The stream id</param>
        /// <returns>Statistics for a stream.</returns>
        public StreamInfo GetStreamStatistics(int streamId)
        {
            StreamInfo ss;
            this.TryGetValue(streamId, out ss);
            return ss;
        }

        #endregion
    }
}