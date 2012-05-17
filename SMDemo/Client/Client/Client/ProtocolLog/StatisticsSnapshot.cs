//-----------------------------------------------------------------------
// <copyright file="StatisticsSnapshot.cs" company="Microsoft Open Technologies, Inc.">
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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsSnapshot"/> struct.
        /// </summary>
        /// <param name="totals">The SM statistics.</param>
        public StatisticsSnapshot(ProtocolMonitorLog totals)
        {
            this.splusmLog = new ProtocolMonitorLog(totals);
            this.httpLog = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsSnapshot"/> struct.
        /// </summary>
        /// <param name="http">The HTTP statistics.</param>
        public StatisticsSnapshot(HttpTrafficLog http)
        {
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
            if (this.splusmLog != null)
            {
                return this.splusmLog.LogTitle;
            }

            if (this.httpLog != null)
            {
                return this.httpLog.LogTitle;
            }

            return string.Empty;
        }

        #endregion
    }
}
