//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Client.Utils
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Setup for console window client runs in 
    /// </summary>
    public static class NativeMethods
    {
        /// <summary>
        /// Constant for std input
        /// </summary>
        private const int STD_INPUT_HANDLE = -10;

        /// <summary>
        /// Constant for QuickEdit mode in console
        /// </summary>
        private const int ENABLE_QUICK_EDIT_MODE = 0x40 | 0x80;

        /// <summary>
        /// Set console Quick Edit mode for the console client
        /// </summary>
        public static void EnableQuickEditMode()
        {
            int mode;
            IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(handle, mode);
        }

        /// <summary>
        /// Native Win32 GetConsoleMode API
        /// </summary>
        /// <param name="hConsoleHandle">Console Win43 HANDLE</param>
        /// <param name="mode">Console mode</param>
        /// <returns>The return code.</returns>
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        /// <summary>
        /// Native Win32 GetStdHandle API
        /// </summary>
        /// <param name="handle">STDIN number</param>
        /// <returns>The Win32 console HANDLE.</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int handle);

        /// <summary>
        /// Native Win32 SetConsoleMode API
        /// </summary>
        /// <param name="hConsoleHandle">Console Win43 HANDLE</param>
        /// <param name="mode">Console mode</param>
        /// <returns>The return code.</returns>
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
    }
}
