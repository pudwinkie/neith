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
      get { return !deleted && !mailbox.Flags.Has(ImapMailboxFlag.NonExistent); }
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
      get { return mailbox.Flags.Has(ImapMailboxFlag.NoInferiors) || string.IsNullOrEmpty(mailbox.HierarchyDelimiter); }
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

    internal static ImapStatusDataItem GetStatusDataItem(ImapCapabilityList serverCapabilities)
    {
      if (serverCapabilities.Has(ImapCapability.CondStore))
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

    public virtual ImapOpenedMailboxInfo Open(bool readOnly)
    {
      return client.OpenMailbox(this, readOnly);
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
        ProcessResult(client.Session.Delete(mailbox));

      if (unsubscribe)
        ProcessResult(client.Session.Unsubscribe(mailbox.Name)); // mailbox is detached already

      deleted = true;
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
      ProcessResult(client.Session.Create(mailbox.Name));

      deleted = false;

      // remove \NonExistent flag
      if (mailbox.Flags.Has(ImapMailboxFlag.NonExistent)) {
        var flags = new ImapMailboxFlagList(mailbox.Flags);

        flags.Remove(ImapMailboxFlag.NonExistent);

        mailbox.Flags = flags.AsReadOnly();
      }

      if (subscribe)
        ProcessResult(client.Session.Subscribe(mailbox.Name));
    }

    public static ImapMailboxInfo Create(ImapClient client, string mailboxName)
    {
      return Create(client, mailboxName, ImapClient.DefaultSubscription);
    }

    public static ImapMailboxInfo Create(ImapClient client, string mailboxName, bool subscribe)
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

    public void MoveTo(ImapMailboxInfo destinationMailbox, bool subscribe)
    {
      if (destinationMailbox == null)
        throw new ArgumentNullException("destinationMailbox");

      destinationMailbox.ThrowIfCanNotHaveChild();

      MoveTo(destinationMailbox.GetFullNameOf(mailbox.LeafName), subscribe);
    }

    public void MoveTo(string newMailboxName)
    {
      MoveTo(newMailboxName, ImapClient.DefaultSubscription);
    }

    public void MoveTo(string newMailboxName, bool subscribe)
    {
      if (newMailboxName == null)
        throw new ArgumentNullException("newMailboxName");
      else if (mailbox.NameEquals(newMailboxName))
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
        return null;

      return client.GetMailbox(mailbox.SuperiorName, options);
    }

    public ImapMailboxInfo GetOrCreateParent()
    {
      return GetOrCreateParent(ImapClient.DefaultSubscription, ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateParent(ImapMailboxListOptions options)
    {
      return GetOrCreateParent(ImapClient.DefaultSubscription, options);
    }

    public ImapMailboxInfo GetOrCreateParent(bool subscribe)
    {
      return GetOrCreateParent(subscribe, ImapMailboxListOptions.Default);
    }

    public ImapMailboxInfo GetOrCreateParent(bool subscribe, ImapMailboxListOptions options)
    {
      if (string.Empty.Equals(mailbox.SuperiorName))
        return null;

      return client.GetOrCreateMailbox(mailbox.SuperiorName, subscribe, options);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public string GetFullNameOf(string childName)
    {
      ThrowIfCanNotHaveChild();

      return mailbox.GetChildName(childName);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo CreateChild(string name)
    {
      return CreateChild(name, ImapClient.DefaultSubscription);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo CreateChild(string name, bool subscribe)
    {
      return client.CreateMailbox(GetFullNameOf(name), subscribe);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetChild(string name)
    {
      return GetChild(name, ImapMailboxListOptions.Default);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetChild(string name, ImapMailboxListOptions options)
    {
      return client.GetMailbox(GetFullNameOf(name), options);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name)
    {
      return GetOrCreateChild(name, ImapClient.DefaultSubscription, ImapMailboxListOptions.Default);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name, ImapMailboxListOptions options)
    {
      return GetOrCreateChild(name, ImapClient.DefaultSubscription, options);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name, bool subscribe)
    {
      return GetOrCreateChild(name, subscribe, ImapMailboxListOptions.Default);
    }

    /// <remarks>This method throws NotSupportedException if <see cref="CanHaveChild"/> is false.</remarks>
    public ImapMailboxInfo GetOrCreateChild(string name, bool subscribe, ImapMailboxListOptions options)
    {
      return client.GetMailboxNoException(GetFullNameOf(name), options) ?? CreateChild(name, subscribe);
    }

    /*
     * APPEND
     */
    public void AppendMessage(Stream stream)
    {
      AppendMessage(new ImapAppendMessage(stream));
    }

    public void AppendMessage(Stream stream, DateTimeOffset internalDate)
    {
      AppendMessage(new ImapAppendMessage(stream, internalDate));
    }

    public void AppendMessage(Stream stream, IImapMessageFlagSet flags)
    {
      AppendMessage(new ImapAppendMessage(stream, flags));
    }

    public void AppendMessage(Stream stream, DateTimeOffset internalDate, IImapMessageFlagSet flags)
    {
      AppendMessage(new ImapAppendMessage(stream, internalDate, flags));
    }

    public void AppendMessage(IImapAppendMessage message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      ProcessResult(Client.Session.Append(message, Mailbox));
    }

    public void AppendMessages(IEnumerable<IImapAppendMessage> messages)
    {
      if (messages == null)
        throw new ArgumentNullException("messages");

      if (Client.ServerCapabilities.Has(ImapCapability.MultiAppend)) {
        ProcessResult(Client.Session.AppendMultiple(messages, Mailbox));
      }
      else {
        foreach (var message in messages) {
          ProcessResult(Client.Session.Append(message, Mailbox));
        }
      }
    }

    public void WriteMessage(Action<Stream> write)
    {
      WriteMessageCore(null, null, null, write);
    }

    public void WriteMessage(long length, Action<Stream> write)
    {
      WriteMessageCore(length, null, null, write);
    }

    public void WriteMessage(DateTimeOffset internalDate, Action<Stream> write)
    {
      WriteMessageCore(null, internalDate, null, write);
    }

    public void WriteMessage(IImapMessageFlagSet flags, Action<Stream> write)
    {
      WriteMessageCore(null, null, flags, write);
    }

    public void WriteMessage(DateTimeOffset internalDate, IImapMessageFlagSet flags, Action<Stream> write)
    {
      WriteMessageCore(null, internalDate, flags, write);
    }

    public void WriteMessage(long length, DateTimeOffset internalDate, Action<Stream> write)
    {
      WriteMessageCore(length, internalDate, null, write);
    }

    public void WriteMessage(long length, IImapMessageFlagSet flags, Action<Stream> write)
    {
      WriteMessageCore(length, null, flags, write);
    }

    public void WriteMessage(long length, DateTimeOffset internalDate, IImapMessageFlagSet flags, Action<Stream> write)
    {
      WriteMessageCore(length, internalDate, flags, write);
    }

    private void WriteMessageCore(long? length, DateTimeOffset? internalDate, IImapMessageFlagSet flags, Action<Stream> write)
    {
      if (write == null)
        throw new ArgumentNullException("write");
      if (length.HasValue && length.Value < 0L)
        throw new ArgumentOutOfRangeException("length", length.Value, "must be zero or positive number");

      using (var stream = new ImapAppendMessageBodyStream()) {
        if (length.HasValue)
          stream.SetLength(length.Value);

        try {
          var asyncResult = client.Session.BeginAppend(stream, internalDate, flags, mailbox);

          if (!asyncResult.IsCompleted) {
            write(stream);

            stream.UpdateLength();
          }

          ProcessResult(client.Session.EndAppend(asyncResult));
        }
        finally {
          stream.InternalDispose();
        }
      }
    }

    /*
     * GETQUOTAROOT
     */
    public IEnumerable<ImapQuota> GetQuota()
    {
      if (!client.IsCapable(ImapCapability.Quota))
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
      return ImapClient.ThrowIfError(result, throwIfError);
    }

    private void ThrowIfCanNotHaveChild()
    {
      if (CanNotHaveChild)
        throw new NotSupportedException(string.Format("It is not possible for any child levels of hierarchy to exist under this mailbox. (mailbox name: '{0}')", FullName));
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
