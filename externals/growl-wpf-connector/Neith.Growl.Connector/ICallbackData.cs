using System;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>コールバック情報</summary>
    public interface ICallbackData : ICallbackDataBase
    {
        /// <summary>通知ID</summary>
        string NotificationID { get; }

        /// <summary>コールバック応答</summary>
        CallbackResult Result { get; }
    }
}