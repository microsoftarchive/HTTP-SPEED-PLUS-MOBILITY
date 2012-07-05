//-----------------------------------------------------------------------
// <copyright file="PriorityRecord.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Client.Utils
{
    /// <summary>
    /// Priority description for download stream.
    /// </summary>
    public struct PriorityRecord
    {
        #region Public Fields

        /// <summary>
        /// Priority of this stream
        /// </summary>
        public PriorityLevel Level;

        /// <summary>
        /// Original file name
        /// </summary>
        public string OriginalName;

        /// <summary>
        /// Stream id for the file
        /// </summary>
        public int StreamId;

        /// <summary>
        /// Stream is opened or closed.
        /// </summary>
        public bool StreamOpened;

        /// <summary>
        /// Stream is active.
        /// </summary>
        public bool StreamActive;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityRecord"/> struct.
        /// </summary>
        /// <param name="l">Priority level.</param>
        /// <param name="on">Original file name.</param>
        public PriorityRecord(PriorityLevel l, string on)
        {
            this.Level = l;
            this.OriginalName = on;
            this.StreamId = 0;
            this.StreamOpened = false;
            this.StreamActive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityRecord"/> struct.
        /// </summary>
        /// <param name="on">Original file name.</param>
        public PriorityRecord(string on)
        {
            this.Level = PriorityLevel.HighPriority;
            this.OriginalName = on;
            this.StreamId = 0;
            this.StreamOpened = false;
            this.StreamActive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityRecord"/> struct.
        /// </summary>
        /// <param name="sid">Stream Id.</param>
        public PriorityRecord(int sid)
        {
            this.Level = PriorityLevel.HighPriority;
            this.OriginalName = string.Empty;
            this.StreamId = sid;
            this.StreamOpened = false;
            this.StreamActive = false;
        }

        #endregion
    }
}
