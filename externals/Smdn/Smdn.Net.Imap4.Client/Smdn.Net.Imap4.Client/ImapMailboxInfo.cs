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

using Smdn.IO;
using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  public class ImapMailboxInfo : IImapUrl {
    public ImapClient Client {
      get { return client; }
    }

    public Uri Url {
      get { return mailbox.Url; }
    }

    public string FullName {
      get { return mailbox.Name; }
    }

    public string Name {
      get { return mailbox.LeafName; }
    }

    public string ParentMailboxName {
      get { return mailbox.SuperiorName; }
    }

    public string MailboxSeparator {
      get { return mailbox.HierarchyDelimiter; }
    }

    public long UidValidity {
      get { return mailbox.UidValidity; }
    }

    public long UnseenMessageCount {
      get { return mailbox.UnseenMessage; }
    }

    public long ExistMessageCount {
      get { return mailbox.ExistsMessage; }
    }

    public long RecentMessageCount {
      get { return mailbox.RecentMessage; }
    }

    public long NextUid {
      get { return mailbox.UidNext; }
    }

    public IImapMailboxFlagSet Flags {
      get { return mailbox.Flags; }
    }

    [CLSCompliant(false)]
    public ulong HighestModSeq {
      get { return mailbox.HighestModSeq; }
    }

    public bool IsModSequencesAvailable {
      get { return mailbox.ModificationSequences || 0 < mailbox.HighestModSeq; }
    }

    public bool Exists {
      get { return !deleted && !mailbox.Flags.Contains(ImapMailboxFlag.NonExistent); }
    }

    public bool IsUnselectable {
      get { return mailbox.IsUnselectable; }
    }

    public bool IsOpen {
      get { return client.IsSelected(this); }
    }

    public bool IsInbox {
      get { return mailbox.IsInbox; }
    }

    public bool CanHaveChild {
      get { return !CanNotHaveChild; }
    }

    protected bool CanNotHaveChild {
      get { return mailbox.Flags.Contains(ImapMailboxFlag.NoInferiors) || string.IsNullOrEmpty(mailbox.HierarchyDelimiter); }
    }

    internal protected ImapMailbox Mailbox {
      get { return mailbox; }
    }

    internal ImapMailboxInfo(ImapClient client, ImapMailbox mailbox)
    {
      this.client = client;
      this.mailbox = mailbox;
    }

    private ImapClient client;
    private ImapMailbox mailbox;
    private bool deleted = false;

    /*
     * operations
     */
    public virtual void Refresh()
    {
      if (!Exists || mailbox.IsUnselectable)
        return;

      ProcessResult(client.Session.Status(mailbox,
                                          GetStatusDataItem(client.ServerCapabilities)));
    }

    internal static ImapStatusDataItem GetStatusDataItem(ImapCapabilitySet serverCapabilities)
    {
      if (serverCapabilities.Contains(ImapCapability.CondStore))
        return ImapStatusDataItem.StandardAll + ImapStatusDataItem.HighestModSeq;
      else
        return ImapStatusDataItem.StandardAll;
    }

    /*
     * SELECT/EXAMINE
     */
    public ImapOpenedMailboxInfo Open()
    {
      return Open(ImapClient.DefaultSelectReadOnly);
    }

    public virtual ImapOpenedMailboxInfo Open(bool asReadOnly)
    {
      return client.OpenMailbox(this, asReadOnly);
    }

    /*
     * DELETE
     */
    public void Delete()
    {
      Delete(ImapClient.DefaultSubscription);
    }

    public void Delete(bool unsubscribe)
    {
      if (client.IsSelected(this))
        client.CloseMailbox();

      if (Exists)
        ProcessResult(client.Session.Delete(mailbox),
                      delegate(ImapResponseCode code) {
          if (code == ImapResponseCode.NonExistent)
            return false; // throw no exception
          else
            return true;
        });

      deleted = true;

      if (unsubscribe)
        ProcessResult(client.Session.Unsubscribe(mailbox.Name)); // mailbox is detached already
    }

    /*
     * CREATE
     */
    public void Create()
    {
      Create(ImapClient.DefaultSubscription);
    }

    public void Create(bool subscribe)
    {
      ProcessResult(client.Session.Create(mailbox.Name),
                    delegate(ImapResponseCode code) {
        if (code == ImapResponseCode.AlreadyExists)
          return false; // throw no exception
        else
          return true;
      });

      deleted = false;

      // remove \NonExistent flag
      if (mailbox.Flags.Contains(ImapMailboxFlag.NonExistent)) {
        var flags = new ImapMailboxFlagSet(mailbox.Flags);

        flags.Remove(ImapMailboxFlag.NonExistent);

        mailbox.Flags = flags.AsReadOnly();
      }

      if (subscribe)
        ProcessResult(client.Session.Subscribe(mailbox.Name));
    }

    public static ImapMailboxInfo Create(ImapClient client,
                                         string mailboxName)
    {
      return Create(client,
                    mailboxName,
                    ImapClient.DefaultSubscription);
    }

    public static ImapMailboxInfo Create(ImapClient client,
                                         string mailboxName,
                                         bool subscribe)
    {
      if (client == null)
        throw new ArgumentNullException("client");

      return client.CreateMailbox(mailboxName, subscribe);
    }

    /*
     * RENAME
     */
    public void MoveTo(ImapMailboxInfo destinationMailbox)
    {
      MoveTo(destinationMailbox, ImapClient.DefaultSubscription);
    }

    public void MoveTo(ImapMailboxInfo destinationMailbox,
                       bool subscribe)
    {
      if (destinationMailbox == null)
        throw new ArgumentNullException("destinationMailbox");
      if (destinationMailbox.client != this.client)
        // TODO: impl
        throw new NotImplementedException("moving to mailbox of different session is currently not supported");

      destinationMailbox.ThrowIfCanNotHaveChild();

      MoveTo(destinationMailbox.GetFullNameOf(mailbox.LeafName),
             subscribe);
    }

    public void MoveTo(string newMailboxName)
    {
      MoveTo(newMailboxName, ImapClient.DefaultSubscription);
    }

    public void MoveTo(string newMailboxName,
                       bool subscribe)
    {
      ImapClient.GetValidatedMailboxName(newMailboxName);

      if (mailbox.NameEquals(newMailboxName))
        throw new ImapProtocolViolationException("new mailbox name must be different from current mailbox name");
      else if (ImapMailbox.IsNameInbox(newMailboxName))
        throw new ImapProtocolViolationException("can't move to INBOX");

      bool readOnly;
      var selected = client.IsSelected(this, out readOnly);
      var isInbox = mailbox.IsInbox;

      var oldMailboxName = mailbox.Name;

      if (selected)
        client.CloseMailbox();

      Dictionary<string, ImapMailboxInfo> subscribingChildren = null;

      if (subscribe && !isInbox) {
        subscribingChildren = new Dictionary<string, ImapMailboxInfo>();

        foreach (var subscribingChild in client.GetMailboxes(mailbox, ImapMailboxListOptions.SubscribedOnly)) {
          subscribingChildren.Add(subscribingChild.FullName, subscribingChild);
        }
      }

      ProcessResult(client.Session.Rename(mailbox, newMailboxName));

      if (subscribe) {
        if (!isInbox)
          // don't UNSUBSCRIBE old INBOX
          ProcessResult(client.Session.Unsubscribe(oldMailboxName));

        if (Exists)
          ProcessResult(client.Session.Subscribe(newMailboxName));

        if (!isInbox) {
          foreach (var pair in subscribingChildren) {
            ProcessResult(client.Session.Unsubscribe(pair.Key));

            if (pair.Value.Exists)
              pair.Value.Subscribe(); // mailbox name is renamed
          }
        }
      }

      if (selected)
        client.OpenMailbox(this, readOnly);
    }

#if false
    public void CopyTo(ImapMailboxInfo destinationMailbox)
    {
      CopyTo(destinationMailbox, ImapClient.DefaultSubscription);
    }

    public void CopyTo(ImapMailboxInfo destinationMailbox,
                       bool subscribe)
    {
      if (destinationMailbox == null)
        throw new ArgumentNullException("destinationMailbox");
      if (destinationMailbox.client != this.client)
        // TODO: impl
        throw new NotImplementedException("moving to mailbox of different session is currently not supported");
    }

    public void CopyTo(string newMailboxName)
    {
      MoveTo(newMailboxName, ImapClient.DefaultSubscription);
    }

    public void CopyTo(string newMailboxName,
                       bool subscribe)
    {
      if (newMailboxName == null)
        throw new ArgumentNullException("newMailboxName");
      else if (mailbox.NameEquals(newMailboxName))
        throw new ImapProtocolViolationException("new mailbox name must be different from current mailbox name");
      else if (ImapMailbox.IsNameInbox(newMailboxName))
        throw new ImapProtocolViolationException("can't move to INBOX");
    }
#endif

    /*
     * SUBSCRIBE/UNSUBSCRIBE
     */
    public void Subscribe()
    {
      Subscribe(false);
    }

    public void Subscribe(bool recursive)
    {
      ProcessResult(client.Session.Subscribe(mailbox));

      if (!recursive)
        return;

      foreach (var childMailbox in GetMailboxes(ImapMailboxListOptions.Default)) {
        ProcessResult(client.Session.Subscribe(childMailbox.mailbox));
      }
    }

    public void Unsubscribe()
    {
      Unsubscribe(false);
    }

    public void Unsubscribe(bool recursive)
    {
      ProcessResult(client.Session.Unsubscribe(mailbox));

      if (!recursive)
        return;

      foreach (var childMailbox in GetMailboxes(ImapMailboxListOptions.SubscribedOnly)) {
        ProcessResult(client.Session.Unsubscribe(childMailbox.mailbox));
      }
    }

    /*
     * LIST/LSUB/CREATE
     */
    public IEnumerable<ImapMailboxInfo> GetMailboxes()
    {
      return GetMailboxes(ImapMailboxListOptions.Default);
    }

    public IEnumerable<ImapMailboxInfo> GetMailboxes(ImapMailboxListOptions options)
    {
      return client.GetMailboxes(mailbox, options);
    }

    public ImapMailboxInfo GetParent()
    {
      return GetParent(ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetParent(ImapMailboxListOptions options)
    {
      if (string.Empty.Equals(mailbox.SuperiorName))
        return null; // TODO: throw exception

      return client.GetMailbox(mailbox.SuperiorName, options);
    }

    public ImapMailboxInfo GetOrCreateParent()
    {
      return GetOrCreateParent(ImapClient.DefaultSubscription,
                               ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateParent(ImapMailboxListOptions options)
    {
      return GetOrCreateParent(ImapClient.DefaultSubscription,
                               options);
    }

    public ImapMailboxInfo GetOrCreateParent(bool subscribeIfCreated)
    {
      return GetOrCreateParent(subscribeIfCreated,
                               ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateParent(bool subscribeIfCreated,
                                             ImapMailboxListOptions options)
    {
      if (string.Empty.Equals(mailbox.SuperiorName))
        return null; // TODO: throw exception

      return client.GetOrCreateMailbox(mailbox.SuperiorName,
                                       subscribeIfCreated,
                                       options);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public string GetFullNameOf(string childName)
    {
      // XXX: use ImapClient.GetValidatedMailboxName
      if (childName == null)
        throw new ArgumentNullException("childName");
      if (childName.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("childName");

      ThrowIfCanNotHaveChild();

      return mailbox.GetChildName(childName);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo CreateChild(string name)
    {
      return CreateChild(name, ImapClient.DefaultSubscription);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo CreateChild(string name,
                                       bool subscribe)
    {
      return client.CreateMailbox(GetFullNameOf(name), subscribe);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetChild(string name)
    {
      return GetChild(name, ImapMailboxListOptions.Default);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetChild(string name,
                                    ImapMailboxListOptions options)
    {
      return client.GetMailbox(GetFullNameOf(name), options);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name)
    {
      return GetOrCreateChild(name,
                              ImapClient.DefaultSubscription,
                              ImapMailboxListOptions.Default);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name,
                                            ImapMailboxListOptions options)
    {
      return GetOrCreateChild(name,
                              ImapClient.DefaultSubscription,
                              options);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name,
                                            bool subscribeIfCreated)
    {
      return GetOrCreateChild(name,
                              subscribeIfCreated,
                              ImapMailboxListOptions.Default);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name,
                                            bool subscribeIfCreated,
                                            ImapMailboxListOptions options)
    {
      var mailbox = client.GetMailboxNoException(GetFullNameOf(name), options);

      if (mailbox == null)
        return CreateChild(name, subscribeIfCreated);
      else
        return mailbox;
    }

    /*
     * APPEND
     */
    public void AppendMessage(Stream stream)
    {
      AppendMessage(new ImapAppendMessage(stream));
    }

    public void AppendMessage(Stream stream,
                              DateTimeOffset internalDate)
    {
      AppendMessage(new ImapAppendMessage(stream,
                                          internalDate));
    }

    public void AppendMessage(Stream stream,
                              IImapMessageFlagSet flags)
    {
      AppendMessage(new ImapAppendMessage(stream,
                                          flags));
    }

    public void AppendMessage(Stream stream,
                              DateTimeOffset internalDate,
                              IImapMessageFlagSet flags)
    {
      AppendMessage(new ImapAppendMessage(stream,
                                          internalDate,
                                          flags));
    }

    public void AppendMessage(IImapAppendMessage message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      ThrowIfNonExistentOrUnselectable();

      ProcessResult(Client.Session.Append(message, Mailbox),
                    ThrowIfAppendErrorResponse);
    }

    public void AppendMessages(IEnumerable<IImapAppendMessage> messages)
    {
      if (messages == null)
        throw new ArgumentNullException("messages");

      ThrowIfNonExistentOrUnselectable();

      if (Client.ServerCapabilities.Contains(ImapCapability.MultiAppend)) {
        ProcessResult(Client.Session.AppendMultiple(messages, Mailbox),
                      ThrowIfAppendErrorResponse);
      }
      else {
        foreach (var message in messages) {
          ProcessResult(Client.Session.Append(message, Mailbox),
                        ThrowIfAppendErrorResponse);
        }
      }
    }

    public void AppendMessage(Action<Stream> writeMessage)
    {
      AppendMessageCore(null,
                        null,
                        null,
                        writeMessage);
    }

    public void AppendMessage(long length,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(length,
                        null,
                        null,
                        writeMessage);
    }

    public void AppendMessage(DateTimeOffset internalDate,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(null,
                        internalDate,
                        null,
                        writeMessage);
    }

    public void AppendMessage(IImapMessageFlagSet flags,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(null,
                        null,
                        flags,
                        writeMessage);
    }

    public void AppendMessage(DateTimeOffset internalDate,
                              IImapMessageFlagSet flags,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(null,
                        internalDate,
                        flags,
                        writeMessage);
    }

    public void AppendMessage(long length,
                              DateTimeOffset internalDate,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(length,
                        internalDate,
                        null,
                        writeMessage);
    }

    public void AppendMessage(long length,
                              IImapMessageFlagSet flags,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(length,
                        null,
                        flags,
                        writeMessage);
    }

    public void AppendMessage(long length,
                              DateTimeOffset internalDate,
                              IImapMessageFlagSet flags,
                              Action<Stream> writeMessage)
    {
      AppendMessageCore(length,
                        internalDate,
                        flags,
                        writeMessage);
    }

    public void AppendMessage(ImapMessageInfo message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      if (message.Client == this.client)
        message.CopyTo(this);
      else
        AppendMessageInternal(message);
    }

    internal void AppendMessageInternal(ImapMessageInfo message)
    {
      var ret = message.OpenRead(string.Empty,
                                 null,
                                 null,
                                 ImapMessageFetchBodyOptions.Default,
                                 ImapFetchDataItem.Rfc822Size + ImapFetchDataItem.InternalDate + ImapFetchDataItem.Flags);

      using (var sourceStream = ret.Item1) {
        AppendMessageCore(ret.Item2.Rfc822Size,
                          ret.Item2.InternalDate,
                          ret.Item2.Flags,
                          delegate(Stream destStream) {
          sourceStream.CopyTo(destStream,
                              ImapFetchMessageBodyStream.DefaultFetchBlockSize);
        });
      }
    }

    private void AppendMessageCore(long? length,
                                   DateTimeOffset? internalDate,
                                   IImapMessageFlagSet flags,
                                   Action<Stream> writeMessage)
    {
      if (writeMessage == null)
        throw new ArgumentNullException("writeMessage");
      if (length.HasValue && length.Value < 0L)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("length", length.Value);

      ThrowIfNonExistentOrUnselectable();

      var appendContext = client.Session.PrepareAppend(length,
                                                       internalDate,
                                                       flags,
                                                       mailbox);

      writeMessage(appendContext.WriteStream);

      ImapAppendedUidSet appendedUidSet;

      ProcessResult(appendContext.GetResult(out appendedUidSet),
                    ThrowIfAppendErrorResponse);
    }

    private bool ThrowIfAppendErrorResponse(ImapResponseCode code)
    {
      if (code == ImapResponseCode.TryCreate)
        throw new ImapMailboxNotFoundException(FullName);
      else
        return true;
    }

    /*
     * GETQUOTAROOT
     */
    public IEnumerable<ImapQuota> GetQuota()
    {
      if (!client.ServerCapabilities.Contains(ImapCapability.Quota))
        yield break;

      IDictionary<string, ImapQuota[]> quotaRoots;

      ProcessResult(client.Session.GetQuotaRoot(Mailbox, out quotaRoots));

      foreach (var pair in quotaRoots) {
        foreach (var quota in pair.Value) {
          yield return quota;
        }
      }
    }

    internal protected ImapCommandResult ProcessResult(ImapCommandResult result)
    {
      return ProcessResult(result, null);
    }

    internal protected virtual ImapCommandResult ProcessResult(ImapCommandResult result,
                                                               Func<ImapResponseCode, bool> throwIfError)
    {
      return client.ProcessResult(result, throwIfError);
    }

    private void ThrowIfCanNotHaveChild()
    {
      if (CanNotHaveChild)
        throw new NotSupportedException(string.Format("It is not possible for any child levels of hierarchy to exist under this mailbox. (mailbox name: '{0}')", FullName));
    }

    internal void ThrowIfNonExistentOrUnselectable()
    {
      if (!Exists)
        throw new ImapProtocolViolationException(string.Format("mailbox '{0}' is not existent", FullName));
      if (mailbox.IsUnselectable)
        throw new ImapProtocolViolationException(string.Format("mailbox '{0}' is unselectable", FullName));
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    void IImapUrl.SetBaseUrl(ImapUriBuilder baseUrl)
    {
      throw new NotSupportedException();
    }

    public override string ToString()
    {
      return string.Format("{{ImapMailboxInfo: Authority='{0}', FullName='{1}', UidValidity={2}, NextUid={3}}}",
                           ImapStyleUriParser.GetStrongAuthority(Url),
                           FullName,
                           UidValidity,
                           NextUid);
    }
  }
}
