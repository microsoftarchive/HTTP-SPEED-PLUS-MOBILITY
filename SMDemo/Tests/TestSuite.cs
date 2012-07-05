using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.SMProtocol;
using System.Threading;
using Client;
using ClientProtocol.ServiceModel.SMProtocol.MessageProcessing;
using NUnit.Framework;
using System.Configuration;

namespace Tests
{
	/// <summary>
	/// Integrational test suite
	/// </summary>
	[TestFixture]
	public class TestSuite
	{
		private enum CheckModes
		{
			session,
			stream,
			fin
		}
		#region Fields

		private static SMSession _session;
		private static List<SMSession> _sessions;
		private static readonly string Host = String.Empty;
		private static readonly string Port = String.Empty;
		private static readonly string FileName = String.Empty;
		private static readonly string LargeFileName = String.Empty;
		private static readonly string CountSession = String.Empty;
		private static readonly string CountStream = String.Empty;

		private static string _errorMessage = String.Empty;
		private static bool _isOpenSession = false;
		private static bool _isOpenStream = false;
		private static bool _isError = false;
		private static bool _isReceived = false;
		private static bool _isClose = false;
		private static ManualResetEvent _eventRaisedStream = new ManualResetEvent(false);
		private static ManualResetEvent _eventRaisedSession = new ManualResetEvent(false);
		private static ManualResetEvent _eventRaisedSmProtocol = new ManualResetEvent(false);

		private static SMProtocolOptions _options;
		#endregion

		#region Constructor

