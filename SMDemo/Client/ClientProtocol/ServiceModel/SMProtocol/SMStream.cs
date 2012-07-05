//-----------------------------------------------------------------------
// <copyright file="SMStream.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol
{
    /// <summary>
    /// Speed+Mobility stream that runs inside Web Socket.
    /// </summary>
    public class SMStream
    {
        #region Fields

        /// <summary>
        /// internal SM protocol reference
        /// </summary>
        private readonly SMProtocol protocol;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SMStream"/> class.
        /// </summary>
        /// <param name="id">The stream id.</param>
        /// <param name="session">The stream session.</param>
        public SMStream(int id, SMSession session)
        {
            this.StreamId = id;
            this.Session = session;
            this.Headers = new SMHeaders();
            this.protocol = session.Protocol;
            this.protocol.OnStreamFrame += this.OnProtocolData;
            this.protocol.OnStreamError += this.OnStreamError;
            this.protocol.OnSessionFrame += this.OnSessionFrame;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when data received on the stream.
        /// </summary>
        public event EventHandler<StreamDataEventArgs> OnDataReceived;

        /// <summary>
        /// Occurs when RST received on the stream.
        /// </summary>
        public event EventHandler<RSTEventArgs> OnRSTReceived;

        /// <summary>
        /// Occurs when stream is closed.
        /// </summary>
        public event EventHandler<RSTEventArgs> OnClose;

        /// <summary>
        /// Occurs when headers received on the stream.
        /// </summary>
        public event EventHandler<HeadersEventArgs> OnHeadersReceived;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the stream id.
        /// </summary>
        public int StreamId { get; private set; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        public SMSession Session { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public SMStreamState State { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the stream is closed.
        /// </summary>
        public bool IsClosed 
        { 
            get 
            { 
                return this.State == SMStreamState.Closed; 
            } 
        }

        /// <summary>
        /// Gets the stream request headers.
        /// </summary>
        public SMHeaders Headers { get; internal set; }

        #endregion

        #region Methods

        /// <summary>
        /// Closes the stream with specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void Close(StatusCode reason)
        {
            this.protocol.OnStreamFrame -= this.OnProtocolData;
            this.protocol.OnStreamError -= this.OnStreamError;

            this.State = SMStreamState.Closed;

            if (reason != StatusCode.Success)
            {
                this.SendRST(reason);
            }

            if (this.OnClose != null)
            {
                this.OnClose(this, new RSTEventArgs(this, reason));
            }
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            this.Close(StatusCode.Success);
        }

        /// <summary>
        /// Sends the data to the stream.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="isFin">if set to <c>true</c> than this stream will be half-closed.</param>
        public void SendData(SMData data, bool isFin)
        {
            SMData dataFrame;
            if (Session.IsFlowControlEnabled)
            {
                Session.CurrentCreditBalanceToServer -=  data.Data.Length;
                dataFrame = new SMData(data.Data);
            }
            else
            {
                dataFrame = new SMData(data.Data);
            }

            this.protocol.SendData(this, dataFrame, isFin);
        }

        /// <summary>
        /// Sends the headers to the stream.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">if set to <c>true</c> than this stream will be half-closed.</param>
        public void SendHeaders(SMHeaders headers, bool isFin)
        {
            this.Headers.Merge(headers);
            this.protocol.SendHeaders(this, headers, isFin);
        }

        /// <summary>
        /// Sends the RST to the stream.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void SendRST(StatusCode reason)
        {
            this.protocol.SendRST(this, reason);
        }

        /// <summary>
        /// Send the CREDIT_UPDATE frame to the stream.
        /// </summary>
        /// <param name="creditAddition"></param>
        public void SendCreditUpdate(Int64 creditAddition)
        {
            Session.CurrentCreditBalanceFromServer += creditAddition;
            this.protocol.SendCreditUpdate(this, creditAddition);
        }

        /// <summary>
        /// Opens the stream.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <param name="isFin">The Final flag.</param>
        internal void Open(SMHeaders headers, bool isFin)
        {
            this.Headers.Merge(headers);
            if (isFin)
            {
                this.State = SMStreamState.HalfClosed;
            }

            this.protocol.SendSynStream(this, headers, isFin);
        }

        /// <summary>
        /// Event handler for Session frame.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSessionFrame(object sender, ControlFrameEventArgs e)
        {
            if (e.Frame.StreamId == this.StreamId
                && e.Frame.IsFinal
                && this.State == SMStreamState.HalfClosed)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Event handler for Stream error.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnStreamError(object sender, StreamErrorEventArgs e)
        {
            if (e.Stream == this)
            {
                if (e.Exeption is SMProtocolExeption)
                {
                    this.Close(((SMProtocolExeption)e.Exeption).StatusCode);
                }
                else
                {
                    this.Close(StatusCode.InternalError);
                }
            }
        }

        /// <summary>
        /// Event handler for SM protocol data.
        /// </summary>
        /// <param name="sender">the sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnProtocolData(object sender, StreamEventArgs e)
        {
            if (e.Stream == this)
            {
                if (SMStreamState.Closed == e.Stream.State)
                {
                    this.Close(StatusCode.ProtocolError);
                    return;
                }

                if (e is HeadersEventArgs)
                {
                    var args = (HeadersEventArgs)e;
                    this.Headers.Merge(args.Headers);

                    if (this.OnHeadersReceived != null)
                    {
                        this.OnHeadersReceived(this, args);
                    }
                }
                else if (e is StreamDataEventArgs)
                {
                    StreamDataEventArgs arg = e as StreamDataEventArgs;
                    if (arg.IsFin && this.State == SMStreamState.HalfClosed)
                    {
                        this.Close();
                    }

                    if (this.OnDataReceived != null)
                    {
                        StreamDataEventArgs args = (StreamDataEventArgs)e;
                        this.OnDataReceived(this, e as StreamDataEventArgs);

                        if (Session.IsFlowControlEnabled)
                        {
                            Session.CurrentCreditBalanceFromServer -= args.Data.Data.Length;
                            if (Session.CurrentCreditBalanceFromServer + Session.CreditAddition < Session.MaxCreditBalance)
                            {
                                if (Session.CurrentCreditBalanceFromServer < 0)
                                    SendCreditUpdate(Convert.ToInt32((-1*Session.CurrentCreditBalanceFromServer/Session.CreditAddition +
                                                      1)*Session.CreditAddition));
                                else
                                    SendCreditUpdate(Convert.ToInt32(Session.CreditAddition));
                            }
                        }
                    }
                }
                else if (e is RSTEventArgs)
                {
                    if (this.OnRSTReceived != null)
                    {
                        this.OnRSTReceived(this, e as RSTEventArgs);
                    }
                }
            }
        }

        #endregion
    }
}
