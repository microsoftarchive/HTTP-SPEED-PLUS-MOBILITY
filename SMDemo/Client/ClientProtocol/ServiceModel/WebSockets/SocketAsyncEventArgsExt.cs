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
	using Net.Sockets;

	/// <summary>
	/// Extended socket args.
	/// </summary>
	public class SocketAsyncEventArgsExt: SocketAsyncEventArgs
	{
		private int? _bytesTransfered;

		/// <summary>
		/// Count of bytes transferred.
		/// </summary>
		public new int BytesTransferred
		{
			get { return _bytesTransfered.HasValue ? _bytesTransfered.Value : base.BytesTransferred; }
		}

		/// <summary>
		/// Dispatches the completed event.
		/// </summary>
		/// <param name="bytesReceived">The bytes received.</param>
		public void DispatchCompleted(int bytesReceived)
		{
			_bytesTransfered = bytesReceived;
			OnCompleted(this);
		}

		/// <summary>
		/// Dispatches the completed event.
		/// </summary>
		public void DispatchCompleted()
		{
			OnCompleted(this);
		}
	}
}
