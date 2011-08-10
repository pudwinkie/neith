using System;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>Growlからの通知を受け取る購読者</summary>
    public interface ISubscriber : IExtensibleObject
    {
        /// <summary>ID</summary>
        string ID { get; set; }

        /// <summary>名称</summary>
        string Name { get; set; }

        /// <summary>IPアドレス</summary>
        string IPAddress { get; set; }

        /// <summary>ポート</summary>
        int Port { get; set; }

        /// <summary>購読Key</summary>
        SubscriberKey Key { get; set; }
    }
}
