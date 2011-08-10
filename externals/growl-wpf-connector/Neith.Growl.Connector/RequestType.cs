using System;

namespace Neith.Growl.Connector
{
    /// <summary>リクエストのタイプ</summary>
    public enum RequestType
    {
        /// <summary>アプリケーションの登録と通知タイプの設定</summary>
        REGISTER,

        /// <summary>通知</summary>
        NOTIFY,

        /// <summary>購買要求</summary>
        SUBSCRIBE
    }
}
