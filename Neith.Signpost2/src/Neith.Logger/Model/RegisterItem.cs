using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using con = Neith.Growl.Connector;
using Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
    public sealed class RegisterItem : MessageItem
    {
        public IApplication Application { get; private set; }
        public IList<INotificationType> NotificationTypes { get; private set; }

        private RegisterItem(
            MessageItem mes, IApplication app,
            IList<INotificationType> notificationTypes)
            : base(mes)
        {
            Application = app;
            NotificationTypes = notificationTypes;
        }

        public static RegisterItem Create(MessageItem item)
        {
            var request = item.Request;
            var app = con::Application.FromHeaders(request.Headers);
            var notificationTypes = new List<INotificationType>();
            foreach (var headers in request.NotificationsToBeRegistered) {
                notificationTypes.Add(NotificationType.FromHeaders(headers));
            }
            var regItem = new RegisterItem(item, app, notificationTypes);
            return regItem;
        }


    }
}
