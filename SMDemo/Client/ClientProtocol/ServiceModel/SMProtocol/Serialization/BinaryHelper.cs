//-----------------------------------------------------------------------
// <copyright file="BinaryHelper.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol
{
    /// <summary>
    /// Binary operations helper class.
    /// </summary>
    internal static class BinaryHelper
    {
        /// <summary>
        /// Converts array of bytes to int32
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>The Int32 number.</returns>
        public static Int32 Int32FromBytes(params byte[] bytes)
        {
            Int32 result = 0;
            for (int i = bytes.Length - 1; i >= 0; --i)
            {
                result |= bytes[i] << (8 * i);
            }

            return result;
        }

        /// <summary>
        /// Converts array of bytes to int32
        /// </summary>
        /// <param name="bytes">ArraySegment array</param>
        /// <returns>The Int32 number.</returns>
        public static Int32 Int32FromBytes(ArraySegment<byte> bytes)
        {
            return Int32FromBytes(bytes, 0);
        }

        /// <summary>
        /// Converts array of bytes to int32
        /// </summary>
        /// <param name="bytes">ArraySegment array</param>
        /// <param name="ignoreFirstBitsNum">Number of bits to ignore</param>
        /// <returns>The Int32 number.</returns>
        public static Int32 Int32FromBytes(ArraySegment<byte> bytes, int ignoreFirstBitsNum)
        {
            Int32 result = 0;
            for (int i = 0; i < bytes.Count; ++i)
            {
                byte b = bytes.Array[i + bytes.Offset];
                if (i == 0 && ignoreFirstBitsNum > 0)
                {
                    b &= (byte)(0xFF >> ignoreFirstBitsNum);
                }

                result = result << 8;
                result |= b;
            }

            return result;
        }

        /// <summary>
        /// Converts int32 to byte array
        /// </summary>
        /// <param name="value">Int32 value</param>
        /// <param name="bytes">ArraySegment array to fill</param>
        public static void Int32ToBytes(Int32 value, ArraySegment<byte> bytes)
        {
            for (int i = 0; i < bytes.Count; ++i)
            {
                bytes.Array[bytes.Count - 1 - i + bytes.Offset] = (byte)((value & (0xFF << (i << 3))) >> (i * 8));
            }
        }

        /// <summary>
        /// Creates array of bytes, then converts int32 to the array
        /// </summary>
        /// <param name="value">Int32 value</param>
        /// <param name="bytesNum">Number of bytes </param>
        /// <returns>Array of bytes of size bytesNum.</returns>
        public static byte[] Int32ToBytes(Int32 value, int bytesNum)
        {
            var bytes = new byte[bytesNum];
            for (int i = 0; i < bytesNum; ++i)
            {
                bytes[bytesNum - 1 - i] = (byte)((value & (0xFF << (i << 3))) >> (i * 8));
            }

            return bytes;
        }

        /// <summary>
        /// Converts int32 to the array of bytes size 4
        /// </summary>
        /// <param name="value">Int32 value</param>
        /// <returns>Array of bytes of size 4.</returns>
        public static byte[] Int32ToBytes(Int32 value)
        {
            return Int32ToBytes(value, 4);
        }

        /// <summary>
        /// Converts int16 to the array of bytes size 2
        /// </summary>
        /// <param name="value">Int16 value</param>
        /// <returns>Array of bytes of size 2.</returns>
        public static byte[] Int16ToBytes(Int16 value)
        {
            return Int32ToBytes(value, 2);
        }

        /// <summary>
        /// Converts array of bytes to Int16
        /// </summary>
        /// <param name="msByte">Most significant byte</param>
        /// <param name="lsByte">Least significant byte</param>
        /// <returns>Int16 integer.</returns>
        public static Int16 Int16FromBytes(byte msByte, byte lsByte)
        {
            return Int16FromBytes(msByte, lsByte, 0);
        }

        /// <summary>
        /// Converts array of bytes to Int16, ignoring higher bits
        /// </summary>
        /// <param name="msByte">Most significant byte</param>
        /// <param name="lsByte">Least significant byte</param>
        /// <param name="ignoreFirstBitsNum">Number of higher bits to ignore</param>
        /// <returns>Int16 integer.</returns>
        public static Int16 Int16FromBytes(byte msByte, byte lsByte, int ignoreFirstBitsNum)
        {
            if (ignoreFirstBitsNum > 0)
            {
                msByte &= (byte)(0xFF >> ignoreFirstBitsNum);
            }

            return (Int16)((msByte << 8) | lsByte);
        }
    }
}
