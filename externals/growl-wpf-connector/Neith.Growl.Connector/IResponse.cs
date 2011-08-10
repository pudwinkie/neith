using System;
using System.Collections.Generic;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>レスポンス情報</summary>
    public interface IResponse : IError
    {
        /// <summary>レスポンス元</summary>
        string InResponseTo { get; set; }

        /// <summary>コールバックがあるならtrue</summary>
        bool IsCallback { get; }

        /// <summary>エラーがあるならtrue</summary>
        bool IsError { get; }

        /// <summary>OKならtrue</summary>
        bool IsOK { get; set; }

        /// <summary>リクエスト情報</summary>
        RequestData RequestData { get; set; }

        /// <summary>コールバック情報</summary>
        ICallbackData CallbackData { get; set; }

        /// <summary>コールバック情報を設定します。</summary>
        void SetCallbackData(string notificationID, ICallbackContext callbackContext, CallbackResult callbackResult);
    }
}
