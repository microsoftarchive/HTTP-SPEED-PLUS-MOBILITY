//-----------------------------------------------------------------------
// <copyright file="StreamDataEventArgs.cs" company="Microsoft Open Technologies, Inc.">
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
    /// Stream data event arguments.
    /// </summary>
    public class StreamDataEventArgs : StreamEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDataEventArgs"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <param name="isFin">final flag.</param>
        public StreamDataEventArgs(SMStream stream, SMData data, bool isFin)
            : base(stream)
        {
            this.Data = data;
            this.IsFin = isFin;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public SMData Data { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this the last data on the stream.
        /// </summary>
        public bool IsFin { get; set; }
    }
}
