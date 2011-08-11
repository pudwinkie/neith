using System;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    public interface IMessageHandler
    {
        CallbackInfo CallbackInfo { get; }
        event MessageHandler.MessageHandlerErrorEventHandler Error;
        void Log(Data data);
        event MessageHandler.MessageHandlerMessageParsedEventHandler MessageParsed;
        IGNTPRequest Request { get; }
        IRequestInfo RequestInfo { get; }
        void WriteError(int errorCode, string errorMessage, params object[] args);
        void WriteResponse(MessageBuilder mb, bool requestComplete);
    }
}
