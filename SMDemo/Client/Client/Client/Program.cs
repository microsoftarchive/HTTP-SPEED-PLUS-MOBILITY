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
        /// Internal command queue
        /// </summary>
        private static readonly Queue parameters = new Queue();

        /// <summary>
        /// Internal session object
        /// </summary>
        private static SMSession session;

        /// <summary>
        /// Session monitor.
        /// </summary>
        private static ProtocolMonitor protocolMonitor;

        /// <summary>
        /// Main of client
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
        /// Command line parser and executer
        /// </summary>
        /// <param name="args">The command list</param>
        /// <returns>The return code, 0 for success, 1 for error.</returns>
        private static int ParseAndExec(string[] args)
        {
            int res = 0;

            bool needNext = false;
            string cmd = string.Empty;
            foreach (string txt in args)
            {
                if (needNext)
                {
                    ArgPair argpair = new ArgPair(cmd, txt);
                    needNext = false;
                    parameters.Enqueue(argpair);
                }
                else
                {
                    switch (txt.ToUpper())
                    {
                        case "CLOSE":
                        case "HELP":
                        case "DIR":
                        case "DUMP-STATS":
                        case "EXIT":
                            ArgPair argpair = new ArgPair(txt, string.Empty);
                            parameters.Enqueue(argpair);
                            break;

                        case "CAPTURE-STATS":
                        case "CONNECT":
                        case "GET":
                        case "HTTPGET":
                        case "RUN":
                        case "SAVE-STATS":
                        case "VERBOSE":
                            cmd = txt;
                            needNext = true;
                            break;

                        default:
                            SMLogger.LogError("Unknown command " + txt);
                            return 1;
                    }
                }
            }

            if (needNext)
            {
                SMLogger.LogError("Command " + cmd + " missing value.");
                return 1;
            }

            while ((parameters.Count != 0) && (res == 0))
            {
                ArgPair ap = (ArgPair)parameters.Dequeue();
                res = ExecuteOneCommand(ap.Cmd, ap.Value);
                Thread.Sleep(400);
            }

            return 0;
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
            switch (cmd.ToUpper())
            {
                case "HELP":
                    DisplayHelp();
                    break;
                case "EXIT":
                    return 1;
                case "VERBOSE":
                    VerboseMode(val);
                    break;
                case "CONNECT":
                    OpenSession(val);
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
                    if (session != null)
                    {
                        DisplayFilesListing();
                    }
                    else
                    {
                        SMLogger.LogError("Session is not opened.");
                    }

                    break;
                case "CAPTURE-STATS":
                    MonitoringControl(ProtocolMonitor.StringToState(val));
                    break;
                case "DUMP-STATS":
                    GetMonitoringStats();
                    break;
                case "SAVE-STATS":
                    SaveStats(val);
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
                if (commands.Length > 1)
                {
                    value = commands[1];
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
        /// <param name="slotid">The slot id</param>
        private static void SaveStats(string slotid)
        {
            if (slotid == string.Empty)
            {
                SMLogger.LogError("SAVE-STATS needs integer SlotId (0,1,..).");
                return;
            }

            int slot = 0;

            // process potential garbage from input
            try
            {
                slot = int.Parse(slotid);
            }
            catch
            {
                SMLogger.LogError("SAVE-STATS needs integer SlotId (0,1,..).");
                return;
            }

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

            if (!protocolMonitor.SaveSlot(slot))
            {
                SMLogger.LogError("Unable to save statistics into slot " + slot);
            }
        }

        /// <summary>
        /// Help for client.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("HTTP Speed+Mobility Prototype Client help\n");
            Console.WriteLine("HELP                          Display this help information");
            Console.WriteLine("CONNECT <URI>                 Connect to the server endpoint.\n" +
                              "                              If no URI is specified,\n" +
                              "                              defaults to ws://smserver.cloudapp.net:8080");
            Console.WriteLine("DIR                           List files on server.");
            Console.WriteLine("GET <filename>                Download web page and associated resources.\n" + 
                              "                              Ex. GET /files/test.txt");
            Console.WriteLine("VERBOSE [1|2|3]               Display verbose output.\n" + 
                              "                              1 is least and 3 most verbose");
            Console.WriteLine("CAPTURE-STATS [On|Off|Reset]  Start/stop/reset protocol monitoring.");
            Console.WriteLine("SAVE-STATS <Id>               Save statistics for side-by-side viewing.");
            Console.WriteLine("DUMP-STATS                    Display statistics captured using CAPTURE-STATS.");
            Console.WriteLine("HTTPGET                       Download file using http.\n" + 
                              "                              Ex. http://localhost:8080/microsoft/default.htm.\n" +
                              "                              Or for current connection /microsoft/default.htm");
            Console.WriteLine("CLOSE                         Close session");
            Console.WriteLine("RUN <filename>                Run command script");
            Console.WriteLine("EXIT                          Exit application");
            Console.WriteLine();
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
                        SMHeaders headers = new SMHeaders();
                        headers[SMHeaders.Path] = "index";
                        headers[SMHeaders.Version] = "http/1.1";
                        headers[SMHeaders.Method] = "GET";
                        headers[SMHeaders.Scheme] = "http";
                        headers[SMHeaders.ContentType] = ContentTypes.TextPlain;
                        SMStream stream = session.OpenStream(headers, true);
                        stream.OnDataReceived += OnDataReceived;
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
                session.End();
            }
        }

        /// <summary>
        /// Open SM session.
        /// </summary>
        /// <param name="val">The URI</param>
        private static void OpenSession(string val)
        {
            Uri uri;
            if (val == string.Empty)
            {
                // set default value for now
                val = "ws://smserver.cloudapp.net:8080";
            }

            if (!Uri.TryCreate(val, UriKind.Absolute, out uri))
            {
                SMLogger.LogError("Uri is not in correct format.");
                return;
            }

            CreateSession(uri);
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
        private static void CreateSession(Uri uri)
        {
            CloseSession();

            session = new SMSession(uri, false);

            // URI can still be invalid, missing protocol prefix for example
            try
            {
                session.Open();

                session.OnOpen += OnSessionOpened;
                session.OnClose += OnSessionClosed;
                session.OnError += OnSessionError;
                session.OnStreamOpened += OnStreamOpened;
            }
            catch
            {
                SMLogger.LogError("Unable to open session for " + uri.ToString());
            }
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
                protocolMonitor.Totals.LogTitle = "S+M load " + fileName;

                // clear previous HTTP download
                protocolMonitor.LastHTTPLog = null;
            }

            DownloadPath(fileName);
        }

        /// <summary>
        /// File download.
        /// </summary>
        /// <param name="fileName">The file name</param>
        private static void DownloadPath(string fileName)
        {
            if (session == null)
            {
                SMLogger.LogError("Not connected to server. Please use CONNECT to connect to server first.");
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
                        break;

                    default:
                        SMLogger.LogError("Unknown SMSessionState " + session.State);
                        break;
                }
            }
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
            string newfilepath;

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

                        foreach (var image in document.Images)
                        {
                            DownloadPath(string.Format("{0}/{1}", directory, image));
                        }

                        foreach (var link in document.Links)
                        {
                            DownloadPath(string.Format("{0}/{1}", directory, link));
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
        }

        /// <summary>
        /// Event handler for SM session error.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnSessionError(object sender, SMProtocolErrorEventArgs e)
        {
            SMLogger.LogError("Session error: " + e.Exeption.Message);
        }

        /// <summary>
        /// Event handler for SM session open.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event data</param>
        private static void OnSessionOpened(object sender, EventArgs e)
        {
            // start monitoring by default
            MonitoringControl(MonitorState.MonitorOn);
            SMLogger.LogConsole("Session is opened: " + session.Uri);
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
        /// Download file using http.
        /// </summary>
        /// <param name="uri">The URI.</param>
        private static void HttpGetFile(string uri)
        {
            if (uri == string.Empty)
            {
                SMLogger.LogError("HTTPGET needs file name.");
                return;
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

                    uri = "http://" + session.Uri.Host + portString + uri;
                }
                else
                {
                    SMLogger.LogError("HTTPGET " + uri + " needs server name. Please use CONNECT to connect to server first.");
                    return;
                }
            }

            HttpRequest request = new HttpRequest();
            protocolMonitor.LastHTTPLog = request.GetFile(uri);
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
