// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal abstract class SubscribeTransactionBase : ImapTransactionBase {
    protected SubscribeTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name"))
        return ProcessArgumentNotSetted;
#endif

      return ProcessSubscribe;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' must be setted");
    }
#endif

    // 6.3.6. SUBSCRIBE Command
    //    Arguments:  mailbox
    //    Responses:  no specific responses for this command
    //    Result:     OK - subscribe completed
    //                NO - subscribe failure: can't subscribe to that name
    //                BAD - command unknown or arguments invalid

    // 6.3.7. UNSUBSCRIBE Command
    //    Arguments:  mailbox name
    //    Responses:  no specific responses for this command
    //    Result:     OK - unsubscribe completed
    //                NO - unsubscribe failure: can't unsubscribe that name
    //                BAD - command unknown or arguments invalid
    private void ProcessSubscribe()
    {
      if (this is SubscribeTransaction)
        SendCommand("SUBSCRIBE",
                    ProcessReceiveResponse,
                    RequestArguments["mailbox name"]);
      else if (this is UnsubscribeTransaction)
        SendCommand("UNSUBSCRIBE",
                    ProcessReceiveResponse,
                    RequestArguments["mailbox name"]);
    }
  }
}