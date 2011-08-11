using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
    public class MessageItem
    {
        public IMessageHandler MessageHandler { get; private set; }

        public IResponse Response { get; set; }

        public IRequestInfo RequestInfo { get { return MessageHandler.RequestInfo; } }

        public IGNTPRequest Request { get { return MessageHandler.Request; } }


        public MessageItem(IMessageHandler mh)
        {
            MessageHandler = mh;
        }

    }
}
