//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Open Technologies, Inc.">
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
	using Net;
	using Net.Security;
	using Net.Sockets;
	using Security.Authentication;
	using Security.Cryptography.X509Certificates;

	/// <summary>
	/// Http2 proxy class.
	/// </summary>
	public class SecureSocketProxy: IDisposable
	{
        private Socket _socket;
		private bool _isSecure;
		public SslStream _sslStream;
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
			return _socket.ConnectAsync(args);
		}

        /// <summary>
        /// Async connection to the server.
        /// </summary>
        /// <param name="args">Object type of SocketAsyncEventArgs that determines args for 
        /// connect. </param>
        /// <returns>If connect's state is successfully than true else false.</returns>
        public void Connect(string address, int port)
        {
            DnsEndPoint remoteEndpoint = new DnsEndPoint(address, port);
            if (_isSecure)
            {
                ConnectSocket(remoteEndpoint, TimeSpan.FromSeconds(5));
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
            }
            else
            {
                ConnectSocket(remoteEndpoint, TimeSpan.FromSeconds(5));
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
            if (_sslStream != null)
            {
                _sslStream.Dispose();
                _sslStream = null;
            }
            
			_socket.Dispose();

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
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
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Receive(byte[] buffer)
        {
            if (_isSecure)
            {
                return _sslStream.Read(buffer, 0, buffer.Length);
            }

            return _socket.Receive(buffer);
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
        /// Sync send of data.
        /// </summary>
        /// <param name="buffer">Data for sending.</param>
        public void Send(byte[] buffer)
        {
            if (_isSecure)
            {
                _sslStream.Write(buffer);
                _sslStream.Flush();
            }
            else
            {
                int offset = 0;
                while (offset < buffer.Length)
                    offset += _socket.Send(buffer, offset, buffer.Length - offset, SocketFlags.None);
            }
        }

		/// <summary>
		/// End of the async sending (callback).
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

        private void ConnectSocket(EndPoint endpoint, TimeSpan timeout)
        {
            var result = _socket.BeginConnect(endpoint, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (success)
            {
                _socket.EndConnect(result);
            }
            else
            {
                _socket.Close();
                throw new SocketException(10060); // Connection timed out.
            }
        }
	}
}
