//-----------------------------------------------------------------------
// <copyright file="BinaryHelper.cs" company="Microsoft Open Technologies, Inc.">
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

using ClientProtocol.ServiceModel.Http2Protocol.MessageProcessing;

namespace ClientProtocol.ServiceModel.SMProtocol.MessageProcessing
{
	using System.ServiceModel.Http2Protocol;

	public class EncryptionProcessor: IMessageProcessor
	{
		/// <summary>
		/// Gets the type of the message process.
		/// </summary>
		/// <value>
		/// The type of the message process.
		/// </value>
		public MessageProcessType MessageProcessType
		{
			get { return MessageProcessType.Message; }
		}

		public ProcessType ProcessType
		{
			get { return ProcessType.Compression; }
		}

		/// <summary>
		/// Processes the specified input data.
		/// </summary>
		/// <param name="inputData">The input data.</param>
		/// <param name="type">The type.</param>
		/// <param name="optios">Options.</param>
		/// <param name="flags">Flags.</param>
		public void Process(ref byte[] inputData, DirectionProcessType type, ProtocolOptions optios, int flags)
		{
			inputData = type == DirectionProcessType.Inbound ? Encrypt(inputData) : Decrypt(inputData);
			
		}

		/// <summary>
		/// Encription frame.
		/// </summary>
		/// <param name="data">Byte array of frame data.</param>
		/// <returns>Encription byte array.</returns>
		private  byte[] Encrypt(byte[] data)
		{
			return data;
		}

		/// <summary>
		/// Decription frame.
		/// </summary>
		/// <param name="data">Byte array of frame data.</param>
		/// <returns>Decription byte array.</returns>
		private byte[] Decrypt(byte[] data)
		{
			return data;
		}
	}
}
