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
  internal sealed class ExpungeTransaction : ImapTransactionBase<ImapCommandResult<long[]>>, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (uid)
          yield return ImapCapability.UidPlus;
      }
    }

    public ExpungeTransaction(ImapConnection connection, bool uid)
      : base(connection)
    {
      this.uid = uid;
    }

    // 6.4.3. EXPUNGE Command
    //    Arguments:  none
    //    Responses:  untagged responses: EXPUNGE
    //    Result:     OK - expunge completed
    //                NO - expunge failure: can't expunge (e.g., permission
    //                     denied)
    //                BAD - command unknown or arguments invalid

    // http://tools.ietf.org/html/rfc4315
    // RFC 4315 - Internet Message Access Protocol (IMAP) - UIDPLUS extension
    // 2.1. UID EXPUNGE Command
    //    Arguments:  sequence set
    //    Data:       untagged responses: EXPUNGE
    //    Result:     OK - expunge completed
    //                NO - expunge failure (e.g., permission denied)
    //                BAD - command unknown or arguments invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (uid && !RequestArguments.ContainsKey("sequence set")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'sequence set' must be setted");
        return null;
      }
#endif

      // EXPUNGE / UID EXPUNGE
      if (uid)
        return Connection.CreateCommand("UID EXPUNGE",
                                        RequestArguments["sequence set"]);
      else
        return Connection.CreateCommand("EXPUNGE");
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Expunge)
        expunged.Add(ImapDataResponseConverter.FromExpunge(data));

      base.OnDataResponseReceived (data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        Finish(new ImapCommandResult<long[]>(expunged.ToArray(), tagged.ResponseText));
        expunged.Clear();
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

      private readonly bool uid;
    private List<long> expunged = new List<long>();
  }
}
