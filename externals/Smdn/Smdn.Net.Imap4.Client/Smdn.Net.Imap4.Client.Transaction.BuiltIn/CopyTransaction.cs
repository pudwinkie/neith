// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class CopyTransaction : ImapTransactionBase<ImapCommandResult<ImapCopiedUidSet>> {
    public CopyTransaction(ImapConnection connection, bool uid)
      : base(connection)
    {
      this.uid = uid;
    }

    // 6.4.7. COPY Command
    //    Arguments:  sequence set
    //                mailbox name
    //    Responses:  no specific responses for this command
    //    Result:     OK - copy completed
    //                NO - copy error: can't copy those messages or to that
    //                     name
    //                BAD - command unknown or arguments invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name") ||
          !RequestArguments.ContainsKey("sequence set")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' and 'sequence set' must be setted");
        return null;
      }
#endif

      // COPY / UID COPY
      return Connection.CreateCommand(uid ? "UID COPY" : "COPY",
                                      RequestArguments["sequence set"],
                                      RequestArguments["mailbox name"]);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        if (tagged.ResponseText.Code == ImapResponseCode.CopyUid)
          Finish(new ImapCommandResult<ImapCopiedUidSet>(ImapResponseTextConverter.FromCopyUid(tagged.ResponseText),
                                                         tagged.ResponseText));
        else
          FinishOk(tagged);
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private readonly bool uid;
  }
}
