using System;
using System.IO;
using System.Net;
using System.ServiceModel.Http2Protocol;
using System.Threading;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Ssl.Shared.Extensions;

namespace HTTP2DemoServer
{
    /// <summary>
    /// Http2 demo server class.
    /// </summary>
    public class Http2Server: IDisposable
    {
        private SecureTcpListener _server;

        /// <summary>
        /// Gets port that server will listen on.
        /// </summary>
        public Int32 Port { get; private set; }

        /// <summary>
        /// Initializes new instance of Http2 server.
        /// </summary>
        /// <param name="port">Port to listen.</param>
        public Http2Server(int port)
        {
            this.Port = port;

            ExtensionType[] extensions = new ExtensionType[] { ExtensionType.Renegotiation, ExtensionType.ALPN };
            SecurityOptions options = new SecurityOptions(SecureProtocol.Tls1, extensions, ConnectionEnd.Server);

            options.VerificationType = CredentialVerification.None;
            options.Certificate = Org.Mentalis.Security.Certificates.Certificate.CreateFromCerFile(@"certificate.pfx");
            options.Flags = SecurityFlags.Default;
            options.AllowedAlgorithms = SslAlgorithms.RSA_AES_128_SHA | SslAlgorithms.NULL_COMPRESSION;
            _server = new SecureTcpListener(Port, options);
        }

        private void CreateSession(VirtualSocket socket)
        {
            try
            {
                Console.WriteLine("New connection");
                
                var options = new ProtocolOptions();
                ProtocolSession session = new ProtocolSession(socket, options);
                session.OnStreamOpened += session_OnStreamOpened;
                session.OnError += session_OnError;
                session.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void session_OnError(object sender, ProtocolErrorEventArgs e)
        {
            Console.WriteLine(e.Exeption.Message);
        }

        private void session_OnStreamOpened(object sender, StreamEventArgs e)
        {
            string file = e.Stream.Headers[ProtocolHeaders.Path];
            Console.WriteLine("Requested file: " + file);

            if (string.IsNullOrEmpty(file))
            {
                Console.WriteLine("Empty file name in the request");
                e.Stream.Close(StatusCode.RefusedStream);
                return;
            }

            string path = Path.GetFullPath("root" + file);

            if (!File.Exists(path))
            {
                Console.WriteLine("File {0} not found", file);
                e.Stream.Close(StatusCode.RefusedStream);
                return;
            }

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    e.Stream.SendData(new ProtocolData(sr.ReadToEnd()), true);
                    Console.WriteLine("File {0} sent", file);
                }
            }
            finally
            {
                e.Stream.Close();
            }
        }

        /// <summary>
        /// Start server.
        /// </summary>
        public void Start()
        {
            _server.Start();
            Console.WriteLine("Started");

            while (true)
            {
                try
                {
                    VirtualSocket lastAcceptedSocket = _server.AcceptSocket();

                    if (lastAcceptedSocket != null)
                    {
                        ThreadPool.QueueUserWorkItem(delegate(object o)
                        {
                            CreateSession(lastAcceptedSocket);
                        });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }

        public void Dispose()
        {
            if (_server != null)
            {
                _server.Stop();
            }
        }
    }
}
