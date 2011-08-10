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
  internal sealed class StatusTransaction : ImapTransactionBase<ImapCommandResult<ImapStatusAttributeList>> {
    public StatusTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    // 6.3.10. STATUS Command
    //    Arguments:  mailbox name
    //                status data item names
    //    Responses:  untagged responses: STATUS
    //    Result:     OK - status completed
    //                NO - status failure: no status for that name
    //                BAD - command unknown or arguments invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name") ||
          !RequestArguments.ContainsKey("status data item names")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' and 'status data item names' must be setted");
        return null;
      }
#endif

      return Connection.CreateCommand("STATUS",
                                      RequestArguments["mailbox name"],
                                      RequestArguments["status data item names"]);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Status) {
        string mailboxName;
        var att = ImapDataResponseConverter.FromStatus(data, out mailboxName);

        if (RequestArguments["mailbox name"].Equals(mailboxName))
          statusAtt = att;
      }

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult<ImapStatusAttributeList>(statusAtt, tagged.ResponseText));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private /*readonly*/ ImapStatusAttributeList statusAtt = new ImapStatusAttributeList();
  }
}
