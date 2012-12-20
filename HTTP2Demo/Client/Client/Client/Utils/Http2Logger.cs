// -----------------------------------------------------------------------
// <copyright file="Http2Logger.cs" company="Microsoft Open Technologies, Inc.">
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
// -----------------------------------------------------------------------
namespace Client.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Http2 logger class
    /// </summary>
    public class Http2Logger
    {
        #region Fields

        /// <summary>
        /// Gets or sets logging level.
        /// </summary>
        public static Http2LoggerState LoggerLevel { get; set; }

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
            if (LoggerLevel > Http2LoggerState.NoLogging)
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
            if ((LoggerLevel > Http2LoggerState.NoLogging) && (LoggerConsole == true))
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
            if (LoggerLevel >= Http2LoggerState.VerboseLogging)
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
            if (LoggerLevel >= Http2LoggerState.DebugLogging)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("T") + "] DBG:" + debugString);
            }
        }

        #endregion
    }
}
