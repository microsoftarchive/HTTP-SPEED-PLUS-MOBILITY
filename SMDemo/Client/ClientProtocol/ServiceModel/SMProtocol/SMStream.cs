//-----------------------------------------------------------------------
// <copyright file="SMStream.cs" company="Microsoft Open Technologies, Inc.">
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
            this.protocol.SendData(this, data, isFin);
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
                        this.OnDataReceived(this, e as StreamDataEventArgs);
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
