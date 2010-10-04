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
  /*
   * RFC 5255 - Internet Message Access Protocol Internationalization
   * http://tools.ietf.org/html/rfc5255
   */
  internal sealed class ComparatorTransaction :
    ImapTransactionBase<ImapCommandResult<Tuple<ImapCollationAlgorithm, ImapCollationAlgorithm[]>>>,
    IImapExtension
  {
    ImapCapability IImapExtension.RequiredCapability {
      get { return ImapCapability.I18NLevel2; }
    }

    public ComparatorTransaction(ImapConnection connection)
      : base(connection)
    {
    }

    protected override ProcessTransactionDelegate Reset()
    {
      return ProcessComparator;
    }

    /*
     * 4.7. COMPARATOR Command
     *    Arguments: Optional comparator order arguments.
     *    Response:  A possible COMPARATOR response (see Section 4.8).
     *    Result:    OK - Command completed
     *               NO - No matching comparator found
     *               BAD - Arguments invalid
     */
    private void ProcessComparator()
    {
      ImapString comparatorOrder;

      // COMPARATOR
      if (RequestArguments.TryGetValue("comparator order arguments", out comparatorOrder))
        SendCommand("COMPARATOR", ProcessReceiveResponse, comparatorOrder);
      else
        SendCommand("COMPARATOR", ProcessReceiveResponse);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Comparator)
        activeComparator = ImapDataResponseConverter.FromComparator(data, out matchingComparators);

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        Finish(new ImapCommandResult<Tuple<ImapCollationAlgorithm, ImapCollationAlgorithm[]>>(Tuple.Create(activeComparator, matchingComparators),
                                                                                              tagged.ResponseText));
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private ImapCollationAlgorithm activeComparator;
    private ImapCollationAlgorithm[] matchingComparators;
  }
}