		static TestSuite()
		{
			try
			{
				_options = new SMProtocolOptions("s");
				Host = ConfigurationManager.AppSettings["host"];
				Port = ConfigurationManager.AppSettings["port"];
				FileName = ConfigurationManager.AppSettings["file"];
				LargeFileName = ConfigurationManager.AppSettings["largefile"];
				CountSession = ConfigurationManager.AppSettings["countsession"];
				CountStream = ConfigurationManager.AppSettings["countstream"];
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		#endregion

		#region Support methods

		/// <summary>
		/// Create new session.
		/// </summary>
		/// <param name="uri">Uri.</param>
		/// <returns>New session.</returns>
		private static SMSession CreateSession(Uri uri, SMProtocolOptions options)
		{
			_session = new SMSession(uri, false, options);
			_session.Open();
			return _session;
		}
		/// <summary>
		/// Create new session.
		/// </summary>
		/// <param name="uri">Uri.</param>
		/// <returns>New session.</returns>
		private static SMSession CreateSession(Uri uri)
		{
			return CreateSession(uri, _options);
		}
		/// <summary>
		/// Open of stream and sent request for receive data.
		/// </summary>
		/// <param name="fileName">Path to the data on server.</param>
		/// <param name="isFin">Flag fin.</param>
		/// <returns>New SMstream.</returns>
		private static SMStream DownloadPath(string fileName, bool isFin)
		{
			if (_session.State == SMSessionState.Opened)
			{
				SMHeaders headers = new SMHeaders();
				headers[SMHeaders.Path] = fileName;
				headers[SMHeaders.Version] = "http/1.1";
				headers[SMHeaders.Method] = "GET";
				headers[SMHeaders.Scheme] = "http";
				headers[SMHeaders.ContentType] = ContentTypes.GetTypeFromFileName(fileName);

				return _session.OpenStream(headers, isFin);
			}
			return null;
		}
		/// <summary>
		/// Open of stream and sent request for receive data.
		/// </summary>
		/// <param name="fileName">Path to the data on server.</param>
		/// <param name="session">Current session.</param>
		/// <param name="isFin">Flag fin.</param>
		/// <returns>New SMstream.</returns>
		private static SMStream DownloadPathForList(string fileName, ref SMSession session, bool isFin)
		{
			if (session.State == SMSessionState.Opened)
			{
				SMHeaders headers = new SMHeaders();
				headers[SMHeaders.Path] = fileName;
				headers[SMHeaders.Version] = "http/1.1";
				headers[SMHeaders.Method] = "GET";
				headers[SMHeaders.Scheme] = "http";
				headers[SMHeaders.ContentType] = ContentTypes.GetTypeFromFileName(fileName);

				return session.OpenStream(headers, isFin);
			}
			return null;
		}
		/// <summary>
		/// Attached to events on session.
		/// </summary>
		/// <param name="session">Session.</param>
		private static void AttachSessionEvents(SMSession session)
		{
			session.OnOpen += (s, e) =>
			{
				_isOpenSession = true;
				_eventRaisedSession.Set();
			};
			
			session.OnStreamOpened += (s, e) =>
			{
				if (e.Stream.StreamId % 2 == 0)
				{
					_isError = true;
					_errorMessage = "StreamId must be odd";
				}
				_isOpenStream = true;
				_eventRaisedStream.Set();
			};
			session.OnError += (s, e) =>
									{
										_isError = true;
										_errorMessage = e.Exeption.Message;
										_eventRaisedStream.Set();
										_eventRaisedSession.Set();
									};
		}
		/// <summary>
		/// Attached to events on stream.
		/// </summary>
		/// <param name="stream">Stream.</param>
		private static void AttachStreamEvents(SMStream stream)
		{
			stream.OnRSTReceived += (s, e) =>
			                        	{
			                        		_isError = true;
			                        		_errorMessage = e.Reason.ToString();
			                        		_eventRaisedStream.Set();
			                        	};
			stream.OnDataReceived += (s, e) =>
			                         	{
			                         		string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

			                         		using (var fs = new FileStream(file, FileMode.Create))
			                         		{
			                         			fs.Write(e.Data.Data, 0, e.Data.Data.Length);
			                         		}

			                         		string text = e.Data.AsUtf8Text();
			                         		_isReceived = !string.IsNullOrEmpty(text);
			                         		if (_isClose)
			                         			_eventRaisedStream.Set();
			                         	};
			stream.OnClose += (s, e) =>
			                  	{
			                  		_isClose = true;
			                  		if (_isReceived)
			                  			_eventRaisedStream.Set();
			                  	};
		}
		/// <summary>
		/// Attached to events on protocol.
		/// </summary>
		/// <param name="smProtocol">Protocol.</param>
		private static void AttachProtocolEvents(SMProtocol smProtocol)
		{
			smProtocol.OnError += (s, e) =>
			{
				_eventRaisedSmProtocol.Reset();
				_isError = true;
				_eventRaisedSmProtocol.Set();
				_eventRaisedStream.Set();
			};
			/*	smProtocol.OnSessionFrame += (Object s, ControlFrameEventArgs e) =>
												{
													eventRaisedSMProtocol.Reset();
													ControlFrame frame = e.Frame;
													// isFinal and (State == HalfClose) MUST take the same values
													if (frame.IsFinal ^
														(_session.GetStreamById(frame.StreamId).State == SMStreamState.HalfClosed))
													{
														isError = true;
														errorMessage = "Incorrect value Stream.State.";

														eventRaised.Set();
													}
													eventRaisedSMProtocol.Set();
												};*/
			smProtocol.OnStreamError += (s, e) =>
			{
				_eventRaisedSmProtocol.Reset();
				_isError = true;
				_errorMessage = "Internal stream error.";
				_eventRaisedSmProtocol.Set();
				_eventRaisedStream.Set();
			};
			smProtocol.OnStreamFrame += (s, e) =>
			{
				_eventRaisedSmProtocol.Reset();

				if (e is HeadersEventArgs)
				{
					HeadersEventArgs headersEventArgs = e as HeadersEventArgs;

					if (headersEventArgs.Headers.Count == 0)
					{
						_isError = true;
						_errorMessage = "Incorrect value Frame.Headers.Count.";
						_eventRaisedStream.Set();
					}
				}
				else if (e is StreamDataEventArgs)
				{
					StreamDataEventArgs streamDataEventArgs = e as StreamDataEventArgs;

					if (streamDataEventArgs.Data.Data.Length == 0)
					{
						_isError = true;
						_errorMessage = "Incorrect Data.Length.";
						_eventRaisedStream.Set();
					}
				}
				else if (e is RSTEventArgs)
				{
					RSTEventArgs rstEventArgs = e as RSTEventArgs;

					if (rstEventArgs.Reason != StatusCode.Cancel)
					{
						_isError = true;
						_errorMessage = "Incorrect reason in RST frame.";
						_eventRaisedStream.Set();
					}
				}

				_eventRaisedSmProtocol.Set();
			};
		}
		/// <summary>
		/// Checked current state on exceptions.
		/// </summary>
		/// <param name="mode">Mode for checked.</param>
		private static void Check(CheckModes mode)
		{
			if (_isError)
				Assert.Fail(_errorMessage);
			switch (mode)
			{
				case CheckModes.session:
					if (!_isOpenSession)
						Assert.Fail("Could not open session.");
					break;
				case CheckModes.fin:
					if (!_isClose)
						Assert.Fail("Could not close stream.");
					if (!_isReceived)
						Assert.Fail("Could not recieve data.");
					break;
				case CheckModes.stream:
					if (!_isOpenStream)
						Assert.Fail("Could not open stream.");
					break;
			}
		}
		/// <summary>
		/// Reset flags and EventRaised.
		/// </summary>
		private static void Reset()
		{
			_errorMessage = String.Empty;
			_isOpenSession = false;
			_isOpenStream = false;
			_isError = false;
			_isReceived = false;
			_isClose = false;

			_eventRaisedStream.Reset();
			_eventRaisedSession.Reset();
			_eventRaisedSmProtocol.Reset();
		}
		#endregion
		
		#region Test methods
		/// <summary>
		/// Successful test for get file without compress.
		/// </summary>
		[STAThread]
		[Test]
		public void CompressOffSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			_options.UseCompression = false;
			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			bool isFin = true;
			SMStream stream = DownloadPath(FileName, isFin);
			AttachStreamEvents(stream);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
		}

		/// <summary>
		/// Successful test for use dictionary without adaptation.
		/// </summary>
		[STAThread]
		[Test]
		public void AdaptiveDictionaryOffSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			_options.CoompressionIsStateful = false;
			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			bool isFin = true;
			SMStream stream = DownloadPath(FileName, isFin);
			AttachStreamEvents(stream);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
		}
		/// <summary>
		/// Successful test for use dictionary with adaptation.
		/// </summary>
		[STAThread]
		[Test]
		public void AdaptiveDictionaryOnSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne(); 
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			bool isFin = true;
			SMStream stream = DownloadPath(FileName, isFin);
			AttachStreamEvents(stream);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
		}

