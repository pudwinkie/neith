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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  partial class ImapClient {
    internal static bool DefaultSubscription = false;
    internal static bool DefaultSelectReadOnly = false;

    public ImapOpenedMailboxInfo OpenedMailbox {
      get { ThrowIfNotConnected(); return openedMailbox; }
    }

    /*
     * events
     */
    public event EventHandler<ImapMailboxSizeChangedEventArgs> ExistMessageCountChanged;

    internal void RaiseExistMessageCountChanged(ImapOpenedMailboxInfo mailbox, long prevCount)
    {
      var ev = ExistMessageCountChanged;

      if (ev != null)
        ev(this, new ImapMailboxSizeChangedEventArgs(mailbox, mailbox.ExistMessageCount, prevCount));
    }

    public event EventHandler<ImapMailboxSizeChangedEventArgs> RecentMessageCountChanged;

    internal void RaiseRecentMessageCountChanged(ImapOpenedMailboxInfo mailbox, long prevCount)
    {
      var ev = RecentMessageCountChanged;

      if (ev != null)
        ev(this, new ImapMailboxSizeChangedEventArgs(mailbox, mailbox.RecentMessageCount, prevCount));
    }

    public event EventHandler<ImapMessageStatusChangedEventArgs> MessageDeleted;

    internal void RaiseMessageDeleted(ImapMessageInfo[] messages)
    {
      var ev = MessageDeleted;

      if (ev != null)
        ev(this, new ImapMessageStatusChangedEventArgs(messages));
    }

    public event EventHandler<ImapMessageStatusChangedEventArgs> MessageStatusChanged;

    internal void RaiseMessageStatusChanged(ImapMessageInfo[] messages)
    {
      var ev = MessageStatusChanged;

      if (ev != null)
        ev(this, new ImapMessageStatusChangedEventArgs(messages));
    }

    /*
     * LIST/LSUB
     */
    public ImapMailboxInfo GetInbox()
    {
      return GetMailbox(ImapMailbox.Inbox, ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetInbox(ImapMailboxListOptions options)
    {
      return GetMailbox(ImapMailbox.Inbox, options);
    }

    public ImapMailboxInfo GetMailbox(string mailboxName)
    {
      return GetMailbox(mailboxName, ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetMailbox(string mailboxName, ImapMailboxListOptions options)
    {
      var mailbox = GetMailboxNoException(mailboxName, options);

      if (mailbox == null)
        throw new ImapMailboxNotFoundException(mailboxName);
      else
        return mailbox;
    }

    internal ImapMailboxInfo GetMailboxNoException(string mailboxName, ImapMailboxListOptions options)
    {
      return GetMailboxes(mailboxName, null, false, false, options).FirstOrDefault();
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName)
    {
      return GetOrCreateMailbox(mailboxName, DefaultSubscription, ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName, ImapMailboxListOptions options)
    {
      return GetOrCreateMailbox(mailboxName, DefaultSubscription, options);
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName, bool subscribe)
    {
      return GetOrCreateMailbox(mailboxName, subscribe, ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName, bool subscribe, ImapMailboxListOptions options)
    {
      var mailbox = GetMailboxNoException(mailboxName, options);

      if (mailbox == null)
        return CreateMailbox(mailboxName, subscribe);
      else
        return mailbox;
    }

    public IEnumerable<ImapMailboxInfo> GetMailboxes()
    {
      return GetMailboxes(ImapMailboxListOptions.Default);
    }

    public IEnumerable<ImapMailboxInfo> GetMailboxes(ImapMailboxListOptions options)
    {
      return GetMailboxes(null, null, true, false, options);
    }

    internal IEnumerable<ImapMailboxInfo> GetMailboxes(ImapMailbox mailbox,
                                                       ImapMailboxListOptions options)
    {
      return GetMailboxes(mailbox.Name,
                          mailbox.HierarchyDelimiter,
                          true,
                          mailbox.Flags.Has(ImapMailboxFlag.NoInferiors), // ignore HasNoChildren
                          options);
    }

    internal IEnumerable<ImapMailboxInfo> GetMailboxes(string mailboxName,
                                                       string hierarchyDelimiter,
                                                       bool patternMatching,
                                                       bool isNonHierarchical,
                                                       ImapMailboxListOptions options)
    {
      string mailboxNamePattern;

      if (patternMatching) {
        if (mailboxName == null) {
          mailboxNamePattern = string.Empty;
        }
        else {
          if (string.IsNullOrEmpty(hierarchyDelimiter))
            yield break;
          else if (isNonHierarchical)
            yield break;

          mailboxNamePattern = mailboxName + hierarchyDelimiter;
        }

        mailboxNamePattern += ((int)(options & ImapMailboxListOptions.TopLevelOnly) != 0)
          ? "%"
          : "*";
      }
      else {
        mailboxNamePattern = mailboxName ?? string.Empty;
      }

      bool subscribedOnly, listRemote, requestStatus;

      TranslateListOptions(options, out subscribedOnly, out listRemote, out requestStatus);

      ImapMailbox[] mailboxes;

      if (ServerCapabilities.Has(ImapCapability.ListExtended)) {
        var selectionOptions = ImapListSelectionOptions.Empty;
        var returnOptions = ImapListReturnOptions.Children;

        if (subscribedOnly)
          selectionOptions += ImapListSelectionOptions.Subscribed;

        if (listRemote)
          selectionOptions += ImapListSelectionOptions.Remote;

        if (requestStatus && ServerCapabilities.Has(ImapCapability.ListStatus)) {
          returnOptions += ImapListReturnOptions.StatusDataItems(ImapMailboxInfo.GetStatusDataItem(ServerCapabilities));
          requestStatus = false; // no need to request STATUS
        }

        ThrowIfError(Session.ListExtended(mailboxNamePattern,
                                          selectionOptions,
                                          returnOptions,
                                          out mailboxes));
      }
      else if (ServerCapabilities.Has(ImapCapability.MailboxReferrals) &&
               listRemote) {
        if (subscribedOnly)
          ThrowIfError(Session.RLsub(mailboxNamePattern, out mailboxes));
        else
          ThrowIfError(Session.RList(mailboxNamePattern, out mailboxes));
      }
      else {
        if (subscribedOnly)
          ThrowIfError(Session.Lsub(mailboxNamePattern, out mailboxes));
        else
          ThrowIfError(Session.List(mailboxNamePattern, out mailboxes));
      }

      var selectedMailboxName = openedMailbox == null ? null : openedMailbox.FullName;

      foreach (var m in mailboxes) {
        if (requestStatus && !m.IsUnselectable && !string.Equals(m.Name, selectedMailboxName))
          ThrowIfError(Session.Status(m, ImapMailboxInfo.GetStatusDataItem(ServerCapabilities)));

        yield return new ImapMailboxInfo(this, m);
      }
    }

    private static void TranslateListOptions(ImapMailboxListOptions options,
                                             out bool subscribedOnly,
                                             out bool listRemote,
                                             out bool requestStatus)
    {
      subscribedOnly  = 0 != (int)(options & ImapMailboxListOptions.SubscribedOnly);
      listRemote      = 0 != (int)(options & ImapMailboxListOptions.Remote);
      requestStatus   = 0 != (int)(options & ImapMailboxListOptions.RequestStatus);
    }

    /*
     * CREATE
     */
    public ImapMailboxInfo CreateMailbox(string mailboxName)
    {
      return CreateMailbox(mailboxName, DefaultSubscription);
    }

    public ImapMailboxInfo CreateMailbox(string mailboxName, bool subscribe)
    {
      ImapMailbox createdMailbox;

      ThrowIfError(Session.Create(GetValidatedMailboxName(mailboxName), out createdMailbox));

      if (subscribe)
        ThrowIfError(Session.Subscribe(createdMailbox));

      // retrieve mailbox flags
      ThrowIfError(Session.List(createdMailbox));

      return new ImapMailboxInfo(this, createdMailbox);
    }

    /*
     * SELECT/EXAMINE
     */
    public ImapOpenedMailboxInfo OpenInbox()
    {
      return OpenMailbox(ImapMailbox.Inbox, DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenInbox(bool readOnly)
    {
      return OpenMailbox(ImapMailbox.Inbox, readOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapMailboxInfo mailbox)
    {
      return OpenMailbox(mailbox, DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapMailboxInfo mailbox, bool readOnly)
    {
      if (mailbox == null)
        throw new ArgumentNullException("mailbox");
      if (!mailbox.Exists)
        throw new ImapProtocolViolationException("mailbox is not existent");
      else if (mailbox.IsUnselectable)
        throw new ImapProtocolViolationException("mailbox is not existent");

      if (ServerCapabilities.Has(ImapCapability.CondStore))
        ThrowIfError(readOnly
                     ? Session.ExamineCondStore(mailbox.FullName)
                     : Session.SelectCondStore(mailbox.FullName));
      else
        ThrowIfError(readOnly
                     ? Session.Examine(mailbox.FullName)
                     : Session.Select(mailbox.FullName));

      openedMailbox = new ImapOpenedMailboxInfo(this, Session.SelectedMailbox);

      return openedMailbox;
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName)
    {
      return OpenMailbox(mailboxName, DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName, bool readOnly)
    {
      return OpenMailbox(GetMailbox(mailboxName), readOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName, ImapMailboxListOptions options)
    {
      return OpenMailbox(mailboxName, options, DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName, ImapMailboxListOptions options, bool readOnly)
    {
      return OpenMailbox(GetMailbox(mailboxName, options), readOnly);
    }

    /*
     * CLOSE
     */
    public void CloseMailbox()
    {
      /*
       *       The CLOSE command permanently removes all messages that have the
       *       \Deleted flag set from the currently selected mailbox, and returns
       *       to the authenticated state from the selected state.  No untagged
       *       EXPUNGE responses are sent.
       * 
       *       No messages are removed, and no error is given, if the mailbox is
       *       selected by an EXAMINE command or is otherwise selected read-only.
       */
      if (Session.SelectedMailbox != null)
        ThrowIfError(Session.Close());

      openedMailbox = null;
    }

    /*
     * NOOP
     */
    public void Refresh()
    {
      if (openedMailbox == null)
        ThrowIfError(Session.NoOp());
      else
        openedMailbox.ProcessResult(Session.NoOp());
    }

    /*
     * GETQUOTA
     */
    public ImapQuota GetQuota(string quotaRoot)
    {
      if (quotaRoot == null)
        throw new ArgumentNullException("quotaRoot");

      if (!IsCapable(ImapCapability.Quota))
        return null; // as default

      ImapQuota quota;

      ThrowIfError(Session.GetQuota(quotaRoot, out quota));

      return quota;
    }

    public double GetQuotaUsage(string quotaRoot, string resourceName)
    {
      if (quotaRoot == null)
        throw new ArgumentNullException("quotaRoot");
      if (string.IsNullOrEmpty(resourceName))
        throw new ArgumentException("must be non-empty value", "resourceName");

      var quota = GetQuota(quotaRoot);

      if (quota == null)
        return 0.0; // as default

      foreach (var resource in quota.Resources) {
        if (string.Equals(resource.Name, resourceName, StringComparison.OrdinalIgnoreCase)) {
          if (resource.Limit == 0L)
            return 0.0;
          else
            return (double)resource.Usage / (double)resource.Limit;
        }
      }

      throw new ImapProtocolViolationException(string.Format("no such resource name: '{0}'", resourceName));
    }

    /*
     * utilities
     */
    internal bool IsCapable(ImapCapability capability)
    {
      return ServerCapabilities.Has(capability);
    }

    internal bool IsSelected(ImapMailboxInfo mailbox)
    {
      bool discard;

      return IsSelected(mailbox, out discard);
    }

    internal bool IsSelected(ImapMailboxInfo mailbox, out bool readOnly)
    {
      var selected = Session.SelectedMailbox != null && string.Equals(Session.SelectedMailbox.Name, mailbox.Mailbox.Name);

      if (selected)
        readOnly = Session.SelectedMailbox.ReadOnly;
      else
        readOnly = false;

      return selected;
    }

    internal static string GetValidatedMailboxName(string mailboxName)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");
      else if (mailboxName.Length == 0)
        throw new ArgumentException("empty mailbox name is not allowed", "mailboxName");
      else if(mailboxName.Contains("%") || mailboxName.Contains("*"))
        throw new ArgumentException("mailbox name with wildcards is not allowed", "mailboxName");

      return mailboxName;
    }

    internal static ImapCommandResult ThrowIfError(ImapCommandResult result)
    {
      return ThrowIfError(result, null);
    }

    internal static ImapCommandResult ThrowIfError(ImapCommandResult result, Func<ImapResponseCode, bool> throwIfError)
    {
      if (result.Succeeded)
        return result;

      if (throwIfError != null && result.TaggedStatusResponse != null)
        if (!throwIfError(result.TaggedStatusResponse.ResponseText.Code))
          return result;

      throw new ImapErrorResponseException(result);
    }

    internal void ThrowIfIncapable(ImapCapability requiredCapability)
    {
      if (!ServerCapabilities.Has(requiredCapability))
        throw new ImapIncapableException(requiredCapability);
    }

    internal void ThrowIfNotSelected(ImapMailboxInfo mailbox)
    {
      if (!IsSelected(mailbox))
        throw new ImapMailboxClosedException(mailbox.FullName);
    }

    private ImapOpenedMailboxInfo openedMailbox;
  }
}
