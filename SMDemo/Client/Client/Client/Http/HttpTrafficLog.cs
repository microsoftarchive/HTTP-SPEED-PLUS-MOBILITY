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
        /// <summary>
        /// Separator constant.
        /// </summary>
        private const string Separator = "______________________________________\n";

        /// <summary>
        /// List of http logs.
        /// </summary>
        private readonly List<HttpHeaderLogItem> httpLogs = new List<HttpHeaderLogItem>();

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

        /// <summary>
        /// Logs request headers.
        /// </summary>
        /// <param name="header">the headers byte array.</param>
        public void LogRequest(byte[] header)
        {
            this.HttpLogs.Add(new HttpHeaderLogItem(header));

            this.TotalCountRequestSent++;
            this.TotalSizeHeadersSent += header.Length;
        }

        /// <summary>
        /// Logs response headers.
        /// </summary>
        /// <param name="header">the headers byte array.</param>
        /// <param name="totalLength">the length of header.</param>
        public void LogResponse(byte[] header, int totalLength)
        {
            this.HttpLogs.Add(new HttpHeaderLogItem(header));

            this.TotalCountResponseReceived++;
            this.TotalSizeHeadersReceived += header.Length;
            this.TotalSizeDataReceived += totalLength - header.Length;
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
            result += string.Format("Total size of data received(bytes):       {0}\n", this.TotalSizeDataReceived);
            result += string.Format("Total size of headers received(bytes):    {0}\n", this.TotalSizeHeadersReceived);
            result += string.Format("Total size received(bytes):               {0}\n", this.TotalSizeHeadersReceived + this.TotalSizeDataReceived);
            result += string.Format("Total size of headers sent(bytes):        {0}\n", this.TotalSizeHeadersSent);
            result += string.Format("Total size of data exchanged(bytes):      {0}\n", this.TotalSizeHeadersSent + this.TotalSizeHeadersReceived + this.TotalSizeDataReceived);
            result += string.Format("Total count of requests sent:             {0}\n", this.TotalCountRequestSent);
            result += string.Format("Total count of responses received:        {0}\n", this.TotalCountResponseReceived);
            result += string.Format("Total count of connections opened:        {0}\n", this.TotalCountRequestSent);

            result += Separator;

            return result;
        }
    }
}
