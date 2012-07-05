//-----------------------------------------------------------------------
// <copyright file="ContentTypes.cs" company="Microsoft Open Technologies, Inc.">
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
namespace Client
{
    using System.IO;

    /// <summary>
    /// Content types helper class
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// Plain text definition
        /// </summary>
        public const string TextPlain = "text/plain";

        /// <summary>
        /// CSS text definition
        /// </summary>
        public const string TextCss = "text/css";

        /// <summary>
        /// HTML text definition
        /// </summary>
        public const string TextHtml = "text/html";

        /// <summary>
        /// Script text definition
        /// </summary>
        public const string TextScript = "text/script";

        /// <summary>
        /// Returns file type from file extension.
        /// </summary>
        /// <param name="name">File extension name.</param>
        /// <returns>File type.</returns>
        public static string GetTypeFromFileName(string name)
        {
            switch (Path.GetExtension(name))
            {
                case ".html":
                case ".htm":
                    return TextHtml;
                case ".js":
                    return TextScript;
                case ".css":
                    return TextCss;
                default:
                    return TextPlain;
            }                                                            
        }
    }
}
