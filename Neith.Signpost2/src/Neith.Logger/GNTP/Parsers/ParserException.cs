using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Logger.GNTP;
using Neith.Growl.Connector;

namespace Neith.Logger.GNTP.Parsers
{
    /// <summary>
    /// ParserException
    /// </summary>
    public class ParserException : GNTPException
    {
        public ParserException(ErrorType errorType)
            : base(errorType) { }

        public ParserException(ErrorType errorType, string message)
            : base(errorType, message) { }

        public ParserException(ErrorType errorType, string message, Exception innerException)
            : base(errorType, message, innerException) { }
    }

    /// <summary>
    /// 不適切な文字、または並びの検出。
    /// </summary>
    public class InvalidCharException : InvalidOperationException
    {
        public InvalidCharException()
            : base() { }

        public InvalidCharException(string message)
            : base(message) { }

        public InvalidCharException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}
