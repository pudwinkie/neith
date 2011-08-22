using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
    public sealed class RegisterItem : MessageItem
    {
        public IApplication Application { get; private set; }
        public IList<INotificationType> NotificationTypes { get; private set; }

        public RegisterItem(
            MessageItem mes, IApplication app,
            IList<INotificationType> notificationTypes)
            : base(mes)
        {
            Application = app;
            NotificationTypes = notificationTypes;
        }
    }
}
