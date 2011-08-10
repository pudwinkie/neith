using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapClientTests {
    [Test]
    public void TestConstruct()
    {
      Assert.AreEqual(new Uri("imap://localhost/"),
                      DefaultPropertyAssertion(new ImapClient()).Profile.Authority,
                      "#1 authority");

      Assert.AreEqual(new Uri("imap://user;AUTH=LOGIN@imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient(new Uri("imap://user;AUTH=LOGIN@imap.example.net:10110/"))).Profile.Authority,
                      "#2 authority");

      Assert.AreEqual(new Uri("imap://imap.example.net/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net")).Profile.Authority,
                      "#3 authority");

      Assert.AreEqual(new Uri("imap://user@imap.example.net/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", "user")).Profile.Authority,
                      "#4 authority");

      Assert.AreEqual(new Uri("imaps://user@imap.example.net/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", true, "user")).Profile.Authority,
                      "#5 authority");

      Assert.AreEqual(new Uri("imap://imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110)).Profile.Authority,
                      "#6 authority");

      Assert.AreEqual(new Uri("imap://user@imap.example.net:10110/"), 
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110, "user")).Profile.Authority,
                      "#7 authority");

      Assert.AreEqual(new Uri("imap://user@imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110, "user")).Profile.Authority,
                      "#8 authority");

      Assert.AreEqual(new Uri("imaps://user@imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110, true, "user")).Profile.Authority,
                      "#9 authority");

      Assert.AreEqual(new Uri("imap://user;AUTH=PLAIN@imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110, "user", "PLAIN")).Profile.Authority,
                      "#10 authority");

      Assert.AreEqual(new Uri("imaps://user;AUTH=DIGEST-MD5@imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110, true, "user", "DIGEST-MD5")).Profile.Authority,
                      "#11 authority");

      Assert.AreEqual(new Uri("imaps://user@imap.example.net:10110/"),
                      DefaultPropertyAssertion(new ImapClient("imap.example.net", 10110, true, "user", null, 1000), 1000).Profile.Authority,
                      "#12 authority");

      var profile = new ImapClientProfile(new Uri("imaps://user@imap.example.net/"));

      Assert.AreEqual(new Uri("imaps://user@imap.example.net/"),
                      DefaultPropertyAssertion(new ImapClient(profile)).Profile.Authority,
                      "#13 authority");
    }

    private ImapClient DefaultPropertyAssertion(ImapClient client)
    {
      return DefaultPropertyAssertion(client, System.Threading.Timeout.Infinite);
    }

    private ImapClient DefaultPropertyAssertion(ImapClient client, int timeoutMilliseconds)
    {
      Assert.IsNotNull(client.Profile);
      Assert.IsTrue(client.Profile.UseTlsIfAvailable);
      Assert.IsFalse(client.Profile.AllowInsecureLogin);
      Assert.IsFalse(client.IsConnected);
      Assert.AreEqual(timeoutMilliseconds, client.Timeout);
      Assert.AreEqual(System.Threading.Timeout.Infinite, client.ReceiveTimeout);
      Assert.AreEqual(System.Threading.Timeout.Infinite, client.SendTimeout);

      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.OpenedMailbox));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerID));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerNamespace));

      return client;
    }

    [Test]
    public void TestConnectWithPassword()
    {
      Connect(delegate(ImapPseudoServer server, ImapClient client) {
        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                     "$tag OK done\r\n");

        client.Profile.AllowInsecureLogin = true;
        client.Connect("pass");

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
        Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);
      });
    }

    [Test]
    public void TestConnectWithCredentials()
    {
      Connect(delegate(ImapPseudoServer server, ImapClient client) {
        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                     "$tag OK done\r\n");

        client.Profile.AllowInsecureLogin = true;
        client.Connect(new NetworkCredential("user", "pass"));

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
        Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);
      });
    }

    [Test]
    public void TestConnectWithSpecifiedSaslMechanism()
    {
      Connect(delegate(ImapPseudoServer server, ImapClient client) {
        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // AUTHENTICATE X-PSEUDO-MECHANISM
        server.EnqueueResponse("+ \r\n");
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                     "$tag OK done\r\n");

        using (var authMechanism = new SaslPseudoMechanism(1)) {
          client.Connect(authMechanism);

          Assert.AreEqual(Smdn.Security.Authentication.Sasl.SaslExchangeStatus.Succeeded,
                          authMechanism.ExchangeStatus);
        }

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
        Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);
      });
    }

    [Test]
    public void TestConnectAlreadyConnected()
    {
      Connect(delegate(ImapPseudoServer server, ImapClient client) {
        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                     "$tag OK done\r\n");

        client.Profile.AllowInsecureLogin = true;
        client.Connect(new NetworkCredential("user", "pass"));

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
        Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);

        try {
          client.Connect("pass");
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
          Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);
        }
      });
    }

    [Test]
    public void TestConnectIDCapable()
    {
      Connect(delegate(ImapPseudoServer server, ImapClient client) {
        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1 ID\r\n" + 
                                     "$tag OK done\r\n");
        // ID
        server.EnqueueTaggedResponse("* ID (\"name\" \"ImapPseudoServer\")\r\n" + 
                                     "$tag OK done\r\n");

        client.Profile.AllowInsecureLogin = true;
        client.Connect("pass");

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
        Assert.AreEqual("ImapPseudoServer", client.ServerID["name"]);
      });
    }

    [Test]
    public void TestConnectNamespaceCapable()
    {
      Connect(delegate(ImapPseudoServer server, ImapClient client) {
        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1 NAMESPACE\r\n" + 
                                     "$tag OK done\r\n");
        // NAMESPACE
        server.EnqueueTaggedResponse("* NAMESPACE ((\"\" \"/\")) NIL NIL\r\n" + 
                                     "$tag OK done\r\n");

        client.Profile.AllowInsecureLogin = true;
        client.Connect("pass");

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
        Assert.AreEqual(1, client.ServerNamespace.PersonalNamespaces.Length);
        Assert.AreEqual("/", client.ServerNamespace.PersonalNamespaces[0].HierarchyDelimiter);
        Assert.AreEqual(0, client.ServerNamespace.SharedNamespaces.Length);
        Assert.AreEqual(0, client.ServerNamespace.OtherUsersNamespaces.Length);
      });
    }

    private void Connect(Action<ImapPseudoServer, ImapClient> action)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        using (var client = new ImapClient(server.Host, server.Port, "user")) {
          Assert.IsFalse(client.IsConnected);
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.OpenedMailbox));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerNamespace));
          TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerID));

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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        using (var client = new ImapClient(server.Host, server.Port, "user")) {
          client.Profile.Timeout = 5000;

          var callbacked = false;

          var asyncCallback = (AsyncCallback)delegate(IAsyncResult ar) {
            callbacked = true;
            Assert.IsNotNull(ar);
            Assert.AreSame(ar.AsyncState, client);
          };

          client.Profile.AllowInsecureLogin = true;

          var asyncResult = usePassword
            ? (ImapClient.ConnectAsyncResult)client.BeginConnect("pass", asyncCallback, client)
            : (ImapClient.ConnectAsyncResult)client.BeginConnect(new NetworkCredential("user", "pass"), asyncCallback, client);

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
            var s = state as ImapPseudoServer;

            Thread.Sleep(250);

            // greeting
            s.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
            // LOGIN
            s.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                    "$tag OK done\r\n");
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

          Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);

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
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);

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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        using (var client = new ImapClient(server.Host, server.Port, "user")) {
          client.Profile.Timeout = 250;

          Assert.IsFalse(client.IsConnected);

          var asyncResult = client.BeginConnect("pass");

          try {
            client.EndConnect(asyncResult);
            Assert.Fail("ImapConnectionException not thrown");
          }
          catch (ImapConnectionException) {
            Assert.IsNull((client.Profile as IImapSessionProfile).Credentials);
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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        using (var client = new ImapClient(server.Host, server.Port, "user")) {
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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        using (var client = new ImapClient(server.Host, server.Port, "user")) {
          client.Profile.AllowInsecureLogin = true;
          client.Profile.Timeout = 5000;

          Assert.IsFalse(client.IsConnected);

          var asyncResult = (ImapClient.ConnectAsyncResult)client.BeginConnect("pass");

          // greeting
          server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
          // LOGIN
          server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                       "$tag OK done\r\n");

          Assert.IsFalse(asyncResult.EndConnectCalled);

          ((IDisposable)client).Dispose();

          StringAssert.Contains("LOGIN user pass", server.DequeueRequest());
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
      using (ImapPseudoServer
             server1 = new ImapPseudoServer(),
             server2 = new ImapPseudoServer()) {
        server1.Start();
        server2.Start();

        using (ImapClient
               client1 = new ImapClient(server1.Host, server1.Port, "user1"),
               client2 = new ImapClient(server2.Host, server2.Port, "user2")) {
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
          catch (ImapConnectionException) {
            // expected exception
          }

          try {
            client2.EndConnect(asyncResult2);
          }
          catch (ImapConnectionException) {
            // expected exception
          }
        }
      }
    }

    [Test]
    public void TestConnectBeginConnectRunning()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        using (var client = new ImapClient(server.Host, server.Port, "user")) {
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
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        if (disconnect)
          client.Disconnect();
        else
          ((IDisposable)client).Dispose();

        DisconnectedPropertyAssertion(client);
      });
    }

    private void DisconnectedPropertyAssertion(ImapClient client)
    {
      Assert.IsFalse(client.IsConnected);
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.OpenedMailbox));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerNamespace));
      TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerID));
    }

    [Test]
    public void TestCheckConnected()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.Disconnect();

        Assert.IsFalse(client.IsConnected);

        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => client.Refresh());
      });
    }

    [Test]
    public void TestOperationAfterDisconnected()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.Disconnect();

        OperationAfterException(client);
      });
    }

    [Test]
    public void TestOperationAfterTimeout()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.Timeout = 100;

        try {
          client.Refresh();
        }
        catch (TimeoutException) {
        }

        OperationAfterException(client);
      });
    }

    [Test]
    public void TestOperationAfterInternalError()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        server.EnqueueResponse("-ERR\r\n");

        try {
          client.Refresh();
        }
        catch (ImapException) {
        }

        OperationAfterException(client);
      });
    }

    private void OperationAfterException(ImapClient client)
    {
      try {
        client.Refresh();
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {
      }

      DisconnectedPropertyAssertion(client);
    }

    [Test]
    public void TestDisconnectedFromServerWithByeResponse()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // NOOP
        server.EnqueueTaggedResponse("* BYE server shutdown\r\n");

        try {
          client.Refresh();
          Assert.Fail("ImapErrorResponseException not thrown");
        }
        catch (ImapConnectionException ex) {
          StringAssert.Contains("server shutdown", ex.Message);
        }

        OperationAfterException(client);
      });
    }

    [Test]
    public void TestDisconnectedFromServerWithoutByeResponse()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        server.Stop();
        // NOOP
        //server.EnqueueTaggedResponse("* BYE server shutdown\r\n");

        try {
          client.Refresh();
          Assert.Fail("ImapErrorResponseException not thrown");
        }
        catch (ImapConnectionException) {
        }

        OperationAfterException(client);
      });
    }

    [Test]
    public void TestLogout()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LOGOUT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Logout();

        StringAssert.EndsWith("LOGOUT\r\n", server.DequeueRequest());

        DisconnectedPropertyAssertion(client);
      });
    }

    [Test]
    public void TestLogoutSelectedState()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsNotNull(client.OpenInbox());
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.IsTrue(client.OpenedMailbox.IsInbox);

        StringAssert.EndsWith("LIST \"\" INBOX\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT INBOX\r\n", server.DequeueRequest());

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LOGOUT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Logout();

        StringAssert.EndsWith("CLOSE\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LOGOUT\r\n", server.DequeueRequest());

        DisconnectedPropertyAssertion(client);
      });
    }

    [Test]
    public void TestReconnect()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LOGOUT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Logout();

        StringAssert.EndsWith("LOGOUT\r\n", server.DequeueRequest());

        Assert.IsFalse(client.IsConnected);

        // reconnect
        server.Stop();
        server.Start();

        client.Profile.Host = server.Host;
        client.Profile.Port = server.Port;

        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapPseudoServer ready\r\n");
        // LOGIN
        server.EnqueueTaggedResponse("* CAPABILITY IMAP4rev1\r\n" + 
                                     "$tag OK done\r\n");

        client.Connect("pass");

        Assert.IsTrue(client.IsConnected);
        Assert.IsFalse(client.IsSecureSession);
        Assert.IsNotNull(client.ServerCapabilities);
        Assert.IsNotNull(client.ServerNamespace);
        Assert.IsNotNull(client.ServerID);
      });
    }

    [Test]
    public void TestSerializeBinaryErrorResponseException()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("$tag NO no such quota root\r\n");

        try {
          client.GetQuotaUsage("NON-EXISTENT", "STORAGE");
          Assert.Fail("ImapErrorResponseException not thrown");
        }
        catch (ImapErrorResponseException ex) {
          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapErrorResponseException deserialized) {
            Assert.AreEqual(ex.Message, deserialized.Message);
            Assert.IsNotNull(deserialized.Result);
            Assert.IsNotNull(deserialized.Result.TaggedStatusResponse);
            Assert.AreEqual(ImapResponseCondition.No, deserialized.Result.TaggedStatusResponse.Condition);
            Assert.AreEqual("no such quota root", deserialized.Result.ResponseText);

            var responses = deserialized.Result.ReceivedResponses.ToArray();

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(typeof(ImapTaggedStatusResponse), responses[0]);
          });
        }
      });
    }
  }
}
