//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Open Technologies, Inc.">
//
// The copyright in this software is being made available under the BSD License, included below. 
// This software may be subject to other third party and contributor rights, including patent rights, 
// and no such rights are granted under this license.
//
// Copyright (c) 2012, Microsoft Open Technologies, Inc. 
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice, 
//   this list of conditions and the following disclaimer in the documentation 
//   and/or other materials provided with the distribution.
// - Neither the name of Microsoft Open Technologies, Inc. nor the names of its contributors 
//   may be used to endorse or promote products derived from this software 
//   without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
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
    using System.ServiceModel.Http2Protocol;
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
        /// Server port for unencrypted protocol
        /// </summary>
        private const int UNSECURE_PORT = 8080;

        /// <summary>
        /// Server port for encrypted protocol
        /// </summary>
        private const int SECURE_PORT = 8081;

        /// <summary>
        /// Dir command download file.
        /// </summary>
        private const string DIR_FILE = "index.html";

        /// <summary>
        /// Internal command queue
        /// </summary>
        private static readonly Queue Parameters = new Queue();

        /// <summary>
        /// Internal session object
        /// </summary>
        private static ProtocolSession session;

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
            Http2Logger.LoggerLevel = Http2LoggerState.VerboseLogging;
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

                case "HTTP11GET":
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
                    Http2Logger.LogError("Unknown command " + tok);
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
            Http2Logger.LogConsole("Executing " + cmd + " " + val);
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
                    return -1;
                case "VERBOSE":
                    VerboseMode(val);
                    break;
#if DEBUG
                case "CONNECT":
                    res = OpenSession(val);
                    break;
#endif
                case "GET":
                    DownloadFileCommand(val, null);
                    break;
                case "DIR":
                    DownloadFileCommand(val, DIR_FILE);
                    break;
                case "CAPTURE-STATS":
                    MonitoringControl(ProtocolMonitor.StringToState(val));
                    break;
                case "DUMP-STATS":
                    GetMonitoringStats();
                    break;
#if DEBUG
                case "CLOSE":
                    CloseSession();
                    break;
