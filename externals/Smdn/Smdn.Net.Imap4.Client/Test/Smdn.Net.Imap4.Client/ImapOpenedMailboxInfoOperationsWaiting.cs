using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapOpenedMailboxInfoOperationsWaitingTests {
    public static void TestOpenedMailboxDisconnectable(ImapCapability[] capabilities, string mailboxName, string selectResponse, Action<ImapPseudoServer, ImapOpenedMailboxInfo> action)
    {
      TestUtils.TestAuthenticated(capabilities, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse(string.Format("* LIST () \"\" {0}\r\n", mailboxName) +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse(selectResponse);

        var opened = client.OpenMailbox(mailboxName);

        server.DequeueRequest(); // LIST
        server.DequeueRequest(); // SELECT

        action(server, opened);
      });
    }

    [Test]
    public void TestIdleIdleCapable()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(0.5);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(timeout + TimeSpan.FromSeconds(0.25));

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          mailbox.Idle(timeout);

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, timeout);
        }
      });
    }

    [Test]
    public void TestIdleIdleCapableTimeoutZero()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var delay = TimeSpan.FromSeconds(0.1);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(delay);

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          mailbox.Idle(0);

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, delay);
        }
      });
    }

    [Test]
    public void TestIdleIdleCapableDisconnectFromServer()
    {
      TestOpenedMailboxDisconnectable(new[] {ImapCapability.Idle},
                                      "INBOX",
                                      "$tag OK [READ-WRITE] done\r\n",
                                      delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(1.5);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(TimeSpan.FromSeconds(0.25));

            server.EnqueueResponse("* BYE Server shutting down.\r\n");
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          try {
            mailbox.Idle(timeout);

            Assert.Fail("ImapConnectionException not thrown");
          }
          catch (ImapConnectionException) {
            sw.Stop();

            Assert.LessOrEqual(sw.Elapsed, timeout);
          }

          server.Stop();
        }
      });
    }

    [Test]
    public void TestIdleIdleIncapable()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // NOOP (initial)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (first polling)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (stopped)
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        var timeout = TimeSpan.FromSeconds(0.5);
        var sw = Stopwatch.StartNew();

        mailbox.Idle(timeout, TimeSpan.FromSeconds(0.35));

        sw.Stop();

        Assert.GreaterOrEqual(sw.Elapsed, timeout);
      });
    }

    [Test]
    public void TestIdleIdleIncapableTimeoutZero()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // NOOP (initial)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (stopped)
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        var sw = Stopwatch.StartNew();

        mailbox.Idle(0, 1000);

        sw.Stop();

        Assert.GreaterOrEqual(sw.Elapsed, TimeSpan.Zero);
      });
    }

    [Test]
    public void TestIdleIdleIncapableDisconnectFromServer()
    {
      TestOpenedMailboxDisconnectable(new ImapCapability[0],
                                      "INBOX",
                                      "$tag OK [READ-WRITE] done\r\n",
                                      delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // NOOP (initial)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (first polling)
        server.EnqueueResponse("* BYE Server shutting down.\r\n");

        var timeout = TimeSpan.FromSeconds(2.5);
        var pollingInterval = TimeSpan.FromSeconds(0.25);
        var sw = Stopwatch.StartNew();

        try {
          mailbox.Idle(timeout, pollingInterval);

          Assert.Fail("ImapConnectionException not thrown");
        }
        catch (ImapConnectionException) {
          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, pollingInterval);
        }

        server.Stop();
      });
    }

    [Test]
    public void TestIdleStopIdleWhileAwaiting()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(2.5);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");

            try {
              mailbox.StopIdle();
            }
            catch {
              // ignore exceptions
            }
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          try {
            mailbox.Idle(timeout);

            Assert.Fail("ImapProtocolViolationException not thrown");
          }
          catch (ImapProtocolViolationException) {
            sw.Stop();

            Assert.LessOrEqual(sw.Elapsed, timeout);
          }
        }
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIdleTimeoutInfinite()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.Idle(Timeout.Infinite);
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIdleTimeoutArgumentOutOfRange()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.Idle(-2);
      });
    }

    [Test]
    public void TestStartIdleIdleCapablePollingIntervalInifinite()
    {
      StartIdleIdleCapablePollingIntervalIgnored(Timeout.Infinite);
    }

    [Test]
    public void TestStartIdleIdleCapablePollingIntervalArgumentOutOfRange1()
    {
      StartIdleIdleCapablePollingIntervalIgnored(-2);
    }

    [Test]
    public void TestStartIdleIdleCapablePollingIntervalArgumentOutOfRange2()
    {
      StartIdleIdleCapablePollingIntervalIgnored((int)TimeSpan.FromMinutes(30.0).TotalMilliseconds);
    }

    private void StartIdleIdleCapablePollingIntervalIgnored(int millisecondsPollingInterval)
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(0.25);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            waitHandle.WaitOne();

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          waitHandle.WaitOne();
          waitHandle.Reset();

          var sw = Stopwatch.StartNew();

          mailbox.StartIdle(millisecondsPollingInterval);

          Thread.Sleep(timeout);

          waitHandle.Set();

          mailbox.StopIdle();

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, timeout);
        }
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestStartIdleIdleIncapablePollingIntervalInfinite()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StartIdle(Timeout.Infinite);
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestStartIdleIdleIncapablePollingIntervalArgumentOutOfRange1()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StartIdle(-2);
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestStartIdleIdleIncapablePollingIntervalArgumentOutOfRange2()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StartIdle(TimeSpan.FromMinutes(30.0));
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStartIdleAnotherWatchOperationProceeding1()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // first
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartIdle();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        // second
        try {
          mailbox.StartIdle();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopIdle();
        }
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStartIdleAnotherWatchOperationProceeding2()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // first
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartWaitForMessageArrival();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        // second
        try {
          mailbox.StartIdle();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopWaitForMessageArrival();
        }
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStopIdleWatchOperationNotStarted()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StopIdle();
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStopIdleAnotherWatchOperationProceeding()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartWaitForMessageArrival();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        try {
          mailbox.StopIdle();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopWaitForMessageArrival();
        }
      });
    }

    [Test]
    public void TestWaitForMessageArrivalIdleCapable()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(0.5);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(timeout + TimeSpan.FromSeconds(0.25));

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          var arrivalMessages = mailbox.WaitForMessageArrival(timeout);

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, timeout);

          Assert.IsNotNull(arrivalMessages);
          CollectionAssert.IsEmpty(arrivalMessages);
        }
      });
    }

    [Test]
    public void TestWaitForMessageArrivalIdleCapableTimeoutZero()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var delay = TimeSpan.FromSeconds(0.1);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(delay);

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          var arrivalMessages = mailbox.WaitForMessageArrival(0);

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, delay);

          Assert.IsNotNull(arrivalMessages);
          CollectionAssert.IsEmpty(arrivalMessages);
        }
      });
    }

    [Test]
    public void TestWaitForMessageArrivalIdleCapableTimeoutInfinite()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var delay = TimeSpan.FromSeconds(0.1);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(delay);

            server.EnqueueResponse("* RECENT 1\r\n");
            server.EnqueueResponse("* EXISTS 1\r\n");

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          var arrivalMessages = mailbox.WaitForMessageArrival(Timeout.Infinite);

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, delay);

          Assert.IsNotNull(arrivalMessages);

          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                       "$tag OK done\r\n");

          var arrivalMessageArray = arrivalMessages.ToArray();

          Assert.AreEqual(1, arrivalMessageArray.Length);
          Assert.AreEqual(1L, arrivalMessageArray[0].Uid, "arrival message uid");
          Assert.AreEqual(1L, arrivalMessageArray[0].Sequence, "arrival message seq");
        }
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestWaitForMessageArrivalIdleIncapable()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        server.WaitForRequest = true;
        // NOOP (initial)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (first polling)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (stopped)
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        var timeout = TimeSpan.FromSeconds(0.5);
        var sw = Stopwatch.StartNew();

        var arrivalMessages = mailbox.WaitForMessageArrival(timeout,
                                                            TimeSpan.FromSeconds(0.35));

        sw.Stop();

        Assert.GreaterOrEqual(sw.Elapsed, timeout);

        Assert.IsNotNull(arrivalMessages);
        CollectionAssert.IsEmpty(arrivalMessages);
      });
    }

    [Test]
    [Ignore("to be added")]
    public void TestWaitForMessageArrivalIdleIncapableTimeoutZero()
    {
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestWaitForMessageArrivalIdleIncapableTimeoutInfinite()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
#if false
        // NOOP (initial)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (first polling)
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // NOOP (second polling)
        server.EnqueueTaggedResponse("* RECENT 1\r\n" +
                                     "* EXISTS 1\r\n" +
                                     "$tag OK done\r\n");
#endif

        var pollingInterval = TimeSpan.FromSeconds(0.25);
        var sw = Stopwatch.StartNew();

        var arrivalMessages = mailbox.WaitForMessageArrival(Timeout.Infinite,
                                                            (int)pollingInterval.TotalMilliseconds);

#if false
        sw.Stop();

        Assert.GreaterOrEqual(sw.Elapsed,
                              TimeSpan.FromSeconds(pollingInterval.TotalSeconds * 2.0));

        Assert.IsNotNull(arrivalMessages);

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                     "$tag OK done\r\n");

        var arrivalMessageArray = arrivalMessages.ToArray();

        Assert.AreEqual(1, arrivalMessageArray.Length);
        Assert.AreEqual(1L, arrivalMessageArray[0].Uid, "arrival message uid");
        Assert.AreEqual(1L, arrivalMessageArray[0].Sequence, "arrival message seq");
