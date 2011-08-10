using System.Collections.Generic;
using System;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>リクエスト情報</summary>
    public interface IGNTPRequest
    {
        /// <summary>アプリケーション名</summary>
        string ApplicationName { get; }

        /// <summary>バージョン</summary>
        string Version { get; }

        /// <summary>リクエストタイプ</summary>
        RequestType Directive { get; }

        /// <summary>暗号キー</summary>
        Key Key { get; }

        /// <summary>ヘッダ情報</summary>
        HeaderCollection Headers { get; }

        /// <summary>登録に必要な全ての情報</summary>
        List<HeaderCollection> NotificationsToBeRegistered { get; }

        /// <summary>コールバック情報</summary>
        ICallbackContext CallbackContext { get; }
    }
}
