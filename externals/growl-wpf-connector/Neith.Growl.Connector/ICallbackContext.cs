using System;
namespace Neith.Growl.Connector
{
    /// <summary>コールバックコンテキスト</summary>
    public interface ICallbackContext : ICallbackDataBase
    {
        /// <summary>コールバックURL</summary>
        string CallbackUrl { get; }
    }
}
