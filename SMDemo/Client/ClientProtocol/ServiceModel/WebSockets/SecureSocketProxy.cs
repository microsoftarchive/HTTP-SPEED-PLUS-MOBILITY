//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Open Technologies, Inc.">
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

namespace System.ServiceModel.WebSockets
{
	using Net;
	using Net.Security;
	using Net.Sockets;
	using Security.Authentication;
	using Security.Cryptography.X509Certificates;

	/// <summary>
	/// Web socket proxy class.
	/// </summary>
	public class SecureSocketProxy: IDisposable
	{
		private readonly Socket _socket;
		private bool _isSecure;
		protected SslStream _sslStream;
		private NetworkStream _stream;

		/// <summary>
		/// No delay.
		/// </summary>
		public bool NoDelay
		{
			get { return _socket.NoDelay; }
			set { _socket.NoDelay = value; }
		}

		/// <summary>
		/// Is secure session.
		/// </summary>
		public bool IsSecure
		{
			get { return _isSecure; }
			protected set { _isSecure = value; }
		}

		/// <summary>
		/// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
		/// </summary>
		/// <param name="sender">An object that contains state information for this validation.</param>
		/// <param name="certificate">The certificate used to authenticate the remote party.</param>
		/// <param name="chain">The chain of certificate authorities associated with the 
		/// remote certificate.</param>
		/// <param name="sslPolicyErrors">One or more errors associated with the remote 
		/// certificate.</param>
		/// <returns>A Boolean value that determines whether the specified certificate is 
		/// accepted for authentication.</returns>
		public static bool ValidateServerCertificate(
			  object sender,
			  X509Certificate certificate,
			  X509Chain chain,
			  SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		/// <summary>
		/// Async connection to the server.
		/// </summary>
		/// <param name="args">Object type of SocketAsyncEventArgs that determines args for 
		/// connect. </param>
		/// <returns>If connect's state is successfully than true else false.</returns>
		public bool ConnectAsync(SocketAsyncEventArgs args)
		{
			if (_isSecure)
			{
				DnsEndPoint remoteEndpoint = (DnsEndPoint) args.RemoteEndPoint;
				_socket.Connect(remoteEndpoint);
				_stream = new NetworkStream(_socket);
				_sslStream = new SslStream(_stream, true, ValidateServerCertificate, null);
				X509Certificate certificate = new X509Certificate("certificate.pfx");
				try
				{
					_sslStream.AuthenticateAsClient(remoteEndpoint.Host, new X509CertificateCollection(new[] { certificate }), SslProtocols.Tls, false);

				}
				catch (Exception)
				{
					// socket was closed forcibly, protocol will handle this
				}
				return false;
			}
			else
			{
				return _socket.ConnectAsync(args);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SecureSocketProxy"/> class.
		/// </summary>
		/// <param name="isSecure"></param>
		public SecureSocketProxy(bool isSecure)
		{
			this._isSecure = isSecure;
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Disables sends and receives on a Socket.
		/// </summary>
		/// <param name="how">One of the SocketShutdown values that specifies the operation 
		/// that will no longer be allowed.</param>
		public void Shutdown(SocketShutdown how)
		{
			_socket.Shutdown(how);
		}

		/// <summary>
		/// Close socket.
		/// </summary>
		public void Close()
		{
			_socket.Close();
		}

		/// <summary>
		/// Disposes the instance.
		/// </summary>
		public void Dispose()
		{
			_socket.Dispose();
		}

		/// <summary>
		/// Begins an asynchronous read operation that reads data from the stream and stores 
		/// it in the specified array.
		/// </summary>
		/// <param name="args">A user-defined object that contains information about the read 
		/// operation. This object is passed to the asyncCallback delegate when the operation 
		/// completes.</param>
		/// <returns>Return true if state of receive is success.</returns>
		public bool ReceiveAsync(SocketAsyncEventArgsExt args)
		{
			if (_isSecure)
			{
				_sslStream.BeginRead(args.Buffer, args.Offset, args.Count, ReceiveComplete, args);
				return true;
			}

			return _socket.ReceiveAsync(args);
		}

		/// <summary>
		/// End of the receive.
		/// </summary>
		/// <param name="result">Result sslStream.BeginRead.</param>
		private void ReceiveComplete(IAsyncResult result)
		{
			try
			{
				int bytesCount = _sslStream.EndRead(result);
				var args = (SocketAsyncEventArgsExt)result.AsyncState;
				args.SocketError = SocketError.Success;
				args.DispatchCompleted(bytesCount);
			}
			catch (Exception)
			{
				// socket was closed forcibly, protocol will handle this
			}
		}

		/// <summary>
		/// Async send of data.
		/// </summary>
		/// <param name="args">A user-defined object that contains information about the 
		/// write operation. This object is passed to the asyncCallback delegate when the 
		/// operation completes.</param>
		/// <returns>Return true if state of send is success.</returns>
		public bool SendAsync(SocketAsyncEventArgs args)
		{
			if (_isSecure)
			{
				_sslStream.BeginWrite(args.Buffer, args.Offset, args.Count, SendComplete, args);
				return true;
			}

			return _socket.SendAsync(args);
		}

		/// <summary>
		/// End of the sending.
		/// </summary>
		/// <param name="result">Result sslStream.BeginWrite.</param>
		private void SendComplete(IAsyncResult result)
		{
			try
			{
				_sslStream.EndWrite(result);
				var args = (SocketAsyncEventArgsExt)result.AsyncState;
				args.SocketError = SocketError.Success;
				args.DispatchCompleted();
			}
			catch (Exception)
			{
				// socket was closed forcibly, protocol will handle this
			}
		}
	}
}
