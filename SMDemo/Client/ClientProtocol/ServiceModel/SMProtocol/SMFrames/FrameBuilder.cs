//-----------------------------------------------------------------------
// <copyright file="FrameBuilder.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol.SMFrames
{
    /// <summary>
    /// Class that incapsulates frame building logic.
    /// </summary>
    internal class FrameBuilder
    {
        /// <summary>
        /// RSTPriority constant.
        /// </summary>
        private const byte RSTPriority = 0;

        /// <summary>
        /// SYNSTREAM constant.
        /// </summary>
        private const byte SYNStreamPriority = 1;

        /// <summary>
        /// Control priority constant.
        /// </summary>
        private const byte ControlPriority = 2;

        /// <summary>
        /// Builds the data frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <param name="final">if set to <c>true</c> than this frame is final for the stream.</param>
        /// <returns>returns DataFrame.</returns>
        public DataFrame BuildDataFrame(SMStream stream, SMData data, bool final)
        {
            DataFrame frame = BuildDataFrame(stream.StreamId, data);
            frame.IsFinal = final;
            return frame;
        }

        /// <summary>
        /// Builds the data frame.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="data">The data.</param>
        /// <returns>Returns DataFrame.</returns>
        public DataFrame BuildDataFrame(int streamId, SMData data)
        {
            DataFrame frame = new DataFrame { Data = data.Data, StreamId = streamId, Length = data.Data.Length };
            return frame;
        }

        /// <summary>
        /// Builds the data frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <returns>Returns DataFrame.</returns>
        public DataFrame BuildDataFrame(SMStream stream, SMData data)
        {
            return this.BuildDataFrame(stream, data, false);
        }

        /// <summary>
        /// Builds the RST Frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="reason">The reason for RST.</param>
        /// <returns>RST frame.</returns>
        public ControlFrame BuildRSTFrame(SMStream stream, StatusCode reason)
        {
            ControlFrame frame = new ControlFrame();
            frame.StreamId = stream.StreamId;
            frame.Type = FrameType.RTS;
            frame.StatusCode = reason;
            frame.Priority = RSTPriority;

            return frame;
        }

        /// <summary>
        /// Builds the RST Frame.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="reason">The reason for RST.</param>
        /// <returns>RST frame.</returns>
        public ControlFrame BuildRSTFrame(int streamId, StatusCode reason)
        {
            ControlFrame frame = new ControlFrame();
            frame.StreamId = streamId;
            frame.Type = FrameType.RTS;
            frame.StatusCode = reason;
            frame.Priority = RSTPriority;

            return frame;
        }

        /// <summary>
        /// Builds the SYN_STREAM frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="final">Indicates that stream is not going to send any more data.</param>
        /// <returns>SYN_STREAM frame.</returns>
        public ControlFrame BuildSynStreamFrame(SMStream stream, SMHeaders headers, bool final)
        {
            ControlFrame frame = BuildControlFrame(FrameType.SynStream, stream, headers);
            frame.Priority = SYNStreamPriority;
            frame.IsFinal = final;

            return frame;
        }

        /// <summary>
        /// Builds the SYN_STREAM frame.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>SYN_STREAM frame.</returns>
        public ControlFrame BuildSynStreamFrame(int streamId, SMHeaders headers)
        {
            ControlFrame frame = BuildControlFrame(FrameType.SynStream, streamId, headers);
            frame.Priority = SYNStreamPriority;

            return frame;
        }

        /// <summary>
        /// Builds the SYN_REPLY frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>SYN_REPLY frame.</returns>
        public ControlFrame BuildSynReplyFrame(SMStream stream, SMHeaders headers)
        {
            return BuildControlFrame(FrameType.SynReply, stream, headers);
        }

        /// <summary>
        /// Builds the SYN_REPLY frame.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>SYN_REPLY frame.</returns>
        public ControlFrame BuildSynReplyFrame(int streamId, SMHeaders headers)
        {
            return BuildControlFrame(FrameType.SynReply, streamId, headers);
        }

        /// <summary>
        /// Builds the headers frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="final">if set to <c>true</c> than this frame is final for the stream.</param>
        /// <returns>Headers frame.</returns>
        public ControlFrame BuildHeadersFrame(SMStream stream, SMHeaders headers, bool final)
        {
            ControlFrame frame = BuildControlFrame(FrameType.Headers, stream, headers);
            frame.IsFinal = final;
            return frame;
        }

        /// <summary>
        /// Builds the headers frame.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="final">if set to <c>true</c> than this frame is final for the stream.</param>
        /// <returns>Headers frame.</returns>
        public ControlFrame BuildHeadersFrame(int streamId, SMHeaders headers, bool final)
        {
            ControlFrame frame = BuildControlFrame(FrameType.Headers, streamId, headers);
            frame.IsFinal = final;
            return frame;
        }

        /// <summary>
        /// Builds the control frame.
        /// </summary>
        /// <param name="type">The frame type.</param>
        /// <param name="stream">The SM stream object.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>Returns Control frame object.</returns>
        private static ControlFrame BuildControlFrame(FrameType type, SMStream stream, SMHeaders headers)
        {
            ControlFrame frame = new ControlFrame(headers);
            frame.StreamId = stream.StreamId;
            frame.Type = type;
            frame.Priority = ControlPriority;

            return frame;
        }

        /// <summary>
        /// Builds the control frame.
        /// </summary>
        /// <param name="type">The frame type.</param>
        /// <param name="streamId">The SM stream id.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>Returns Control frame object.</returns>
        private static ControlFrame BuildControlFrame(FrameType type, int streamId, SMHeaders headers)
        {
            ControlFrame frame = new ControlFrame(headers);
            frame.StreamId = streamId;
            frame.Type = type;
            frame.Priority = ControlPriority;

            return frame;
        }
    }
}
