//-----------------------------------------------------------------------
// <copyright file="ControlFrame.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;

namespace System.ServiceModel.Http2Protocol.ProtocolFrames
{
    /// <summary>
    /// Class that represents control frame.
    /// </summary>
    public class ControlFrame : BaseFrame
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlFrame"/> class.
        /// </summary>
        public ControlFrame()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlFrame"/> class.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public ControlFrame(ProtocolHeaders headers)
        {
            this.IsControl = true;
            this.Version = Http2Protocol.Version;
            this.Headers = headers ?? new ProtocolHeaders();
        }

        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FrameType Type { get; set; }

        public Int32 NumberOfEntries { get; set; }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        public ProtocolHeaders Headers { get; private set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public byte Priority { get; set; }

        /// <summary>
        /// Gets or sets the slot.
        /// </summary>
        /// <value>
        /// The slot.
        /// </value>
        public byte Slot { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public short Version { get; set; }

        /// <summary>
        /// Gets or sets the settings headers.
        /// </summary>
        /// <value>
        /// The settings headers.
        /// </value>
        public Dictionary<Int32, Int32> SettingsHeaders { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the delta window.
        /// </summary>
        /// <value>
        /// The size of the delta window.
        /// </value>
        public Int64 DeltaWindowSize { get; set; }

        /// <summary>
        /// Gets or sets the associated to stream id.
        /// </summary>
        /// <value>
        /// The associated to stream id.
        /// </value>
        public Int32 AssociatedToStreamId { get; set; }

        #endregion
    }
}
