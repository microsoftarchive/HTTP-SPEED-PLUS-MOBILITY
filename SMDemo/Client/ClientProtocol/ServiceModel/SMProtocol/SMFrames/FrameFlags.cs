//-----------------------------------------------------------------------
// <copyright file="BinaryHelper.cs" company="Microsoft Open Technologies, Inc.">
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

namespace ClientProtocol.ServiceModel.SMProtocol.SMFrames
{
	/// <summary>
	/// Frame flags.
	/// </summary>
	public enum FrameFlags: byte
	{
		FlagNormal = 0x00,
        /// <summary>
        /// marks this frame as the last frame to be
        ///transmitted on this stream and puts the sender in the half-closed
        /// </summary>
		FlagFin = 0x01,
		/// <summary>
		/// Header frame headers without compression (for HEADERS frame).
		/// </summary>
		FlagNoHeaderCompression2 = 0x02,
        /// <summary>
        /// Control frame headers without compression (for SYN_STREAM - SYN_REPLY).
        /// </summary>
        FlagNoHeaderCompression1 = 0x04,
		///// <summary>
		///// Compression without adaptive dictionary.
		///// </summary>
		FlagNoStatefulDictionary = 0x06
	}
}
