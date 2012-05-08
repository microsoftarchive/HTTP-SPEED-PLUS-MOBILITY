//-----------------------------------------------------------------------
// <copyright file="FrameLogItem.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.ServiceModel.SMProtocol.SMFrames;

    /// <summary>
    /// Log item for frame.
    /// </summary>
    public struct FrameLogItem
    {
        /// <summary>
        /// Time stamp
        /// </summary>   
        public DateTime TimeStamp;

        /// <summary>
        /// Duration of frame
        /// </summary>   
        public TimeSpan Duration;

        /// <summary>
        /// Length of frame
        /// </summary>   
        public long Length;

        /// <summary>
        /// Frame headers
        /// </summary>   
        public string Headers;

        /// <summary>
        /// Stream Id
        /// </summary>   
        public int StreamId;

        /// <summary>
        /// Frame type
        /// </summary>   
        public FrameType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameLogItem"/> struct.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public FrameLogItem(BaseFrame frame)
        {
            this.Length = frame.Length;
            this.StreamId = frame.StreamId;

            if (frame is ControlFrame)
            {
                this.Type = ((ControlFrame)frame).Type;
                this.Headers = ((ControlFrame)frame).Headers.ToString();
                this.TimeStamp = DateTime.Now;
                this.Duration = TimeSpan.Zero;
            }
            else
            {
                this.Type = FrameType.Data;
                this.TimeStamp = DateTime.Now;
                this.Duration = TimeSpan.Zero;
                this.Headers = string.Empty;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = "--------------------\r\n";
            result += "Frame:\r\n";
            result += "Type: " + this.Type + "\r\n";
            result += "TimeStamp: " + this.TimeStamp + "\r\n";
            result += "Length: " + this.Length + "\r\n";
            result += "StreamId: " + this.StreamId + "\r\n";
            if (this.Duration.TotalMilliseconds > 0)
            {
                result += "Duration(ms): " + this.Duration.TotalMilliseconds + "\r\n";
            }

            result += "Headers: " + this.Headers + "\r\n";
            result += "--------------------";

            return result;
        }
    }
}
