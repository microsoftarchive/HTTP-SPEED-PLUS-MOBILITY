//-----------------------------------------------------------------------
// <copyright file="FrameSerializer.cs" company="Microsoft Corp">
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
    using ClientProtocol.ServiceModel.Http2Protocol.ProtocolFrames;
    using ClientProtocol.ServiceModel.Http2Protocol.MessageProcessing;
    using System.Collections.Generic;
    using System.ServiceModel.Http2Protocol.ProtocolFrames;
    using System.Text;

    /// <summary>
    /// Class that provides frames serialization and deserialization logic.
    /// </summary>
    internal class FrameSerializer : IDisposable
    {
        /// <summary>
        /// Options.
        /// </summary>
        internal ProtocolOptions Option { set; get; }


        /// <summary>
        /// Unused byte constant.
        /// </summary>
        private const byte Unused = 0;

        /// <summary>
        /// Processors list.
        /// </summary>
        private List<IMessageProcessor> _processors = new List<IMessageProcessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameSerializer"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public FrameSerializer(ProtocolOptions options)
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
        /// Deserializes the Http2 Close Extension Data.
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

            var headersArray = new byte[0];
            switch (frame.Type)
            {
                case FrameType.SynStream:
                    byteList.Add(Convert.ToByte(frame.Flags | (frame.IsFinal ? 0x01 : 0x00)));

                    headersArray = SerializeControlFrameHeaders(frame.Headers);

                    if (headersArray.Length > 0)
                    {
                        ProcessorRun(ref headersArray, DirectionProcessType.Outbound, frame.Flags);
                    }

                    byteList.AddRange(BinaryHelper.Int32ToBytes(headersArray.Length + 10, 3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));

                    byteList.AddRange(BinaryHelper.Int32ToBytes(0));

                    byteList.Add(Convert.ToByte(frame.Priority >> 5));

                    byteList.Add(Unused);

                    break;
                case FrameType.RTS:
                    byteList.Add(0);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(headersArray.Length + 8, 3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));
                    byteList.AddRange(BinaryHelper.Int32ToBytes((int)frame.StatusCode));
                    break;
                case FrameType.SynReply:
                    byteList.Add(frame.Flags);
                    headersArray = SerializeControlFrameHeaders(frame.Headers);

                    if (headersArray.Length > 0)
                    {
                        ProcessorRun(ref headersArray, DirectionProcessType.Outbound, frame.Flags);
                    }

                    byteList.AddRange(BinaryHelper.Int32ToBytes(headersArray.Length + 4, 3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));
                    break;
                case FrameType.GoAway:
                    byteList.Add(frame.Flags);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.Length,3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));
                    byteList.AddRange(BinaryHelper.Int32ToBytes((int)frame.StatusCode));
                    break;
                case FrameType.Ping:
                    byteList.Add(frame.Flags);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.Length,3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));
                    break;
                case FrameType.WindowUpdate:
                    byteList.Add(frame.Flags);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.Length,3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));
                    byteList.AddRange(BinaryHelper.Int64ToBytes(((WindowUpdateFrame)frame).DeltaWindowSize));
                    break;
                case FrameType.Settings:
                    byteList.Add(frame.Flags);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.Length,3));
                    byteList.AddRange(BinaryHelper.Int32ToBytes(frame.NumberOfEntries));
                    break;
            }


            byteList.AddRange(headersArray);
            return byteList.ToArray();
        }

        private byte[] SerializeControlFrameHeaders(ProtocolHeaders frameHeaders)
        {
            var headers = new List<byte>(256);
            headers.AddRange(BinaryHelper.Int32ToBytes(frameHeaders.Count));
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
            data[4] = Convert.ToByte(frame.Flags | (frame.IsFinal ? 0x01 : 0x00));
            BinaryHelper.Int32ToBytes(frame.Length, new ArraySegment<byte>(data, 5, 3));
            Buffer.BlockCopy(frame.Data, 0, data, 8, frame.Length);

            return data;
        }

        private FrameType GetFrameType(byte[] data)
        {
            return (FrameType)BinaryHelper.Int16FromBytes(data[2], data[3]);
        }

        /// <summary>
        /// Deserializes the data into control frame.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized frame.</returns>
        private BaseFrame DeserializeControlFrame(byte[] data)
        {
            FrameType type = GetFrameType(data);
            ControlFrame frame = new ControlFrame();

            switch (type)
            {
                case FrameType.RTS:
                    frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
                    frame.StatusCode = (StatusCode)BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 12, 4));
                    break;
                case FrameType.Headers:
                case FrameType.SynReply:
                    ParseControlFrameHeader(ref frame, data);

                    frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));

                    ParseControlFrameHeaders(ref frame, data, 12);
                    break;
                case FrameType.SynStream:
                    ParseControlFrameHeader(ref frame, data);

                    frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
                    frame.AssociatedToStreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 12, 4));
                    frame.Priority = (byte)(data[16] >> 5);
                    frame.Slot = data[17];

                    ParseControlFrameHeaders(ref frame, data, 18);
                    break;
                case FrameType.Settings:
                    int numberOfEntries = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
                    frame = new SettingsFrame(numberOfEntries);
                    int headersOffset = 12;

                    for (int i = 0; i < numberOfEntries; i++)
                    {
                        int key = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, headersOffset, 4));
                        headersOffset += 4;
                        int value = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, headersOffset, 4));
                        headersOffset += 4;

                        frame.SettingsHeaders.Add(key, value);
                    }

                    ParseControlFrameHeader(ref frame, data);
                    break;
                case FrameType.GoAway:
                    int lastSeenGoodStreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
                    StatusCode status = StatusCode.Success;

                    if (data.Length > 12)
                        status = (StatusCode)BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 12, 4));

                    frame = new GoAwayFrame(lastSeenGoodStreamId, status);
                    ParseControlFrameHeader(ref frame, data);
                    break;
                case FrameType.Ping:
                    int streamID = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 4, 4));
                    frame = new ControlFrame { StreamId = streamID };

                    ParseControlFrameHeader(ref frame, data);
                    break;
                case FrameType.WindowUpdate:
                    int streamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 4, 4));
                    int deltaWindowSize = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4));
                    frame = new WindowUpdateFrame(streamId, deltaWindowSize);

                    ParseControlFrameHeader(ref frame, data);
                    break;
            }
            frame.Type = type;

            return frame;
        }

        /// <summary>
        /// Parses HTTP headers of Http2 control frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="data">The data.</param>
        /// <param name="offset">Offset of HTTP headers in the data.</param>
        private void ParseControlFrameHeaders(ref ControlFrame frame, byte[] data, int offset)
        {
            var headers = new byte[data.Length - offset];
            Array.Copy(data, offset, headers, 0, headers.Length);
            if (headers.Length > 0)
                ProcessorRun(ref headers, DirectionProcessType.Inbound, frame.Flags);

            int headersOffset = 0;
            int numberOfKeyValue = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(headers, headersOffset, 4));
            headersOffset += 4;

            for (int i = 0; i < numberOfKeyValue; ++i)
            {
                int nameLength = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(headers, headersOffset, 4));
                headersOffset += 4;
                string name = Encoding.UTF8.GetString(headers, headersOffset, nameLength);
                headersOffset += nameLength;

                int valLength = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(headers, headersOffset, 4));
                headersOffset += 4;
                string val = Encoding.UTF8.GetString(headers, headersOffset, valLength);
                headersOffset += valLength;
                // Ensure no duplicates.
                if (frame.Headers.ContainsKey(name))
                {
                    throw new ProtocolExeption(StatusCode.InternalError);
                }

                frame.Headers.Add(name, val);
            }
        }

        /// <summary>
        /// Parses HTTP header of Http2 control frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="data">The data.</param>
        private static void ParseControlFrameHeader(ref ControlFrame frame, byte[] data)
        {
            frame.Version = BinaryHelper.Int16FromBytes(data[0], data[1], 1);
            frame.Type = (FrameType)BinaryHelper.Int16FromBytes(data[2], data[3]);

            frame.Flags = data[4]; //  it would be always 4 th byte for flags in spec.

            frame.Length = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 5, 3));
            frame.IsFinal = (frame.Flags & 0x01) != 0;
        }

        /// <summary>
        /// Deserialize Http2 data frame
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
        /// Parses HTTP header of Http2 data frame.
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
