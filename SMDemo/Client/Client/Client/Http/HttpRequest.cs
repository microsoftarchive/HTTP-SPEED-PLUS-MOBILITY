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
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Client.HttpBenchmark;
    using Client.Utils;

    /// <summary>
    /// Http helper class. 
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// Const for buffer size.
        /// </summary>
        private const int BufferSize = 1024 * 1024 * 4;

        /// <summary>
        /// Internal buffer.
        /// </summary>
        private readonly byte[] buffer;

        /// <summary>
        /// HTTP monitor.
        /// </summary>
        private readonly HttpTrafficLog httpMonitor = new HttpTrafficLog();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class.
        /// </summary>
        public HttpRequest()
        {
            this.buffer = new byte[BufferSize];
        }

        /// <summary>
        /// Get files via http request.
        /// </summary>
        /// <param name="uri">The address site.</param>
        /// <returns>New traffic log.</returns>
        public HttpTrafficLog GetFile(string uri)
        {
            string type = ContentTypes.GetTypeFromFileName(uri);
            string file = Path.GetFileName(uri);
            try
            {
                Uri requetUri = new Uri(uri);
                byte[] content = this.Get(requetUri);
                byte[] headers = this.GetHeaders(content);
                this.httpMonitor.LogResponse(headers, content.Length);

                if (headers == null)
                {
                    SMLogger.LogError("HTTP response: Invalid");
                    return this.httpMonitor;
                }

                int status = this.GetStatus(headers);

                SMLogger.LogInfo(string.Format("HTTP response: {0}, length: {1}", status, content.LongLength));

                if (status == 200)
                {
                    string url = requetUri.Scheme + "://" + requetUri.Authority;
                    string directory = string.Empty;

                    for (int i = 0; i < requetUri.Segments.Length - 1; i++)
                    {
                        directory += requetUri.Segments[i];
                    }

                    int contentOffset = headers.Length;
                    using (var fs = new FileStream(file, FileMode.Create))
                    {
                        fs.Write(content, contentOffset, content.Length - contentOffset);
                    }

                    if (type == ContentTypes.TextHtml)
                    {
                        XHtmlDocument document = XHtmlDocument.Parse(Encoding.UTF8.GetString(content, contentOffset, content.Length - contentOffset));

                        foreach (var image in document.Images)
                        {
                            this.GetFile(string.Format("{0}/{1}", url + directory, image));
                        }

                        foreach (var link in document.Links)
                        {
                            this.GetFile(string.Format("{0}/{1}", url + directory, link));
                        }

                        foreach (var script in document.Scripts)
                        {
                            this.GetFile(string.Format("{0}/{1}", url + directory, script));
                        }
                    }
                }
            }
            catch
            {
                SMLogger.LogError("Unable to execute HTTPGET for " + uri.ToString());
            }

            return this.httpMonitor;
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
                var stream = tcpClient.GetStream();

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

                do
                {
                    int c = stream.Read(this.buffer, totalCount, this.buffer.Length - totalCount);
                    if (c <= 0)
                    {
                        break;
                    }

                    totalCount += c;
                    Thread.Sleep(150);
                } 
                while (tcpClient.Available > 0);

                byte[] result = new byte[totalCount];
                Buffer.BlockCopy(this.buffer, 0, result, 0, totalCount);
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
    }
}
