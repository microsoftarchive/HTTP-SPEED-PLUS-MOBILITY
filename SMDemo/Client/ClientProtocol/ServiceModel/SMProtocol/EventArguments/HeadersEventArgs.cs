//-----------------------------------------------------------------------
// <copyright file="HeadersEventArgs.cs" company="Microsoft Open Technologies, Inc.">
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
    /// Headers event arguments.
    /// </summary>
    public class HeadersEventArgs : StreamEventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadersEventArgs"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        public HeadersEventArgs(SMStream stream, SMHeaders headers)
            : base(stream)
        {
            this.Headers = headers;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the headers collection.
        /// </summary>
        public SMHeaders Headers { get; private set; }

        #endregion
    }
}
