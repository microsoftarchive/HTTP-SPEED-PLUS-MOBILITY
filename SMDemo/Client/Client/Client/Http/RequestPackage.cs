//-----------------------------------------------------------------------
// <copyright file="RequestPackage.cs" company="Microsoft Open Technologies, Inc.">
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
namespace Client.Http
{
    using System;
    using System.Threading;

    /// <summary>
    /// Http download request. 
    /// </summary>
    public struct RequestPackage
    {
        /// <summary>
        /// Event to signal end of download
        /// </summary>
        public ManualResetEvent LoaderEvent;

        /// <summary>
        /// Type of document to load
        /// </summary>
        public string Type;

        /// <summary>
        /// name of document to load
        /// </summary>
        public string Name;

        /// <summary>
        /// Full path of document
        /// </summary>
        public Uri RequestUri;

        /// <summary>
        /// Download thread id
        /// </summary>
        public int ThreadId;

        /// <summary>
        /// Flag for post-download processing
        /// </summary>
        public int Processed;
    }
}
