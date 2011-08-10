using System;
using  Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>購読応答</summary>
    public interface ISubscriptionResponse : IResponse
    {
        /// <summary>有効期間（秒）</summary>
        int TTL { get; }
    }
}