#endif
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestWaitForMessageArrivalTimeoutArgumentOutOfRange()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.WaitForMessageArrival(-2);
      });
    }

    [Test]
    public void TestWaitForMessageArrivalStopWaitForMessageArrivalWhileAwaiting()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(2.5);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(TimeSpan.FromSeconds(0.25));

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");

            mailbox.StopWaitForMessageArrival();
          });

          var sw = Stopwatch.StartNew();

          waitHandle.WaitOne();
          waitHandle.Reset();

          try {
            mailbox.WaitForMessageArrival(timeout);

            Assert.Fail("ImapProtocolViolationException not thrown");
          }
          catch (ImapProtocolViolationException) {
            sw.Stop();

            Assert.LessOrEqual(sw.Elapsed, timeout);
          }
        }
      });
    }

    [Test]
    public void TestStartWaitForMessageArrivalIdleCapablePollingIntervalInifinite()
    {
      StartWaitForMessageArrivalIdleCapablePollingIntervalIgnored(-1);
    }

    [Test]
    public void TestStartWaitForMessageArrivalIdleCapablePollingIntervalArgumentOutOfRange1()
    {
      StartWaitForMessageArrivalIdleCapablePollingIntervalIgnored(-2);
    }

    [Test]
    public void TestStartWaitForMessageArrivalIdleCapablePollingIntervalArgumentOutOfRange2()
    {
      StartWaitForMessageArrivalIdleCapablePollingIntervalIgnored((int)TimeSpan.FromMinutes(30.0).TotalMilliseconds);
    }

    private void StartWaitForMessageArrivalIdleCapablePollingIntervalIgnored(int millisecondsPollingInterval)
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var timeout = TimeSpan.FromSeconds(0.25);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            waitHandle.WaitOne();

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          waitHandle.WaitOne();
          waitHandle.Reset();

          var sw = Stopwatch.StartNew();

          mailbox.StartWaitForMessageArrival(millisecondsPollingInterval);

          Thread.Sleep(timeout);

          waitHandle.Set();

          mailbox.StopWaitForMessageArrival();

          sw.Stop();

          Assert.GreaterOrEqual(sw.Elapsed, timeout);
        }
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException/*ArgumentOutOfRangeException*/))]
    public void TestStartWaitForMessageArrivalIdleIncapablePollingIntervalInfinite()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StartWaitForMessageArrival(Timeout.Infinite);
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException/*ArgumentOutOfRangeException*/))]
    public void TestStartWaitForMessageArrivalIdleIncapablePollingIntervalArgumentOutOfRange1()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StartWaitForMessageArrival(-2);
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException/*ArgumentOutOfRangeException*/))]
    public void TestStartWaitForMessageArrivalIdleIncapablePollingIntervalArgumentOutOfRange2()
    {
      TestUtils.TestOpenedMailbox("INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StartWaitForMessageArrival(TimeSpan.FromMinutes(30.0));
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStartWaitForMessageArrivalAnotherWatchOperationProceeding1()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {

        // first
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartWaitForMessageArrival();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        // second
        try {
          mailbox.StartWaitForMessageArrival();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopWaitForMessageArrival();
        }
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStartWaitForMessageArrivalAnotherWatchOperationProceeding2()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // first
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartIdle();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        // second
        try {
          mailbox.StartWaitForMessageArrival();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopIdle();
        }
      });
    }

    [Test, Ignore("to be modified")]
    public void TestStartWaitForMessageArrivalCallback()
    {
#if false
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var delay = TimeSpan.FromSeconds(0.1);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(delay);

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          waitHandle.WaitOne();

          var sw = Stopwatch.StartNew();
          var calledBack = false;

          waitHandle.Reset();

          mailbox.StartWaitForMessageArrival(mailbox, delegate(object state) {
            calledBack = true;

            Assert.AreSame(state, mailbox);

            sw.Stop();
          });

          var arrivalMessages = mailbox.StopWaitForMessageArrival();

          Assert.IsTrue(calledBack);
          Assert.GreaterOrEqual(sw.Elapsed, delay);

          Assert.IsNotNull(arrivalMessages);
          CollectionAssert.IsEmpty(arrivalMessages);
        }
      });
