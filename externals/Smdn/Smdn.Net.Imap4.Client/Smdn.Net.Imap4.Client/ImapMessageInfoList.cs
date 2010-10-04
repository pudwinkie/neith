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
using Smdn.Net.Imap4;

namespace Smdn.Net.Imap4.Client {
  public class ImapMessageInfoList : ImapMessageInfoBase, IEnumerable<ImapMessageInfo> {
    internal ImapMessageInfoList(ImapOpenedMailboxInfo mailbox,
                                 ImapMessageFetchAttributeOptions fetchOptions,
                                 ImapSequenceSet sequenceOrUidSet)
      : this(mailbox, fetchOptions, false)
    {
      this.sequenceOrUidSet = sequenceOrUidSet;
    }

    internal ImapMessageInfoList(ImapOpenedMailboxInfo mailbox,
                                 ImapMessageFetchAttributeOptions fetchOptions,
                                 IImapMessageQuery query)
      : this(mailbox, fetchOptions, false)
    {
      this.query = query;
    }

    internal ImapMessageInfoList(ImapOpenedMailboxInfo mailbox,
                                 ImapMessageFetchAttributeOptions fetchOptions,
                                 bool orderBySequenceNumber,
                                 IImapMessageQuery query)
      : this(mailbox, fetchOptions, orderBySequenceNumber)
    {
      this.query = query;
    }

    private ImapMessageInfoList(ImapOpenedMailboxInfo mailbox,
                                ImapMessageFetchAttributeOptions fetchOptions,
                                bool orderBySequenceNumber)
      : base(mailbox)
    {
      this.fetchOptions = fetchOptions;
      this.orderBySequenceNumber = orderBySequenceNumber;
    }

    protected override ImapSequenceSet GetSequenceOrUidSet()
    {
      Mailbox.CheckSelected();

      if (sequenceOrUidSet == null)
        return query.GetSequenceOrUidSet(Mailbox);
      else
        return sequenceOrUidSet;
    }

    private const int splitCount = 100;

    public IEnumerator<ImapMessageInfo> GetEnumerator()
    {
      var sequenceOrUidSet = GetSequenceOrUidSet();

      if (sequenceOrUidSet.IsEmpty)
        yield break;

      Mailbox.CheckUidValidity(UidValidity, sequenceOrUidSet);

      bool fetchStaticAttr, fetchDynamicAttr;
      var fetchDataItem = ImapMessageInfo.TranslateFetchOption(Mailbox,
                                                               fetchOptions,
                                                               out fetchStaticAttr,
                                                               out fetchDynamicAttr);

      foreach (var fetchSet in sequenceOrUidSet.SplitIntoEach(splitCount)) {
        ImapMessageAttribute[] messages;

        Mailbox.ProcessResult(Client.Session.Fetch(fetchSet,
                                                   fetchDataItem,
                                                   out messages));

        if (orderBySequenceNumber) {
          foreach (var sequence in fetchSet) {
            var message = messages.FirstOrDefault(delegate(ImapMessageAttribute attr) {
              return attr.Sequence == sequence;
            });

            yield return Mailbox.ToMessageInfo(message, fetchStaticAttr, fetchDynamicAttr);
          }
        }
        else {
          foreach (var message in messages) {
            yield return Mailbox.ToMessageInfo(message, fetchStaticAttr, fetchDynamicAttr);
          }
        }
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public override string ToString()
    {
      return string.Format("{{ImapMessageInfoList: Mailbox='{0}', UidValidity={1}}}",
                           Mailbox.FullName,
                           UidValidity);
    }

    private ImapSequenceSet sequenceOrUidSet;
    private IImapMessageQuery query;
    private readonly ImapMessageFetchAttributeOptions fetchOptions;
    private readonly bool orderBySequenceNumber;
  }
}
