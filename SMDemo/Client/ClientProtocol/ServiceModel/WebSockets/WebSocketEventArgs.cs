// <copyright file="WebSocketEventArgs.cs" company="Microsoft Open Technologies, Inc.">
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

namespace System.ServiceModel.WebSockets
{
#if !SILVERLIGHT
#else
    using System.Windows.Browser;
#endif

#if !SILVERLIGHT
#else
    [ScriptableType]
#endif
    /// <summary>
    /// Provides data for the <see cref="OnMessage"/> event.
    /// </summary>
    public class WebSocketEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketEventArgs"/> class.
        /// </summary>
        public WebSocketEventArgs()
        {
        }

        /// <summary>
        /// Gets or sets the text data received on this websocket connection.
        /// </summary>
        public string TextData { get; set; }

        /// <summary>
        /// Gets or sets the binary data received on this websocket connection.
        /// </summary>
        public byte[] BinaryData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the received data is a message or a fragment of a message.
        /// </summary>
        public bool IsFragment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fragment received on this websocket connection is final within the message.
        /// </summary>
        public bool IsFinal { get; set; }
    }
}
