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
  internal sealed class FetchTransaction<TMessageAttribute> :
    FetchStoreTransactionBase<TMessageAttribute>,
    IImapExtension
    where TMessageAttribute : ImapMessageAttributeBase
  {
    ImapCapability IImapExtension.RequiredCapability {
      get { return fetchModifiersCapabilityRequirement; }
    }

    public FetchTransaction(ImapConnection connection, bool uid, ImapCapability fetchModifiersCapabilityRequirement)
      : base(connection)
    {
      this.uid = uid;
      this.fetchModifiersCapabilityRequirement = fetchModifiersCapabilityRequirement;
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("sequence set"))
        return ProcessArgumentNotSetted;
      else if (!RequestArguments.ContainsKey("message data item names or macro"))
        return ProcessArgumentNotSetted;
      else
#endif
        return ProcessFetch;
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      FinishError(ImapCommandResultCode.RequestError, "arguments 'sequence set' and 'message data item names or macro' must be setted");
    }
#endif

    // RFC 4466 - Collected Extensions to IMAP4 ABNF
    // http://tools.ietf.org/html/rfc4466
    // 2.4. Extensions to FETCH and UID FETCH Commands
    //    Arguments:  sequence set
    //                message data item names or macro
    //                OPTIONAL fetch modifiers
    //    Responses:  untagged responses: FETCH
    //    Result:     OK - fetch completed
    //                NO - fetch error: cannot fetch that data
    //                BAD - command unknown or arguments invalid
    private void ProcessFetch()
    {
      // FETCH / UID FETCH
      ImapString fetchModifiers;

      if (RequestArguments.TryGetValue("fetch modifiers", out fetchModifiers))
        SendCommand(uid ? "UID FETCH" : "FETCH",
                    ProcessReceiveResponse,
                    RequestArguments["sequence set"],
                    RequestArguments["message data item names or macro"],
                    fetchModifiers);
      else
        SendCommand(uid ? "UID FETCH" : "FETCH",
                    ProcessReceiveResponse,
                    RequestArguments["sequence set"],
                    RequestArguments["message data item names or macro"]);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(CreateResult(tagged));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private readonly bool uid;
    private /*readonly*/ ImapCapability fetchModifiersCapabilityRequirement;
  }
}
