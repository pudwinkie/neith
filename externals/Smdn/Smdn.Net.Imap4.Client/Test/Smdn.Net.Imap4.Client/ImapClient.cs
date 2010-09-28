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

        Assert.IsFalse(client.IsConnected);
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.OpenedMailbox));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerNamespace));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerID));
      });
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
    }

    [Test]
    public void TestLogout()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LOGOUT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Logout();

        StringAssert.EndsWith("LOGOUT\r\n", server.DequeueRequest());

        Assert.IsFalse(client.IsConnected);
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.OpenedMailbox));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerNamespace));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerID));
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

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LOGOUT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Logout();

        StringAssert.EndsWith("CLOSE\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LOGOUT\r\n", server.DequeueRequest());

        Assert.IsFalse(client.IsConnected);
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsTrue(client.IsSecureSession));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNull(client.OpenedMailbox));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerCapabilities));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerNamespace));
        TestUtils.ExpectExceptionThrown<InvalidOperationException>(() => Assert.IsNotNull(client.ServerID));
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
    public void TestGetInbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetInbox();

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsTrue(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Draft\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("Draft");

        StringAssert.EndsWith("LIST \"\" \"Draft\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Draft", mailbox.Name);
        Assert.AreEqual("Draft", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetMailboxNotFound()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          client.GetMailbox("NonExistent");
          Assert.Fail("ImapMailboxNotFoundException not thrown");
        }
        catch (ImapMailboxNotFoundException ex) {
          Assert.IsNotNull(ex.Mailbox);
          Assert.AreEqual("NonExistent", ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxNotFoundException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestGetOrCreateMailbox()
    {
      GetOrCreateMailbox(false);
    }

    [Test]
    public void TestGetOrCreateMailboxSubscribe()
    {
      GetOrCreateMailbox(true);
    }

    private void GetOrCreateMailbox(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Trash\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = subscribe
          ? client.GetOrCreateMailbox("Trash", true)
          : client.GetOrCreateMailbox("Trash");

        StringAssert.EndsWith("LIST \"\" \"Trash\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"Trash\"\r\n", server.DequeueRequest());
        if (subscribe)
          StringAssert.EndsWith("SUBSCRIBE \"Trash\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" \"Trash\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Trash", mailbox.Name);
        Assert.AreEqual("Trash", mailbox.FullName);
        Assert.IsFalse(mailbox.IsInbox);
      });
    }

    [Test]
    public void TestGetMailboxes()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes();

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* LIST () \".\" INBOX.Child1\r\n" +
                                     "* LIST () \".\" INBOX.Child2\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.IsInbox);

        StringAssert.EndsWith("LIST \"\" \"*\"\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);
        Assert.IsTrue(enumerator.Current.IsInbox);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child1", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child1", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.IsInbox);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child2", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child2", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.IsInbox);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesNothingMatched()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes();

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesTopLevelOnly()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.TopLevelOnly);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" \"%\"\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesSubscribed()
    {
      GetMailboxesSubscribed(false);
    }

    [Test]
    public void TestGetMailboxesSubscribedMailboxReferralsCapable()
    {
      GetMailboxesSubscribed(true);
    }

    private void GetMailboxesSubscribed(bool mailboxReferralsCapable)
    {
      var capas = mailboxReferralsCapable
        ? new ImapCapability[] {ImapCapability.MailboxReferrals}
        : new ImapCapability[] {};

      TestUtils.TestAuthenticated(capas, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.SubscribedOnly | ImapMailboxListOptions.Remote);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LSUB/RLSUB
        server.EnqueueTaggedResponse("* LSUB () \".\" Trash\r\n" +
                                     "* LSUB () \".\" INBOX\r\n" +
                                     "* LSUB () \".\" INBOX.Child2\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        if (mailboxReferralsCapable)
          StringAssert.EndsWith("RLSUB \"\" \"*\"\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("LSUB \"\" \"*\"\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child2", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child2", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesSubscribedListExtendedCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended}, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.SubscribedOnly);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Marked \\NoInferiors \\Subscribed) \"/\" \"inbox\"\r\n" +
                                     "* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                                     "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.CanHaveChild);

        StringAssert.Contains("LIST (SUBSCRIBED) \"\" \"*\"", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Banana", enumerator.Current.Name);
        Assert.AreEqual("Fruit/Banana", enumerator.Current.FullName);
        Assert.IsTrue(enumerator.Current.Exists);
        Assert.IsTrue(enumerator.Current.CanHaveChild);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Peach", enumerator.Current.Name);
        Assert.AreEqual("Fruit/Peach", enumerator.Current.FullName);
        Assert.IsFalse(enumerator.Current.Exists);
        Assert.IsTrue(enumerator.Current.CanHaveChild);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRemoteListExtendedCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended}, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.Remote);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" \"Fruit/Banana\"\r\n" +
                                     "* LIST (\\Remote) \"/\" \"Bread\"\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Banana", enumerator.Current.Name);
        Assert.AreEqual("Fruit/Banana", enumerator.Current.FullName);

        StringAssert.Contains("LIST (REMOTE) \"\" \"*\"", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Bread", enumerator.Current.Name);
        Assert.AreEqual("Bread", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRemoteMailboxReferralsCapable()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.MailboxReferrals}, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.Remote);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // RLIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        StringAssert.EndsWith("RLIST \"\" \"*\"\r\n", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatus()
    {
      GetMailboxesRequestStatus(false);
    }

    [Test]
    public void TestGetMailboxesRequestStatusCondStoreCapable()
    {
      GetMailboxesRequestStatus(true);
    }

    private void GetMailboxesRequestStatus(bool condStoreCapable)
    {
      var capas = condStoreCapable
        ? new ImapCapability[] {ImapCapability.CondStore}
        : new ImapCapability[] {};
      var expectedStatusDataItems = condStoreCapable
        ? "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN HIGHESTMODSEQ)"
        : "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)";

      TestUtils.TestAuthenticated(capas, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* LIST () \".\" INBOX.Child\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS Trash (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1 HIGHESTMODSEQ 1)\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);
        Assert.AreEqual(3L, enumerator.Current.ExistMessageCount);
        Assert.AreEqual(1L, enumerator.Current.RecentMessageCount);
        Assert.AreEqual(4L, enumerator.Current.NextUid);
        Assert.AreEqual(1L, enumerator.Current.UidValidity);
        if (condStoreCapable)
          Assert.AreEqual(1L, enumerator.Current.HighestModSeq);

        StringAssert.EndsWith("LIST \"\" \"*\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith(string.Format("STATUS \"Trash\" {0}\r\n", expectedStatusDataItems), server.DequeueRequest());

        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        StringAssert.EndsWith(string.Format("STATUS \"INBOX\" {0}\r\n", expectedStatusDataItems), server.DequeueRequest());

        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX.Child ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child", enumerator.Current.FullName);

        StringAssert.EndsWith(string.Format("STATUS \"INBOX.Child\" {0}\r\n", expectedStatusDataItems), server.DequeueRequest());

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusNothingMatched()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusUnselectableMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" blurdybloop\r\n" +
                                     "* LIST (\\Noselect) \"/\" foo\r\n" +
                                     "* LIST () \"/\" foo/bar\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS blurdybloop ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("blurdybloop", enumerator.Current.Name);
        Assert.AreEqual("blurdybloop", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" \"*\"\r\n", server.DequeueRequest());
        StringAssert.Contains("STATUS \"blurdybloop\"", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("foo", enumerator.Current.Name);
        Assert.AreEqual("foo", enumerator.Current.FullName);
        Assert.IsTrue(enumerator.Current.IsUnselectable);

        // STATUS
        server.EnqueueTaggedResponse("* STATUS foo/bar ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("bar", enumerator.Current.Name);
        Assert.AreEqual("foo/bar", enumerator.Current.FullName);

        StringAssert.Contains("STATUS \"foo/bar\"", server.DequeueRequest());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusSelectedMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        /*
         * OpenInbox
         */
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        Assert.IsNotNull(client.OpenInbox());
        Assert.IsNotNull(client.OpenedMailbox);

        server.DequeueRequest(); // LIST
        server.DequeueRequest(); // SELECT

        /*
         * GetMailboxes
         */
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* LIST () \".\" INBOX.Child\r\n" +
                                     "$tag OK done\r\n");
        // STATUS
        server.EnqueueTaggedResponse("* STATUS Trash ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);

        StringAssert.EndsWith("LIST \"\" \"*\"\r\n", server.DequeueRequest());
        StringAssert.Contains("STATUS \"Trash\"", server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        // STATUS
        server.EnqueueTaggedResponse("* STATUS INBOX.Child ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child", enumerator.Current.FullName);

        StringAssert.Contains("STATUS \"INBOX.Child\"", server.DequeueRequest());

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestGetMailboxesRequestStatusListStatusCapable()
    {
      GetMailboxesRequestStatusListStatusCapable(false);
    }

    [Test]
    public void TestGetMailboxesRequestStatusListStatusCapableCondStoreCapable()
    {
      GetMailboxesRequestStatusListStatusCapable(true);
    }

    private void GetMailboxesRequestStatusListStatusCapable(bool condStoreCapable)
    {
      var capas = condStoreCapable
        ? new ImapCapability[] {ImapCapability.ListExtended, ImapCapability.ListStatus, ImapCapability.CondStore}
        : new ImapCapability[] {ImapCapability.ListExtended, ImapCapability.ListStatus};
      var expectedStatusDataItems = condStoreCapable
        ? "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN HIGHESTMODSEQ)"
        : "(MESSAGES RECENT UIDNEXT UIDVALIDITY UNSEEN)";

      TestUtils.TestAuthenticated(capas, delegate(ImapPseudoServer server, ImapClient client) {
        var mailboxes = client.GetMailboxes(ImapMailboxListOptions.RequestStatus);

        Assert.IsNotNull(mailboxes);

        var enumerator = mailboxes.GetEnumerator();

        Assert.IsNotNull(mailboxes);

        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" Trash\r\n" +
                                     "* STATUS Trash (MESSAGES 3 RECENT 1 UIDNEXT 4 UIDVALIDITY 1 UNSEEN 1 HIGHESTMODSEQ 1)\r\n" +
                                     "* LIST () \".\" INBOX\r\n" +
                                     "* STATUS INBOX ()\r\n" +
                                     "* LIST () \".\" INBOX.Child\r\n" +
                                     "* STATUS INBOX.Child ()\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Trash", enumerator.Current.Name);
        Assert.AreEqual("Trash", enumerator.Current.FullName);
        Assert.AreEqual(3L, enumerator.Current.ExistMessageCount);
        Assert.AreEqual(1L, enumerator.Current.RecentMessageCount);
        Assert.AreEqual(4L, enumerator.Current.NextUid);
        Assert.AreEqual(1L, enumerator.Current.UidValidity);
        Assert.AreEqual(1L, enumerator.Current.UnseenMessageCount);
        if (condStoreCapable)
          Assert.AreEqual(1L, enumerator.Current.HighestModSeq);

        StringAssert.EndsWith(string.Format("LIST () \"\" \"*\" RETURN (CHILDREN STATUS {0})\r\n", expectedStatusDataItems),
                              server.DequeueRequest());

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("INBOX", enumerator.Current.Name);
        Assert.AreEqual("INBOX", enumerator.Current.FullName);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(enumerator.Current.Client, client);
        Assert.AreEqual("Child", enumerator.Current.Name);
        Assert.AreEqual("INBOX.Child", enumerator.Current.FullName);

        Assert.IsFalse(enumerator.MoveNext());
      });
    }

    [Test]
    public void TestCreateMailbox()
    {
      CreateMailbox(false);
    }

    [Test]
    public void TestCreateMailboxSubscribe()
    {
      CreateMailbox(true);
    }

    private void CreateMailbox(bool subscribe)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // CREATE
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        if (subscribe)
          // SUBSCRIBE
          server.EnqueueTaggedResponse("$tag OK done\r\n");
        // LIST
        server.EnqueueTaggedResponse("* LIST () \".\" INBOX.Recent\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = subscribe
          ? client.CreateMailbox("INBOX.Recent", true)
          : client.CreateMailbox("INBOX.Recent");

        StringAssert.EndsWith("CREATE \"INBOX.Recent\"\r\n", server.DequeueRequest());
        if (subscribe)
          StringAssert.EndsWith("SUBSCRIBE \"INBOX.Recent\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("LIST \"\" \"INBOX.Recent\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Recent", mailbox.Name);
        Assert.AreEqual("INBOX.Recent", mailbox.FullName);
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestCreateMailboxContainsWildcard1()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.CreateMailbox("INBOX.*");
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestCreateMailboxContainsWildcard2()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.CreateMailbox("INBOX.%");
      });
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestCreateMailboxEmptyName()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        client.CreateMailbox(string.Empty);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestCreateMailboxAlreadyExists()
    {
    }

    [Test]
    public void TestOpenInbox()
    {
      OpenInbox(false);
    }

    [Test]
    public void TestOpenInboxReadOnly()
    {
      OpenInbox(true);
    }

    private void OpenInbox(bool readOnly)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (readOnly)
          server.EnqueueTaggedResponse("* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = readOnly
          ? client.OpenInbox(true)
          : client.OpenInbox();

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());

        if (readOnly)
          StringAssert.EndsWith("EXAMINE \"INBOX\"\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);

        if (readOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailbox()
    {
      OpenMailbox(false);
    }

    [Test]
    public void TestOpenMailboxReadOnly()
    {
      OpenMailbox(true);
    }

    private void OpenMailbox(bool readOnly)
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" Draft\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (readOnly)
          server.EnqueueTaggedResponse("* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = readOnly
          ? client.OpenMailbox("Draft", true)
          : client.OpenMailbox("Draft");

        StringAssert.EndsWith("LIST \"\" \"Draft\"\r\n", server.DequeueRequest());

        if (readOnly)
          StringAssert.EndsWith("EXAMINE \"Draft\"\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT \"Draft\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("Draft", mailbox.Name);
        Assert.AreEqual("Draft", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);

        if (readOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailboxCondStoreCapable()
    {
      OpenMailboxCondStore(false);
    }

    [Test]
    public void TestOpenMailboxCondStoreCapableReadOnly()
    {
      OpenMailboxCondStore(true);
    }

    private void OpenMailboxCondStore(bool readOnly)
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.CondStore}, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT/EXAMINE
        if (readOnly)
          server.EnqueueTaggedResponse("* OK [HIGHESTMODSEQ 1]\r\n" +
                                       "* OK [READ-ONLY]\r\n" +
                                       "$tag OK done\r\n");
        else
          server.EnqueueTaggedResponse("* OK [HIGHESTMODSEQ 1]\r\n" +
                                       "* OK [READ-WRITE]\r\n" +
                                       "$tag OK done\r\n");

        var mailbox = readOnly
          ? client.OpenMailbox("INBOX", true)
          : client.OpenMailbox("INBOX");

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());

        if (readOnly)
          StringAssert.EndsWith("EXAMINE \"INBOX\" (CONDSTORE)\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("SELECT \"INBOX\" (CONDSTORE)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsTrue(mailbox.IsModSequencesAvailable);

        if (readOnly)
          Assert.IsTrue(mailbox.IsReadOnly);
        else
          Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailboxCondStoreCapableNoModSeq()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.CondStore}, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("* OK [NOMODSEQ]\r\n" +
                                     "* OK [READ-WRITE]\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.OpenMailbox("INBOX");

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"INBOX\" (CONDSTORE)\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);
        Assert.IsFalse(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenMailboxNonExistent()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.ListExtended}, delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Subscribed \\NonExistent) \"/\" \"INBOX/Child\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("INBOX/Child", ImapMailboxListOptions.SubscribedOnly);

        StringAssert.Contains("LIST (SUBSCRIBED) \"\" \"INBOX/Child\"", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Child", mailbox.Name);
        Assert.AreEqual("INBOX/Child", mailbox.FullName);
        Assert.IsFalse(mailbox.Exists);

        TestUtils.ExpectExceptionThrown<ImapProtocolViolationException>(delegate {
          client.OpenMailbox(mailbox);
        });
      });
    }

    [Test]
    public void TestOpenMailboxUnselectable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("[Gmail]");

        StringAssert.EndsWith("LIST \"\" \"[Gmail]\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("[Gmail]", mailbox.Name);
        Assert.AreEqual("[Gmail]", mailbox.FullName);
        Assert.IsTrue(mailbox.IsUnselectable);

        TestUtils.ExpectExceptionThrown<ImapProtocolViolationException>(delegate {
          client.OpenMailbox(mailbox);
        });

        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestOpenMailboxDeleted()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"/\" \"INBOX/Child\"\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.GetMailbox("INBOX/Child");

        StringAssert.EndsWith("LIST \"\" \"INBOX/Child\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.AreEqual("Child", mailbox.Name);
        Assert.AreEqual("INBOX/Child", mailbox.FullName);
        Assert.IsTrue(mailbox.Exists);

        // DELETE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Delete();

        TestUtils.ExpectExceptionThrown<ImapProtocolViolationException>(delegate {
          client.OpenMailbox(mailbox);
        });

        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestOpenMailboxReopenCurrentlySelected()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestOpenMailboxOpenNewly()
    {
    }

    [Test]
    public void TestOpenMailboxError()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag NO unselectable\r\n");

        Assert.IsNull(client.OpenedMailbox);

        TestUtils.ExpectExceptionThrown<ImapErrorResponseException>(delegate {
          client.OpenMailbox("INBOX");
        });

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());

        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestCloseMailbox()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        var mailbox = client.OpenInbox();

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.CloseMailbox();

        Assert.IsNull(client.OpenedMailbox);
        Assert.IsFalse(mailbox.IsOpen);
      });
    }

    [Test]
    public void TestCloseMailboxNotOpened()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        Assert.IsNull(client.OpenedMailbox);

        client.CloseMailbox();

        Assert.IsNull(client.OpenedMailbox);

        client.CloseMailbox();
      });
    }


    [Test]
    public void TestGetQuota()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        var quota = client.GetQuota(string.Empty);

        Assert.That(server.DequeueRequest(), Text.EndsWith("GETQUOTA \"\"\r\n"));

        Assert.IsNotNull(quota);
        Assert.AreEqual(string.Empty, quota.Root);
        Assert.AreEqual(1, quota.Resources.Length);
        Assert.AreEqual("STORAGE", quota.Resources[0].Name);
        Assert.AreEqual(10, quota.Resources[0].Usage);
        Assert.AreEqual(512, quota.Resources[0].Limit);
      });
    }

    [Test]
    public void TestGetQuotaQuotaIncapable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var quota = client.GetQuota(string.Empty);

        Assert.IsNull(quota);
      });
    }

    [Test]
    public void TestGetQuotaUsage()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        var usage = client.GetQuotaUsage(string.Empty, "STORAGE");

        Assert.That(server.DequeueRequest(), Text.EndsWith("GETQUOTA \"\"\r\n"));

        Assert.AreEqual(10.0 / 512.0, usage);
      });
    }

    [Test, ExpectedException(typeof(ImapErrorResponseException))]
    public void TestGetQuotaUsageNoSuchQuotaRoot()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("$tag NO no such quota root\r\n");

        client.GetQuotaUsage("NON-EXISTENT", "STORAGE");
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestGetQuotaUsageNoSuchResourceName()
    {
      TestUtils.TestAuthenticated(new[] {ImapCapability.Quota}, delegate(ImapPseudoServer server, ImapClient client) {
        // GETQUOTA
        server.EnqueueTaggedResponse("* QUOTA \"\" (STORAGE 10 512)\r\n" +
                                     "$tag OK done\r\n");

        client.GetQuotaUsage(string.Empty, "NON-EXITENT");
      });
    }

    [Test]
    public void TestGetQuotaUsageQuotaIncapable()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        var usage = client.GetQuotaUsage(string.Empty, "STORAGE");

        Assert.AreEqual(0.0, usage);
      });
    }

    [Test]
    public void TestRefresh()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());
      });
    }

    [Test]
    public void TestRefreshMailboxOpened()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        server.EnqueueTaggedResponse("* EXISTS 10\r\n" +
                                     "* RECENT 3\r\n" +
                                     "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                                     "* OK [READ-WRITE]\r\n" +
                                     "$tag OK done\r\n");

        var mailbox = client.OpenInbox();

        StringAssert.EndsWith("LIST \"\" \"INBOX\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());

        Assert.IsNotNull(mailbox);
        Assert.IsNotNull(client.OpenedMailbox);
        Assert.AreSame(mailbox, client.OpenedMailbox);
        Assert.AreSame(mailbox.Client, client);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.AreEqual("INBOX", mailbox.Name);
        Assert.AreEqual("INBOX", mailbox.FullName);
        Assert.IsFalse(mailbox.IsReadOnly);
        Assert.AreEqual(10L, mailbox.ExistMessageCount);
        Assert.AreEqual(3L, mailbox.RecentMessageCount);
        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(5, mailbox.ApplicableFlags.Count);
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Draft));

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 3 (UID 3 FLAGS ())\r\n" +
                                     "$tag OK done\r\n");

        var message = mailbox.GetMessageByUid(3L, ImapMessageFetchAttributeOptions.DynamicAttributes);

        StringAssert.EndsWith("UID FETCH 3 (UID FLAGS)\r\n", server.DequeueRequest());

        Assert.AreEqual(3L, message.Sequence);
        Assert.AreEqual(3L, message.Uid);
        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(0, message.Flags.Count);
        Assert.IsFalse(message.IsSeen);

        // NOOP
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 5\r\n" +
                                     "* EXISTS 8\r\n" +
                                     "* RECENT 4\r\n" +
                                     "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft $label1)\r\n" +
                                     "* FETCH 2 (FLAGS (\\Seen \\Deleted $label1))\r\n" +
                                     "$tag OK done\r\n");

        client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.AreEqual(8L, mailbox.ExistMessageCount);
        Assert.AreEqual(4L, mailbox.RecentMessageCount);
        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(6, mailbox.ApplicableFlags.Count);
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Answered));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(mailbox.ApplicableFlags.Has(ImapMessageFlag.Draft));
        Assert.IsTrue(mailbox.ApplicableFlags.Has("$label1"));

        Assert.AreEqual(2L, message.Sequence);
        Assert.AreEqual(3L, message.Uid);
        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(3, message.Flags.Count);
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(message.Flags.Has("$label1"));
        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.IsMarkedAsDeleted);
      });
    }

    [Test, ExpectedException(typeof(Smdn.Net.Imap4.Protocol.ImapConnectionException))]
    public void TestRefreshBye()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        server.EnqueueResponse("* BYE\r\n");
        server.Stop();

        client.Refresh();
      });
    }

    [Test, ExpectedException(typeof(Smdn.Net.Imap4.Protocol.ImapConnectionException))]
    public void TestRefreshDisconnected()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        server.Stop();

        client.Refresh();
      });
    }

    [Test]
    public void TestEventExistMessageCountChanged()
    {
      var selectResp =
        "* EXISTS 10\r\n" +
        "* RECENT 3\r\n" +
        "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(10L, mailbox.ExistMessageCount);

        var eventRaised = false;

        mailbox.Client.ExistMessageCountChanged += delegate(object sender, ImapMailboxSizeChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(mailbox, e.Mailbox);
          Assert.AreEqual(10L, e.PrevCount);
          Assert.AreEqual(12L, e.CurrentCount);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* EXISTS 12\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(12L, mailbox.ExistMessageCount);
      });
    }

    [Test]
    public void TestEventRecentMessageCountChanged()
    {
      var selectResp =
        "* EXISTS 10\r\n" +
        "* RECENT 3\r\n" +
        "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.RecentMessageCount);

        var eventRaised = false;

        mailbox.Client.RecentMessageCountChanged += delegate(object sender, ImapMailboxSizeChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(mailbox, e.Mailbox);
          Assert.AreEqual(3L, e.PrevCount);
          Assert.AreEqual(4L, e.CurrentCount);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* RECENT 4\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);
        Assert.AreEqual(4L, mailbox.RecentMessageCount);
      });
    }

    [Test]
    public void TestEventMessageStatusChanged()
    {
      var selectResp =
        "* EXISTS 3\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 3 (UID 3 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes).ToArray();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());
        StringAssert.EndsWith("FETCH 1:3 (UID FLAGS)\r\n", server.DequeueRequest());

        Assert.IsTrue (messages[0].IsSeen);
        Assert.IsFalse(messages[0].IsMarkedAsDeleted);
        Assert.IsTrue (messages[1].IsSeen);
        Assert.IsFalse(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue (messages[2].IsSeen);
        Assert.IsFalse(messages[2].IsMarkedAsDeleted);

        var eventRaised = false;

        mailbox.Client.MessageStatusChanged += delegate(object sender, ImapMessageStatusChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(2, e.Messages.Length);

          Assert.AreEqual(1L, e.Messages[0].Sequence);
          Assert.IsTrue(e.Messages[0].IsSeen);
          Assert.IsTrue(e.Messages[0].IsMarkedAsDeleted);

          Assert.AreEqual(3L, e.Messages[1].Sequence);
          Assert.IsFalse(e.Messages[1].IsSeen);
          Assert.IsFalse(e.Messages[1].IsMarkedAsDeleted);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted))\r\n" +
                                     "* FETCH 3 (FLAGS ())\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);

        Assert.IsTrue (messages[0].IsSeen);
        Assert.IsTrue (messages[0].IsMarkedAsDeleted);
        Assert.IsTrue (messages[1].IsSeen);
        Assert.IsFalse(messages[1].IsMarkedAsDeleted);
        Assert.IsFalse(messages[2].IsSeen);
        Assert.IsFalse(messages[2].IsMarkedAsDeleted);
      });
    }

    [Test]
    public void TestEventMessageDeleted()
    {
      var selectResp =
        "* EXISTS 3\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
                                     "* FETCH 3 (UID 3 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes).ToArray();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());
        StringAssert.EndsWith("FETCH 1:3 (UID FLAGS)\r\n", server.DequeueRequest());

        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsFalse(messages[0].IsDeleted);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsFalse(messages[0].IsDeleted);
        Assert.AreEqual(3L, messages[2].Sequence);
        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsFalse(messages[0].IsDeleted);

        var eventRaised = false;

        mailbox.Client.MessageDeleted += delegate(object sender, ImapMessageStatusChangedEventArgs e) {
          Assert.AreSame(mailbox.Client, sender);
          Assert.AreEqual(2, e.Messages.Length);

          Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, e.Messages[0].Sequence);
          Assert.AreEqual(3L, e.Messages[0].Uid);
          Assert.IsTrue(e.Messages[0].IsDeleted);

          Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, e.Messages[1].Sequence);
          Assert.AreEqual(1L, e.Messages[1].Uid);
          Assert.IsTrue(e.Messages[1].IsDeleted);

          eventRaised = true;
        };

        // NOOP
        server.EnqueueTaggedResponse("* EXPUNGE 3\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Client.Refresh();

        StringAssert.EndsWith("NOOP\r\n", server.DequeueRequest());

        Assert.IsTrue(eventRaised);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[0].Sequence);
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsTrue(messages[0].IsDeleted);
        Assert.AreEqual(1L, messages[1].Sequence);
        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsFalse(messages[1].IsDeleted);
        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[2].Sequence);
        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsTrue(messages[2].IsDeleted);
      });
    }
  }
}
