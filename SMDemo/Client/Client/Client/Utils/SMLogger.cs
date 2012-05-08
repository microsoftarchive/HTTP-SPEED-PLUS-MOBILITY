// -----------------------------------------------------------------------
// <copyright file="SMLogger.cs" company="Microsoft Open Technologies, Inc.">
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
// -----------------------------------------------------------------------
namespace Client.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// SM logger class
    /// </summary>
    public class SMLogger
    {
        #region Fields

        /// <summary>
        /// Gets or sets logging level.
        /// </summary>
        public static SMLoggerState LoggerLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether interactive console mode is on.
        /// </summary>
        public static bool LoggerConsole { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Logs error.
        /// </summary>
        /// <param name="errString">String to log</param>
        public static void LogError(string errString)
        {
            if (LoggerLevel > SMLoggerState.NoLogging)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("T") + "] ERROR:" + errString);
            }
        }

        /// <summary>
        /// Console output messages for interactive users.
        /// </summary>
        /// <param name="consoleString">String to display</param>
        public static void LogConsole(string consoleString)
        {
            if ((LoggerLevel > SMLoggerState.NoLogging) && (LoggerConsole == true))
            {
                Console.WriteLine("[" + DateTime.Now.ToString("T") + "] " + consoleString);
            }
        }

        /// <summary>
        /// Logs informational message.
        /// </summary>
        /// <param name="infoString">String to log</param>
        public static void LogInfo(string infoString)
        {
            if (LoggerLevel >= SMLoggerState.VerboseLogging)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("T") + "] INFO:" + infoString);
            }
        }

        /// <summary>
        /// Logs debug message.
        /// </summary>
        /// <param name="debugString">String to log</param>
        public static void LogDebug(string debugString)
        {
            if (LoggerLevel >= SMLoggerState.DebugLogging)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("T") + "] DBG:" + debugString);
            }
        }

        #endregion
    }
}
