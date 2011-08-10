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
using System.Threading;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  partial class ImapOpenedMailboxInfo {
    private const int defaultPollingInterval = 10 * 60 * 1000; // 10 mins
    private const int maximumPollingInterval = 30 * 60 * 1000; // 30 mins
    private const int reidleInterval = Timeout.Infinite; // 180 * 1000;

    internal delegate void WatchMailboxProc(WatchMailboxContextBase context);
    internal delegate bool KeepWatchingPredicate(WatchMailboxContextBase context,
                                                 IEnumerable<ImapResponse> receivedResponses);
    private WatchMailboxContextBase watchMailboxContext = null;
    private readonly object watchMailboxContextLockObject = new object();

    public class WatchMailboxContext {
      public ImapOpenedMailboxInfo Mailbox {
        get { return mailbox; }
      }

      internal WatchMailboxContext(ImapOpenedMailboxInfo mailbox,
                                   WatchMailboxContextBase context)
      {
        this.context = context;
        this.mailbox = mailbox;
      }

      public bool Wait()
      {
        return Wait(Timeout.Infinite);
      }

      public bool Wait(int millisecondsTimeout)
      {
        return context.Wait(millisecondsTimeout);
      }

      private readonly WatchMailboxContextBase context;
      private readonly ImapOpenedMailboxInfo mailbox;
    }

    internal abstract class WatchMailboxContextBase : IDisposable {
      public int NoOpPollingInterval {
        get { return noopPollingInterval; }
      }

      public KeepWatchingPredicate KeepWatchingPredicate {
        get { return keepWatchingPredicate; }
      }

      public WaitHandle StopEvent {
        get { return stopEvent; }
      }

      internal protected WatchMailboxContextBase(WatchMailboxProc watchMailboxProc,
                                                 int noopPollingInterval,
                                                 KeepWatchingPredicate keepWatchingPredicate)
      {
        this.watchMailboxProc = watchMailboxProc;
        this.noopPollingInterval = noopPollingInterval;
        this.keepWatchingPredicate = keepWatchingPredicate;
      }

      public void Dispose()
      {
        if (stopEvent != null) {
          stopEvent.Close();
          stopEvent = null;
        }
      }

      public void StartWatching()
      {
        watchMailboxAsyncResult = watchMailboxProc.BeginInvoke(this,
                                                               null,
                                                               null);
      }

      public void StopWatching()
      {
        stopEvent.Set();

        try {
          if (Runtime.IsRunningOnMono && !watchMailboxAsyncResult.IsCompleted)
            // XXX: mono 2.6.x bug?
            watchMailboxAsyncResult.AsyncWaitHandle.WaitOne();

          watchMailboxProc.EndInvoke(watchMailboxAsyncResult);
        }
        finally {
          watchMailboxAsyncResult = null;
        }
      }

      public bool Wait(int millisecondsTimeout)
      {
        if (watchMailboxAsyncResult == null)
          throw new InvalidOperationException("watch operation already stopped");

        if (watchMailboxAsyncResult.IsCompleted || watchMailboxAsyncResult.CompletedSynchronously)
          return true;

        return watchMailboxAsyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout, false);
      }

      public void Complete(bool stopped)
      {
        if (doneCalled)
          return;

        try {
          Done(stopped);
        }
        finally {
          doneCalled = true;
        }
      }

      protected virtual void Done(bool stopped)
      {
        // do nothing
      }

      private readonly WatchMailboxProc watchMailboxProc;
      private readonly int noopPollingInterval;
      private readonly KeepWatchingPredicate keepWatchingPredicate;
      private ManualResetEvent stopEvent = new ManualResetEvent(false);
      private IAsyncResult watchMailboxAsyncResult;
      private bool doneCalled = false;
    }

    private void WatchMailboxByIdle(WatchMailboxContextBase context)
    {
      try {
        var waitHandles = new WaitHandle[] {context.StopEvent, null};
        var receivedResponses = new ImapResponse[1];

        for (;;) {
          var idleAsyncResult = Client.Session.BeginIdle(context, delegate(object idleState, ImapResponse receivedResponse) {
            var ctx = idleState as WatchMailboxContextBase;

            receivedResponses[0] = receivedResponse;

            ProcessSizeAndStatusResponse(receivedResponses);

            return ctx.KeepWatchingPredicate(ctx, receivedResponses);
          });

          waitHandles[1] = idleAsyncResult.AsyncWaitHandle;

          var index = WaitHandle.WaitAny(waitHandles, reidleInterval, false);

          // ignores mailbox size and message status update
          base.ProcessResult(Client.Session.EndIdle(idleAsyncResult));

          switch (index) {
            case 0:
              context.Complete(true);
              return; // stopped

            case 1: // IDLE completed
              context.Complete(false);
              return; // done

            case WaitHandle.WaitTimeout:
            default:
              continue; // re-IDLE
          }
        }
      }
      catch {
        context.Complete(true); // stopped by exception

        throw;
      }
    }

    private void WatchMailboxByNoOp(WatchMailboxContextBase context)
    {
      try {
        ImapCommandResult result = null;

        for (;;) {
          result = ProcessResult(Client.Session.NoOp(), null);

          if (!context.KeepWatchingPredicate(context, result.ReceivedResponses)) {
            context.Complete(false);
            return; // done
          }

          if (context.StopEvent.WaitOne(context.NoOpPollingInterval, false))
            break; // stopped
        }

        result = ProcessResult(Client.Session.NoOp(), null);

        context.KeepWatchingPredicate(context, result.ReceivedResponses);

        context.Complete(true);
      }
      catch {
        context.Complete(true); // stopped by exception

        throw;
      }
    }

    private WatchMailboxProc GetWatchMailboxProc(int millisecondsPollingInterval,
                                                 bool throwIfIdleIncapable)
    {
      if (watchMailboxContext != null)
        throw new ImapProtocolViolationException("another watch operation proceeding");

      if (Client.ServerCapabilities.Contains(ImapCapability.Idle)) {
        return WatchMailboxByIdle;
      }
      else {
        if (throwIfIdleIncapable)
          throw new ImapIncapableException(ImapCapability.Idle);

        if (millisecondsPollingInterval <= -1) // disallow Timeout.Infinite
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("millisecondsPollingInterval",
                                                                  millisecondsPollingInterval);
        if (maximumPollingInterval <= millisecondsPollingInterval)
          throw ExceptionUtils.CreateArgumentMustBeLessThan(maximumPollingInterval,
                                                            "millisecondsPollingInterval",
                                                            millisecondsPollingInterval);

        return WatchMailboxByNoOp;
      }
    }

    private static Exception CreateInvalidStopWatchMailboxOperation()
    {
      return new ImapProtocolViolationException("watching operation not started or another operation proceeding");
    }

    private static void CheckArgWatchTimeoutMilliseconds(int millisecondsTimeout,
                                                         bool allowInfinite)
    {
      if (allowInfinite) {
        if (millisecondsTimeout < Timeout.Infinite)
          throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(Timeout.Infinite,
                                                                        "millisecondsTimeout",
                                                                        millisecondsTimeout);
      }
      else {
        if (millisecondsTimeout <= -1) // disallow Timeout.Infinite
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("millisecondsTimeout",
                                                                  millisecondsTimeout);
      }
    }

    /*
     * (Start|Stop)Idle
     */
    private class IdleContext : WatchMailboxContextBase {
      public IdleContext(WatchMailboxProc watchMailboxProc,
                         int noopPollingInterval)
        : base(watchMailboxProc,
               noopPollingInterval,
               IdleKeepWatchingProc)
      {
      }
    }

    private static bool IdleKeepWatchingProc(WatchMailboxContextBase context,
                                             IEnumerable<ImapResponse> receivedResponses)
    {
      return true;
    }

    public void Idle(TimeSpan timeout)
    {
      Idle((int)timeout.TotalMilliseconds,
           defaultPollingInterval);
    }

    public void Idle(int millisecondsTimeout)
    {
      Idle(millisecondsTimeout,
           defaultPollingInterval);
    }

    public void Idle(TimeSpan timeout,
                     TimeSpan pollingInterval)
    {
      Idle((int)timeout.TotalMilliseconds,
           (int)pollingInterval.TotalMilliseconds);
    }

    public void Idle(int millisecondsTimeout,
                     int millisecondsPollingInterval)
    {
      CheckArgWatchTimeoutMilliseconds(millisecondsTimeout,
                                       false);

      StartIdle(millisecondsPollingInterval).Wait(millisecondsTimeout);

      StopIdle();
    }

    public WatchMailboxContext StartIdle()
    {
      return StartIdle(defaultPollingInterval);
    }

    public WatchMailboxContext StartIdle(TimeSpan pollingInterval)
    {
      return StartIdle((int)pollingInterval.TotalMilliseconds);
    }

    public WatchMailboxContext StartIdle(int millisecondsPollingInterval)
    {
      lock (watchMailboxContextLockObject) {
        watchMailboxContext = new IdleContext(GetWatchMailboxProc(millisecondsPollingInterval,
                                                                  false),
                                              millisecondsPollingInterval);

        // TODO: throw exception if NO, BYE, etc
        watchMailboxContext.StartWatching();

        return new WatchMailboxContext(this, watchMailboxContext);
      }
    }

    public void StopIdle()
    {
      lock (watchMailboxContextLockObject) {
        using (var context = watchMailboxContext as IdleContext) {
          if (context == null)
            throw CreateInvalidStopWatchMailboxOperation();

          try {
            context.StopWatching();
          }
          finally {
            watchMailboxContext = null;
          }
        }
      }
    }

    /*
     * (Start|Stop)WaitForMessageArrival
     */
    private class WaitForMessageArrivalContext : WatchMailboxContextBase {
      public ImapMessageFetchAttributeOptions FetchAttributeOptions {
        get { return fetchAttributeOptions; }
      }

      public long ExistMessageCount;
      public ImapSequenceSet ArrivalMessageSequenceSet;

      public WaitForMessageArrivalContext(WatchMailboxProc watchMailboxProc,
                                          int noopPollingInterval,
                                          long existMessageCount,
                                          ImapMessageFetchAttributeOptions fetchAttributeOptions)
        : base(watchMailboxProc,
               noopPollingInterval,
               WaitForMessageArrivalKeepWatchingProc)
      {
        this.ExistMessageCount = existMessageCount;
        this.fetchAttributeOptions = fetchAttributeOptions;
      }

      private long existMessageCount;
      private readonly ImapMessageFetchAttributeOptions fetchAttributeOptions;
    }

    private static bool WaitForMessageArrivalKeepWatchingProc(WatchMailboxContextBase context,
                                                              IEnumerable<ImapResponse> receivedResponses)
    {
      var ctx = context as WaitForMessageArrivalContext;
      var prevExistMessageCount = ctx.ExistMessageCount;

      foreach (var response in receivedResponses) {
        var data = response as ImapDataResponse;

        if (data == null)
          continue;

        if (data.Type == ImapDataResponseType.Exists) {
          ctx.ExistMessageCount = ImapDataResponseConverter.FromExists(data);
        }
        else if (data.Type == ImapDataResponseType.Expunge) {
          if (0L < ctx.ExistMessageCount)
            ctx.ExistMessageCount--;
        }
      }

      if (prevExistMessageCount < ctx.ExistMessageCount) {
        ctx.ArrivalMessageSequenceSet = ImapSequenceSet.CreateRangeSet(prevExistMessageCount + 1,
                                                                       ctx.ExistMessageCount);

        return false; // stop watching
      }
      else {
        return true; // keep watching
      }
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan timeout)
    {
      return WaitForMessageArrival((int)timeout.TotalMilliseconds,
                                   ImapMessageFetchAttributeOptions.Default,
                                   defaultPollingInterval);
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan timeout,
                                                     ImapMessageFetchAttributeOptions fetchAttributeOptions)
    {
      return WaitForMessageArrival((int)timeout.TotalMilliseconds,
                                   fetchAttributeOptions,
                                   defaultPollingInterval);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsTimeout)
    {
      return WaitForMessageArrival(millisecondsTimeout,
                                   ImapMessageFetchAttributeOptions.Default,
                                   defaultPollingInterval);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsTimeout,
                                                     ImapMessageFetchAttributeOptions fetchAttributeOptions)
    {
      return WaitForMessageArrival(millisecondsTimeout,
                                   fetchAttributeOptions,
                                   defaultPollingInterval);
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan timeout,
                                                     TimeSpan pollingInterval)
    {
      return WaitForMessageArrival((int)timeout.TotalMilliseconds,
                                   ImapMessageFetchAttributeOptions.Default,
                                   (int)pollingInterval.TotalMilliseconds);
    }

    public ImapMessageInfoList WaitForMessageArrival(TimeSpan timeout,
                                                     ImapMessageFetchAttributeOptions fetchAttributeOptions,
                                                     TimeSpan pollingInterval)
    {
      return WaitForMessageArrival((int)timeout.TotalMilliseconds,
                                   fetchAttributeOptions,
                                   (int)pollingInterval.TotalMilliseconds);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsTimeout,
                                                     int millisecondsPollingInterval)
    {
      return WaitForMessageArrival(millisecondsTimeout,
                                   ImapMessageFetchAttributeOptions.Default,
                                   millisecondsPollingInterval);
    }

    public ImapMessageInfoList WaitForMessageArrival(int millisecondsTimeout,
                                                     ImapMessageFetchAttributeOptions fetchAttributeOptions,
                                                     int millisecondsPollingInterval)
    {
      CheckArgWatchTimeoutMilliseconds(millisecondsTimeout,
                                       true);

      StartWaitForMessageArrival(fetchAttributeOptions,
                                 millisecondsPollingInterval).Wait(millisecondsTimeout);

      return StopWaitForMessageArrival();
    }

    public WatchMailboxContext StartWaitForMessageArrival()
    {
      return StartWaitForMessageArrival(ImapMessageFetchAttributeOptions.Default,
                                        defaultPollingInterval);
    }

    public WatchMailboxContext StartWaitForMessageArrival(TimeSpan pollingInterval)
    {
      return StartWaitForMessageArrival(ImapMessageFetchAttributeOptions.Default,
                                        (int)pollingInterval.Milliseconds);
    }

    public WatchMailboxContext StartWaitForMessageArrival(int millisecondsPollingInterval)
    {
      return StartWaitForMessageArrival(ImapMessageFetchAttributeOptions.Default,
                                        millisecondsPollingInterval);
    }

    public WatchMailboxContext StartWaitForMessageArrival(ImapMessageFetchAttributeOptions fetchAttributeOptions)
    {
      return StartWaitForMessageArrival(fetchAttributeOptions,
                                        defaultPollingInterval);
    }

    public WatchMailboxContext StartWaitForMessageArrival(ImapMessageFetchAttributeOptions fetchAttributeOptions,
                                                          TimeSpan pollingInterval)
    {
      return StartWaitForMessageArrival(fetchAttributeOptions,
                                        (int)pollingInterval.TotalMilliseconds);
    }

    public WatchMailboxContext StartWaitForMessageArrival(ImapMessageFetchAttributeOptions fetchAttributeOptions,
                                                          int millisecondsPollingInterval)
    {
      lock (watchMailboxContextLockObject) {
        watchMailboxContext = new WaitForMessageArrivalContext(GetWatchMailboxProc(millisecondsPollingInterval,
                                                                                   true),
                                                               millisecondsPollingInterval,
                                                               Mailbox.ExistsMessage,
                                                               fetchAttributeOptions);

        // TODO: throw exception if NO, BYE, etc
        watchMailboxContext.StartWatching();

        return new WatchMailboxContext(this, watchMailboxContext);
      }
    }

    public ImapMessageInfoList StopWaitForMessageArrival()
    {
      lock (watchMailboxContextLockObject) {
        using (var context = watchMailboxContext as WaitForMessageArrivalContext) {
          if (context == null)
            throw CreateInvalidStopWatchMailboxOperation();

          try {
            context.StopWatching();

            return new ImapMessageInfoList(this,
                                           context.FetchAttributeOptions,
                                           new ArrivalMessageQuery(context.ArrivalMessageSequenceSet));
          }
          finally {
            watchMailboxContext = null;
          }
        }
      }
    }

    private class ArrivalMessageQuery : IImapMessageQuery {
      public ArrivalMessageQuery(ImapSequenceSet arrivalMessageSequenceSet)
      {
        this.sequenceSet = arrivalMessageSequenceSet ?? ImapSequenceSet.CreateSet(new long[0]);
      }

      public ImapSequenceSet GetSequenceOrUidSet(ImapOpenedMailboxInfo mailbox)
      {
        return sequenceSet;
      }

      private ImapSequenceSet sequenceSet;
    }

#if false
    public ImapMessageInfoList WaitForMessageArrival(int millisecondsPollingInterval,
                                                     Func<ImapOpenedMailboxInfo, bool> keepWatching,
                                                     ImapMessageFetchAttributeOptions options)
    {
    }
#endif
  }
}

