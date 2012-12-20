//-----------------------------------------------------------------------
// <copyright file="SettingsEventArgs.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Http2Protocol.ProtocolFrames;
using System.Text;

namespace System.ServiceModel.Http2Protocol
{
    public class SettingsEventArgs : ControlFrameEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEventArgs"/> class.
        /// </summary>
        /// <param name="stream">The settings name/value pairs.</param>
        public SettingsEventArgs(ControlFrame settingsFrame)
            : base(settingsFrame)
        {
            this.SettingsHeaders = settingsFrame.SettingsHeaders;
        }

        /// <summary>
        /// Gets the settings name/value pairs.
        /// </summary>
        /// <value>
        /// The dictionary.
        /// </value>
        public Dictionary<Int32, Int32> SettingsHeaders { get; private set; }
    }

    /// <summary>
    /// Ids used in settings frame.
    /// </summary>
    public enum SettingsIds
    {
        SETTINGS_NONE = 0,
        SETTINGS_UPLOAD_BANDWIDTH = 1,
        SETTINGS_DOWNLOAD_BANDWIDTH = 2,
        SETTINGS_ROUND_TRIP_TIME = 3,
        SETTINGS_MAX_CONCURRENT_STREAMS = 4,
        SETTINGS_CURRENT_CWND = 5,
        SETTINGS_DOWNLOAD_RETRANS_RATE = 6,
        SETTINGS_INITIAL_WINDOW_SIZE = 7,
        SETTINGS_CLIENT_CERTIFICATE_VECTOR_SIZE = 8
    }
}
