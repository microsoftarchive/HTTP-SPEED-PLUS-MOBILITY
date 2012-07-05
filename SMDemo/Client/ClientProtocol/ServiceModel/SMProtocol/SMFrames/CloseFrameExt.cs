//-----------------------------------------------------------------------
// <copyright file="CloseFrameExt.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol.SMFrames
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// WebSocket Close Frame Extension Data.
    /// </summary>
    public class CloseFrameExt
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets status code.
        /// </summary>        
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets last good session id.
        /// </summary>        
        public int LastGoodSessionId { get; set; }

        #endregion
    }
}
