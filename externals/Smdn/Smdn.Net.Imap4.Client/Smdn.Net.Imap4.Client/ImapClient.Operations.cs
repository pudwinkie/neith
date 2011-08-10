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

    public event EventHandler<ImapAlertReceivedEventArgs> AlertReceived;

    internal void RaiseAlertReceived(string alert, ImapResponseCondition condition)
    {
      var ev = AlertReceived;

      if (ev != null)
        ev(this, new ImapAlertReceivedEventArgs(alert, condition));
    }

    /*
     * LIST/LSUB
     */
    public ImapMailboxInfo GetInbox()
    {
      return GetMailbox(ImapMailbox.Inbox,
                        ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetInbox(ImapMailboxListOptions options)
    {
      return GetMailbox(ImapMailbox.Inbox,
                        options);
    }

    public ImapMailboxInfo GetMailbox(string mailboxName)
    {
      return GetMailbox(mailboxName,
                        ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetMailbox(string mailboxName,
                                      ImapMailboxListOptions options)
    {
      var mailbox = GetMailboxNoException(mailboxName,
                                          options);

      if (mailbox == null)
        throw new ImapMailboxNotFoundException(mailboxName);
      else
        return mailbox;
    }

    internal ImapMailboxInfo GetMailboxNoException(string mailboxName,
                                                   ImapMailboxListOptions options)
    {
      return GetMailboxes(GetValidatedMailboxName(mailboxName),
                          false,
                          options,
                          null,
                          ListMethod.SelectAppropriate,
                          delegate(ImapMailbox m) {
        return ImapMailbox.NameEquals(m.Name, mailboxName);
      }).FirstOrDefault();
    }

    public ImapMailboxInfo GetMailbox(ImapSpecialMailbox mailbox)
    {
      return GetMailbox(mailbox,
                        ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetMailbox(ImapSpecialMailbox mailbox,
                                      ImapMailboxListOptions options)
    {
      var m = GetMailboxNoException(mailbox, options);

      if (m == null)
        throw new ImapMailboxNotFoundException(mailbox.ToString()/*TODO*/);
      else
        return m;
    }

    private ImapMailboxInfo GetMailboxNoException(ImapSpecialMailbox mailbox,
                                                  ImapMailboxListOptions options)
    {
      if (ServerCapabilities.Contains(ImapCapability.SpecialUse)) {
        var expectedUseFlag = TranslateSpecialMailboxUseFlag(mailbox, true);

        if (ServerCapabilities.Contains(ImapCapability.ListExtended)) {
          return GetMailboxes(string.Empty,
                              true,
                              options,
                              ImapListSelectionOptions.SpecialUse,
                              ListMethod.ListExtended,
                              delegate(ImapMailbox m) {
            return m.Flags.Contains(expectedUseFlag);
          }).FirstOrDefault();
        }
        else {
          return GetMailboxes(string.Empty,
                              true,
                              options,
                              null,
                              ListMethod.ListOrListRemote,
                              delegate(ImapMailbox m) {
            return m.Flags.Contains(expectedUseFlag);
          }).FirstOrDefault();
        }
      }
      else if (ServerCapabilities.Contains(ImapCapability.GimapXlist)) {
        var expectedUseFlag = TranslateSpecialMailboxUseFlag(mailbox, false);

        return GetMailboxes(string.Empty,
                            true,
                            options,
                            null,
                            ListMethod.XList,
                            delegate(ImapMailbox m) {
          return m.Flags.Contains(expectedUseFlag);
        }).FirstOrDefault();
      }
      else {
        throw new ImapIncapableException(ImapCapability.SpecialUse);
      }
    }

    private static ImapMailboxFlag TranslateSpecialMailboxUseFlag(ImapSpecialMailbox mailbox,
                                                                  bool standard)
    {
      switch (mailbox) {
        case ImapSpecialMailbox.All:      return standard ? ImapMailboxFlag.All     : ImapMailboxFlag.GimapAllMail;
        case ImapSpecialMailbox.Archive:  return            ImapMailboxFlag.Archive;
        case ImapSpecialMailbox.Drafts:   return            ImapMailboxFlag.Drafts;
        case ImapSpecialMailbox.Flagged:  return standard ? ImapMailboxFlag.Flagged : ImapMailboxFlag.GimapStarred;
        case ImapSpecialMailbox.Junk:     return standard ? ImapMailboxFlag.Junk    : ImapMailboxFlag.GimapSpam;
        case ImapSpecialMailbox.Sent:     return            ImapMailboxFlag.Sent;
        case ImapSpecialMailbox.Trash:    return            ImapMailboxFlag.Trash;

        default:
          throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("mailbox", mailbox);
      }
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName)
    {
      return GetOrCreateMailbox(mailboxName,
                                DefaultSubscription,
                                ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName,
                                              ImapMailboxListOptions options)
    {
      return GetOrCreateMailbox(mailboxName,
                                DefaultSubscription,
                                options);
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName,
                                              bool subscribeIfCreated)
    {
      return GetOrCreateMailbox(mailboxName,
                                subscribeIfCreated,
                                ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateMailbox(string mailboxName,
                                              bool subscribeIfCreated,
                                              ImapMailboxListOptions options)
    {
      var mailbox = GetMailboxNoException(mailboxName, options);

      if (mailbox == null)
        return CreateMailbox(mailboxName, subscribeIfCreated);
      else
        return mailbox;
    }

#if false
    public ImapMailboxInfo GetOrCreateMailbox(ImapSpecialMailbox mailbox)
    {
      return GetOrCreateMailbox(mailbox,
                                DefaultSubscription,
                                ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateMailbox(ImapSpecialMailbox mailbox,
                                              ImapMailboxListOptions options)
    {
      return GetOrCreateMailbox(mailbox,
                                DefaultSubscription,
                                options);
    }

    public ImapMailboxInfo GetOrCreateMailbox(ImapSpecialMailbox mailbox,
                                              bool subscribeIfCreated)
    {
      return GetOrCreateMailbox(mailbox,
                                subscribeIfCreated,
                                ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateMailbox(ImapSpecialMailbox mailbox,
                                              bool subscribeIfCreated,
                                              ImapMailboxListOptions options)
    {
      var m = GetMailboxNoException(mailbox, options);

      if (m == null)
        return CreateMailbox(mailbox, subscribeIfCreated);
      else
        return m;
    }
#endif

    public IEnumerable<ImapMailboxInfo> GetMailboxes()
    {
      return GetMailboxes(string.Empty,
                          true,
                          ImapMailboxListOptions.Default,
                          null,
                          ListMethod.SelectAppropriate,
                          null);
    }

    public IEnumerable<ImapMailboxInfo> GetMailboxes(ImapMailboxListOptions options)
    {
      return GetMailboxes(string.Empty,
                          true,
                          options,
                          null,
                          ListMethod.SelectAppropriate,
                          null);
    }

    internal IEnumerable<ImapMailboxInfo> GetMailboxes(ImapMailbox mailbox,
                                                       ImapMailboxListOptions options)
    {
      if (mailbox.Flags.Contains(ImapMailboxFlag.NoInferiors))
        // non hierarchical (ignore \HasNoChildren)
        return Enumerable.Empty<ImapMailboxInfo>();
      else if (string.IsNullOrEmpty(mailbox.HierarchyDelimiter))
        // non hierarchical
        return Enumerable.Empty<ImapMailboxInfo>();
      else
        return GetMailboxes(mailbox.Name + mailbox.HierarchyDelimiter,
                            true,
                            options,
                            null,
                            ListMethod.SelectAppropriate,
                            null);
    }

    private enum ListMethod {
      SelectAppropriate,
      ListOrListRemote,
      ListExtended,
      ListRemote,
      XList,
      List,
    }

    private IEnumerable<ImapMailboxInfo> GetMailboxes(string mailboxName,
                                                      bool patternMatching,
                                                      ImapMailboxListOptions options,
                                                      ImapListSelectionOptions selectionOptions,
                                                      ListMethod listMethod,
                                                      Predicate<ImapMailbox> match)
    {
      string referenceName, mailboxNamePattern;

      if (patternMatching) {
        referenceName = mailboxName;
        mailboxNamePattern = ((int)(options & ImapMailboxListOptions.TopLevelOnly) != 0)
          ? "%"
          : "*";
      }
      else {
        referenceName = string.Empty;
        mailboxNamePattern = mailboxName;
      }

      bool subscribedOnly, listRemote, requestStatus;

      TranslateListOptions(options, out subscribedOnly, out listRemote, out requestStatus);

      ImapMailbox[] mailboxes;

      switch (listMethod) {
        case ListMethod.SelectAppropriate:
          if (ServerCapabilities.Contains(ImapCapability.ListExtended))
            goto case ListMethod.ListExtended;
          else if (listRemote &&
                   ServerCapabilities.Contains(ImapCapability.MailboxReferrals))
            goto case ListMethod.ListRemote;
          else
            goto case ListMethod.List;

        case ListMethod.ListOrListRemote:
          if (listRemote)
            goto case ListMethod.ListRemote;
          else
            goto case ListMethod.List;

        case ListMethod.ListExtended: {
          var returnOptions = ImapListReturnOptions.Children;

          if (selectionOptions == null)
            selectionOptions = ImapListSelectionOptions.Empty;

          if (subscribedOnly)
            selectionOptions += ImapListSelectionOptions.Subscribed;

          if (listRemote)
            selectionOptions += ImapListSelectionOptions.Remote;

          if (requestStatus && ServerCapabilities.Contains(ImapCapability.ListStatus)) {
            returnOptions += ImapListReturnOptions.StatusDataItems(ImapMailboxInfo.GetStatusDataItem(ServerCapabilities));
            requestStatus = false; // no need to request STATUS
          }

          ProcessResult(Session.ListExtended(referenceName,
                                             new[] {mailboxNamePattern},
                                             selectionOptions,
                                             returnOptions,
                                             out mailboxes));

          break;
        }

        case ListMethod.ListRemote: {
          if (subscribedOnly)
            ProcessResult(Session.RLsub(referenceName,
                                        mailboxNamePattern,
                                        out mailboxes));
          else
            ProcessResult(Session.RList(referenceName,
                                        mailboxNamePattern,
                                        out mailboxes));

          break;
        }

        case ListMethod.XList: {
          ProcessResult(Session.XList(referenceName,
                                      mailboxNamePattern,
                                      out mailboxes));

          break;
        }

        case ListMethod.List: {
          if (subscribedOnly)
            ProcessResult(Session.Lsub(referenceName,
                                       mailboxNamePattern,
                                       out mailboxes));
          else
            ProcessResult(Session.List(referenceName,
                                       mailboxNamePattern,
                                       out mailboxes));

          break;
        }

        default:
          throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("listMethod", listMethod);
      }

      var selectedMailboxName = openedMailbox == null ? null : openedMailbox.FullName;

      foreach (var m in mailboxes) {
        if (match != null && !match(m))
          continue;

        if (requestStatus && !m.IsUnselectable && !m.NameEquals(selectedMailboxName))
          ProcessResult(Session.Status(m, ImapMailboxInfo.GetStatusDataItem(ServerCapabilities)));

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

    public ImapMailboxInfo CreateMailbox(string mailboxName,
                                         bool subscribeIfCreated)
    {
      ImapMailbox createdMailbox;

      ProcessResult(Session.Create(GetValidatedMailboxName(mailboxName),
                                   out createdMailbox));

      if (subscribeIfCreated)
        ProcessResult(Session.Subscribe(createdMailbox));

      return CreateFlagsRetrieved(createdMailbox);
    }

    internal ImapMailboxInfo CreateFlagsRetrieved(ImapMailbox mailbox)
    {
      // retrieve mailbox flags
      ProcessResult(Session.List(mailbox));

      return new ImapMailboxInfo(this, mailbox);
    }

#if false
    public ImapMailboxInfo CreateMailbox(ImapSpecialMailbox mailbox)
    {
      return CreateMailbox(mailbox, DefaultSubscription);
    }

    public ImapMailboxInfo CreateMailbox(ImapSpecialMailbox mailbox,
                                         bool subscribeIfCreated)
    {
      throw new NotImplementedException();
    }
#endif

    /*
     * SELECT/EXAMINE
     */
    public ImapOpenedMailboxInfo OpenInbox()
    {
      return OpenMailbox(ImapMailbox.Inbox,
                         DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenInbox(bool asReadOnly)
    {
      return OpenMailbox(ImapMailbox.Inbox,
                         asReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapMailboxInfo mailbox)
    {
      return OpenMailbox(mailbox,
                         DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapMailboxInfo mailbox,
                                             bool asReadOnly)
    {
      if (mailbox == null)
        throw new ArgumentNullException("mailbox");
      if (mailbox.Client != this)
        throw new ArgumentException("can't open mailbox of different session");

      mailbox.ThrowIfNonExistentOrUnselectable();

      if (ServerCapabilities.Contains(ImapCapability.CondStore))
        ProcessResult(asReadOnly
                      ? Session.ExamineCondStore(mailbox.FullName)
                      : Session.SelectCondStore(mailbox.FullName));
      else
        ProcessResult(asReadOnly
                      ? Session.Examine(mailbox.FullName)
                      : Session.Select(mailbox.FullName));

      openedMailbox = new ImapOpenedMailboxInfo(this, Session.SelectedMailbox);

      return openedMailbox;
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName)
    {
      return OpenMailbox(mailboxName,
                         DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName,
                                             bool asReadOnly)
    {
      return OpenMailbox(GetMailbox(mailboxName),
                         asReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName,
                                             ImapMailboxListOptions options)
    {
      return OpenMailbox(mailboxName,
                         options,
                         DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(string mailboxName,
                                             ImapMailboxListOptions options,
                                             bool asReadOnly)
    {
      return OpenMailbox(GetMailbox(mailboxName, options),
                         asReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapSpecialMailbox mailbox)
    {
      return OpenMailbox(mailbox,
                         DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapSpecialMailbox mailbox,
                                             bool asReadOnly)
    {
      return OpenMailbox(GetMailbox(mailbox),
                         asReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapSpecialMailbox mailbox,
                                             ImapMailboxListOptions options)
    {
      return OpenMailbox(mailbox,
                         options,
                         DefaultSelectReadOnly);
    }

    public ImapOpenedMailboxInfo OpenMailbox(ImapSpecialMailbox mailbox,
                                             ImapMailboxListOptions options,
                                             bool asReadOnly)
    {
      return OpenMailbox(GetMailbox(mailbox, options),
                         asReadOnly);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(string mailboxName)
    {
      return OpenOrCreateMailbox(mailboxName,
                                 DefaultSubscription,
                                 DefaultSelectReadOnly,
                                 ImapMailboxListOptions.Default);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(string mailboxName,
                                                     ImapMailboxListOptions options)
    {
      return OpenOrCreateMailbox(mailboxName,
                                 DefaultSubscription,
                                 DefaultSelectReadOnly,
                                 options);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(string mailboxName,
                                                     bool asReadOnly)
    {
      return OpenOrCreateMailbox(mailboxName,
                                 DefaultSubscription,
                                 asReadOnly,
                                 ImapMailboxListOptions.Default);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(string mailboxName,
                                                     bool subscribeIfCreated,
                                                     bool asReadOnly,
                                                     ImapMailboxListOptions options)
    {
      // XXX: remove ImapMailboxListOptions.RequestStatus
      var mailbox = GetMailboxNoException(mailboxName, options);

      if (mailbox == null)
        mailbox = CreateMailbox(mailboxName, subscribeIfCreated);

      return OpenMailbox(mailbox, asReadOnly);
    }

#if false
    public ImapOpenedMailboxInfo OpenOrCreateMailbox(ImapSpecialMailbox mailbox)
    {
      return OpenOrCreateMailbox(mailbox,
                                 DefaultSubscription,
                                 DefaultSelectReadOnly,
                                 ImapMailboxListOptions.Default);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(ImapSpecialMailbox mailbox,
                                                     ImapMailboxListOptions options)
    {
      return OpenOrCreateMailbox(mailbox,
                                 DefaultSubscription,
                                 DefaultSelectReadOnly,
                                 options);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(ImapSpecialMailbox mailbox,
                                                     bool asReadOnly)
    {
      return OpenOrCreateMailbox(mailbox,
                                 DefaultSubscription,
                                 asReadOnly,
                                 ImapMailboxListOptions.Default);
    }

    public ImapOpenedMailboxInfo OpenOrCreateMailbox(ImapSpecialMailbox mailbox,
                                                     bool subscribeIfCreated,
                                                     bool asReadOnly,
                                                     ImapMailboxListOptions options)
    {
      // XXX: remove ImapMailboxListOptions.RequestStatus
      var m = GetMailboxNoException(mailbox, options);

      if (m == null)
        m = CreateMailbox(mailbox, subscribeIfCreated);

      return OpenMailbox(m, asReadOnly);
    }
#endif

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
        ProcessResult(Session.Close());

      openedMailbox = null;
    }

    /*
     * NOOP
     */
    public void Refresh()
    {
      if (openedMailbox == null)
        ProcessResult(Session.NoOp());
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

      if (!ServerCapabilities.Contains(ImapCapability.Quota))
        return null; // as default

      ImapQuota quota;

      ProcessResult(Session.GetQuota(quotaRoot, out quota));

      return quota;
    }

    public double GetQuotaUsage(string quotaRoot, string resourceName)
    {
      if (quotaRoot == null)
        throw new ArgumentNullException("quotaRoot");
      if (resourceName == null)
        throw new ArgumentNullException("resourceName");
      if (resourceName.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("resourceName");

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
    internal bool IsSelected(ImapMailboxInfo mailbox)
    {
      bool discard;

      return IsSelected(mailbox, out discard);
    }

    internal bool IsSelected(ImapMailboxInfo mailbox, out bool readOnly)
    {
      var selected = Session.SelectedMailbox != null && Session.SelectedMailbox.NameEquals(mailbox.Mailbox);

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
      if (mailboxName.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("mailboxName");

      return mailboxName;
    }

    internal protected ImapCommandResult ProcessResult(ImapCommandResult result)
    {
      return ProcessResult(result, null);
    }

    internal protected virtual ImapCommandResult ProcessResult(ImapCommandResult result,
                                                               Func<ImapResponseCode, bool> throwIfError)
    {
      var alert = result.GetResponseCode(ImapResponseCode.Alert);

      if (alert != null)
        RaiseAlertReceived(alert.ResponseText.Text,
                           alert.Condition);

      if (result.Succeeded)
        return result;

      if (throwIfError != null && result.TaggedStatusResponse != null)
        if (!throwIfError(result.TaggedStatusResponse.ResponseText.Code))
          return result;

      if (result.Code == ImapCommandResultCode.Bye)
        throw new ImapConnectionException(string.Format("disconnected from server: \"{0}\"",
                                                        result.ResponseText));
      else
        throw new ImapErrorResponseException(result);
    }

    internal void ThrowIfIncapable(ImapCapability requiredCapability)
    {
      if (!ServerCapabilities.Contains(requiredCapability))
        throw new ImapIncapableException(requiredCapability);
    }

    private ImapOpenedMailboxInfo openedMailbox;
  }
}
