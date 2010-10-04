using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using NUnit.Framework;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Pop3.Client.Session;
using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3.Client {
  [TestFixture]
  public class PopClientTests {
    [Test]
    public void TestConstruct()
    {
      Assert.AreEqual(new Uri("pop://localhost/"),
                      DefaultPropertyAssertion(new PopClient()).Profile.Authority,
                      "#1 authority");

      Assert.AreEqual(new Uri("pop://user;AUTH=+APOP@pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient(new Uri("pop://user;AUTH=+APOP@pop.example.net:10110/"))).Profile.Authority,
                      "#2 authority");

      Assert.AreEqual(new Uri("pop://pop.example.net/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net")).Profile.Authority,
                      "#3 authority");

      Assert.AreEqual(new Uri("pop://user@pop.example.net/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", "user")).Profile.Authority,
                      "#4 authority");

      Assert.AreEqual(new Uri("pops://user@pop.example.net/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", true, "user")).Profile.Authority,
                      "#5 authority");

      Assert.AreEqual(new Uri("pop://pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110)).Profile.Authority,
                      "#6 authority");

      Assert.AreEqual(new Uri("pop://user@pop.example.net:10110/"), 
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110, "user")).Profile.Authority,
                      "#7 authority");

      Assert.AreEqual(new Uri("pop://user@pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110, "user")).Profile.Authority,
                      "#8 authority");

      Assert.AreEqual(new Uri("pops://user@pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110, true, "user")).Profile.Authority,
                      "#9 authority");

      Assert.AreEqual(new Uri("pop://user;AUTH=+APOP@pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110, "user", "+APOP")).Profile.Authority,
                      "#10 authority");

      Assert.AreEqual(new Uri("pops://user;AUTH=DIGEST-MD5@pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110, true, "user", "DIGEST-MD5")).Profile.Authority,
                      "#11 authority");

      Assert.AreEqual(new Uri("pops://user@pop.example.net:10110/"),
                      DefaultPropertyAssertion(new PopClient("pop.example.net", 10110, true, "user", null, 1000), 1000).Profile.Authority,
                      "#12 authority");

      var profile = new PopClientProfile(new Uri("pops://user@pop.example.net/"));

      Assert.AreEqual(new Uri("pops://user@pop.example.net/"),
                      DefaultPropertyAssertion(new PopClient(profile)).Profile.Authority,
                      "#13 authority");
    }

    private PopClient DefaultPropertyAssertion(PopClient client)
    {
      return DefaultPropertyAssertion(client, System.Threading.Timeout.Infinite);
    }

    private PopClient DefaultPropertyAssertion(PopClient client, int timeoutMilliseconds)
    {
      Assert.IsNotNull(client.Profile);
      Assert.IsTrue(client.Profile.UseTlsIfAvailable);
      Assert.IsFalse(client.Profile.AllowInsecureLogin);
      Assert.IsFalse(client.DeleteAfterRetrieve);
      Assert.AreEqual(timeoutMilliseconds, client.Timeout);
      Assert.AreEqual(System.Threading.Timeout.Infinite, client.ReceiveTimeout);
      Assert.AreEqual(System.Threading.Timeout.Infinite, client.SendTimeout);

      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.TotalSize));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.MessageCount));

      return client;
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetIsSecureSessionNotConnected()
    {
      using (var client = new PopClient()) {
        Assert.IsTrue(client.IsSecureSession);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetMessageCountNotConnected()
    {
      using (var client = new PopClient()) {
        Assert.AreNotEqual(0L, client.MessageCount);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetTotalSizeNotConnected()
    {
      using (var client = new PopClient()) {
        Assert.AreNotEqual(0L, client.TotalSize);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestPropertyGetServerCapabilitiesNotConnected()
    {
      using (var client = new PopClient()) {
        Assert.IsNotNull(client.ServerCapabilities);
      }
    }

    [Test]
    public void TestConnectWithPassword()
    {
      Connect(delegate(PopPseudoServer server, PopClient client) {
        client.Profile.AllowInsecureLogin = true;

        client.Connect("pass");

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNull((client.Profile as IPopSessionProfile).Credentials);
      });
    }

    [Test]
    public void TestConnectWithCredentials()
    {
      Connect(delegate(PopPseudoServer server, PopClient client) {
        client.Profile.AllowInsecureLogin = true;

        client.Connect(new NetworkCredential("user", "pass"));

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNull((client.Profile as IPopSessionProfile).Credentials);
      });
    }

    [Test]
    public void TestConnectWithSpecifiedSaslMechanism()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port)) {
          // greeting
          server.EnqueueResponse("+OK\r\n");
          // CAPA
          server.EnqueueResponse("+OK\r\n" +
                                 ".\r\n");
          // AUTH X-PSEUDO-MECHANISM
          server.EnqueueResponse("+ \r\n");
          server.EnqueueResponse("+OK\r\n");

          using (var authMechanism = new SaslPseudoMechanism(false, 1)) {
            client.Connect(authMechanism);

            Assert.AreEqual(Smdn.Security.Authentication.Sasl.SaslExchangeStatus.Succeeded,
                            authMechanism.ExchangeStatus);
          }

          Assert.IsTrue(client.IsConnected);
          Assert.IsFalse(client.IsSecureSession);
          Assert.IsNotNull(client.ServerCapabilities);
          Assert.IsNull((client.Profile as IPopSessionProfile).Credentials);
        }
      }
    }

    [Test]
    public void TestConnectAlreadyConnected()
    {
      Connect(delegate(PopPseudoServer server, PopClient client) {
        client.Profile.AllowInsecureLogin = true;

        client.Connect(new NetworkCredential("user", "pass"));

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);

        try {
          client.Connect("pass");
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
          Assert.IsNull((client.Profile as IPopSessionProfile).Credentials);
        }
      });
    }

    private void Connect(Action<PopPseudoServer, PopClient> action)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port, "user")) {
          Assert.IsFalse(client.IsConnected);
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.TotalSize));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.MessageCount));

          // greeting
          server.EnqueueResponse("+OK\r\n");
          // CAPA
          server.EnqueueResponse("+OK\r\n" +
                                 ".\r\n");
          // USER/PASS
          server.EnqueueResponse("+OK\r\n");
          server.EnqueueResponse("+OK\r\n");

          action(server, client);
        }
      }
    }

    [Test]
    public void TestBeginConnectPassword()
    {
      BeginConnect(true);
    }

    [Test]
    public void TestBeginConnectCredentials()
    {
      BeginConnect(false);
    }

    private void BeginConnect(bool usePassword)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port, "user")) {
          client.Profile.Timeout = 5000;
          client.Profile.AllowInsecureLogin = true;

          var callbacked = false;

          var asyncCallback = (AsyncCallback)delegate(IAsyncResult ar) {
            callbacked = true;
            Assert.IsNotNull(ar);
            Assert.AreSame(ar.AsyncState, client);
          };

          var asyncResult = usePassword
            ? (PopClient.ConnectAsyncResult)client.BeginConnect("pass", asyncCallback, client)
            : (PopClient.ConnectAsyncResult)client.BeginConnect(new NetworkCredential("user", "pass"), asyncCallback, client);

          Assert.IsFalse(client.IsConnected);
          Assert.IsNotNull(asyncResult);
          Assert.IsFalse(asyncResult.IsCompleted);
          Assert.IsFalse(asyncResult.EndConnectCalled);
          Assert.IsFalse(callbacked);

          try {
            client.BeginConnect("pass");
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }

          var sw = Stopwatch.StartNew();

          ThreadPool.QueueUserWorkItem(delegate(object state) {
            var s = state as PopPseudoServer;

            Thread.Sleep(250);

            // greeting
            s.EnqueueResponse("+OK\r\n");
            // CAPA
            s.EnqueueResponse("+OK\r\n" +
                                   ".\r\n");
            // USER/PASS
            s.EnqueueResponse("+OK\r\n");
            s.EnqueueResponse("+OK\r\n");
          }, server);

          Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(3000, false));

          sw.Stop();

          Assert.IsFalse(client.IsConnected);
          Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 250);

          Thread.Sleep(50); // wait for callback
          Assert.IsTrue(callbacked);
          Assert.IsFalse(asyncResult.EndConnectCalled);

          client.EndConnect(asyncResult);

          Assert.IsTrue(asyncResult.EndConnectCalled);
          Assert.IsTrue(client.IsConnected);
          Assert.IsFalse(client.IsSecureSession);

          Assert.IsNull((client.Profile as IPopSessionProfile).Credentials);

          try {
            client.EndConnect(asyncResult);
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (ArgumentException) {
          }
        }
      }
    }

    [Test]
    public void TestBeginConnectAlreadyConnected()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);

        client.Profile.AllowInsecureLogin = true;

        try {
          client.BeginConnect("pass");
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
      });
    }

    [Test]
    public void TestBeginConnectException()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port, "user")) {
          client.Profile.Timeout = 250;

          Assert.IsFalse(client.IsConnected);

          var asyncResult = client.BeginConnect("pass");

          try {
            client.EndConnect(asyncResult);
            Assert.Fail("PopConnectionException not thrown");
          }
          catch (PopConnectionException) {
            Assert.IsNull((client.Profile as IPopSessionProfile).Credentials);
          }

          Assert.IsFalse(client.IsConnected);
        }
      }
    }


    [Test]
    public void TestLogoutAsyncConnectRunning()
    {
      DisconnectAsyncConnectRunning(true);
    }

    [Test]
    public void TestDisconnectAsyncConnectRunning()
    {
      DisconnectAsyncConnectRunning(false);
    }

    private void DisconnectAsyncConnectRunning(bool logout)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port, "user")) {
          client.BeginConnect("pass");

          try {
            if (logout)
              client.Logout();
            else
              client.Disconnect();

            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }
        }
      }
    }

    [Test]
    public void TestDisposeAsyncConnectRunning()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port, "user")) {
          client.Profile.AllowInsecureLogin = true;
          client.Profile.Timeout = 5000;

          Assert.IsFalse(client.IsConnected);

          var asyncResult = (PopClient.ConnectAsyncResult)client.BeginConnect("pass");

          // greeting
          server.EnqueueResponse("+OK\r\n");
          // CAPA
          server.EnqueueResponse("+OK\r\n" +
                                 ".\r\n");
          // USER/PASS
          server.EnqueueResponse("+OK\r\n");
          server.EnqueueResponse("+OK\r\n");

          Assert.IsFalse(asyncResult.EndConnectCalled);

          ((IDisposable)client).Dispose();

          StringAssert.StartsWith("CAPA", server.DequeueRequest());
          StringAssert.StartsWith("USER user", server.DequeueRequest());
          StringAssert.StartsWith("PASS pass", server.DequeueRequest());
          Assert.IsFalse(client.IsConnected);

          client.EndConnect(asyncResult);

          Assert.IsTrue(asyncResult.EndConnectCalled);

          Assert.IsFalse(client.IsConnected);
        }
      }
    }

    [Test]
    public void TestEndConnectInvalidAsyncResult()
    {
      using (PopPseudoServer
             server1 = new PopPseudoServer(),
             server2 = new PopPseudoServer()) {
        server1.Start();
        server2.Start();

        using (PopClient
               client1 = new PopClient(server1.Host, server1.Port, "user1"),
               client2 = new PopClient(server2.Host, server2.Port, "user2")) {
          client1.Profile.Timeout = 10;
          client2.Profile.Timeout = 10;

          var asyncResult1 = client1.BeginConnect("pass1");
          var asyncResult2 = client2.BeginConnect(new NetworkCredential("user2", "pass2"));

          try {
            client1.EndConnect(asyncResult2);
            Assert.Fail("ArgumentException not thrown");
          }
          catch (ArgumentException) {
          }

          try {
            client2.EndConnect(asyncResult1);
            Assert.Fail("ArgumentException not thrown");
          }
          catch (ArgumentException) {
          }

          try {
            client1.EndConnect(asyncResult1);
          }
          catch (PopConnectionException) {
            // expected exception
          }

          try {
            client2.EndConnect(asyncResult2);
          }
          catch (PopConnectionException) {
            // expected exception
          }
        }
      }
    }

    [Test]
    public void TestConnectBeginConnectRunning()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        using (var client = new PopClient(server.Host, server.Port, "user")) {
          client.Profile.AllowInsecureLogin = true;

          client.BeginConnect("pass");

          try {
            client.Connect("pass");
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }

          try {
            client.Connect(new NetworkCredential("user", "pass"));
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }
        }
      }
    }

    [Test]
    public void TestDisconnect()
    {
      DisposeOrDisconnect(true);
    }

    [Test]
    public void TestDispose()
    {
      DisposeOrDisconnect(false);
    }

    private void DisposeOrDisconnect(bool disconnect)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        if (disconnect)
          client.Disconnect();
        else
          ((IDisposable)client).Dispose();

        Assert.IsFalse(client.IsConnected);

        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.TotalSize));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.MessageCount));
      });
    }

    [Test]
    public void TestCheckConnected()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.Disconnect();

        Assert.IsFalse(client.IsConnected);

        TestUtils.ExpectExceptionThrown<InvalidOperationException>(delegate { client.KeepAlive(); });
      });
    }

    [Test]
    public void TestOperationAfterDisconnected()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.Disconnect();

        OperationAfterException(client);
      });
    }

    [Test]
    public void TestOperationAfterTimeout()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.Timeout = 100;

        try {
          client.CancelDelete();
        }
        catch (TimeoutException) {
        }

        OperationAfterException(client);
      });
    }

    [Test]
    public void TestOperationAfterInternalError()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        server.EnqueueResponse("+OK\r\n" +
                               "\r\n" + // invalid response
                               ".\r\n");

        try {
          client.GetMessages().ToArray();
        }
        catch (PopException) {
        }

        OperationAfterException(client);
      });
    }

    private void OperationAfterException(PopClient client)
    {
      try {
        client.CancelDelete();
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {
      }
    }

    [Test]
    public void TestLogout()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        client.Logout();

        Assert.AreEqual(server.DequeueRequest(), "QUIT\r\n");

        Assert.IsFalse(client.IsConnected);

        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.TotalSize));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.AreNotEqual(0L, client.MessageCount));
      });
    }

    [Test]
    public void TestReconnect()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        client.Logout();

        Assert.AreEqual(server.DequeueRequest(), "QUIT\r\n");

        Assert.IsFalse(client.IsConnected);

        // reconnect
        server.Stop();
        server.Start();

        client.Profile.Host = server.Host;
        client.Profile.Port = server.Port;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER/PASS
        server.EnqueueResponse("+OK\r\n");
        server.EnqueueResponse("+OK\r\n");
        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        client.Connect("pass");

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
      });
    }

    [Test]
    public void TestGetMessage()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        var message = client.GetMessage(1L);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);

        // get again
        Assert.AreSame(message, client.GetMessage(1L));
      });
    }

    [Test]
    public void TestGetFirstMessage()
    {
      GetFirstMessage(false);
    }

    [Test]
    public void TestGetFirstMessageGetUniqueId()
    {
      GetFirstMessage(true);
    }

    private void GetFirstMessage(bool getUniqueId)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 5 1200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        if (getUniqueId)
          // UIDL
          server.EnqueueResponse("+OK 1 whqtswO00WBw418f9t5JxYwZ\r\n");

        var message = getUniqueId
          ? client.GetFirstMessage(true)
          : client.GetFirstMessage();

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");

        if (getUniqueId)
          Assert.AreEqual(server.DequeueRequest(), "UIDL 1\r\n");

        Assert.AreEqual(5L, client.MessageCount);
        Assert.AreEqual(1200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);

        if (getUniqueId)
          Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);

        // get again
        Assert.AreSame(message, getUniqueId ? client.GetFirstMessage(true) : client.GetFirstMessage());
      });
    }

    [Test]
    public void TestGetLastMessage()
    {
      GetLastMessage(false);
    }

    [Test]
    public void TestGetLastMessageGetUniqueId()
    {
      GetLastMessage(true);
    }

    private void GetLastMessage(bool getUniqueId)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 5 1200\r\n");
        // LIST
        server.EnqueueResponse("+OK 5 200\r\n");

        if (getUniqueId)
          // UIDL
          server.EnqueueResponse("+OK 5 whqtswO00WBw418f9t5JxYwZ\r\n");

        var message = getUniqueId
          ? client.GetLastMessage(true)
          : client.GetLastMessage();

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");

        if (getUniqueId)
          Assert.AreEqual(server.DequeueRequest(), "UIDL 5\r\n");

        Assert.AreEqual(5L, client.MessageCount);
        Assert.AreEqual(1200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(5L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);

        if (getUniqueId)
          Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);

        // get again
        Assert.AreSame(message, getUniqueId ? client.GetLastMessage(true) : client.GetLastMessage());
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetFirstMessageNoMessagesExist()
    {
      GetFirstOrLastMessage(delegate(PopClient client) {
        Assert.IsNull(client.GetFirstMessage());
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetLastMessageNoMessagesExist()
    {
      GetFirstOrLastMessage(delegate(PopClient client) {
        Assert.IsNull(client.GetLastMessage());
      });
    }

    private void GetFirstOrLastMessage(Action<PopClient> action)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        action(client);
      });
    }

    [Test]
    public void TestGetMessageGetUniqueId()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");
        // UIDL
        server.EnqueueResponse("+OK 1 whqtswO00WBw418f9t5JxYwZ\r\n");

        var message = client.GetMessage(1L, true);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "UIDL 1\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);
        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);
      });
    }

    [Test]
    public void TestGetMessageDeleted()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        var message = client.GetMessage(1L);

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        Assert.IsTrue(message.IsMarkedAsDeleted);

        try {
          client.GetMessage(1L);
          Assert.Fail("PopMessageDeletedException not thrown");
        }
        catch (PopMessageDeletedException ex) {
          Assert.AreEqual(1L, ex.MessageNumber);
        }
      });
    }

    [Test]
    public void TestGetMessageNotFound()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 120\r\n");
        // LIST
        server.EnqueueResponse("-ERR no such message\r\n");

        try {
          client.GetMessage(1L);
          Assert.Fail("PopMessageNotFoundException not thrown");
        }
        catch (PopMessageNotFoundException ex) {
          Assert.AreEqual(1L, ex.MessageNumber);
          Assert.IsNull(ex.UniqueId);
        }
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetMessageZero()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.GetMessage(0L);
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetMessageGreaterThanMessageCount()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 3 1024\r\n");

        client.GetMessage(4L);
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetMessageNotConnected()
    {
      using (var client = new PopClient()) {
        client.GetMessage(1L);
      }
    }

    [Test]
    public void TestGetMessageReconnect()
    {
      var sessionMessages = new PopMessageInfo[2];

      for (var i = 0; i < 2; i++) {
        TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
          // STAT
          server.EnqueueResponse("+OK 1 200\r\n");
          // LIST
          server.EnqueueResponse("+OK 1 200\r\n");

          var message = client.GetMessage(1L);

          Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
          Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");

          Assert.AreEqual(1L, client.MessageCount);
          Assert.AreEqual(200L, client.TotalSize);

          Assert.IsNotNull(message);
          Assert.AreEqual(1L, message.MessageNumber);
          Assert.AreEqual(200L, message.Length);

          sessionMessages[i] = message;
        });
      }

      Assert.AreNotEqual(sessionMessages[0], sessionMessages[1]);
    }

    [Test]
    public void TestGetMessageByUid()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");
        // LIST
        server.EnqueueResponse("+OK 2 240\r\n");

        var message = client.GetMessage("QhdPYR:00WBw1Ph7x7");

        Assert.AreEqual(server.DequeueRequest(), "UIDL\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 2\r\n");

        Assert.AreEqual(2L, message.MessageNumber);
        Assert.AreEqual(240L, message.Length);
        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", message.UniqueId);
      });
    }

    [Test]
    public void TestGetMessageByUidDeleted()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");
        // LIST
        server.EnqueueResponse("+OK 2 240\r\n");

        var message = client.GetMessage("QhdPYR:00WBw1Ph7x7");

        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        try {
          client.GetMessage(message.UniqueId);
          Assert.Fail("PopMessageDeletedException not thrown");
        }
        catch (PopMessageDeletedException ex) {
          Assert.AreEqual(message.MessageNumber, ex.MessageNumber);
        }
      });
    }

    [Test]
    public void TestGetMessageByUidAlreadyListed()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");

        var messages = client.GetMessages(true).ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");
        Assert.AreEqual(server.DequeueRequest(), "UIDL\r\n");

        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", messages[1].UniqueId);

        var message = client.GetMessage("QhdPYR:00WBw1Ph7x7");

        Assert.IsNotNull(message);
        Assert.AreEqual(2L, message.MessageNumber);
        Assert.AreSame(messages[1], message);
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetMessageByUidNull()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.GetMessage((string)null);
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestGetMessageByUidEmpty()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.GetMessage(string.Empty);
      });
    }

    [Test]
    public void TestGetMessageByUidNotFound()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");

        try {
          client.GetMessage("non-existent");
          Assert.Fail("PopMessageNotFoundException not thrown");
        }
        catch (PopMessageNotFoundException ex) {
          Assert.AreEqual(0L, ex.MessageNumber);
          Assert.IsNotNull(ex.UniqueId);
          Assert.AreEqual("non-existent", ex.UniqueId);
        }
      });
    }

    [Test]
    public void TestGetMessages()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");

        var messages = client.GetMessages().ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].MessageNumber);
        Assert.AreEqual(120L, messages[0].Length);

        Assert.AreEqual(2L, messages[1].MessageNumber);
        Assert.AreEqual(240L, messages[1].Length);

        Assert.AreEqual(3L, messages[2].MessageNumber);
        Assert.AreEqual(360L, messages[2].Length);

        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");

        var messagesSecond = client.GetMessages().ToArray();

        Assert.AreSame(messages[0], messagesSecond[0]);
        Assert.AreSame(messages[1], messagesSecond[1]);
        Assert.AreSame(messages[2], messagesSecond[2]);
      });
    }

    [Test]
    public void TestGetMessagesGetUniqueId()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");

        var messages = client.GetMessages(true).ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].MessageNumber);
        Assert.AreEqual(120L, messages[0].Length);
        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", messages[0].UniqueId);

        Assert.AreEqual(2L, messages[1].MessageNumber);
        Assert.AreEqual(240L, messages[1].Length);
        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", messages[1].UniqueId);

        Assert.AreEqual(3L, messages[2].MessageNumber);
        Assert.AreEqual(360L, messages[2].Length);
        Assert.AreEqual("xxxxxxxxx", messages[2].UniqueId);
      });
    }

    [Test]
    public void TestGetMessagesEmpty()
    {
      GetMessagesEmpty(false);
    }

    [Test]
    public void TestGetMessagesEmptyGetUniqueId()
    {
      GetMessagesEmpty(true);
    }

    private void GetMessagesEmpty(bool getUniqueId)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");

        var messages = (getUniqueId ? client.GetMessages(true) : client.GetMessages()).ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");

        Assert.AreEqual(0, messages.Length);
      });
    }

    [Test]
    public void TestCancelDelete()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");

        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        var message = client.GetMessage(1L);

        server.DequeueRequest(); // LIST

        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        server.DequeueRequest();

        Assert.IsTrue(message.IsMarkedAsDeleted);

        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        Assert.AreEqual(0L, client.MessageCount);
        Assert.AreEqual(0L, client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");

        // RSET
        server.EnqueueResponse("+OK\r\n");

        client.CancelDelete();

        Assert.AreEqual(server.DequeueRequest(), "RSET\r\n");

        Assert.IsFalse(message.IsMarkedAsDeleted);

        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
      });
    }

    [Test]
    public void TestCancelDeleteMessageNotListed()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // RSET
        server.EnqueueResponse("+OK\r\n");

        client.CancelDelete();

        Assert.AreEqual(server.DequeueRequest(), "RSET\r\n");
      });
    }

    [Test]
    public void TestKeepAlive()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // NOOP
        server.EnqueueResponse("+OK\r\n");

        client.KeepAlive();

        Assert.AreEqual(server.DequeueRequest(), "NOOP\r\n");
      });
    }

    [Test, ExpectedException(typeof(Smdn.Net.Pop3.Protocol.PopConnectionException))]
    public void TestKeepAliveDisconnected()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        server.Stop();

        client.KeepAlive();
      });
    }
  }
}
