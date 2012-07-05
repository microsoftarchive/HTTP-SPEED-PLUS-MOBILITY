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

namespace ClientProtocol.ServiceModel.SMProtocol.MessageProcessing
{
	using System.ServiceModel.SMProtocol;
	using System;
	using ClientProtocol.ServiceModel.SMProtocol.SMFrames;
	using ComponentAce.Compression.Libs.zlib;
	using System.IO;

	public class CompressionProcessor: IMessageProcessor, IDisposable
	{
		private readonly MemoryStream _memStream;
		private readonly ZOutputStreamExt _compressOutZStream;
		private readonly ZOutputStreamExt _decompressOutZStream;

		/// <summary>
		/// Compression data.
		/// </summary>
		public CompressionProcessor()
		{
			_memStream = new MemoryStream();
			_compressOutZStream = new ZOutputStreamExt(_memStream, zlibConst.Z_DEFAULT_COMPRESSION, CompressionDictionary.Dictionary);
			_decompressOutZStream = new ZOutputStreamExt(_memStream, CompressionDictionary.Dictionary);
		}

		/// <summary>
		/// Gets the type of the message process.
		/// </summary>
		/// <value>
		/// The type of the message process.
		/// </value>
		public MessageProcessType MessageProcessType
		{
			get { return MessageProcessType.Headers; }
		}

		/// <summary>
		/// Gets the type of the process.
		/// </summary>
		/// <value>
		/// The type of the process.
		/// </value>
		public ProcessType ProcessType
		{
			get { return ProcessType.Compression; }
		}

		/// <summary>
		/// Processes the specified headers.
		/// </summary>
		/// <param name="headers">The headers.</param>
		/// <param name="type">The type.</param>
		/// <param name="options">Options.</param>
		/// <param name="flags">Flags.</param>
		public void Process(ref byte[] headers, DirectionProcessType type, SMProtocolOptions options, int flags)
		{
			if (!options.UseCompression)
				return;

			if (!options.CompressionIsStateful)
			{
				_compressOutZStream.SetDictionary(CompressionDictionary.Dictionary);
				_decompressOutZStream.SetDictionary(CompressionDictionary.Dictionary);
			}

			if ((flags & (byte)FrameFlags.FlagNoHeaderCompression1) == 0 && (flags & (byte)FrameFlags.FlagNoHeaderCompression2) == 0)
				headers = type == DirectionProcessType.Inbound ? Decompress(headers) : Compress(headers);
		}

		/// <summary>
		/// Copy stream buffer for input stream to output stream.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="output">Output stream.</param>
		private static void CopyStream(Stream input, Stream output)
		{
			byte[] buffer = new byte[input.Length];
			int len;
			while ((len = input.Read(buffer, 0, (int)input.Length)) > 0)
			{
				output.Write(buffer, 0, len);
			}
			output.Flush();
		}

		/// <summary>
		/// Clear stream buffer.
		/// </summary>
		/// <param name="input">Stream.</param>
		/// <param name="len">Length stream buffer.</param>
		private static void ClearStream(Stream input, int len)
		{
			byte[] buffer = new byte[len];
			input.Position = 0;
			input.Write(buffer, 0, len);
			input.SetLength(0);
		}
		/// <summary>
		/// Compress frame.
		/// </summary>
		/// <param name="data">Byte array of frame data.</param>
		/// <returns>Compressed byte array.</returns>
		private byte[] Compress(byte[] data)
		{
			byte[] compressArray;
				
			using (MemoryStream memStream = new MemoryStream(data))
			{
				try
				{
					_compressOutZStream.FlushMode = 2;
					CopyStream(memStream, _compressOutZStream);
				}
				finally
				{
					compressArray = _memStream.ToArray();
					ClearStream(_memStream, (int)_memStream.Length);
				}
			}

			return compressArray;
		}

		/// <summary>
		/// Decompress frame.
		/// </summary>
		/// <param name="data">Byte array of frame data.</param>
		/// <returns>Decompressed byte array.</returns>
		private byte[] Decompress(byte[] data)
		{
			byte[] decompressArray;
			_memStream.Position = 0;
			using (MemoryStream memStream = new MemoryStream(data))
			{
				try
				{
					CopyStream(memStream, _decompressOutZStream);
				}
				finally
				{
					_decompressOutZStream.finish();
					decompressArray = _memStream.ToArray();
				}
			}
			
			return decompressArray;
		}

		public void Dispose()
		{
			_decompressOutZStream.Close();
			_compressOutZStream.Close();
			_memStream.Close();
		}
	}
}
