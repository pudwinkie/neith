using System;
using System.IO;
using System.Net;
using NUnit.Framework;

using Smdn.Net.Pop3.Protocol;
using Smdn.Net.Pop3.Protocol.Client;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Pop3.Client.Session {
  [TestFixture]
  public class PopSessionCommandsAuthorizationStateTests : PopSessionTestsBase {
    [Test]
    public void TestStls()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK Begin TLS negotiation\r\n");

        var prevAuthority = new Uri(session.Authority.ToString());

        Assert.IsFalse(session.IsSecureConnection);

        var streamUpgraded = false;

        Assert.IsTrue((bool)session.Stls(delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SslStream
        }));

        Assert.IsTrue(streamUpgraded, "stream upgraded");
        Assert.IsTrue(session.IsSecureConnection, "IsSecureConnection");

        StringAssert.AreEqualIgnoringCase("STLS\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(prevAuthority, session.Authority);
      });
    }

    [Test]
    public void TestStlsExceptionWhileUpgrading()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK Begin TLS negotiation\r\n");

        Assert.IsFalse(session.IsSecureConnection);

        try {
          Assert.IsTrue((bool)session.Stls(delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          }));
          Assert.Fail("PopUpgradeConnectionException not thrown");
        }
        catch (PopUpgradeConnectionException) {
        }

        Assert.IsNull(session.Authority);
        Assert.AreEqual(PopSessionState.NotConnected, session.State);
      });
    }

    [Test]
    public void TestStlsError()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("-ERR Command not permitted when TLS active\r\n");

        var prevAuthority = new Uri(session.Authority.ToString());

        Assert.IsFalse(session.IsSecureConnection);

        var streamUpgraded = false;

        Assert.IsFalse((bool)session.Stls(delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SslStream
        }));

        Assert.IsFalse(streamUpgraded, "stream upgraded");

        StringAssert.AreEqualIgnoringCase("STLS\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(prevAuthority, session.Authority);
        Assert.IsFalse(session.IsSecureConnection, "IsSecureConnection");
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestStlsIncapable()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK List of capabilities follows\r\n" +
                               ".\r\n");

        Assert.IsTrue((bool)session.Capa());
        Assert.IsFalse(session.ServerCapabilities.IsCapable(PopCapability.Stls));

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());

        Assert.IsFalse(session.IsSecureConnection);

        var streamUpgraded = false;

        session.HandlesIncapableAsException = true;
        session.Stls(delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream;
        });

        Assert.IsFalse(streamUpgraded, "stream upgraded");
        Assert.IsFalse(session.IsSecureConnection, "IsSecureConnection");
      });
    }

    [Test]
    public void TestUser()
    {
      Connect(delegate(PopSession session, PopPseudoServer server, Uri expectedAuthority) {
        server.EnqueueResponse("-ERR sorry, no mailbox for frated here\r\n");

        Assert.IsFalse((bool)session.User("frated"));
        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);

        StringAssert.AreEqualIgnoringCase("USER frated\r\n",
                                          server.DequeueRequest());

        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");

        Assert.IsTrue((bool)session.User("mrose"));

        StringAssert.AreEqualIgnoringCase("USER mrose\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);
      });
    }

    [Test]
    public void TestUserCredentialNotFound()
    {
      Connect(delegate(PopSession session) {
        Assert.IsFalse((bool)session.User(new NullCredential()));
      });
    }

    [Test]
    public void TestUserNull()
    {
      Connect(delegate(PopSession session) {
        Assert.IsFalse((bool)session.User((string)null));
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestUserIncapable()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK List of capabilities follows\r\n" +
                               ".\r\n");

        Assert.IsTrue((bool)session.Capa());
        Assert.IsFalse(session.ServerCapabilities.IsCapable(PopCapability.User));

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());

        session.HandlesIncapableAsException = true;
        session.User("user");
      });
    }

    [Test]
    public void TestPass()
    {
      Connect(delegate(PopSession session, PopPseudoServer server, Uri expectedAuthority) {
        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");
        server.EnqueueResponse("-ERR maildrop already locked\r\n");

        Assert.IsTrue((bool)session.User("mrose"));
        Assert.IsFalse((bool)session.Pass("secret"));

        StringAssert.AreEqualIgnoringCase("USER mrose\r\n",
                                          server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase("PASS secret\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);

        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");
        server.EnqueueResponse("+OK mrose's maildrop has 2 messages (320 octets)\r\n");

        Assert.IsTrue((bool)session.User("mrose"));
        Assert.IsTrue((bool)session.Pass("secret"));

        StringAssert.AreEqualIgnoringCase("USER mrose\r\n",
                                          server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase("PASS secret\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);
      });
    }

    [Test]
    public void TestPassCredentialNotFound()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");

        Assert.IsTrue((bool)session.User("mrose"));

        Assert.IsFalse((bool)session.Pass(new NullCredential()));
      });
    }

    [Test]
    public void TestPassNull()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");

        Assert.IsTrue((bool)session.User("mrose"));

        Assert.IsFalse((bool)session.Pass((string)null));
      });
    }

    [Test, ExpectedException(typeof(PopProtocolViolationException))]
    public void TestPassBeforeUserCommandIssued()
    {
      Connect(delegate(PopSession session) {
        session.Pass("secret");
      });
    }

    [Ignore("can't test"), Test, ExpectedException(typeof(PopIncapableException))]
    public void TestPassIncapable()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK List of capabilities follows\r\n" +
                               ".\r\n");

        Assert.IsTrue((bool)session.Capa());
        Assert.IsFalse(session.ServerCapabilities.IsCapable(PopCapability.User));

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());

        session.HandlesIncapableAsException = true;

        var credential = new NetworkCredential("user", "pass");

        session.User(credential); // USER must be issued first
        session.Pass(credential); // throwing PopIncapableException is expected, but it throwed while USER command
      });
    }

    [Test]
    public void TestLogin()
    {
      Connect(delegate(PopSession session, PopPseudoServer server, Uri expectedAuthority) {
        server.EnqueueResponse("-ERR sorry, no mailbox for frated here\r\n");

        Assert.IsFalse((bool)session.Login("mrose", "secret"));

        StringAssert.AreEqualIgnoringCase("USER mrose\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);

        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");
        server.EnqueueResponse("-ERR maildrop already locked\r\n");

        Assert.IsFalse((bool)session.Login("mrose", "secret"));

        StringAssert.AreEqualIgnoringCase("USER mrose\r\n",
                                          server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase("PASS secret\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);

        server.EnqueueResponse("+OK mrose is a real hoopy frood\r\n");
        server.EnqueueResponse("+OK mrose's maildrop has 2 messages (320 octets)\r\n");

        Assert.IsTrue((bool)session.Login("mrose", "secret"));

        StringAssert.AreEqualIgnoringCase("USER mrose\r\n",
                                          server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase("PASS secret\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);
      });
    }

    [Test]
    public void TestLoginWithNetworkCredential()
    {
      Connect(delegate(PopSession session, PopPseudoServer server, Uri expectedAuthority) {
        server.EnqueueResponse("+OK\r\n");
        server.EnqueueResponse("+OK 2 messages (320 octets)\r\n");

        var credential = new NetworkCredential("user", "pass");

        Assert.IsTrue((bool)session.Login(credential));

        StringAssert.AreEqualIgnoringCase(string.Format("USER {0}\r\n", credential.UserName),
                                          server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase(string.Format("PASS {0}\r\n", credential.Password),
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, session.State);
        Assert.AreEqual(expectedAuthority, session.Authority);
      });
    }

    [Test]
    public void TestLoginCredentialNotFound()
    {
      Connect(delegate(PopSession session) {
        Assert.IsFalse((bool)session.Login(new NullCredential()));
        Assert.IsFalse((bool)session.Login(new NullCredential(), "user"));
      });
    }

    [Test]
    public void TestLoginCredentialUsernameNull()
    {
      Connect(delegate(PopSession session) {
        Assert.IsFalse((bool)session.Login(new NetworkCredential(null, "pass")));
      });
    }

    [Test]
    public void TestLoginCredentialPasswordNull()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK\r\n");

        Assert.IsFalse((bool)session.Login(new NetworkCredential("user", (string)null)));

        StringAssert.AreEqualIgnoringCase("USER user\r\n",
                                          server.DequeueRequest());
      });
    }

    [Test]
    public void TestLoginSelectAppropriateCredentialUsernameSpecified()
    {
      LoginSelectAppropriateCredential(true);
    }

    [Test]
    public void TestLoginSelectAppropriateCredentialUsernameNotSpecified()
    {
      LoginSelectAppropriateCredential(false);
    }

    private void LoginSelectAppropriateCredential(bool specifyUsername)
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK done\r\n");
        server.EnqueueResponse("+OK done\r\n");

        var credentials = new CredentialCache();

        credentials.Add("pop.example.net", 110, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        server.EnqueueResponse("0000 OK authenticated\r\n");

        if (specifyUsername)
          Assert.IsTrue((bool)session.Login(credentials, "user"));
        else
          Assert.IsTrue((bool)session.Login(credentials));

        Assert.AreEqual("USER user\r\n", server.DequeueRequest());
        Assert.AreEqual("PASS pass4\r\n", server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, session.State);
        Assert.AreEqual(new Uri(string.Format("pop://user@{0}:{1}/", server.Host, server.Port)),
                        session.Authority);
      });
    }

    [Test]
    public void TestLoginSelectAppropriateCredentialNotFound()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        var credentials = new CredentialCache();

        credentials.Add("imap.example.net", 143, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        var result = session.Login(credentials, "xxxx");

        Assert.IsFalse((bool)result);
        Assert.AreEqual(PopCommandResultCode.RequestError, result.Code);
        Assert.AreEqual(PopSessionState.Authorization, session.State);
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestApopIncapable()
    {
      Connect(delegate(PopSession session) {
        session.HandlesIncapableAsException = true;

        Assert.IsFalse(session.ApopAvailable);

        var credential = new NetworkCredential("user", "pass");

        session.Apop(credential, "user");
      });
    }

    [Test]
    public void TestApop()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

        using (var session = new PopSession(server.Host, server.Port)) {
          Assert.IsTrue(session.ApopAvailable);

          server.EnqueueResponse("+OK maildrop has 1 message (369 octets)\r\n");

          Assert.IsTrue((bool)session.Apop(new NetworkCredential("mrose", "tanstaaf")));

          StringAssert.AreEqualIgnoringCase("APOP mrose c4c9334bac560ecc979e58001b3e22fb\r\n",
                                            server.DequeueRequest());

          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(new Uri(string.Format("pop://mrose;AUTH=+APOP@{0}:{1}/", server.Host, server.Port)),
                          session.Authority);
        }
      }
    }

    [Test]
    public void TestApopNoWithInUseResponseCode()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

        using (var session = new PopSession(server.Host, server.Port)) {
          Assert.IsTrue(session.ApopAvailable);

          server.EnqueueResponse("-ERR [IN-USE] Do you have another POP session running?\r\n");

          var result = session.Apop(new NetworkCredential("mrose", "tanstaaf"));

          Assert.IsTrue(result.Failed);
          Assert.IsNotNull(result.GetResponseCode(PopResponseCode.InUse));

          StringAssert.AreEqualIgnoringCase("APOP mrose c4c9334bac560ecc979e58001b3e22fb\r\n",
                                            server.DequeueRequest());

          Assert.AreEqual(PopSessionState.Authorization, session.State);
          Assert.AreEqual(new Uri(string.Format("pop://{0}:{1}/", server.Host, server.Port)),
                          session.Authority);
        }
      }
    }

    [Test]
    public void TestApopCredentialNotFound()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

        using (var session = new PopSession(server.Host, server.Port)) {
          Assert.IsTrue(session.ApopAvailable);

          Assert.IsFalse((bool)session.Apop(new NullCredential()));
          Assert.IsFalse((bool)session.Apop(new NullCredential(),"user"));
        }
      }
    }

    [Test]
    public void TestApopCredentialUsernameNull()
    {
      ApopCredentialPropertyNull(new NetworkCredential(null, "password"));
    }

    [Test]
    public void TestApopCredentialPasswordNull()
    {
      ApopCredentialPropertyNull(new NetworkCredential("username", (string)null));
    }

    private void ApopCredentialPropertyNull(ICredentialsByHost credential)
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

        using (var session = new PopSession(server.Host, server.Port)) {
          Assert.IsFalse((bool)session.Apop(credential));
        }
      }
    }

    [Test]
    public void TestApopSelectAppropriateCredentialUsernameSpecified()
    {
      ApopSelectAppropriateCredential(true);
    }

    [Test]
    public void TestApopSelectAppropriateCredentialUsernameNotSpecified()
    {
      ApopSelectAppropriateCredential(false);
    }

    private void ApopSelectAppropriateCredential(bool specifyUsername)
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK POP3 server ready <timestamp>\r\n");

        using (var session = new PopSession(server.Host, server.Port)) {
          server.EnqueueResponse("+OK done\r\n");

          var credentials = new CredentialCache();

          credentials.Add("pop.example.net", 110, "LOGIN", new NetworkCredential("user", "pass1"));
          credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
          credentials.Add(server.Host, server.Port, "+APOP", new NetworkCredential("user", "pass3"));
          credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

          server.EnqueueResponse("+OK done\r\n");

          if (specifyUsername)
            Assert.IsTrue((bool)session.Apop(credentials, "user"));
          else
            Assert.IsTrue((bool)session.Apop(credentials));

          StringAssert.AreEqualIgnoringCase("APOP user 80455cef894e28a3589d57c72e788afd\r\n",
                                            server.DequeueRequest());

          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(new Uri(string.Format("pop://user;+APOP@{0}:{1}/", server.Host, server.Port)),
                          session.Authority);
        }
      }
    }

    [Test]
    public void TestApopSelectAppropriateCredentialNotFound()
    {
      using (var server = CreateServer()) {
        server.EnqueueResponse("+OK POP3 server ready <1896.697170952@dbc.mtview.ca.us>\r\n");

        using (var session = new PopSession(server.Host, server.Port)) {
          var credentials = new CredentialCache();

          credentials.Add("pop.example.net", 110, "LOGIN", new NetworkCredential("user", "pass1"));
          credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
          credentials.Add(server.Host, server.Port, "+APOP", new NetworkCredential("user", "pass3"));
          credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

          var result = session.Apop(credentials, "xxxx");

          Assert.IsFalse((bool)result);
          Assert.AreEqual(PopCommandResultCode.RequestError, result.Code);
          Assert.AreEqual(PopSessionState.Authorization, session.State);
        }
      }
    }

    [Test]
    public void TestAuth()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK Maildrop locked and ready\r\n");

        Assert.IsTrue((bool)session.Auth(new NetworkCredential("test", "test", "test"),
                                         PopAuthenticationMechanism.Plain));

        StringAssert.AreEqualIgnoringCase("AUTH PLAIN dGVzdAB0ZXN0AHRlc3Q=\r\n",
                                          server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, session.State);
        Assert.AreEqual(new Uri(string.Format("pop://test;AUTH=PLAIN@{0}:{1}/", server.Host, server.Port)),
                        session.Authority);
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestAuthIncapable()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK List of capabilities follows\r\n" +
                               ".\r\n");

        Assert.IsTrue((bool)session.Capa());
        Assert.IsFalse(session.ServerCapabilities.IsCapable(PopCapability.Sasl));

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());

        session.HandlesIncapableAsException = true;

        var credential = new NetworkCredential("user", "pass");

        session.Auth(credential, PopAuthenticationMechanism.DigestMD5);
      });
    }

    [Test, ExpectedException(typeof(PopIncapableException))]
    public void TestAuthIncapableMechanism()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+OK List of capabilities follows\r\n" +
                               "SASL PLAIN DIGEST-MD5\r\n" + 
                               ".\r\n");

        Assert.IsTrue((bool)session.Capa());
        Assert.IsFalse(session.ServerCapabilities.IsCapable(PopAuthenticationMechanism.CRAMMD5));

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());

        server.EnqueueResponse("-ERR\r\n");

        session.HandlesIncapableAsException = true;

        var credential = new NetworkCredential("user", "pass");

        session.Auth(credential, PopAuthenticationMechanism.CRAMMD5);
      });
    }

    [Test]
    public void TestAuthInvalidResponse()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+ xxxxx-invalid-response-xxxxx\r\n");

        try {
          session.Auth(new NetworkCredential("test", "test", "test"),
                       PopAuthenticationMechanism.DigestMD5);
          Assert.Fail("PopException not thrown");
        }
        catch (PopException) {
        }

        Assert.AreEqual(PopSessionState.NotConnected, session.State);
      });
    }

    [Test]
    public void TestAuthCancelExchanging()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+ eD0xLHk9Mix6PTM=\r\n"); // x=1,y=2,z=3
        server.EnqueueResponse("-ERR AUTH failed.\r\n");

        session.Auth(new NetworkCredential("test", "test", "test"),
                     PopAuthenticationMechanism.DigestMD5);

        StringAssert.AreEqualIgnoringCase("AUTH DIGEST-MD5\r\n", server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase("*\r\n", server.DequeueRequest());
      });
    }

    [Test]
    public void TestAuthCredentialNotFound()
    {
      Connect(delegate(PopSession session) {
        Assert.IsFalse((bool)session.Auth(new NullCredential(), PopAuthenticationMechanism.DigestMD5));
        Assert.IsFalse((bool)session.Auth(new NullCredential(),"user", PopAuthenticationMechanism.DigestMD5));
      });
    }

    [Test]
    public void TestAuthCredentialUsernameNullInitialClientResponse()
    {
      AuthCredentialPropertyNullInitialClientResponse(new NetworkCredential(null, "password"));
    }

    [Test]
    public void TestAuthCredentialPasswordNullInitialClientResponse()
    {
      AuthCredentialPropertyNullInitialClientResponse(new NetworkCredential("username", (string)null));
    }

    private void AuthCredentialPropertyNullInitialClientResponse(ICredentialsByHost credential)
    {
      Connect(delegate(PopSession session) {
        Assert.IsFalse((bool)session.Auth(credential,
                                          PopAuthenticationMechanism.Plain));
      });
    }

    [Test]
    public void TestAuthCredentialUsernameNull()
    {
      AuthCredentialPropertyNull(new NetworkCredential(null, "password"));
    }

    [Test]
    public void TestAuthCredentialPasswordNull()
    {
      AuthCredentialPropertyNull(new NetworkCredential("username", (string)null));
    }

    private void AuthCredentialPropertyNull(ICredentialsByHost credential)
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+ PDE4OTYuNjk3MTcwOTUyQHBvc3RvZmZpY2UucmVzdG9uLm1jaS5uZXQ+\r\n");
        server.EnqueueResponse("-ERR\r\n");

        Assert.IsFalse((bool)session.Auth(credential,
                                          PopAuthenticationMechanism.CRAMMD5));

        StringAssert.AreEqualIgnoringCase("AUTH CRAM-MD5\r\n", server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase("*\r\n", server.DequeueRequest());
      });
    }

    [Test]
    public void TestAuthSelectAppropriateCredentialUsernameSpecified()
    {
      AuthSelectAppropriateCredential(true);
    }

    [Test]
    public void TestAuthSelectAppropriateCredentialUsernameNotSpecified()
    {
      AuthSelectAppropriateCredential(false);
    }

    private void AuthSelectAppropriateCredential(bool specifyUsername)
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+OK done\r\n");

        var credentials = new CredentialCache();

        credentials.Add("pop.example.net", 110, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        server.EnqueueResponse("0000 OK authenticated\r\n");

        if (specifyUsername)
          Assert.IsTrue((bool)session.Auth(credentials, "user", PopAuthenticationMechanism.Login));
        else
          Assert.IsTrue((bool)session.Auth(credentials, PopAuthenticationMechanism.Login));

        Assert.AreEqual(string.Format("AUTH LOGIN {0}\r\n", Convert.ToBase64String(NetworkTransferEncoding.Transfer7Bit.GetBytes("user"))),
                        server.DequeueRequest());

        Assert.AreEqual(string.Format("{0}\r\n", Convert.ToBase64String(NetworkTransferEncoding.Transfer7Bit.GetBytes("pass2"))),
                        server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, session.State);
        Assert.AreEqual(new Uri(string.Format("pop://user;AUTH=PLAIN@{0}:{1}/", server.Host, server.Port)),
                        session.Authority);
      });
    }

    [Test]
    public void TestAuthSelectAppropriateCredentialNotFound()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        var credentials = new CredentialCache();

        credentials.Add("imap.example.net", 143, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        var result = session.Auth(credentials, "xxxx", PopAuthenticationMechanism.Login);

        Assert.IsFalse((bool)result);
        Assert.AreEqual(PopCommandResultCode.RequestError, result.Code);
        Assert.AreEqual(PopSessionState.Authorization, session.State);
      });
    }

    [Test]
    public void TestAuthNoWithAuthResponseCode()
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        server.EnqueueResponse("-ERR [AUTH] PLAIN authentication is disabled\r\n");

        session.HandlesIncapableAsException = false;

        var result = session.Auth(new NetworkCredential("test", "test", "test"),
                                  PopAuthenticationMechanism.Plain);

        Assert.IsTrue(result.Failed);
        Assert.IsNotNull(result.GetResponseCode(PopResponseCode.Auth));

        Assert.AreEqual(PopSessionState.Authorization, session.State);
        Assert.AreEqual(new Uri(string.Format("pop://{0}:{1}/", server.Host, server.Port)),
                        session.Authority);
      });
    }

    [Test]
    public void TestAuthSpecificMechanism()
    {
      using (var authMechanism = new SaslPseudoMechanism(false, 2)) {
        authMechanism.Credential = new NetworkCredential("user", (string)null);

        AuthSpecificMechanism(authMechanism);
      }
    }

    [Test]
    public void TestAuthSpecificMechanismClientFirst()
    {
      using (var authMechanism = new SaslPseudoMechanism(true, 2)) {
        authMechanism.Credential = new NetworkCredential("user", (string)null);

        AuthSpecificMechanism(authMechanism);
      }
    }

    [Test]
    public void TestAuthSpecificMechanismWithNoCredential()
    {
      using (var authMechanism = new SaslPseudoMechanism(false, 2)) {
        AuthSpecificMechanism(authMechanism);
      }
    }

    [Test]
    public void TestAuthSpecificMechanismAlreadyExchanged()
    {
      using (var authMechanism = new SaslPseudoMechanism(false, 2)) {
        byte[] clientResponse;

        authMechanism.Exchange(null, out clientResponse);

        Assert.AreNotEqual(SaslExchangeStatus.None,
                           authMechanism.ExchangeStatus);

        AuthSpecificMechanism(authMechanism);
      }
    }

    private void AuthSpecificMechanism(SaslClientMechanism authMechanism)
    {
      Connect(delegate(PopSession session, PopPseudoServer server) {
        session.HandlesIncapableAsException = true;

        if (!authMechanism.ClientFirst)
          server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+OK\r\n");

        Assert.IsTrue((bool)session.Auth(authMechanism));

        if (authMechanism.ClientFirst) {
          StringAssert.AreEqualIgnoringCase("AUTH X-PSEUDO-MECHANISM c3RlcDA=\r\n",
                                            server.DequeueRequest());
          StringAssert.AreEqualIgnoringCase("c3RlcDE=\r\n",
                                            server.DequeueRequest());
        }
        else {
          StringAssert.AreEqualIgnoringCase("AUTH X-PSEUDO-MECHANISM\r\n",
                                            server.DequeueRequest());
          StringAssert.AreEqualIgnoringCase("c3RlcDA=\r\n",
                                            server.DequeueRequest());
          StringAssert.AreEqualIgnoringCase("c3RlcDE=\r\n",
                                            server.DequeueRequest());
        }

        // not disposed
        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        authMechanism.ExchangeStatus);

        Assert.AreEqual(new Uri(string.Format("pop://{0};AUTH=X-PSEUDO-MECHANISM@{1}:{2}/",
                                              (authMechanism.Credential == null) ? null : authMechanism.Credential.UserName,
                                              server.Host,
                                              server.Port)),
                        session.Authority);
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestAuthSpecificMechanismArgumentNull()
    {
      Connect(delegate(PopSession session) {
        session.HandlesIncapableAsException = true;

        SaslClientMechanism authMechanism = null;

        session.Auth(authMechanism);
      });
    }

    [Test]
    public void TestUserAfterAuthenticated()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        var credential = new NetworkCredential("user", "pass");

        Assert.IsTrue((bool)session.User(credential));
        Assert.AreEqual(PopSessionState.Transaction, session.State);
      });
    }

    [Test]
    public void TestPassAfterAuthenticated()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        var credential = new NetworkCredential("user", "pass");

        Assert.IsTrue((bool)session.Pass(credential));
        Assert.AreEqual(PopSessionState.Transaction, session.State);
      });
    }

    [Test]
    public void TestAuthAfterAuthenticated()
    {
      Login(new[] {new PopCapability("SASL", "DIGEST-MD5")}, delegate(PopSession session, PopPseudoServer server) {
        var credential = new NetworkCredential("user", "pass");

        Assert.IsTrue((bool)session.Auth(credential, PopAuthenticationMechanism.DigestMD5));
        Assert.AreEqual(PopSessionState.Transaction, session.State);
      });
    }

    [Test]
    public void TestApopAfterAuthenticated()
    {
      Login(delegate(PopSession session, PopPseudoServer server) {
        var credential = new NetworkCredential("user", "pass");

        Assert.IsTrue((bool)session.Apop(credential));
        Assert.AreEqual(PopSessionState.Transaction, session.State);
      });
    }
  }
}
