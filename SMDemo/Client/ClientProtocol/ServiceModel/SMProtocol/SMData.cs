//-----------------------------------------------------------------------
// <copyright file="SMData.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Text;

    /// <summary>
    /// Stream data helper class.
    /// </summary>
    public class SMData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SMData"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public SMData(byte[] data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMData"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="encoding">The encoding.</param>
        public SMData(string text, Encoding encoding)
        {
            this.Data = encoding.GetBytes(text);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMData"/> class.
        /// </summary>
        /// <param name="utf8Text">The UTF8 text.</param>
        public SMData(string utf8Text)
        {
            this.Data = Encoding.UTF8.GetBytes(utf8Text);
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; set; }

        /// <summary>
        /// Returns data as UTF8 string.
        /// </summary>
        /// <returns>Encoded string</returns>
        public string AsUtf8Text()
        {
            return Encoding.UTF8.GetString(this.Data);
        }
    }
}
