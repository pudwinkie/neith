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
using System.Collections.Generic;
using System.Text;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class ThreadTransaction : ImapTransactionBase<ImapCommandResult<ImapThreadList>> {
    public ThreadTransaction(ImapConnection connection, bool uid)
      : base(connection)
    {
      this.uid = uid;
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("threading algorithm"))
        return ProcessArgumentNotSetted;
      if (!RequestArguments.ContainsKey("charset specification"))
        return ProcessArgumentNotSetted;
      if (!RequestArguments.ContainsKey("searching criteria"))
        return ProcessArgumentNotSetted;
#endif

      return ProcessThread;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'threading algorithm', 'charset specification' and 'searching criteria' must be setted");
    }
#endif

    // BASE.6.4.THREAD. THREAD Command
    // Arguments:  threading algorithm
    //             charset specification
    //             searching criteria (one or more)
    // Data:       untagged responses: THREAD
    // Result:     OK - thread completed
    //             NO - thread error: can't thread that charset or
    //                  criteria
    //             BAD - command unknown or arguments invalid
    private void ProcessThread()
    {
      // THREAD / UID THREAD
      SendCommand(uid ? "UID THREAD" : "THREAD",
                  ProcessReceiveResponse,
                  RequestArguments["threading algorithm"],
                  RequestArguments["charset specification"],
                  RequestArguments["searching criteria"]);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Thread)
        threadList = ImapDataResponseConverter.FromThread(data, uid);

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult<ImapThreadList>(threadList, tagged.ResponseText));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private readonly bool uid;
    private ImapThreadList threadList = null;
  }
}
