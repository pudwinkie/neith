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
using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.Client {
  partial class ImapMessageInfo {
    public void Refresh()
    {
      EnsureDynamicAttributesFetched(true);
    }

    private bool FindBySequence(ImapMessageAttributeBase attribute)
    {
      return attribute.Sequence == Sequence;
    }

    internal static readonly ImapFetchDataItem FetchStaticDataItem =
      ImapFetchDataItem.BodyStructure +
      ImapFetchDataItem.Envelope + 
      ImapFetchDataItem.InternalDate +
      ImapFetchDataItem.Rfc822Size;

    private void EnsureStaticAttributesFetched()
    {
      if (staticAttribute != null)
        return;

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, SequenceOrUidSet);

      PrepareOperation();

      ImapMessageStaticAttribute[] staticAttrs;

      Mailbox.ProcessResult(Client.Session.Fetch(SequenceOrUidSet,
                                                 FetchStaticDataItem,
                                                 out staticAttrs));

      var staticAttr = staticAttrs.FirstOrDefault(FindBySequence);

      if (staticAttr == null) {
        Sequence = ExpungedMessageSequenceNumber;
        throw new ImapMessageDeletedException(this);
      }
      else {
        staticAttribute = staticAttr;
      }
    }

    internal static ImapFetchDataItem GetFetchDynamicDataItem(ImapOpenedMailboxInfo mailbox)
    {
      if (mailbox.IsModSequencesAvailable)
        return ImapFetchDataItem.Flags + ImapFetchDataItem.ModSeq;
      else
        return ImapFetchDataItem.Flags;
    }

    internal static ImapFetchDataItem TranslateFetchOption(ImapOpenedMailboxInfo mailbox,
                                                           ImapMessageFetchAttributeOptions options,
                                                           out bool fetchStaticAttr,
                                                           out bool fetchDynamicAttr)
    {
      fetchStaticAttr = (int)(options & ImapMessageFetchAttributeOptions.StaticAttributes) != 0;
      fetchDynamicAttr = (int)(options & ImapMessageFetchAttributeOptions.DynamicAttributes) != 0;

      var fetchDataItem = ImapFetchDataItem.Uid;

      if (fetchStaticAttr)
        fetchDataItem += ImapMessageInfo.FetchStaticDataItem;
      if (fetchDynamicAttr)
        fetchDataItem += ImapMessageInfo.GetFetchDynamicDataItem(mailbox);

      return fetchDataItem;
    }

    private void EnsureDynamicAttributesFetched(bool refresh)
    {
      if (!refresh && dynamicAttribute != null)
        return;

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, SequenceOrUidSet);

      PrepareOperation();

      ImapMessageDynamicAttribute[] dynamicAttrs;

      Mailbox.ProcessResult(Client.Session.Fetch(SequenceOrUidSet,
                                                 GetFetchDynamicDataItem(Mailbox),
                                                 out dynamicAttrs));

      var dynamicAttr = dynamicAttrs.FirstOrDefault(FindBySequence);

      if (dynamicAttr == null) {
        Sequence = ExpungedMessageSequenceNumber;
        throw new ImapMessageDeletedException(this);
      }
      else {
        dynamicAttribute = dynamicAttr;
      }
    }

    public void ToggleFlags(ImapMessageFlag flag,
                            params ImapMessageFlag[] flags)
    {
      ToggleFlags(new ImapMessageFlagSet(flags.Prepend(flag)));
    }

    public void ToggleKeywords(string keyword,
                               params string[] keywords)
    {
      ToggleFlags(new ImapMessageFlagSet(keywords.Prepend(keyword)));
    }

    public void ToggleFlags(IImapMessageFlagSet flagsAndKeywords)
    {
      if (flagsAndKeywords == null)
        throw new ArgumentNullException("flagsAndKeywords");

      var addFlags = new ImapMessageFlagSet();
      var removeFlags = new ImapMessageFlagSet();

      EnsureDynamicAttributesFetched(false);

      foreach (var flag in flagsAndKeywords) {
        if (dynamicAttribute.Flags.Contains(flag))
          removeFlags.Add(flag);
        else
          addFlags.Add(flag);
      }

      if (0 < removeFlags.Count)
        Store(ImapStoreDataItem.RemoveFlags(removeFlags));

      if (0 < addFlags.Count)
        Store(ImapStoreDataItem.AddFlags(addFlags));
    }
  }
}
