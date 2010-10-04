using System;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionTests : ImapSessionTestsBase {
    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyServerCapabilities()
    {
      using (var session = Connect()) {
        session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyHierarchyDelimiters()
    {
      using (var session = Connect()) {
        session.HierarchyDelimiters.Add("#refname", ".");
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyServerID()
    {
      using (var session = Connect()) {
        session.ServerID.Add("key", "value");
      }
    }

    private void Idle(Action<ImapSession> preprocessSession, Action<ImapSession> processSession)
    {
      using (var session = Authenticate("IDLE")) {
        preprocessSession(session);

        server.EnqueueResponse("+ idling\r\n");

        var asyncResult = session.BeginIdle();

        Assert.IsTrue(session.IsIdling);
        Assert.IsTrue(session.IsTransactionProceeding);

        processSession(session);

        server.EnqueueResponse("0002 OK done\r\n");

        Assert.IsTrue((bool)session.EndIdle(asyncResult));

        Assert.IsFalse(session.IsIdling);
        Assert.IsFalse(session.IsTransactionProceeding);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetTransactionTimeoutWhileTransactionProceeding()
    {
      Idle(delegate(ImapSession session) {
        session.TransactionTimeout = 1000;
        Assert.AreEqual(1000, session.TransactionTimeout);
      },
      delegate(ImapSession session) {
        session.TransactionTimeout = 1000;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetSendTimeoutWhileTransactionProceeding()
    {
      Idle(delegate(ImapSession session) {
        session.SendTimeout = 1000;
        Assert.AreEqual(1000, session.SendTimeout);
      },
      delegate(ImapSession session) {
        session.SendTimeout = 1000;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetReceiveTimeoutWhileTransactionProceeding()
    {
      Idle(delegate(ImapSession session) {
        session.ReceiveTimeout = 1000;
        Assert.AreEqual(1000, session.ReceiveTimeout);
      },
      delegate(ImapSession session) {
        session.ReceiveTimeout = 1000;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestBeginNewTransactionWhileTransactionProceeding1()
    {
      Idle(delegate(ImapSession session) {
        Assert.IsFalse(session.IsTransactionProceeding);
      },
      delegate(ImapSession session) {
        Assert.IsTrue(session.IsTransactionProceeding);
        session.NoOp();
      });
    }

    [Test]
    public void TestBeginNewTransactionWhileTransactionProceeding2()
    {
      using (var session = Connect()) {
        using (var waitForFinishedEvent = new ManualResetEvent(false)) {
          session.TransactionTimeout = 500;

          Exception unexpectedException = null;

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            try {
              try {
                (state as ImapSession).NoOp();
              }
              catch (TimeoutException) {
                // expected exception
              }
              catch (Exception ex) {
                unexpectedException = ex;
              }
            }
            finally {
              waitForFinishedEvent.Set();
            }
          }, session);

          Thread.Sleep(250);

          Assert.IsTrue(session.IsTransactionProceeding);

          try {
            session.NoOp();
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }

          waitForFinishedEvent.WaitOne();

          if (unexpectedException != null)
            Assert.Fail("unexpected exception {0}", unexpectedException);
        }
      }
    }

    [Test]
    public void TestPreProcessTransactionSetLiteralOptions()
    {
      using (var session = Connect()) {
        session.HandlesIncapableAsException = false;

        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.LiteralNonSync));
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Binary));

        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0000 OK done\r\n");

        var arguments = new ImapString[] {
          new ImapLiteralString("literal1", Encoding.ASCII, ImapLiteralOptions.Synchronizing),
          new ImapLiteralString("literal2", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing),
          new ImapLiteralString("literal3", Encoding.ASCII, ImapLiteralOptions.NonSynchronizingIfCapable),
          new ImapLiteralString("literal4", Encoding.ASCII, ImapLiteralOptions.Literal),
          new ImapLiteralString("literal5", Encoding.ASCII, ImapLiteralOptions.Literal8),
          new ImapLiteralString("literal6", Encoding.ASCII, ImapLiteralOptions.Literal8IfCapable),
          new ImapLiteralString("literal7", Encoding.ASCII, ImapLiteralOptions.Synchronizing | ImapLiteralOptions.Literal8IfCapable),
          new ImapLiteralString("literal8", Encoding.ASCII, ImapLiteralOptions.NonSynchronizingIfCapable | ImapLiteralOptions.Literal8),
          new ImapParenthesizedString(new ImapLiteralString("nested1", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing),
                                      new ImapLiteralString("nested2", Encoding.ASCII, ImapLiteralOptions.NonSynchronizingIfCapable),
                                      new ImapLiteralString("nested3", Encoding.ASCII, ImapLiteralOptions.Literal8),
                                      new ImapLiteralString("nested4", Encoding.ASCII, ImapLiteralOptions.Literal8IfCapable)),
        };

        session.GenericCommand("X-TEST", arguments);

        Assert.AreEqual("0000 X-TEST " +
                        "{8}\r\nliteral1 " +
                        "{8+}\r\nliteral2 " +
                        "{8}\r\nliteral3 " +
                        "{8}\r\nliteral4 " +
                        "~{8}\r\nliteral5 " +
                        "{8}\r\nliteral6 " +
                        "{8}\r\nliteral7 " +
                        "~{8}\r\nliteral8 " +
                        "({7+}\r\nnested1 " +
                        "{7}\r\nnested2 " +
                        "~{7}\r\nnested3 " +
                        "{7}\r\nnested4)\r\n",
                        server.DequeueAll());
      }
    }

    [Test]
    public void TestPreProcessTransactionSetLiteralOptionsLiteralNonSyncCapable()
    {
      using (var session = Connect("LITERAL+")) {
        session.HandlesIncapableAsException = true;

        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.LiteralNonSync));

        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0000 OK done\r\n");

        var arguments = new ImapParenthesizedString(new ImapString[] {
          new ImapLiteralString("literal1", Encoding.ASCII, ImapLiteralOptions.Synchronizing),
          new ImapLiteralString("literal2", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing),
          new ImapLiteralString("literal3", Encoding.ASCII, ImapLiteralOptions.NonSynchronizingIfCapable),
        });

        session.GenericCommand("X-TEST", arguments);

        Assert.AreEqual("0000 X-TEST (" +
                        "{8}\r\nliteral1 " +
                        "{8+}\r\nliteral2 " +
                        "{8+}\r\nliteral3)\r\n",
                        server.DequeueAll());
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestPreProcessTransactionSetLiteralOptionsLiteralNonSyncIncapable()
    {
      using (var session = Connect()) {
        session.HandlesIncapableAsException = true;

        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.LiteralNonSync));

        session.GenericCommand("X-TEST", new ImapLiteralString("literal", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing));
      }
    }

    [Test]
    public void TestPreProcessTransactionSetLiteralOptionsLiteral8Capable()
    {
      using (var session = Connect("BINARY")) {
        session.HandlesIncapableAsException = true;

        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Binary));

        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("+ continue\r\n"); server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0000 OK done\r\n");

        var arguments = new ImapParenthesizedString(new ImapString[] {
          new ImapLiteralString("literal1", Encoding.ASCII, ImapLiteralOptions.Literal),
          new ImapLiteralString("literal2", Encoding.ASCII, ImapLiteralOptions.Literal8),
          new ImapLiteralString("literal3", Encoding.ASCII, ImapLiteralOptions.Literal8IfCapable),
        });

        session.GenericCommand("X-TEST", arguments);

        Assert.AreEqual("0000 X-TEST (" +
                        "{8}\r\nliteral1 " +
                        "~{8}\r\nliteral2 " +
                        "~{8}\r\nliteral3)\r\n",
                        server.DequeueAll());
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestPreProcessTransactionSetLiteralOptionsLiteral8Incapable()
    {
      using (var session = Connect()) {
        session.HandlesIncapableAsException = true;

        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Binary));

        session.GenericCommand("X-TEST", new ImapLiteralString("literal", Encoding.ASCII, ImapLiteralOptions.Literal8));
      }
    }

    [Test]
    public void TestConnect()
    {
      using (var session = Connect()) {
        Assert.IsFalse(session.IsIdling);
        Assert.IsNull(session.SelectedMailbox);
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
        Assert.AreEqual(new Uri(string.Format("imap://{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(0, session.ServerCapabilities.Count);
      }
    }

    [Test]
    public void TestConnectBye()
    {
      server.EnqueueResponse("* BYE not available.\r\n");

      try {
        using (var session = new ImapSession(host, port)) {
          Assert.Fail("ImapConnectionException not thrown");
        }
      }
      catch (ImapConnectionException ex) {
        Assert.IsNull(ex.InnerException);
        StringAssert.Contains("not available.", ex.Message);
      }
    }

    [Test]
    public void TestConnectInvalidResponse()
    {
      server.EnqueueResponse("+OK pseudo POP server ready.\r\n");

      try {
        using (var session = new ImapSession(host, port)) {
          Assert.Fail("ImapConnectionException not thrown");
        }
      }
      catch (ImapConnectionException ex) {
        Assert.IsNull(ex.InnerException);
        StringAssert.Contains("unexpected data response", ex.Message);
      }
    }

    [Test]
    public void TestConnectTransactionTimeout()
    {
      server.Stop();

      try {
        using (var session = new ImapSession(host, port, 500)) {
          Assert.Fail("ImapConnectionException not thrown");
        }
      }
      catch (ImapConnectionException ex) {
        var timeoutException = ex.InnerException as TimeoutException;

        Assert.IsNotNull(timeoutException);
      }
    }

    [Test]
    public void TestConnectSocketError()
    {
      try {
        using (var session = new ImapSession("imap.invalid", 10143)) {
          Assert.Fail("ImapConnectionException not thrown");
        }
      }
      catch (ImapConnectionException ex) {
        var socketException = ex.InnerException as System.Net.Sockets.SocketException;

        Assert.IsNotNull(socketException);
      }
    }

    [Test]
    public void TestConnectSsl()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      var streamUpgraded = false;

      using (var session = new ImapSession(host, port, delegate(ConnectionBase connection, Stream baseStream) {
        streamUpgraded = true;
        return baseStream; // TODO: return SslStream
      })) {
        Assert.IsFalse(session.IsIdling);
        Assert.IsNull(session.SelectedMailbox);
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
        Assert.AreEqual(new Uri(string.Format("imaps://{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(0, session.ServerCapabilities.Count);
        Assert.IsTrue(streamUpgraded, "stream upgraded");
        Assert.IsTrue(session.IsSecureConnection);
      }
    }

    [Test]
    public void TestConnectSslExceptionWhileUpgrading()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      try {
        using (var session = new ImapSession(host, port, delegate(ConnectionBase connection, Stream baseStream) {
          throw new System.Security.Authentication.AuthenticationException();
        })) {
          Assert.Fail("connected");
        }
      }
      catch (ImapSecureConnectionException ex) {
        var upgradeException = ex.InnerException as ImapUpgradeConnectionException;

        Assert.IsNotNull(upgradeException);
        Assert.IsNotNull(upgradeException.InnerException);
        Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                upgradeException.InnerException);
      }
    }

    [Test]
    public void TestDisconnect()
    {
      var session = Connect();

      session.Disconnect(false);

      Assert.IsTrue(session.IsDisposed);

      try {
        session.NoOp();
        Assert.Fail("ObjectDisposedException not thrown");
      }
      catch (ObjectDisposedException) {
      }
    }

    [Test]
    public void TestImap4Rev1Incapable()
    {
      using (var session = Connect()) {
        server.EnqueueResponse("* CAPABILITY NONE\r\n0000 OK ImapCapability completed\r\n");

        ImapCapabilityList caps;

        Assert.IsTrue((bool)session.Capability(out caps));
        Assert.IsNotNull(caps);
        Assert.IsFalse(caps.Has(ImapCapability.Imap4Rev1));
        Assert.AreEqual("0000 CAPABILITY\r\n",
                        server.DequeueRequest());

        try {
          session.HandlesIncapableAsException = true;
          session.NoOp();
          Assert.Fail("exception not occured");
        }
        catch (ImapIncapableException) {
        }
      }
    }

    [Test]
    public void TestGreetingWithCapability()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=CRAM-MD5 AUTH=PLAIN CHILDREN THREAD=REFERENCES X-EXTENSION1 X-EXTENSION2] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
        Assert.AreEqual(7, session.ServerCapabilities.Count);

        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Children));
        Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.CRAMMD5));
        Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.Plain));

        Assert.IsTrue(session.ServerCapabilities.Has("THREAD=REFERENCES"));
        Assert.IsTrue(session.ServerCapabilities.Has("X-EXTENSION1"));
        Assert.IsTrue(session.ServerCapabilities.Has("X-EXTENSION2"));

        try {
          session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      }
    }

    [Test]
    public void TestGreetingWithNoCapability()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
        Assert.AreEqual(0, session.ServerCapabilities.Count);

        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));

        try {
          session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      }
    }

    [Test]
    public void TestGreetingPreAuthWithCapability()
    {
      server.EnqueueResponse("* PREAUTH [CAPABILITY IMAP4rev1 AUTH=PLAIN] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.AreEqual(2, session.ServerCapabilities.Count);
        Assert.AreEqual(new Uri(string.Format("imap://{0}:{1}/", host, port)), session.Authority);

        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
        Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.Plain));

        try {
          session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGreetingOkWithNoImapCapability()
    {
      server.EnqueueResponse("* OK [CAPABILITY AUTH=PLAIN] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.IsTrue(!session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));

        session.HandlesIncapableAsException = true;

        session.Login(credential);
      }
    }

    [Test]
    public void TestGreetingHomeServerReferralAsException()
    {
      server.EnqueueResponse("* BYE [REFERRAL IMAP://user;AUTH=*@SERVER2/] Server not accepting connections.  Try SERVER2\r\n");

      try {
        new ImapSession(host, port, true);

        Assert.Fail("connected without exception");
      }
      catch (ImapLoginReferralException ex) {
        Assert.IsTrue(ex.Message.Contains("Server not accepting connections.  Try SERVER2"));
        Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER2/"), ex.ReferToUri);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapConnectionException))]
    public void TestGreetingHomeServerReferralAsError()
    {
      server.EnqueueResponse("* BYE [REFERRAL IMAP://user;AUTH=*@SERVER2/] Server not accepting connections.  Try SERVER2\r\n");

      using (var session = new ImapSession(host, port)) {
      }
    }

    [Test]
    public void TestTransactionTimeoutWhileTransactionProceeding()
    {
      using (var session = Connect()) {
        session.TransactionTimeout = 250;
        session.SendTimeout = System.Threading.Timeout.Infinite;
        session.ReceiveTimeout = System.Threading.Timeout.Infinite;

        // not respond NOOP response termination(CRLF)
        server.EnqueueResponse("0000 OK");

        try {
          session.NoOp();
          Assert.Fail("TimeoutException not thrown");
        }
        catch (TimeoutException) {
          Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        }

        try {
          session.NoOp();
          Assert.Fail("ImapProtocolViolationException not thrown");
        }
        catch (ImapProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestSocketTimeoutWhileTransactionProceeding()
    {
      using (var session = Connect()) {
        session.TransactionTimeout = System.Threading.Timeout.Infinite;
        session.SendTimeout = 250;
        session.ReceiveTimeout = 250;

        // not respond NOOP
        //server.EnqueueResponse("0000 OK\r\n");

        try {
          session.NoOp();
          Assert.Fail("TimeoutException not thrown");
        }
        catch (TimeoutException) {
          Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        }

        try {
          session.NoOp();
          Assert.Fail("ImapProtocolViolationException not thrown");
        }
        catch (ImapProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestInternalErrorWhileTransactionProceeding()
    {
      using (var session = Connect()) {
        server.EnqueueResponse("*\r\n" + // invalid response
                               "0000 CAPABILITY Ok\r\n");

        try {
          session.Capability();
          Assert.Fail("ImapException not thrown");
        }
        catch (ImapException ex) {
          Assert.IsNotNull(ex.InnerException);
          Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        }

        try {
          session.NoOp();
          Assert.Fail("ImapProtocolViolationException not thrown");
        }
        catch (ImapProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestDiconnectedFromServerWithByeResponse()
    {
      using (var session = Connect()) {
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);

        server.EnqueueResponse("* BYE Disconnected for inactivity.\r\n");
        server.Stop(true);

        Assert.IsTrue(session.NoOp().Code == ImapCommandResultCode.Bye);

        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        Assert.AreEqual(null, session.Authority);

        try {
          session.NoOp();
          Assert.Fail("ImapProtocolViolationException not thrown");
        }
        catch (ImapProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestDiconnectedFromServerWithoutByeResponse()
    {
      using (var session = Connect()) {
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);

        server.Stop(true);

        try {
          session.NoOp();
          Assert.Fail("ImapException not thrown");
        }
        catch (ImapConnectionException) {
        }

        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
        Assert.AreEqual(null, session.Authority);

        try {
          session.NoOp();
          Assert.Fail("ImapProtocolViolationException not thrown");
        }
        catch (ImapProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestReceivedCommandTagUnmatched()
    {
      using (var session = Connect()) {
        server.EnqueueResponse("0001 OK authenticated\r\n");

        try {
          session.Login(credential);
          Assert.Fail("ImapException not thrown");
        }
        catch (ImapException ex) {
          Assert.IsNotNull(ex.InnerException);
          Assert.IsInstanceOfType(typeof(ImapMalformedResponseException), ex.InnerException);
        }
      }
    }

    [Test]
    public void TestListStatusSelect()
    {
      // greeting and login transaction
      using (var session = Connect()) {
        server.EnqueueResponse("0000 OK authenticated\r\n");
  
        session.Login(credential);

        Assert.AreEqual(new Uri(string.Format("imap://{0}@{1}:{2}/", username, host, port)), session.Authority);

        server.DequeueRequest();
  
        // CAPABILITY transaction
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK CAPABILITY completed\r\n");
  
        session.Capability();
  
        server.DequeueRequest();
  
        // LIST transaction
        server.EnqueueResponse("* LIST (\\HasChildren) \".\" \"INBOX\"\r\n" +
                               "0002 OK LIST completed\r\n");
  
        ImapMailbox[] mailboxes;
  
        Assert.IsTrue((bool)session.List("*", out mailboxes));
  
        Assert.AreEqual("0002 LIST \"\" \"*\"\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("INBOX", mailboxes[0].Name);
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter);
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.HasChildren));
        Assert.AreEqual(new Uri(uri, "./INBOX"), mailboxes[0].Url);
  
        // LSUB transaction
        server.EnqueueResponse("* LSUB (\\HasChildren) \"/\" \"Trash\"\r\n" +
                               "* LSUB (\\HasNoChildren) \"/\" \"Trash/mail\"\r\n" +
                               "* LSUB (\\HasNoChildren) \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "* LSUB (\\HasNoChildren) \".\" \"INBOX.&U,BTFw-\"\r\n" +
                               "0003 OK LSUB completed\r\n");
  
        Assert.IsTrue((bool)session.Lsub("*", out mailboxes));
  
        Assert.AreEqual("0003 LSUB \"\" \"*\"\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(4, mailboxes.Length);
        Assert.AreEqual("Trash", mailboxes[0].Name);
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.HasChildren));
  
        Assert.AreEqual("Trash/mail", mailboxes[1].Name);
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.HasNoChildren));
        Assert.AreEqual(new Uri(uri, "./Trash/mail"), mailboxes[1].Url);
  
        Assert.AreEqual("INBOX.日本語", mailboxes[2].Name);
        Assert.AreEqual("INBOX.台北", mailboxes[3].Name);
  
        // STATUS transaction
        server.EnqueueResponse("* STATUS \"Trash\" (MESSAGES 3 RECENT 1 UIDVALIDITY 123456 UNSEEN 2 UIDNEXT 4)\r\n" +
                               "0004 OK STATUS completed\r\n");
  
        Assert.IsTrue((bool)session.Status(mailboxes[0], ImapStatusDataItem.StandardAll));
  
        Assert.AreEqual("0004 STATUS \"Trash\" (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(3, mailboxes[0].ExistsMessage);
        Assert.AreEqual(1, mailboxes[0].RecentMessage);
        Assert.AreEqual(123456, mailboxes[0].UidValidity);
        Assert.AreEqual(2, mailboxes[0].UnseenMessage);
        Assert.AreEqual(4, mailboxes[0].UidNext);
  
        // SELECT transaction
        Assert.IsNull(session.SelectedMailbox);
  
        server.EnqueueResponse("* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft custom1 custom2)\r\n" +
                               "* OK [PERMANENTFLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft custom1 \\*)] Flags permitted\r\n" +
                               "* 15 EXISTS\r\n" +
                               "* 2 RECENT\r\n" +
                               "* OK [UIDVALIDITY 1202674433] UIDs valid\r\n" +
                               "* OK [UIDNEXT 16]\r\n" +
                               "* OK [UNSEEN 13]\r\n" +
                               "0005 OK [READ-WRITE] SELECT completed\r\n");
  
        var mailbox = mailboxes[2];
  
        Assert.IsTrue((bool)session.Select(mailbox));
  
        Assert.AreEqual("0005 SELECT \"INBOX.&ZeVnLIqe-\"\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.AreSame(mailbox, session.SelectedMailbox);
  
        Assert.AreEqual("INBOX.日本語", mailbox.Name);
        Assert.AreEqual(15, mailbox.ExistsMessage);
        Assert.AreEqual(2, mailbox.RecentMessage);
        Assert.AreEqual(0, mailbox.UnseenMessage);
        Assert.AreEqual(13, mailbox.FirstUnseen);
        Assert.AreEqual(1202674433, mailbox.UidValidity);
        Assert.AreEqual(16, mailbox.UidNext);
        Assert.IsFalse(mailbox.ReadOnly);
        Assert.IsTrue(mailbox.UidPersistent);
        Assert.AreEqual(new Uri(uri, "./INBOX.日本語;UIDVALIDITY=1202674433"), mailbox.Url);

        /*
        Assert.AreEqual(selectedMailbox.Name, mailbox.Name);
        Assert.AreEqual(selectedMailbox.ExistsMessage, mailbox.ExistsMessage);
        Assert.AreEqual(selectedMailbox.RecentMessage, mailbox.RecentMessage);
        Assert.AreEqual(selectedMailbox.UnseenMessage, mailbox.UnseenMessage);
        Assert.AreEqual(selectedMailbox.FirstUnseen, mailbox.FirstUnseen);
        Assert.AreEqual(selectedMailbox.UidValidity, mailbox.UidValidity);
        Assert.AreEqual(selectedMailbox.UidNext, mailbox.UidNext);
        Assert.AreEqual(selectedMailbox.ReadOnly, mailbox.ReadOnly);
        Assert.AreEqual(selectedMailbox.ApplicableFlags.Count, mailbox.ApplicableFlags.Count);
        Assert.AreEqual(selectedMailbox.PermanentFlags.Count, mailbox.PermanentFlags.Count);
        */

        Assert.AreEqual(7, mailbox.ApplicableFlags.Count);
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Draft));
        Assert.IsTrue(mailbox.ApplicableFlags.Has("custom1"));
        Assert.IsTrue(mailbox.ApplicableFlags.Has("custom2"));
  
        Assert.IsTrue(mailbox.PermanentFlags.Has(ImapMessageFlag.AllowedCreateKeywords));
        Assert.IsTrue(mailbox.PermanentFlags.Has(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.PermanentFlags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.PermanentFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.PermanentFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.PermanentFlags.Has(ImapMessageFlag.Draft));
        Assert.IsTrue(mailbox.PermanentFlags.Has("custom1"));
  
        // CLOSE transaction
        server.EnqueueResponse("0006 OK CLOSE completed\r\n");
  
        Assert.IsTrue((bool)session.Close());
  
        Assert.AreEqual("0006 CLOSE\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      }
    }

    [Test]
    public void TestGenericCommandNoWithArguments()
    {
      using (var session = Authenticate()) {
        // generic command transaction
        server.EnqueueResponse("* X-EXT-COMMAND data\r\n" +
                               "0002 OK X-EXT-COMMAND completed\r\n");
  
        ImapDataResponse[] dataResps;
  
        Assert.IsTrue((bool)session.GenericCommand("X-EXT-COMMAND", out dataResps));
  
        Assert.AreEqual("0002 X-EXT-COMMAND\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(1, dataResps.Length);
        Assert.AreEqual(2, dataResps[0].Data.Length);
        Assert.AreEqual(ImapDataFormat.Text, dataResps[0].Data[0].Format);
        Assert.AreEqual("X-EXT-COMMAND", dataResps[0].Data[0].GetTextAsString());
        Assert.AreEqual(ImapDataFormat.Text, dataResps[0].Data[1].Format);
        Assert.AreEqual("data", dataResps[0].Data[1].GetTextAsString());
      }
    }

    [Test]
    public void TestGenericCommandWithArguments()
    {
      using (var session = Authenticate()) {
        // generic command transaction
        server.EnqueueResponse("+ OK continue\r\n");
        server.EnqueueResponse("0002 OK X-EXT-COMMAND completed\r\n");
  
        ImapDataResponse[] dataResps;
  
        Assert.IsTrue((bool)session.GenericCommand("X-EXT-COMMAND", out dataResps,
                                           new ImapParenthesizedString("arg1",
                                                                       "arg2",
                                                                       new ImapParenthesizedString("arg3-1", "arg3-2"),
                                                                       new ImapLiteralString("ascii text", NetworkTransferEncoding.Transfer8Bit)),
                                           new ImapParenthesizedString(new ImapQuotedString("quoted"),
                                                                       "nonquoted")
                                           ));
  
        Assert.AreEqual("0002 X-EXT-COMMAND (arg1 arg2 (arg3-1 arg3-2) {10}\r\n" +
                        "ascii text) (\"quoted\" nonquoted)\r\n",
                        server.DequeueAll());
  
        Assert.AreEqual(0, dataResps.Length);
      }
    }
  }
}
