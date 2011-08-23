using System;
using System.Net;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// 通知元とのやりとりを行うためのインターフェース。
    /// 固有の通信メソッドを隠匿します。
    /// </summary>
    public interface IMessageHandler
    {
        CallbackInfo CallbackInfo { get; }
        event MessageHandler.MessageHandlerErrorEventHandler Error;
        void Log(Data data);
        event MessageHandler.MessageHandlerMessageParsedEventHandler MessageParsed;
        IGNTPRequest Request { get; }
        IRequestInfo RequestInfo { get; }

        EndPoint RemoteEndPoint { get; }
        IPEndPoint RemoteIPEndPoint { get; }

        void WriteError(int errorCode, string errorMessage, params object[] args);
        void WriteResponse(MessageBuilder mb, bool requestComplete);


    }
}
