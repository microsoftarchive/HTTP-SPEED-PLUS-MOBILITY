//-----------------------------------------------------------------------
// <copyright file="HttpHeaderLogItem.cs" company="Microsoft Open Technologies, Inc.">
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
namespace Client.HttpBenchmark
{
    using System.Text;

    /// <summary>
    /// Http header log.
    /// </summary>
    public class HttpHeaderLogItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderLogItem"/> class.
        /// </summary>
        /// <param name="header">the headers byte array.</param>
        public HttpHeaderLogItem(byte[] header)
        {
            this.Header = Encoding.UTF8.GetString(header).Trim('\r', '\n');
            this.Length = header.Length;
        }

        /// <summary>
        /// Gets or sets header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets length.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>returns string representation.</returns>
        public override string ToString()
        {
            string result = "--------------------\r\n";
            result += "Headers: " + this.Header + "\r\n";
            result += "Length: " + this.Length + "\r\n";
            result += "--------------------\r\n";
            return result;
        }
    }
}
