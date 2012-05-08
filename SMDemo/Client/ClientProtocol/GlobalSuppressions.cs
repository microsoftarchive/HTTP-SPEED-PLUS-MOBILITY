// <copyright file="GlobalSuppressions.cs" company="Microsoft Open Technologies, Inc.">
//
// Copyright Microsoft Open Technologies, Inc.
//
// Microsoft Reference Source License.
//
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, 
// do not use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," and "distribution" have the same 
// meaning here as under U.S. copyright law.
// "You" means the licensee of the software.
// "Your company" means the company you worked for when you downloaded the 
// software.
// "Reference use" means use of the software within your company as a 
// reference, in read only form, for the sole purposes of debugging 
// your products, maintaining your products, or enhancing the 
// interoperability of your products with the software, and specifically 
// excludes the right to distribute the software outside of your company.
// "Licensed patents" means any Licensor patent claims which read directly 
// on the software as distributed by the Licensor under this license. 
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, the Licensor
// grants you a non-transferable, non-exclusive, worldwide, royalty-free
// copyright license to reproduce the software for reference use.
// (B) Patent Grant- Subject to the terms of this license, the Licensor
// grants you a non-transferable, non-exclusive, worldwide, royalty-free
// patent license under licensed patents for reference use. 
//
// 3. Limitations
// (A) No Trademark License- This license does not grant you any rights
// to use the Licensor’s name, logo, or trademarks.
// (B) If you begin patent litigation against the Licensor over patents
// that you think may apply to the software (including a cross-claim or
// counterclaim in a lawsuit), your license to the software ends automatically.
// (C) The software is licensed "as-is." You bear the risk of using it.
// The Licensor gives no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws which
// this license cannot change. To the extent permitted under your local laws,
// the Licensor excludes the implied warranties of merchantability, fitness
// for a particular purpose and non-infringement. 
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
