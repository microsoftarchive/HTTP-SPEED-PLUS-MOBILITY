//-----------------------------------------------------------------------
// <copyright file="HttpRequest.cs" company="Microsoft Open Technologies, Inc.">
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
