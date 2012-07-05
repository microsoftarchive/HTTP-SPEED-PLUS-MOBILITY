//-----------------------------------------------------------------------
// <copyright file="PriorityList.cs" company="Microsoft Open Technologies, Inc.">
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
