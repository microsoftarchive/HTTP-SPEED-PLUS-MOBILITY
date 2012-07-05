//-----------------------------------------------------------------------
// <copyright file="SMProtocolOptions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace System.ServiceModel.SMProtocol
{
	/// <summary>
	/// SMProtocolOptions class
	/// </summary>
	public class SMProtocolOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SMProtocolOptions"/> class.
		/// </summary>
		public SMProtocolOptions()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SMProtocolOptions"/> class.
		/// </summary>
		/// <param name="str">Options string representation.</param>
		public SMProtocolOptions(string str) : this(str, string.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SMProtocolOptions"/> class.
		/// </summary>
		/// <param name="compression">Compression flag</param>
		/// <param name="quantum">Credit Update size</param>
        public SMProtocolOptions(string compression, string quantum)
        {
			if (string.IsNullOrEmpty(compression))
			{
				this.UseCompression = false;
				this.CompressionIsStateful = false;
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
				this.CreditAddition = Convert.ToUInt32(quantum);
            }
			else
			{
				this.IsFlowControl = false;
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
        public UInt32 CreditAddition { get; set; }
	}
}
