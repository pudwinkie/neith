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
  internal abstract class ListTransactionBase : ImapTransactionBase<ImapCommandResult<ImapMailboxList[]>> {
    protected ListTransactionBase(ImapConnection connection)
      : base(connection)
    {
    }

    /*
     * 6.3.8. LIST Command
     *    Arguments:  reference name
     *                mailbox name with possible wildcards
     *    Responses:  untagged responses: LIST
     *    Result:     OK - list completed
     *                NO - list failure: can't list that reference or name
     *                BAD - command unknown or arguments invalid
     *
     * 6.3.9. LSUB Command
     *    Arguments:  reference name
     *                mailbox name with possible wildcards
     *    Responses:  untagged responses: LSUB
     *    Result:     OK - lsub completed
     *                NO - lsub failure: can't list that reference or name
     *                BAD - command unknown or arguments invalid
     */

    /*
     * http://tools.ietf.org/html/rfc2193
     * RFC 2193 IMAP4 Mailbox Referrals
     * 
     * 5.1 RLIST command
     *   Arguments:  reference name
     *               mailbox name with possible wildcards
     *   Responses:  untagged responses: LIST
     *   Result:     OK - RLIST Completed
     *               NO - RLIST Failure
     *               BAD - command unknown or arguments invalid
     * 
     *   The RLIST command behaves identically to its LIST counterpart, except
     *   remote mailboxes are returned in addition to local mailboxes in the
     *   LIST responses.
     * 
     * 5.2 RLSUB Command
     *    Arguments:  reference name
     *                mailbox name with possible wildcards
     *    Responses:  untagged responses: LSUB
     *    Result:     OK - RLSUB Completed
     *                NO - RLSUB Failure
     *                BAD - command unknown or arguments invalid
     * 
     *    The RLSUB command behaves identically to its LSUB counterpart, except
     *    remote mailboxes are returned in addition to local mailboxes in the
     *    LSUB responses.
     */

    /*
     * http://tools.ietf.org/html/rfc5258
     * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
     * 3. Extended LIST Command
     *    The LIST command syntax is also extended in two additional ways: by
     *    adding a parenthesized list of command options between the command
     *    name and the reference name (LIST selection options) and an optional
     *    list of options at the end that control what kind of information
     *    should be returned (LIST return options).
     */
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("reference name") ||
          !RequestArguments.ContainsKey("mailbox name")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'reference name' and 'mailbox name' must be setted");
        return null;
      }
#endif

      if (this is ListExtendedTransaction) {
        var args = new List<ImapString>(5);

        if (RequestArguments.ContainsKey("selection options"))
          args.Add(RequestArguments["selection options"]);

        args.Add(RequestArguments["reference name"]);
        args.Add(RequestArguments["mailbox name"]);

        if (RequestArguments.ContainsKey("return options")) {
          args.Add("RETURN");
          args.Add(RequestArguments["return options"]);
        }

        return Connection.CreateCommand("LIST",
                                        args.ToArray());
      }
      else {
        string commandString = null;

        if (this is ListTransaction)
          commandString = "LIST";
        else if (this is LsubTransaction)
          commandString = "LSUB";
        else if (this is RListTransaction)
          commandString = "RLIST";
        else if (this is RLsubTransaction)
          commandString = "RLSUB";
        else if (this is XListTransaction)
          commandString = "XLIST";

        return Connection.CreateCommand(commandString,
                                        RequestArguments["reference name"],
                                        RequestArguments["mailbox name"]);
      }
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (this is LsubTransaction || this is RLsubTransaction) {
        if (data.Type == ImapDataResponseType.Lsub)
          mailboxLists.Add(ImapDataResponseConverter.FromLsub(data));
      }
      else {
        if (data.Type == ImapDataResponseType.List)
          mailboxLists.Add(ImapDataResponseConverter.FromList(data));
        else if (data.Type == ImapDataResponseType.XList)
          /*
           * Gimap XLIST capability extension
           */
          mailboxLists.Add(ImapDataResponseConverter.FromXList(data));
      }

      base.OnDataResponseReceived(data);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok) {
        Finish(new ImapCommandResult<ImapMailboxList[]>(mailboxLists.ToArray(), tagged.ResponseText));
        mailboxLists.Clear();
      }
      else {
        base.OnTaggedStatusResponseReceived(tagged);
      }
    }

    private /*readonly*/ List<ImapMailboxList> mailboxLists = new List<ImapMailboxList>();
  }
}
