//-----------------------------------------------------------------------
// <copyright file="ProtocolOptions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace System.ServiceModel.Http2Protocol
{
	/// <summary>
	/// Http2ProtocolOptions class
	/// </summary>
	public class ProtocolOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolOptions"/> class.
		/// </summary>
		public ProtocolOptions()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolOptions"/> class.
		/// </summary>
		/// <param name="str">Options string representation.</param>
		public ProtocolOptions(string str) : this(str, string.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolOptions"/> class.
		/// </summary>
		/// <param name="compression">Compression flag</param>
		/// <param name="quantum">Credit Update size</param>
        public ProtocolOptions(string compression, string quantum)
        {
            if (string.IsNullOrEmpty(compression))
            {
                this.UseCompression = true;
                this.CompressionIsStateful = true;
                //this.UseCompression = false;
                //this.CompressionIsStateful = false;
            }
            else
            {
                compression = compression.ToLower();
                if (compression.IndexOf("s") != -1)
                {
                    this.UseCompression = true;
                    this.CompressionIsStateful = true;
                }
                else if (compression.IndexOf("c") != -1)
                {
                    this.UseCompression = true;
                    this.CompressionIsStateful = false;
                }
            }

            if (!string.IsNullOrEmpty(quantum))
            {
				this.IsFlowControl = true;
				this.WindowAddition = Convert.ToUInt32(quantum);
            }
			else
			{
				//this.IsFlowControl = false;
                this.IsFlowControl = true;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use compression.
		/// </summary>
		/// <value>
		///   <c>true</c> if use compression; otherwise, <c>false</c>.
		/// </value>
		public bool UseCompression { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether coompression is stateful.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if coompression is stateful; otherwise, <c>false</c>.
		/// </value>
		public bool CompressionIsStateful { get; set; }

        /// <summary>
        /// Gets or sets flow control flag.
        /// </summary>
        public bool IsFlowControl { get; set; }

        /// <summary>
        /// Gets or sets a credit addition.
        /// </summary>
        public UInt32 WindowAddition { get; set; }
	}
}
