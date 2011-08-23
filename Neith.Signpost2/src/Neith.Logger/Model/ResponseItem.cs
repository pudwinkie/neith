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
        public ResponseItem(MessageItem item)
            : base(item)
        {
        }


        public ResponseItem(MessageItem item, IResponse res)
            : base(item)
        {
            Response = res;
        }

        private ResponseItem(MessageItem item, int errorCode, string errorDescription, params object[] errorInfo)
            : this(item, CreateError(errorCode, errorDescription, errorInfo))
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

        public static ResponseItem Create(MessageItem item, GrowlException ex)
        {
            return new ResponseItem(item, ex.ErrorCode, ex.Message, ex.AdditionalInfo);
        }

        public static ResponseItem Create(MessageItem item, Exception ex)
        {
            return new ResponseItem(item, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
        }

    }
}
