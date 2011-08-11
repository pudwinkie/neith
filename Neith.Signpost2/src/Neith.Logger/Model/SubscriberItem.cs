using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.Daemon;

namespace Neith.Logger.Model
{
  public sealed  class SubscriberItem : MessageItem
    {
      public ISubscriber Subscriber { get; private set; }

        public SubscriberItem(IMessageHandler mh, ISubscriber subscriber)
            :base(mh)
        {
            Subscriber = subscriber;
        }
    }
}
