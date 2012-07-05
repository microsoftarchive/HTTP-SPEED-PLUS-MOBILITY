// -----------------------------------------------------------------------
// <copyright file="PriorityLevel.cs" company="Microsoft Open Technologies, Inc.">
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
// -----------------------------------------------------------------------

namespace Client.Utils
{
    /// <summary>
    /// Stream priority level
    /// </summary>
    public enum PriorityLevel
    {
        /// <summary>
        /// high priority stream, like HTML, CSS and JS
        /// </summary>
        HighPriority,

        /// <summary>
        /// Medium priority stream like contents of 'a' HTML tag
        /// </summary>
        MediumPriority,

        /// <summary>
        /// Low priority stream, like images
        /// </summary>
        LowPriority
    }
}
