using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using NUnit.Framework;

using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3.Client.Session {
  [TestFixture]
  public class PopSessionCreatorTests {
    private class Profile : IPopSessionProfile {
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
        : this(cred, PopUri.UriSchemePop, userinfo, hostPort)
      {
      }

      public Profile(NetworkCredential cred, string scheme, string userinfo, string hostPort)
      {
        this.UseTlsIfAvailable = true;
        this.AllowInsecureLogin = false;
        this.Timeout = 1000;
        this.SendTimeout = -1;
        this.ReceiveTimeout = -1;
        this.Credentials = cred;

        if (userinfo == null)
          this.Authority = new Uri(string.Format("{0}://{1}/", scheme, hostPort));
        else
          this.Authority = new Uri(string.Format("{0}://{1}@{2}/", scheme, userinfo, hostPort));
      }
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestCreateSessionCreateSslStreamCallbackNull()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "POPS", "user", server.HostPort);

        prof.Timeout = 500;

        server.Stop();

        PopSessionCreator.CreateSession(prof, null, null);
      }
    }

    [Test]
    public void TestCreateSessionConnectRefusedByTimeout()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.Timeout = 500;

        server.Stop();

        try {
          PopSessionCreator.CreateSession(prof, null, null);

          Assert.Fail("PopConnectionException not thrown");
        }
        catch (PopConnectionException ex) {
          var timeoutException = ex.InnerException as TimeoutException;

          Assert.IsNotNull(timeoutException);
        }
      }
    }

    [Test]
    public void TestCreateSessionConnectRefusedByDnsError()
    {
      var prof = new Profile(new NetworkCredential("user", "pass"), "user", "pop.invalid");

      try {
        PopSessionCreator.CreateSession(prof, null, null);

        Assert.Fail("PopConnectionException not thrown");
      }
      catch (PopConnectionException ex) {
        var socketException = ex.InnerException as System.Net.Sockets.SocketException;

        Assert.IsNotNull(socketException);
      }
    }

    [Test]
    public void TestCreateSessionConnectSecurePort()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "pops", "user", server.HostPort);

        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // USER response
        server.EnqueueResponse("+OK\r\n");
        // PASS response
        server.EnqueueResponse("+OK\r\n");

        var streamUpgraded = false;

        using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.IsTrue(streamUpgraded, "stream upgraded");

          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsTrue(session.IsSecureConnection);
        }
      }
    }

    [Test]
    public void TestCreateSessionConnectSecurePortException()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "pops", "user", server.HostPort);

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          }));
          Assert.Fail("PopUpgradeConnectionException not thrown");
        }
        catch (PopUpgradeConnectionException ex) {
          Assert.IsNotNull(ex.InnerException);
          Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                  ex.InnerException);
        }
      }
    }

    [Test]
    public void TestCreateSessionStlsUseTlsIfAvailable()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.AllowInsecureLogin = false;
        prof.UseTlsIfAvailable = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // STLS response
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER response
        server.EnqueueResponse("+OK\r\n");
        // PASS response
        server.EnqueueResponse("+OK\r\n");

        var streamUpgraded = false;

        using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.IsTrue(streamUpgraded, "stream upgraded");

          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsTrue(session.IsSecureConnection);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("STLS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionStlsDontUseTlsWhetherAvailable()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.AllowInsecureLogin = true;
        prof.UseTlsIfAvailable = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // USER response
        server.EnqueueResponse("+OK\r\n");
        // PASS response
        server.EnqueueResponse("+OK\r\n");

        var streamUpgraded = false;

        using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.IsFalse(streamUpgraded, "stream upgraded");

          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsFalse(session.IsSecureConnection);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("USER", server.DequeueRequest());
        StringAssert.StartsWith("PASS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionStlsFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // STLS response
        server.EnqueueResponse("-ERR\r\n");

        var streamUpgraded = false;

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
            streamUpgraded = true;
            return baseStream; // TODO: return SSL stream
          }));
          Assert.Fail("PopUpgradeConnectionException not thrown");
        }
        catch (PopUpgradeConnectionException ex) {
          Assert.IsNull(ex.InnerException);
        }

        Assert.IsFalse(streamUpgraded, "stream upgraded");

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("STLS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreateSessionStlsException()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // STLS response
        server.EnqueueResponse("+OK\r\n");

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          }));
          Assert.Fail("PopUpgradeConnectionException not thrown");
        }
        catch (PopUpgradeConnectionException ex) {
          Assert.IsNotNull(ex.InnerException);
          Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                  ex.InnerException);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("STLS", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAuthMechanismSpecified()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL PLAIN\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAuthMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL PLAIN\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("-ERR\r\n");

        try {
          Assert.IsNull(PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.PopCommandResultCode.Error, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndAuthMechanismSpecified()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), ";auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL PLAIN\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndAuthMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), ";auth=plain", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL PLAIN\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("-ERR\r\n");

        try {
          Assert.IsNull(PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.PopCommandResultCode.Error, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH LOGIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH LOGIN", server.DequeueRequest());
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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"DIGEST-MD5", "PLAIN"};

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN\r\n" +
                               ".\r\n");
        // AUTH DIGEST-MD5 response
        server.EnqueueResponse("+ xxx-invalid-response-xxx\r\n");

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopException not thrown");
        }
        catch (PopException ex) {
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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH LOGIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("-ERR\r\n");
        // AUTH PLAIN response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH LOGIN", server.DequeueRequest());
        server.DequeueRequest(); // AUTH LOGIN client response
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedFallbackToApop()
    {
      AuthenticateWithAppropriateMechanismFallbackToApop("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedFallbackToApop()
    {
      AuthenticateWithAppropriateMechanismFallbackToApop("user");
    }

    private void AuthenticateWithAppropriateMechanismFallbackToApop(string userinfo)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "PLAIN", "LOGIN"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK <timestamp@localhost>\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH PLAIN response
        server.EnqueueResponse("-ERR\r\n");
        // AUTH LOGIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("-ERR\r\n");
        // APOP response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
        StringAssert.StartsWith("AUTH LOGIN", server.DequeueRequest());
        server.DequeueRequest(); // AUTH LOGIN client response
        StringAssert.StartsWith("APOP user", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedFallbackToUserPass()
    {
      AuthenticateWithAppropriateMechanismFallbackToUserPass("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedFallbackToUserPass()
    {
      AuthenticateWithAppropriateMechanismFallbackToUserPass("user");
    }

    private void AuthenticateWithAppropriateMechanismFallbackToUserPass(string userinfo)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER response
        server.EnqueueResponse("+OK\r\n");
        // PASS response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("USER user", server.DequeueRequest());
        StringAssert.StartsWith("PASS pass", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateUserAndAppropriateAuthMechanismSpecifiedFallbackToUserPassAuthenticationFailure()
    {
      AuthenticateWithAppropriateMechanismFallbackToUserPassAuthenticationFailure("user;auth=*");
    }

    [Test]
    public void TestAuthenticateUserAndNoAuthMechanismSpecifiedFallbackToUserPassAuthenticationFailure()
    {
      AuthenticateWithAppropriateMechanismFallbackToUserPassAuthenticationFailure("user");
    }

    private void AuthenticateWithAppropriateMechanismFallbackToUserPassAuthenticationFailure(string userinfo)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER response
        server.EnqueueResponse("-ERR\r\n");

        try {
          Assert.IsNull(PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.PopCommandResultCode.Error, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("USER user", server.DequeueRequest());
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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), userinfo, server.HostPort);

        prof.AllowInsecureLogin = false; // LOGINDISABLE equiv

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNull(ex.Result);
        }

        server.DequeueRequest(); // CAPA
      }
    }

    [Test]
    public void TestAuthenticateAnonymousMechanismSpecified()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(null, "user%40pop.example.net;auth=anonymous", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH ANONYMOUS dXNlckBwb3AuZXhhbXBsZS5uZXQ=", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateAnonymousMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(null, "user%40pop.example.net;auth=anonymous", server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("-ERR\r\n");

        try {
          Assert.IsNull(PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.PopCommandResultCode.Error, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH ANONYMOUS dXNlckBwb3AuZXhhbXBsZS5uZXQ=", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndNoAuthMechanismSpecified()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(null, null, server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH response
        server.EnqueueResponse("-ERR\r\n");
        // USER response
        server.EnqueueResponse("+OK\r\n");
        // PASS response
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH ANONYMOUS YW5vbnltb3Vz", server.DequeueRequest());
        StringAssert.StartsWith("USER anonymous", server.DequeueRequest());
        StringAssert.StartsWith("PASS anonymous@", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateNoUserAndNoAuthMechanismSpecifiedAuthenticationFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(null, null, server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER response
        server.EnqueueResponse("+OK\r\n");
        // PASS response
        server.EnqueueResponse("-ERR\r\n");

        try {
          Assert.IsNull(PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.PopCommandResultCode.Error, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("USER anonymous", server.DequeueRequest()); // anonymous credential
        StringAssert.StartsWith("PASS anonymous@", server.DequeueRequest());
      }
    }

    [Test, Ignore("not work; AUTH ANONYMOUS not supported && LOGINDISABLE")]
    public void TestAuthenticateNoUserAndNoAuthMechanismSpecifiedAppropriateMechanismNotFound()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(null, null, server.HostPort);

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNull(ex.Result);
        }

        server.DequeueRequest(); // CAPA
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowed()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "CRAM-MD5"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("+OK <timestamp@localhost>\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL X-UNKNOWN DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH CRAM-MD5 response
        server.EnqueueResponse("+ PDQwMDEzNDQxMTIxNDM1OTQuMTI3MjQ5OTU1MEBsb2NhbGhvc3Q+\r\n");
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.Contains("AUTH CRAM-MD5", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedSecureSessionApopAvailable()
    {
      AuthenticateInsecureLoginDisallowedSecureSession(true);
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedSecureSessionApopNotAvailable()
    {
      AuthenticateInsecureLoginDisallowedSecureSession(false);
    }

    private void AuthenticateInsecureLoginDisallowedSecureSession(bool apopAvailable)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse(apopAvailable ? "+OK <timestamp@localhost>\r\n" : "+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // STLS response
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH LOGIN response
        server.EnqueueResponse("-ERR\r\n");
        // AUTH PLAIN response
        server.EnqueueResponse("-ERR\r\n");

        if (apopAvailable) {
          // APOP response
          server.EnqueueResponse("+OK\r\n");
        }
        else {
          // USER response
          server.EnqueueResponse("+OK\r\n");
          // PASS response
          server.EnqueueResponse("+OK\r\n");
        }

        using (var session = PopSessionCreator.CreateSession(prof, null, delegate(ConnectionBase connection, Stream baseStream) {
          return baseStream; // TODO: return SSL stream
        })) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
          Assert.IsTrue(session.IsSecureConnection);
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // STLS
        server.DequeueRequest(); // CAPA
        StringAssert.Contains("AUTH LOGIN", server.DequeueRequest());
        StringAssert.Contains("AUTH PLAIN", server.DequeueRequest());

        if (apopAvailable) {
          StringAssert.Contains("APOP", server.DequeueRequest());
        }
        else {
          StringAssert.Contains("USER", server.DequeueRequest());
          StringAssert.Contains("PASS", server.DequeueRequest());
        }
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedFallbackToNextAppropriateMechanism()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "DIGEST-MD5", "CRAM-MD5"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("+OK <timestamp@localhost>\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL X-UNKNOWN DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH DIGEST-MD5 response
        server.EnqueueResponse("+ cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                               "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                               "cnNldD11dGYtOA==\r\n");
        server.EnqueueResponse("-ERR\r\n");
        // AUTH CRAM-MD5 response
        server.EnqueueResponse("+ PDQwMDEzNDQxMTIxNDM1OTQuMTI3MjQ5OTU1MEBsb2NhbGhvc3Q+\r\n");
        server.EnqueueResponse("+OK\r\n");

        using (var session = PopSessionCreator.CreateSession(prof, null, null)) {
          Assert.AreEqual(PopSessionState.Transaction, session.State);
          Assert.AreEqual(prof.Authority, session.Authority);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.Contains("AUTH DIGEST-MD5", server.DequeueRequest());
        server.DequeueRequest(); // AUTH DIGEST-MD5 client response
        StringAssert.Contains("AUTH CRAM-MD5", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedFallbackToNextAppropriateMechanismAuthenticationFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "DIGEST-MD5", "CRAM-MD5"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("+OK <timestamp@localhost>\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL X-UNKNOWN DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");
        // AUTH DIGEST-MD5 response
        server.EnqueueResponse("+ cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                               "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                               "cnNldD11dGYtOA==\r\n");
        server.EnqueueResponse("-ERR\r\n");
        // AUTH CRAM-MD5 response
        server.EnqueueResponse("+ PDQwMDEzNDQxMTIxNDM1OTQuMTI3MjQ5OTU1MEBsb2NhbGhvc3Q+\r\n");
        server.EnqueueResponse("-ERR\r\n");

        try {
          Assert.IsNull(PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNotNull(ex.Result);
          Assert.AreEqual(Protocol.Client.PopCommandResultCode.Error, ex.Result.Code);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.Contains("AUTH DIGEST-MD5", server.DequeueRequest());
        server.DequeueRequest(); // AUTH DIGEST-MD5 client response
        StringAssert.Contains("AUTH CRAM-MD5", server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateInsecureLoginDisallowedAppropriateMechanismNotFound()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("user", "pass"), "user;auth=*", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"ANONYMOUS", "LOGIN", "PLAIN"};
        prof.AllowInsecureLogin = false;

        // greeting
        server.EnqueueResponse("+OK <timestamp@localhost>\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL X-UNKNOWN DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN ANONYMOUS\r\n" +
                               ".\r\n");

        try {
          using (var session = PopSessionCreator.CreateSession(prof, null, null));
          Assert.Fail("PopAuthenticationException not thrown");
        }
        catch (PopAuthenticationException ex) {
          Assert.IsNull(ex.InnerException);
          Assert.IsNull(ex.Result);
        }

        server.DequeueRequest(); // CAPA
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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var prof = new Profile(new NetworkCredential("pop-user", (string)null), "pop", server.HostPort);

        prof.UsingSaslMechanisms = new[] {"LOGIN", "PLAIN", "ANONYMOUS", "DIGEST-MD5", "CRAM-MD5"};
        prof.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // AUTH X-PSEUDO-MECHANISM response
        server.EnqueueResponse("+\r\n");
        server.EnqueueResponse("+OK\r\n");

        using (var authMechanism = new SaslPseudoMechanism(false, 1)) {
          authMechanism.Credential = credential;

          using (var session = PopSessionCreator.CreateSession(prof, authMechanism, null)) {
            Assert.AreEqual(PopSessionState.Transaction, session.State);

            if (credential == null)
              Assert.AreEqual(new Uri(string.Format("pop://AUTH=X-PSEUDO-MECHANISM@{0}/", server.HostPort)),
                              session.Authority);
            else
              Assert.AreEqual(new Uri(string.Format("pop://sasl-user;AUTH=X-PSEUDO-MECHANISM@{0}/", server.HostPort)),
                              session.Authority);
          }

          Assert.AreSame(credential,
                         authMechanism.Credential,
                         "credential must be kept");

          Assert.AreEqual(Smdn.Security.Authentication.Sasl.SaslExchangeStatus.Succeeded,
                          authMechanism.ExchangeStatus);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTH X-PSEUDO-MECHANISM", server.DequeueRequest());
      }
    }
  }
}
