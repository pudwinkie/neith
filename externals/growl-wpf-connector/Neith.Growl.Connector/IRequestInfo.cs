using System;
using System.Collections.Generic;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// リクエスト受付情報。
    /// </summary>
    public interface IRequestInfo
    {
        /// <summary>受付UTC時刻</summary>
        DateTime TimeReceived { get; }

        /// <summary>送信者アドレス</summary>
        string ReceivedFrom { get; set; }

        /// <summary>受信者アドレス</summary>
        string ReceivedBy { get; set; }

        /// <summary>受信サーバ</summary>
        string ReceivedWith { get; set; }

        /// <summary>リクエストID(GUID)</summary>
        string RequestID { get; }

        /// <summary>前回の受信情報</summary>
        List<IHeader> PreviousReceivedHeaders { get; }

        /// <summary>リクエストに対する処理情報ログ</summary>
        List<string> HandlingInfo { get; }
    }
}
