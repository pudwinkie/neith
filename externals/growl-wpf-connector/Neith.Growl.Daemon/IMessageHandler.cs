using System;
using System.Net;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// 通知元とのやりとりを行うためのインターフェース。
    /// 固有の通信メソッドを隠匿します。
    /// Dispose時にレスポンスを発行して通信処理を終了します。
    /// </summary>
    public interface IMessageHandler : IDisposable
    {
        /// <summary>エラーが出た場合のイベントハンドラ</summary>
        event MessageHandler.MessageHandlerErrorEventHandler Error;

        /// <summary>解釈完了した時のイベントハンドラ</summary>
        event MessageHandler.MessageHandlerMessageParsedEventHandler MessageParsed;

        /// <summary>リクエスト本体</summary>
        IGNTPRequest Request { get; }

        /// <summary>リクエストの受付情報</summary>
        IRequestInfo RequestInfo { get; }

        /// <summary>コールバック情報</summary>
        CallbackInfo CallbackInfo { get; }

        /// <summary>通知元の接続情報</summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>通知元の接続情報(IP接続の場合)</summary>
        IPEndPoint RemoteIPEndPoint { get; }

        /// <summary>レスポンス情報</summary>
        IResponse Response { get; set; }


    }
}
