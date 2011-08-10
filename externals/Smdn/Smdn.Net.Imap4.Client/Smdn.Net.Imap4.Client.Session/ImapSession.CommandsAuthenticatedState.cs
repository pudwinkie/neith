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
using System.IO;
#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Net.Imap4.Client.Transaction;
using Smdn.Net.Imap4.Client.Transaction.BuiltIn;

namespace Smdn.Net.Imap4.Client.Session {
  partial class ImapSession {
    /*
     * transaction methods : authenticated state
     */

    /// <summary>sends SELECT command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Select(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return Select(mailbox.Name);
    }

    /// <summary>sends SELECT command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Select(string mailboxName)
    {
      return SelectExamineInternal(false, mailboxName, null, null);
    }

    /// <summary>sends EXAMINE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Examine(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return Examine(mailbox.Name);
    }

    /// <summary>sends EXAMINE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Examine(string mailboxName)
    {
      return SelectExamineInternal(true, mailboxName, null, null);
    }

    /// <summary>sends SELECT command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    public ImapCommandResult SelectCondStore(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return SelectCondStore(mailbox.Name);
    }

    /// <summary>sends SELECT command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    public ImapCommandResult SelectCondStore(string mailboxName)
    {
      return SelectExamineInternal(false,
                                   mailboxName,
                                   new ImapParenthesizedString("CONDSTORE"),
                                   ImapCapability.CondStore);
    }

    /// <summary>sends EXAMINE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    public ImapCommandResult ExamineCondStore(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return ExamineCondStore(mailbox.Name);
    }

    /// <summary>sends EXAMINE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CONDSTORE extension.
    /// </remarks>
    public ImapCommandResult ExamineCondStore(string mailboxName)
    {
      return SelectExamineInternal(true,
                                   mailboxName,
                                   new ImapParenthesizedString("CONDSTORE"),
                                   ImapCapability.CondStore);
    }

    private ImapCommandResult SelectExamineInternal(bool selectAsReadOnly,
                                                    string mailboxName,
                                                    ImapParenthesizedString selectParameters,
                                                    ImapCapability selectParametersCapabilityRequirement)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      var selectingMailbox = mailboxManager.GetExist(mailboxName);
      var selectingMailboxExists = (selectingMailbox != null);

      if (selectingMailboxExists) {
        if (selectedMailbox == selectingMailbox)
          return new ImapCommandResult(ImapCommandResultCode.RequestDone,
                                       "mailbox has already been selected");
        else if (selectingMailbox.IsUnselectable)
          throw new ImapProtocolViolationException("mailbox is not selectable or not existent");
      }
      else {
        selectingMailbox = new ImapMailbox(mailboxName);
      }

      using (var t = selectAsReadOnly
             ? (SelectTransactionBase)new ExamineTransaction(connection, selectingMailbox, selectParametersCapabilityRequirement)
             : (SelectTransactionBase)new SelectTransaction (connection, selectingMailbox, selectParametersCapabilityRequirement)) {
        /*
         * RFC 4466 - Collected Extensions to IMAP4 ABNF
         * http://tools.ietf.org/html/rfc4466
         *
         *    examine         = "EXAMINE" SP mailbox [select-params]
         *                      ;; modifies the original IMAP EXAMINE command
         *                      ;; to accept optional parameters
         *    select          = "SELECT" SP mailbox [select-params]
         *                      ;; modifies the original IMAP SELECT command to
         *                      ;; accept optional parameters
         *    select-params   = SP "(" select-param *(SP select-param) ")"
         *    select-param    = select-param-name [SP select-param-value]
         *                      ;; a parameter to SELECT may contain one or
         *                      ;; more atoms and/or strings and/or lists.
         *    select-param-name= tagged-ext-label
         *    select-param-value= tagged-ext-val
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions
         */
        t.RequestArguments["mailbox name"] = argMailboxName;

        // select-params
        if (selectParameters != null)
          t.RequestArguments["select parameters"] = selectParameters;

        if (ProcessTransaction(t).Succeeded) {
          if (selectingMailboxExists)
            selectedMailbox = selectingMailbox;
          else
            selectedMailbox = mailboxManager.Add(selectingMailbox);

          TransitStateTo(ImapSessionState.Selected);
        }
        else {
          selectedMailbox = null;

          if (state != ImapSessionState.NotConnected)
            TransitStateTo(ImapSessionState.Authenticated);

          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);
        }

        return t.Result;
      }
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Create(string mailboxName)
    {
      ImapMailbox discard;

      return Create(mailboxName, out discard);
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Create(string mailboxName, out ImapMailbox createdMailbox)
    {
      using (var t = new CreateTransaction(connection, null)) {
        return CreateInternal(t, mailboxName, null, out createdMailbox);
      }
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CREATE-SPECIAL-USE extension.
    /// </remarks>
    public ImapCommandResult CreateSpecialUse(string mailboxName, params ImapMailboxFlag[] useFlags)
    {
      ImapMailbox discard;

      return CreateSpecialUse(mailboxName, new ImapMailboxFlagSet(useFlags), out discard);
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CREATE-SPECIAL-USE extension.
    /// </remarks>
    public ImapCommandResult CreateSpecialUse(string mailboxName, out ImapMailbox createdMailbox, params ImapMailboxFlag[] useFlags)
    {
      return CreateSpecialUse(mailboxName, new ImapMailboxFlagSet(useFlags), out createdMailbox);
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CREATE-SPECIAL-USE extension.
    /// </remarks>
    public ImapCommandResult CreateSpecialUse(string mailboxName, IImapMailboxFlagSet useFlags)
    {
      ImapMailbox discard;

      return CreateSpecialUse(mailboxName, useFlags, out discard);
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CREATE-SPECIAL-USE extension.
    /// </remarks>
    public ImapCommandResult CreateSpecialUse(string mailboxName, IImapMailboxFlagSet useFlags, out ImapMailbox createdMailbox)
    {
      if (useFlags == null)
        throw new ArgumentNullException("useFlags");

      if (!ImapMailboxFlag.UseFlags.IsSupersetOf(useFlags))
        throw new ArgumentException("contains non-use-flag", "useFlags");

      using (var t = new CreateTransaction(connection, ImapCapability.CreateSpecialUse)) {
        var result = CreateInternal(t,
                                    mailboxName,
                                    new ImapParenthesizedString("USE", new ImapParenthesizedString(useFlags.ToArray())),
                                    out createdMailbox);

        if (result.Succeeded)
          createdMailbox.Flags = (new ImapMailboxFlagSet(useFlags)).AsReadOnly();

        return result;
      }
    }

    private ImapCommandResult CreateInternal(CreateTransaction t, string mailboxName, ImapParenthesizedString createParams, out ImapMailbox createdMailbox)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      /*
       * 6.3.3. CREATE Command
       *       It is an error to attempt to create INBOX or a mailbox
       *       with a name that refers to an extant mailbox. 
       */
      if (ImapMailbox.IsNameInbox(mailboxName))
        throw new ImapProtocolViolationException("It is an error to attempt to create INBOX.");

      var existingMailbox = mailboxManager.GetExist(mailboxName);

      if (existingMailbox != null && !existingMailbox.Flags.Contains(ImapMailboxFlag.NonExistent))
        throw new ImapProtocolViolationException(string.Format("It is an error to attempt to create a mailbox with a name that refers to an extent mailbox. (mailboxName: '{0}')", mailboxName));

      createdMailbox = null;

      /*
       * RFC 4466 - Collected Extensions to IMAP4 ABNF
       * http://tools.ietf.org/html/rfc4466
       *
       *    create          = "CREATE" SP mailbox
       *                      [create-params]
       *                      ;; Use of INBOX gives a NO error.
       *    create-params   = SP "(" create-param *( SP create-param) ")"
       *    create-param-name = tagged-ext-label
       *    create-param      = create-param-name [SP create-param-value]
       *    create-param-value= tagged-ext-val
       *                      ;; This non-terminal shows recommended syntax
       *                      ;; for future extensions.
       */
      // mailbox name
      t.RequestArguments["mailbox name"] = argMailboxName;

      // create-params
      if (createParams != null)
        t.RequestArguments["create parameters"] = createParams;

      if (ProcessTransaction(t).Succeeded) {
        if (existingMailbox == null)
          createdMailbox = mailboxManager.Add(new ImapMailbox(mailboxName));
        else
          createdMailbox = mailboxManager.Rename(existingMailbox, mailboxName);
      }
      else {
        ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);
      }

      return t.Result;
    }

    /// <summary>sends DELETE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Delete(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return Delete(mailbox.Name);
    }

    /// <summary>sends DELETE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Delete(string mailboxName)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      /*
       * 6.3.4. DELETE Command
       *       It is an error to attempt to delete INBOX or a
       *       mailbox name that does not exist.
       */
      if (ImapMailbox.IsNameInbox(mailboxName))
        throw new ImapProtocolViolationException("It is an error to attempt to delete INBOX.");

      /*
       * 6.3.4. DELETE Command
       *       It is an error to attempt to
       *       delete a name that has inferior hierarchical names and also has
       *       the \Noselect mailbox name attribute (see the description of the
       *       LIST response for more details).
       * 
       *       It is permitted to delete a name that has inferior hierarchical
       *       names and does not have the \Noselect mailbox name attribute.  In
       *       this case, all messages in that mailbox are removed, and the name
       *       will acquire the \Noselect mailbox name attribute.
       */
      var deletingMailbox = mailboxManager.GetExist(mailboxName);

      if (deletingMailbox != null) {
        if (deletingMailbox.IsUnselectable && mailboxManager.ExistChildrenOf(deletingMailbox))
          throw new ImapProtocolViolationException("It is an error to attempt to delete a name that has inferior hierarchical names and also has the \\Noselect or \\NonExistent mailbox name attribute.");
      }

      using (var t = new DeleteTransaction(connection)) {
        t.RequestArguments["mailbox name"] = argMailboxName;

        if (ProcessTransaction(t).Succeeded) {
          if (selectedMailbox != null && deletingMailbox == selectedMailbox) {
            selectedMailbox = null;
            TransitStateTo(ImapSessionState.Authenticated);
          }

          mailboxManager.Delete(mailboxName);
        }
        else {
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);
        }

        return t.Result;
      }
    }

    /// <summary>sends RENAME command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Rename(ImapMailbox existingMailbox, string newMailboxName)
    {
      ValidateMailboxRelationship(existingMailbox);

      ImapMailbox discard;

      return Rename(existingMailbox.Name, newMailboxName, out discard);
    }

    /// <summary>sends RENAME command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Rename(ImapMailbox existingMailbox, string newMailboxName, out ImapMailbox renamedMailbox)
    {
      ValidateMailboxRelationship(existingMailbox);

      return Rename(existingMailbox.Name, newMailboxName, out renamedMailbox);
    }

    /// <summary>sends RENAME command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Rename(string existingMailboxName, string newMailboxName)
    {
      ImapMailbox discard;

      return Rename(existingMailboxName, newMailboxName, out discard);
    }

    /// <summary>sends RENAME command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Rename(string existingMailboxName, string newMailboxName, out ImapMailbox renamedMailbox)
    {
      RejectNonAuthenticatedState();

      var argExistingMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(existingMailboxName);
      var argNewMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(newMailboxName);

      if (ImapMailbox.NameEquals(existingMailboxName, newMailboxName))
        throw new ArgumentException("An existing mailbox name and a new mailbox name are same.", "newMailboxName");

      /* 
       * 6.3.5. RENAME Command
       *       It is
       *       an error to attempt to rename from a mailbox name that does not
       *       exist or to a mailbox name that already exists.
       */
      var existingRenameToMailbox = mailboxManager.GetExist(newMailboxName);

      if (ImapMailbox.IsNameInbox(newMailboxName) ||
          (existingRenameToMailbox != null && !existingRenameToMailbox.Flags.Contains(ImapMailboxFlag.NonExistent)))
        throw new ImapProtocolViolationException(string.Format("It is an error to attempt to rename to a mailbox name that already exists. (newMailboxName: '{0}')", newMailboxName));

      renamedMailbox = null;

      using (var t = new RenameTransaction(connection)) {
        /*
         * RFC 4466 - Collected Extensions to IMAP4 ABNF
         * http://tools.ietf.org/html/rfc4466
         *
         *    rename          = "RENAME" SP mailbox SP mailbox
         *                      [rename-params]
         *                      ;; Use of INBOX as a destination gives
         *                      ;; a NO error, unless rename-params
         *                      ;; is not empty.
         *    rename-params     = SP "(" rename-param *( SP rename-param) ")"
         *    rename-param      = rename-param-name [SP rename-param-value]
         *    rename-param-name = tagged-ext-label
         *    rename-param-value= tagged-ext-val
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions.
         */
        t.RequestArguments["existing mailbox name"] = argExistingMailboxName;
        t.RequestArguments["new mailbox name"] = argNewMailboxName;

        // rename-params
        //t.RequestArguments["rename parameters"] = new ImapParenthesizedString();

        if (ProcessTransaction(t).Succeeded)
          renamedMailbox = mailboxManager.Rename(existingMailboxName, newMailboxName);
        else
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);

        return t.Result;
      }
    }

    /// <summary>sends SUBSCRIBE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Subscribe(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return Subscribe(mailbox.Name);
    }

    /// <summary>sends SUBSCRIBE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Subscribe(string mailboxName)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      var existingSubscribeMailbox = mailboxManager.GetExist(mailboxName);

      if (existingSubscribeMailbox != null && existingSubscribeMailbox.Flags.Contains(ImapMailboxFlag.NonExistent))
        throw new ImapProtocolViolationException("It is an error to attempt to subscribe a name that has the \\NonExistent mailbox name attribute.");

      using (var t = new SubscribeTransaction(connection)) {
        t.RequestArguments["mailbox name"] = argMailboxName;

        if (ProcessTransaction(t).Failed)
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);

        return t.Result;
      }
    }

    /// <summary>sends UNSUBSCRIBE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Unsubscribe(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return Unsubscribe(mailbox.Name);
    }

    /// <summary>sends UNSUBSCRIBE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Unsubscribe(string mailboxName)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      using (var t = new UnsubscribeTransaction(connection)) {
        t.RequestArguments["mailbox name"] = argMailboxName;

        if (ProcessTransaction(t).Failed)
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);

        return t.Result;
      }
    }

    /// <summary>sends STATUS command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Status(ImapMailbox mailbox, ImapStatusDataItem statusDataItem)
    {
      ImapStatusAttributeList discard1;
      ImapMailbox discard2;

      ValidateMailboxRelationship(mailbox);

      return StatusInternal(mailbox.Name, statusDataItem, out discard1, out discard2);
    }

    /// <summary>sends STATUS command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Status(string mailboxName, ImapStatusDataItem statusDataItem)
    {
      ImapStatusAttributeList discard1;
      ImapMailbox discard2;

      return StatusInternal(mailboxName, statusDataItem, out discard1, out discard2);
    }

    /// <summary>sends STATUS command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Status(string mailboxName, ImapStatusDataItem statusDataItem, out ImapStatusAttributeList statusAttributes)
    {
      ImapMailbox discard;

      return StatusInternal(mailboxName, statusDataItem, out statusAttributes, out discard);
    }

    /// <summary>sends STATUS command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Status(string mailboxName, ImapStatusDataItem statusDataItem, out ImapMailbox statusMailbox)
    {
      ImapStatusAttributeList discard;

      return StatusInternal(mailboxName, statusDataItem, out discard, out statusMailbox);
    }

    /// <summary>sends STATUS command</summary>
    /// <remarks>valid in authenticated state</remarks>
    private ImapCommandResult StatusInternal(string mailboxName, ImapStatusDataItem statusDataItem, out ImapStatusAttributeList statusAttributes, out ImapMailbox statusMailbox)
    {
      RejectNonAuthenticatedState();

      var argMailboxName = ImapMailboxNameString.CreateMailboxNameNonEmpty(mailboxName);

      if (statusDataItem == null)
        throw new ArgumentNullException("statusDataItem");

      /*
       * 6.3.10. STATUS Command
       * 
       *            Note: The STATUS command is intended to access the
       *            status of mailboxes other than the currently selected
       *            mailbox.  Because the STATUS command can cause the
       *            mailbox to be opened internally, and because this
       *            information is available by other means on the selected
       *            mailbox, the STATUS command SHOULD NOT be used on the
       *            currently selected mailbox.
       */
      var existStatusMailbox = mailboxManager.GetExist(mailboxName);

      if (existStatusMailbox != null) {
        if (existStatusMailbox == selectedMailbox)
          throw new ImapProtocolViolationException("the STATUS command SHOULD NOT be used on the currently selected mailbox");

        if (existStatusMailbox.IsUnselectable)
          throw new ImapProtocolViolationException("the STATUS command SHOULD NOT be used on the mailbox that has \\Noselect or \\NonExistent attribute");
      }

      statusAttributes = null;
      statusMailbox = null;

      using (var t = new StatusTransaction(connection)) {
        // mailbox name
        t.RequestArguments["mailbox name"] = argMailboxName;

        // status data item names
        t.RequestArguments["status data item names"] = statusDataItem;

        if (ProcessTransaction(t).Succeeded) {
          statusMailbox = mailboxManager.GetExistOrCreate(mailboxName);

          statusAttributes = t.Result.Value;
          statusMailbox.UpdateStatus(statusAttributes);
        }
        else {
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);
        }

        return t.Result;
      }
    }

    /// <summary>sends ENABLE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Enable(params string[] capabilityNames)
    {
      ImapCapabilitySet discard;

      return Enable(out discard, capabilityNames);
    }

    /// <summary>sends ENABLE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Enable(out ImapCapabilitySet enabledCapabilities, params string[] capabilityNames)
    {
      RejectNonAuthenticatedState();

      if (capabilityNames == null || capabilityNames.Length == 0)
        throw new ArgumentException("at least one capability name is required", "capabilityNames");

      enabledCapabilities = null;

      using (var t = new EnableTransaction(connection)) {
        // capability names
        t.RequestArguments["capability names"] = new ImapStringList(capabilityNames);

        if (ProcessTransaction(t).Succeeded)
          enabledCapabilities = t.Result.Value;

        return t.Result;
      }
    }

    private bool ProcessMailboxRefferalResponse(ImapTaggedStatusResponse tagged)
    {
      if (tagged == null)
        return false;
      else if (tagged.Condition != ImapResponseCondition.No)
        return false;
      else if (tagged.ResponseText.Code != ImapResponseCode.Referral)
        return false;

      // RFC 2193 IMAP4 Mailbox Referrals
      // http://tools.ietf.org/html/rfc2193
      // 4.1. SELECT, EXAMINE, DELETE, SUBSCRIBE, UNSUBSCRIBE, STATUS and APPEND
      //      Referrals
      //    An IMAP4 server MAY respond to the SELECT, EXAMINE, DELETE,
      //    SUBSCRIBE, UNSUBSCRIBE, STATUS or APPEND command with one or more
      //    IMAP mailbox referrals to indicate to the client that the mailbox is
      //    hosted on a remote server.

      // 4.2. CREATE Referrals
      //    An IMAP4 server MAY respond to the CREATE command with one or more
      //    IMAP mailbox referrals, if it wishes to direct the client to issue
      //    the CREATE against another server.  The server can employ any means,
      //    such as examining the hierarchy of the specified mailbox name, in
      //    determining which server the mailbox should be created on.

      // 4.3. RENAME Referrals
      //    An IMAP4 server MAY respond to the RENAME command with one or more
      //    pairs of IMAP mailbox referrals.  In each pair of IMAP mailbox
      //    referrals, the first one is an URL to the existing mailbox name and
      //    the second is an URL to the requested new mailbox name.

      // 4.4. COPY Referrals
      //    An IMAP4 server MAY respond to the COPY command with one or more IMAP
      //    mailbox referrals.  This indicates that the destination mailbox is on
      //    a remote server.  To achieve the same behavior of a server COPY, the
      //    client MAY issue the constituent FETCH and APPEND commands against
      //    both servers.
      var referrals = ImapResponseTextConverter.FromReferral(tagged.ResponseText);

      if (handlesReferralAsException) {
        throw new ImapMailboxReferralException(string.Format("try another server: '{0}'", tagged.ResponseText.Text),
                                               referrals);
      }
      else {
        TraceInfo("mailbox referral: '{0}'", tagged.ResponseText.Text);
        TraceInfo("  try mailboxes below:");

        foreach (var referral in referrals) {
          TraceInfo(string.Concat("    ", referral));
        }
      }

      return true;
    }
  }
}
