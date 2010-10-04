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
using System.Threading;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  public class ImapOpenedMailboxInfo : ImapMailboxInfo, IDisposable {
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
      get { return Mailbox.PermanentFlags.Has(ImapMessageFlag.AllowedCreateKeywords); }
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
      this.Close();
    }

    public override ImapOpenedMailboxInfo Open(bool readOnly)
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
      Client.ThrowIfNotSelected(this);
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
      var prevExistMessageCount = Mailbox.ExistsMessage;
      var prevRecentMessageCount = Mailbox.RecentMessage;
      var deletedMessages = new List<ImapMessageInfo>();
      var statusChangedMessages = new List<ImapMessageInfo>();

      foreach (var response in result.ReceivedResponses) {
        var data = response as ImapDataResponse;

        if (data == null)
          continue;

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

      var ret = base.ProcessResult(result, throwIfError);

      if (prevExistMessageCount != Mailbox.ExistsMessage)
        Client.RaiseExistMessageCountChanged(this, prevExistMessageCount);

      if (prevRecentMessageCount != Mailbox.RecentMessage)
        Client.RaiseRecentMessageCountChanged(this, prevRecentMessageCount);

      if (0 < statusChangedMessages.Count)
        Client.RaiseMessageStatusChanged(statusChangedMessages.ToArray());

      if (0 < deletedMessages.Count)
        Client.RaiseMessageDeleted(deletedMessages.ToArray());

      return ret;
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
      return GetMessageByUid(uid, ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfo GetMessageByUid(long uid, ImapMessageFetchAttributeOptions options)
    {
      if (uid <= 0L)
        throw new ArgumentOutOfRangeException("uid", uid, "uid must be non-zero positive number");

      /*
       * 6.4.8. UID Command
       *       A non-existent unique identifier is ignored without any error
       *       message generated.  Thus, it is possible for a UID FETCH command
       *       to return an OK without any data or a UID COPY or UID STORE to
       *       return an OK without performing any operations.
       */
      return GetMessage(ImapSequenceSet.CreateUidSet(uid), options);
    }

    public ImapMessageInfo GetMessageBySequence(long sequence)
    {
      return GetMessageBySequence(sequence, ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfo GetMessageBySequence(long sequence, ImapMessageFetchAttributeOptions options)
    {
      if (sequence <= 0L)
        throw new ArgumentOutOfRangeException("sequence", sequence, "sequence must be non-zero positive number");

      // Refresh();

      if (ExistMessageCount < sequence)
        throw new ArgumentOutOfRangeException("sequence",
                                              sequence,
                                              string.Format("specified sequence number is greater than exist message count. (exist message count = {0})",
                                                             ExistMessageCount));

      return GetMessage(ImapSequenceSet.CreateSet(sequence), options);
    }

    private ImapMessageInfo GetMessage(ImapSequenceSet sequenceOrUidSet, ImapMessageFetchAttributeOptions options)
    {
      var message = (new ImapMessageInfoList(this, options, sequenceOrUidSet)).FirstOrDefault();

      if (message == null)
        throw new ImapMessageNotFoundException(sequenceOrUidSet);

      return message;
    }

    public ImapMessageInfoList GetMessages(long uid, params long[] uids)
    {
      return GetMessages(ImapMessageFetchAttributeOptions.Default, uid, uids);
    }

    public ImapMessageInfoList GetMessages(ImapMessageFetchAttributeOptions options, long uid, params long[] uids)
    {
      return new ImapMessageInfoList(this, options, ImapSequenceSet.CreateUidSet(uid, uids));
    }

    /*
     * GetMessages by SEARCHing
     */
    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria)
    {
      return GetMessages(searchCriteria, null, ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria,
                                           ImapMessageFetchAttributeOptions options)
    {
      return GetMessages(searchCriteria, null, options);
    }

    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria,
                                           Encoding encoding)
    {
      return GetMessages(searchCriteria, encoding, ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetMessages(ImapSearchCriteria searchCriteria,
                                           Encoding encoding,
                                           ImapMessageFetchAttributeOptions options)
    {
      if (searchCriteria == null)
        throw new ArgumentNullException("searchCriteria");

      return new ImapMessageInfoList(this, options, new SearchMessageQuery(searchCriteria, encoding));
    }

    private class SearchMessageQuery : IImapMessageQuery {
      public SearchMessageQuery(ImapSearchCriteria searchCriteria, Encoding encoding)
      {
        this.searchCriteria = searchCriteria;
        this.encoding = encoding;
      }

      public ImapSequenceSet GetSequenceOrUidSet(ImapOpenedMailboxInfo mailbox)
      {
        ImapMatchedSequenceSet matchedSequenceNumbers;

        if (mailbox.Client.IsCapable(ImapCapability.Searchres))
          mailbox.ProcessResult(mailbox.Client.Session.ESearch(searchCriteria,
                                                               encoding,
                                                               ImapSearchResultOptions.Save,
                                                               out matchedSequenceNumbers));
        else
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
      return GetSortedMessages(sortOrder, searchCriteria, null, ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList GetSortedMessages(ImapSortCriteria sortOrder,
                                                 ImapSearchCriteria searchCriteria,
                                                 ImapMessageFetchAttributeOptions options)
    {
      return GetSortedMessages(sortOrder, searchCriteria, null, options);
    }

    public ImapMessageInfoList GetSortedMessages(ImapSortCriteria sortOrder,
                                                 ImapSearchCriteria searchCriteria,
                                                 Encoding encoding)
    {
      return GetSortedMessages(sortOrder, searchCriteria, encoding, ImapMessageFetchAttributeOptions.Default);
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

    /*
     * miscellaneous operation methods
     */

    /*
     * WaitForMessageArrival
     */
    private const int defaultPollingInterval = 10 * 60 * 1000; // 10 mins
    private const int maximumPollingInterval = 30 * 60 * 1000; // 30 mins

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan timeout)
    {
      return WaitForMessageArrival(defaultPollingInterval,
                                   (int)timeout.TotalMilliseconds,
                                   ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan timeout,
                                                     ImapMessageFetchAttributeOptions options)
    {
      return WaitForMessageArrival(defaultPollingInterval,
                                   (int)timeout.TotalMilliseconds,
                                   options);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsTimeout)
    {
      return WaitForMessageArrival(defaultPollingInterval,
                                   millisecondsTimeout,
                                   ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsTimeout,
                                                     ImapMessageFetchAttributeOptions options)
    {
      return WaitForMessageArrival(defaultPollingInterval,
                                   millisecondsTimeout,
                                   options);
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan pollingInterval,
                                                     TimeSpan timeout)
    {
      return WaitForMessageArrival((int)pollingInterval.TotalMilliseconds,
                                   (int)timeout.TotalMilliseconds,
                                   ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan pollingInterval,
                                                     TimeSpan timeout,
                                                     ImapMessageFetchAttributeOptions options)
    {
      return WaitForMessageArrival((int)pollingInterval.TotalMilliseconds,
                                   (int)timeout.TotalMilliseconds,
                                   options);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsPollingInterval,
                                                     int millisecondsTimeout)
    {
      return WaitForMessageArrival(millisecondsPollingInterval,
                                   millisecondsTimeout,
                                   ImapMessageFetchAttributeOptions.Default);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsPollingInterval,
                                                     int millisecondsTimeout,
                                                     ImapMessageFetchAttributeOptions options)
    {
      if (millisecondsTimeout < -1)
        throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, "must be greater than or equals to -1");

      var asyncResult = BeginWaitForMessageArrival(millisecondsPollingInterval, options, null, null);

      asyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout, false);

      return EndWaitForMessageArrival(asyncResult);
    }

    /*
     * BeginWaitForMessageArrival
     */
    public IAsyncResult BeginWaitForMessageArrival(object asyncState,
                                                   AsyncCallback asyncCallback)
    {
      return BeginWaitForMessageArrival(defaultPollingInterval,
                                        ImapMessageFetchAttributeOptions.Default,
                                        asyncState,
                                        asyncCallback);
    }

    public IAsyncResult BeginWaitForMessageArrival(TimeSpan pollingInterval,
                                                   object asyncState,
                                                   AsyncCallback asyncCallback)
    {
      return BeginWaitForMessageArrival((int)pollingInterval.Milliseconds,
                                        ImapMessageFetchAttributeOptions.Default,
                                        asyncState,
                                        asyncCallback);
    }

    public IAsyncResult BeginWaitForMessageArrival(int millisecondsPollingInterval,
                                                   object asyncState,
                                                   AsyncCallback asyncCallback)
    {
      return BeginWaitForMessageArrival(millisecondsPollingInterval,
                                        ImapMessageFetchAttributeOptions.Default,
                                        asyncState,
                                        asyncCallback);
    }

    public IAsyncResult BeginWaitForMessageArrival(TimeSpan pollingInterval,
                                                   ImapMessageFetchAttributeOptions options,
                                                   object asyncState,
                                                   AsyncCallback asyncCallback)
    {
      return BeginWaitForMessageArrival((int)pollingInterval.TotalMilliseconds,
                                        options,
                                        asyncState,
                                        asyncCallback);
    }

    public IAsyncResult BeginWaitForMessageArrival(int millisecondsPollingInterval,
                                                   ImapMessageFetchAttributeOptions options,
                                                   object asyncState,
                                                   AsyncCallback asyncCallback)
    {
      if (millisecondsPollingInterval <= 0)
        throw new ArgumentOutOfRangeException("millisecondsPollingInterval", millisecondsPollingInterval, "must be non-zero positive number");
      if (waitForMessageArrivalAsyncResult != null)
        throw new InvalidOperationException("another operation proceeding");

      var isIdleCapable = Client.ServerCapabilities.Has(ImapCapability.Idle);

      if (!isIdleCapable && maximumPollingInterval <= millisecondsPollingInterval)
        throw new ArgumentOutOfRangeException("millisecondsPollingInterval",
                                              millisecondsPollingInterval,
                                              string.Format("maximum interval is {0} but requested was {1}", maximumPollingInterval, millisecondsPollingInterval));

      var proc = isIdleCapable
        ? new WaitForMessageArrivalProc(WaitForMessageArrivalIdle)
        : new WaitForMessageArrivalProc(WaitForMessageArrivalNoOp);

      waitForMessageArrivalAsyncResult = new WaitForMessageArrivalAsyncResult(millisecondsPollingInterval,
                                                                              Mailbox,
                                                                              options,
                                                                              proc,
                                                                              asyncState,
                                                                              asyncCallback);

      waitForMessageArrivalAsyncResult.BeginProc();

      return waitForMessageArrivalAsyncResult;
    }

    public ImapMessageInfoList EndWaitForMessageArrival(IAsyncResult asyncResult)
    {
      if (asyncResult != waitForMessageArrivalAsyncResult)
        throw new ArgumentException("invalid IAsyncResult", "asyncResult");

      try {
        waitForMessageArrivalAsyncResult.EndProc();

        return GetArrivalMessages(waitForMessageArrivalAsyncResult);
      }
      finally {
        waitForMessageArrivalAsyncResult = null;
      }
    }

    private ImapMessageInfoList GetArrivalMessages(WaitForMessageArrivalContext context)
    {
      try {
        return new ImapMessageInfoList(this, context.FetchOptions, new ArrivalMessageQuery(context));
      }
      finally {
        context.Dispose();
      }
    }

    private class ArrivalMessageQuery : IImapMessageQuery {
      public ArrivalMessageQuery(WaitForMessageArrivalContext context)
      {
        sequenceSet = (!context.MessageArrived || context.Mailbox.ExistsMessage <= context.CurrentMessageCount)
          ? ImapSequenceSet.CreateSet(new long[0])
          : ImapSequenceSet.CreateRangeSet(context.CurrentMessageCount + 1, context.Mailbox.ExistsMessage);
      }

      public ImapSequenceSet GetSequenceOrUidSet(ImapOpenedMailboxInfo mailbox)
      {
        return sequenceSet;
      }

      private ImapSequenceSet sequenceSet;
    }

    private delegate bool WaitForMessageArrivalProc(WaitForMessageArrivalContext context);
    private WaitForMessageArrivalAsyncResult waitForMessageArrivalAsyncResult = null;

    private class WaitForMessageArrivalContext : IDisposable {
      public bool MessageArrived {
        get; protected set;
      }

      public WaitHandle AbortEvent {
        get { return abortEvent; }
      }

      public long CurrentMessageCount {
        get; set;
      }

      public int PollingInterval {
        get; private set;
      }

      public ImapMailbox Mailbox {
        get; private set;
      }

      public ImapMessageFetchAttributeOptions FetchOptions {
        get; private set;
      }

      public WaitForMessageArrivalContext(int pollingInterval,
                                          ImapMailbox mailbox,
                                          ImapMessageFetchAttributeOptions fetchOptions)
      {
        this.PollingInterval = pollingInterval;
        this.Mailbox = mailbox;
        this.CurrentMessageCount = mailbox.ExistsMessage;
        this.FetchOptions = fetchOptions;
      }

      public void Dispose()
      {
        if (abortEvent != null) {
          abortEvent.Close();
          abortEvent = null;
        }
      }

      protected void Abort()
      {
        abortEvent.Set();
      }

      private ManualResetEvent abortEvent = new ManualResetEvent(false);
    }

    private class WaitForMessageArrivalAsyncResult :
      WaitForMessageArrivalContext,
      IAsyncResult
    {
      public object AsyncState {
        get; private set;
      }

      public WaitHandle AsyncWaitHandle {
        get { return procAsyncResult.AsyncWaitHandle; }
      }

      public bool CompletedSynchronously {
        get { return false; }
      }

      public bool IsCompleted {
        get { return procAsyncResult.AsyncWaitHandle.WaitOne(0, false); }
      }

      private AsyncCallback asyncCallback;
      private WaitForMessageArrivalProc proc;
      private IAsyncResult procAsyncResult;

      public WaitForMessageArrivalAsyncResult(int pollingInterval,
                                              ImapMailbox mailbox,
                                              ImapMessageFetchAttributeOptions fetchOptions,
                                              WaitForMessageArrivalProc proc,
                                              object asyncState,
                                              AsyncCallback asyncCallback)
        : base(pollingInterval, mailbox, fetchOptions)
      {
        this.proc = proc;
        this.AsyncState = asyncState;
        this.asyncCallback = asyncCallback;
      }

      public void BeginProc()
      {
        procAsyncResult = proc.BeginInvoke(this, null, null);
      }

      public void EndProc()
      {
        Abort();

        MessageArrived = proc.EndInvoke(procAsyncResult);

        if (MessageArrived && asyncCallback != null)
          asyncCallback(this);
      }
    }

    private bool WaitForMessageArrivalIdle(WaitForMessageArrivalContext context)
    {
      var waitHandles = new WaitHandle[] {context.AbortEvent, null};

      for (;;) {
        if (context.AbortEvent.WaitOne(0, false))
          return false; // aborted

        var idleAsyncResult = Client.Session.BeginIdle(context, delegate(object idleState, ImapUpdatedStatus updatedStatus) {
          var ctx = idleState as WaitForMessageArrivalAsyncResult;

          if (updatedStatus.Expunge.HasValue)
            ctx.CurrentMessageCount--;
          else if (updatedStatus.Exists.HasValue && ctx.CurrentMessageCount < updatedStatus.Exists.Value)
            return false; // DONE

          return true; // keep IDLEing
        });

        waitHandles[1] = idleAsyncResult.AsyncWaitHandle;

        var index = WaitHandle.WaitAny(waitHandles, 180 * 1000, false); // send DONE 3 minute after IDLE started

        ProcessResult(Client.Session.EndIdle(idleAsyncResult));

        switch (index) {
          case 0: // aborted
            return false;

          case 1: // IDLE completed
            return true;

          case WaitHandle.WaitTimeout:
          default:
            continue; // re-IDLE
        }
      }
    }

    private bool WaitForMessageArrivalNoOp(WaitForMessageArrivalContext context)
    {
      for (;;) {
        if (context.AbortEvent.WaitOne(context.PollingInterval, false))
          return false; // aborted

        var result = ProcessResult(Client.Session.NoOp());

        foreach (var response in result.ReceivedResponses) {
          var dataResponse = response as ImapDataResponse;

          if (dataResponse == null)
            continue;

          if (dataResponse.Type == ImapDataResponseType.Expunge)
            context.CurrentMessageCount--;
        }

        if (context.CurrentMessageCount < context.Mailbox.ExistsMessage)
          return true; // new message arrival
      }
    }
  }
}
