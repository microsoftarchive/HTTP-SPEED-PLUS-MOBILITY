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
	using System.IO;
	using ComponentAce.Compression.Libs.zlib;

	/// <summary>
	/// ZStream with dictionary support.
	/// </summary>
	class ZOutputStreamExt : ZOutputStream
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ZOutputStreamExt"/> class.
		/// </summary>
		/// <param name="outStream">The out stream.</param>
		public ZOutputStreamExt(Stream outStream)
			: base(outStream)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZOutputStreamExt"/> class.
		/// </summary>
		/// <param name="outStream">The out stream.</param>
		/// <param name="dictionary">The dictionary.</param>
		public ZOutputStreamExt(Stream outStream, byte[] dictionary)
			: base(outStream)
		{
			z.inflateSetDictionary(dictionary, dictionary.Length);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZOutputStreamExt"/> class.
		/// </summary>
		/// <param name="outStream">The out stream.</param>
		/// <param name="level">The level.</param>
		public ZOutputStreamExt(Stream outStream, int level)
			: base(outStream, level)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZOutputStreamExt"/> class.
		/// </summary>
		/// <param name="outStream">The out stream.</param>
		/// <param name="level">The level.</param>
		/// <param name="dictionary">The dictionary.</param>
		public ZOutputStreamExt(Stream outStream, int level, byte[] dictionary)
			: base(outStream, level)
		{
			z.deflateSetDictionary(dictionary, dictionary.Length);
		}

		/// <summary>
		/// Set dictionary compress.
		/// </summary>
		/// <param name="dictionary">Dictionary compression.</param>
		/// <returns></returns>
		internal int SetDictionary(byte[] dictionary)
		{
			int error;
			if (compress)
			{
				error = z.deflateSetDictionary(dictionary, dictionary.Length);
				z.deflateInit(zlibConst.Z_DEFAULT_COMPRESSION);
			}
			else
			{
				error = z.inflateSetDictionary(dictionary, dictionary.Length);
				z.inflateInit();
			}

			return error;
		}
	}
}
