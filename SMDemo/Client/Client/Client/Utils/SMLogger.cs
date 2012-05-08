// -----------------------------------------------------------------------
// <copyright file="SMLogger.cs" company="Microsoft Open Technologies, Inc.">
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
