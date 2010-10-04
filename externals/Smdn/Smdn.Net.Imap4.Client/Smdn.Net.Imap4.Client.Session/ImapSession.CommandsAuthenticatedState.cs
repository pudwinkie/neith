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
using System.IO;

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

      RejectInvalidMailboxNameArgument(mailboxName);

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
        t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

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

      return CreateSpecialUse(mailboxName, new ImapMailboxFlagList(useFlags), out discard);
    }

    /// <summary>sends CREATE command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support CREATE-SPECIAL-USE extension.
    /// </remarks>
    public ImapCommandResult CreateSpecialUse(string mailboxName, out ImapMailbox createdMailbox, params ImapMailboxFlag[] useFlags)
    {
      return CreateSpecialUse(mailboxName, new ImapMailboxFlagList(useFlags), out createdMailbox);
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

      foreach (var useFlag in useFlags) {
        if (!ImapMailboxFlag.UseFlags.Has(useFlag))
          throw new ArgumentException(string.Format("must be one of use-flag. '{0}' is invalid.", useFlag),
                                      "useFlags");
      }

      using (var t = new CreateTransaction(connection, ImapCapability.CreateSpecialUse)) {
        var result = CreateInternal(t,
                                    mailboxName,
                                    new ImapParenthesizedString("USE", new ImapParenthesizedString(useFlags.ToArray())),
                                    out createdMailbox);

        if (result.Succeeded)
          createdMailbox.Flags = (new ImapMailboxFlagList(useFlags)).AsReadOnly();

        return result;
      }
    }

    private ImapCommandResult CreateInternal(CreateTransaction t, string mailboxName, ImapParenthesizedString createParams, out ImapMailbox createdMailbox)
    {
      RejectNonAuthenticatedState();

      RejectInvalidMailboxNameArgument(mailboxName);

      /*
       * 6.3.3. CREATE Command
       *       It is an error to attempt to create INBOX or a mailbox
       *       with a name that refers to an extant mailbox. 
       */
      if (ImapMailbox.IsNameInbox(mailboxName))
        throw new ImapProtocolViolationException("It is an error to attempt to create INBOX.");

      var existingMailbox = mailboxManager.GetExist(mailboxName);

      if (existingMailbox != null && !existingMailbox.Flags.Has(ImapMailboxFlag.NonExistent))
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
      t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

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

      RejectInvalidMailboxNameArgument(mailboxName);

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
        t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

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

      RejectInvalidMailboxNameArgument(existingMailboxName);
      RejectInvalidMailboxNameArgument(newMailboxName);

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
          (existingRenameToMailbox != null && !existingRenameToMailbox.Flags.Has(ImapMailboxFlag.NonExistent)))
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
        t.RequestArguments["existing mailbox name"] = new ImapMailboxNameString(existingMailboxName);
        t.RequestArguments["new mailbox name"] = new ImapMailboxNameString(newMailboxName);

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

      RejectInvalidMailboxNameArgument(mailboxName);

      var existingSubscribeMailbox = mailboxManager.GetExist(mailboxName);

      if (existingSubscribeMailbox != null && existingSubscribeMailbox.Flags.Has(ImapMailboxFlag.NonExistent))
        throw new ImapProtocolViolationException("It is an error to attempt to subscribe a name that has the \\NonExistent mailbox name attribute.");

      using (var t = new SubscribeTransaction(connection)) {
        t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

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

      RejectInvalidMailboxNameArgument(mailboxName);

      using (var t = new UnsubscribeTransaction(connection)) {
        t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

        if (ProcessTransaction(t).Failed)
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);

        return t.Result;
      }
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty mailbox name and an empty reference name.
    /// </remarks>
    public ImapCommandResult ListRoot(out ImapMailboxList root)
    {
      return ListRoot(string.Empty, out root);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty mailbox name.
    /// </remarks>
    public ImapCommandResult ListRoot(string referenceName, out ImapMailboxList root)
    {
      RejectNonAuthenticatedState();

      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      root = null;

      using (var t = new ListTransaction(connection)) {
        t.RequestArguments["reference name"] = new ImapQuotedString(referenceName);
        t.RequestArguments["mailbox name"] = new ImapQuotedString(string.Empty);

        if (ProcessTransaction(t).Succeeded) {
          hierarchyDelimiters[referenceName] = t.Result.Value[0].HierarchyDelimiter;

          root = t.Result.Value[0];
        }

        return t.Result;
      }
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult List(out ImapMailbox[] mailboxes)
    {
      return ListInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult List(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return ListInternal(string.Empty, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult List(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox[] discard;

      return ListInternal(string.Empty, new ImapMailboxNameString(mailbox.Name), out discard);
    }

    /// <summary>sends LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult List(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return ListInternal(referenceName, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    private ImapCommandResult ListInternal(string referenceName,
                                           ImapString mailboxName,
                                           out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new ListTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LSUB command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult Lsub(out ImapMailbox[] mailboxes)
    {
      return LsubInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Lsub(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return LsubInternal(string.Empty, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Lsub(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox[] discard;

      return LsubInternal(string.Empty, new ImapMailboxNameString(mailbox.Name), out discard);
    }

    /// <summary>sends LSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Lsub(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return LsubInternal(referenceName, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    private ImapCommandResult LsubInternal(string referenceName,
                                           ImapString mailboxName,
                                           out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new LsubTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends Gimap XLIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends Gimap XLIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult XList(out ImapMailbox[] mailboxes)
    {
      return XListInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends Gimap LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult XList(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return XListInternal(string.Empty, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    /// <summary>sends Gimap LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult XList(ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox[] discard;

      return XListInternal(string.Empty, new ImapMailboxNameString(mailbox.Name), out discard);
    }

    /// <summary>sends Gimap LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult XList(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return XListInternal(referenceName, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    private ImapCommandResult XListInternal(string referenceName,
                                            ImapString mailboxName,
                                            out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new XListTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends RLIST command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends RLIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult RList(out ImapMailbox[] mailboxes)
    {
      return RListInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends RLIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RList(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return RListInternal(string.Empty, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    /// <summary>sends RLIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RList(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return RListInternal(referenceName, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    private ImapCommandResult RListInternal(string referenceName,
                                            ImapString mailboxName,
                                            out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new RListTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

    /// <summary>sends RLSUB command</summary>
    /// <remarks>
    /// valid in authenticated state
    /// This method sends LIST command with an empty reference name and a wildcard "*" as mailbox name
    /// </remarks>
    public ImapCommandResult RLsub(out ImapMailbox[] mailboxes)
    {
      return RLsubInternal(string.Empty, "*", out mailboxes);
    }

    /// <summary>sends RLSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RLsub(string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return RLsubInternal(string.Empty, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    /// <summary>sends RLSUB command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult RLsub(string referenceName, string mailboxName, out ImapMailbox[] mailboxes)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return RLsubInternal(referenceName, new ImapMailboxNameString(mailboxName), out mailboxes);
    }

    private ImapCommandResult RLsubInternal(string referenceName,
                                            ImapString mailboxName,
                                            out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");

      using (var t = new RLsubTransaction(connection)) {
        return ListLsubInternal(t, referenceName, mailboxName, out mailboxes);
      }
    }

#region "extended LIST with no reference name"
#region "extended LIST with multiple mailbox name pattern"
    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, null, null, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(ImapListSelectionOptions selectionOptions,
                                          out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, selectionOptions, null, out mailboxes);
    }

      /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, null, returnOptions, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(ImapListSelectionOptions selectionOptions,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes,
                                          string mailboxNamePattern,
                                          params string[] mailboxNamePatterns)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, mailboxNamePatterns, selectionOptions, returnOptions, out mailboxes);
    }
#endregion

#region "extended LIST with single mailbox name pattern"
    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          out ImapMailbox[] mailboxes)
    {
      return ListExtendedInternalNoRefName(mailboxNamePattern, null, null, null, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          ImapListSelectionOptions selectionOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, null, selectionOptions, null, out mailboxes);
    }

      /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, null, null, returnOptions, out mailboxes);
    }

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string mailboxNamePattern,
                                          ImapListSelectionOptions selectionOptions,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternalNoRefName(mailboxNamePattern, null, selectionOptions, returnOptions, out mailboxes);
    }
#endregion

    private ImapCommandResult ListExtendedInternalNoRefName(string mailboxNamePattern,
                                                            string[] mailboxNamePatterns,
                                                            ImapListSelectionOptions selectionOptions,
                                                            ImapListReturnOptions returnOptions,
                                                            out ImapMailbox[] mailboxes)
    {
      if (mailboxNamePattern == null)
        throw new ArgumentNullException("mailboxNamePattern");

      if (mailboxNamePatterns == null || mailboxNamePatterns.Length == 0)
        return ListExtendedInternal(string.Empty, new[] {mailboxNamePattern}, selectionOptions, returnOptions, out mailboxes);
      else
        return ListExtendedInternal(string.Empty, mailboxNamePatterns.Prepend(mailboxNamePattern), selectionOptions, returnOptions, out mailboxes);
    }
#endregion

    /// <summary>sends extended LIST command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult ListExtended(string referenceName,
                                          string[] mailboxNamePatterns,
                                          ImapListSelectionOptions selectionOptions,
                                          ImapListReturnOptions returnOptions,
                                          out ImapMailbox[] mailboxes)
    {
      if (referenceName == null)
        throw new ArgumentNullException("referenceName");
      if (mailboxNamePatterns == null)
        throw new ArgumentNullException("mailboxNamePatterns");
      else if (mailboxNamePatterns.Length == 0)
        throw new ArgumentException("must be non-empty array", "mailboxNamePatterns");
      if (selectionOptions == null)
        throw new ArgumentNullException("selectionOptions");
      if (returnOptions == null)
        throw new ArgumentNullException("returnOptions");

      return ListExtendedInternal(referenceName, mailboxNamePatterns, selectionOptions, returnOptions, out mailboxes);
    }

    private ImapCommandResult ListExtendedInternal(string referenceName,
                                                   string[] mailboxNamePatterns,
                                                   ImapListSelectionOptions selectionOptions,
                                                   ImapListReturnOptions returnOptions,
                                                   out ImapMailbox[] mailboxes)
    {
      using (var t = new ListExtendedTransaction(connection)) {
        if (selectionOptions != null)
        t.RequestArguments["selection options"] = selectionOptions;

        if (returnOptions != null)
          t.RequestArguments["return options"] = returnOptions;

        var quotedMailboxNamePatterns = Array.ConvertAll(mailboxNamePatterns, delegate(string pattern) {
          return new ImapMailboxNameString(pattern);
        });

        if (quotedMailboxNamePatterns.Length == 1)
          ListLsubInternal(t, referenceName, quotedMailboxNamePatterns[0], out mailboxes);
        else
          ListLsubInternal(t, referenceName, new ImapParenthesizedString(quotedMailboxNamePatterns), out mailboxes);

        /*
         * IMAP4 Extension for Returning STATUS Information in Extended LIST
         * http://tools.ietf.org/html/rfc5819
         */
        if (returnOptions != null && returnOptions.RequiredCapabilities.Contains(ImapCapability.ListStatus)) {
          // XXX: converting STATUS response
          foreach (var response in t.Result.ReceivedResponses) {
            var data = response as ImapDataResponse;

            if (data == null || data.Type != ImapDataResponseType.Status)
              continue;

            string mailboxName;
            var statusAttr = ImapDataResponseConverter.FromStatus(data, out mailboxName);

            var statusMailbox = Array.Find(mailboxes, delegate(ImapMailbox mailbox) {
              return mailbox.Name == mailboxName;
            });

            if (statusMailbox != null)
              statusMailbox.UpdateStatus(statusAttr);
          }
        }

        return t.Result;
      }
    }

    private ImapCommandResult ListLsubInternal(ListTransactionBase t,
                                               string referenceName,
                                               ImapString mailboxName,
                                               out ImapMailbox[] mailboxes)
    {
      RejectNonAuthenticatedState();

      mailboxes = null;

      t.RequestArguments["reference name"] = new ImapQuotedString(referenceName);
      t.RequestArguments["mailbox name"] = mailboxName;

      if (ProcessTransaction(t).Succeeded)
        mailboxes = Array.ConvertAll<ImapMailboxList, ImapMailbox>(t.Result.Value, mailboxManager.GetExistOrCreate);

      return t.Result;
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

      RejectInvalidMailboxNameArgument(mailboxName);

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
        t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

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

    /// <summary>sends APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Append(IImapAppendMessage message, ImapMailbox mailbox)
    {
      return AppendInternal(new[] {message}, false, mailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, ImapMailbox mailbox)
    {
      return AppendInternal(new[] {message}, true, mailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox)
    {
      return AppendInternal(messages, false, mailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox)
    {
      return AppendInternal(messages, true, mailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      ImapAppendedUidSet discard;
      ImapMailbox discard2;

      return AppendInternal(messages, binary, mailbox.Name, false, out discard, out discard2);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, false, mailbox, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, true, mailbox, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, false, mailbox, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, true, mailbox, out appendedUids);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, ImapMailbox mailbox, out ImapAppendedUidSet appendedUids)
    {
      ValidateMailboxRelationship(mailbox);

      ImapMailbox discard;

      return AppendInternal(messages, binary, mailbox.Name, false, out appendedUids, out discard);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName)
    {
      return AppendInternal(new[] {message}, false, mailboxName);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName)
    {
      return AppendInternal(new[] {message}, true, mailboxName);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName)
    {
      return AppendInternal(messages, false, mailboxName);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName)
    {
      return AppendInternal(messages, true, mailboxName);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName)
    {
      ImapAppendedUidSet discard;
      ImapMailbox discard2;

      return AppendInternal(messages, binary, mailboxName, false, out discard, out discard2);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, false, mailboxName, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(new[] {message}, true, mailboxName, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, false, mailboxName, out appendedUids);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      return AppendInternal(messages, true, mailboxName, out appendedUids);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, out ImapAppendedUidSet appendedUids)
    {
      ImapMailbox discard;

      return AppendInternal(messages, binary, mailboxName, false, out appendedUids, out discard);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, false, mailboxName, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, true, mailboxName, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, false, mailboxName, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, true, mailboxName, out createdMailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, out ImapMailbox createdMailbox)
    {
      ImapAppendedUidSet discard;

      return AppendInternal(messages, binary, mailboxName, true, out discard, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult Append(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, false, mailboxName, out appendedUids, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinary(IImapAppendMessage message, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(new[] {message}, true, mailboxName, out appendedUids, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, false, mailboxName, out appendedUids, out createdMailbox);
    }

    /// <summary>sends APPEND command</summary>
    /// <remarks>
    /// valid in authenticated state.
    /// this method will fail if server does not support BINARY or MULTIAPPEND extension.
    /// this method tries to automatically create the mailbox when server sent [TRYCREATE] response code.
    /// the out parameter <paramref name="appendedUids"/> will be set if the server supports UIDPLUS extension and returns [APPENDUID] responce code, otherwise null.
    /// </remarks>
    public ImapCommandResult AppendBinaryMultiple(IEnumerable<IImapAppendMessage> messages, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, true, mailboxName, out appendedUids, out createdMailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      return AppendInternal(messages, binary, mailboxName, true, out appendedUids, out createdMailbox);
    }

    private ImapCommandResult AppendInternal(IEnumerable<IImapAppendMessage> messages, bool binary, string mailboxName, bool tryCreate, out ImapAppendedUidSet appendedUids, out ImapMailbox createdMailbox)
    {
      RejectNonAuthenticatedState();

      if (messages == null)
        throw new ArgumentNullException("messages");

      RejectInvalidMailboxNameArgument(mailboxName);

      appendedUids = null;
      createdMailbox = null;

      // append message
      var messagesToUpload = new List<ImapString>();
      var messageCount = 0;
      var literalOptions = ImapLiteralOptions.NonSynchronizingIfCapable
                           | (binary ? ImapLiteralOptions.Literal8 : ImapLiteralOptions.Literal);

      foreach (var message in messages) {
        if (message == null)
          throw new ArgumentException("contains null", "messages");

        /*
         * RFC 4466 - Collected Extensions to IMAP4 ABNF
         * http://tools.ietf.org/html/rfc4466
         * 
         *    append          = "APPEND" SP mailbox 1*append-message
         *                      ;; only a single append-message may appear
         *                      ;; if MULTIAPPEND [MULTIAPPEND] capability
         *                      ;; is not present
         *    append-message  = append-opts SP append-data
         *    append-ext      = append-ext-name SP append-ext-value
         *                      ;; This non-terminal define extensions to
         *                      ;; to message metadata.
         *    append-ext-name = tagged-ext-label
         *    append-ext-value= tagged-ext-val
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions.
         *    append-data     = literal / literal8 / append-data-ext
         *    append-data-ext = tagged-ext
         *                      ;; This non-terminal shows recommended syntax
         *                      ;; for future extensions,
         *                      ;; i.e., a mandatory label followed
         *                      ;; by parameters.
         *    append-opts     = [SP flag-list] [SP date-time] *(SP append-ext)
         *                      ;; message metadata
         */

        // flag-list
        if (message.Flags != null && 0 < message.Flags.Count)
          messagesToUpload.Add(new ImapParenthesizedString(message.Flags.GetNonApplicableFlagsRemoved().ToArray()));

        // date-time
        if (message.InternalDate.HasValue)
          messagesToUpload.Add(ImapDateTimeFormat.ToDateTimeString(message.InternalDate.Value));

        // append-data
        messagesToUpload.Add(new ImapLiteralStream(message.GetMessageStream(),
                                                   literalOptions));

        messageCount++;
      }

      if (messageCount == 0)
        throw new ArgumentException("at least 1 message must be specified", "messages");

      ImapCommandResult failedResult = null;

      for (var i = 0; i < 2; i++) {
        var respTryCreate = false;

        using (var t = new AppendTransaction(connection, 1 < messageCount)) {
          // mailbox name
          t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

          // messages to upload
          t.RequestArguments["messages to upload"] = new ImapStringList(messagesToUpload.ToArray());

          if (ProcessTransaction(t).Succeeded) {
            appendedUids = t.Result.Value;
            return t.Result;
          }
          else {
            if (ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse) || !tryCreate)
              return t.Result;
          }

          failedResult = t.Result;

          // 6.3.11. APPEND Command
          //       If the destination mailbox does not exist, a server MUST return an
          //       error, and MUST NOT automatically create the mailbox.  Unless it
          //       is certain that the destination mailbox can not be created, the
          //       server MUST send the response code "[TRYCREATE]" as the prefix of
          //       the text of the tagged NO response.  This gives a hint to the
          //       client that it can attempt a CREATE command and retry the APPEND
          //       if the CREATE is successful.
          respTryCreate = (t.Result.GetResponseCode(ImapResponseCode.TryCreate) is ImapTaggedStatusResponse);
        }

        // try create
        if (i == 0 && respTryCreate)
          if (Create(mailboxName, out createdMailbox).Failed)
            return failedResult;
      }

      return failedResult;
    }

    /// <summary>begins to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public IAsyncResult BeginAppend(Stream messageBodyStream, DateTimeOffset? internalDate, IImapMessageFlagSet flags, ImapMailbox mailbox)
    {
      ValidateMailboxRelationship(mailbox);

      return BeginAppend(messageBodyStream, internalDate, flags, mailbox.Name);
    }

    /// <summary>begins to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public IAsyncResult BeginAppend(Stream messageBodyStream, DateTimeOffset? internalDate, IImapMessageFlagSet flags, string mailboxName)
    {
      RejectNonAuthenticatedState();
      RejectTransactionProceeding();

      if (messageBodyStream == null)
        throw new ArgumentNullException("messageBodyStream");

      RejectInvalidMailboxNameArgument(mailboxName);

      // append message
      var messagesToUpload = new List<ImapString>(1);

      // flag-list
      if (flags != null && 0 < flags.Count)
        messagesToUpload.Add(new ImapParenthesizedString(flags.GetNonApplicableFlagsRemoved().ToArray()));

      // date-time
      if (internalDate.HasValue)
        messagesToUpload.Add(ImapDateTimeFormat.ToDateTimeString(internalDate.Value));

      // append-data
      messagesToUpload.Add(new ImapLiteralStream(messageBodyStream, ImapLiteralOptions.Synchronizing));

      AppendTransaction t = null;

      try {
        t = new AppendTransaction(connection, false);

        // mailbox name
        t.RequestArguments["mailbox name"] = new ImapMailboxNameString(mailboxName);

        // messages to upload
        t.RequestArguments["messages to upload"] = new ImapStringList(messagesToUpload.ToArray());

        var asyncResult = BeginProcessTransaction(t, handlesIncapableAsException);

        // wait for started (or completed)
        for (;;) {
          if (asyncResult.IsCompleted)
            break;
          else if (IsTransactionProceeding)
            break;
          else
            System.Threading.Thread.Sleep(10);
        }

        return asyncResult;
      }
      catch {
        if (t != null) {
          t.Dispose();
          t = null;
        }

        throw;
      }
    }

    /// <summary>ends to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult EndAppend(IAsyncResult asyncResult)
    {
      ImapAppendedUidSet discard;

      return EndAppend(asyncResult, out discard);
    }

    /// <summary>ends to send APPEND command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult EndAppend(IAsyncResult asyncResult, out ImapAppendedUidSet appendedUid)
    {
      appendedUid = null;

      var appendAsyncResult = asyncResult as TransactionAsyncResult;

      if (appendAsyncResult == null)
        throw new ArgumentException("invalid IAsyncResult", "asyncResult");

      using (var t = appendAsyncResult.Transaction as AppendTransaction) {
        if (EndProcessTransaction(appendAsyncResult).Succeeded)
          appendedUid = t.Result.Value;
        else
          ProcessMailboxRefferalResponse(t.Result.TaggedStatusResponse);

        return t.Result;
      }
    }

    /// <summary>sends ENABLE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Enable(params string[] capabilityNames)
    {
      ImapCapabilityList discard;

      return Enable(out discard, capabilityNames);
    }

    /// <summary>sends ENABLE command</summary>
    /// <remarks>valid in authenticated state</remarks>
    public ImapCommandResult Enable(out ImapCapabilityList enabledCapabilities, params string[] capabilityNames)
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
        Trace.Info("mailbox referral: '{0}'", tagged.ResponseText.Text);
        Trace.Info("  try mailboxes below:");

        foreach (var referral in referrals) {
          Trace.Info("    {0}", referral);
        }
      }

      return true;
    }
  }
}
