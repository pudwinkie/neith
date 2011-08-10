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
using System.Collections.Generic;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class AppendTransaction : ImapTransactionBase<ImapCommandResult<ImapAppendedUidSet>>, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (multiple)
          yield return ImapCapability.MultiAppend;
      }
    }

    public AppendTransaction(ImapConnection connection, bool multiple)
      : base(connection)
    {
      this.multiple = multiple;
    }

    // 6.3.11. APPEND Command
    //    Arguments:  mailbox name
    //                OPTIONAL flag parenthesized list
    //                OPTIONAL date/time string
    //                message literal
    //    Responses:  no specific responses for this command
    //    Result:     OK - append completed
    //                NO - append error: can't append to that mailbox, error
    //                     in flags or date/time or message text
    //                BAD - command unknown or arguments invalid

    // http://tools.ietf.org/html/rfc3502
    // RFC 3502 - Internet Message Access Protocol (IMAP) - MULTIAPPEND Extension
    // 6.3.11. APPEND Command
    //    Arguments:  mailbox name
    //                one or more messages to upload, specified as:
    //                   OPTIONAL flag parenthesized list
    //                   OPTIONAL date/time string
    //                   message literal
    //    Data:       no specific responses for this command
    //    Result:     OK - append completed
    //                NO - append error: can't append to that mailbox, error
    //                     in flags or date/time or message text,
    //                     append cancelled
    //                BAD - command unknown or arguments invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name") ||
          !RequestArguments.ContainsKey("messages to upload")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' and 'messages to upload' must be setted");
        return null;
      }
#endif

      return Connection.CreateCommand("APPEND",
                                      RequestArguments["mailbox name"],
                                      RequestArguments["messages to upload"]);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        if (tagged.ResponseText.Code == ImapResponseCode.AppendUid)
          Finish(new ImapCommandResult<ImapAppendedUidSet>(ImapResponseTextConverter.FromAppendUid(tagged.ResponseText),
                                                           tagged.ResponseText));
        else
          FinishOk(tagged);
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private readonly bool multiple;
  }
}