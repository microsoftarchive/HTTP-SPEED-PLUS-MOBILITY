//-----------------------------------------------------------------------
// <copyright file="SMHeaders.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stream headers collection.
    /// </summary>
    [Serializable]
    public class SMHeaders : Dictionary<string, string>
    {
        /// <summary>
        /// Path HTTP header constant
        /// </summary>   
        public const string Path = ":path";

        /// <summary>
        /// Method HTTP header constant
        /// </summary>   
        public const string Method = ":method";

        /// <summary>
        /// Version HTTP header constant
        /// </summary>   
        public const string Version = ":version";

        /// <summary>
        /// Host HTTP header constant
        /// </summary>   
        public const string Host = ":host";

        /// <summary>
        /// Scheme HTTP header constant
        /// </summary>   
        public const string Scheme = ":scheme";

        /// <summary>
        /// Status HTTP header constant
        /// </summary>   
        public const string Status = ":status";

        /// <summary>
        /// User-Agent HTTP header constant
        /// </summary>   
        public const string UserAgent = ":user-agent";

        /// <summary>
        /// Accept-encoding HTTP header constant
        /// </summary>   
        public const string AcceptEncoding = ":accept-encoding";

        /// <summary>
        /// Content-type HTTP header constant
        /// </summary>   
        public const string ContentType = ":content-type";

        /// <summary>
        /// Initializes a new instance of the <see cref="SMHeaders"/> class.
        /// </summary>
        public SMHeaders()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMHeaders"/> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The serialization context.</param>
        protected SMHeaders(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Indexer for SMHeaders class
        /// </summary>
        /// <param name="key">New or existing key</param>
        /// <returns>New string</returns>
        public new string this[string key]
        {
            get 
            {
                return ContainsKey(key) ? base[key] : null;
            }

            set
            {
                if (ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>returns string representation.</returns>
        public override string ToString()
        {
            string strHeader = string.Empty;

            if (!string.IsNullOrEmpty(this[Path]))
            {
                strHeader += string.Format("path: {0} \n", this[Path]);
            }

            if (!string.IsNullOrEmpty(this[Method]))
            {
                strHeader += string.Format("method: {0} \n", this[Method]);
            }

            if (!string.IsNullOrEmpty(this[Method]))
            {
                strHeader += string.Format("version: {0} \n", this[Version]);
            }

            if (!string.IsNullOrEmpty(this[Host]))
            {
                strHeader += string.Format("host: {0} \n", this[Host]);
            }

            if (!string.IsNullOrEmpty(this[Scheme]))
            {
                strHeader += string.Format("scheme: {0} \n", this[Scheme]);
            }

            if (!string.IsNullOrEmpty(this[Status]))
            {
                strHeader += string.Format("status: {0} \n", this[Status]);
            }

            if (!string.IsNullOrEmpty(this[UserAgent]))
            {
                strHeader += string.Format("user-agent: {0} \n", this[UserAgent]);
            }

            if (!string.IsNullOrEmpty(this[AcceptEncoding]))
            {
                strHeader += string.Format("accept-encoding: {0} \n", this[AcceptEncoding]);
            }

            if (!string.IsNullOrEmpty(this[ContentType]))
            {
                strHeader += string.Format("content-type: {0} \n", this[ContentType]);
            }

            if (string.IsNullOrEmpty(strHeader))
            {
                strHeader = "{Empty}\n";
            }

            return strHeader;
        }

        /// <summary>
        /// Merge two instances of SMHeaders
        /// </summary>
        /// <param name="headers">SMHeaders object</param>
        public void Merge(SMHeaders headers)
        {
            foreach (var smheader in headers)
            {
                this[smheader.Key] = smheader.Value;
            }
        }
    }
}
