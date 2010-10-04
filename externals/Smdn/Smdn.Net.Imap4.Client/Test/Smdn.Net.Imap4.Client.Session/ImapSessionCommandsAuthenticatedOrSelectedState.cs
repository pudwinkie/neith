using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsAuthenticatedOrSelectedStateTests : ImapSessionTestsBase {
    [Test]
    public void TestIdleBeginIdleEndIdle()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        session.SendTimeout         = 100;
        session.ReceiveTimeout      = 100;
        session.TransactionTimeout  = 100;

        // IDLE
        server.EnqueueResponse("+ idling\r\n");

        var asyncResult = session.BeginIdle();

        Assert.IsNotNull(asyncResult);
        Assert.IsTrue(session.IsIdling);
        Assert.IsTrue(session.IsTransactionProceeding);
        Assert.AreEqual(100, session.SendTimeout);
        Assert.AreNotEqual(100, session.ReceiveTimeout);
        Assert.AreEqual(100, session.TransactionTimeout);

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());

        Thread.Sleep(500);

        // DONE
        server.EnqueueResponse("0004 OK done.\r\n");

        Assert.IsTrue((bool)session.EndIdle(asyncResult));

        Assert.AreEqual("DONE\r\n",
                                   server.DequeueRequest());

        Assert.IsFalse(session.IsIdling);
        Assert.IsFalse(session.IsTransactionProceeding);
        Assert.AreEqual(100, session.SendTimeout);
        Assert.AreEqual(100, session.ReceiveTimeout);
        Assert.AreEqual(100, session.TransactionTimeout);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestIdleBeginIdleNo()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        // IDLE
        server.EnqueueResponse("0004 NO not allowed at this time.\r\n");

        Assert.IsFalse((bool)session.EndIdle(session.BeginIdle()));

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.IsIdling);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestIdleBeginIdleNestedCall()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        // IDLE
        server.EnqueueResponse("+ idling\r\n");
        server.EnqueueResponse("0004 OK done.\r\n");

        var asyncResult = session.BeginIdle();

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());

        try {
          session.BeginIdle();
          Assert.Fail("ImapProtocolViolationException not thrown");
        }
        catch (ImapProtocolViolationException) {
        }

        session.EndIdle(asyncResult);

        Assert.AreEqual("DONE\r\n",
                                   server.DequeueRequest());

        CloseMailbox(session);
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestIdleBeginIdleIncapable()
    {
      using (var session = SelectMailbox()) {
        Assert.IsFalse(session.IsIdling);

        session.HandlesIncapableAsException = true;

        session.BeginIdle();
      }
    }

    [Test]
    public void TestIdleEndIdleInvalidAsyncResult()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        try {
          session.EndIdle(null);
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }

        CloseMailbox(session, "0004");
      }
    }

    [Test]
    public void TestIdleStatusUpdate()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        session.SendTimeout         = 50;
        session.ReceiveTimeout      = 50;
        session.TransactionTimeout  = 50;

        // IDLE
        server.EnqueueResponse("* 2 EXPUNGE\r\n" +
                               "+ idling\r\n" +
                               "* 3 EXISTS\r\n");

        var asyncResult = session.BeginIdle();

        Assert.IsNotNull(asyncResult);
        Assert.IsTrue(session.IsIdling);

        Assert.AreEqual("0004 IDLE\r\n",
                                   server.DequeueRequest());

        Thread.Sleep(500);

        Assert.IsFalse(3 == session.SelectedMailbox.ExistsMessage);

        server.EnqueueResponse("* 4 EXISTS\r\n" +
                               "0004 OK IDLE terminated\r\n");

        Assert.IsTrue((bool)session.EndIdle(asyncResult));

        Assert.AreEqual("DONE\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.IsIdling);
        Assert.AreEqual(4, session.SelectedMailbox.ExistsMessage);

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestIdle()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        // IDLE
        server.EnqueueResponse("+ idling\r\n");
        server.EnqueueResponse("0004 OK done.\r\n");

        var sw = new Stopwatch();

        sw.Reset();
        sw.Start();

        Assert.IsTrue((bool)session.Idle(500));

        sw.Stop();

        Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 500);
        Assert.IsFalse(session.IsIdling);
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("DONE\r\n",
                        server.DequeueRequest());

        CloseMailbox(session);
      }
    }

    [Test]
    public void TestIdleNo()
    {
      using (var session = SelectMailbox("IDLE")) {
        Assert.IsFalse(session.IsIdling);

        // IDLE
        server.EnqueueResponse("0004 NO not allowed at this time.\r\n");

        var sw = new Stopwatch();

        sw.Reset();
        sw.Start();

        Assert.IsFalse((bool)session.Idle(500));

        sw.Stop();

        Assert.Less(sw.ElapsedMilliseconds, 500);
        Assert.IsFalse(session.IsIdling);
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());

        CloseMailbox(session);
      }
    }

    private class IdleState {
      public ImapSession Session;
      public long CurrentMessageCount;
    }

    [Test]
    public void TestIdleKeepIdleCallback()
    {
      using (var session = Authenticate("IDLE")) {
        // SELECT
        server.EnqueueResponse("* FLAGS (\\Deleted \\Seen)\r\n" +
                               "* 4 EXISTS\r\n" +
                               "* 0 RECENT\r\n" +
                               "* OK [UIDVALIDITY 1]\r\n" +
                               "0002 OK SELECT completed\r\n");

        Assert.IsTrue((bool)session.Select("INBOX"));

        Assert.AreEqual("0002 SELECT \"INBOX\"\r\n",
                                   server.DequeueRequest());

        Assert.AreEqual(4, session.SelectedMailbox.ExistsMessage);

        // IDLE
        Assert.IsFalse(session.IsIdling);

        server.WaitForRequest = false;

        session.SendTimeout         = 50;
        session.ReceiveTimeout      = 50;
        session.TransactionTimeout  = 50;

        var waitForIdleFinishEvent = new ManualResetEvent(false);

        ThreadPool.QueueUserWorkItem(delegate(object state) {
          var s = state as ImapPseudoServer;

          s.EnqueueResponse("* 2 EXPUNGE\r\n" +
                            "* 3 EXISTS\r\n" +
                            "+ idling\r\n");

          Thread.Sleep(250);

          // ...time passes; another client expunges message 3...
          s.EnqueueResponse("* 3 EXPUNGE\r\n" +
                            "* 2 EXISTS\r\n");

          Thread.Sleep(250);

          // ...time passes; new mail arrives...
          s.EnqueueResponse("* 3 EXISTS\r\n");

          waitForIdleFinishEvent.WaitOne();

          Thread.Sleep(500);

          s.EnqueueResponse("0003 OK IDLE terminated\r\n");
        }, server);

        var idleState = new IdleState();

        idleState.Session = session;
        idleState.CurrentMessageCount = session.SelectedMailbox.ExistsMessage;

        Assert.IsTrue((bool)session.Idle(1000, idleState, delegate(object state, ImapUpdatedStatus updatedStatus) {
          var s = state as IdleState;

          if (updatedStatus.Expunge.HasValue) {
            s.CurrentMessageCount--;
          }
          else if (updatedStatus.Exists.HasValue) {
            if (s.CurrentMessageCount < updatedStatus.Exists.Value) {
              waitForIdleFinishEvent.Set();
              return false;
            }
            else {
              s.CurrentMessageCount = updatedStatus.Exists.Value;
            }
          }

          return true;
        }));

        waitForIdleFinishEvent.Close();

        Assert.AreEqual("0003 IDLE\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("DONE\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3L, session.SelectedMailbox.ExistsMessage);

        CloseMailbox(session, "0004");
      }
    }

    [Test]
    public void TestIdleKeepIdleCallbackTimeout()
    {
      using (var session = SelectMailbox("IDLE")) {
        // IDLE
        Assert.IsFalse(session.IsIdling);

        session.SendTimeout         = 50;
        session.ReceiveTimeout      = 50;
        session.TransactionTimeout  = 50;

        server.WaitForRequest = false;

        ThreadPool.QueueUserWorkItem(delegate(object state) {
          var s = state as ImapPseudoServer;

          s.EnqueueResponse("+ idling\r\n");

          Thread.Sleep(250);

          s.EnqueueResponse("* 3 EXPUNGE\r\n" +
                            "* 2 EXISTS\r\n");

          Thread.Sleep(250);

          s.EnqueueResponse("0004 OK done.\r\n");
        }, server);

        var sw = new Stopwatch();

        sw.Reset();
        sw.Start();

        Assert.IsTrue((bool)session.Idle(500, null, delegate {
          return true;
        }));

        sw.Stop();

        Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 500);

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("DONE\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.IsIdling);
        Assert.IsFalse(session.IsTransactionProceeding);

        // can't continue
        //CloseMailbox(session);
      }
    }

    [Test]
    public void TestIdleKeepIdleCallbackException()
    {
      using (var session = SelectMailbox("IDLE")) {
        // IDLE
        Assert.IsFalse(session.IsIdling);

        session.SendTimeout         = 50;
        session.ReceiveTimeout      = 50;
        session.TransactionTimeout  = 50;

        server.WaitForRequest = false;

        ThreadPool.QueueUserWorkItem(delegate(object state) {
          var s = state as ImapPseudoServer;

          s.EnqueueResponse("+ idling\r\n");

          Thread.Sleep(250);

          s.EnqueueResponse("* 3 EXPUNGE\r\n" +
                            "* 2 EXISTS\r\n");
        }, server);

        try {
          session.Idle(1000, null, delegate {
            throw new NullReferenceException();
          });

          Assert.Fail("ImapException not thrown");
        }
        catch (ImapException ex) {
          Assert.IsInstanceOfType(typeof(NullReferenceException), ex.InnerException);
        }

        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        //Assert.IsFalse(session.IsIdling); throws NullReferenceException
        Assert.IsFalse(session.IsTransactionProceeding);

        Assert.AreEqual("0004 IDLE\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestNamespace()
    {
      using (var session = Authenticate("NAMESPACE")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Namespace));
  
        // NAMESPACE transaction
        server.EnqueueResponse("* NAMESPACE ((\"\" \"/\")) ((\"~\" \"/\")) ((\"#shared/\" \"/\")(\"#public/\" \"/\")(\"#ftp/\" \"/\")(\"#news.\" \".\"))\r\n" +
                               "0002 OK NAMESPACE command completed\r\n");
  
        ImapNamespace namespaces = null;
  
        Assert.IsTrue((bool)session.Namespace(out namespaces));
  
        Assert.AreEqual("0002 NAMESPACE\r\n",
                        server.DequeueRequest());
  
        Assert.IsNotNull(namespaces);
        Assert.AreEqual(1, namespaces.PersonalNamespaces.Length);
        Assert.AreEqual(string.Empty, namespaces.PersonalNamespaces[0].Prefix);
        Assert.AreEqual("/", namespaces.PersonalNamespaces[0].HierarchyDelimiter);
        Assert.AreEqual(0, namespaces.PersonalNamespaces[0].Extensions.Count);
  
        Assert.AreEqual(1, namespaces.OtherUsersNamespaces.Length);
        Assert.AreEqual("~", namespaces.OtherUsersNamespaces[0].Prefix);
        Assert.AreEqual("/", namespaces.OtherUsersNamespaces[0].HierarchyDelimiter);
        Assert.AreEqual(0, namespaces.OtherUsersNamespaces[0].Extensions.Count);
  
        Assert.AreEqual(4, namespaces.SharedNamespaces.Length);
        Assert.AreEqual("#shared/", namespaces.SharedNamespaces[0].Prefix);
        Assert.AreEqual("#public/", namespaces.SharedNamespaces[1].Prefix);
        Assert.AreEqual("#ftp/", namespaces.SharedNamespaces[2].Prefix);
        Assert.AreEqual("#news.", namespaces.SharedNamespaces[3].Prefix);
  
        Assert.IsTrue(namespaces.PersonalNamespaces.Length == session.Namespaces.PersonalNamespaces.Length);
        Assert.IsTrue(namespaces.OtherUsersNamespaces.Length == session.Namespaces.OtherUsersNamespaces.Length);
        Assert.IsTrue(namespaces.SharedNamespaces.Length == session.Namespaces.SharedNamespaces.Length);
        Assert.IsTrue(namespaces.PersonalNamespaces[0].Prefix == session.Namespaces.PersonalNamespaces[0].Prefix);
        Assert.IsTrue(namespaces.OtherUsersNamespaces[0].Prefix == session.Namespaces.OtherUsersNamespaces[0].Prefix);
        Assert.IsTrue(namespaces.SharedNamespaces[0].Prefix == session.Namespaces.SharedNamespaces[0].Prefix);
        Assert.IsTrue(namespaces.SharedNamespaces[1].Prefix == session.Namespaces.SharedNamespaces[1].Prefix);
        Assert.IsTrue(namespaces.SharedNamespaces[2].Prefix == session.Namespaces.SharedNamespaces[2].Prefix);
        Assert.IsTrue(namespaces.SharedNamespaces[3].Prefix == session.Namespaces.SharedNamespaces[3].Prefix);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestComparatorIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.I18NLevel2));

        session.HandlesIncapableAsException = true;

        session.Comparator();
      }
    }

    [Test]
    public void TestComparator()
    {
      using (var session = Authenticate("I18NLEVEL=2")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.I18NLevel2));
        Assert.AreEqual(ImapCollationAlgorithm.Default, session.ActiveComparator);

        // COMPARATOR transaction
        server.EnqueueResponse("* COMPARATOR i;unicode-casemap\r\n" +
                               "0002 OK Will use i;unicode-casemap for collation\r\n");

        ImapCollationAlgorithm activeComparator;

        Assert.IsTrue((bool)session.Comparator(out activeComparator));

        Assert.AreEqual("0002 COMPARATOR\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapCollationAlgorithm.UnicodeCasemap, session.ActiveComparator);
      }
    }

    [Test]
    public void TestComparatorChangeComparator()
    {
      using (var session = Authenticate("I18NLEVEL=2")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.I18NLevel2));
        Assert.AreEqual(ImapCollationAlgorithm.Default, session.ActiveComparator);

        // COMPARATOR transaction
        server.EnqueueResponse("* COMPARATOR i;basic\r\n" +
                               "0002 OK Will use i;basic for collation\r\n");

        ImapCollationAlgorithm activeComparator;
        ImapCollationAlgorithm[] matchingComparators;

        Assert.IsTrue((bool)session.Comparator(out activeComparator,
                                               out matchingComparators,
                                               new ImapCollationAlgorithm("cz;*"),
                                               new ImapCollationAlgorithm("i;basic")));

        Assert.AreEqual("0002 COMPARATOR \"cz;*\" \"i;basic\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(new ImapCollationAlgorithm("i;basic"), activeComparator);
        Assert.AreEqual(0, matchingComparators.Length);

        Assert.AreEqual(new ImapCollationAlgorithm("i;basic"), session.ActiveComparator);
      }
    }

    [Test]
    public void TestSetQuota()
    {
      using (var session = Authenticate("QUOTA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Quota));

        // SETQUOTA transaction
        server.EnqueueResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                               "0002 OK Setquota completed\r\n");

        ImapQuota changedQuota;

        Assert.IsTrue((bool)session.SetQuota(string.Empty, "STORAGE", 512L, out changedQuota));

        Assert.AreEqual("0002 SETQUOTA \"\" (STORAGE 512)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(changedQuota);
        Assert.AreEqual(string.Empty, changedQuota.Root);
        Assert.AreEqual(1, changedQuota.Resources.Length);
        Assert.AreEqual("STORAGE", changedQuota.Resources[0].Name);
        Assert.AreEqual(10L, changedQuota.Resources[0].Usage);
        Assert.AreEqual(512L, changedQuota.Resources[0].Limit);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSetQuotaIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Quota));

        session.HandlesIncapableAsException = true;

        session.SetQuota(string.Empty, "STORAGE", 512L);
      }
    }

    [Test]
    public void TestGetQuota()
    {
      using (var session = Authenticate("QUOTA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Quota));

        // SETQUOTA transaction
        server.EnqueueResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                               "0002 OK Getquota completed\r\n");

        ImapQuota quota;

        Assert.IsTrue((bool)session.GetQuota(string.Empty, out quota));

        Assert.AreEqual("0002 GETQUOTA \"\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(quota);
        Assert.AreEqual(string.Empty, quota.Root);
        Assert.AreEqual(1, quota.Resources.Length);
        Assert.AreEqual("STORAGE", quota.Resources[0].Name);
        Assert.AreEqual(10L, quota.Resources[0].Usage);
        Assert.AreEqual(512L, quota.Resources[0].Limit);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGetQuotaIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Quota));

        session.HandlesIncapableAsException = true;

        ImapQuota quota;

        session.GetQuota(string.Empty, out quota);
      }
    }

    [Test]
    public void TestGetQuotaRoot()
    {
      using (var session = Authenticate("QUOTA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Quota));

        // GETQUOTAROOT transaction
        server.EnqueueResponse("* QUOTAROOT INBOX \"\"\r\n" +
                               "* QUOTA \"\" (STORAGE 10 512)\r\n" +
                               "0002 OK Getquota completed\r\n");

        IDictionary<string, ImapQuota[]> quotaRoots;

        Assert.IsTrue((bool)session.GetQuotaRoot("INBOX", out quotaRoots));

        Assert.AreEqual("0002 GETQUOTAROOT \"INBOX\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(quotaRoots);
        Assert.IsTrue(quotaRoots.IsReadOnly, "returns read-only dictionary");
        Assert.AreEqual(1, quotaRoots.Count);
        Assert.IsTrue(quotaRoots.ContainsKey("INBOX"));

        var inboxQuotaRoot = quotaRoots["INBOX"];

        Assert.IsNotNull(inboxQuotaRoot);
        Assert.AreEqual(1, inboxQuotaRoot.Length);
        Assert.AreEqual(string.Empty, inboxQuotaRoot[0].Root);
        Assert.AreEqual(1, inboxQuotaRoot[0].Resources.Length);
        Assert.AreEqual("STORAGE", inboxQuotaRoot[0].Resources[0].Name);
        Assert.AreEqual(10L, inboxQuotaRoot[0].Resources[0].Usage);
        Assert.AreEqual(512L, inboxQuotaRoot[0].Resources[0].Limit);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGetQuotaRootIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Quota));

        session.HandlesIncapableAsException = true;

        IDictionary<string, ImapQuota[]> quotaRoots;

        session.GetQuotaRoot("INBOX", out quotaRoots);
      }
    }

    [Test]
    public void TestGetMetadataServer()
    {
      using (var session = Authenticate("METADATA-SERVER")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"\" (/shared/comment \"Shared comment\")\r\n" +
                               "0002 OK GETMETADATA complete\r\n");

        ImapMetadata[] metadata;

        Assert.IsTrue((bool)session.GetMetadata("/shared/comment", out metadata));

        Assert.AreEqual("0002 GETMETADATA \"\" /shared/comment\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(metadata);
        Assert.AreEqual(1, metadata.Length);
        Assert.AreEqual("/shared/comment", metadata[0].EntryName);
        Assert.AreEqual("Shared comment", (string)metadata[0].Value);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGetMetadataServerIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        ImapMetadata[] metadata;

        session.GetMetadata("/shared/comment", out metadata);
      }
    }

    [Test]
    public void TestGetMetadataServerCapable1()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        ImapMetadata[] metadata;

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"\" (/shared/comment \"Shared comment\")\r\n" +
                               "0002 OK GETMETADATA complete\r\n");

        Assert.IsTrue((bool)session.GetMetadata("/shared/comment", out metadata));
      }
    }

    [Test]
    public void TestGetMetadataServerCapable2()
    {
      using (var session = Authenticate("METADATA-SERVER")) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        ImapMetadata[] metadata;

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"\" (/shared/comment \"Shared comment\")\r\n" +
                               "0002 OK GETMETADATA complete\r\n");

        Assert.IsTrue((bool)session.GetMetadata("/shared/comment", out metadata));
      }
    }

    [Test]
    public void TestGetMetadata1()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"INBOX\" (/private/comment \"My own comment\")\r\n" +
                               "0002 OK GETMETADATA complete\r\n");

        ImapMetadata[] metadata;

        Assert.IsTrue((bool)session.GetMetadata("INBOX", "/private/comment", out metadata));

        Assert.AreEqual("0002 GETMETADATA \"INBOX\" /private/comment\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(metadata);
        Assert.AreEqual(1, metadata.Length);
        Assert.AreEqual("/private/comment", metadata[0].EntryName);
        Assert.AreEqual("My own comment", (string)metadata[0].Value);
      }
    }

    [Test]
    public void TestGetMetadata2()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"INBOX\" (/shared/comment \"Shared comment\" " +
                               "/private/comment \"My own comment\")\r\n"+
                               "0002 OK GETMETADATA complete\r\n");

        ImapMetadata[] metadata;

        Assert.IsTrue((bool)session.GetMetadata("INBOX", new[] {"/shared/comment", "/private/comment"}, out metadata));

        Assert.AreEqual("0002 GETMETADATA \"INBOX\" (/shared/comment /private/comment)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(metadata);
        Assert.AreEqual(2, metadata.Length);
        Assert.AreEqual("/shared/comment", metadata[0].EntryName);
        Assert.AreEqual("Shared comment", (string)metadata[0].Value);
        Assert.AreEqual("/private/comment", metadata[1].EntryName);
        Assert.AreEqual("My own comment", (string)metadata[1].Value);
      }
    }

    [Test]
    public void TestGetMetadataMaxSize()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"INBOX\" (/private/comment \"My own comment\")\r\n"+
                               "0002 OK [METADATA LONGENTRIES 2199] GETMETADATA complete\r\n");

        ImapMetadata[] metadata;

        var result = session.GetMetadata("INBOX",
                                         new[] {"/shared/comment", "/private/comment"},
                                         ImapGetMetadataOptions.MaxSize(1024L),
                                         out metadata);

        Assert.IsTrue((bool)result);

        Assert.AreEqual("0002 GETMETADATA \"INBOX\" (MAXSIZE 1024) (/shared/comment /private/comment)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(metadata);
        Assert.AreEqual(1, metadata.Length);
        Assert.AreEqual("/private/comment", metadata[0].EntryName);
        Assert.AreEqual("My own comment", (string)metadata[0].Value);

        var resp = result.GetResponseCode(ImapResponseCode.MetadataLongEntries);

        Assert.IsNotNull(resp);
        Assert.AreEqual(2199, ImapResponseTextConverter.FromMetadataLongEntries(resp.ResponseText));
      }
    }

    [Test]
    public void TestGetMetadataDepth()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // GETMETADATA transaction
        server.EnqueueResponse("* METADATA \"INBOX\" (/private/filters/values/small " +
                               "\"SMALLER 5000\" /private/filters/values/boss " +
                               "\"FROM \\\"boss@example.com\\\"\")\r\n" +
                               "0002 OK GETMETADATA complete\r\n");

        ImapMetadata[] metadata;

        Assert.IsTrue((bool)session.GetMetadata("INBOX",
                                                "/private/filters/values",
                                                ImapGetMetadataOptions.Depth1,
                                                out metadata));

        Assert.AreEqual("0002 GETMETADATA \"INBOX\" (DEPTH 1) /private/filters/values\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(metadata);
        Assert.AreEqual(2, metadata.Length);
        Assert.AreEqual("/private/filters/values/small", metadata[0].EntryName);
        Assert.AreEqual("SMALLER 5000", (string)metadata[0].Value);
        Assert.AreEqual("/private/filters/values/boss", metadata[1].EntryName);
        Assert.AreEqual("FROM \"boss@example.com\"", (string)metadata[1].Value);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGetMetadataIncapable1()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));

        session.HandlesIncapableAsException = true;

        ImapMetadata[] metadata;

        session.GetMetadata("/shared/comment", out metadata);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGetMetadataIncapable2()
    {
      using (var session = Authenticate("METADATA-SERVER")) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        ImapMetadata[] metadata;

        session.GetMetadata("INBOX", "/shared/comment", out metadata);
      }
    }

    [Test]
    public void TestSetMetadataServer()
    {
      using (var session = Authenticate("METADATA-SERVER")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        // SETMETADATA transaction
        server.EnqueueResponse("0002 OK SETMETADATA complete\r\n");

        Assert.IsTrue((bool)session.SetMetadata(new[] {"/shared/vendor/foo/bar"}));

        Assert.AreEqual("0002 SETMETADATA \"\" (/shared/vendor/foo/bar NIL)\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSetMetadataServerIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        session.SetMetadata(new[] {"/shared/vendor/foo/bar"});
      }
    }

    [Test]
    public void TestSetMetadataServerCapable1()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        // SETMETADATA transaction
        server.EnqueueResponse("0002 OK SETMETADATA complete\r\n");

        Assert.IsTrue((bool)session.SetMetadata(new[] {"/shared/vendor/foo/bar"}));
      }
    }

    [Test]
    public void TestSetMetadataServerCapable2()
    {
      using (var session = Authenticate("METADATA-SERVER")) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        // SETMETADATA transaction
        server.EnqueueResponse("0002 OK SETMETADATA complete\r\n");

        Assert.IsTrue((bool)session.SetMetadata(new[] {"/shared/vendor/foo/bar"}));
      }
    }

    [Test]
    public void TestSetMetadata1()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // SETMETADATA transaction
        server.EnqueueResponse("+ ready for data\r\n");
        server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0002 OK SETMETADATA complete\r\n");

        var metadata = new ImapMetadata("/private/comment",
                                        new ImapLiteralString("My new comment across\n" +
                                                              "two lines.\n"));

        Assert.IsTrue((bool)session.SetMetadata("INBOX", metadata));

        Assert.AreEqual("0002 SETMETADATA \"INBOX\" (/private/comment {33}\r\n" +
                        "My new comment across\n" +
                        "two lines.\n" +
                        ")\r\n",
                        server.DequeueAll());
      }
    }

    [Test]
    public void TestSetMetadata2()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // SETMETADATA transaction
        server.EnqueueResponse("0002 OK SETMETADATA complete\r\n");

        Assert.IsTrue((bool)session.SetMetadata("INBOX", "/private/comment"));

        Assert.AreEqual("0002 SETMETADATA \"INBOX\" (/private/comment NIL)\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestSetMetadata3()
    {
      using (var session = Authenticate("METADATA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Metadata));

        // SETMETADATA transaction
        server.EnqueueResponse("0002 OK SETMETADATA complete\r\n");

        var metadata = new[] {
          new ImapMetadata("/private/comment", new ImapQuotedString("My new comment")),
          new ImapMetadata("/shared/comment", new ImapQuotedString("This one is for you!")),
        };

        Assert.IsTrue((bool)session.SetMetadata("INBOX", metadata));

        Assert.AreEqual("0002 SETMETADATA \"INBOX\" (/private/comment \"My new comment\" " +
                        "/shared/comment \"This one is for you!\")\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSetMetadataIncapable1()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));

        session.HandlesIncapableAsException = true;

        session.SetMetadata("INBOX", "/private/comment");
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSetMetadataIncapable2()
    {
      using (var session = Authenticate("METADATA-SERVER")) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Metadata));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.MetadataServer));

        session.HandlesIncapableAsException = true;

        session.SetMetadata("INBOX", "/private/comment");
      }
    }
  }
}
