//-----------------------------------------------------------------------
// <copyright file="StatusCode.cs" company="Microsoft Open Technologies, Inc.">
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
    /// Protocol status codes.
    /// </summary>
    public enum StatusCode : int
    {
        /// <summary>
        /// Indicates no status. This is not a valid value to receive or send.
        /// </summary>
        None = 0,

        /// <summary>
        /// All is ok.
        /// </summary>
        Success = 1000,

        /// <summary>
        /// This is a generic error, and should only be used if a more specific error is not available.
        /// </summary>
        ProtocolError = 1002,

        /// <summary>
        /// This is returned when a frame is received for a stream which is not active
        /// </summary>
        InvalidStream = 2,

        /// <summary>
        /// Indicates that the stream was refused before any processing has been done on the stream.
        /// </summary>
        RefusedStream = 3,

        /// <summary>
        /// Indicates that the recipient of a stream does not support the SM version requested.
        /// </summary>
        UnsupportedVersion = 4,

        /// <summary>
        /// Used by the creator of a stream to indicate that the stream is no longer needed.
        /// </summary>
        Cancel = 5,

        /// <summary>
        /// This is a generic error which can be used when the implementation has internally failed, not due to anything in the protocol.
        /// </summary>
        InternalError = 6,

        /// <summary>
        /// The endpoint detected that its peer violated the flow control protocol.
        /// </summary>
        FlowControlError = 7,

        /// <summary>
        /// The endpoint received a SYN_REPLY for a stream already open.
        /// </summary>
        StreamInUse = 8,

        /// <summary>
        /// The endpoint received a data or SYN_REPLY frame for a stream which is half closed.
        /// </summary>
        StreamAlreadyClosed = 9,

        /// <summary>
        /// The server received a request for a resource whose origin does not have valid credentials in the client certificate vector.
        /// </summary>
        InvalidCredentials = 10,

        /// <summary>
        /// The endpoint received a frame which this implementation could not support.
        /// </summary>
        FrameTooLarge = 11
    }
}
