//-----------------------------------------------------------------------
// <copyright file="FrameBuilder.cs" company="Microsoft Open Technologies, Inc.">
//
// ---------------------------------------
// HTTPbis
// Copyright Microsoft Open Technologies, Inc.
// ---------------------------------------
// Microsoft Reference Source License.
// 
// This license governs use of the accompanying software. 
// If you use the software, you accept this license. 
// If you do not accept the license, do not use the software.
// 
// 1. Definitions
// 
// The terms "reproduce," "reproduction," and "distribution" have the same meaning here 
// as under U.S. copyright law.
// "You" means the licensee of the software.
// "Your company" means the company you worked for when you downloaded the software.
// "Reference use" means use of the software within your company as a reference, in read // only form, 
// for the sole purposes of debugging your products, maintaining your products, 
// or enhancing the interoperability of your products with the software, 
// and specifically excludes the right to distribute the software outside of your company.
// "Licensed patents" means any Licensor patent claims which read directly on the software 
// as distributed by the Licensor under this license. 
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free copyright license to reproduce the software for reference use.
// (B) Patent Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free patent license under licensed patents for reference use. 
// 
// 3. Limitations
// (A) No Trademark License- This license does not grant you any rights 
// to use the Licensor’s name, logo, or trademarks.
// (B) If you begin patent litigation against the Licensor over patents that you think may apply 
// to the software (including a cross-claim or counterclaim in a lawsuit), your license 
// to the software ends automatically. 
// (C) The software is licensed "as-is." You bear the risk of using it. 
// The Licensor gives no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws 
// which this license cannot change. To the extent permitted under your local laws, 
// the Licensor excludes the implied warranties of merchantability, 
// fitness for a particular purpose and non-infringement. 
// 
// -----------------End of License---------
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
