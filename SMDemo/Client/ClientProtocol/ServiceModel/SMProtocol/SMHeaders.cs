//-----------------------------------------------------------------------
// <copyright file="SMHeaders.cs" company="Microsoft Open Technologies, Inc.">
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
