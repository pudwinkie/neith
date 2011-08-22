using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
    public sealed class ResponseItem : MessageItem
    {
        public ResponseItem(MessageItem mes)
            : base(mes)
        {
        }


        public ResponseItem(MessageItem mes, IResponse res)
            : base(mes)
        {
            Response = res;
        }

        public ResponseItem(MessageItem mes, int errorCode, string errorDescription, params object[] errorInfo)
            : this(mes, CreateError(errorCode, errorDescription, errorInfo))
        {
        }

        private static IResponse CreateError(int errorCode, string errorDescription, object[] errorInfo)
        {
            if (errorInfo != null) {
                var lines = errorInfo
                    .Where(a => a != null)
                    .Select(a => string.Format(" ({0})", a));
                errorDescription += string.Join("", lines);
            }
            var res = new Response(errorCode, errorDescription);
            return res;
        }


    }
}
