//-----------------------------------------------------------------------
// <copyright file="HttpRequest.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Client.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Client.HttpBenchmark;
    using Client.Utils;

	/// <summary>
	/// Http helper class. 
	/// </summary>
	public sealed class HttpRequest : IDisposable
	{
		/// <summary>
		/// Const for number of parallel loaders
		/// </summary>
		private const int MaxLoaderThreads = 6;

		/// <summary>
		/// Const for buffer size.
		/// </summary>
		private const int BufferSize = 1024 * 1024 * 4;

		/// <summary>
		/// HTTP monitor.
		/// </summary>
		private readonly HttpTrafficLog httpMonitor = new HttpTrafficLog();

		/// <summary>
		/// Download thread events.
		/// </summary>
		private ManualResetEvent[] loaderEvents;

		/// <summary>
		/// Read/write events.
		/// </summary>
		private ManualResetEvent[] readWriteEvents;

		/// <summary>
		/// Download name monitor event
		/// </summary>
		private ManualResetEvent nameMonitorEvent;

		/// <summary>
		/// Download buffers
		/// </summary>
		private byte[][] inbuffers;

		/// <summary>
		/// Download thread info packages
		/// </summary>
		private RequestPackage[] getFilesPackages;

		/// <summary>
		/// Disposed flag
		/// </summary>   
		private bool disposed;

		/// <summary>
		/// Name monitor lock
		/// </summary>
		[NonSerialized]
		private object exclusiveLock;

		/// <summary>
		/// Download name list
		/// </summary>
		private List<string> namesToDownload;

        /// <summary>
        /// Server side latency
        /// </summary>
        private int serverLatency = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequest"/> class.
		/// </summary>
        /// <param name="serverLatency">Server latency.</param>
		public HttpRequest(int serverLatency)
		{
			this.exclusiveLock = new object();
			this.namesToDownload = new List<string>();
			this.loaderEvents = new ManualResetEvent[MaxLoaderThreads];
			this.readWriteEvents = new ManualResetEvent[MaxLoaderThreads];
			this.nameMonitorEvent = new ManualResetEvent(false);
			this.getFilesPackages = new RequestPackage[MaxLoaderThreads];
			this.inbuffers = new byte[MaxLoaderThreads][];

			for (int i = 0; i < MaxLoaderThreads; i++)
			{
				this.loaderEvents[i] = new ManualResetEvent(true);
				this.readWriteEvents[i] = new ManualResetEvent(false);
				this.getFilesPackages[i].LoaderEvent = this.loaderEvents[i];
				this.getFilesPackages[i].ThreadId = i;
				this.getFilesPackages[i].Processed = 1;
			}

            this.serverLatency = serverLatency;
		}

		/// <summary>
		/// Dispose this instance.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.nameMonitorEvent.Dispose();
				for (int i = 0; i < MaxLoaderThreads; i++)
				{
					this.loaderEvents[i].Dispose();
				}
			}
		}

		/// <summary>
		/// Get files via http request. Public interface to call for top file of the tree
		/// </summary>
		/// <param name="uri">The address site.</param>
		/// <returns>New traffic log.</returns>
		public HttpTrafficLog GetFile(string uri)
		{
			this.httpMonitor.LogTitle = "HTTP " + Path.GetFileName(uri);

			// top level download is in main thread context
			// because we parse HTML to get list of other files to download
			string type = ContentTypes.GetTypeFromFileName(uri);
			Uri requestUri = new Uri(uri);
			byte[] content = this.Get(requestUri);

			this.PrivateGetFile(content, type, requestUri, uri);

			// give some time to name list to spin up
			Thread.Sleep(50);

			// wait for name monitor to finish
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.NameListMonitorProc), 0);
			this.nameMonitorEvent.WaitOne(600000);

			// wait for downloads to finish
			WaitHandle.WaitAll(this.loaderEvents);

			return this.httpMonitor;
		}

		/// <summary>
		/// Authenticate client.
		/// </summary>
		/// <param name="remoteEndpoint">Uri connect.</param>
		/// <param name="sslStream">Sslstreamn argument.</param>
		/// <returns>Returned true if success.</returns>
		private static bool AuthenticateAsClient(Uri remoteEndpoint, SslStream sslStream)
		{
			try
			{
				X509Certificate certificate = new X509Certificate("certificate.pfx");
				sslStream.AuthenticateAsClient(remoteEndpoint.Host, new X509CertificateCollection(new[] { certificate }), SslProtocols.Tls, false);
			}
			catch (AuthenticationException e)
			{
				SMLogger.LogError(e.Message);
				if (e.InnerException != null)
				{
					SMLogger.LogError(string.Format("Inner exception: {0}", e.InnerException.Message));
				}

				SMLogger.LogError("Authentication failed - closing the connection.");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get files via http request.
		/// </summary>
		/// <param name="content">Downloaded file.</param>
		/// <param name="type">Type of file.</param>
		/// <param name="requestUri">Full name of file.</param>
		/// <param name="uri">The address site.</param>
		private void PrivateGetFile(byte[] content, string type, Uri requestUri, string uri)
		{
			byte[] headers = this.GetHeaders(content);
			if (headers == null)
			{
				SMLogger.LogError("HTTP response: Invalid");
				return;
			}

			this.httpMonitor.LogResponse(headers, content.Length);

			int status = this.GetStatus(headers);

			SMLogger.LogInfo(string.Format("HTTP response: {0}, length: {1}  name:{2}", status, content.LongLength, uri));

			if (status == 200)
			{
				string url = requestUri.Scheme + "://" + requestUri.Authority;
				string directory = string.Empty;
				string localDir = string.Empty;
				string file = requestUri.LocalPath;
				string localFile = Path.GetFileName(uri);

				for (int i = 0; i < requestUri.Segments.Length - 1; i++)
				{
					directory += requestUri.Segments[i];
					localDir += requestUri.Segments[i].Replace('/', '\\');
				}

				if (!string.IsNullOrEmpty(localDir))
				{
					if (localDir[0] == '\\')
					{
						localDir = '.' + localDir;
					}

					Directory.CreateDirectory(localDir);
					localFile = localDir + '\\' + localFile;
				}

				int contentOffset = headers.Length;
				using (var fs = new FileStream(localFile, FileMode.Create))
				{
					fs.Write(content, contentOffset, content.Length - contentOffset);
				}

				if (type == ContentTypes.TextHtml)
				{
					XHtmlDocument document = XHtmlDocument.Parse(Encoding.UTF8.GetString(content, contentOffset, content.Length - contentOffset));

					string path = url + directory;
					foreach (var image in document.Images)
					{
						this.AddNameToDownloadList(string.Format("{0}/{1}", path.ToLower(), image.ToLower()));
					}

					foreach (var link in document.Links)
					{
						this.AddNameToDownloadList(string.Format("{0}/{1}", path.ToLower(), link.ToLower()));
					}

					foreach (var script in document.Scripts)
					{
						this.AddNameToDownloadList(string.Format("{0}/{1}", path.ToLower(), script.ToLower()));
					}
				}
			} // if status 200
		}

		/// <summary>
		/// Get files via URI.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns>File as byte array.</returns>
		private byte[] Get(Uri uri)
		{
			using (TcpClient tcpClient = new TcpClient())
			{
				tcpClient.Connect(uri.Host, uri.Port);
				var streamHttp = tcpClient.GetStream();
				Stream stream = null;

				if (uri.Scheme.Equals("http"))
				{
					stream = tcpClient.GetStream();
				}
				else
				{
					stream = new SslStream(streamHttp, false, null, null);
					if (!AuthenticateAsClient(uri, (SslStream)stream))
					{
						tcpClient.Close();
						return null;
					}
				}

				string headers = string.Format(
					"GET {2} HTTP/1.1\r\n"
					+ "Host: {0}:{1}\r\n"
					+ "Connection: Keep-Alive\r\n"
					+ "User-Agent: SMClient\r\n"
					+ "Accept: {3},application/xml;q=0.9,*/*;q=0.8\r\n",
					uri.Host,
					uri.Port,
					uri,
					ContentTypes.GetTypeFromFileName(uri.ToString()));

				byte[] headersBytes = Encoding.UTF8.GetBytes(headers + "\r\n");
				this.httpMonitor.LogRequest(headersBytes);

				stream.Write(headersBytes, 0, headersBytes.Length);

				int totalCount = 0;
				byte[] inbuffer = new byte[BufferSize];

                int max = 12;
                if (this.serverLatency > 200)
                {
                    max += (this.serverLatency - 200) / 14;
                }

                do
				{
					int c = stream.Read(inbuffer, totalCount, inbuffer.Length - totalCount);
					if (c <= 0)
					{
						break;
					}

					totalCount += c;

					// speed up sync socket
					int i = 0;
					for (i = 0; i < max; i++)
					{
						if (tcpClient.Available > 0)
						{
							break;
						}

						Thread.Sleep(40);
					}
				}
				while (tcpClient.Available > 0);

				byte[] result = new byte[totalCount];
				Buffer.BlockCopy(inbuffer, 0, result, 0, totalCount);
				return result;
			}
		}

		/// <summary>
		/// Get headers.
		/// </summary>
		/// <param name="content">The stream content.</param>
		/// <returns>headers as byte array.</returns>
		private byte[] GetHeaders(byte[] content)
		{
			if (content.Length <= 12)
			{
				return null;
			}

			int cur = 0;
			while (Encoding.UTF8.GetString(content, ++cur, 4) != "\r\n\r\n")
			{
			}

			byte[] headers = new byte[cur + 4];
			Buffer.BlockCopy(content, 0, headers, 0, headers.Length);
			return headers;
		}

		/// <summary>
		/// Get status from headers.
		/// </summary>
		/// <param name="headers">The headers.</param>
		/// <returns>Status as int.</returns>
		private int GetStatus(byte[] headers)
		{
			return int.Parse(Regex.Match(Encoding.UTF8.GetString(headers, 0, 12), "\\d{3}").Value);
		}

		/// <summary>
		/// Thread proc for file download
		/// </summary>
		/// <param name="stateInfo">RequestPackage for the thread</param>
		private void HttpLoaderProc(object stateInfo)
		{
			RequestPackage package = (RequestPackage)stateInfo;

			this.inbuffers[package.ThreadId] = this.Get(package.RequestUri);
			package.Processed = 0;
			this.PrivateGetFile(this.inbuffers[package.ThreadId], package.Type, package.RequestUri, package.Name);
			this.getFilesPackages[package.ThreadId].Processed = 1;
			this.getFilesPackages[package.ThreadId].LoaderEvent.Set();
		}

		/// <summary>
		/// Thread proc for name list monitor
		/// </summary>
		/// <param name="stateInfo">RequestPackage for the thread</param>
		private void NameListMonitorProc(object stateInfo)
		{
			while (true)
			{
				string name = this.GetNameToDownload();
				if (name == string.Empty)
				{
					// done downloading
					this.nameMonitorEvent.Set();
					return;
				}

				bool found = false;
				int currentThread = 0;
				while (found == false)
				{
					// get first available thread
					for (int i = 0; i < MaxLoaderThreads; i++)
					{
						if (this.getFilesPackages[i].Processed == 1)
						{
							currentThread = i;
							found = true;
							break;
						}
					}

					if (found == false)
					{
						// wait a bit before trying again
						Thread.Sleep(100);
					}
				}

				// start downloading
				Uri requestUri = new Uri(name);
				this.getFilesPackages[currentThread].Type = ContentTypes.GetTypeFromFileName(name);
				this.getFilesPackages[currentThread].RequestUri = requestUri;
				this.getFilesPackages[currentThread].Name = name;
				this.getFilesPackages[currentThread].LoaderEvent = this.loaderEvents[currentThread];
				this.getFilesPackages[currentThread].LoaderEvent.Reset();
				this.getFilesPackages[currentThread].Processed = 0;

				ThreadPool.QueueUserWorkItem(new WaitCallback(this.HttpLoaderProc), this.getFilesPackages[currentThread]);
			}
		}

		/// <summary>
		/// Add name to list of files to download
		/// </summary>
		/// <param name="uri">name of file</param>
		private void AddNameToDownloadList(string uri)
		{
			lock (this.exclusiveLock)
			{
				this.namesToDownload.Add(uri);
			}
		}

		/// <summary>
		/// Get next name to download
		/// </summary>
		/// <returns>name of empty string</returns>
		private string GetNameToDownload()
		{
			string ret = string.Empty;
			lock (this.exclusiveLock)
			{
				if (this.namesToDownload.Count > 0)
				{
					ret = this.namesToDownload[0];
					this.namesToDownload.Remove(ret);
				}
			}

			return ret;
		}
	}
}
