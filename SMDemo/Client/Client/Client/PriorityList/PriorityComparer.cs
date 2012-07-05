//-----------------------------------------------------------------------
// <copyright file="PriorityComparer.cs" company="Microsoft Open Technologies, Inc.">
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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Comparer for stream priority records
    /// </summary>
    public class PriorityComparer : IComparer<PriorityRecord>
    {
        #region Private Fields

        /// <summary>
        /// Sort field
        /// </summary>
        private PrioritySortField sortField;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityComparer"/> class.
        /// </summary>
        /// <param name="sortF">sort field enum.</param>
        public PriorityComparer(PrioritySortField sortF)
        {
            this.sortField = sortF;
        }

        #endregion

        /// <summary>
        /// Compare method
        /// </summary>
        /// <param name="lhv">left-hand value</param>
        /// <param name="rhv">right-hand value</param>
        /// <returns>Returns 1 if left is higher, -1 if left is lower, 0 if equal</returns>
        public int Compare(PriorityRecord lhv, PriorityRecord rhv)
        {
            if (this.sortField == PrioritySortField.PRIORITY_NAME)
            {
                if (lhv.Level < rhv.Level)
                {
                    // left is higher priority
                    return 1;
                }

                if (lhv.Level > rhv.Level)
                {
                    // right is higher priority
                    return -1;
                }
            }

            if ((this.sortField == PrioritySortField.NAME) ||
                (this.sortField == PrioritySortField.PRIORITY_NAME))
            {
                // compare by original name
                return lhv.OriginalName.CompareTo(rhv.OriginalName);
            }

            if (this.sortField == PrioritySortField.STREAMID)
            {
                if (lhv.StreamId < rhv.StreamId)
                {
                    // right is higher id
                    return 1;
                }

                if (lhv.StreamId > rhv.StreamId)
                {
                    // left is higher id
                    return -1;
                }
            }

            // default return equal
            return 0;
        }
    }
}
