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
using System.Text;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  public partial class ImapOpenedMailboxInfo : ImapMailboxInfo, IDisposable {
    public bool IsReadOnly {
      get { return Mailbox.ReadOnly; }
    }

    public long FirstUnseenMessageNumber {
      get { return Mailbox.FirstUnseen; }
    }

    public IImapMessageFlagSet ApplicableFlags {
      get { return Mailbox.ApplicableFlags; }
    }

    public IImapMessageFlagSet PermanentFlags {
      get { return Mailbox.PermanentFlags; }
    }

    public bool IsAllowedToCreateKeywords {
      get { return Mailbox.PermanentFlags.Contains(ImapMessageFlag.AllowedCreateKeywords); }
    }

    public bool IsUidPersistent {
      get { return Mailbox.UidPersistent; }
    }

    private ImapMessageInfoCollection messages = new ImapMessageInfoCollection();

    internal ImapOpenedMailboxInfo(ImapClient client, ImapMailbox mailbox)
      : base(client, mailbox)
    {
    }

    void IDisposable.Dispose()
    {
      if (Client.IsConnected)
        Close();
    }

    public override ImapOpenedMailboxInfo Open(bool asReadOnly)
    {
      if (IsOpen)
        return this;
      else
        // messages.Clear();
        throw new NotImplementedException();
    }

    public void Close()
    {
      Client.CloseMailbox();
    }

    internal void CheckSelected()
    {
      if (!Client.IsSelected(this))
        throw new ImapMailboxClosedException(FullName);
    }

    internal void CheckUidValidity(long uidValidity, ImapSequenceSet sequenceOrUidSet)
    {
      // TODO: impl
#if false
      if (!sequenceOrUidSet.IsUidSet)
        return;
      if (uidValidity != UidValidity)
        throw new ImapException("UIDVALIDITY value has been changed");
#endif
    }

    internal ImapMessageInfo ToMessageInfo(ImapMessageAttribute message, bool hasStaticAttr, bool hasDynamicAttr)
    {
      ImapMessageInfo info;

      if (messages.Contains(message.Uid)) {
        info = messages[message.Uid];
        info.Sequence = message.Sequence;
      }
      else {
        info = new ImapMessageInfo(this, message.Uid, message.Sequence);

        messages.Add(info);
      }

      if (hasStaticAttr)
        info.StaticAttribute = message.GetStaticAttributeImpl();
      if (hasDynamicAttr)
        info.DynamicAttribute = message.GetDynamicAttributeImpl();

      return info;
    }

    internal protected override ImapCommandResult ProcessResult(ImapCommandResult result,
                                                                Func<ImapResponseCode, bool> throwIfError)
    {
      ProcessSizeAndStatusResponse(result.ReceivedResponses);

      return base.ProcessResult(result, throwIfError);
    }

    private void ProcessSizeAndStatusResponse(IEnumerable<ImapResponse> receivedResponses)
    {
      var prevExistMessageCount = Mailbox.ExistsMessage;
      var prevRecentMessageCount = Mailbox.RecentMessage;
      var deletedMessages = new List<ImapMessageInfo>();
      var statusChangedMessages = new List<ImapMessageInfo>();

      foreach (var response in receivedResponses) {
        var data = response as ImapDataResponse;

        if (data == null)
          continue;

        // XXX: equality
        if (data.Type == ImapDataResponseType.Fetch) {
          var message = messages.Find(ImapDataResponseConverter.FromFetch(messages, data));

          if (message != null)
            statusChangedMessages.Add(message);
        }
        else if (data.Type == ImapDataResponseType.Expunge) {
          var expunged = ImapDataResponseConverter.FromExpunge(data);

          foreach (var message in messages) {
            if (message.Sequence == expunged) {
              message.Sequence = ImapMessageInfo.ExpungedMessageSequenceNumber;

              deletedMessages.Add(message);
            }
            else if (expunged < message.Sequence) {
              message.Sequence--;
            }
          }

          if (0L < Mailbox.ExistsMessage)
            Mailbox.ExistsMessage -= 1L;
        }
        else if (data.Type == ImapDataResponseType.Exists) {
          Mailbox.ExistsMessage = ImapDataResponseConverter.FromExists(data);
        }
        else if (data.Type == ImapDataResponseType.Recent) {
          Mailbox.RecentMessage = ImapDataResponseConverter.FromRecent(data);
        }
        else if (data.Type == ImapDataResponseType.Flags) {
          Mailbox.ApplicableFlags = ImapDataResponseConverter.FromFlags(data);
        }
      }

      if (prevExistMessageCount != Mailbox.ExistsMessage)
        Client.RaiseExistMessageCountChanged(this, prevExistMessageCount);

      if (prevRecentMessageCount != Mailbox.RecentMessage)
        Client.RaiseRecentMessageCountChanged(this, prevRecentMessageCount);

      if (0 < statusChangedMessages.Count)
        Client.RaiseMessageStatusChanged(statusChangedMessages.ToArray());

      if (0 < deletedMessages.Count)
        Client.RaiseMessageDeleted(deletedMessages.ToArray());
    }

    /*
     * operations
     */
    public override void Refresh()
    {
      CheckSelected();

      ProcessResult(Client.Session.NoOp());
    }

    public void Expunge()
    {
      CheckSelected();

      ProcessResult(Client.Session.Expunge());
    }

    /*
     * MoveMessagesTo
     */
    public void MoveMessagesTo(ImapMailboxInfo destinationMailbox)
    {
      MoveOrCopyMessagesToCore(false,
                               GetMessages(ImapMessageFetchAttributeOptions.None),
                               destinationMailbox);
    }

    public void MoveMessagesTo(ImapMailboxInfo destinationMailbox,
                               long uid,
                               params long[] uids)
    {
      MoveOrCopyMessagesToCore(false,
                               GetMessages(ImapMessageFetchAttributeOptions.None,
                                           uid,
                                           uids),
                               destinationMailbox);
    }

    public void MoveMessagesTo(ImapSearchCriteria searchCriteria,
                               ImapMailboxInfo destinationMailbox)
    {
      MoveOrCopyMessagesToCore(false,
                               GetMessages(searchCriteria,
                                           null,
                                           ImapMessageFetchAttributeOptions.None),
                               destinationMailbox);
    }

    public void MoveMessagesTo(ImapSearchCriteria searchCriteria,
                               Encoding encoding,
                               ImapMailboxInfo destinationMailbox)
    {
      MoveOrCopyMessagesToCore(false,
                               GetMessages(searchCriteria,
                                           encoding,
                                           ImapMessageFetchAttributeOptions.None),
                               destinationMailbox);
    }

    /*
     * CopyMessagesTo
     */
    public void CopyMessagesTo(ImapMailboxInfo destinationMailbox)
    {
      MoveOrCopyMessagesToCore(true,
                               GetMessages(ImapMessageFetchAttributeOptions.None),
                               destinationMailbox);
    }

    public void CopyMessagesTo(ImapMailboxInfo destinationMailbox,
                               long uid,
                               params long[] uids)
    {
      MoveOrCopyMessagesToCore(true,
                               GetMessages(ImapMessageFetchAttributeOptions.None,
                                           uid,
                                           uids),
                               destinationMailbox);
    }

    public void CopyMessagesTo(ImapSearchCriteria searchCriteria,
                               ImapMailboxInfo destinationMailbox)
    {
      MoveOrCopyMessagesToCore(true,
                               GetMessages(searchCriteria,
                                           null,
                                           ImapMessageFetchAttributeOptions.None),
                               destinationMailbox);
    }

    public void CopyMessagesTo(ImapSearchCriteria searchCriteria,
                               Encoding encoding,
                               ImapMailboxInfo destinationMailbox)
    {
      MoveOrCopyMessagesToCore(true,
                               GetMessages(searchCriteria,
                                           encoding,
                                           ImapMessageFetchAttributeOptions.None),
                               destinationMailbox);
    }

    private void MoveOrCopyMessagesToCore(bool copy,
                                          ImapMessageInfoList messages,
                                          ImapMailboxInfo destinationMailbox)
    {
      if (messages == null)
        throw new ArgumentNullException("messages");
      if (destinationMailbox == null)
        throw new ArgumentNullException("destinationMailbox");

      if (copy)
        messages.CopyTo(destinationMailbox);
      else
        messages.MoveTo(destinationMailbox);
    }
  }
}