#endif
                case "HTTP11GET":
                    HttpGetFile(val, false);
                    break;
                case "RUN":
                    RunScriptFile(val);
                    break;
                default:
                    Http2Logger.LogError("Unknown command " + cmd);
                    break;
            }

            return res;
        }

        /// <summary>
        /// Command loop executer.
        /// </summary>
        /// <returns>The result code. 0 for success, 1 for error.</returns>
        private static int ExecuteCommandLoop()
        {
            DisplayHelp();
            Http2Logger.LoggerConsole = true;
            Http2Logger.LogConsole("Please type a command:\n");
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

                if (ExecuteOneCommand(command, value) == -1)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Command to download specified file.
        /// </summary>
        /// <param name="uri">File uri.</param>
        /// <param name="fileName">File name.</param>
        /// <returns>Command result.</returns>
        private static int DownloadFileCommand(string uri, string fileName)
        {
            if (uri == string.Empty)
            {
                Http2Logger.LogError("Please provide file/host uri.");
                return 1;
            }

            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                Http2Logger.LogError("Incorrect Uri format.");
                return 1;
            }

            Uri getUri = new Uri(uri);

            int res = 0;
            if (session == null || session.State != ProtocolSessionState.Opened)
            {
                string openSessionArgs = getUri.Scheme + "://" + getUri.Authority;
                res = OpenSession(openSessionArgs);
                if (protocolMonitor != null && !protocolMonitor.IsAttached)
                {
                    protocolMonitor.Attach(session);
                }
            }

            DownloadRootFile(fileName ?? getUri.AbsolutePath);
            return res;
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
                    if (session != null && session.State == ProtocolSessionState.Opened)
                    {
                        if (protocolMonitor != null)
                        {
                            protocolMonitor.Dispose();
                        }

                        protocolMonitor = new ProtocolMonitor();
                        protocolMonitor.Attach(session);
                    }
                    else
                    {
                        protocolMonitor = new ProtocolMonitor();
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
                    Http2Logger.LogError("CAPTURE-STATS needs [On|Off|Reset].");
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
                string output = protocolMonitor.GetMonitoringStats(Http2Logger.LoggerLevel);

                if (Http2Logger.LoggerLevel < Http2LoggerState.VerboseLogging)
                {
                    // verbose level is too low to output as INFO
                    // if we trigger DUMP-STATS from script we still want to see the results
                    Http2Logger.LoggerConsole = true;
                    Http2Logger.LogConsole("\r\n" + output);
                    Http2Logger.LoggerConsole = false;
                }
                else
                {
                    Http2Logger.LogInfo("\r\n" + output);
                }
            }
            else
            {
                if (session == null || session.State != ProtocolSessionState.Opened)
                {
                    Http2Logger.LogError("Session was closed due to error or not opened.");
                }
                else
                {
                    Http2Logger.LogInfo("Please use \"CAPTURE-STATS On\" to start monitoring.");
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
                if (session == null || session.State != ProtocolSessionState.Opened)
                {
                    Http2Logger.LogInfo("Stats will not be saved since session is not opened.");
                }
                else
                {
                    Http2Logger.LogError("Please use \"CAPTURE-STATS On\" to start monitoring.");
                }

                return;
            }

            if (!protocolMonitor.SaveSlot(slotId))
            {
                Http2Logger.LogError("Unable to save statistics into slot " + slotId);
            }
        }

        /// <summary>
        /// Help for client.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("HTTP2 Prototype Client help\n");
            Console.WriteLine("HELP                          Display this help information");
            Console.WriteLine("HELP command                  Display detailed help for command\n" +
                              "                              Ex. HELP GET");
            Console.WriteLine("DIR <host url>                List files on server.");
            Console.WriteLine("GET <resource url>            Download web page and associated resources.\n" +
                              "                              E.g.: http://http2test.cloudapp.net/index.html");
            Console.WriteLine("VERBOSE   [1|2|3]             Display verbose output.");
            Console.WriteLine("CAPTURE-STATS [On|Off|Reset]  Start/stop/reset protocol monitoring.");
            Console.WriteLine("DUMP-STATS                    Display statistics captured using CAPTURE-STATS.");
            Console.WriteLine("HTTP11GET <filename>          Download file using HTTP 1.1.");
            Console.WriteLine("RUN  <filename>               Run command script");
            Console.WriteLine("EXIT                          Exit application");
            Console.WriteLine();
        }

        /// <summary>
        /// Help for client.
        /// </summary>
        /// <param name="val">Command name for HELP</param>
        private static void DisplayDetailedHelp(string val)
        {
            Console.WriteLine("HTTP2 Prototype Client help\n");
            switch (val.ToUpper())
            {
                case "DIR":
                    Console.WriteLine("DIR <host url>  Lists files on server available for download.\n");
                    Console.WriteLine("  This command does not list all the files, only download targets.");
                    Console.WriteLine("  Download targets are either text files, or top level HTML files.");
                    Console.WriteLine("  When you apply GET ot HTTP11GET to download target, all associated files");
                    Console.WriteLine("  are also downloaded.");
                    Console.WriteLine("\n");
                    Console.WriteLine("Note: You can still download specific associated file by specifying exact path.");
                    Console.WriteLine("       DIR requests file \"index.html\".");
                    Console.WriteLine("  Examples of DIR:\n");
                    Console.WriteLine("  GET https://microsoft/");
                    Console.WriteLine("\n");
                    break;

                case "GET":
                    Console.WriteLine("GET <resource url> Download web page and associated resources.\n");
                    Console.WriteLine("  <resource url> should be path to web page.");
                    Console.WriteLine("  Localy downloaded files are stored in directory relative to current.");
                    Console.WriteLine("  Directory structure is preserved.");
                    Console.WriteLine("  Download is done using HTTP2 protocol.");
                    Console.WriteLine("  You can get list of files with command DIR.\n");
                    Console.WriteLine("  Examples of GET:\n");
                    Console.WriteLine("  GET https://microsoft/default.htm");
                    Console.WriteLine("     download web page and all associated resources to local directory .\\microsoft\\");
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

                case "HTTP11GET":
                    Console.WriteLine("HTTP11GET <latency> <filename>       Download web page using HTTP\n");
                    Console.WriteLine("  Latency is server latency in ms. Default value is zero.");
                    Console.WriteLine("  This command can take full URL path or relative to web root path.");
                    Console.WriteLine("  If there exists open session, relative path is assumed to refer to");
                    Console.WriteLine("  current web root. If no session is opened, user should supply full");
                    Console.WriteLine("  URL for the file.");
                    Console.WriteLine("  Locally downloaded files are stored in directory relative to current.");
                    Console.WriteLine("  Directory structure is preserved.");
                    Console.WriteLine("  Download is done using 6 concurrent threads to simulate IE.");
                    Console.WriteLine("  You can get list of files with command DIR.\n");
                    Console.WriteLine("  Examples of HTTP11GET:\n");
                    Console.WriteLine("  HTTP11GET http://localhost:8080/microsoft/default_files/style.css");
                    Console.WriteLine("     download just style.css to local directory .\\microsoft\\default_files\\");
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
                    Console.WriteLine("# Get file with HTTP2 protocol");
                    Console.WriteLine("GET /files/test.txt");
                    Console.WriteLine("# Get file with HTTP1.1");
                    Console.WriteLine("HTTP11GET /files/test.txt");
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
        /// Close Http2 session.
        /// </summary>
        private static void CloseSession()
        {
            if (session != null && session.State == ProtocolSessionState.Opened)
            {
                // wait until session changes its state
                // for high latency connections we cannot assume this session closes synchronosly
                sessionMonitorEvent.Reset();

                session.End();

                sessionMonitorEvent.WaitOne(30000);
            }
        }

        /// <summary>
        /// Open http2 session.
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

            if (options != string.Empty && options != "c" && options != "s")
            {
                Http2Logger.LogError(options + ": Compression option should be 'c' or 's'.");
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
                    Http2Logger.LogError(quantum + ": Credit update quantum should be a positive integer.");
                    return 1;
                }
            }

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                Http2Logger.LogError(" [" + url + "] Uri is not in correct format.");
                return 1;
            }

            if (uri.Port == -1)
            {
                // user did not specify the port
                if (url.Substring(0, 6) == "https://")
                {
                    url = url + ":" + SECURE_PORT.ToString();
                }
                else
                {
                    Http2Logger.LogError("Unrecognized URL scheme. Specify 'http/https' URL scheme.");
                    return 1;
                }

                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    Http2Logger.LogError(" [" + url + "] Uri is not in correct format.");
                    return 1;
                }
            }

            return CreateSession(uri, new ProtocolOptions(options, quantum));
        }

        /// <summary>
        /// Create http2 session. Close if previously opened.
        /// </summary>
        /// <param name="val">The verbose switch</param>
        private static void VerboseMode(string val)
        {
            if (val == string.Empty)
            {
                Http2Logger.LogError("VERBOSE needs level [1|2|3].");
                return;
            }

            // process potential garbage from input
            try
            {
                Http2Logger.LoggerLevel = (Http2LoggerState)int.Parse(val);
            }
            catch
            {
                Http2Logger.LoggerLevel = Http2LoggerState.ErrorsOnly;
            }

            if ((Http2Logger.LoggerLevel < Http2LoggerState.NoLogging) || (Http2Logger.LoggerLevel > Http2LoggerState.MaxLogging))
            {
                Http2Logger.LogError("VERBOSE needs level [1|2|3].");
                Http2Logger.LoggerLevel = Http2LoggerState.ErrorsOnly;
            }

            Http2Logger.LogConsole("VERBOSE output " + Http2Logger.LoggerLevel);
        }

        /// <summary>
        /// Create http2 session. Close if previously opened.
        /// </summary>
        /// <param name="uri">The URI</param>
        /// <param name="option">Options for session</param>
        /// <returns>The return code.</returns>
        private static int CreateSession(Uri uri, ProtocolOptions option)
        {
            int res = 0;
            CloseSession();

            session = new ProtocolSession(uri, false, option);

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
            }
            catch (Exception e)
            {
                Http2Logger.LogError("Unable to open session for " + uri + ". " + e.Message);
                res = 1;
            }

            return res;
        }

        /// <summary>
        /// File download. Called from command processor for top of the file tree
        /// </summary>
        /// <param name="fileName">The file name</param>
        private static void DownloadRootFile(string fileName)
        {
            if (protocolMonitor != null)
            {
                // set title of HTTP2 dowload
                protocolMonitor.Totals.LogTitle = "HTTP2 " + fileName;
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
                // if session state not "Opened" we got server side error and there is no connection
                if (session.State == ProtocolSessionState.Opened)
                {
                    // wait until all streams are closed
                    timeoutStreamMonitor = false;
                    streamMonitorEvent.Reset();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(StreamListMonitorProc), 0);

                    streamMonitorEvent.WaitOne(1000);
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
                        Http2Logger.LogDebug("Zero stream count reached");

                        // done downloading, save log data
                        if (protocolMonitor != null)
                        {
                            protocolMonitor.LastEndDate = DateTime.Now;
                        }

                        if (protocolMonitor != null)
                            SaveStats();
                        streamMonitorEvent.Set();
                        return;
                    }
                }

                Thread.Sleep(100);
            }
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
                Http2Logger.LogError("Not connected to server. Please try again.");
                return false;
            }
            else
            {
                switch (session.State)
                {
                    case ProtocolSessionState.Created:
                        Http2Logger.LogError("Session was created but not opened yet.");
                        break;

                    case ProtocolSessionState.Opened:
                        ProtocolHeaders headers = new ProtocolHeaders();
                        headers[ProtocolHeaders.Path] = "/" + fileName.TrimStart('/');
                        headers[ProtocolHeaders.Version] = "HTTP/1.1";
                        headers[ProtocolHeaders.Method] = "GET";
                        headers[ProtocolHeaders.Scheme] = session.Uri.Scheme;
                        headers[ProtocolHeaders.Host] = session.Uri.Host + ":" + session.Uri.Port;
                        headers[ProtocolHeaders.ContentType] = ContentTypes.GetTypeFromFileName(fileName);


                        // If true, then stream will be half closed.
                        Http2Stream stream = session.OpenStream(headers, true);
                        stream.OnDataReceived += OnDataReceived;
                        stream.OnRSTReceived += OnRSTReceived;
                        break;
                    case ProtocolSessionState.Closed:
                        Http2Logger.LogError("Session was closed due to error or not opened.");
                        return false;

                    default:
                        Http2Logger.LogError("Unknown ProtocolSessionState " + session.State);
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
            string file = Path.GetFileName(e.Stream.Headers[ProtocolHeaders.Path]);

            string directory = Path.GetDirectoryName(e.Stream.Headers[ProtocolHeaders.Path]);
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
            int fileNameStart = e.Stream.Headers[ProtocolHeaders.Path].LastIndexOf(file);
            string origdir = e.Stream.Headers[ProtocolHeaders.Path].Substring(0, fileNameStart);

            if (File.Exists(newfilepath))
            {
                try
                {
                    File.Delete(newfilepath);
                }
                catch (Exception)
                {
                    Http2Logger.LogConsole("Cant overwrite file: " + newfilepath);
                }
            }
            using (var fs = new FileStream(newfilepath, FileMode.Create))
            {
                fs.Write(e.Data.Data, 0, e.Data.Data.Length);
            }

            Http2Logger.LogConsole("File downloaded: " + file);

            if (e.Stream.Headers[ProtocolHeaders.Path].Trim('/').Equals(DIR_FILE))
            {
                string text = e.Data.AsUtf8Text();
                Console.WriteLine();
                Console.WriteLine(text);
            }
            else
            {
                switch (e.Stream.Headers[ProtocolHeaders.ContentType])
                {
                    case ContentTypes.TextHtml:
                        {
                            string text = e.Data.AsUtf8Text();

                            XHtmlDocument document = null;

                            try
                            {
                                document = XHtmlDocument.Parse(text);
                                MakeDownloadList(origdir, document);
                            }
                            catch (Exception ex)
                            {
                                Http2Logger.LogError(ex.Message);
                                return;
                            }

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
                }
            }

            if (e.Stream.State != Http2StreamState.Closed)
            {
                // stream must be closed by this time since it was created with FIN flag.
                // if this is not the case - close the stream with error
                e.Stream.Close(StatusCode.Cancel);
            }

            if (protocolMonitor != null)
            {
                StreamInfo ss = protocolMonitor.StreamStatistics(e.Stream.StreamId);
                Http2Logger.LogInfo("Bytes transferred:" +
                    ss.UpCount + " upstream, " +
                    ss.DownCount + " downstream, " +
                    (ss.DownCount + ss.UpCount) + " Total");
            }

            if (rootFileName == e.Stream.Headers[ProtocolHeaders.Path])
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
        /// <param name="e">The <see cref="System.ServiceModel.Http2Protocol.RSTEventArgs"/> instance containing the event data.</param>
        private static void OnRSTReceived(object sender, RSTEventArgs e)
        {
            string filename = e.Stream.Headers[ProtocolHeaders.Path];
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

            Http2Logger.LogError("Stream reset (File not found): stream id=" + e.Stream.StreamId + " filename=" + filename);
        }

        /// <summary>
        /// Event handler for open stream.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnStreamOpened(object sender, StreamEventArgs e)
        {
            Http2Logger.LogConsole("Stream is opened: id=" + e.Stream.StreamId);
            string filename = e.Stream.Headers[ProtocolHeaders.Path];
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
            string filename = e.Stream.Headers[ProtocolHeaders.Path];
            if (filename == null)
            {
                filename = string.Empty;
            }

            Http2Logger.LogConsole("Stream is closed: id=" + e.Stream.StreamId);
            lock (exclusiveLock)
            {
                currentStreams.CloseStream(filename);
            }
        }

        /// <summary>
        /// Event handler for Http2 session error.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnSessionError(object sender, ProtocolErrorEventArgs e)
        {
            Http2Logger.LogError("Session error: " + e.Exeption.Message);
            if (session.State == ProtocolSessionState.Closed)
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
        /// Event handler for Http2 session open.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnSessionOpened(object sender, EventArgs e)
        {
            Http2Logger.LogConsole("Session is opened: " + session.Uri);
            Http2Logger.LogDebug("Session open URI=" + session.Uri + " State=" + session.State + " IsFlowControlEnabled=" + session.IsFlowControlEnabled);
        }

        /// <summary>
        /// Called when session is closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void OnSessionClosed(object sender, EventArgs e)
        {
            Http2Logger.LogConsole("Session closed");
            MonitoringControl(MonitorState.MonitorOff);
            session.Dispose();
        }

        /// <summary>
        /// Download file using http. May have server latency as first argument.
        /// </summary> 
        /// <param name="arguments">The arguments.</param>
        private static void HttpGetFile(string arguments, bool http2)
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
                Http2Logger.LogError("HTTP11GET needs file name.");
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
                if (session != null && session.State == ProtocolSessionState.Opened)
                {
                    string portString = string.Empty;
                    if (session.Uri.Port != -1)
                    {
                        // non-default HTTP port
                        portString = ":" + session.Uri.Port.ToString();
                    }
                }
                else
                {
                    Http2Logger.LogError("HTTPGET " + uri + " needs server name.");
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

            HttpRequest request = new HttpRequest(serverLatency, http2);
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
            catch (Exception ex)
            {
                Http2Logger.LogError(ex.Message);
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
                Http2Logger.LogError("RUN needs valid script file name.");
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
