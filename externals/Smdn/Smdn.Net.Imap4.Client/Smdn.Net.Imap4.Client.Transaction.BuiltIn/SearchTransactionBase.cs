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
  internal abstract class SearchTransactionBase : ImapTransactionBase<ImapCommandResult<ImapMatchedSequenceSet>>, IImapExtension {
    protected virtual ImapCapability RequiredCapability {
      get { return resultSpecifierCapabilityRequirement; }
    }

    ImapCapability IImapExtension.RequiredCapability {
      get { return RequiredCapability; }
    }

    protected SearchTransactionBase(ImapConnection connection, bool uid, ImapCapability resultSpecifierCapabilityRequirement)
      : base(connection)
    {
      this.uid = uid;
      this.resultSpecifierCapabilityRequirement = resultSpecifierCapabilityRequirement;
    }

    protected override ProcessTransactionDelegate Reset()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("searching criteria"))
        return ProcessArgumentNotSetted;
#endif

      if (this is SearchTransaction) {
        return ProcessSearch;
      }
      else {
#if DEBUG
        // http://tools.ietf.org/html/rfc5256
        // BASE.6.4.SORT. SORT Command
        //   The charset argument is mandatory (unlike SEARCH)
        if (!RequestArguments.ContainsKey("charset specification"))
          return ProcessArgumentNotSetted;
        if (!RequestArguments.ContainsKey("sort criteria"))
          return ProcessArgumentNotSetted;
#endif

        return ProcessSearch;
      }
    }

#if DEBUG
    private void ProcessArgumentNotSetted()
    {
      if (this is SearchTransaction)
        FinishError(ImapCommandResultCode.RequestError, "arguments 'searching criteria' must be setted");
      else
        FinishError(ImapCommandResultCode.RequestError, "arguments 'sort criteria', 'charset specification' and 'searching criteria' must be setted");
    }
#endif

    // RFC 4466 - Collected Extensions to IMAP4 ABNF
    // http://tools.ietf.org/html/rfc4466
    // 2.6. Extensions to SEARCH Command
    // 2.6.1. Extended SEARCH Command
    //    Arguments:  OPTIONAL result specifier
    //                OPTIONAL [CHARSET] specification
    //                searching criteria (one or more)
    //    Responses:  REQUIRED untagged response: SEARCH (*)
    //    Result:     OK - search completed
    //                NO - search error: cannot search that [CHARSET] or
    //                     criteria
    //                BAD - command unknown or arguments invalid

    // http://tools.ietf.org/html/rfc5256
    // BASE.6.4.SORT. SORT Command
    //    Arguments:  sort program
    //                charset specification
    //                searching criteria (one or more)
    //    Data:       untagged responses: SORT
    //    Result:     OK - sort completed
    //                NO - sort error: can't sort that charset or
    //                     criteria
    //                BAD - command unknown or arguments invalid
    private void ProcessSearch()
    {
      if (this is SearchTransaction) {
        // SEARCH / UID SEARCH
        var args = new List<ImapString>();

        if (RequestArguments.ContainsKey("result specifier")) {
          args.Add("RETURN");
          args.Add(RequestArguments["result specifier"]);
        }

        if (RequestArguments.ContainsKey("charset specification")) {
          args.Add("CHARSET");
          args.Add(RequestArguments["charset specification"]);
        }

        args.Add(RequestArguments["searching criteria"]);

        SendCommand(uid ? "UID SEARCH" : "SEARCH",
                    ProcessReceiveResponse,
                    args.ToArray());
      }
      else {
        // SORT / UID SORT
        ImapString resultSpecifier;

        if (RequestArguments.TryGetValue("result specifier", out resultSpecifier))
          SendCommand(uid ? "UID SORT" : "SORT",
                      ProcessReceiveResponse,
                      "RETURN",
                      resultSpecifier,
                      RequestArguments["sort criteria"],
                      RequestArguments["charset specification"],
                      RequestArguments["searching criteria"]);
        else
          SendCommand(uid ? "UID SORT" : "SORT",
                      ProcessReceiveResponse,
                      RequestArguments["sort criteria"],
                      RequestArguments["charset specification"],
                      RequestArguments["searching criteria"]);
      }
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      // 7.2.5 SEARCH Response
      //   Contents:   zero or more numbers
      //   Example:    S: * SEARCH 2 3 6
      if (this is SearchTransaction && data.Type == ImapDataResponseType.Search)
        messages = ImapDataResponseConverter.FromSearch(data, uid);
      // http://tools.ietf.org/html/rfc5256
      // BASE.7.2.SORT. SORT Response
      //   Data:       zero or more numbers
      //   Example:    S: * SORT 2 3 6
      else if (this is SortTransaction && data.Type == ImapDataResponseType.Sort)
        messages = ImapDataResponseConverter.FromSort(data, uid);
      // http://tools.ietf.org/html/rfc4466
      // 2.6.2. ESEARCH untagged response
      //    Contents:   one or more search-return-data pairs
      else if (data.Type == ImapDataResponseType.ESearch)
        messages = ImapDataResponseConverter.FromESearch(data);

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult<ImapMatchedSequenceSet>(ImapCommandResultCode.Ok,
                                                             messages,
                                                             tagged.ResponseText));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private readonly bool uid;
    private /*readonly*/ ImapCapability resultSpecifierCapabilityRequirement;
    private ImapMatchedSequenceSet messages = null;
  }
}
