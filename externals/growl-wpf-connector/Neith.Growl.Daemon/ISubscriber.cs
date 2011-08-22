using System;
using System.Net;
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

        /// <summary>接続先</summary>
        EndPoint EndPoint { get; set; }

        /// <summary>IPアドレス</summary>
        string IPAddress { get; }

        /// <summary>ポート</summary>
        int Port { get; }

        /// <summary>購読Key</summary>
        SubscriberKey Key { get; set; }
    }
}