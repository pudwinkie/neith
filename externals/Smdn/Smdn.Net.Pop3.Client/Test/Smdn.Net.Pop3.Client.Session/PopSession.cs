using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client.Session {
  [TestFixture]
  public class PopSessionTests : PopSessionTestsBase {
    [Test]
    public void TestConnect()
    {
      server.EnqueueResponse("+OK PopPseudoServer ready\r\n");

      using (var session = new PopSession(host, port)) {
        Assert.IsFalse(session.ApopAvailable);
        Assert.IsTrue(string.IsNullOrEmpty(session.Timestamp));
        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(new Uri(string.Format("pop://{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(0, session.ServerCapabilities.Count);
      }
    }

    [Test]
    public void TestConnectTransactionTimeout()
    {
      server.Stop();

      try {
        using (var session = new PopSession(host, port, 500)) {
          Assert.Fail("PopConnectionException not thrown");
        }
      }
      catch (PopConnectionException ex) {
        var timeoutException = ex.InnerException as TimeoutException;

        Assert.IsNotNull(timeoutException);
      }
    }

    [Test]
    public void TestConnectErr()
    {
      server.EnqueueResponse("-ERR try again\r\n");

      try {
        using (var session = new PopSession(host, port)) {
          Assert.Fail("PopConnectionException not thrown");
        }
      }
      catch (PopConnectionException ex) {
        Assert.IsNull(ex.InnerException);
        StringAssert.Contains("try again", ex.Message);
      }
    }

    [Test]
    public void TestConnectInvalidResponse()
    {
      server.EnqueueResponse("* OK pseudo IMAP server ready\r\n");

      try {
        using (var session = new PopSession(host, port)) {
          Assert.Fail("PopConnectionException not thrown");
        }
      }
      catch (PopConnectionException ex) {
        Assert.IsNull(ex.InnerException);
        StringAssert.Contains("unexpected response", ex.Message);
      }
    }

    [Test]
    public void TestConnectSocketError()
    {
      try {
        using (var session = new PopSession("pop.invalid", 110)) {
          Assert.Fail("PopConnectionException not thrown");
        }
      }
      catch (PopConnectionException ex) {
        var socketException = ex.InnerException as System.Net.Sockets.SocketException;

        Assert.IsNotNull(socketException);
      }
    }

    [Test]
    public void TestConnectSsl()
    {
      server.EnqueueResponse("+OK PopPseudoServer ready\r\n");

      var streamUpgraded = false;

      using (var session = new PopSession(host, port, delegate(ConnectionBase connection, Stream baseStream) {
        streamUpgraded = true;
        return baseStream; // TODO: return SslStream
      })) {
        Assert.IsFalse(session.ApopAvailable);
        Assert.IsTrue(string.IsNullOrEmpty(session.Timestamp));
        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(new Uri(string.Format("pops://{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(0, session.ServerCapabilities.Count);
        Assert.IsTrue(streamUpgraded, "stream upgraded");
        Assert.IsTrue(session.IsSecureConnection);
      }
    }

    [Test]
    public void TestConnectSslExceptionWhileUpgrading()
    {
      server.EnqueueResponse("+OK PopPseudoServer ready\r\n");

      try {
        using (var session = new PopSession(host, port, delegate(ConnectionBase connection, Stream baseStream) {
          throw new System.Security.Authentication.AuthenticationException();
        })) {
          Assert.Fail("connected");
        }
      }
      catch (PopUpgradeConnectionException ex) {
        Assert.IsNotNull(ex.InnerException);
        Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                ex.InnerException);
      }
    }

    [Test]
    public void TestDisconnect()
    {
      server.EnqueueResponse("+OK PopPseudoServer ready\r\n");

      var session = new PopSession(host, port);

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
    public void TestDisconnectedFromServer()
    {
      using (var session = Connect(null)) {
        Assert.AreEqual(PopSessionState.Authorization, session.State);

        server.Stop();

        try {
          session.Quit();
          Assert.Fail("PopConnectionException not thrown");
        }
        catch (PopConnectionException) {
        }

        Assert.AreEqual(PopSessionState.NotConnected, session.State);
        Assert.AreEqual(null, session.Authority);

        try {
          session.NoOp();
          Assert.Fail("PopProtocolViolationException not thrown");
        }
        catch (PopProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestGreetingWithTimestamp()
    {
      server.EnqueueResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

      using (var session = new PopSession(host, port)) {
        Assert.IsTrue(session.ApopAvailable);
        Assert.AreEqual("<1896.697170952@dbc.mtview.ca.us>", session.Timestamp);
        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(new Uri(string.Format("pop://{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(0, session.ServerCapabilities.Count);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestChangeReadOnlyPropertyServerCapabilities()
    {
      using (var session = Connect(null)) {
        session.ServerCapabilities.Add(PopCapability.User);
      }
    }

    [Test]
    public void TestTransactionTimeoutWhileTransactionProceeding()
    {
      using (var session = Connect(null)) {
        session.TransactionTimeout = 250;
        session.SendTimeout = System.Threading.Timeout.Infinite;
        session.ReceiveTimeout = System.Threading.Timeout.Infinite;

        // not respond CAPA response termination
        server.EnqueueResponse("+OK List of capabilities follows\r\n");

        try {
          session.Capa();
          Assert.Fail("TimeoutException not thrown");
        }
        catch (TimeoutException) {
          Assert.AreEqual(PopSessionState.NotConnected, session.State);
        }

        try {
          session.NoOp();
          Assert.Fail("PopProtocolViolationException not thrown");
        }
        catch (PopProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestSocketTimeoutWhileTransactionProceeding()
    {
      using (var session = Connect(null)) {
        session.TransactionTimeout = System.Threading.Timeout.Infinite;
        session.SendTimeout = 250;
        session.ReceiveTimeout = 250;

        // not respond CAPA
        //server.EnqueueResponse("+OK List of capabilities follows\r\n.\r\n");

        try {
          session.Capa();
          Assert.Fail("TimeoutException not thrown");
        }
        catch (TimeoutException) {
          Assert.AreEqual(PopSessionState.NotConnected, session.State);
        }

        try {
          session.NoOp();
          Assert.Fail("PopProtocolViolationException not thrown");
        }
        catch (PopProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestInternalErrorWhileTransactionProceeding()
    {
      using (var session = Connect(null)) {
        server.EnqueueResponse("+OK List of capabilities follows\r\n" +
                               "\r\n" + // invalid response
                               ".\r\n");

        try {
          session.Capa();
          Assert.Fail("PopException not thrown");
        }
        catch (PopException ex) {
          Assert.IsNotNull(ex.InnerException);
          Assert.AreEqual(PopSessionState.NotConnected, session.State);
        }

        try {
          session.NoOp();
          Assert.Fail("PopProtocolViolationException not thrown");
        }
        catch (PopProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestBeginNewTransactionWhileTransactionProceeding()
    {
      using (var session = Connect(null)) {
        using (var waitForFinishedEvent = new ManualResetEvent(false)) {
          session.TransactionTimeout = 500;

          Exception unexpectedException = null;

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            try {
              try {
                (state as PopSession).Capa();
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
            session.Capa();
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
    public void TestQuitInAuthorizationState()
    {
      using (var session = Connect(null)) {
        server.EnqueueResponse("+OK dewey POP3 server signing off\r\n");

        Assert.IsTrue((bool)session.Quit());

        StringAssert.AreEqualIgnoringCase("QUIT\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.NotConnected, session.State);
        Assert.IsNull(session.Authority);
      }
    }

    [Test]
    public void TestQuitAfterDisconnected()
    {
      using (var session = Connect(null)) {
        server.EnqueueResponse("+OK dewey POP3 server signing off\r\n");

        Assert.IsTrue((bool)session.Quit());

        StringAssert.AreEqualIgnoringCase("QUIT\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.NotConnected, session.State);

        Assert.IsTrue((bool)session.Quit());

        Assert.AreEqual(PopSessionState.NotConnected, session.State);
      }
    }

    [Test]
    public void TestCapa()
    {
      using (var session = Connect(null)) {
        Assert.IsNotNull(session.ServerCapabilities);
        Assert.AreEqual(0, session.ServerCapabilities.Count);

        server.EnqueueResponse("+OK Capability list follows\r\n" +
                               "TOP\r\n" +
                               "USER\r\n" +
                               "SASL CRAM-MD5 KERBEROS_V4\r\n" +
                               "RESP-CODES\r\n" +
                               "LOGIN-DELAY 900\r\n" +
                               "PIPELINING\r\n" +
                               "EXPIRE 60\r\n" +
                               "UIDL\r\n" +
                               "IMPLEMENTATION Shlemazle-Plotz-v302\r\n" +
                               ".\r\n");

        PopCapabilityList capabilities;

        Assert.IsTrue((bool)session.Capa(out capabilities));

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(9, capabilities.Count);
        Assert.IsTrue(capabilities.Has(PopCapability.Top));
        Assert.IsTrue(capabilities.Has(PopCapability.User));
        Assert.IsTrue(capabilities.Has(new PopCapability("SASL", "CRAM-MD5", "KERBEROS_V4")));
        Assert.IsTrue(capabilities.Has(PopCapability.RespCodes));
        Assert.IsTrue(capabilities.Has(new PopCapability((string)PopCapability.LoginDelay, "900")));
        Assert.IsTrue(capabilities.Has(PopCapability.Pipelining));
        Assert.IsTrue(capabilities.Has(new PopCapability((string)PopCapability.Expire, "60")));
        Assert.IsTrue(capabilities.Has(PopCapability.Uidl));
        Assert.IsTrue(capabilities.Has(new PopCapability((string)PopCapability.Implementation, "Shlemazle-Plotz-v302")));

        Assert.IsTrue(capabilities.IsCapable(PopCapability.Sasl));
        Assert.IsTrue(capabilities.IsCapable(PopCapability.LoginDelay));
        Assert.IsTrue(capabilities.IsCapable(PopCapability.Expire));
        Assert.IsTrue(capabilities.IsCapable(PopCapability.Implementation));
        Assert.IsTrue(capabilities.IsCapable(PopAuthenticationMechanism.CRAMMD5));
        Assert.IsTrue(capabilities.IsCapable(PopAuthenticationMechanism.KerberosV4));

        Assert.IsNotNull(session.ServerCapabilities);
        Assert.AreEqual(9, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(PopCapability.User));
        Assert.IsTrue(session.ServerCapabilities.IsCapable(PopCapability.Sasl));

        try {
          session.ServerCapabilities.Add(PopCapability.Lang);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      }
    }




    [Test]
    public void TestQuitInTransactionState()
    {
      using (var session = Login()) {
        server.EnqueueResponse("+OK dewey POP3 server signing off (maildrop empty)\r\n");

        Assert.IsTrue((bool)session.Quit());

        StringAssert.AreEqualIgnoringCase("QUIT\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.NotConnected, session.State);
        Assert.IsNull(session.Authority);
      }
    }

    [Test]
    public void TestGenericCommandWithNoArguments()
    {
      using (var session = Connect(null)) {
        // generic command transaction
        server.EnqueueResponse("+OK done\r\n");

        PopResponse[] responses;

        Assert.IsTrue((bool)session.GenericCommand("X-EXT-COMMAND", out responses));

        StringAssert.AreEqualIgnoringCase("X-EXT-COMMAND\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(1, responses.Length);
        Assert.IsInstanceOfType(typeof(PopStatusResponse), responses[0]);

        var status = responses[0] as PopStatusResponse;

        Assert.AreEqual("done", status.Text);
      }
    }

    [Test]
    public void TestGenericCommandWithArguments()
    {
      using (var session = Connect(null)) {
        // generic command transaction
        server.EnqueueResponse("+OK done\r\n");

        PopResponse[] responses;

        Assert.IsTrue((bool)session.GenericCommand("X-EXT-COMMAND", out responses, "arg1", "arg2", "arg3"));

        StringAssert.AreEqualIgnoringCase("X-EXT-COMMAND arg1 arg2 arg3\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(1, responses.Length);
        Assert.IsInstanceOfType(typeof(PopStatusResponse), responses[0]);

        var status = responses[0] as PopStatusResponse;

        Assert.AreEqual("done", status.Text);
      }
    }

    [Test]
    public void TestGenericCommandMultilineResponse()
    {
      using (var session = Connect(null)) {
        // generic command transaction
        server.EnqueueResponse("+OK [X-RESP-CODE] done\r\n" +
                               "1st line\r\n" +
                               "2nd line\r\n" +
                               ".byte-stuffed line\r\n" +
                               ".\r\n");

        PopResponse[] responses;

        Assert.IsTrue((bool)session.GenericCommand("X-EXT-COMMAND", true, out responses));

        StringAssert.AreEqualIgnoringCase("X-EXT-COMMAND\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(5, responses.Length);
        Assert.IsInstanceOfType(typeof(PopStatusResponse), responses[0]);
        Assert.IsInstanceOfType(typeof(PopFollowingResponse), responses[1]);
        Assert.IsInstanceOfType(typeof(PopFollowingResponse), responses[2]);
        Assert.IsInstanceOfType(typeof(PopFollowingResponse), responses[3]);
        Assert.IsInstanceOfType(typeof(PopTerminationResponse), responses[4]);

        var status = responses[0] as PopStatusResponse;

        Assert.AreEqual("done", status.Text);
        Assert.IsNotNull(status.ResponseText);
        Assert.AreEqual("X-RESP-CODE", (string)status.ResponseText.Code);

        Assert.AreEqual("1st line", (responses[1] as PopFollowingResponse).Text.ToString());
        Assert.AreEqual("2nd line", (responses[2] as PopFollowingResponse).Text.ToString());
        Assert.AreEqual("byte-stuffed line", (responses[3] as PopFollowingResponse).Text.ToString());
      }
    }
  }
}
