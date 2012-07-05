// <copyright file="WebSocket.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.IO;
	using System.Text;
	using System.Collections.Generic;
    using System.Threading;
#if !SILVERLIGHT
#else
    using System.Windows.Browser;
#endif

    // API specification at http://www.w3.org/TR/websockets/    
#if !SILVERLIGHT
#else
    [ScriptableType]
#endif
    /// <summary>
    /// Implements the WebSocket protocol API.
    /// </summary>
    public class WebSocket : IDisposable
    {
        private WebSocketProtocol protocol;
        private string origin;
        private string protocolName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket"/> class.
        /// </summary>
        public WebSocket()
            : this(null)
        {
            // empty
        }


		public WebSocket(string url)
			: this(url, "http://tempuri", null)
		{
			// empty
		}

    	/// <summary>
    	/// Initializes a new instance of the <see cref="WebSocket"/> class using the specified remote url, origin url, protocol and websocket version.
    	/// </summary>
    	/// <param name="url">The remote url to connect to.</param>
    	/// <param name="origin">The origin url.</param>
    	/// <param name="protocol">The name of the protocol version to use.</param>       
    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "This is destined as an interface with a web browser's javascript engine that does not understand Uri.")]
		public WebSocket(string url, string origin, string protocol)
			: this(url, origin, protocol, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket"/> class using the specified remote url, origin url, protocol and websocket version.
        /// </summary>
        /// <param name="url">The remote url to connect to.</param>
        /// <param name="origin">The origin url.</param>
        /// <param name="protocol">The name of the protocol version to use.</param>        
        /// <param name="noDelay">Specifies whether the WebSocket uses Nagle algorithm.</param> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "This is destined as an interface with a web browser's javascript engine that does not understand Uri.")]
        public WebSocket(string url, string origin, string protocol, bool noDelay)
        {
            this.Url = url;
            this.origin = origin;
            this.protocolName = protocol;
            this.NoDelay = noDelay;
            this.DispatchSynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Occurs when the WebSocket is opened.
        /// </summary>
        public event EventHandler<EventArgs> OnOpen;

        /// <summary>
        /// Occurs when the WebSocket receives a message.
        /// </summary>
        public event EventHandler<WebSocketEventArgs> OnData;

        /// <summary>
        /// Occurs when the WebSocket is closed.
        /// </summary>
        public event EventHandler<WebSocketProtocolEventArgs> OnClose;

        /// <summary>
        /// Occurs when the Ping frame.
        /// </summary>
        public event EventHandler<EventArgs> OnPing;

        /// <summary>
        /// Gets or sets the URL of the remote service used by the WebSocket.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "This is destined as an interface with a web browser's javascript engine that does not understand Uri.")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Nagle algorithm is used
        /// </summary>
        public bool NoDelay { get; set; }

        /// <summary>
        /// Gets the current state of the WebSocket.
        /// </summary>
        public WebSocketState ReadyState { get; private set; }

        /// <summary>
        /// Gets the last <see cref="Exception"/> that occured on the WebSocket.
        /// </summary>
        public Exception LastError { get; private set; }

        /// <summary>
        /// Gets or sets the maximum size of the message that can be sent using the WebSocket.
        /// </summary>
        public int MaxInputBufferSize
        {
            get
            {
                if (this.protocol != null)
                {
                    return this.protocol.MaxInputBufferSize;
                }
                else
                {
                    throw new InvalidOperationException("The WebSocket must be opened first before changing the buffer quota.");
                }
            }

            set
            {
                if (this.protocol != null)
                {
                    this.protocol.MaxInputBufferSize = value;
                }
                else
                {
                    throw new InvalidOperationException("The WebSocket must be opened first before changing the buffer quota.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SynchronizationContext"/> associated with the WebSocket.
        /// </summary>
        public SynchronizationContext DispatchSynchronizationContext { get; set; }

        /// <summary>
        /// Establishes a websocket connection to the remote host.
        /// </summary>
		public void Open(Dictionary<string, string> customHeaders)
        {
            if (this.ReadyState == WebSocketState.Open)
            {
                throw new InvalidOperationException("The WebSocket is already opened.");
            }

            if (this.ReadyState == WebSocketState.Closed)
            {
                throw new InvalidOperationException("The WebSocket is closed and cannot be reused.");
            }

            if (string.IsNullOrEmpty(this.Url))
            {
                throw new InvalidOperationException("The Url parameter must be set before the WebSocket can be opened.");
            }

			this.protocol = new IETFHyBiWebSocketPotocol(this.Url, this.origin, this.protocolName, this.NoDelay);
            this.protocol.OnClose += this.OnProtocolClose;
            this.protocol.OnData += this.OnProtocolData;
            this.protocol.OnConnected += this.OnProtocolConnected;
            this.protocol.OnPing += this.OnProtocolPing;
			this.protocol.Start(customHeaders);
        }

        /// <summary>
        /// Sends the specified <see cref="String"/> to the remote service.
        /// </summary>
        /// <param name="data">The data to send as a string.</param>
        public void SendMessage(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.ReadyState == WebSocketState.Closed)
            {
                throw new InvalidOperationException("Cannot send data because the web socket has been closed.");
            }

            if (this.ReadyState == WebSocketState.Connecting)
            {
                throw new InvalidOperationException("Cannot send data because the web socket is not connected yet.");
            }

            this.protocol.SendMessage(data);
        }

        /// <summary>
        /// Sends the specified <see cref="String"/> to the remote service.
        /// </summary>
        /// <param name="data">The data to send as a string.</param>
        public void SendMessage(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.ReadyState == WebSocketState.Closed)
            {
                throw new InvalidOperationException("Cannot send data because the web socket has been closed.");
            }

            if (this.ReadyState == WebSocketState.Connecting)
            {
                throw new InvalidOperationException("Cannot send data because the web socket is not connected yet.");
            }

            this.protocol.SendMessage(data);
        }

        /// <summary>
        /// Sends the specified <see cref="String"/> to the remote service.
        /// </summary>
        /// <param name="final">Indicates whether the fragment is final.</param>
        /// <param name="data">The string to send.</param>
        public void SendFragment(bool final, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.ReadyState == WebSocketState.Closed)
            {
                throw new InvalidOperationException("Cannot send data because the web socket has been closed.");
            }

            if (this.ReadyState == WebSocketState.Connecting)
            {
                throw new InvalidOperationException("Cannot send data because the web socket is not connected yet.");
            }

            this.protocol.SendFragment(final, data);
        }

        /// <summary>
        /// Sends the specified <see cref="String"/> to the remote service.
        /// </summary>
        /// <param name="final">Indicates whether the fragment is final.</param>
        /// <param name="data">The binary data to send.</param>
        public void SendFragment(bool final, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.ReadyState == WebSocketState.Closed)
            {
                throw new InvalidOperationException("Cannot send data because the web socket has been closed.");
            }

            if (this.ReadyState == WebSocketState.Connecting)
            {
                throw new InvalidOperationException("Cannot send data because the web socket is not connected yet.");
            }

            this.protocol.SendFragment(final, data);
        }

        public void SendPing()
        {
            this.protocol.SendPing();
        }

        /// <summary>
        /// Initiates WebSockets close handshake .
        /// </summary>
        /// <param name="data">The extension data.</param>
        public void Close(byte[] data)
        {
            this.protocol.Close(data);
        }

        /// <summary>
        /// Closes the WebSockets session.
        /// </summary>
        public void CloseInternal()
        {
            if (this.ReadyState != WebSocketState.Closed && this.protocol != null)
            {
                this.protocol.Close(null);
            }
            else if (this.ReadyState == WebSocketState.Connecting)
            {
                this.ReadyState = WebSocketState.Closed;
            }
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        /// <param name="disposing">True is using the Dispose method, False if this object was garbage collected without disposing first.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.CloseInternal();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Do not propagate exception to the web browser.")]
        private void OnProtocolClose(object sender, WebSocketProtocolEventArgs args)
        {
            this.LastError = args.Exception;
            this.ReadyState = WebSocketState.Closed;
            if (this.OnClose != null)
            {
                this.DispatchEvent(delegate(object state)
                {
                    try
                    {
                        this.OnClose(this, args);
                    }
                    catch
                    {
                    }
                });
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Do not propagate exception to web browser.")]
        private void OnProtocolData(object sender, WebSocketProtocolEventArgs args)
        {
            if (this.OnData != null)
            {
                this.DispatchEvent(delegate(object state)
                {
                    try
                    {
                        this.OnData(
                            this,
                            new WebSocketEventArgs
                            {
                                BinaryData = args.BinaryData,
                                TextData = args.TextData,
                                IsFragment = args.IsFragment,
                                IsFinal = args.IsFinal
                            });
                    }
                    catch
                    {
                    }
                });
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Do not propagate exception to web browser.")]
        private void OnProtocolConnected(object sender, EventArgs args)
        {
            this.ReadyState = WebSocketState.Open;
            if (this.OnOpen != null)
            {
                this.DispatchEvent(delegate(object state)
                {
                    try
                    {
                        this.OnOpen(this, null);
                    }
                    catch
                    {
                    }
                });
            }
        }

        private void OnProtocolPing(object sender, EventArgs args)
        {
            if (this.OnPing != null)
            {
                this.DispatchEvent(delegate(object state)
                {
                    try
                    {
                        this.OnPing(this, null);
                    }
                    catch
                    {
                    }
                });
            }
        }

        private void DispatchEvent(SendOrPostCallback callback)
        {
            if (this.DispatchSynchronizationContext != null)
            {
                this.DispatchSynchronizationContext.Post(callback, this);
            }
            else
            {
                callback(this);
            }
        }
    }
}
