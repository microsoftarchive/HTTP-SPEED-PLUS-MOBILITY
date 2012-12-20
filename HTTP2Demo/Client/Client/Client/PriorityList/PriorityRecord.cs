//-----------------------------------------------------------------------
// <copyright file="PriorityRecord.cs" company="Microsoft Open Technologies, Inc.">
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
