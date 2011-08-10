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
  internal abstract class SelectTransactionBase : ImapTransactionBase, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (selectParametersCapabilityRequirement != null)
          yield return selectParametersCapabilityRequirement;
      }
    }

    protected SelectTransactionBase(ImapConnection connection, ImapMailbox selectingMailbox, ImapCapability selectParametersCapabilityRequirement)
      : base(connection)
    {
      this.selectingMailbox = selectingMailbox;
      this.selectParametersCapabilityRequirement = selectParametersCapabilityRequirement;
    }

    // 6.3.1. SELECT Command
    //    Arguments:  mailbox name
    //    Responses:  REQUIRED untagged responses: FLAGS, EXISTS, RECENT
    //                REQUIRED OK untagged responses:  UNSEEN,  PERMANENTFLAGS,
    //                UIDNEXT, UIDVALIDITY
    //    Result:     OK - select completed, now in selected state
    //                NO - select failure, now in authenticated state: no
    //                     such mailbox, can't access mailbox
    //                BAD - command unknown or arguments invalid

    // 6.3.2. EXAMINE Command
    //    Arguments:  mailbox name
    //    Responses:  REQUIRED untagged responses: FLAGS, EXISTS, RECENT
    //                REQUIRED OK untagged responses:  UNSEEN,  PERMANENTFLAGS,
    //                UIDNEXT, UIDVALIDITY
    //    Result:     OK - examine completed, now in selected state
    //                NO - examine failure, now in authenticated state: no
    //                     such mailbox, can't access mailbox
    //                BAD - command unknown or arguments invalid
    protected override ImapCommand PrepareCommand()
    {
#if DEBUG
      if (!RequestArguments.ContainsKey("mailbox name")) {
        FinishError(ImapCommandResultCode.RequestError, "arguments 'mailbox name' must be setted");
        return null;
      }
#endif

      ImapString selectParameters;

      if (RequestArguments.TryGetValue("select parameters", out selectParameters))
        return Connection.CreateCommand((this is SelectTransaction) ? "SELECT" : "EXAMINE",
                                        RequestArguments["mailbox name"],
                                        selectParameters);
      else
        return Connection.CreateCommand((this is SelectTransaction) ? "SELECT" : "EXAMINE",
                                        RequestArguments["mailbox name"]);
    }

    protected override void OnDataResponseReceived(ImapDataResponse data)
    {
      if (data.Type == ImapDataResponseType.Flags)
        selectingMailbox.ApplicableFlags = ImapDataResponseConverter.FromFlags(data);
      if (data.Type == ImapDataResponseType.Exists)
        selectingMailbox.ExistsMessage = ImapDataResponseConverter.FromExists(data);
      else if (data.Type == ImapDataResponseType.Recent)
        selectingMailbox.RecentMessage = ImapDataResponseConverter.FromRecent(data);

      base.OnDataResponseReceived(data);
    }

    protected override void OnStatusResponseReceived(ImapStatusResponse status)
    {
      if (status.Condition == ImapResponseCondition.Ok) {
        if (status.ResponseText.Code == ImapResponseCode.Unseen)
          selectingMailbox.FirstUnseen = ImapResponseTextConverter.FromUnseen(status.ResponseText);
        else if (status.ResponseText.Code == ImapResponseCode.PermanentFlags)
          selectingMailbox.PermanentFlags = ImapResponseTextConverter.FromPermanentFlags(status.ResponseText);
        else if (status.ResponseText.Code == ImapResponseCode.UidNext)
          selectingMailbox.UidNext = ImapResponseTextConverter.FromUidNext(status.ResponseText);
        else if (status.ResponseText.Code == ImapResponseCode.UidValidity)
          selectingMailbox.UidValidity = ImapResponseTextConverter.FromUidValidity(status.ResponseText);
        else if (status.ResponseText.Code == ImapResponseCode.ReadOnly)
          selectingMailbox.ReadOnly = true;
        else if (status.ResponseText.Code == ImapResponseCode.ReadWrite)
          selectingMailbox.ReadOnly = false;
        /*
         * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
         * http://tools.ietf.org/html/rfc4551
         * 3. IMAP Protocol Changes
         * 3.1. New OK Untagged Responses for SELECT and EXAMINE
         */
        else if (status.ResponseText.Code == ImapResponseCode.HighestModSeq) {
          selectingMailbox.ModificationSequences = true;
          selectingMailbox.HighestModSeq = ImapResponseTextConverter.FromHighestModSeq(status.ResponseText);
        }
        else if (status.ResponseText.Code == ImapResponseCode.NoModSeq)
          selectingMailbox.ModificationSequences = false;
      }
      else if (status.Condition == ImapResponseCondition.No) {
        if (status.ResponseText.Code == ImapResponseCode.UidNotSticky)
          selectingMailbox.UidPersistent = false;
      }

      base.OnStatusResponseReceived(status);
    }

    protected override void OnTaggedStatusResponseReceived(ImapTaggedStatusResponse tagged)
    {
      if (tagged.Condition == ImapResponseCondition.Ok)
        FinishOk(tagged);
      else
        base.OnTaggedStatusResponseReceived(tagged);
    }

    private /*readonly*/ ImapMailbox selectingMailbox;
    private /*readonly*/ ImapCapability selectParametersCapabilityRequirement;
  }
}
