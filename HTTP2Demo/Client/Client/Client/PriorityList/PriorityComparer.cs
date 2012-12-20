//-----------------------------------------------------------------------
// <copyright file="PriorityComparer.cs" company="Microsoft Open Technologies, Inc.">
//
// The copyright in this software is being made available under the BSD License, included below. 
// This software may be subject to other third party and contributor rights, including patent rights, 
// and no such rights are granted under this license.
//
// Copyright (c) 2012, Microsoft Open Technologies, Inc. 
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice, 
//   this list of conditions and the following disclaimer in the documentation 
//   and/or other materials provided with the distribution.
// - Neither the name of Microsoft Open Technologies, Inc. nor the names of its contributors 
//   may be used to endorse or promote products derived from this software 
//   without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
