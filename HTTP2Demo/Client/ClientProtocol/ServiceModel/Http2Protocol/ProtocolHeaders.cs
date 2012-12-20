//-----------------------------------------------------------------------
// <copyright file="ProtocolHeaders.cs" company="Microsoft Open Technologies, Inc.">
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
//-----------------------------------------------------------------------
namespace System.ServiceModel.Http2Protocol
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stream headers collection.
    /// </summary>
    [Serializable]
    public class ProtocolHeaders : Dictionary<string, string>
    {
        #region Public Constants
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
        /// Cache-control HTTP header constant
        /// </summary>
        public const string CacheControl = ":cache-control";

        /// <summary>
        /// Cookie HTTP header constant
        /// </summary>
        public const string Cookie = ":cookie";


        public const string Connection = ":connection";

        public const string Upgrade = ":upgrade";

        #endregion

        #region Private Constants

        /// <summary>
        /// Valid HTTP cache control strings
        /// </summary>
        private readonly string[] CacheControlValues = new string[] { 
            "no-cache",
            "no-store",
            "max-age=1250",
            "max-stale=99999",
            "min-fresh=8304",
            "no-transform",
            "only-if-cached",
            "public",
            "private",
            "must-revalidate",
            "proxy-revalidate",
            "s-maxage=56",
            "community=\"S+M protocol demo communication group\"",
            "company=\"Microsoft OpenTech\""
        };

        #endregion

        #region Private Fields

        /// <summary>
        /// Random generator for populating exter HTTP headers
        /// </summary>
        static Random RandIndex = new Random((int)DateTime.Now.Ticks);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolHeaders"/> class.
        /// </summary>
        public ProtocolHeaders()
        {
            InitExtraHeaders();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolHeaders"/> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The serialization context.</param>
        protected ProtocolHeaders(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            InitExtraHeaders();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indexer for ProtocolHeaders class
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
                    base[key] = value ?? value.ToLower();
                }
                else
                {
                    Add(key, value ?? value.ToLower());
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

            if (!string.IsNullOrEmpty(this[Connection]))
            {
                strHeader += string.Format("connection: {0} \n", this[Connection]);
            }

            if (!string.IsNullOrEmpty(this[Upgrade]))
            {
                strHeader += string.Format("upgrade: {0} \n", this[Upgrade]);
            }

            if (string.IsNullOrEmpty(strHeader))
            {
                strHeader = "{Empty}\n";
            }

            return strHeader;
        }

        /// <summary>
        /// Merge two instances of ProtocolHeaders
        /// </summary>
        /// <param name="headers">Protocoleaders object</param>
        public void Merge(ProtocolHeaders headers)
        {
            foreach (var smheader in headers)
            {
                this[smheader.Key] = smheader.Value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add headers which are not compulsory, but valid. These headers have no impact 
        /// on transfers. We add then to vary compression and encryption load.
        /// </summary>
        private void InitExtraHeaders()
        {
            // add Cache-Control header. From client side we put one attribute
            //this[CacheControl] = this.CacheControlValues[RandIndex.Next(0, this.CacheControlValues.Length)];
        }

        #endregion

    }
}
