using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionTests : ImapSessionTestsBase {
    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyServerCapabilities()
    {
      Connect(delegate(ImapSession session) {
        session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
      });
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyHierarchyDelimiters()
    {
      Connect(delegate(ImapSession session) {
        session.HierarchyDelimiters.Add("#refname", ".");
      });
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyServerID()
    {
      Connect(delegate(ImapSession session) {
        session.ServerID.Add("key", "value");
      });
    }

    [Test]
    public void TestSetReceiveTimeoutZero()
    {
      Connect(delegate(ImapSession session) {
        session.ReceiveTimeout = 0;

        Assert.AreEqual(0, session.ReceiveTimeout);
      });
    }

    [Test]
    public void TestSetReceiveTimeoutInfinite()
    {
      Connect(delegate(ImapSession session) {
        session.ReceiveTimeout = Timeout.Infinite;

        Assert.AreEqual(Timeout.Infinite, session.ReceiveTimeout);
      });
    }

    [Test]
    public void TestSetSendTimeoutZero()
    {
      Connect(delegate(ImapSession session) {
        session.SendTimeout = 0;

        Assert.AreEqual(0, session.SendTimeout);
      });
    }

    [Test]
    public void TestSetSendTimeoutInfinite()
    {
      Connect(delegate(ImapSession session) {
        session.SendTimeout = Timeout.Infinite;

        Assert.AreEqual(Timeout.Infinite, session.SendTimeout);
      });
    }

    [Test]
    public void TestSetTransactionTimeoutZero()
    {
      Connect(delegate(ImapSession session) {
        session.TransactionTimeout = 0;

        Assert.AreEqual(0, session.TransactionTimeout);
      });
    }

    [Test]
    public void TestSetTransactionTimeoutInfinite()
    {
      Connect(delegate(ImapSession session) {
        session.TransactionTimeout = Timeout.Infinite;

        Assert.AreEqual(Timeout.Infinite, session.TransactionTimeout);
      });
    }

    private void Idle(Action<ImapSession> preprocessSession, Action<ImapSession> processSession)
    {
      Authenticate(new[] {"IDLE"}, delegate(ImapSession session, ImapPseudoServer server) {
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
      });
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
      Connect(delegate(ImapSession session) {
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
      });
    }

    [Test]
    public void TestPreProcessTransactionSetLiteralOptions()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = false;

        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.LiteralNonSync));
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Binary));

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
      });
    }

    [Test]
    public void TestPreProcessTransactionSetLiteralOptionsLiteralNonSyncCapable()
    {
      Connect(new[] {"LITERAL+"}, delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;

        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.LiteralNonSync));

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
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestPreProcessTransactionSetLiteralOptionsLiteralNonSyncIncapable()
    {
      Connect(delegate(ImapSession session) {
        session.HandlesIncapableAsException = true;

        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.LiteralNonSync));

        session.GenericCommand("X-TEST", new ImapLiteralString("literal", Encoding.ASCII, ImapLiteralOptions.NonSynchronizing));
      });
    }

    [Test]
    public void TestPreProcessTransactionSetLiteralOptionsLiteral8Capable()
    {
      Connect(new[] {"BINARY"}, delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;

        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Binary));

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
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestPreProcessTransactionSetLiteralOptionsLiteral8Incapable()
    {
      Connect(delegate(ImapSession session) {
        session.HandlesIncapableAsException = true;

        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Binary));

        session.GenericCommand("X-TEST", new ImapLiteralString("literal", Encoding.ASCII, ImapLiteralOptions.Literal8));
      });
    }

    [Test]
    public void TestConnect()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsFalse(session.IsIdling);
        Assert.IsNull(session.SelectedMailbox);
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
        Assert.AreEqual(new Uri(string.Format("imap://{0}:{1}/", server.Host, server.Port)), session.Authority);
        Assert.AreEqual(0, session.ServerCapabilities.Count);
      });
    }

    [Test]
    public void TestConnectBye()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* BYE not available.\r\n");

        try {
          using (var session = new ImapSession(server.Host, server.Port)) {
            Assert.Fail("ImapConnectionException not thrown");
          }
        }
        catch (ImapConnectionException ex) {
          Assert.IsNull(ex.InnerException);
          StringAssert.Contains("not available.", ex.Message);
        }
      }
    }

    [Test]
    public void TestConnectInvalidResponse()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK pseudo POP server ready.\r\n");

        try {
          using (var session = new ImapSession(server.Host, server.Port)) {
            Assert.Fail("ImapConnectionException not thrown");
          }
        }
        catch (ImapConnectionException ex) {
          Assert.IsNull(ex.InnerException);
          StringAssert.Contains("unexpected data response", ex.Message);
        }
      }
    }

    [Test]
    public void TestConnectTransactionTimeout()
    {
      using (var server = CreateServer()) {
        server.Stop();

        try {
          using (var session = new ImapSession(server.Host, server.Port, 500)) {
            Assert.Fail("ImapConnectionException not thrown");
          }
        }
        catch (ImapConnectionException ex) {
          var timeoutException = ex.InnerException as TimeoutException;

          Assert.IsNotNull(timeoutException);
        }
      }
    }

    [Test]
    public void TestConnectConnectionTimeout()
    {
      try {
        using (var session = new ImapSession("imap.localhost", 10143, 1000)) {
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
      using (var server = CreateServer()) {
        server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

        var streamUpgraded = false;

        using (var session = new ImapSession(server.Host, server.Port, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SslStream
        })) {
          Assert.IsFalse(session.IsIdling);
          Assert.IsNull(session.SelectedMailbox);
          Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
          Assert.AreEqual(new Uri(string.Format("imaps://{0}:{1}/", server.Host, server.Port)), session.Authority);
          Assert.AreEqual(0, session.ServerCapabilities.Count);
          Assert.IsTrue(streamUpgraded, "stream upgraded");
          Assert.IsTrue(session.IsSecureConnection);
        }
      }
    }

    [Test]
    public void TestConnectSslExceptionWhileUpgrading()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

        try {
          using (var session = new ImapSession(server.Host, server.Port, delegate(ConnectionBase connection, Stream baseStream) {
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
    }

    [Test]
    public void TestDisconnect()
    {
      Connect(delegate(ImapSession session) {
        session.Disconnect(false);

        Assert.IsTrue(session.IsDisposed);

        try {
          session.NoOp();
          Assert.Fail("ObjectDisposedException not thrown");
        }
        catch (ObjectDisposedException) {
        }
      });
    }

    [Test]
    public void TestImap4Rev1Incapable()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        server.EnqueueResponse("* CAPABILITY NONE\r\n0000 OK ImapCapability completed\r\n");

        ImapCapabilitySet caps;

        Assert.IsTrue((bool)session.Capability(out caps));
        Assert.IsNotNull(caps);
        Assert.IsFalse(caps.Contains(ImapCapability.Imap4Rev1));
        Assert.AreEqual("0000 CAPABILITY\r\n",
                        server.DequeueRequest());

        try {
          session.HandlesIncapableAsException = true;
          session.NoOp();
          Assert.Fail("exception not occured");
        }
        catch (ImapIncapableException) {
        }
      });
    }

    [Test]
    public void TestGreetingWithCapability()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=CRAM-MD5 AUTH=PLAIN CHILDREN THREAD=REFERENCES X-EXTENSION1 X-EXTENSION2] ImapSimulatedServer ready\r\n");

        using (var session = new ImapSession(server.Host, server.Port)) {
          Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
          Assert.AreEqual(7, session.ServerCapabilities.Count);

          Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));
          Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Children));
          Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.CRAMMD5));
          Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.Plain));

          Assert.IsTrue(session.ServerCapabilities.Contains("THREAD=REFERENCES"));
          Assert.IsTrue(session.ServerCapabilities.Contains("X-EXTENSION1"));
          Assert.IsTrue(session.ServerCapabilities.Contains("X-EXTENSION2"));

          try {
            session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
            Assert.Fail("NotSupportedException not thrown");
          }
          catch (NotSupportedException) {
          }
        }
      }
    }

    [Test]
    public void TestGreetingWithNoCapability()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

        using (var session = new ImapSession(server.Host, server.Port)) {
          Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
          Assert.AreEqual(0, session.ServerCapabilities.Count);

          Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));

          try {
            session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
            Assert.Fail("NotSupportedException not thrown");
          }
          catch (NotSupportedException) {
          }
        }
      }
    }

    [Test]
    public void TestGreetingPreAuthWithCapability()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* PREAUTH [CAPABILITY IMAP4rev1 AUTH=PLAIN] ImapSimulatedServer ready\r\n");

        using (var session = new ImapSession(server.Host, server.Port)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(2, session.ServerCapabilities.Count);
          Assert.AreEqual(new Uri(string.Format("imap://{0}:{1}/", server.Host, server.Port)), session.Authority);

          Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));
          Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.Plain));

          try {
            session.ServerCapabilities.Add(ImapCapability.Imap4Rev1);
            Assert.Fail("NotSupportedException not thrown");
          }
          catch (NotSupportedException) {
          }
        }
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestGreetingOkWithNoImapCapability()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* OK [CAPABILITY AUTH=PLAIN] ImapSimulatedServer ready\r\n");

        using (var session = new ImapSession(server.Host, server.Port)) {
          Assert.IsTrue(!session.ServerCapabilities.Contains(ImapCapability.Imap4Rev1));

          session.HandlesIncapableAsException = true;

          session.Login(new NetworkCredential("user", "pass"));
        }
      }
    }

    [Test]
    public void TestGreetingHomeServerReferralAsException()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* BYE [REFERRAL IMAP://user;AUTH=*@SERVER2/] Server not accepting connections.  Try SERVER2\r\n");

        try {
          using (var session = new ImapSession(server.Host, server.Port, true)) {
            Assert.Fail("connected without exception");
          }
        }
        catch (ImapLoginReferralException ex) {
          Assert.IsTrue(ex.Message.Contains("Server not accepting connections.  Try SERVER2"));
          Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER2/"), ex.ReferToUri);
        }
      }
    }

    [Test]
    [ExpectedException(typeof(ImapConnectionException))]
    public void TestGreetingHomeServerReferralAsError()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("* BYE [REFERRAL IMAP://user;AUTH=*@SERVER2/] Server not accepting connections.  Try SERVER2\r\n");

        using (var session = new ImapSession(server.Host, server.Port)) {
        }
      }
    }

    [Test]
    public void TestTransactionTimeoutWhileTransactionProceeding()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
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
      });
    }

    [Test]
    public void TestSocketTimeoutWhileTransactionProceeding()
    {
      Connect(delegate(ImapSession session) {
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
      });
    }

    [Test]
    public void TestInternalErrorWhileTransactionProceeding()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
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
      });
    }

    [Test]
    public void TestDiconnectedFromServerWithByeResponse()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
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
      });
    }

    [Test]
    public void TestDiconnectedFromServerWithoutByeResponse()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
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
      });
    }

    [Test]
    public void TestReceivedCommandTagUnmatched()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        server.EnqueueResponse("0001 OK authenticated\r\n");

        try {
          session.Login(new NetworkCredential("user", "pass"));
          Assert.Fail("ImapException not thrown");
        }
        catch (ImapException ex) {
          Assert.IsNotNull(ex.InnerException);
          Assert.IsInstanceOfType(typeof(ImapMalformedResponseException), ex.InnerException);
        }
      });
    }

    [Test]
    public void TestListStatusSelect()
    {
      // greeting and login transaction
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        server.EnqueueResponse("0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Login(new NetworkCredential("user", "pass")));

        var expectedAuthority = new Uri(string.Format("imap://user@{0}/", server.HostPort));

        Assert.AreEqual(expectedAuthority, session.Authority);

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

        Assert.AreEqual("0002 LIST \"\" *\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("INBOX", mailboxes[0].Name);
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter);
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.HasChildren));
        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX"), mailboxes[0].Url);

        // LSUB transaction
        server.EnqueueResponse("* LSUB (\\HasChildren) \"/\" \"Trash\"\r\n" +
                               "* LSUB (\\HasNoChildren) \"/\" \"Trash/mail\"\r\n" +
                               "* LSUB (\\HasNoChildren) \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "* LSUB (\\HasNoChildren) \".\" \"INBOX.&U,BTFw-\"\r\n" +
                               "0003 OK LSUB completed\r\n");

        Assert.IsTrue((bool)session.Lsub("*", out mailboxes));

        Assert.AreEqual("0003 LSUB \"\" *\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(4, mailboxes.Length);
        Assert.AreEqual("Trash", mailboxes[0].Name);
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.HasChildren));

        Assert.AreEqual("Trash/mail", mailboxes[1].Name);
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.HasNoChildren));
        Assert.AreEqual(new Uri(expectedAuthority, "./Trash/mail"), mailboxes[1].Url);

        Assert.AreEqual("INBOX.日本語", mailboxes[2].Name);
        Assert.AreEqual("INBOX.台北", mailboxes[3].Name);

        // STATUS transaction
        server.EnqueueResponse("* STATUS \"Trash\" (MESSAGES 3 RECENT 1 UIDVALIDITY 123456 UNSEEN 2 UIDNEXT 4)\r\n" +
                               "0004 OK STATUS completed\r\n");

        Assert.IsTrue((bool)session.Status(mailboxes[0], ImapStatusDataItem.StandardAll));

        Assert.AreEqual("0004 STATUS Trash (MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)\r\n",
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

        Assert.AreEqual("0005 SELECT INBOX.&ZeVnLIqe-\r\n",
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
        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX.日本語;UIDVALIDITY=1202674433"), mailbox.Url);

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
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains("custom1"));
        Assert.IsTrue(mailbox.ApplicableFlags.Contains("custom2"));

        Assert.IsTrue(mailbox.PermanentFlags.Contains(ImapMessageFlag.AllowedCreateKeywords));
        Assert.IsTrue(mailbox.PermanentFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.PermanentFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.PermanentFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.PermanentFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.PermanentFlags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue(mailbox.PermanentFlags.Contains("custom1"));

        // CLOSE transaction
        server.EnqueueResponse("0006 OK CLOSE completed\r\n");

        Assert.IsTrue((bool)session.Close());

        Assert.AreEqual("0006 CLOSE\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      });
    }

    [Test]
    public void TestSerializeBinaryCommandResultCodeOk()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // CAPABILITY transaction
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0002 OK CAPABILITY completed\r\n");

        var result = session.Capability();

        Smdn.Net.TestUtils.SerializeBinary(result, delegate(ImapCommandResult deserialized) {
          Assert.AreEqual(ImapCommandResultCode.Ok, deserialized.Code);
          Assert.IsNotNull(deserialized.TaggedStatusResponse);
          Assert.AreEqual("0002", deserialized.TaggedStatusResponse.Tag);
          Assert.AreEqual(ImapResponseCondition.Ok, deserialized.TaggedStatusResponse.Condition);
          Assert.IsNull(deserialized.TaggedStatusResponse.ResponseText.Code);
          Assert.AreEqual("CAPABILITY completed", deserialized.ResponseText);
          Assert.IsNull(deserialized.Description);

          var responses = deserialized.ReceivedResponses.ToArray();

          Assert.AreEqual(2, responses.Length);
          Assert.IsInstanceOfType(typeof(ImapDataResponse), responses[0]);
          Assert.IsInstanceOfType(typeof(ImapTaggedStatusResponse), responses[1]);
        });
      });
    }

    [Test]
    public void TestSerializeBinaryCommandResultCodeRequestDone()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        var result = session.Login(new NetworkCredential("user", "pass")); // already authenticated

        Smdn.Net.TestUtils.SerializeBinary(result, delegate(ImapCommandResult deserialized) {
          Assert.AreEqual(ImapCommandResultCode.RequestDone, deserialized.Code);
          Assert.IsNull(deserialized.TaggedStatusResponse);
          Assert.IsNull(deserialized.ResponseText);
          Assert.IsNotNull(deserialized.Description);

          var responses = deserialized.ReceivedResponses.ToArray();

          Assert.AreEqual(0, responses.Length);
        });
      });
    }

    class NullCredential : ICredentialsByHost {
      public NetworkCredential GetCredential(string host, int port, string authType)
      {
        return null;
      }
    }

    [Test]
    public void TestSerializeBinaryCommandResultCodeRequestError()
    {
      Connect(delegate(ImapSession session, ImapPseudoServer server) {
        var result = session.Login(new NullCredential());

        Smdn.Net.TestUtils.SerializeBinary(result, delegate(ImapCommandResult deserialized) {
          Assert.AreEqual(ImapCommandResultCode.RequestError, deserialized.Code);
          Assert.IsNull(deserialized.TaggedStatusResponse);
          Assert.IsNull(deserialized.ResponseText);
          Assert.IsNotNull(deserialized.Description);

          var responses = deserialized.ReceivedResponses.ToArray();

          Assert.AreEqual(0, responses.Length);
        });
      });
    }

    [Test]
    public void TestGenericCommandNoWithArguments()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
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
      });
    }

    [Test]
    public void TestGenericCommandWithArguments()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
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
      });
    }
  }
}
