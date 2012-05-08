using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.SMProtocol;
using System.Threading;
using Client;
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
		#region Fields

		private static SMSession _session;
		private static List<SMSession> _sessions;
		private static readonly string Host = String.Empty;
		private static readonly string Port = String.Empty;
		private static readonly string FileName = String.Empty;
		private static readonly string LargeFileName = String.Empty;
		private static readonly string CountSession = String.Empty;
		private static readonly string CountStream = String.Empty;

		#endregion

		#region Constructor

		static TestSuite()
		{
			try
			{
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

		private static SMSession CreateSession(Uri uri)
		{
			_session = new SMSession(uri, false);
			_session.Open();
			return _session;
		}

		private static SMStream DownloadPath(string fileName, bool isFin)
		{
			if (_session.State == SMSessionState.Opened)
			{
				SMHeaders headers = new SMHeaders();
				headers[SMHeaders.Path] = fileName;
				headers[SMHeaders.Scheme] = fileName;
				headers[SMHeaders.Version] = "http/1.1";
				headers[SMHeaders.Method] = "GET";
				headers[SMHeaders.Scheme] = "http";
				headers[SMHeaders.ContentType] = ContentTypes.GetTypeFromFileName(fileName);

				return _session.OpenStream(headers, isFin);
			}
			return null;
		}

		private static SMStream DownloadPathForList(string fileName, ref SMSession session, bool isFin)
		{
			if (session.State == SMSessionState.Opened)
			{
				SMHeaders headers = new SMHeaders();
				headers[SMHeaders.Path] = fileName;
				headers[SMHeaders.Scheme] = fileName;
				headers[SMHeaders.Version] = "http/1.1";
				headers[SMHeaders.Method] = "GET";
				headers[SMHeaders.Scheme] = "http";
				headers[SMHeaders.ContentType] = ContentTypes.GetTypeFromFileName(fileName);

				return session.OpenStream(headers, isFin);
			}
			return null;
		}
		#endregion

		#region Test methods

		[STAThread]
		[Test]
		public void ConnectionOpenOddStreamSuccessfull()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			string errorMessage = String.Empty;
			bool isOpenSession = false;
			bool isError = false;

			_session.OnOpen += (s, e) =>
			{
				isOpenSession = true;
				eventRaised.Set();
			};
			_session.OnError += (s, e) =>
			{
				isError = true;
				errorMessage = "Internal session error.";
				eventRaised.Set();
			};
			_session.OnStreamOpened += (s, e) =>
			{
				if (e.Stream.StreamId % 2 == 0)
				{
					isError = true;
					errorMessage = "StreamId must be odd";
				}
				eventRaised.Set();
			};
			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if (!isOpenSession)
				Assert.Fail("Could not open stream.");

			eventRaised.Reset();

			string fileName = FileName;
			bool isReceived = false;
			bool isFin = true;
			bool isClose = false;
			SMStream stream = DownloadPath(fileName, isFin);

			eventRaised.WaitOne();
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			stream.OnRSTReceived += (s, e) => eventRaised.Set();
			stream.OnDataReceived += (s, e) =>
			{
				string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

				using (var fs = new FileStream(file, FileMode.Create))
				{
					fs.Write(e.Data.Data, 0, e.Data.Data.Length);
				}

				string text = e.Data.AsUtf8Text();
				isReceived = !string.IsNullOrEmpty(text);
				eventRaised.Set();
			};
			stream.OnClose += (s, e) =>
			{
				isClose = true;
				eventRaised.Set();
			};

			eventRaised.WaitOne();

			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			if (isClose)
				Assert.Fail("Could not close stream.");
			if (isReceived)
				Assert.Fail("Could not recieve data.");
		}

		[STAThread]
		[Test]
		public void ConnectionOpenAndTextDataReceivedSuccessfull()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);
			string errorMessage = String.Empty;
			bool isOpenSession = false;
			bool isError = false;

			_session.OnOpen += (s, e) =>
								{
									isOpenSession = true;
									eventRaised.Set();
								};
			_session.OnError += (s, e) =>
			{
				isError = true;
				errorMessage = "Internal session error.";
				eventRaised.Set();
			};
			_session.OnStreamOpened += (s, e) =>
			{
				eventRaised.Set();
			};
			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if (!isOpenSession)
				Assert.Fail("Could not open stream.");

			eventRaised.Reset();

			string fileName = FileName;
			bool isReceived = false;
			bool isFin = true;
			bool isClose = false;
			SMStream stream = DownloadPath(fileName, isFin);

			eventRaised.WaitOne();
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			stream.OnRSTReceived += (s, e) => eventRaised.Set();
			stream.OnDataReceived += (s, e) =>
											{
												string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

												using (var fs = new FileStream(file, FileMode.Create))
												{
													fs.Write(e.Data.Data, 0, e.Data.Data.Length);
												}

												string text = e.Data.AsUtf8Text();
												isReceived = !string.IsNullOrEmpty(text);
												eventRaised.Set();
											};
			stream.OnClose += (s, e) =>
								{
									isClose = true;
									eventRaised.Set();
								};

			eventRaised.WaitOne();

			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			if (isClose)
				Assert.Fail("Could not close stream.");
			if (isReceived)
				Assert.Fail("Could not recieve data.");
		}

		[STAThread]
		[Test]
		public void ManyConnectionOpenAndTextDataReceivedSuccessfully()
		{
			int numberSessions = Convert.ToInt32(CountSession);
			int numberStreams = Convert.ToInt32(CountStream);

			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			_sessions = new List<SMSession>(numberSessions);
			string fileName = FileName;
			bool isClose = false;
			bool isError = false;
			string errorMessage = String.Empty;

			for (int i = 0; i < numberSessions; i++)
			{
				eventRaised.Reset();
				SMSession newSession = CreateSession(uri);
				bool isOpenSession = false;
				newSession.OnStreamOpened += (s, e) => eventRaised.Set();
				newSession.OnOpen += (s, e) =>
				{
					isOpenSession = true;
					eventRaised.Set();
				};

				newSession.OnError += (s, e) =>
										{
											isError = true;
											errorMessage = "Internal session error.";
											eventRaised.Set();
										};
				eventRaised.WaitOne();

				if (!isOpenSession)
					Assert.Fail("Could not open session.");
				if (isError)
					Assert.Fail(errorMessage);

				eventRaised.Reset();

				for (int j = 0; j < numberStreams; j++)
				{
					eventRaised.Reset();
					bool isReceived = false;
					bool isFin = true;
					SMStream stream = DownloadPathForList(fileName, ref newSession, isFin);

					eventRaised.WaitOne();
					if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");

					stream.OnRSTReceived += (s, e) => eventRaised.Set();
					stream.OnDataReceived += (s, e) =>
					{
						string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

						using (var fs = new FileStream(file, FileMode.Create))
						{
							fs.Write(e.Data.Data, 0, e.Data.Data.Length);
						}

						string text = e.Data.AsUtf8Text();
						isReceived = true;
					};

					stream.OnClose += (s, e) =>
					{
						isClose = true;
						eventRaised.Set();
					};
					eventRaised.Reset();
					eventRaised.WaitOne();

					if (isError)
						Assert.Fail("Session error");
					if ((stream.State == SMStreamState.Closed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");
					if (!isReceived)
						Assert.Fail("Could not recieve data.");
				}
				_sessions.Add(newSession);
			}

			if (!isClose)
				Assert.Fail("Could not close stream.");

			Assert.Pass();
		}

		[STAThread]
		[Test]
		public void ConnectionOpenAndLargeTextDataReceivedSuccessfull()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);

			bool isOpenSession = false;
			bool isError = false;
			bool isOpenStream = false;
			string errorMessage = String.Empty;
			_session.OnOpen += (s, e) =>
			{
				isOpenSession = true;
				eventRaised.Set();
			};
			_session.OnError += (s, e) =>
			{
				isError = true;
				errorMessage = "Internal session error.";
				eventRaised.Set();
			};
			_session.OnStreamOpened += (s, e) =>
										{
											isOpenStream = true;
											eventRaised.Set();
										};
			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if (!isOpenSession)
				Assert.Fail("Could not open session.");

			eventRaised.Reset();

			string fileName = LargeFileName;
			bool isReceived = false;
			bool isFin = true;
			bool isClose = false;
			SMStream stream = DownloadPath(fileName, isFin);

			eventRaised.WaitOne();
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			stream.OnRSTReceived += (s, e) => eventRaised.Set();
			stream.OnDataReceived += (s, e) =>
			{
				string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

				using (var fs = new FileStream(file, FileMode.Create))
				{
					fs.Write(e.Data.Data, 0, e.Data.Data.Length);
				}

				string text = e.Data.AsUtf8Text();
				isReceived = !string.IsNullOrEmpty(text);
			};
			stream.OnClose += (s, e) =>
			{
				isClose = true;
				eventRaised.Set();
			};
			eventRaised.Reset();
			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if (!isOpenStream)
				Assert.Fail("Could not open stream.");
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			if (!isReceived)
				Assert.Fail("Could not recieve data.");
			if (!isClose)
				Assert.Fail("Could not close stream.");

		}

		[STAThread]
		[Test]
		public void ManyConnectionOpenAndLargeTextDataReceivedSuccessfull()
		{
			int numberSessions = Convert.ToInt32(CountSession);
			int numberStreams = Convert.ToInt32(CountStream);

			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			_sessions = new List<SMSession>(numberSessions);
			string fileName = LargeFileName;
			bool isClose = false;
			for (int i = 0; i < numberSessions; i++)
			{
				eventRaised.Reset();
				SMSession newSession = CreateSession(uri);
				bool isOpenSession = false;
				newSession.OnOpen += (s, e) =>
										{
											isOpenSession = true;
											eventRaised.Set();
										};

				bool isError = false;
				newSession.OnError += (s, e) =>
										{
											isError = true;
											eventRaised.Set();
										};
				newSession.OnStreamOpened += (s, e) => eventRaised.Set();
				eventRaised.WaitOne();

				if (isError)
					Assert.Fail("Session error.");

				if (!isOpenSession)
					Assert.Fail("Could not open session.");

				eventRaised.Reset();

				for (int j = 0; j < numberStreams; j++)
				{
					eventRaised.Reset();
					bool isReceived = false;
					bool isFin = true;
					SMStream stream = DownloadPathForList(fileName, ref newSession, isFin);

					eventRaised.WaitOne();
					if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");

					stream.OnRSTReceived += (s, e) => eventRaised.Set();
					stream.OnDataReceived += (s, e) =>
												{
													string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

													using (var fs = new FileStream(file, FileMode.Create))
													{
														fs.Write(e.Data.Data, 0, e.Data.Data.Length);
													}

													string text = e.Data.AsUtf8Text();
													isReceived = !string.IsNullOrEmpty(text);
												};
					stream.OnClose += (s, e) =>
										{
											isClose = true;
											eventRaised.Set();
										};
					eventRaised.Reset();
					eventRaised.WaitOne();

					if ((stream.State == SMStreamState.Closed) ^ isFin)
						Assert.Fail("Incorrect SMStreamState.");
					if (!isReceived)
						Assert.Fail("Could not recieve data.");
					if (!isClose)
						Assert.Fail("Could not close session.");
				}
				_sessions.Add(newSession);
			}
			
		}

		[STAThread]
		[Test]
		public void ConnectionOpenAndCheckControlFrameSuccessfull()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			ManualResetEvent eventRaisedSmProtocol = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);

			CreateSession(uri);
			bool isOpenSession = false;
			bool isError = false;

			string errorMessage = String.Empty;
			_session.OnOpen += (s, e) =>
								{
									isOpenSession = true;
									eventRaised.Set();
								};
			_session.OnError += (s, e) =>
									{
										isError = true;
										errorMessage = "Internal session error.";
										eventRaised.Set();
									};
			_session.OnStreamOpened += (s, e) => eventRaised.Set();

			SMProtocol smProtocol = _session.Protocol;

			#region Protocol events

			smProtocol.OnError += (s, e) =>
									{
										eventRaisedSmProtocol.Reset();
										isError = true;
										eventRaisedSmProtocol.Set();
										eventRaised.Set();
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
												eventRaisedSmProtocol.Reset();
												isError = true;
												errorMessage = "Internal stream error.";
												eventRaisedSmProtocol.Set();
												eventRaised.Set();
											};
			smProtocol.OnStreamFrame += (s, e) =>
											{
												eventRaisedSmProtocol.Reset();

												if (e is HeadersEventArgs)
												{
													HeadersEventArgs headersEventArgs = e as HeadersEventArgs;

													if (headersEventArgs.Headers.Count == 0)
													{
														isError = true;
														errorMessage = "Incorrect value Frame.Headers.Count.";
														eventRaised.Set();
													}
												}
												else if (e is StreamDataEventArgs)
												{
													StreamDataEventArgs streamDataEventArgs = e as StreamDataEventArgs;

													if (streamDataEventArgs.Data.Data.Length == 0)
													{
														isError = true;
														errorMessage = "Incorrect Data.Length.";
														eventRaised.Set();
													}
												}
												else if (e is RSTEventArgs)
												{
													RSTEventArgs rstEventArgs = e as RSTEventArgs;

													if (rstEventArgs.Reason != StatusCode.Cancel)
													{
														isError = true;
														errorMessage = "Incorrect reason in RST frame.";
														eventRaised.Set();
													}
												}

												eventRaisedSmProtocol.Set();
											};
			#endregion

			eventRaised.WaitOne();
			if (!isOpenSession)
				Assert.Fail("Could not open session.");

			string fileName = FileName;
			eventRaised.Reset();
			bool isFin = true;
			bool isRecieveData = false;
			SMStream stream = DownloadPath(fileName, isFin);

			eventRaised.WaitOne();
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			if (isError)
				Assert.Fail(errorMessage);

			stream.OnRSTReceived += (s, e) =>
			                        	{
			                        		isError = true;
			                        		errorMessage = e.Reason.ToString();
											eventRaised.Set();
										};
			stream.OnDataReceived += (s, e) =>
				{
					string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

					using (var fs = new FileStream(file, FileMode.Create))
					{
						fs.Write(e.Data.Data, 0, e.Data.Data.Length);
					}
					string text = e.Data.AsUtf8Text();
					isRecieveData = !string.IsNullOrEmpty(text);
				};

			bool isClose = false;
			stream.OnClose += (s, e) =>
								{
									isClose = true;
									eventRaised.Set();
								};
			eventRaised.Reset();
			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect value Stream.State.");
			if (!isRecieveData)
				Assert.Fail("Could not recieve data.");
			if (!isClose)
				Assert.Fail("Could not close stream.");

			eventRaisedSmProtocol.WaitOne();

			Assert.Pass();
		}

		[STAThread]
		[Test]
		public void SendFrameOnCloseStreamFailure()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);

			bool isError = false;
			string errorMessage = String.Empty;

			bool isOpenSession = false;
			_session.OnOpen += (s, e) =>
								{
									isOpenSession = true;
									eventRaised.Set();
								};
			_session.OnError += (s, e) =>
									{
										isError = true;
										errorMessage = "Internal session error.";
										eventRaised.Set();
									};
			_session.OnStreamOpened += (s, e) => eventRaised.Set();

			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if (!isOpenSession)
				Assert.Fail("Could not open session.");

			eventRaised.Reset();

			string fileName = FileName;
			bool isReceived = false;
			bool isFin = true;

			SMStream stream = DownloadPath(fileName, isFin);

			eventRaised.WaitOne();
			eventRaised.Reset();
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");

			stream.OnRSTReceived += (s, e) => eventRaised.Set();
			stream.OnDataReceived += (s, e) =>
										{
											string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

											using (var fs = new FileStream(file, FileMode.Create))
											{
												fs.Write(e.Data.Data, 0, e.Data.Data.Length);
											}

											string text = e.Data.AsUtf8Text();
											isReceived = !string.IsNullOrEmpty(text);
										};

			bool isClose = false;
			stream.OnClose += (s, e) =>
								{
									isClose = true;
									eventRaised.Set();
								};

			_session.OnError += (s, e) =>
									{
										isError = true;
										eventRaised.Set();
									};
			eventRaised.Reset();
			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if ((stream.State == SMStreamState.Closed) ^ isFin)
				Assert.Fail("Incorrect value Stream.State.");
			if (!isClose)
				Assert.Fail("Could not close stream.");

			isError = false;
			SMData data = new SMData(new byte[] { 12, 3, 23, 35, 3, 11 });
			eventRaised.Reset();

			stream.SendData(data, isFin);
			eventRaised.WaitOne();

			Assert.IsTrue(isError);
		}

		[STAThread]
		[Test]
		public void IncorrectFileNameFailure()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Uri uri;
			Uri.TryCreate(Host + Port, UriKind.Absolute, out uri);
			CreateSession(uri);

			bool isOpenSession = false;
			bool isError = false;
			string errorMessage = String.Empty;

			_session.OnOpen += (s, e) =>
								{
									isOpenSession = true;
									eventRaised.Set();
								};
			_session.OnError += (s, e) =>
									{
										isError = true;
										errorMessage = "Internal session error.";
										eventRaised.Set();
									};
			_session.OnStreamOpened += (s, e) => eventRaised.Set();

			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if (!isOpenSession)
				Assert.Fail("Could not open session.");

			eventRaised.Reset();
			Guid guid = Guid.NewGuid();
			string fileName = guid +  FileName;
			bool isReceived = false;
			bool isFin = false;
			SMStream stream = DownloadPath(fileName, isFin);

			eventRaised.WaitOne();
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect SMStreamState.");
			StatusCode reason = StatusCode.Success;
			stream.OnRSTReceived += (s, e) =>
			                        	{
			                        		isError = true;
			                        		reason = e.Reason;
			                        		errorMessage = "Recieve RST frame. Reason: " +  e.Reason;
			                        		eventRaised.Set();
			                        	};
			stream.OnDataReceived += (s, e) =>
			{
				string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

				using (var fs = new FileStream(file, FileMode.Create))
				{
					fs.Write(e.Data.Data, 0, e.Data.Data.Length);
				}

				string text = e.Data.AsUtf8Text();
				isReceived = !string.IsNullOrEmpty(text);
				eventRaised.Set();
			};

			bool isClose = false;
			stream.OnClose += (s, e) =>
			{
				isClose = true;
				eventRaised.Set();
			};

			eventRaised.WaitOne();

			if (isError)
				Assert.Fail(errorMessage);
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect value Stream.State.");

			eventRaised.Reset();
			eventRaised.WaitOne();

			if (isError)
			{
				if (reason == StatusCode.InternalError)
					Assert.Pass();
				Assert.Fail(errorMessage);
			}
			if ((stream.State == SMStreamState.HalfClosed) ^ isFin)
				Assert.Fail("Incorrect value Stream.State.");
			if (!isClose)
				Assert.Fail("Could not close session.");
		}

		[STAThread]
		[Test]
		public void IncorrectAddressFailure()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Random rand = new Random(DateTime.Now.Millisecond);
			int random = rand.Next() % 1000;
			Uri uri;
			Uri.TryCreate(String.Format("ws://{0}:{1}", random, Port), UriKind.Absolute, out uri);

			if (_session != null && _session.State == SMSessionState.Opened)
				_session.End();


			bool isError = false;
			_session = new SMSession(uri, false);

			_session.OnError += (s, e) =>
									{
										isError = true;
										eventRaised.Set();
									};

			SMProtocol smProtocol = _session.Protocol;
			smProtocol.OnError += (s, e) =>
									{
										isError = true;
										eventRaised.Set();
									};

			_session.Open();
			eventRaised.WaitOne();
			Assert.IsTrue(isError);
		}

		[STAThread]
		[Test]
		public void IncorrectPortFailure()
		{
			ManualResetEvent eventRaised = new ManualResetEvent(false);
			Random rand = new Random(DateTime.Now.Millisecond);
			int random = rand.Next() % 1000;
			Uri uri;
			Uri.TryCreate(Host + random, UriKind.Absolute, out uri);

			if (_session != null && _session.State == SMSessionState.Opened)
				_session.End();

			_session = new SMSession(uri, false);
			bool isError = false;
			_session.OnError += (s, e) =>
									{
										isError = true;
									};

			SMProtocol smProtocol = _session.Protocol;
			smProtocol.OnError += (s, e) =>
			{
				isError = true;
				eventRaised.Set();
			};

			_session.Open();
			eventRaised.WaitOne();

			Assert.IsTrue(isError);
		}

		#endregion
	}
}
