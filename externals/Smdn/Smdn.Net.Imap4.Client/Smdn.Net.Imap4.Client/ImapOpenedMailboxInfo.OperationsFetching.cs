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
  partial class ImapOpenedMailboxInfo {
    /*
     * GetMessages by FETCHing
     */
    public ImapMessageInfoList GetMessages()
    {
      return GetMessages(ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetMessages(ImapMessageFetchAttributeOptions options)
    {
      return new ImapMessageInfoList(this, options, new AllMessageQuery());
    }

    private class AllMessageQuery : IImapMessageQuery {
      public ImapSequenceSet GetSequenceOrUidSet(ImapOpenedMailboxInfo mailbox)
      {
        mailbox.Refresh();

        if (mailbox.ExistMessageCount <= 0L)
          return ImapSequenceSet.CreateSet(new long[0]);
        else
          return ImapSequenceSet.CreateRangeSet(1L, mailbox.ExistMessageCount);
      }
    }

    /*
     * GetMessage by FETCHing with uid or sequence
     */
    public ImapMessageInfo GetMessageByUid(long uid)
    {
      return GetMessageByUid(uid,
                             ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfo GetMessageByUid(long uid,
                                           ImapMessageFetchAttributeOptions options)
    {
      if (uid <= 0L)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("uid", uid);

      /*
       * 6.4.8. UID Command
       *       A non-existent unique identifier is ignored without any error
       *       message generated.  Thus, it is possible for a UID FETCH command
       *       to return an OK without any data or a UID COPY or UID STORE to
       *       return an OK without performing any operations.
       */
      return GetMessage(ImapSequenceSet.CreateUidSet(uid),
                        options);
    }

    public ImapMessageInfo GetMessageBySequence(long sequence)
    {
      return GetMessageBySequence(sequence,
                                  ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfo GetMessageBySequence(long sequence,
                                                ImapMessageFetchAttributeOptions options)
    {
      if (sequence <= 0L)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("sequence", sequence);

      // Refresh();

      if (ExistMessageCount < sequence)
        throw new ArgumentOutOfRangeException("sequence",
                                              sequence,
                                              string.Format("specified sequence number is greater than exist message count. (exist message count = {0})",
                                                             ExistMessageCount));

      return GetMessage(ImapSequenceSet.CreateSet(sequence),
                        options);
    }

    private ImapMessageInfo GetMessage(ImapSequenceSet sequenceOrUidSet,
                                       ImapMessageFetchAttributeOptions options)
    {
      var message = (new ImapMessageInfoList(this, options, sequenceOrUidSet)).FirstOrDefault();

      if (message == null)
        throw new ImapMessageNotFoundException(sequenceOrUidSet);

      return message;
    }

    public ImapMessageInfoList GetMessages(long uid, params long[] uids)
    {
      return GetMessages(ImapMessageFetchAttributeOptions.Default,
                         uid,
                         uids);
    }

    public ImapMessageInfoList GetMessages(ImapMessageFetchAttributeOptions options,
                                           long uid,
                                           params long[] uids)
    {
      return new ImapMessageInfoList(this,
                                     options,
                                     ImapSequenceSet.CreateUidSet(uid, uids));
    }

    /*
     * GetMessages by SEARCHing
     */
    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria)
    {
      return GetMessages(searchCriteria,
                         null,
                         ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria,
                                           ImapMessageFetchAttributeOptions options)
    {
      return GetMessages(searchCriteria,
                         null,
                         options);
    }

    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria,
                                           Encoding encoding)
    {
      return GetMessages(searchCriteria,
                         encoding,
                         ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria,
                                           Encoding encoding,
                                           ImapMessageFetchAttributeOptions options)
    {
      if (searchCriteria == null)
        throw new ArgumentNullException("searchCriteria");

      return new ImapMessageInfoList(this,
                                     options,
                                     new SearchMessageQuery(searchCriteria, encoding));
    }

    private class SearchMessageQuery : IImapMessageQuery {
      public SearchMessageQuery(ImapSearchCriteria searchCriteria, Encoding encoding)
      {
        this.searchCriteria = searchCriteria;
        this.encoding = encoding;
      }

      public ImapSequenceSet GetSequenceOrUidSet(ImapOpenedMailboxInfo mailbox)
      {
        ImapMatchedSequenceSet matchedSequenceNumbers = null;

        if (mailbox.Client.ServerCapabilities.Contains(ImapCapability.Searchres)) {
          mailbox.ProcessResult(mailbox.Client.Session.ESearch(searchCriteria,
                                                               encoding,
                                                               ImapSearchResultOptions.Save,
                                                               out matchedSequenceNumbers),
                                delegate(ImapResponseCode code) {
            if (code == ImapResponseCode.NotSaved)
              return false; // throw no exception
            else
              return true;
          });
        }

        if (matchedSequenceNumbers == null)
          mailbox.ProcessResult(mailbox.Client.Session.Search(searchCriteria,
                                                              encoding,
                                                              out matchedSequenceNumbers));

        return matchedSequenceNumbers;
      }

      private ImapSearchCriteria searchCriteria;
      private Encoding encoding;
    }

    /*
     * GetMessages by SORTing
     */
    public ImapMessageInfoList GetSortedMessages(ImapSortCriteria sortOrder,
                                                 ImapSearchCriteria searchCriteria)
    {
      return GetSortedMessages(sortOrder,
                               searchCriteria,
                               null,
                               ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetSortedMessages(ImapSortCriteria sortOrder,
                                                 ImapSearchCriteria searchCriteria,
                                                 ImapMessageFetchAttributeOptions options)
    {
      return GetSortedMessages(sortOrder,
                               searchCriteria,
                               null,
                               options);
    }

    public ImapMessageInfoList GetSortedMessages(ImapSortCriteria sortOrder,
                                                 ImapSearchCriteria searchCriteria,
                                                 Encoding encoding)
    {
      return GetSortedMessages(sortOrder,
                               searchCriteria,
                               encoding,
                               ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetSortedMessages(ImapSortCriteria sortOrder,
                                                 ImapSearchCriteria searchCriteria,
                                                 Encoding encoding,
                                                 ImapMessageFetchAttributeOptions options)
    {
      if (sortOrder == null)
        throw new ArgumentNullException("sortOrder");
      if (searchCriteria == null)
        throw new ArgumentNullException("searchCriteria");

      Client.ThrowIfIncapable(ImapCapability.Sort);

      return new ImapMessageInfoList(this, options, new SortMessageQuery(sortOrder, searchCriteria, encoding));
    }

    private class SortMessageQuery : IImapMessageQuery {
      public SortMessageQuery(ImapSortCriteria sortOrder, ImapSearchCriteria searchCriteria, Encoding encoding)
      {
        this.sortOrder = sortOrder;
        this.searchCriteria = searchCriteria;
        this.encoding = encoding;
      }

      public ImapSequenceSet GetSequenceOrUidSet(ImapOpenedMailboxInfo mailbox)
      {
        ImapMatchedSequenceSet matchedSequenceNumbers;

        mailbox.ProcessResult(mailbox.Client.Session.Sort(sortOrder,
                                                          searchCriteria,
                                                          encoding,
                                                          out matchedSequenceNumbers));

        return matchedSequenceNumbers;
      }

      private ImapSortCriteria sortOrder;
      private ImapSearchCriteria searchCriteria;
      private Encoding encoding;
    }
  }
}

