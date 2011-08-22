using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
    public sealed class NotificationItem : MessageItem
    {
        public INeithNotification Notification { get; private set; }

        public NotificationItem(MessageItem mes, INeithNotification notification)
            : base(mes)
        {
            Notification = notification;
        }
    }
}