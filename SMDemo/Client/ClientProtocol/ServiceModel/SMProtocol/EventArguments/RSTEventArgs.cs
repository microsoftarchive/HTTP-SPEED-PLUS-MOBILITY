//-----------------------------------------------------------------------
// <copyright file="RSTEventArgs.cs" company="Microsoft Open Technologies, Inc.">
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
    /// RST stream event argument class.
    /// </summary>
    public class RSTEventArgs : StreamEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RSTEventArgs"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="reason">The reason for RST.</param>
        public RSTEventArgs(SMStream stream, StatusCode reason)
            : base(stream)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public StatusCode Reason { get; set; }
    }
}
