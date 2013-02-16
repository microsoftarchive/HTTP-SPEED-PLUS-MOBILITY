using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ServiceModel.Http2Protocol
{
    [Serializable]
    public class InterruptedConnectionException : Exception
    {
        #region Constructors

        public InterruptedConnectionException(string msg)
            :base(msg)
        {

        }

        #endregion
    }
}
