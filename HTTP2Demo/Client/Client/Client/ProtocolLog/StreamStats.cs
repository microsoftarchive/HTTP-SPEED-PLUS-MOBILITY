//-----------------------------------------------------------------------
// <copyright file="StreamStats.cs" company="Microsoft Open Technologies, Inc.">
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