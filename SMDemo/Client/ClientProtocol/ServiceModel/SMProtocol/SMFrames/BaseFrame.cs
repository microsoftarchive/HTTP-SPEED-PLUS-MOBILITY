//-----------------------------------------------------------------------
// <copyright file="BaseFrame.cs" company="Microsoft Open Technologies, Inc.">
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
    /// <summary>
    /// Base class fro control and data frames.
    /// </summary>
    public class BaseFrame
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether control frame attribute is true.
        /// </summary>        
        public bool IsControl { get; protected set; }

        /// <summary>
        /// Gets or sets flags.
        /// </summary>        
        public byte Flags { get; set; }

        /// <summary>
        /// Gets or sets stream id.
        /// </summary>        
        public int StreamId { get; set; }

        /// <summary>
        /// Gets or sets length of frame.
        /// </summary>        
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether final attribute is true.
        /// </summary>        
        public bool IsFinal { get; set; }

        #endregion
    }
}
