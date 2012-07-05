//-----------------------------------------------------------------------
// <copyright file="ControlFrame.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol.SMFrames
{
    /// <summary>
    /// Class that represents control frame.
    /// </summary>
    public class ControlFrame : BaseFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlFrame"/> class.
        /// </summary>
        public ControlFrame()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlFrame"/> class.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public ControlFrame(SMHeaders headers)
        {
            this.IsControl = true;
            this.Version = SMProtocol.Version;
            this.Headers = headers ?? new SMHeaders();
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public FrameType Type { get; set; }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        public SMHeaders Headers { get; private set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public byte Priority { get; set; }

        /// <summary>
        /// Gets or sets the slot.
        /// </summary>
        /// <value>
        /// The slot.
        /// </value>
        public byte Slot { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public short Version { get; set; }

        /// <summary>
        /// Gets or sets the credit addition
        /// </summary>
        /// <value>
        /// In bytes, that the recipient must add to the stream's credit balance.
        /// </value>
      //  public Int32 CreditAddition { get; set; }
        /*
        /// <summary>
        /// Gets or sets the associated to stream id.
        /// </summary>
        /// <value>
        /// The associated to stream id.
        /// </value>
       // public Int32 AssociatedToStreamId { get; set; }
         */
    }
}
