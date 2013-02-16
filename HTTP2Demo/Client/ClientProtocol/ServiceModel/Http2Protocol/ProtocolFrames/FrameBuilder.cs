//-----------------------------------------------------------------------
// <copyright file="FrameBuilder.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.Http2Protocol.ProtocolFrames
{
    using ClientProtocol.ServiceModel.Http2Protocol.ProtocolFrames;
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
        public DataFrame BuildDataFrame(Http2Stream stream, ProtocolData data, bool final)
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
        public DataFrame BuildDataFrame(int streamId, ProtocolData data)
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
        public DataFrame BuildDataFrame(Http2Stream stream, ProtocolData data)
        {
            return this.BuildDataFrame(stream, data, false);
        }

        /// <summary>
        /// Builds the RST Frame.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="reason">The reason for RST.</param>
        /// <returns>RST frame.</returns>
        public ControlFrame BuildRSTFrame(Http2Stream stream, StatusCode reason)
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
        public ControlFrame BuildSynStreamFrame(Http2Stream stream, ProtocolHeaders headers, bool final)
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
        public ControlFrame BuildSynStreamFrame(int streamId, ProtocolHeaders headers)
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
        public ControlFrame BuildSynReplyFrame(Http2Stream stream, ProtocolHeaders headers)
        {
            var frame = BuildControlFrame(FrameType.SynReply, stream, headers);
            frame.StreamId = stream.StreamId;
            frame.Length = sizeof(Int32) + sizeof(Int32); // sizeof(StreamId) + sizeof(numberOfEntries) 
            return frame;
        }

        /// <summary>
        /// Builds the SYN_REPLY frame.
        /// </summary>
        /// <param name="streamId">The stream id.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>SYN_REPLY frame.</returns>
        public ControlFrame BuildSynReplyFrame(int streamId, ProtocolHeaders headers)
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
        public ControlFrame BuildHeadersFrame(Http2Stream stream, ProtocolHeaders headers, bool final)
        {
            ControlFrame frame = BuildControlFrame(FrameType.Headers, stream, headers);
            frame.IsFinal = final;
            return frame;
        }

        /// <summary>
        /// Builds the GoAway frame.
        /// </summary>
        /// <param name="lastSeenGoodStreamId">The last seen good stream id.</param>
        /// <param name="reason">The reason of GoAway.</param>
        /// <returns>Builded GoAway Frame</returns>
        public GoAwayFrame BuildGoAwayFrame(int lastSeenGoodStreamId, StatusCode reason)
        {
            GoAwayFrame frame = new GoAwayFrame(lastSeenGoodStreamId, reason);
            frame.Flags = 0;
            frame.Length = 8;
            return frame;
        }

        /// <summary>
        /// Builds the Ping frame.
        /// </summary>
        /// <param name="lastSeenGoodStreamId">The stream id.</param>
        /// <returns>Builded Ping Frame</returns>
        public ControlFrame BuildPingFrame(int streamId)
        {
            ControlFrame frame = new ControlFrame { StreamId = streamId };
            frame.Flags = 0;
            frame.Length = 4;
            return frame;
        }
        
        /// <summary>
        /// Builds the WindowUpdate Frame.
        /// </summary>
        /// /// <param name="lastSeenGoodStreamId">The difference between current WindowSize and new WindowSize.</param>
        /// <param name="lastSeenGoodStreamId">The stream id.</param>
        /// <returns>Builded WindowUpdate Frame</returns>
        public WindowUpdateFrame BuildWindowUpdateFrame(int streamId, Int64 deltaSize)
        {
            WindowUpdateFrame frame = new WindowUpdateFrame(streamId, deltaSize);
            frame.Flags = 0;
            frame.Length = 8;
            return frame;
        }

        public WindowUpdateFrame BuildWindowUpdateFrame(Http2Stream stream, Int64 deltaSize)
        {
            WindowUpdateFrame frame = new WindowUpdateFrame(stream.StreamId, deltaSize);
            frame.Flags = 0;
            frame.Length = 8;
            return frame;
        }

        public ControlFrame BuildSettingsFrame(Http2Stream stream)
        {
            //TODO Add more logic at building settings frame
            var frame = BuildControlFrame(FrameType.Settings, stream, null);
            frame.SettingsHeaders = null;
            frame.Length = 4;
            frame.NumberOfEntries = 0;
            return frame;
        }

        /// <summary>
        /// Builds the control frame.
        /// </summary>
        /// <param name="type">The frame type.</param>
        /// <param name="stream">The Http2 stream object.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>Returns Control frame object.</returns>
        private static ControlFrame BuildControlFrame(FrameType type, Http2Stream stream, ProtocolHeaders headers)
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
        /// <param name="streamId">The Http2 stream id.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>Returns Control frame object.</returns>
        private static ControlFrame BuildControlFrame(FrameType type, int streamId, ProtocolHeaders headers)
        {
            ControlFrame frame = new ControlFrame(headers);
            frame.StreamId = streamId;
            frame.Type = type;
            frame.Priority = ControlPriority;

            return frame;
        }
    }
}
