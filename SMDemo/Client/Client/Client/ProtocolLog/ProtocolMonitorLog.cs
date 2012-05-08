//-----------------------------------------------------------------------
// <copyright file="ProtocolMonitorLog.cs" company="Microsoft Open Technologies, Inc.">
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
