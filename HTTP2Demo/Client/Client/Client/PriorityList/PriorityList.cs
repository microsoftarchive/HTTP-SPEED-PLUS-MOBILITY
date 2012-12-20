//-----------------------------------------------------------------------
// <copyright file="PriorityList.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Collections.Generic;

    /// <summary>
    /// Collection of stream descritions with priorities
    /// </summary>
    public class PriorityList
    {
        /// <summary>
        /// Sent to control flow when all streams are completed
        /// </summary>
        public const int NOMORESTREAMS = -1;

        /// <summary>
        /// Sent to control flow when number of registered streams is more than completed streams 
        /// </summary>
        public const int WAITFORMORESTREAMS = -2;

        /// <summary>
        /// List of stream priority records
        /// </summary>
        private List<PriorityRecord> streamList = new List<PriorityRecord>();

        /// <summary>
        /// Count of opened streams
        /// </summary>
        private int openStreamCount = 0;

        /// <summary>
        /// Count of completed streams
        /// </summary>
        private int completedStreamCount = 0;

        /// <summary>
        /// Current stream index
        /// </summary>
        private int currentRecordIndex = -1;

        /// <summary>
        /// Add stream priority record
        /// </summary>
        /// <param name="rec">Record to find</param>
        public void Add(PriorityRecord rec)
        {
            this.streamList.Add(rec);
        }

        /// <summary>
        /// Sets stream to open status
        /// </summary>
        /// <param name="name">Name of original file</param>
        /// <param name="streamId">Stream Id</param>
        public void StartStream(string name, int streamId)
        {
            int result = this.streamList.BinarySearch(new PriorityRecord(name), new PriorityComparer(PrioritySortField.NAME));
            if (result >= 0)
            {
                PriorityRecord pr = this.streamList[result];
                pr.StreamId = streamId;
                pr.StreamOpened = true;
                this.streamList[result] = pr;
                this.openStreamCount++;
            }
        }

        /// <summary>
        /// Sets stream to closed status if it can find such stream
        /// </summary>
        /// <param name="name">Name of original file</param>
        public void CloseStream(string name)
        {
            int result = this.streamList.BinarySearch(new PriorityRecord(name), new PriorityComparer(PrioritySortField.NAME));
            if (result >= 0)
            {
                PriorityRecord pr = this.streamList[result];
                pr.StreamOpened = false;
                this.streamList[result] = pr;
                this.openStreamCount--;
                this.completedStreamCount++;
            }
        }

        /// <summary>
        /// sort list using custom comparer
        /// </summary>
        public void SortByPriority()
        {
            PriorityComparer pc = new PriorityComparer(PrioritySortField.PRIORITY_NAME);
            this.streamList.Sort(pc);
        }

        /// <summary>
        /// sort list using name of original file
        /// </summary>
        public void SortByName()
        {
            PriorityComparer pc = new PriorityComparer(PrioritySortField.NAME);
            this.streamList.Sort(pc);
        }

        /// <summary>
        /// Returns count of opened streams
        /// </summary>
        /// <returns>Returns number of open streams</returns>
        public int CountOpenStreams()
        {
            return this.openStreamCount;
        }

        /// <summary>
        /// Returns stream id of next open stream
        /// </summary>
        /// <returns>Stream id</returns>
        public int GetNextOpenStream()
        {
            if (this.streamList.Count == 0)
            {
                this.currentRecordIndex = NOMORESTREAMS;
                return NOMORESTREAMS;
            }

            this.currentRecordIndex++;
            int startIndex = this.currentRecordIndex;

            while (this.currentRecordIndex < this.streamList.Count)
            {
                if (this.streamList[this.currentRecordIndex].StreamOpened)
                {
                    return this.streamList[this.currentRecordIndex].StreamId;
                }

                this.currentRecordIndex++;
            }

            this.currentRecordIndex = 0;

            while (this.currentRecordIndex < startIndex)
            {
                if (this.streamList[this.currentRecordIndex].StreamOpened)
                {
                    return this.streamList[this.currentRecordIndex].StreamId;
                }

                this.currentRecordIndex++;
            }

            // if all streams are completed, send NO_MORE_STREAMS
            if (this.completedStreamCount == this.streamList.Count)
            {
                return NOMORESTREAMS;
            }

            // send WAIT_FOR_MORE_STREAMS
            return WAITFORMORESTREAMS;
        }

        /// <summary>
        /// Sets current stream active
        /// </summary>
        /// <param name="active">Flag to set stream active</param>
        public void SetCurrentStreamActive(bool active)
        {
            if (this.currentRecordIndex >= 0)
            {
                PriorityRecord pr = this.streamList[this.currentRecordIndex];
                pr.StreamActive = active;
                this.streamList[this.currentRecordIndex] = pr;
            }
        }
    }
}
