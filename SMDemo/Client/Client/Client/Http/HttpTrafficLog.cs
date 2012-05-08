//-----------------------------------------------------------------------
// <copyright file="HttpTrafficLog.cs" company="Microsoft Open Technologies, Inc.">
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
