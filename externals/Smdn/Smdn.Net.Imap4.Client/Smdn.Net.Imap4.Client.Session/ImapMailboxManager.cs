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

namespace Smdn.Net.Imap4.Client.Session {
  internal sealed class ImapMailboxManager {
    public ImapMailboxManager(ImapSession session)
    {
      this.session = session;
    }

    public ImapMailbox GetExistOrCreate(ImapMailboxList mailboxList)
    {
      var mailbox = GetExistOrCreate(mailboxList.Name);

      mailbox.Flags = mailboxList.NameAttributes;
      mailbox.HierarchyDelimiter = mailboxList.HierarchyDelimiter;

      return mailbox;
    }

    public ImapMailbox GetExistOrCreate(string name)
    {
      if (ImapMailbox.IsNameInbox(name))
        name = ImapMailbox.Inbox;

      ImapMailbox existMailbox;

      if (mailboxes.TryGetValue(name, out existMailbox))
        return existMailbox;
      else
        return Add(new ImapMailbox(name));
    }

    public ImapMailbox GetExist(string mailboxName)
    {
      ImapMailbox existMailbox;

      if (mailboxes.TryGetValue(mailboxName, out existMailbox))
        return existMailbox;
      else
        return null;
    }

    public ImapMailbox Add(ImapMailbox mailbox)
    {
      mailbox.AttachToSession(session);

      mailboxes[mailbox.Name] = mailbox;

      return mailbox;
    }

    public void Delete(string mailboxName)
    {
      ImapMailbox existMailbox;

      if (mailboxes.TryGetValue(mailboxName, out existMailbox)) {
        existMailbox.DetachFromSession();

        mailboxes.Remove(mailboxName);
      }
    }

    public ImapMailbox Rename(ImapMailbox existingMailbox, string newMailboxName)
    {
      mailboxes.Remove(existingMailbox.Name);
      existingMailbox.Name = newMailboxName;
      mailboxes[existingMailbox.Name] = existingMailbox;

      return existingMailbox;
    }

    public ImapMailbox Rename(string existingMailboxName, string newMailboxName)
    {
      var mailbox = GetExist(existingMailboxName);

      if (mailbox == null) {
        var newMailbox = new ImapMailbox(newMailboxName);

        Add(newMailbox);

        return newMailbox;
      }

      if (mailbox.IsInbox) {
        /*
         * 6.3.5. RENAME Command
         *       Renaming INBOX is permitted, and has special behavior.  It moves
         *       all messages in INBOX to a new mailbox with the given name,
         *       leaving INBOX empty.  If the server implementation supports
         *       inferior hierarchical names of INBOX, these are unaffected by a
         *       rename of INBOX.
         */
        Add(new ImapMailbox(ImapMailbox.Inbox));

        return Rename(mailbox, newMailboxName);
      }
      else {
        if (!string.IsNullOrEmpty(mailbox.HierarchyDelimiter)) {
          /*
           * 6.3.5. RENAME Command
           *       If the name has inferior hierarchical names, then the inferior
           *       hierarchical names MUST also be renamed.  For example, a rename of
           *       "foo" to "zap" will rename "foo/bar" (assuming "/" is the
           *       hierarchy delimiter character) to "zap/bar".
           */
          var existingNamePrefix = mailbox.Name + mailbox.HierarchyDelimiter;
          var newNamePrefix = newMailboxName + mailbox.HierarchyDelimiter;

          var inferiorNames = new List<string>();

          foreach (var mailboxName in mailboxes.Keys) {
            if (mailboxName.StartsWith(existingNamePrefix, StringComparison.Ordinal))
              inferiorNames.Add(mailboxName);
          }

          foreach (var inferiorName in inferiorNames) {
            var inferior = mailboxes[inferiorName];

            Rename(inferior, newNamePrefix + inferior.Name.Substring(existingNamePrefix.Length));
          }
        }

        return Rename(mailbox, newMailboxName);
      }
    }

    public void DetachFromSession()
    {
      foreach (var pair in mailboxes) {
        pair.Value.DetachFromSession();
      }

      mailboxes.Clear();
    }

    public bool ExistChildrenOf(ImapMailbox mailbox)
    {
      if (mailbox.Flags.Contains(ImapMailboxFlag.HasChildren))
        return true;

      if (string.IsNullOrEmpty(mailbox.HierarchyDelimiter))
        // mailbox has no hierarchy
        return false;

      var inferiorNamePrefix = mailbox.Name + mailbox.HierarchyDelimiter;

      foreach (var mailboxName in mailboxes.Keys) {
        if (mailboxName.StartsWith(inferiorNamePrefix, StringComparison.Ordinal))
          return true;
      }

      return false;
    }

    private /*readonly*/ ImapSession session;
    private /*readonly*/ Dictionary<string, ImapMailbox> mailboxes = new Dictionary<string, ImapMailbox>(StringComparer.Ordinal);
  }
}
