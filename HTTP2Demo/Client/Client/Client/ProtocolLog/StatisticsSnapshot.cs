//-----------------------------------------------------------------------
// <copyright file="StatisticsSnapshot.cs" company="Microsoft Open Technologies, Inc.">
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
    using Client.HttpBenchmark;

    /// <summary>
    /// Statistics for transaction.
    /// </summary>
    public struct StatisticsSnapshot
    {
        #region Fields

        /// <summary>
        /// frame totals
        /// </summary>   
        private readonly ProtocolMonitorLog splusmLog;

        /// <summary>
        /// HTTP1.1 log
        /// </summary>   
        private HttpTrafficLog httpLog;

        /// <summary>
        /// Duration of this snapshot
        /// </summary>
        private TimeSpan duration;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsSnapshot"/> struct.
        /// </summary>
        /// <param name="totals">The Protocol statistics.</param>
        /// <param name="dur">The time span.</param>
        public StatisticsSnapshot(ProtocolMonitorLog totals, TimeSpan dur)
        {
            this.duration = dur;
            this.splusmLog = new ProtocolMonitorLog(totals);
            this.httpLog = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsSnapshot"/> struct.
        /// </summary>
        /// <param name="http">The HTTP statistics.</param>
        /// <param name="dur">The time  span.</param>
        public StatisticsSnapshot(HttpTrafficLog http, TimeSpan dur)
        {
            this.duration = dur;
            this.splusmLog = null;
            this.httpLog = new HttpTrafficLog(http);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of lines in totals report
        /// </summary>
        public int MaxTotalsLines
        {
            get
            {
                return (this.splusmLog != null) ? this.splusmLog.MaxLines : this.httpLog.MaxLines;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets start of side-by-side output
        /// </summary>
        public void StartSxSOutput()
        {
            if (this.splusmLog != null)
            {
                this.splusmLog.StartSxSOutput();
            }

            if (this.httpLog != null)
            {
                this.httpLog.StartSxSOutput();
            }
        }

        /// <summary>
        /// Sets start of side-by-side output
        /// </summary>
        /// <returns>
        /// one report string
        /// </returns>
        public string GetSxSLine()
        {
            if (this.splusmLog != null)
            {
                return this.splusmLog.GetSxSLine();
            }

            if (this.httpLog != null)
            {
                return this.httpLog.GetSxSLine();
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns log title
        /// </summary>
        /// <returns>
        /// Title of the log
        /// </returns>
        public string GetLogTitle()
        {
            string title = string.Format("{0}:{1}:{2,3} ", this.duration.Minutes, this.duration.Seconds, this.duration.Milliseconds);
            if (this.splusmLog != null)
            {
                title += this.splusmLog.LogTitle;
            }

            if (this.httpLog != null)
            {
                title += this.httpLog.LogTitle;
            }

            return title;
        }

        #endregion
    }
}
