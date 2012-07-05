//-----------------------------------------------------------------------
// <copyright file="FrameSerializer.cs" company="Microsoft Corp">
//
// Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0.  
//                                    
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING 
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, 
// FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
//
// See the Apache License, Version 2.0 for specific language governing 
// permissions and limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
namespace System.ServiceModel.SMProtocol
{
	using ClientProtocol.ServiceModel.SMProtocol.SMFrames;
	using ClientProtocol.ServiceModel.SMProtocol.MessageProcessing;
	using System.Collections.Generic;
	using System.ServiceModel.SMProtocol.SMFrames;
	using System.Text;

	/// <summary>
	/// Class that provides frames serialization and deserialization logic.
	/// </summary>
	internal class FrameSerializer : IDisposable
	{
		/// <summary>
		/// Options.
		/// </summary>
		internal SMProtocolOptions Option { set; get; }


		/// <summary>
		/// Unused byte constant.
		/// </summary>
		private const byte Unused = 0;

		/// <summary>
		/// Processors list.
		/// </summary>
		private static List<IMessageProcessor> _processors = new List<IMessageProcessor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="FrameSerializer"/> class.
		/// </summary>
		/// <param name="options">The options.</param>
		public FrameSerializer(SMProtocolOptions options)
		{
			Option = options;
		}

		/// <summary>
		/// Start process.
		/// </summary>
		/// <param name="headersArray">Header's byte array.</param>
		/// <param name="type">Type process (inbound|outbound).</param>
		/// <param name="flags">Frame flags.</param>
		private void ProcessorRun(ref byte[] headersArray, DirectionProcessType type, int flags)
		{
			foreach (IMessageProcessor processor in _processors)
			{
				if (processor.MessageProcessType == MessageProcessType.Headers)
				{
					processor.Process(ref headersArray, type, Option, flags);
				}
			}
		}

		/// <summary>
		/// Sets the message processors.
		/// </summary>
		/// <param name="processors">The processors.</param>
		internal void SetProcessors(List<IMessageProcessor> processors)
		{
			_processors = processors;
		}

		/// <summary>
		/// Serializes the specified frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		/// <returns>Binary representation of the frame.</returns>
		public byte[] Serialize(BaseFrame frame)
		{
		    if (frame is ControlFrame)
			{
				if (Option.UseCompression)
				{
					frame.Flags &= (byte)FrameFlags.FlagNormal;
				}
				else
				{
                    frame.Flags &= (byte)FrameFlags.FlagNoHeaderCompression1;
				}
				return SerializeControlFrame(frame as ControlFrame);
			}
		    return SerializeDataFrame(frame as DataFrame);
		}

	    /// <summary>
		/// Deserializes the specified data into frame.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>Deserialized frame.</returns>
		public BaseFrame Deserialize(byte[] data)
		{
			// check whenever this is control or data frame
			bool isControlFrame = (data[0] >> 7) != 0;
		    if (isControlFrame)
		        return DeserializeControlFrame(data);
		    return DeserializeDataFrame(data);
		}

		/// <summary>
		/// Deserializes the WebSocket Close Extension Data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>Deserialized Extension Data.</returns>
		public CloseFrameExt DeserializeCloseFrameExt(byte[] data)
		{
			var extData = new CloseFrameExt();

			extData.StatusCode = BinaryHelper.Int16FromBytes(data[0], data[1], 0);
			extData.LastGoodSessionId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 2, 4));

