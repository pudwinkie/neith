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

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Client {
  public abstract class ImapMessageInfoBase {
    public ImapClient Client {
      get { return Mailbox.Client; }
    }

    public ImapOpenedMailboxInfo Mailbox {
      get; private set;
    }

    public long UidValidity {
      get; protected set;
    }

    protected ImapMessageInfoBase(ImapOpenedMailboxInfo mailbox)
    {
      if (mailbox == null)
        throw new ArgumentNullException("mailbox");

      this.Mailbox = mailbox;
      this.UidValidity = mailbox.UidValidity; // save current UIDVALIDITY
    }

    protected abstract ImapSequenceSet GetSequenceOrUidSet();

    protected virtual void PrepareOperation()
    {
      // do nothing
    }

    /*
     * operations
     */
    public void AddFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      Store(ImapStoreDataItem.AddFlags(flag, flags));
    }

    public void AddKeywords(string keyword, params string[] keywords)
    {
      Store(ImapStoreDataItem.AddFlags(keyword, keywords));
    }

    public void AddFlags(IImapMessageFlagSet flagsAndKeywords)
    {
      Store(ImapStoreDataItem.AddFlags(flagsAndKeywords));
    }

    public void RemoveFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      Store(ImapStoreDataItem.RemoveFlags(flag, flags));
    }

    public void RemoveKeywords(string keyword, params string[] keywords)
    {
      Store(ImapStoreDataItem.RemoveFlags(keyword, keywords));
    }

    public void RemoveFlags(IImapMessageFlagSet flagsAndKeywords)
    {
      Store(ImapStoreDataItem.RemoveFlags(flagsAndKeywords));
    }

    public void ReplaceFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      Store(ImapStoreDataItem.ReplaceFlags(flag, flags));
    }

    public void ReplaceKeywords(string keyword, params string[] keywords)
    {
      Store(ImapStoreDataItem.ReplaceFlags(keyword, keywords));
    }

    public void ReplaceFlags(IImapMessageFlagSet flagsAndKeywords)
    {
      Store(ImapStoreDataItem.ReplaceFlags(flagsAndKeywords));
    }

    public virtual void Store(ImapStoreDataItem storeDataItem)
    {
      if (storeDataItem == null)
        throw new ArgumentNullException("storeDataItem");

      StoreCore(storeDataItem);
    }

    private ImapSequenceSet StoreCore(ImapStoreDataItem storeDataItem)
    {
      var sequenceOrUidSet = GetSequenceOrUidSet();

      StoreCore(sequenceOrUidSet, storeDataItem);

      return sequenceOrUidSet;
    }

    private void StoreCore(ImapSequenceSet sequenceOrUidSet, ImapStoreDataItem storeDataItem)
    {
      if (sequenceOrUidSet.IsEmpty)
        return; // do nothing

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, sequenceOrUidSet);

      PrepareOperation();

      Mailbox.ProcessResult(Client.Session.Store(sequenceOrUidSet, storeDataItem));
    }

    private static ImapStoreDataItem markAsDeletedStoreDataItem = ImapStoreDataItem.AddFlags(ImapMessageFlag.Deleted);

    public void MarkAsDeleted()
    {
      StoreCore(markAsDeletedStoreDataItem);
    }

    private static ImapStoreDataItem markAsSeenStoreDataItem = ImapStoreDataItem.AddFlags(ImapMessageFlag.Seen);

    public void MarkAsSeen()
    {
      StoreCore(markAsSeenStoreDataItem);
    }

    public void Delete()
    {
      var sequenceOrUidSet = StoreCore(markAsDeletedStoreDataItem);

      if (sequenceOrUidSet.IsEmpty)
        return; // do nothing

      if (sequenceOrUidSet.IsUidSet && Client.ServerCapabilities.Has(ImapCapability.UidPlus))
        Mailbox.ProcessResult(Client.Session.UidExpunge(sequenceOrUidSet));
      else
        Mailbox.ProcessResult(Client.Session.Expunge());
    }

    public void CopyTo(ImapMailboxInfo destinationMailbox)
    {
      CopyToCore(destinationMailbox);
    }

    private ImapSequenceSet CopyToCore(ImapMailboxInfo destinationMailbox)
    {
      if (destinationMailbox == null)
        throw new ArgumentNullException("destinationMailbox");
      if (!destinationMailbox.Exists)
        throw new ImapProtocolViolationException("destination mailbox is not existent");

      var sequenceOrUidSet = GetSequenceOrUidSet();

      if (sequenceOrUidSet.IsEmpty)
        return sequenceOrUidSet; // do nothing

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, sequenceOrUidSet);

      PrepareOperation();

      Mailbox.ProcessResult(Client.Session.Copy(sequenceOrUidSet, destinationMailbox.Mailbox));

      return sequenceOrUidSet;
    }

    public void MoveTo(ImapMailboxInfo destinationMailbox)
    {
      StoreCore(CopyToCore(destinationMailbox), markAsDeletedStoreDataItem);
    }

    public void CopyTo(string destinationMailboxName)
    {
      CopyTo(destinationMailboxName, false);
    }

    /// <returns>The mailbox if created, otherwise null.</returns>
    public ImapMailboxInfo CopyTo(string destinationMailboxName, bool tryCreate)
    {
      ImapMailboxInfo createdMailboxInfo;

      CopyToCore(destinationMailboxName, tryCreate, out createdMailboxInfo);

      return createdMailboxInfo;
    }

    private ImapSequenceSet CopyToCore(string destinationMailboxName, bool tryCreate, out ImapMailboxInfo createdMailboxInfo)
    {
      var sequenceOrUidSet = GetSequenceOrUidSet();

      createdMailboxInfo = null;

      if (sequenceOrUidSet.IsEmpty)
        return sequenceOrUidSet; // do nothing

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, sequenceOrUidSet);

      PrepareOperation();

      ImapMailbox createdMailbox = null;

      if (tryCreate) {
        Mailbox.ProcessResult(Client.Session.Copy(sequenceOrUidSet,
                                                  ImapClient.GetValidatedMailboxName(destinationMailboxName),
                                                  out createdMailbox));
      }
      else {
        Mailbox.ProcessResult(Client.Session.Copy(sequenceOrUidSet,
                                                  ImapClient.GetValidatedMailboxName(destinationMailboxName)),
                              delegate(ImapResponseCode code) {
                                if (code == ImapResponseCode.TryCreate)
                                  throw new ImapMailboxNotFoundException(destinationMailboxName);
                                else
                                    return true;
                              });
      }

      if (createdMailbox != null) {
        // retrieve mailbox flags
        ImapClient.ThrowIfError(Client.Session.List(createdMailbox));

        createdMailboxInfo = new ImapMailboxInfo(Client, createdMailbox);
      }

      return sequenceOrUidSet;
    }

    public void MoveTo(string destinationMailboxName)
    {
      MoveTo(destinationMailboxName, false);
    }

    /// <returns>The mailbox if created, otherwise null.</returns>
    public ImapMailboxInfo MoveTo(string destinationMailboxName, bool tryCreate)
    {
      ImapMailboxInfo createdMailboxInfo;

      StoreCore(CopyToCore(destinationMailboxName, tryCreate, out createdMailboxInfo), markAsDeletedStoreDataItem);

      return createdMailboxInfo;
    }
  }
}
