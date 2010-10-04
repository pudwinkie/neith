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

using Smdn.Formats;
using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4 {
  public sealed class ImapMailbox : IImapUrl {
    internal const string Inbox = "INBOX";

    internal ImapSession Session {
      get; private set;
    }

    public Uri Url {
      get
      {
        if (uriBuilder == null)
          throw new NotSupportedException("The base URL is not specified.");

        return uriBuilder.Uri;
      }
    }

    internal ImapUriBuilder UrlBuilder {
      get { return uriBuilder; }
    }

    /// <value>decoded mailbox name</value>
    public string Name {
      get { return name; }
      internal set
      {
        name = value;

        if (uriBuilder != null)
          uriBuilder.Mailbox = value;
      }
    }

    public string LeafName {
      get
      {
        if (string.IsNullOrEmpty(hierarchyDelimiter))
          return name;

        var index = name.LastIndexOf(hierarchyDelimiter, StringComparison.Ordinal);

        if (index < 0)
          return name;
        else
          return name.Substring(index + 1);
      }
    }

    public string SuperiorName {
      get
      {
        if (string.IsNullOrEmpty(hierarchyDelimiter))
          return string.Empty;

        var index = name.LastIndexOf(hierarchyDelimiter, StringComparison.Ordinal);

        if (index < 0)
          return string.Empty;
        else
          return name.Substring(0, index);
      }
    }

    public bool IsInbox {
      get { return IsNameInbox(name); }
    }

    public string HierarchyDelimiter {
      get { return hierarchyDelimiter; }
      internal set { hierarchyDelimiter = value; }
    }

    public IImapMailboxFlagSet Flags {
      get { return flags; }
      internal set { flags = value ?? ImapMailboxFlagList.CreateReadOnlyEmpty(); }
    }

    /*
     * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
     * http://tools.ietf.org/html/rfc5258
     * 3.4. Additional Requirements on LIST-EXTENDED Clients
     *    All clients that support this extension MUST treat an attribute with
     *    a stronger meaning as implying any attribute that can be inferred
     *    from it.  For example, the client must treat the presence of the
     *    \NoInferiors attribute as if the \HasNoChildren attribute was also
     *    sent by the server.
     */
    /// <remarks>returns true if the mailbox is flagged as \NoSelect or \NonExistent.</remarks>
    public bool IsUnselectable {
      get { return flags.Has(ImapMailboxFlag.NoSelect) || flags.Has(ImapMailboxFlag.NonExistent); }
    }

    /// <remarks>returns true if the mailbox is flagged as \NoInferiors or \HasNoChildren.</remarks>
    public bool IsNonHierarchical {
      get { return flags.Has(ImapMailboxFlag.NoInferiors) || flags.Has(ImapMailboxFlag.HasNoChildren); }
    }

    public bool ReadOnly {
      get { return readOnly; }
      internal set { readOnly = value; }
    }

    public IImapMessageFlagSet ApplicableFlags {
      get { return applicableFlags; }
      internal set { applicableFlags = value ?? ImapMessageFlagList.CreateReadOnlyEmpty(); }
    }

    public IImapMessageFlagSet PermanentFlags {
      get { return permanentFlags; }
      internal set { permanentFlags = value ?? ImapMessageFlagList.CreateReadOnlyEmpty(); }
    }

    public long ExistsMessage {
      get { return existsMessage; }
      internal set { existsMessage = value; }
    }

    public long RecentMessage {
      get { return recentMessage; }
      internal set { recentMessage = value; }
    }

    public long UnseenMessage {
      get { return unseenMessage; }
      internal set { unseenMessage = value; }
    }

    public long FirstUnseen {
      get { return firstUnseen; }
      internal set { firstUnseen = value; }
    }

    public long UidNext {
      get { return uidNext; }
      internal set { uidNext = value; }
    }

    public long UidValidity {
      get { return uidValidity; }
      internal set
      {
        uidValidity = value;

        if (uriBuilder != null)
          uriBuilder.UidValidity = value;
      }
    }

    /*
     * RFC 4315 Internet Message Access Protocol (IMAP) - UIDPLUS extension
     * http://tools.ietf.org/html/rfc4315
     */
    public bool UidPersistent {
      get { return uidPersistent; }
      internal set { uidPersistent = value; }
    }

    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     */
    public bool ModificationSequences {
      get { return modificationSequences; }
      internal set { modificationSequences = value; }
    }

    [CLSCompliant(false)]
    public ulong HighestModSeq {
      get { return highestModSeq; }
      internal set { highestModSeq = value; }
    }

    internal static ImapMailbox CreateFrom(ImapMailboxList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      var mailbox = new ImapMailbox(list.Name);

      mailbox.hierarchyDelimiter = list.HierarchyDelimiter;
      mailbox.flags = list.NameAttributes;

      return mailbox;
    }

    internal static bool IsNameInbox(string name)
    {
      return string.Equals(Inbox, name, StringComparison.OrdinalIgnoreCase);
    }

    public static bool NameEquals(string mailboxNameX, string mailboxNameY)
    {
      var isInboxX = IsNameInbox(mailboxNameX);
      var isInboxY = IsNameInbox(mailboxNameY);

      if (isInboxX && isInboxY)
        return true;
      else if (isInboxX || isInboxY)
        return false;
      else
        return string.Equals(mailboxNameX, mailboxNameY, StringComparison.Ordinal);
    }

    public bool NameEquals(ImapMailbox other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      return NameEquals(other.name);
    }

    public bool NameEquals(string otherMailboxName)
    {
      return NameEquals(name, otherMailboxName);
    }

    internal ImapMailbox()
    {
      this.Name = null;
      this.Session = null;
    }

    /// <remarks>name must be decoded</remarks>
    internal ImapMailbox(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      this.Name = name;
      this.Session = null;
    }

    internal void AttachToSession(ImapSession session)
    {
      Session = session;

      (this as IImapUrl).SetBaseUrl(session.AuthorityBuilder);
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    void IImapUrl.SetBaseUrl(ImapUriBuilder baseUrl)
    {
      uriBuilder = baseUrl.Clone();
      uriBuilder.Mailbox = name;
      uriBuilder.UidValidity = uidValidity;
    }

    internal void DetachFromSession()
    {
      Session = null;
    }

    internal void UpdateStatus(ImapStatusAttributeList status)
    {
      if (status.Messages.HasValue)      existsMessage  = (long)status.Messages;
      if (status.Recent.HasValue)        recentMessage  = (long)status.Recent;
      if (status.Unseen.HasValue)        unseenMessage  = (long)status.Unseen;
      if (status.UidNext.HasValue)       uidNext        = (long)status.UidNext;
      if (status.UidValidity.HasValue)   UidValidity    = (long)status.UidValidity;
      if (status.HighestModSeq.HasValue) highestModSeq  = (ulong)status.HighestModSeq;
    }

    /// <remarks>This method throws InvalidOperationException if HierarchyDelimiter is null or empty.</remarks>
    public string GetChildName(string childName)
    {
      if (string.IsNullOrEmpty(hierarchyDelimiter))
        throw new InvalidOperationException("mailbox has no hierarchy");

      return name + hierarchyDelimiter + childName;
    }

    public override string ToString()
    {
      return string.Format("{{Name={0}, HierarchyDelimiter={1}, Flags={2}, ReadOnly={3}, ApplicableFlags={4}, PermanentFlags={5}, ExistsMessage={6}, RecentMessage={7}, UnseenMessage={8}, FirstUnseen={9}, UidNext={10}, UidValidity={11}, UidPersistent={12}, ModificationSequences={13}, HighestModSeq={14}}}",
                           name,
                           hierarchyDelimiter,
                           flags,
                           readOnly,
                           applicableFlags,
                           permanentFlags,
                           existsMessage,
                           recentMessage,
                           unseenMessage,
                           firstUnseen,
                           uidNext,
                           uidValidity,
                           uidPersistent,
                           modificationSequences,
                           highestModSeq);
    }

    private string name;
    private string hierarchyDelimiter = null;
    private bool readOnly = false;
    private IImapMailboxFlagSet flags = ImapMailboxFlagList.CreateReadOnlyEmpty();
    private IImapMessageFlagSet applicableFlags = ImapMessageFlagList.CreateReadOnlyEmpty();
    private IImapMessageFlagSet permanentFlags = ImapMessageFlagList.CreateReadOnlyEmpty();
    private long existsMessage = 0;
    private long recentMessage = 0;
    private long unseenMessage = 0;
    private long firstUnseen = 0;
    private long uidNext = 0;
    private long uidValidity = 0;
    private bool uidPersistent = true;
    private bool modificationSequences = false;
    private ulong highestModSeq = 0;
    private ImapUriBuilder uriBuilder = null;
  }
}