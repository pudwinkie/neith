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

        private NotificationItem(MessageItem mes, INeithNotification notification)
            : base(mes)
        {
            Notification = notification;
        }

        public static NotificationItem Create(MessageItem item)
        {
            var request = item.Request;
            var mh = item.MessageHandler;
            var notification = NeithNotificationModel.FromHeaders(request.Headers);
            mh.CallbackInfo.NotificationID = notification.ID;
            var noteItem = new NotificationItem(item, notification);
            return noteItem;
        }


    }
}