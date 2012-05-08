//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft Open Technologies, Inc.">
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
