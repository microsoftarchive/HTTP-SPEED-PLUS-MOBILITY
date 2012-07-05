//-----------------------------------------------------------------------
// <copyright file="HttpTrafficLog.cs" company="Microsoft Open Technologies, Inc.">
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
namespace Client.HttpBenchmark
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Http traffic log.
    /// </summary>
    public class HttpTrafficLog
    {
        #region Fields

        /// <summary>
        /// Separator constant.
        /// </summary>
        private const string Separator = "______________________________________\n";

        /// <summary>
        /// List of http logs.
        /// </summary>
        private readonly List<HttpHeaderLogItem> httpLogs;

        /// <summary>
        /// Current line number for SxS output.
        /// </summary>
        private HttpReportLines sideBySideLine = HttpReportLines.InvalidLine;

        /// <summary>
        /// Lock for multithreaded access to results
        /// </summary>
        [NonSerialized]
        private object exclusiveLock;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTrafficLog"/> class.
        /// </summary>
        public HttpTrafficLog()
        {
            this.exclusiveLock = new object();
            this.httpLogs = new List<HttpHeaderLogItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTrafficLog"/> class.
        /// </summary>
        /// <param name="theOther">The other log.</param>
        public HttpTrafficLog(HttpTrafficLog theOther)
        {
            this.exclusiveLock = new object();
            this.httpLogs = new List<HttpHeaderLogItem>(theOther.httpLogs);
            this.TotalSizeDataReceived = theOther.TotalSizeDataReceived;
            this.TotalSizeHeadersReceived = theOther.TotalSizeHeadersReceived;
            this.TotalSizeHeadersSent = theOther.TotalSizeHeadersSent;
            this.TotalCountRequestSent = theOther.TotalCountRequestSent;
            this.TotalCountResponseReceived = theOther.TotalCountResponseReceived;
            this.LogTitle = theOther.LogTitle;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets max number of lines in totals report
        /// </summary>   
        public int MaxLines
        {
            get
            {
                return (int)HttpReportLines.HTTPReportMaxLine;
            }
        }

        /// <summary>
        /// Gets or sets title of the log
        /// </summary>   
        public string LogTitle { get; set; }

        /// <summary>
        /// Gets or sets total size data received.
        /// </summary>
        public long TotalSizeDataReceived { get; set; }

        /// <summary>
        /// Gets or sets total headers received.
        /// </summary>
        public long TotalSizeHeadersReceived { get; set; }

        /// <summary>
        /// Gets or sets total size headers sent.
        /// </summary>
        public long TotalSizeHeadersSent { get; set; }

        /// <summary>
        /// Gets or sets total count requests sent.
        /// </summary>
        public long TotalCountRequestSent { get; set; }

        /// <summary>
        /// Gets or sets total count response received.
        /// </summary>
        public long TotalCountResponseReceived { get; set; }

        /// <summary>
        /// Gets http logs.
        /// </summary>
        public List<HttpHeaderLogItem> HttpLogs
        {
            get { return this.httpLogs; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs request headers.
        /// </summary>
        /// <param name="header">the headers byte array.</param>
        public void LogRequest(byte[] header)
        {
            lock (this.exclusiveLock)
            {
            this.HttpLogs.Add(new HttpHeaderLogItem(header));

            this.TotalCountRequestSent++;
            this.TotalSizeHeadersSent += header.Length;
        }
        }

        /// <summary>
        /// Logs response headers.
        /// </summary>
        /// <param name="header">the headers byte array.</param>
        /// <param name="totalLength">the length of header.</param>
        public void LogResponse(byte[] header, int totalLength)
        {
            lock (this.exclusiveLock)
            {
            this.HttpLogs.Add(new HttpHeaderLogItem(header));

            this.TotalCountResponseReceived++;
            this.TotalSizeHeadersReceived += header.Length;
            this.TotalSizeDataReceived += totalLength - header.Length;
        }
        }

        /// <summary>
        /// Starts output for side-by-side
        /// </summary>
        public void StartSxSOutput()
        {
            this.sideBySideLine = 0;
        }

        /// <summary>
        /// Starts output for side-by-side
        /// </summary>
        /// <returns>String representation.</returns>
        public string GetSxSLine()
        {
            string output = string.Empty;

            if ((this.sideBySideLine < 0) || (this.sideBySideLine >= HttpReportLines.HTTPReportMaxLine))
            {
                return output;
            }

            switch (this.sideBySideLine)
            {
                case HttpReportLines.LineZero:
                    output = string.Format("Size of data exchanged:  {0,10}", this.TotalSizeHeadersSent + this.TotalSizeHeadersReceived + this.TotalSizeDataReceived);
                    break;
                case HttpReportLines.LineOne:
                    output = string.Format("Total size received:     {0,10}", this.TotalSizeHeadersReceived + this.TotalSizeDataReceived);
                    break;
                case HttpReportLines.LineTwo:
                    output = string.Format("Size of headers sent:    {0,10}", this.TotalSizeHeadersSent);
                    break;
                case HttpReportLines.LineThree:
                    output = string.Format("Size of data received:   {0,10}", this.TotalSizeDataReceived);
                    break;
                case HttpReportLines.LineFour:
                    output = string.Format("Size of headers received:{0,10}", this.TotalSizeHeadersReceived);
                    break;
                case HttpReportLines.LineFive:
                    output = string.Format("# requests sent:         {0,10}", this.TotalCountRequestSent);
                    break;
                case HttpReportLines.LineSix:
                    output = string.Format("# responses received:    {0,10}", this.TotalCountResponseReceived);
                    break;
                case HttpReportLines.LineSeven:
                    output = string.Format("# connections opened:    {0,10}", this.TotalCountRequestSent);
                    break;
            }

            this.sideBySideLine++;
            return output;
        }

        /// <summary>
        /// Converts to string representation.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            string result = this.HttpLogs.Aggregate(string.Empty, (current, logItem) => current + (logItem.ToString() + "\r\n"));

            result += Separator;
            result += "                TOTAL\n";
            this.StartSxSOutput();
            for (HttpReportLines i = HttpReportLines.LineZero; i < HttpReportLines.HTTPReportMaxLine; i++)
            {
                result += this.GetSxSLine();
                result += "\n";
            }

            result += Separator;

            return result;
        }

        #endregion
    }
}
