//-----------------------------------------------------------------------
// <copyright file="DataFrame.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.ServiceModel.SMProtocol.SMFrames;

    public class CreditUpdateFrame: ControlFrame
    {
        /// <summary>
        /// Gets or sets the credit addition.
        /// </summary>
        /// <value>
        /// The credit addition.
        /// </value>
        public Int64 CreditAddition { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditUpdateFrame"/> class.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="addition">The addition.</param>
        public CreditUpdateFrame(int streamId, Int64 addition)
            :base(null)
        {
            StreamId = streamId;
            CreditAddition = addition;
            Type = FrameType.CreditUpdate;
        }
    }
}
