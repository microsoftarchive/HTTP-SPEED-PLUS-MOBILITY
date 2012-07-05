//-----------------------------------------------------------------------
// <copyright file="IStreamStore.cs" company="Microsoft Open Technologies, Inc.">
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

    /// <summary>
    /// Streams store interface.
    /// </summary>
    internal interface IStreamStore
    {
        /// <summary>
        /// Gets the streams collection.
        /// </summary>
        ICollection<SMStream> Streams { get; }

        /// <summary>
        /// Gets the stream by id.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <returns>The stream.</returns>
        SMStream GetStreamById(int streamId);
    }
}
