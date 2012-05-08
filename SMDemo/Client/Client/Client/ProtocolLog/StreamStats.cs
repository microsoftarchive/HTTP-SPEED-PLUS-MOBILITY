//-----------------------------------------------------------------------
// <copyright file="StreamStats.cs" company="Microsoft Open Technologies, Inc.">
//
// ---------------------------------------
// HTTPbis
// Copyright Microsoft Open Technologies, Inc.
// ---------------------------------------
// Microsoft Reference Source License.
// 
// This license governs use of the accompanying software. 
// If you use the software, you accept this license. 
// If you do not accept the license, do not use the software.
// 
// 1. Definitions
// 
// The terms "reproduce," "reproduction," and "distribution" have the same meaning here 
// as under U.S. copyright law.
// "You" means the licensee of the software.
// "Your company" means the company you worked for when you downloaded the software.
// "Reference use" means use of the software within your company as a reference, in read // only form, 
// for the sole purposes of debugging your products, maintaining your products, 
// or enhancing the interoperability of your products with the software, 
// and specifically excludes the right to distribute the software outside of your company.
// "Licensed patents" means any Licensor patent claims which read directly on the software 
// as distributed by the Licensor under this license. 
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free copyright license to reproduce the software for reference use.
// (B) Patent Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free patent license under licensed patents for reference use. 
// 
// 3. Limitations
// (A) No Trademark License- This license does not grant you any rights 
// to use the Licensor’s name, logo, or trademarks.
// (B) If you begin patent litigation against the Licensor over patents that you think may apply 
// to the software (including a cross-claim or counterclaim in a lawsuit), your license 
// to the software ends automatically. 
// (C) The software is licensed "as-is." You bear the risk of using it. 
// The Licensor gives no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws 
// which this license cannot change. To the extent permitted under your local laws, 
// the Licensor excludes the implied warranties of merchantability, 
// fitness for a particular purpose and non-infringement. 
// 
// -----------------End of License---------
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