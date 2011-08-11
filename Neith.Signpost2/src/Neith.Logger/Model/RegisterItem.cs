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

        public RegisterItem(IMessageHandler mh, IApplication app)
            :base(mh)
        {
            Application = app;
        }
    }
}