#endif
    }

    [Test, Ignore("to be modified")]
    public void TestStartWaitForMessageArrivalCallbackMessageArrived()
    {
#if false
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var delayArrival = TimeSpan.FromSeconds(1.0);
          var delayDone = TimeSpan.FromSeconds(0.25);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            waitHandle.WaitOne();

            Thread.Sleep(delayArrival);

            server.EnqueueResponse("* EXISTS 1\r\n");

            Thread.Sleep(delayDone);

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          waitHandle.WaitOne();

          var sw = Stopwatch.StartNew();
          var calledBack = false;

          waitHandle.Reset();

          mailbox.StartWaitForMessageArrival(mailbox, delegate(object state) {
            calledBack = true;

            Assert.AreSame(state, mailbox);

            sw.Stop();
          });

          waitHandle.Set();

          var arrivalMessages = mailbox.StopWaitForMessageArrival();

          Assert.IsTrue(calledBack);
          Assert.GreaterOrEqual(sw.Elapsed, delayArrival);
          Assert.GreaterOrEqual(sw.Elapsed, delayArrival + delayDone);

          Assert.IsNotNull(arrivalMessages);

          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                       "$tag OK done\r\n");

          var arrivalMessageArray = arrivalMessages.ToArray();

          Assert.AreEqual(1, arrivalMessageArray.Length);
          Assert.AreEqual(1L, arrivalMessageArray[0].Uid, "arrival message uid");
          Assert.AreEqual(1L, arrivalMessageArray[0].Sequence, "arrival message seq");
        }
      });
