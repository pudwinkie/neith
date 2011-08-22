using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.Daemon;
using con = Neith.Growl.Connector;
using daemon = Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
    public sealed class SubscriberItem : MessageItem
    {
        public ISubscriber Subscriber { get; private set; }

        private SubscriberItem(MessageItem mes, ISubscriber subscriber)
            : base(mes)
        {
            Subscriber = subscriber;
        }

        public static SubscriberItem Create(MessageItem item)
        {
            var request = item.Request;
            var mh = item.MessageHandler;
            var subscriber = daemon::Subscriber.FromHeaders(request.Headers);
            subscriber.EndPoint = new IPEndPoint(mh.RemoteIPEndPoint.Address, subscriber.Port);
            subscriber.Key = new SubscriberKey(request.Key, subscriber.ID, request.Key.HashAlgorithm, request.Key.EncryptionAlgorithm);
            var subItem = new SubscriberItem(item, subscriber);
            return subItem;
        }


    }
}
