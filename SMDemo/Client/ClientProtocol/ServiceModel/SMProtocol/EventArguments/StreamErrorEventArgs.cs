//-----------------------------------------------------------------------
// <copyright file="StreamErrorEventArgs.cs" company="Microsoft Open Technologies, Inc.">
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
    /// Stream error event arguments.
    /// </summary>
    public class StreamErrorEventArgs : StreamEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamErrorEventArgs"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="exeption">The exeption.</param>
        public StreamErrorEventArgs(SMStream stream, Exception exeption)
            : base(stream)
        {
            this.Exeption = exeption;
        }

        /// <summary>
        /// Gets or sets the exeption.
        /// </summary>
        /// <value>
        /// The exeption.
        /// </value>
        public Exception Exeption { get; set; }
    }
}
