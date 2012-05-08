//-----------------------------------------------------------------------
// <copyright file="FrameSerializer.cs" company="Microsoft Open Technologies, Inc.">
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
    using System.Collections.Generic;
    using System.ServiceModel.SMProtocol.SMFrames;
    using System.Text;

    /// <summary>
    /// Class that provides frames serialization and deserialization logic.
    /// </summary>
    internal class FrameSerializer
    {
        /// <summary>
        /// Unused byte constant.
        /// </summary>
        private const byte Unused = 0;

        /// <summary>
        /// Serializes the specified frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>Binary representation of the frame.</returns>
        public byte[] Serialize(BaseFrame frame)
        {
            if (frame is ControlFrame)
            {
                return SerializeControlFrame(frame as ControlFrame);
            }
            else
            {
                return SerializeDataFrame(frame as DataFrame);
            }
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
            {
                return DeserializeControlFrame(data);
            }
            else
            {
                return DeserializeDataFrame(data);
            }
        }

        /// <summary>
        /// Deserializes the WebSocket Close Extension Data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized Extension Data.</returns>
        public CloseFrameExt DeserializeCloseFrameExt(byte[] data)
        {
            CloseFrameExt extData = new CloseFrameExt();

            extData.StatusCode = BinaryHelper.Int16FromBytes(data[0], data[1], 0);
            extData.LastGoodSessionId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 2, 4));

            return extData;
        }

        /// <summary>
        /// Serializes the control frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>Serialized control frame.</returns>
        private static byte[] SerializeControlFrame(ControlFrame frame)
        {
            frame.Length = 12;
            if (frame.Type == FrameType.RTS)
            {
                frame.Length += 4;
            }
            else
            {
                if (frame.Type == FrameType.SynStream)
                {
                    frame.Length += 8;
                }

                frame.Length += 4;
                foreach (var header in frame.Headers)
                {
                    frame.Length += 4 + header.Key.Length;
                    frame.Length += 4 + frame.Headers[header.Key].Length;
                }
            }

            List<byte> byteList = new List<byte>();

            byteList.Add((byte)(((frame.Version & 0xFF00) >> 8) | 0x80));
            byteList.Add((byte)(frame.Version & 0x00FF));

            byteList.Add((byte)(((Int16)frame.Type & 0xFF00) >> 8));
            byteList.Add((byte)((Int16)frame.Type & 0x00FF));
            
            byteList.Add(frame.Flags);

            byteList.AddRange(BinaryHelper.Int32ToBytes(frame.Length, 3));
            byteList.AddRange(BinaryHelper.Int32ToBytes(frame.StreamId));

            if (frame.Type == FrameType.SynStream)
            {
                byteList.AddRange(BinaryHelper.Int32ToBytes(frame.AssociatedToStreamId));
                byteList.Add(frame.Priority);
                byteList.Add(frame.Slot);
                byteList.Add(Unused);
                byteList.Add(Unused);
            }

            if (frame.Type == FrameType.RTS)
            {
                byteList.AddRange(BinaryHelper.Int32ToBytes((int)frame.StatusCode));
            }
            else
            {
                byteList.AddRange(BinaryHelper.Int32ToBytes(frame.Headers.Count));

                foreach (KeyValuePair<string, string> pair in frame.Headers)
                {
                    byte[] nameBin = Encoding.UTF8.GetBytes(pair.Key);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(nameBin.Length));

                    byteList.AddRange(nameBin);

                    byte[] valBin = Encoding.UTF8.GetBytes(pair.Value);
                    byteList.AddRange(BinaryHelper.Int32ToBytes(valBin.Length));

                    byteList.AddRange(valBin);
                }
            }

            return byteList.ToArray();
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
        private static BaseFrame DeserializeControlFrame(byte[] data)
        {
            ControlFrame frame = new ControlFrame();
            ParseControlFrameHeader(ref frame, data);

            switch (frame.Type)
            {
                case FrameType.RTS:
                    frame.StatusCode = (StatusCode)BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 12, 4));
                    break;
                case FrameType.Headers:
                case FrameType.SynReply:
                    ParseControlFrameHeaders(ref frame, data, 12);
                    break;
                case FrameType.SynStream:
                    frame.AssociatedToStreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 12, 4), 1);
                    frame.Priority = (byte)(data[16] & 0x7);
                    frame.Slot = data[17];
                    ParseControlFrameHeaders(ref frame, data, 20);
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
        private static void ParseControlFrameHeaders(ref ControlFrame frame, byte[] data, int offset)
        {
            int headersCount = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, offset, 4));
            offset += 4;

            for (int i = 0; i < headersCount; ++i)
            {
                int nameLength = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, offset, 4));
                offset += 4;
                string name = Encoding.UTF8.GetString(data, offset, nameLength);
                offset += nameLength;

                int valLength = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, offset, 4));
                offset += 4;
                string val = Encoding.UTF8.GetString(data, offset, valLength);
                offset += valLength;

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
            frame.Flags = data[4];
            frame.Length = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 5, 3));
            frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 8, 4), 1);
            frame.IsFinal = (frame.Flags & 0x01) != 0;
        }

        /// <summary>
        /// Deserialize SM data frame
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Deserialized frame.</returns>
        private static BaseFrame DeserializeDataFrame(byte[] data)
        {
            DataFrame frame = new DataFrame();
            ParseDataFrameHeader(ref frame, data);

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
            frame.StreamId = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 0, 4), 1);
            frame.Flags = data[4];
            frame.Length = BinaryHelper.Int32FromBytes(new ArraySegment<byte>(data, 5, 3));
            frame.IsFinal = (frame.Flags & 0x01) != 0;
        }
    }
}
