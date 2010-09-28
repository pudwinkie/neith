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

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Transaction.BuiltIn {
  internal sealed class StoreTransaction :
    FetchStoreTransactionBase<ImapMessageAttribute>,
    IImapExtension
  {
    ImapCapability IImapExtension.RequiredCapability {
      get { return storeModifiersCapabilityRequirement; }
    }

    public StoreTransaction(ImapConnection connection, bool uid, ImapCapability storeModifiersCapabilityRequirement)
      : base(connection)
    {
      this.uid = uid;
      this.storeModifiersCapabilityRequirement = storeModifiersCapabilityRequirement;
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("message set"))
        return ProcessArgumentNotSetted;
      else if (!RequestArguments.ContainsKey("message data item name"))
        return ProcessArgumentNotSetted;
      else if (!RequestArguments.ContainsKey("value for message data item"))
        return ProcessArgumentNotSetted;
      else
#endif
        return ProcessStore;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'message set', 'message data item name' and 'value for message data item' must be setted");
    }
#endif

    // RFC 4466 - Collected Extensions to IMAP4 ABNF
    // http://tools.ietf.org/html/rfc4466
    // 2.5. Extensions to STORE and UID STORE Commands
    //    Arguments:  message set
    //                OPTIONAL store modifiers
    //                message data item name
    //                value for message data item
    //    Responses:  untagged responses: FETCH
    //    Result:     OK - store completed
    //                NO - store error: cannot store that data
    //                BAD - command unknown or arguments invalid
    private void ProcessStore()
    {
      // STORE / UID STORE
      ImapString storeModifiers;

      if (RequestArguments.TryGetValue("store modifiers", out storeModifiers))
        SendCommand(uid ? "UID STORE" : "STORE",
                    ProcessReceiveResponse,
                    RequestArguments["message set"],
                    storeModifiers,
                    RequestArguments["message data item name"],
                    RequestArguments["value for message data item"]);
      else
        SendCommand(uid ? "UID STORE" : "STORE",
                    ProcessReceiveResponse,
                    RequestArguments["message set"],
                    RequestArguments["message data item name"],
                    RequestArguments["value for message data item"]);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok || tagged.Condition == ImapResponseCondition.No) {
        var result = CreateResult(tagged);

        // XXX
        if (tagged.Condition == ImapResponseCondition.Ok)
          result.Code = ImapCommandResultCode.Ok;
        else
          result.Code = ImapCommandResultCode.No;

        Finish(result);
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private readonly bool uid;
    private /* readonly */ ImapCapability storeModifiersCapabilityRequirement;
  }
}