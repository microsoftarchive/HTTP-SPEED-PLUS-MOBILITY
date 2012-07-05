// <copyright file="GlobalSuppressions.cs" company="Microsoft Open Technologies, Inc.">
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

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Strong name signing not required for codeplex.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "wss", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocketProtocol.#.ctor(System.String,System.String,System.String,System.Boolean)", Justification = "Literal needs to be added to dictionary.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WebSocket", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocket.#Open()", Justification = "Literal needs to be added to dictionary.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WebSocket", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocket.#MaxInputBufferSize", Justification = "Literal needs to be added to dictionary.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MaxInputBufferSize", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocketProtocol.#EnsureRoomInBuffer()", Justification = "Literal needs to be added to dictionary.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.ServiceModel.WebSockets", Justification = "This namespace defines these types.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "System.ServiceModel.WebSockets.MontenegroHybiUpgradeHelloHandshake00WebSocketProtocol.#StartWebSocketHandshake()", Justification = "Specification requirement")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WebSocket", Scope = "member", Target = "System.ServiceModel.WebSockets.MontenegroHybiUpgradeHelloHandshake00WebSocketProtocol.#ValidateHandshakeResponseHeaders()", Justification = "Specification requirement")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sec-websocket-location", Scope = "member", Target = "System.ServiceModel.WebSockets.MontenegroHybiUpgradeHelloHandshake00WebSocketProtocol.#ValidateHandshakeResponseHeaders()", Justification = "Specification requirement")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sec-websocket-origin", Scope = "member", Target = "System.ServiceModel.WebSockets.MontenegroHybiUpgradeHelloHandshake00WebSocketProtocol.#ValidateHandshakeResponseHeaders()", Justification = "Specification requirement")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sec-websocket-protocol", Scope = "member", Target = "System.ServiceModel.WebSockets.MontenegroHybiUpgradeHelloHandshake00WebSocketProtocol.#ValidateHandshakeResponseHeaders()", Justification = "Specification requirement")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocket.#.ctor(System.String)", Justification = "This is destined as an interface with a web browser's javascript engine that does not understand Uri.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocket.#.ctor(System.String,System.ServiceModel.WebSockets.WebSocketVersion)", Justification = "This is destined as an interface with a web browser's javascript engine that does not understand Uri.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocket.#.ctor(System.String,System.ServiceModel.WebSockets.WebSocketVersion,System.String,System.String)", Justification = "This is destined as an interface with a web browser's javascript engine that does not understand Uri.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WebSocket", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocket.#MaxInputBufferSize", Justification = "Specification requirement")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocketEventArgs.#BinaryData", Justification = "Byte array represents the message buffer")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Scope = "type", Target = "System.ServiceModel.SMProtocol.SMStreamHeaders", Justification = "This type is not supposed for serialization")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Scope = "type", Target = "System.ServiceModel.SMProtocol.SMProtocolExeption", Justification = "This type is not supposed for serialization")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "System.ServiceModel.SMProtocol.FrameSerializer.#Serialize(System.ServiceModel.SMProtocol.SMFrames.BaseFrame)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "System.ServiceModel.SMProtocol.FrameSerializer.#Deserialize(System.Byte[])")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMData.#Data", Justification = "This is a raw buffer property.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMFrames.DataFrame.#Data", Justification = "This is a raw buffer property.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMLogger.#LogDebug(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMLogger.#LogError(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMLogger.#LogInfo(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "System.ServiceModel.SMProtocol.SMFramesMonitor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMFramesMonitor.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMFramesMonitor.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)", Scope = "member", Target = "System.ServiceModel.SMProtocol.SMProtocol.#OnSocketClose(System.Object,System.ServiceModel.WebSockets.WebSocketProtocolEventArgs)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "System.ServiceModel.WebSockets.WebSocketProtocolEventArgs.#BinaryData")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "ClientProtocol.ServiceModel.SMProtocol.MessageProcessing.CompressionDictionary.#Dictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "ClientProtocol.ServiceModel.SMProtocol.MessageProcessing.CompressionProcessor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Scope = "member", Target = "ClientProtocol.ServiceModel.SMProtocol.MessageProcessing.CompressionProcessor.#Dispose()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "ClientProtocol.ServiceModel.SMProtocol.MessageProcessing.CompressionProcessor.#Dispose()")]