		/// <summary>
		/// Successful test for sent header without compression.
		/// </summary>
		[STAThread]
		[Test]
		public void SendHeaderWithoutCompressSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			_options.CoompressionIsStateful = false;
			CreateSession(uri);
			AttachSessionEvents(_session);
			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			_eventRaisedStream.Reset();

			string fileName = FileName;
			bool isFin = true;
			SMStream stream = DownloadPath(fileName, isFin);
			AttachStreamEvents(stream);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);

			_eventRaisedStream.Reset();
			_eventRaisedStream.WaitOne();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
		}

		/// <summary>
		/// Successful test for open stream with odd id.
		/// </summary>
		[STAThread]
		[Test]
		public void ConnectionOpenOddStreamSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			string fileName = FileName;
			bool isFin = true;
			SMStream stream = DownloadPath(fileName, isFin);
			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			AttachStreamEvents(stream);
			
			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
		}

		/// <summary>
		/// Successful test for open connection and recieved data text.
		/// </summary>
		[STAThread]
		[Test]
		public void ConnectionOpenAndTextDataReceivedSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			string fileName = FileName;
			bool isFin = true;
			SMStream stream = DownloadPath(fileName, isFin);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			AttachStreamEvents(stream);
			
			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
		}

		/// <summary>
		/// Successful test for open many connections and recieved data text.
		/// </summary>
		[STAThread]
		[Test]
		public void ManyConnectionOpenAndTextDataReceivedSuccessfully()
		{
			Reset();
			int numberSessions = Convert.ToInt32(CountSession);
			int numberStreams = Convert.ToInt32(CountStream);

			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			_sessions = new List<SMSession>(numberSessions);
			string fileName = FileName;

			for (int i = 0; i < numberSessions; i++)
			{
				Reset();
				SMSession newSession = CreateSession(uri);
				AttachSessionEvents(newSession);

				_eventRaisedSession.WaitOne();
				_eventRaisedSession.Reset();
				Check(CheckModes.session);

				for (int j = 0; j < numberStreams; j++)
				{
					Reset();
					bool isFin = true;
					SMStream stream = DownloadPathForList(fileName, ref newSession, isFin);

					_eventRaisedStream.WaitOne();
					_eventRaisedStream.Reset();
					Check(CheckModes.stream);
					if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");
					AttachStreamEvents(stream);

					_eventRaisedStream.WaitOne();
					_eventRaisedStream.Reset();

					Check(CheckModes.fin);
					if ((stream.State == SMStreamState.Closed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");
				}
				_sessions.Add(newSession);
			}

			Assert.Pass();
		}

		/// <summary>
		/// Sccessful test for open connection and recieved large data text.
		/// </summary>
		[STAThread]
		[Test]
		public void ConnectionOpenAndLargeTextDataReceivedSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			AttachSessionEvents(_session);
		
			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);
			

			string fileName = LargeFileName;
			bool isFin = true;
			SMStream stream = DownloadPath(fileName, isFin);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			AttachStreamEvents(stream);
			
			_eventRaisedStream.Reset();
			_eventRaisedStream.WaitOne();

			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			

		}

		[STAThread]
		[Test]
		public void ManyConnectionOpenAndLargeTextDataReceivedSuccessful()
		{
			Reset();
			int numberSessions = Convert.ToInt32(CountSession);
			int numberStreams = Convert.ToInt32(CountStream);

			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			_sessions = new List<SMSession>(numberSessions);
			string fileName = LargeFileName;
			for (int i = 0; i < numberSessions; i++)
			{
				Reset();
				SMSession newSession = CreateSession(uri);
				AttachSessionEvents(newSession);

				_eventRaisedSession.WaitOne();
				_eventRaisedSession.Reset();
				Check(CheckModes.session);
				
				for (int j = 0; j < numberStreams; j++)
				{
					Reset();
					bool isFin = true;
					SMStream stream = DownloadPathForList(fileName, ref newSession, isFin);

					_eventRaisedStream.WaitOne();
					_eventRaisedStream.Reset();
					Check(CheckModes.stream);
					if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");
					AttachStreamEvents(stream);

					_eventRaisedStream.WaitOne();
					_eventRaisedStream.Reset();
					Check(CheckModes.fin);
					if ((stream.State == SMStreamState.Closed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");
				}
				_sessions.Add(newSession);
			}
			
		}

		/// <summary>
		/// Successful test for open connection and check control frames.
		/// </summary>
		[STAThread]
		[Test]
		public void ConnectionOpenAndCheckControlFrameSuccessful()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);
		
			SMProtocol smProtocol = _session.Protocol;
			AttachProtocolEvents(smProtocol);
			
			string fileName = FileName;
			bool isFin = true;
			SMStream stream = DownloadPath(fileName, isFin);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			AttachStreamEvents(stream);
			
			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect value Stream.State.");

			_eventRaisedSmProtocol.WaitOne();

			Assert.Pass();
		}

		/// <summary>
		/// Failure test for sent frame via closed stream.
		/// </summary>
		[STAThread]
		[Test]
		public void SendFrameOnCloseStreamFailure()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			AttachSessionEvents(_session);

			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			string fileName = FileName;
			bool isFin = true;

			SMStream stream = DownloadPath(fileName, isFin);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			AttachStreamEvents(stream);
			
			_eventRaisedStream.Reset();
			_eventRaisedStream.WaitOne();
			Check(CheckModes.fin);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect value Stream.State.");

			_isError = false;
			SMData data = new SMData(new byte[] { 12, 3, 23, 35, 3, 11 });

			stream.SendData(data, isFin);
			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();

			Assert.IsTrue(_isError);
		}

		/// <summary>
		/// Failure test for try receive file with incorrect name.
		/// </summary>
		[STAThread]
		[Test]
		public void IncorrectFileNameFailure()
		{
			Reset();
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			AttachSessionEvents(_session);
		
			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Check(CheckModes.session);

			Guid guid = Guid.NewGuid();
			string fileName = guid +  FileName;
			bool isFin = false;
			SMStream stream = DownloadPath(fileName, isFin);

			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Check(CheckModes.stream);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			AttachStreamEvents(stream);
			
			_eventRaisedStream.WaitOne();
			_eventRaisedStream.Reset();
			Assert.True(_errorMessage == "InternalError");
		}

		/// <summary>
		/// Failure test for opening session with incorrect address.
		/// </summary>
		[STAThread]
		[Test]
		public void IncorrectAddressFailure()
		{
			Reset();
			Random rand = new Random(DateTime.Now.Millisecond);
			int random = rand.Next() % 1000;
			Uri uri;
			Uri.TryCreate(String.Format("ws://{0}:{1}", random, Port), UriKind.Absolute, out uri);

			_session = new SMSession(uri, false, _options);
			AttachSessionEvents(_session);
			
			SMProtocol smProtocol = _session.Protocol;
			smProtocol.OnError += (s, e) =>
									{
										_isError = true;
										_eventRaisedStream.Set();
									};

			_session.Open();
			_eventRaisedSession.WaitOne();
			_eventRaisedSession.Reset();
			Assert.IsTrue(_isError);
		}

		/// <summary>
		/// failure test for opening session with incorrect port.
		/// </summary>
		[STAThread]
		[Test]
		public void IncorrectPortFailure()
		{
			Reset();
			Random rand = new Random(DateTime.Now.Millisecond);
			int random = rand.Next() % 1000;
			Uri uri;
			Uri.TryCreate(Host + random, UriKind.Absolute, out uri);
			
			_session = new SMSession(uri, false, _options);
			AttachSessionEvents(_session);

			SMProtocol smProtocol = _session.Protocol;
			smProtocol.OnError += (s, e) =>
			{
				_isError = true;
				_eventRaisedStream.Set();
			};

			_session.Open();
			_eventRaisedSession.WaitOne();

			Assert.IsTrue(_isError);
		}

		#endregion
	}
}
