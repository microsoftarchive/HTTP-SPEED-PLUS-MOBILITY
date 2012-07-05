//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Client
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel.SMProtocol;
	using System.Threading;
	using Client.Benchmark;
	using Client.Http;
	using Client.Utils;

	/// <summary>
	/// Main client class
	/// </summary>   
	public class Program
	{
        /// <summary>
        /// Server port for unencrypted S+M protocol
        /// </summary>
        private const int WSPORT = 8080;

        /// <summary>
        /// Server port for encrypted S+M protocol
        /// </summary>
        private const int WSSPORT = 8081;

		/// <summary>
		/// Internal command queue
		/// </summary>
		private static readonly Queue Parameters = new Queue();

		/// <summary>
		/// Internal session object
		/// </summary>
		private static SMSession session;

		/// <summary>
		/// Session monitor
		/// </summary>
		private static ProtocolMonitor protocolMonitor;

        /// <summary>
        /// serialize access to open stream collection
        /// </summary>
        [NonSerialized]
        private static object exclusiveLock = new object();

        /// <summary>
        /// event for stream monitor
        /// </summary>
        private static ManualResetEvent streamMonitorEvent = new ManualResetEvent(false);

        /// <summary>
        /// event for root file download
        /// </summary>
        private static ManualResetEvent rootDownloadEvent = new ManualResetEvent(false);

        /// <summary>
        /// Root file name for GET/DIR command
        /// </summary>
        private static string rootFileName;

        /// <summary>
        /// Timeout flag for stream monitor
        /// </summary>
        private static bool timeoutStreamMonitor = false;

        /// <summary>
        /// List of opened streams
        /// </summary>
        private static PriorityList currentStreams = new PriorityList();

        /// <summary>
        /// Timeout flag for session monitor
        /// </summary>
        private static bool timeoutSessionMonitor = false;

        /// <summary>
        /// event for session monitor
        /// </summary>
        private static ManualResetEvent sessionMonitorEvent = new ManualResetEvent(false);

        /// <summary>
        /// list of current arguments
        /// </summary>
        private static IEnumerable<string> currentArgs;

        /// <summary>
        /// Index of current argument
        /// </summary>
        private static int argsIndex;

		/// <summary>
		/// Index of current statistics slot
		/// </summary>
		private static int slotId = 0;

        /// <summary>
        /// Application executed in script mode
        /// </summary>
        private static bool appScriptMode = false;

		/// <summary>
		/// Server-side latency on firewall
		/// </summary>
		private static int serverLatency = 0;

		/// <summary>
		/// Client entry point
		/// </summary>
		/// <param name="args">Arguments to Main</param>
		/// <returns>The return code.</returns>
		public static int Main(string[] args)
		{
			SMLogger.LoggerLevel = SMLoggerState.VerboseLogging;
			NativeMethods.EnableQuickEditMode();

			int res = args.Length == 0 ? ExecuteCommandLoop() : ParseAndExec(args);

			return res;
		}

        /// <summary>
        /// Command line 
        /// </summary>
        /// <param name="token">the token</param>
        /// <returns>Negative for non-command, otherwise maximum number of parameters</returns>
        private static int IsCommand(string token)
        {
            switch (token.ToUpper())
            {
                case "CLOSE":
                case "DIR":
                case "DUMP-STATS":
                case "EXIT":
                    return 0;

                case "CAPTURE-STATS":
                case "GET":
                case "HELP":
                case "RUN":
                case "VERBOSE":
                    return 1;

                case "HTTPGET":
                case "CONNECT":
                    return 3;

                default:
                    return -1;
            }
        }

        /// <summary>
        /// Command line get next token
        /// </summary>
        /// <returns>Returns next token</returns>
        private static string GetNextToken()
        {
            if (argsIndex >= currentArgs.Count())
            {
                return string.Empty;
            }

            // all tokens are lower case to match file names for case-sensitive filesystems
            string ret = currentArgs.ElementAt(argsIndex).ToLower();
            argsIndex++;
            return ret;
        }

        /// <summary>
        /// Command line put token back
        /// </summary>
        private static void PutBackToken()
        {
            if (argsIndex == 0)
            {
                return;
            }

            argsIndex--;
        }

		/// <summary>
		/// Command line parser and executer
		/// </summary>
		/// <param name="args">The command list</param>
		/// <returns>The return code, 0 for success, 1 for error.</returns>
		private static int ParseAndExec(IEnumerable<string> args)
		{
            // switch to script mode
            appScriptMode = true;

            currentArgs = args;
            argsIndex = 0;

			int res = 0;
			string cmd = string.Empty;

            bool done = false;
            while (done == false)
            {
                string tok = GetNextToken();
                if (tok == string.Empty)
                {
                    break;
                }

                int paramNumber = IsCommand(tok);
                if (paramNumber < 0)
                {
                    SMLogger.LogError("Unknown command " + tok);
                    return 1;
                }

                cmd = tok;
                string paramString = string.Empty;
                while (paramNumber != 0)
                {
                    tok = GetNextToken();
                    if (tok == string.Empty)
                    {
                        done = true;
                        break;
                    }

                    if (IsCommand(tok) >= 0)
                    {
                        // HELP command is legal, for everything else we find next command
                        if (cmd.ToUpper() == "HELP")
                        {
                            // add to list of parameters for current command
                            if (paramString != string.Empty)
                            {
                                paramString += " ";
                            }

                            paramString += tok;
                            paramNumber--;
                        }
                        else
                        {
                            PutBackToken();
                            paramNumber = 0;
                        }
                    }
                    else
                    {
                        // add to list of parameters for current command
                        if (paramString != string.Empty)
                        {
                            paramString += " ";
                        }

                        paramString += tok;
                        paramNumber--;
                    }
                }

                ArgPair argpair = new ArgPair(cmd, paramString);
                Parameters.Enqueue(argpair);
            }

			while ((Parameters.Count != 0) && (res == 0))
			{
				ArgPair ap = (ArgPair)Parameters.Dequeue();
				res = ExecuteOneCommand(ap.Cmd, ap.Value);
                if (res == 0)
                {
                    Thread.Sleep(400);
                }
			}

			return res;
		}

		/// <summary>
		/// One command executer.
		/// </summary>
		/// <param name="cmd">The command</param>
		/// <param name="val">The value</param>
		/// <returns>The result code, 0 for success, 1 for termination.</returns>
		private static int ExecuteOneCommand(string cmd, string val)
		{
			SMLogger.LogConsole("Executing " + cmd + " " + val);
			int res = 0;
			switch (cmd.ToUpper())
			{
				case "HELP":
                    if (val == string.Empty)
                    {
                        DisplayHelp();
                    }
                    else
                    {
                        DisplayDetailedHelp(val);
                    }

					break;
				case "EXIT":
					return 1;
				case "VERBOSE":
					VerboseMode(val);
					break;
				case "CONNECT":
					res = OpenSession(val);
					break;
				case "GET":
					if (val == string.Empty)
					{
						SMLogger.LogError("GET needs file name.");
						break;
					}

					DownloadRootFile(val);
					break;
				case "DIR":
    				DisplayFilesListing();
					break;
				case "CAPTURE-STATS":
					MonitoringControl(ProtocolMonitor.StringToState(val));
					break;
				case "DUMP-STATS":
					GetMonitoringStats();
					break;
				case "CLOSE":
					CloseSession();
					break;
				case "HTTPGET":
					HttpGetFile(val);
					break;
				case "RUN":
					RunScriptFile(val);
					break;
				default:
					SMLogger.LogError("Unknown command " + cmd);
					break;
			}

			return 0;
		}

		/// <summary>
		/// Command loop executer.
		/// </summary>
		/// <returns>The result code. 0 for success, 1 for error.</returns>
		private static int ExecuteCommandLoop()
		{
			DisplayHelp();
			SMLogger.LoggerConsole = true;
			SMLogger.LogConsole("Please type a command:\n");
			while (true)
			{
                Console.Write(">");
                string strcommands = Console.ReadLine();
				string[] commands;

				if (!string.IsNullOrEmpty(strcommands))
				{
					commands = strcommands.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				}
				else
				{
					continue;
				}

				string command = commands.FirstOrDefault();
				string value = string.Empty;
				switch (commands.Length)
				{
					case 2:
						value = commands[1];
						break;
					case 3:
						value = string.Format("{0} {1}", commands[1], commands[2]);
						break;
					case 4:
						value = string.Format("{0} {1} {2}", commands[1], commands[2], commands[3]);
						break;
				}

				if (ExecuteOneCommand(command, value) == 1)
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Benchmark for client.
		/// </summary>
		/// <param name="state">The command [On|Off|Reset]</param>
		private static void MonitoringControl(MonitorState state)
		{
			switch (state)
			{
				case MonitorState.MonitorOn:
					if (session != null && session.State == SMSessionState.Opened)
					{
						if (protocolMonitor != null)
						{
							protocolMonitor.Dispose();
						}

						protocolMonitor = new ProtocolMonitor(session);
						protocolMonitor.Attach();
					}
					else
					{
						SMLogger.LogError("Session was closed due to error or not opened. Use CONNECT <Uri> to open a new session.");
					}

					break;

				case MonitorState.MonitorOff:
					if (protocolMonitor != null)
					{
						protocolMonitor.Dispose();
						protocolMonitor = null;
					}

					break;

				case MonitorState.MonitorReset:
					if (protocolMonitor != null)
					{
						protocolMonitor.Reset();
					}

					break;

				default:
					SMLogger.LogError("CAPTURE-STATS needs [On|Off|Reset].");
					break;
			}
		}

		/// <summary>
		/// Gets the monitoring stats.
		/// </summary>
		private static void GetMonitoringStats()
		{
			if (protocolMonitor != null)
			{
				string output = protocolMonitor.GetMonitoringStats(SMLogger.LoggerLevel);

				if (SMLogger.LoggerLevel < SMLoggerState.VerboseLogging)
				{
					// verbose level is too low to output as INFO
					// if we trigger DUMP-STATS from script we still want to see the results
					SMLogger.LoggerConsole = true;
					SMLogger.LogConsole("\r\n" + output);
					SMLogger.LoggerConsole = false;
				}
				else
				{
					SMLogger.LogInfo("\r\n" + output);
				}
			}
			else
			{
				if (session == null || session.State != SMSessionState.Opened)
				{
					SMLogger.LogError("Session was closed due to error or not opened. Use CONNECT <Uri> to open a new session.");
				}
				else
				{
					SMLogger.LogError("Please use \"CAPTURE-STATS On\" to start monitoring.");
				}
			}
		}

		/// <summary>
		/// saves the monitoring stats.
		/// </summary>
		private static void SaveStats()
		{
			slotId++;

			if (protocolMonitor == null)
			{
				if (session == null || session.State != SMSessionState.Opened)
				{
					SMLogger.LogError("Session was closed due to error or not opened. Use CONNECT <Uri> to open a new session.");
				}
				else
				{
					SMLogger.LogError("Please use \"CAPTURE-STATS On\" to start monitoring.");
				}

				return;
			}

			if (!protocolMonitor.SaveSlot(slotId))
			{
				SMLogger.LogError("Unable to save statistics into slot " + slotId);
			}
		}

        /// <summary>
        /// Help for client.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("HTTP Speed+Mobility Prototype Client help\n");
            Console.WriteLine("HELP                          Display this help information");
            Console.WriteLine("HELP command                  Display detailed help for command\n" + 
                              "                              Ex. HELP CONNECT");
			Console.WriteLine("CONNECT [C|S] [QUANTUM] <URI> Connect to the server endpoint.\n" +
							  "                              If no URI is specified,\n" +
							  "                              defaults to ws://smserver.cloudapp.net:8080.\r\n" +
							  "                              Use C flag to enable compression.\r\n" +
							  "                              S flag enables stateful compression.");
			Console.WriteLine("DIR                           List files on server.");
            Console.WriteLine("GET    \"filename\"             Download web page and associated resources.");
            Console.WriteLine("VERBOSE   [1|2|3]             Display verbose output.");
            Console.WriteLine("CAPTURE-STATS [On|Off|Reset]  Start/stop/reset protocol monitoring.");
            Console.WriteLine("DUMP-STATS                    Display statistics captured using CAPTURE-STATS.");
            Console.WriteLine("HTTPGET \"filename\"            Download file using HTTP 1.1.");
            Console.WriteLine("CLOSE                         Close session");
            Console.WriteLine("RUN  \"filename\"               Run command script");
            Console.WriteLine("EXIT                          Exit application");
            Console.WriteLine();
        }

		/// <summary>
		/// Help for client.
		/// </summary>
        /// <param name="val">Command name for HELP</param>
        private static void DisplayDetailedHelp(string val)
		{
            Console.WriteLine("HTTP Speed+Mobility Prototype Client help\n");
            switch (val.ToUpper())
            {
                case "CONNECT":
					Console.WriteLine("CONNECT [C|S] [QUANTUM] URL        Connect to the server endpoint.\n");
                    Console.WriteLine("  Establish WebSocket connection with Speed+Mobility extension.");
                    Console.WriteLine("  URL should be in the form ws://hostname:port or wss://hostname:port");
                    Console.WriteLine("  Default value for URL is Azure endpoint ws://locahost:{0}", WSPORT.ToString());
                    Console.WriteLine("    ws:// opens unencrypted connection.");
                    Console.WriteLine("    wss:// opens encrypted connection.");
                    Console.WriteLine("  You can enable zLib compression of headers by passing C or S flag.");
                    Console.WriteLine("    \"C\" enables static dictionary compression as defined in SPDY proposal");
                    Console.WriteLine("    \"S\" enables stateful compression.");
                    Console.WriteLine("    Default option is no compression.");
                    Console.WriteLine("    QUANTUM is an number of ms for control flow.");
                    Console.WriteLine("    CONNECT closes current session before creating new one.\n");
                    Console.WriteLine("  Examples of CONNECT:\n");
                    Console.WriteLine("  CONNECT");
                    Console.WriteLine("      open unencrypted connection to default end point, no compression. Same as");
                    Console.WriteLine("      CONNECT ws://smserver.cloudapp.net:8080");
                    Console.WriteLine("  CONNECT C wss://locahost:8081");
                    Console.WriteLine("      open encrypted connection with compression to local server on port 8081.");
                    Console.WriteLine("  CONNECT S ws://smserver.cloudapp.net:8080");
                    Console.WriteLine("      open unencrypted connection with stateful compression to Azure endpoint.");
                    Console.WriteLine("  CONNECT S");
                    Console.WriteLine("      same as above, default value for URL is Azure endpoint.");
                    Console.WriteLine("  CONNECT wss://smserver.cloudapp.net:8081");
                    Console.WriteLine("      open encrypted connection without compression to Azume endpoint.");
                    Console.WriteLine("\n");
                    Console.WriteLine("Note: Endpoint for encrypted connection runs on port 8081");
                    Console.WriteLine("      Endpoint for unencrypted connection runs on port 8080");
                    Console.WriteLine("\n");
                    break;

                case "DIR":
			        Console.WriteLine("DIR   Lists files on server available for download.\n");
                    Console.WriteLine("  This command does not have any arguments.");
                    Console.WriteLine("  This command does not list all the files, only download targets.");
                    Console.WriteLine("  Download targets are either text files, or top level HTML files.");
                    Console.WriteLine("  When you apply GET ot HTTPGET to download target, all associated files");
                    Console.WriteLine("  are also downloaded.");
                    Console.WriteLine("\n");
                    Console.WriteLine("Note: You can still download specific associated file by specifying exact path.");
                    Console.WriteLine("       DIR requests file \"index\" which is created on demand.");
                    Console.WriteLine("\n");
                    break;

                case "GET":
                    Console.WriteLine("GET <filename>       Download web page and associated resources.\n");
                    Console.WriteLine("  <filename> should be path to web page relative to server web root.");
                    Console.WriteLine("  Locally downloaded files are stored in directory relative to current.");
                    Console.WriteLine("  Directory structure is preserved.");
                    Console.WriteLine("  Download is done using S+M protocol.");
                    Console.WriteLine("  You can get list of files with command DIR.\n");
                    Console.WriteLine("  Examples of GET:\n");
                    Console.WriteLine("  GET /microsoft/default.htm");
                    Console.WriteLine("     download web page and all associated resources to local directory .\\microsoft\\");
                    Console.WriteLine("  GET microsoft/default_files/style.css");
                    Console.WriteLine("     download just style.css to local directory .\\microsoft\\default_files\\");
                    Console.WriteLine("\n");
                    break;

                case "VERBOSE":
			        Console.WriteLine("VERBOSE [0-3]         Display verbose output.\n");
                    Console.WriteLine("  Change level of verbosity in command output.");
                    Console.WriteLine("  Number argument in range 0-3 is required.");
                    Console.WriteLine("    0   - no message level of verbosity. Nothing is displayed.");
                    Console.WriteLine("    1   - error level. Only errors are displayed.");
                    Console.WriteLine("    2   - info level. Errors and information messages are displayed.");
                    Console.WriteLine("    3   - debug level. Everything is displayed.");
                    Console.WriteLine("  Examples of VERBOSE:\n");
                    Console.WriteLine("  VERBOSE 2");
                    Console.WriteLine("    Enable errors and infomation messages.");
                    Console.WriteLine("\n");
                    break;

                case "CAPTURE-STATS":
                    Console.WriteLine("CAPTURE-STATS [On|Off|Reset]  Start/stop/reset protocol monitoring.\n");
                    Console.WriteLine("  Controls log of protocol activities. Use DUMP-STATS to see log data.");
                    Console.WriteLine("  Argument of [On|Off|Reset] is required.");
                    Console.WriteLine("    On    - Start logging.");
                    Console.WriteLine("    Off   - Stop logging and clean up results.");
                    Console.WriteLine("    Reset - Clean up current log results, don't stop logging.");
                    Console.WriteLine("  Examples of CAPTURE-STATS:\n");
                    Console.WriteLine("  CAPTURE-STATS reset");
                    Console.WriteLine("    Clean up results in current session.");
                    Console.WriteLine("\n");
                    break;

                case "DUMP-STATS":
                    Console.WriteLine("DUMP-STATS   Display statistics captured using CAPTURE-STATS.\n");
                    Console.WriteLine("  This command does not have any arguments.");
                    Console.WriteLine("\n");
                    break;

                case "HTTPGET":
                    Console.WriteLine("HTTPGET <latency> <filename>       Download web page using HTTP 1.1\n");
                    Console.WriteLine("  Latency is server latency in ms. Default value is zero.");
                    Console.WriteLine("  This command can take full URL path or relative to web root path.");
                    Console.WriteLine("  If there exists open session, relative path is assumed to refer to");
                    Console.WriteLine("  current web root. If no session is opened, user should supply full");
                    Console.WriteLine("  URL for the file.");
                    Console.WriteLine("  Locally downloaded files are stored in directory relative to current.");
                    Console.WriteLine("  Directory structure is preserved.");
                    Console.WriteLine("  Download is done using 6 concurrent threads to simulate IE.");
                    Console.WriteLine("  You can get list of files with command DIR.\n");
                    Console.WriteLine("  Examples of HTTPGET:\n");
                    Console.WriteLine("  HTTPGET /microsoft/default.htm");
                    Console.WriteLine("     download web page and all associated resources to local directory .\\microsoft\\");
                    Console.WriteLine("  HTTPGET http://localhost:8080/microsoft/default_files/style.css");
                    Console.WriteLine("     download just style.css to local directory .\\microsoft\\default_files\\");
                    Console.WriteLine("\n");
                    break;

                case "CLOSE":
                    Console.WriteLine("CLOSE         Close current S+M session\n");
                    Console.WriteLine("  Close session and stop logging. Use DUMP-STATS before closing session to");
                    Console.WriteLine("  to get log data");
                    Console.WriteLine("\n");
                    break;

                case "RUN":
                    Console.WriteLine("RUN  <filename>        Run script file\n");
                    Console.WriteLine("  <filename> should be local file with set of commands to execute.");
                    Console.WriteLine("  Commands are delimited by EOL, symbol '#' in the first column");
                    Console.WriteLine("  indicates comment line. White space lines are ignored.");
                    Console.WriteLine("  RUN executes commands in sequence and terminates application at");
                    Console.WriteLine("  the end.");
                    Console.WriteLine("  Example of RUN script:\n");
                    Console.WriteLine("#---------start----------------");
                    Console.WriteLine("# Set verbose output to Info");
                    Console.WriteLine("VERBOSE 2");
                    Console.WriteLine("# Connect to default endpoint (ws://smserver.cloudapp.net:8080)");
                    Console.WriteLine("CONNECT");
                    Console.WriteLine("# Get file with Speed+Mobility protocol");
                    Console.WriteLine("GET /files/test.txt");
                    Console.WriteLine("# Get file with HTTP1.1");
                    Console.WriteLine("HTTPGET /files/test.txt");
                    Console.WriteLine("# Display statistics side by side");
                    Console.WriteLine("DUMP-STATS");
                    Console.WriteLine("#---------end------------------");
                    Console.WriteLine("\n");
                    break;

                case "EXIT":
                    Console.WriteLine("EXIT   Exit application\n");
                    Console.WriteLine("  EXIT does not close current session.");
                    Console.WriteLine("\n");
                    break;

                case "HELP":
                    Console.WriteLine("HELP   Displays help\n");
                    Console.WriteLine("  HELP without arguments displays list of command with short description.");
                    Console.WriteLine("  HELP COMMAND displays detailed help for COMMAND.");
                    Console.WriteLine("\n");
                    break;

                default:
                    Console.WriteLine("HELP {0}  : Unknown command.", val);
                    break;
            }
		}

		/// <summary>
		/// Displays the files listing on the server.
		/// </summary>
		private static void DisplayFilesListing()
		{
			if (session == null)
			{
				SMLogger.LogError("Not connected to server. Please use CONNECT to connect to server first.");
			}
			else
			{
				switch (session.State)
				{
					case SMSessionState.Opened:

                        // event will be set in OnDataReceived event handler
                        rootDownloadEvent.Reset();
                        rootFileName = "index";

						SMHeaders headers = new SMHeaders();
						headers[SMHeaders.Path] = "index";
						headers[SMHeaders.Version] = "http/1.1";
						headers[SMHeaders.Method] = "GET";
						headers[SMHeaders.Scheme] = "http";
						headers[SMHeaders.ContentType] = ContentTypes.TextPlain;
						SMStream stream = session.OpenStream(headers, true);
						stream.OnDataReceived += OnDataReceived;

                        // Wait till we get OnDataReceived
                        if (false == rootDownloadEvent.WaitOne(30000))
                        {
                            SMLogger.LogError("Timeout on DIR");
                        }

						break;
					case SMSessionState.Closed:
						SMLogger.LogError("Session was closed due to error or not opened. Use CONNECT <Uri> to open new the session.");
						break;

					default:
						SMLogger.LogError("Unknown SMSessionState");
						break;
				}
			}
		}

		/// <summary>
		/// Close SM session.
		/// </summary>
		private static void CloseSession()
		{
			if (session != null && session.State == SMSessionState.Opened)
			{
                // wait until session changes its state
                // for high latency connections we cannot assume this session closes synchronosly
                sessionMonitorEvent.Reset();
                timeoutSessionMonitor = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(SessionMonitorProc), 0);

				session.End();

                sessionMonitorEvent.WaitOne(30000);
                timeoutSessionMonitor = true;
            }
		}

		/// <summary>
		/// Open SM session.
		/// </summary>
		/// <param name="val">Parameters for session</param>
		/// <returns>The return code.</returns>
		private static int OpenSession(string val)
		{
			int res = 0;
			uint result;
			string[] args = val.Split(' ');
			string url = string.Empty;
			string options = string.Empty;
			string quantum = string.Empty;

			if (args.Length > 2)
			{
				url = args[2];
				quantum = args[1];
				options = args[0];
			}
			else if (args.Length > 1)
			{
				url = args[1];
				if (args[0] == "c" || args[0] == "s")
				{
					options = args[0];
				}
				else
				{
					quantum = args[0];
				}
			}
			else if (args.Length > 0)
			{
				url = args[0];
			}

            if ((url == "c" || url == "s") && args.Length == 1)
            {
                // user passed 'c' or 's' without URL
                // lets connect to non-encrypted WinSocket on default server
                options = url;
                url = "ws://smserver.cloudapp.net:" + WSPORT.ToString();
            }

			if (options != string.Empty && options != "c" && options != "s")
			{
				SMLogger.LogError(options + ": Compression option should be 'c' or 's'.");
				return 1;
			}

			// verify quantum is a positive integer or empty string
			if (quantum != string.Empty)
			{
				result = 0;
				try
				{
					result = Convert.ToUInt32(quantum);
				}
				catch
				{
					res = 1;
				}

				if (res == 1)
				{
					SMLogger.LogError(quantum + ": Credit update quantum should be a positive integer.");
					return 1;
				}
			}

			if (string.IsNullOrEmpty(url))
			{
                // user passed nothing
                // lets connect to non-compressed non-encrypted WinSocket on default server
                url = "ws://smserver.cloudapp.net:" + WSPORT.ToString();
			}

			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
                SMLogger.LogError(" [" + url + "] Uri is not in correct format.");
                return 1;
			}

            if (uri.Port == -1)
            {
                // user did not specify the port
                if (url.Substring(0, 6) == "wss://")
                {
                    url = url + ":" + WSSPORT.ToString();
                }
                else if (url.Substring(0, 5) == "ws://")
                {
                    url = url + ":" + WSPORT.ToString();
                }

                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    SMLogger.LogError(" [" + url + "] Uri is not in correct format.");
                    return 1;
                }
            }

			return CreateSession(uri, new SMProtocolOptions(options, quantum));
		}

		/// <summary>
		/// Create SM session. Close if previously opened.
		/// </summary>
		/// <param name="val">The verbose switch</param>
		private static void VerboseMode(string val)
		{
			if (val == string.Empty)
			{
				SMLogger.LogError("VERBOSE needs level [1|2|3].");
				return;
			}

			// process potential garbage from input
			try
			{
				SMLogger.LoggerLevel = (SMLoggerState)int.Parse(val);
			}
			catch
			{
				SMLogger.LoggerLevel = SMLoggerState.ErrorsOnly;
			}

			if ((SMLogger.LoggerLevel < SMLoggerState.NoLogging) || (SMLogger.LoggerLevel > SMLoggerState.MaxLogging))
			{
				SMLogger.LogError("VERBOSE needs level [1|2|3].");
				SMLogger.LoggerLevel = SMLoggerState.ErrorsOnly;
			}

			SMLogger.LogConsole("VERBOSE output " + SMLogger.LoggerLevel);
		}

		/// <summary>
		/// Create SM session. Close if previously opened.
		/// </summary>
		/// <param name="uri">The URI</param>
		/// <param name="option">Options for session</param>
		/// <returns>The return code.</returns>
		private static int CreateSession(Uri uri, SMProtocolOptions option)
		{
			int res = 0;
			CloseSession();

			session = new SMSession(uri, false, option);

			// URI can still be invalid, missing protocol prefix for example
			try
			{
				session.Open();

				session.OnOpen += OnSessionOpened;
				session.OnClose += OnSessionClosed;
				session.OnError += OnSessionError;
				session.OnStreamOpened += OnStreamOpened;
                session.OnStreamClosed += OnStreamClosed;

                // wait until session changes its state
                // for high latency connections we cannot just start using this session
                sessionMonitorEvent.Reset();
                timeoutSessionMonitor = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(SessionMonitorProc), 0);
                sessionMonitorEvent.WaitOne(300000);
                timeoutSessionMonitor = true;
            }
			catch
			{
				SMLogger.LogError("Unable to open session for " + uri);
				res = 1;
			}

			return res;
		}

        /// <summary>
        /// Thread proc for session monitor
        /// </summary>
        /// <param name="stateInfo">state info for the thread</param>
        private static void SessionMonitorProc(object stateInfo)
        {
            // make sure we exit after timeout
            while (timeoutSessionMonitor == false)
            {
                if (session.State != SMSessionState.Created)
                {
                    if (session.State == SMSessionState.Opened)
                    {
                        // start monitoring for opened session
                        MonitoringControl(MonitorState.MonitorOn);
                    }

                    sessionMonitorEvent.Set();
                    return;
                }

                Thread.Sleep(100);
            }

            SMLogger.LogError("Timeout waiting for open/close session event");
        }

		/// <summary>
		/// File download. Called from command processor for top of the file tree
		/// </summary>
		/// <param name="fileName">The file name</param>
		private static void DownloadRootFile(string fileName)
		{
			if (protocolMonitor != null)
			{
				// set title of S+M dowload
				protocolMonitor.Totals.LogTitle = "S+M " + fileName;
                protocolMonitor.LastStartDate = DateTime.Now;

				// clear previous HTTP download
				protocolMonitor.LastHTTPLog = null;
			}

            // event will be set in OnDataReceived event handler
            rootDownloadEvent.Reset();
            rootFileName = fileName;

            // add first stream to list of streams
            PriorityRecord pr = new PriorityRecord(PriorityLevel.HighPriority, rootFileName);
            currentStreams.Add(pr);

            if (true == DownloadPath(fileName))
            {
                // wait until first stream downloaded and parsed
                if (false == rootDownloadEvent.WaitOne(30000))
                {
                    SMLogger.LogError("Timeout on GET " + fileName);
                }

                // if session state not "Opened" we got server side error and there is no connection
                if (session.State == SMSessionState.Opened)
                {
                    // wait until all streams are closed
                    timeoutStreamMonitor = false;
                    streamMonitorEvent.Reset();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(StreamListMonitorProc), 0);
                    streamMonitorEvent.WaitOne(30000);
                    timeoutStreamMonitor = true;
                }
            }
		}

        /// <summary>
        /// Thread proc for stream list monitor
        /// </summary>
        /// <param name="stateInfo">RequestPackage for the thread</param>
        private static void StreamListMonitorProc(object stateInfo)
        {
            while (timeoutStreamMonitor == false)
            {
                lock (exclusiveLock)
                {
                    // no more opened streams
                    if (currentStreams.CountOpenStreams() == 0)
                    {
                        SMLogger.LogDebug("Zero stream count reached");

                        // done downloading, save log data
                        if (protocolMonitor != null)
                        {
                            protocolMonitor.LastEndDate = DateTime.Now;
                        }

                        SaveStats();
                        streamMonitorEvent.Set();
                        return;
                    }
                }

                Thread.Sleep(100);
            }

            SMLogger.LogError("Timeout wating for all streams to close");
        }

		/// <summary>
		/// File download.
		/// </summary>
		/// <param name="fileName">The file name</param>
        /// <returns>The return code.</returns>
		private static bool DownloadPath(string fileName)
		{
			if (session == null)
			{
				SMLogger.LogError("Not connected to server. Please use CONNECT to connect to server first.");
                return false;
			}
			else
			{
				switch (session.State)
				{
					case SMSessionState.Created:
						SMLogger.LogError("Session was created but not opened yet.");
						break;

					case SMSessionState.Opened:
						SMHeaders headers = new SMHeaders();
						headers[SMHeaders.Path] = fileName;
						headers[SMHeaders.Version] = "http/1.1";
						headers[SMHeaders.Method] = "GET";
						headers[SMHeaders.Scheme] = "http";
						headers[SMHeaders.ContentType] = ContentTypes.GetTypeFromFileName(fileName);
						SMStream stream = session.OpenStream(headers, true);
						stream.OnDataReceived += OnDataReceived;
						stream.OnRSTReceived += OnRSTReceived;
						break;
					case SMSessionState.Closed:
						SMLogger.LogError("Session was closed due to error or not opened. Use CONNECT <Uri> to open new the session.");
                        return false;

					default:
						SMLogger.LogError("Unknown SMSessionState " + session.State);
						break;
				}

                return true;
			}
        }

        /// <summary>
        /// Create download list for contents of HTML document
        /// </summary>
        /// <param name="origdir">Original path</param>
        /// <param name="document">The document</param>
        private static void MakeDownloadList(string origdir, XHtmlDocument document)
        {
            foreach (var script in document.Scripts)
            {
                PriorityRecord pr = new PriorityRecord(PriorityLevel.HighPriority, string.Format("{0}/{1}", origdir.ToLower(), script.ToLower()));
                currentStreams.Add(pr);
            }

            foreach (var link in document.Links)
            {
                PriorityRecord pr = new PriorityRecord(PriorityLevel.MediumPriority, string.Format("{0}/{1}", origdir.ToLower(), link.ToLower()));
                currentStreams.Add(pr);
            }

            foreach (var image in document.Images)
            {
                PriorityRecord pr = new PriorityRecord(PriorityLevel.LowPriority, string.Format("{0}/{1}", origdir.ToLower(), image.ToLower()));
                currentStreams.Add(pr);
            }

            // sort by priority, name
            currentStreams.SortByPriority();
        }

        /// <summary>
        /// Event handler for download complete.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnDataReceived(object sender, StreamDataEventArgs e)
        {
            string file = Path.GetFileName(e.Stream.Headers[SMHeaders.Path]);

            string directory = Path.GetDirectoryName(e.Stream.Headers[SMHeaders.Path]);
			string newfilepath = string.Empty;

            // create local file path
			if (!string.IsNullOrEmpty(directory))
			{
				if (directory[0] == '\\')
				{
					directory = '.' + directory;
				}

				Directory.CreateDirectory(directory);
				newfilepath = directory + '\\' + file;
			}
			else
			{
				newfilepath = file;
			}

            // create root path for remote files
            int fileNameStart = e.Stream.Headers[SMHeaders.Path].LastIndexOf(file);
            string origdir = e.Stream.Headers[SMHeaders.Path].Substring(0, fileNameStart);

			using (var fs = new FileStream(newfilepath, FileMode.Create))
			{
				fs.Write(e.Data.Data, 0, e.Data.Data.Length);
			}

			SMLogger.LogConsole("File downloaded: " + file);

			switch (e.Stream.Headers[SMHeaders.ContentType])
			{
				case ContentTypes.TextHtml:
					{
						string text = e.Data.AsUtf8Text();

						XHtmlDocument document = XHtmlDocument.Parse(text);

                        MakeDownloadList(origdir, document);

						foreach (var image in document.Images)
						{
							DownloadPath(string.Format("{0}/{1}", origdir.ToLower(), image.ToLower()));
						}

						foreach (var link in document.Links)
						{
							DownloadPath(string.Format("{0}/{1}", origdir.ToLower(), link.ToLower()));
						}

                        foreach (var script in document.Scripts)
                        {
                            DownloadPath(string.Format("{0}/{1}", origdir.ToLower(), script.ToLower()));
                        }
                    }

					break;
				case ContentTypes.TextPlain:
					if (e.Stream.Headers[SMHeaders.Path].Equals("index"))
					{
						string text = e.Data.AsUtf8Text();
						Console.WriteLine();
						Console.WriteLine(text);
					}

					break;
			}

			if (e.Stream.State != SMStreamState.Closed)
			{
				// stream must be closed by this time since it was created with FIN flag.
				// if this is not the case - close the stream with error
				e.Stream.Close(StatusCode.Cancel);
			}

			if (protocolMonitor != null)
			{
				StreamInfo ss = protocolMonitor.StreamStatistics(e.Stream.StreamId);
				SMLogger.LogInfo("Bytes transferred:" +
					ss.UpCount + " upstream, " +
					ss.DownCount + " downstream, " +
					(ss.DownCount + ss.UpCount) + " Total");
			}

            if (rootFileName == e.Stream.Headers[SMHeaders.Path])
            {
                // signal main thread to proceed
                rootFileName = string.Empty;
                rootDownloadEvent.Set();
            }
		}

		/// <summary>
		/// Event handler for stream reset.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.ServiceModel.SMProtocol.RSTEventArgs"/> instance containing the event data.</param>
		private static void OnRSTReceived(object sender, RSTEventArgs e)
		{
			string filename = e.Stream.Headers[SMHeaders.Path];
			if (filename == null)
			{
				filename = string.Empty;
			}

            lock (exclusiveLock)
            {
                currentStreams.CloseStream(filename);
            }

            if (rootFileName == filename)
            {
                // signal main thread to proceed
                rootFileName = string.Empty;
                rootDownloadEvent.Set();
            }

            SMLogger.LogError("Stream reset (File not found): stream id=" + e.Stream.StreamId + " filename=" + filename);
		}

		/// <summary>
		/// Event handler for open stream.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">Event data</param>
		private static void OnStreamOpened(object sender, StreamEventArgs e)
		{
			SMLogger.LogConsole("Stream is opened: id=" + e.Stream.StreamId);
			string filename = e.Stream.Headers[SMHeaders.Path];
            lock (exclusiveLock)
            {
                currentStreams.StartStream(filename, e.Stream.StreamId);
            }
        }

        /// <summary>
        /// Event handler for close stream.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnStreamClosed(object sender, RSTEventArgs e)
        {
            string filename = e.Stream.Headers[SMHeaders.Path];
            if (filename == null)
            {
                filename = string.Empty;
            }

            SMLogger.LogConsole("Stream is closed: id=" + e.Stream.StreamId);
            lock (exclusiveLock)
            {
                currentStreams.CloseStream(filename);
            }
        }

        /// <summary>
		/// Event handler for SM session error.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">Event data</param>
		private static void OnSessionError(object sender, SMProtocolErrorEventArgs e)
		{
			SMLogger.LogError("Session error: " + e.Exeption.Message);
            if (session.State == SMSessionState.Closed)
            {
                // server closed on error
                if (appScriptMode)
                {
                    // abort execution with error code
                    Environment.Exit(1);
                }
            }
        }

		/// <summary>
		/// Event handler for SM session open.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">Event data</param>
		private static void OnSessionOpened(object sender, EventArgs e)
		{
			SMLogger.LogConsole("Session is opened: " + session.Uri);
			SMLogger.LogDebug("Session open URI=" + session.Uri + " State=" + session.State + " IsFlowControlEnabled=" + session.IsFlowControlEnabled);
		}

		/// <summary>
		/// Called when session is closed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void OnSessionClosed(object sender, EventArgs e)
		{
			SMLogger.LogConsole("Session closed");
			MonitoringControl(MonitorState.MonitorOff);
		}

		/// <summary>
		/// Download file using http. May have server latency as first argument.
		/// </summary> 
        /// <param name="arguments">The arguments.</param>
		private static void HttpGetFile(string arguments)
		{
			string[] args = arguments.Split(' ');
			string uri = string.Empty;
			int latency = -1;

			if (args.Length > 1)
			{
				uri = args[1];
				try
				{
					latency = Convert.ToInt32(args[0]);
				}
				catch
				{
				}
			}
			else if (args.Length > 0)
			{
				uri = args[0];
			}

			if (uri == string.Empty)
			{
				SMLogger.LogError("HTTPGET needs file name.");
				return;
			}

			// if we have partial path without leading slash, add one
			if ((uri[0] != '/') &&
				(string.Compare(uri, 0, "http:", 0, 5, true) != 0) &&
				(string.Compare(uri, 0, "https:", 0, 6, true) != 0))
			{
				uri = "/" + uri;
			}

			if (uri[0] == '/')
			{
				// if session is active prepend http://<HOST>:<PORT>
				if (session != null && session.State == SMSessionState.Opened)
				{
					string portString = string.Empty;
					if (session.Uri.Port != -1)
					{
						// non-default HTTP port
						portString = ":" + session.Uri.Port.ToString();
					}

					uri = "https://" + session.Uri.Host + portString + uri;
				}
				else
				{
					SMLogger.LogError("HTTPGET " + uri + " needs server name. Please use CONNECT to connect to server first.");
					return;
				}
			}

			if (protocolMonitor != null)
			{
				protocolMonitor.LastStartDate = DateTime.Now;
			}

			// set server latency if available
			if (latency >= 0)
			{
				serverLatency = latency;
			}

			HttpRequest request = new HttpRequest(serverLatency);
			try
			{
				if (protocolMonitor != null)
				{
					protocolMonitor.LastHTTPLog = request.GetFile(uri.ToLower());
				}
				else
				{
					request.GetFile(uri.ToLower());
				}

				// done downloading, save log data
				if (protocolMonitor != null)
				{
					protocolMonitor.LastEndDate = DateTime.Now;
				}

				SaveStats();
			}
			finally
			{
				request.Dispose();
			}
		}

		/// <summary>
		/// Run all commands in script file and produce totals
		/// </summary>
		/// <param name="filename">The file name.</param>
		private static void RunScriptFile(string filename)
		{
			if ((filename == string.Empty) || (!File.Exists(filename)))
			{
				SMLogger.LogError("RUN needs valid script file name.");
				return;
			}

			List<string> args = new List<string>();
			using (StreamReader sr = new StreamReader(filename))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					string[] commands;
					if ((line != string.Empty) && (line[0] != '#'))
					{
						commands = line.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						args.AddRange(commands);
					}
				}
			}

			ParseAndExec(args.ToArray());
		}
	}
}