			return extData;
		}

		/// <summary>
		/// Serializes the control frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		/// <returns>Serialized control frame.</returns>
		private byte[] SerializeControlFrame(ControlFrame frame)
		{
			var byteList = new List<byte>();

			byteList.Add((byte)(((frame.Version & 0xFF00) >> 8) | 0x80));
			byteList.Add((byte)(frame.Version & 0x00FF));

			byteList.Add((byte)(((Int16)frame.Type & 0xFF00) >> 8));
			byteList.Add((byte)((Int16)frame.Type & 0x00FF));

            byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));

            var headersArray = new byte[0];
		    switch (frame.Type)
		    {
		            case FrameType.SynStream:
                        byteList.Add(Convert.ToByte(frame.Flags | (frame.IsFinal ? 0x01 : 0x00)));
                        byteList.Add(Convert.ToByte(frame.Priority >> 5));
                        byteList.Add(Unused);
                        byteList.Add(Unused);
		                headersArray = SerializeControlFrameHeaders(frame.Headers);
		                break;
                    case FrameType.RTS:
                        byteList.AddRange(BinaryHelper.Int32ToBytes((int)frame.StatusCode));
                        break;
                    case FrameType.SynReply:
                        byteList.Add(frame.Flags);
                        byteList.Add(Unused);
                        byteList.Add(Unused);
                        byteList.Add(Unused);
                        headersArray = SerializeControlFrameHeaders(frame.Headers);
                        break;
                    case FrameType.CreditUpdate:
		                byteList.AddRange(BinaryHelper.Int64ToBytes(((CreditUpdateFrame)frame).CreditAddition));
                        break;
		    }

            if (headersArray.Length > 0)
			{
				ProcessorRun(ref headersArray, DirectionProcessType.Outbound, frame.Flags);
			}

			byteList.AddRange(headersArray);
			return byteList.ToArray();
		}

        private byte[] SerializeControlFrameHeaders(SMHeaders frameHeaders)
        {
            var headers = new List<byte>();
            foreach (KeyValuePair<string, string> pair in frameHeaders)
            {
                byte[] nameBin = Encoding.UTF8.GetBytes(pair.Key);

                headers.AddRange(BinaryHelper.Int32ToBytes(nameBin.Length));
                headers.AddRange(nameBin);

                byte[] valBin = Encoding.UTF8.GetBytes(pair.Value);
                headers.AddRange(BinaryHelper.Int32ToBytes(valBin.Length));

                headers.AddRange(valBin);
            }

            return headers.ToArray();
        }
		/// <summary>
		/// Serializes the data frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		/// <returns>Binary representation of the frame.</returns>
		private static byte[] SerializeDataFrame(DataFrame frame)
		{
			var data = new byte[8 + frame.Length];
			BinaryHelper.Int32ToBytes(frame.StreamId, new ArraySegment<byte>(data, 0, 4));
			data[4] = frame.Flags;
			BinaryHelper.Int32ToBytes(frame.Length, new ArraySegment<byte>(data, 5, 3));
			Buffer.BlockCopy(frame.Data, 0, data, 8, frame.Length);

			return data;
		}

		/// <summary>
		/// Deserializes the data into control frame.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>Deserialized frame.</returns>
		private BaseFrame DeserializeControlFrame(byte[] data)
		{
			var frame = new ControlFrame();
			ParseControlFrameHeader(ref frame, data);

			switch (frame.Type)
			{
				case FrameType.RTS:
					frame.StatusCode = (StatusCode)BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
					break;
				case FrameType.Headers:
				case FrameType.SynReply:
					ParseControlFrameHeaders(ref frame, data, 12);
					break;
				case FrameType.SynStream:
					frame.Priority = (byte)(data[9] >> 5);
					ParseControlFrameHeaders(ref frame, data, 12);
					break;
                case FrameType.CreditUpdate:
			        ((CreditUpdateFrame)frame).CreditAddition = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
                    break;

			}

			return frame;
		}

		/// <summary>
		/// Parses HTTP headers of SM control frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		/// <param name="data">The data.</param>
		/// <param name="offset">Offset of HTTP headers in the data.</param>
		private void ParseControlFrameHeaders(ref ControlFrame frame, byte[] data, int offset)
		{
			var headers = new byte[data.Length - offset];
			Array.Copy(data, offset, headers, 0, headers.Length);
			if (headers.Length > 0)
				ProcessorRun(ref data, DirectionProcessType.Outbound, frame.Flags);

			for (int i = 0; /*i < headersCount*/ offset < data.Length; ++i)
			{
				int nameLength = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, offset, 4));
				offset += 4;
				string name = Encoding.UTF8.GetString(data, offset, nameLength);
				offset += nameLength;

				int valLength = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, offset, 4));
				offset += 4;
				string val = Encoding.UTF8.GetString(data, offset, valLength);
				offset += valLength;
                // Ensure no duplicates.
                if (frame.Headers.ContainsKey(name))
                {
                    throw new SMProtocolExeption(StatusCode.InternalError);
                }

				frame.Headers.Add(name, val);
			}
		}

		/// <summary>
		/// Parses HTTP header of SM control frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		/// <param name="data">The data.</param>
		private static void ParseControlFrameHeader(ref ControlFrame frame, byte[] data)
		{
			frame.Version = BinaryHelper.Int16FromBytes(data[0], data[1], 1);
			frame.Type = (FrameType)BinaryHelper.Int16FromBytes(data[2], data[3]);

            frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 4, 4), 0);

			frame.Flags = data[8];

		//	frame.Length = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 5, 3));
			frame.IsFinal = (frame.Flags & 0x01) != 0;
		}

		/// <summary>
		/// Deserialize SM data frame
		/// </summary>
		/// <param name="frameData">The data.</param>
		/// <returns>Deserialized frame.</returns>
		private static BaseFrame DeserializeDataFrame(byte[] frameData)
		{
			var data = frameData;

			var frame = new DataFrame();
			ParseDataFrameHeader(ref frame, data);
		    frame.Length = data.Length - 8;
            frame.Data = new byte[frame.Length];
            Buffer.BlockCopy(data, 8, frame.Data, 0, frame.Length);

			return frame;
		}

		/// <summary>
		/// Parses HTTP header of SM data frame.
		/// </summary>
		/// <param name="frame">The frame.</param>
		/// <param name="data">The data.</param>
		private static void ParseDataFrameHeader(ref DataFrame frame, byte[] data)
		{
			frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 0, 4), 0);
			frame.Flags = data[4];
			frame.IsFinal = (frame.Flags & 0x01) != 0;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_processors.Clear();
		}
	}
}
