using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using NUnit.Framework;

using Smdn.Formats;
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCreatorTests {
    private class Profile : IImapSessionProfile {
      public Uri Authority {
        get; set;
      }

      public int Timeout {
        get; set;
      }

      public int SendTimeout {
        get; set;
      }

      public int ReceiveTimeout {
        get; set;
      }

      public ICredentialsByHost Credentials {
        get; set;
      }

      public bool UseDeflateIfAvailable {
        get { return false; }
      }

      public string[] UsingSaslMechanisms {
        get; set;
      }

      public bool UseTlsIfAvailable {
        get; set;
      }

      public bool AllowInsecureLogin {
        get; set;
      }

      public Profile(NetworkCredential cred, string userinfo, string hostPort)
        : this(cred, ImapUri.UriSchemeImap, userinfo, hostPort)
      {
      }

      public Profile(NetworkCredential cred, string scheme, string userinfo, string hostPort)
      {
        this.UseTlsIfAvailable = true;
        this.Timeout = 1000;
        this.SendTimeout = -1;
        this.ReceiveTimeout = -1;
        this.Credentials = cred;
        this.AllowInsecureLogin = false;

        if (userinfo == null)
          this.Authority = new Uri(string.Format("{0}://{1}/", scheme, hostPort));
        else
          this.Authority = new Uri(string.Format("{0}://{1}@{2}/", scheme, userinfo, hostPort));
      }
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestCreateSessionCreateSslStreamCallbackNull()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "IMAPS", "user", server.HostPort);

        prof.Timeout = 500;

        server.Stop();

        ImapSessionCreator.CreateSession(prof, null, null);
      }
    }

    [Test]
    public void TestCreateSessionConnectRefusedByTimeout()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.Timeout = 500;

        server.Stop();

        try {
          ImapSessionCreator.CreateSession(prof, null, null);

          Assert.Fail("ImapConnectionException not thrown");
        }
        catch (ImapConnectionException ex) {
          var timeoutException = ex.InnerException as TimeoutException;

          Assert.IsNotNull(timeoutException);
        }
      }
    }

    [Test]
    public void TestCreateSessionConnectRefusedByDnsError()
    {
      var prof = new Profile(new NetworkCredential("user", "pass"), "user", "imap.invalid");

      try {
        ImapSessionCreator.CreateSession(prof, null, null);

        Assert.Fail("ImapConnectionException not thrown");
      }
      catch (ImapConnectionException ex) {
        var socketException = ex.InnerException as System.Net.Sockets.SocketException;

        Assert.IsNotNull(socketException);
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestCreateSessionImap4Rev1Incapable()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4\r\n" +
                               "0000 OK done\r\n");

        ImapSessionCreator.CreateSession(prof, null, null);
      }
    }

    [Test]
    public void TestCreateSessionConnectSecurePort()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "imaps", "user", server.HostPort);

        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS\r\n" +
                               "0000 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        var streamUpgraded = false;

        using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.IsTrue(streamUpgraded, "stream upgraded");

          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsTrue(session.IsSecureConnection);
        }
      }
    }

    [Test]
    public void TestCreateSessionConnectSecurePortException()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "imaps", "user", server.HostPort);

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          }));
          Assert.Fail("ImapSecureConnectionException not thrown");
        }
        catch (ImapSecureConnectionException ex) {
          Assert.IsNotNull(ex.InnerException);

          var upgradeException = ex.InnerException as ImapUpgradeConnectionException;

          Assert.IsNotNull(upgradeException);
          Assert.IsNotNull(upgradeException.InnerException);
          Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                  upgradeException.InnerException);
        }
      }
    }

    [Test]
    public void TestCreateSessionStartTlsUseTlsIfAvailable()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.AllowInsecureLogin = false;
        prof.UseTlsIfAvailable = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");
        // STARTTLS response
        server.EnqueueResponse("0001 OK done\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0002 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0003 OK done\r\n");

        var streamUpgraded = false;

        using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.IsTrue(streamUpgraded, "stream upgraded");

          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsTrue(session.IsSecureConnection);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.StartsWith("0001 STARTTLS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionStartTlsDontUseTlsWhetherAvailable()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.AllowInsecureLogin = true;
        prof.UseTlsIfAvailable = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS\r\n" +
                               "0000 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        var streamUpgraded = false;

        using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.IsFalse(streamUpgraded, "stream upgraded");

          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsFalse(session.IsSecureConnection);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.StartsWith("0001 LOGIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionStartTlsFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");
        // STARTTLS response
        server.EnqueueResponse("0001 BAD done\r\n");

        var streamUpgraded = false;

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
            streamUpgraded = true;
            return baseStream; // TODO: return SSL stream
          }));
          Assert.Fail("ImapSecureConnectionException not thrown");
        }
        catch (ImapSecureConnectionException ex) {
          Assert.IsNull(ex.InnerException);
        }

        Assert.IsFalse(streamUpgraded, "stream upgraded");

        server.DequeueRequest(); // CAPABILITY
        StringAssert.StartsWith("0001 STARTTLS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionStartTlsException()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");
        // STARTTLS response
        server.EnqueueResponse("0001 OK done\r\n");

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          }));
          Assert.Fail("ImapSecureConnectionException not thrown");
        }
        catch (ImapSecureConnectionException ex) {
          Assert.IsNotNull(ex.InnerException);

          var upgradeException = ex.InnerException as ImapUpgradeConnectionException;

          Assert.IsNotNull(upgradeException);
          Assert.IsNotNull(upgradeException.InnerException);
          Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                  upgradeException.InnerException);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.StartsWith("0001 STARTTLS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionCapabilityAdvertised()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "imap", "user", server.HostPort);

        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ready\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsFalse(session.IsSecureConnection);
        }
      }
    }

    [Test]
    public void TestCreateSessionCapabilityNotAdvertised()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "imap", "user", server.HostPort);

        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("0001 OK done\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0002 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsFalse(session.IsSecureConnection);
        }
      }
    }

    [Test]
    public void TestAuthenticateUserAndAuthMechanismSpecified()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=PLAIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAuthMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=PLAIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0001 NO done\r\n");

        try {
          Assert.IsNull(ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.ImapCommandResultCode.No, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndAuthMechanismSpecified()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), ";auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=PLAIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK ready\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndAuthMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), ";auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=PLAIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0001 NO done\r\n");

        try {
          Assert.IsNull(ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.ImapCommandResultCode.No, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecified()
    {
      AuthenticateWithAppropriateMechanism("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecified()
    {
      AuthenticateWithAppropriateMechanism("user");
    }

    private void AuthenticateWithAppropriateMechanism(string userinfo)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE LOGIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedExceptionWhileAuthentication()
    {
      AuthenticateExceptionWhileAuthentication("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedExceptionWhileAuthentication()
    {
      AuthenticateExceptionWhileAuthentication("user");
    }

    private void AuthenticateExceptionWhileAuthentication(string userinfo)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"DIGEST-MD5", "PLAIN"};

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE DIGEST-MD5 response
        server.EnqueueResponse("+ xxx-invalid-response-xxx\r\n");

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapException not thrown");
        }
        catch (ImapException ex) {
          Assert.IsNotNull(ex.InnerException);
        }
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedFallbackToNextAppropriateMechanism()
    {
      AuthenticateWithAppropriateMechanismFallbackToNextAppropriateMechanism("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedFallbackToNextAppropriateMechanism()
    {
      AuthenticateWithAppropriateMechanismFallbackToNextAppropriateMechanism("user");
    }

    private void AuthenticateWithAppropriateMechanismFallbackToNextAppropriateMechanism(string userinfo)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE LOGIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0001 NO done\r\n");
        // AUTHENTICATE PLAIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0002 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE LOGIN", server.DequeueRequest());
        server.DequeueRequest(); // AUTHENTICATE LOGIN client response
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedFallbackToLogin()
    {
      AuthenticateWithAppropriateMechanismFallbackToLogin("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedFallbackToLogin()
    {
      AuthenticateWithAppropriateMechanismFallbackToLogin("user");
    }

    private void AuthenticateWithAppropriateMechanismFallbackToLogin(string userinfo)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("LOGIN user", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedFallbackToLoginAuthenticationFailure()
    {
      AuthenticateWithAppropriateMechanismFallbackToLoginAuthenticationFailure("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedFallbackToLoginAuthenticationFailure()
    {
      AuthenticateWithAppropriateMechanismFallbackToLoginAuthenticationFailure("user");
    }

    private void AuthenticateWithAppropriateMechanismFallbackToLoginAuthenticationFailure(string userinfo)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("0001 NO done\r\n");

        try {
          Assert.IsNull(ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.ImapCommandResultCode.No, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("LOGIN user", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedAppropriateMechanismNotFound()
    {
      AuthenticateWithAppropriateMechanismAppropriateMechanismNotFound("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedAppropriateMechanismNotFound()
    {
      AuthenticateWithAppropriateMechanismAppropriateMechanismNotFound("user");
    }

    private void AuthenticateWithAppropriateMechanismAppropriateMechanismNotFound(string userinfo)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNull(ex.Result);
        }

        server.DequeueRequest(); // CAPABILITY
      }
    }

    [Test]
    public void TestAuthenticateAnonymousMechanismSpecified()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(null, "user%40imap.example.net;auth=anonymous", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE ANONYMOUS", server.DequeueRequest());
        StringAssert.Contains("user@imap.example.net", Base64.GetDecodedString(server.DequeueRequest()));
      }
    }

    [Test]
    public void TestAuthenticateAnonymousMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(null, "user%40imap.example.net;auth=anonymous", server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0001 NO done\r\n");

        try {
          Assert.IsNull(ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.ImapCommandResultCode.No, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE ANONYMOUS", server.DequeueRequest());
        StringAssert.Contains("user@imap.example.net", Base64.GetDecodedString(server.DequeueRequest()));
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndNoAuthMechanismSpecified()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(null, null, server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0001 NO done\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0002 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE ANONYMOUS", server.DequeueRequest());
        StringAssert.Contains("anonymous@", Base64.GetDecodedString(server.DequeueRequest()));
        StringAssert.Contains("LOGIN anonymous anonymous@", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndNoAuthMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(null, null, server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN response
        server.EnqueueResponse("0001 NO done\r\n");

        try {
          Assert.IsNull(ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.ImapCommandResultCode.No, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("LOGIN anonymous anonymous@", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndNoAuthMechanismSpecifiedAppropriateMechanismNotFound()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(null, null, server.HostPort);

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
        }

        server.DequeueRequest(); // CAPABILITY
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowed()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "CRAM-MD5"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=X-UNKNOWN AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE CRAM-MD5 response
        server.EnqueueResponse("+ PDQwMDEzNDQxMTIxNDM1OTQuMTI3MjQ5OTU1MEBsb2NhbGhvc3Q+\r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE CRAM-MD5", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedSecureSession()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");
        // STARTTLS response
        server.EnqueueResponse("0001 OK done\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 SASL-IR AUTH=LOGIN AUTH=PLAIN AUTH=ANONYMOUS\r\n" +
                               "0002 OK done\r\n");
        // AUTHENTICATE LOGIN response
        server.EnqueueResponse("0003 NO done\r\n");
        // AUTHENTICATE PLAIN response
        server.EnqueueResponse("0004 NO done\r\n");
        // LOGIN response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0005 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsTrue(session.IsSecureConnection);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // STARTTLS
        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE LOGIN", server.DequeueRequest());
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
        StringAssert.Contains("LOGIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedFallbackToNextAppropriateMechanism()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "DIGEST-MD5", "CRAM-MD5"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=X-UNKNOWN AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE DIGEST-MD5 response
        server.EnqueueResponse("+ cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                               "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                               "cnNldD11dGYtOA==\r\n");
        server.EnqueueResponse("0001 NO done\r\n");
        // AUTHENTICATE CRAM-MD5 response
        server.EnqueueResponse("+ PDQwMDEzNDQxMTIxNDM1OTQuMTI3MjQ5OTU1MEBsb2NhbGhvc3Q+\r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0002 OK done\r\n");

        using (var session = ImapSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(ImapSessionState.Authenticated, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE DIGEST-MD5", server.DequeueRequest());
        server.DequeueRequest(); // AUTHENTICATE DIGEST-MD5 client response
        StringAssert.Contains("AUTHENTICATE CRAM-MD5", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedFallbackToNextAppropriateMechanismAuthenticationFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "DIGEST-MD5", "CRAM-MD5"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=X-UNKNOWN AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE DIGEST-MD5 response
        server.EnqueueResponse("+ cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                               "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                               "cnNldD11dGYtOA==\r\n");
        server.EnqueueResponse("0001 NO done\r\n");
        // AUTHENTICATE CRAM-MD5 response
        server.EnqueueResponse("+ PDQwMDEzNDQxMTIxNDM1OTQuMTI3MjQ5OTU1MEBsb2NhbGhvc3Q+\r\n");
        server.EnqueueResponse("0002 NO done\r\n");

        try {
          Assert.IsNull(ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.ImapCommandResultCode.No, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE DIGEST-MD5", server.DequeueRequest());
        server.DequeueRequest(); // AUTHENTICATE DIGEST-MD5 client response
        StringAssert.Contains("AUTHENTICATE CRAM-MD5", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedAppropriateMechanismNotFound()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=X-UNKNOWN AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN AUTH=ANONYMOUS\r\n" +
                               "0000 OK done\r\n");

        try {
          using (var session = ImapSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("ImapAuthenticationException not thrown");
        }
        catch (ImapAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNull(ex.Result);
        }

        server.DequeueRequest(); // CAPABILITY
      }
    }

    [Test]
    public void TestAuthenticateSpecificMechanism()
    {
      AuthenticateSpecificMechanism(new NetworkCredential("sasl-user", (string)null));
    }

    [Test]
    public void TestAuthenticateSpecificMechanismCredentialNotSet()
    {
      AuthenticateSpecificMechanism(null);
    }

    private void AuthenticateSpecificMechanism(NetworkCredential credential)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("imap-user", (string)null), "imap", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "DIGEST-MD5", "CRAM-MD5"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE X-PSEUDO-MECHANISM response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        using (var authMechanism = new SaslPseudoMechanism(1)) {
          authMechanism.Credential = credential;

          using (var session = ImapSessionCreator.CreateSession(prof, authMechanism, null)) {
            Assert.AreEqual(ImapSessionState.Authenticated, session.State);

            if (credential == null)
              Assert.AreEqual(new Uri(string.Format("imap://AUTH=X-PSEUDO-MECHANISM@{0}/", server.HostPort)),
                              session.Authority);
            else
              Assert.AreEqual(new Uri(string.Format("imap://sasl-user;AUTH=X-PSEUDO-MECHANISM@{0}/", server.HostPort)),
                              session.Authority);
          }

          Assert.AreSame(credential,
                         authMechanism.Credential,
                         "credential must be kept");

          Assert.AreEqual(Smdn.Security.Authentication.Sasl.SaslExchangeStatus.Succeeded,
                          authMechanism.ExchangeStatus);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE X-PSEUDO-MECHANISM", server.DequeueRequest());
      }
    }
  }
}