#endif
    }

    [Test, Ignore("to be modified")]
    public void TestStartWaitForMessageArrivalCallbackThrowsException()
    {
#if false
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            // DONE
            server.EnqueueTaggedResponse("$tag OK done\r\n");
          });

          waitHandle.WaitOne();
          waitHandle.Reset();

          var throwingException = new Exception("test exception");

          mailbox.StartWaitForMessageArrival(mailbox, delegate(object state) {
            throw new Exception("test exception");
          });

          try {
            mailbox.StopWaitForMessageArrival();

            Assert.Fail("exception not thrown");
          }
          catch (Exception ex) {
            Assert.IsInstanceOfType(typeof(Exception), ex);
            Assert.AreEqual(throwingException.Message, ex.Message);
          }
        }
      });
#endif
    }

    [Test, Ignore("to be modified")]
    public void TestStartWaitForMessageArrivalCallbackDisconnectFromServer()
    {
#if false
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 1]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestOpenedMailboxDisconnectable(new[] {ImapCapability.Idle},
                                      "INBOX",
                                      selectResp,
                                      delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        using (var waitHandle = new ManualResetEvent(false)) {
          var delay = TimeSpan.FromSeconds(0.25);

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            waitHandle.Set();

            // IDLE
            server.EnqueueResponse("+ idling\r\n");

            Thread.Sleep(delay);

            server.EnqueueResponse("* BYE Server shutting down.\r\n");
          });

          waitHandle.WaitOne();

          var sw = Stopwatch.StartNew();
          var calledBack = false;

          waitHandle.Reset();

          mailbox.StartWaitForMessageArrival(mailbox, delegate(object state) {
            calledBack = true;

            Assert.AreSame(state, mailbox);

            sw.Stop();
          });

          try {
            mailbox.StopWaitForMessageArrival();

            Assert.Fail("ImapConnectionException not thrown");
          }
          catch (ImapConnectionException) {
            Assert.IsTrue(calledBack);
            Assert.GreaterOrEqual(sw.Elapsed, delay);
          }

          server.Stop();
        }
      });
#endif
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStopWaitForMessageArrivalWatchOperationNotStarted()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.StopWaitForMessageArrival();
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStopWaitForMessageArrivalAnotherWatchOperationProceeding()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartIdle();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        try {
          mailbox.StopWaitForMessageArrival();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopIdle();
        }
      });
    }

    [Test, ExpectedException(typeof(/*ImapProtocolViolationException*/InvalidOperationException))]
    public void TestWatchOperationProceedingRefresh()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartIdle();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        try {
          mailbox.Refresh();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopIdle();
        }
      });
    }

    [Test, ExpectedException(typeof(/*ImapProtocolViolationException*/InvalidOperationException))]
    public void TestWatchOperationProceedingClose()
    {
      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Idle},
                                  "INBOX",
                                  "$tag OK [READ-WRITE] done\r\n",
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        try {
          server.EnqueueResponse("+ idling\r\n");

          mailbox.StartIdle();
        }
        catch (Exception ex) {
          Assert.Fail("unexpected exception thrown: {0}", ex);
        }

        Thread.Sleep(100); // XXX: wait for idle started

        try {
          mailbox.Close();

          Assert.Fail("no exception thrown");
        }
        finally {
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          mailbox.StopIdle();
        }
      });
    }
  }
}

