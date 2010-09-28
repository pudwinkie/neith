using System;
using System.IO;
using System.Net;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol;
using Smdn.Net.Imap4.Protocol.Client;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsNonAuthenticatedStateTests : ImapSessionTestsBase {
    class NullCredential : ICredentialsByHost {
      public NetworkCredential GetCredential(string host, int port, string authType)
      {
        return null;
      }
    }

    [Test]
    public void TestLogin()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        session.HandlesIncapableAsException = true;

        server.EnqueueResponse("0000 NO incorrect\r\n");

        Assert.IsFalse((bool)session.Login(credential));

        Assert.AreEqual(string.Format("0000 LOGIN {0} {1}\r\n", username, password),
                        server.DequeueRequest());

        server.EnqueueResponse("0001 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Login(credential));

        Assert.AreEqual(string.Format("0001 LOGIN {0} {1}\r\n", username, password),
                        server.DequeueRequest());

        Assert.AreEqual(new Uri(string.Format("imap://{0}@{1}:{2}/", username, host, port)), session.Authority);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
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
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        session.HandlesIncapableAsException = true;

        var credentials = new CredentialCache();

        credentials.Add("imap.example.net", 143, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        server.EnqueueResponse("0000 OK authenticated\r\n");

        if (specifyUsername)
          Assert.IsTrue((bool)session.Login(credentials, "user"));
        else
          Assert.IsTrue((bool)session.Login(credentials));

        Assert.AreEqual(string.Format("0000 LOGIN user pass4\r\n"),
                        server.DequeueRequest());

        Assert.AreEqual(new Uri(string.Format("imap://user@{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestLoginSelectAppropriateCredentialNotFound()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        session.HandlesIncapableAsException = true;

        var credentials = new CredentialCache();

        credentials.Add("imap.example.net", 143, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        var result = session.Login(credentials, "xxxx");

        Assert.IsFalse((bool)result);
        Assert.AreEqual(ImapCommandResultCode.RequestError, result.Code);
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
      }
    }

    [Test]
    public void TestLoginCredentialNotFound()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.IsFalse((bool)session.Login(new NullCredential()));
        Assert.IsFalse((bool)session.Login(new NullCredential(), "user"));
      }
    }

    [Test]
    public void TestLoginCredentialUsernameNull()
    {
      LoginCredentialPropertyNull(new NetworkCredential(null, "password"),
                                  "\"\" password");
    }

    [Test]
    public void TestLoginCredentialPasswordNull()
    {
      LoginCredentialPropertyNull(new NetworkCredential("username", (string)null),
                                  "username \"\"");
    }

    private void LoginCredentialPropertyNull(ICredentialsByHost credential, string expectedArgs)
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");
      server.EnqueueResponse("0000 NO failed\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.IsFalse((bool)session.Login(credential));

        Assert.AreEqual(string.Format("0000 LOGIN {0}\r\n", expectedArgs),
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestLoginReissueCapabilityDataAdvertised()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Login(credential, true));

        StringAssert.StartsWith("0000 LOGIN ", server.DequeueRequest());

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
      }
    }

    [Test]
    public void TestLoginReissueCapabilityResponseCodeAdvertised()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("0000 OK [CAPABILITY IMAP4rev1] authenticated\r\n");

        Assert.IsTrue((bool)session.Login(credential, true));

        StringAssert.StartsWith("0000 LOGIN ", server.DequeueRequest());

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
      }
    }

    [Test]
    public void TestLoginReissueCapabilityNotAdvertised()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("0000 OK authenticated\r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        Assert.IsTrue((bool)session.Login(credential, true));

        StringAssert.StartsWith("0000 LOGIN ", server.DequeueRequest());
        Assert.AreEqual("0001 CAPABILITY\r\n", server.DequeueRequest());

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestLoginDisabled()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 LOGINDISABLED] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        session.Login(credential);
      }
    }

    [Test]
    public void TestLoginHomerServerReferralAsException()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port, true)) {
        server.EnqueueResponse("0000 NO [REFERRAL IMAP://MIKE@SERVER2/] Specified user is invalid on this server. Try SERVER2.\r\n");

        try {
          session.Login(credential);

          Assert.Fail("logged in without exception");
        }
        catch (ImapLoginReferralException ex) {
          Assert.IsTrue(ex.Message.Contains("Specified user is invalid on this server. Try SERVER2."));
          Assert.AreEqual(new Uri("IMAP://MIKE@SERVER2/"), ex.ReferToUri);
        }
      }
    }

    [Test]
    public void TestLoginHomerServerReferralAsError()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port, false)) {
        server.EnqueueResponse("0000 NO [REFERRAL IMAP://MIKE@SERVER2/] Specified user is invalid on this server. Try SERVER2.\r\n");

        Assert.IsFalse((bool)session.Login(credential));
      }
    }

    [Test]
    public void TestLoginMailboxReferralAsException()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port, true)) {
        server.EnqueueResponse("0000 OK [REFERRAL IMAP://MATTHEW@SERVER2/] Specified user's personal mailboxes located on Server2, but public mailboxes are available.\r\n");

        try {
          session.Login(credential);

          Assert.Fail("logged in without exception");
        }
        catch (ImapLoginReferralException ex) {
          Assert.IsTrue(ex.Message.Contains("Specified user's personal mailboxes located on Server2, but public mailboxes are available."));
          Assert.AreEqual(new Uri("IMAP://MATTHEW@SERVER2/"), ex.ReferToUri);
        }
      }
    }

    [Test]
    public void TestLoginMailboxReferralAsError()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port, false)) {
        server.EnqueueResponse("0000 OK [REFERRAL IMAP://MATTHEW@SERVER2/] Specified user's personal mailboxes located on Server2, but public mailboxes are available.\r\n");

        Assert.IsTrue((bool)session.Login(credential));
      }
    }

    [Test]
    public void TestAuthenticateMailboxReferralAsException()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=LOGIN LOGIN-REFERRALS] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port, true)) {
        server.EnqueueResponse("0000 NO [REFERRAL IMAP://user;AUTH=GSSAPI@SERVER2/] Specified user is invalid on this server. Try SERVER2.\r\n");

        try {
          session.Authenticate(credential, ImapAuthenticationMechanism.Login);

          Assert.Fail("authentcated without exception");
        }
        catch (ImapLoginReferralException ex) {
          Assert.IsTrue(ex.Message.Contains("Specified user is invalid on this server. Try SERVER2."));
          Assert.AreEqual(new Uri("IMAP://user;AUTH=GSSAPI@SERVER2/"), ex.ReferToUri);
        }
      }
    }

    [Test]
    public void TestAuthenticateLogin()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=LOGIN] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ " + Convert.ToBase64String(NetworkTransferEncoding.Transfer8Bit.GetBytes("Username:")) + "\r\n");
        server.EnqueueResponse("+ " + Convert.ToBase64String(NetworkTransferEncoding.Transfer8Bit.GetBytes("Password:")) + "\r\n");
        server.EnqueueResponse("0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(credential, ImapAuthenticationMechanism.Login));

        Assert.AreEqual("0000 AUTHENTICATE LOGIN\r\n",
                        server.DequeueRequest());

        var requested = server.DequeueRequest();

        Assert.AreEqual(username,
                        NetworkTransferEncoding.Transfer8Bit.GetString(Convert.FromBase64String(requested.Substring(0, requested.Length - 2))));

        requested = server.DequeueRequest();

        Assert.AreEqual(password,
                        NetworkTransferEncoding.Transfer8Bit.GetString(Convert.FromBase64String(requested.Substring(0, requested.Length - 2))));

        Assert.AreEqual(new Uri(string.Format("imap://{0};AUTH=LOGIN@{1}:{2}/", username, host, port)), session.Authority);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestAuthenticateSelectAppropriateCredentialUsernameSpecified()
    {
      AuthenticateSelectAppropriateCredential(true);
    }

    [Test]
    public void TestAuthenticateSelectAppropriateCredentialUsernameNotSpecified()
    {
      AuthenticateSelectAppropriateCredential(false);
    }

    private void AuthenticateSelectAppropriateCredential(bool specifyUsername)
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=LOGIN] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ " + Convert.ToBase64String(NetworkTransferEncoding.Transfer8Bit.GetBytes("Username:")) + "\r\n");
        server.EnqueueResponse("+ " + Convert.ToBase64String(NetworkTransferEncoding.Transfer8Bit.GetBytes("Password:")) + "\r\n");
        server.EnqueueResponse("0000 OK authenticated\r\n");

        var credentials = new CredentialCache();

        credentials.Add("imap.example.net", 143, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        server.EnqueueResponse("0000 OK authenticated\r\n");

        if (specifyUsername)
          Assert.IsTrue((bool)session.Authenticate(credentials, "user", ImapAuthenticationMechanism.Login));
        else
          Assert.IsTrue((bool)session.Authenticate(credentials, ImapAuthenticationMechanism.Login));

        Assert.AreEqual("0000 AUTHENTICATE LOGIN\r\n",
                        server.DequeueRequest());

        var requested = server.DequeueRequest();

        Assert.AreEqual("user",
                        NetworkTransferEncoding.Transfer8Bit.GetString(Convert.FromBase64String(requested.Substring(0, requested.Length - 2))));

        requested = server.DequeueRequest();

        Assert.AreEqual("pass2",
                        NetworkTransferEncoding.Transfer8Bit.GetString(Convert.FromBase64String(requested.Substring(0, requested.Length - 2))));

        Assert.AreEqual(new Uri(string.Format("imap://user;AUTH=LOGIN@{0}:{1}/", host, port)), session.Authority);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestAuthenticateSelectAppropriateCredentialNotFound()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=LOGIN] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        var credentials = new CredentialCache();

        credentials.Add("imap.example.net", 143, "LOGIN", new NetworkCredential("user", "pass1"));
        credentials.Add(server.Host, server.Port, "LOGIN", new NetworkCredential("user", "pass2"));
        credentials.Add(server.Host, server.Port, "PLAIN", new NetworkCredential("user", "pass3"));
        credentials.Add(server.Host, server.Port, string.Empty, new NetworkCredential("user", "pass4"));

        var result = session.Authenticate(credentials, "xxxx", ImapAuthenticationMechanism.Login);

        Assert.IsFalse((bool)result);
        Assert.AreEqual(ImapCommandResultCode.RequestError, result.Code);
        Assert.AreEqual(ImapSessionState.NotAuthenticated, session.State);
      }
    }

    [Test]
    public void TestAuthenticatePlain()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=PLAIN] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(credential, ImapAuthenticationMechanism.Plain));

        Assert.AreEqual("0000 AUTHENTICATE PLAIN\r\n",
                        server.DequeueRequest());

        var requested = server.DequeueRequest();
        var userpass = NetworkTransferEncoding.Transfer8Bit.GetString(Convert.FromBase64String(requested.Substring(0, requested.Length - 2))).Split(new[] {"\0"}, StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual(credential.Domain, userpass[0]);
        Assert.AreEqual(credential.UserName, userpass[1]);
        Assert.AreEqual(credential.Password, userpass[2]);

        Assert.AreEqual(new Uri(string.Format("imap://{0};AUTH=PLAIN@{1}:{2}/", username, host, port)), session.Authority);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestAuthenticatePlainSaslIRCapable()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=PLAIN SASL-IR] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(new NetworkCredential("test", "test", "test"),
                                                 ImapAuthenticationMechanism.Plain));

        Assert.AreEqual("0000 AUTHENTICATE PLAIN dGVzdAB0ZXN0AHRlc3Q=\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(new Uri(string.Format("imap://test;AUTH=PLAIN@{0}:{1}/", host, port)), session.Authority);
      }
    }

    [Test]
    public void TestAuthenticateCramMd5()
    {
      AuthenticateCramMd5(false);
    }

    [Test]
    public void TestAuthenticateCramMd5SaslIRCapable()
    {
      AuthenticateCramMd5(true);
    }

    private void AuthenticateCramMd5(bool saslIRCapable)
    {
      if (saslIRCapable)
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=CRAM-MD5] ImapSimulatedServer ready\r\n");
      else
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=CRAM-MD5 SASL-IR] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        var timestamp = ((long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
        var challenge = NetworkTransferEncoding.Transfer8Bit.GetBytes(string.Format("<{0}@{1}>", timestamp, host));

        server.EnqueueResponse("+ " + Convert.ToBase64String(challenge) + "\r\n");
        server.EnqueueResponse("0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(credential, ImapAuthenticationMechanism.CRAMMD5));

        Assert.AreEqual("0000 AUTHENTICATE CRAM-MD5\r\n",
                                   server.DequeueRequest());

        /*
        var requested = server.DequeueRequest();
        var userkeyed = NetworkTransferEncoding.Transfer8Bit.GetString(Convert.FromBase64String(requested.Substring(0, requested.Length - 2))).Split(' ');

        Assert.AreEqual(username, userkeyed[0]);

        var keyed = new List<byte>();

        for (var index = 0; index < userkeyed[1].Length; index += 2) {
          keyed.Add(Convert.ToByte(userkeyed[1].Substring(index, 2), 16));
        }

        Assert.AreEqual(Convert.ToBase64String((new HMACMD5(NetworkTransferEncoding.Transfer8Bit.GetBytes(password))).ComputeHash(challenge)),
                        Convert.ToBase64String(keyed.ToArray()));
        */

        Assert.AreEqual(new Uri(string.Format("imap://{0};AUTH=CRAM-MD5@{1}:{2}/", username, host, port)), session.Authority);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestAuthenticateInvalidResponse()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ xxxxx-invalid-response-xxxxx\r\n");

        try {
          session.Authenticate(new NetworkCredential("test", "test", "test"),
                               ImapAuthenticationMechanism.DigestMD5);
          Assert.Fail("ImapException not thrown");
        }
        catch (ImapException) {
        }

        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
      }
    }

    [Test]
    public void TestAuthenticateCancelExchanging()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ eD0xLHk9Mix6PTM=\r\n"); // x=1,y=2,z=3
        server.EnqueueResponse("0000 NO AUTHENTICATE failed.\r\n");

        session.Authenticate(new NetworkCredential("test", "test", "test"),
                             ImapAuthenticationMechanism.DigestMD5);

        Assert.AreEqual("0000 AUTHENTICATE DIGEST-MD5\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("*\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateCredentialNotFound()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.IsFalse((bool)session.Authenticate(new NullCredential(), ImapAuthenticationMechanism.DigestMD5));
        Assert.IsFalse((bool)session.Authenticate(new NullCredential(), "user", ImapAuthenticationMechanism.DigestMD5));
      }
    }

    [Test]
    public void TestAuthenticateCredentialUsernameNull()
    {
      AuthenticateCredentialPropertyNull(new NetworkCredential(null, "password"));
    }

    [Test]
    public void TestAuthenticateCredentialPasswordNull()
    {
      AuthenticateCredentialPropertyNull(new NetworkCredential("username", (string)null));
    }

    private void AuthenticateCredentialPropertyNull(ICredentialsByHost credential)
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=PLAIN] ImapSimulatedServer ready\r\n");
      server.EnqueueResponse("+ \r\n");
      server.EnqueueResponse("0000 NO failed\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.IsFalse((bool)session.Authenticate(credential,
                                                  ImapAuthenticationMechanism.Plain));

        Assert.AreEqual("0000 AUTHENTICATE PLAIN\r\n",
                        server.DequeueRequest());
        Assert.AreEqual("*\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestAuthenticateCredentialUsernameNullSaslIRCapable()
    {
      AuthenticateCredentialPropertyNullSaslIRCapable(new NetworkCredential(null, "password"));
    }

    [Test]
    public void TestAuthenticateCredentialPasswordNullSaslIRCapable()
    {
      AuthenticateCredentialPropertyNullSaslIRCapable(new NetworkCredential("username", (string)null));
    }

    private void AuthenticateCredentialPropertyNullSaslIRCapable(ICredentialsByHost credential)
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=PLAIN SASL-IR] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.IsFalse((bool)session.Authenticate(credential,
                                                  ImapAuthenticationMechanism.Plain));
      }
    }

    [Test]
    public void TestAuthenticateSpecificMechanism()
    {
      using (var authMechanism = new SaslPseudoMechanism(2)) {
        authMechanism.Credential = new NetworkCredential("user", (string)null);

        AuthenticateSpecificMechanism(false, authMechanism);
      }
    }

    [Test]
    public void TestAuthenticateSpecificMechanismSaslIRCapable()
    {
      using (var authMechanism = new SaslPseudoMechanism(2)) {
        authMechanism.Credential = new NetworkCredential("user", (string)null);

        AuthenticateSpecificMechanism(true, authMechanism);
      }
    }

    [Test]
    public void TestAuthenticateSpecificMechanismWithNoCredential()
    {
      using (var authMechanism = new SaslPseudoMechanism(2)) {
        AuthenticateSpecificMechanism(false, authMechanism);
      }
    }

    [Test]
    public void TestAuthenticateSpecificMechanismAlreadyExchanged()
    {
      using (var authMechanism = new SaslPseudoMechanism(2)) {
        byte[] clientResponse;

        authMechanism.Exchange(null, out clientResponse);

        Assert.AreNotEqual(SaslExchangeStatus.None,
                           authMechanism.ExchangeStatus);

        AuthenticateSpecificMechanism(true, authMechanism);
      }
    }

    private void AuthenticateSpecificMechanism(bool saslIRCapable, SaslClientMechanism authMechanism)
    {
      if (saslIRCapable)
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 SASL-IR] ImapSimulatedServer ready\r\n");
      else
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        session.HandlesIncapableAsException = true;

        if (!saslIRCapable)
          server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0000 OK done\r\n");

        Assert.IsTrue((bool)session.Authenticate(authMechanism));

        if (saslIRCapable) {
          Assert.AreEqual("0000 AUTHENTICATE X-PSEUDO-MECHANISM c3RlcDA=\r\n",
                          server.DequeueRequest());
          Assert.AreEqual("c3RlcDE=\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.AreEqual("0000 AUTHENTICATE X-PSEUDO-MECHANISM\r\n",
                          server.DequeueRequest());
          Assert.AreEqual("c3RlcDA=\r\n",
                          server.DequeueRequest());
          Assert.AreEqual("c3RlcDE=\r\n",
                          server.DequeueRequest());
        }

        // not disposed
        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        authMechanism.ExchangeStatus);

        Assert.AreEqual(new Uri(string.Format("imap://{0};AUTH=X-PSEUDO-MECHANISM@{1}:{2}/",
                                              (authMechanism.Credential == null) ? null : authMechanism.Credential.UserName,
                                              host,
                                              port)),
                        session.Authority);
      }
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestAuthenticateSpecificMechanismArgumentNull()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        session.HandlesIncapableAsException = true;

        SaslClientMechanism authMechanism = null;

        session.Authenticate(authMechanism);
      }
    }

    [Test]
    public void TestAuthenticateReissueCapabilityDataAdvertised()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(credential, ImapAuthenticationMechanism.Login, true));

        StringAssert.StartsWith("0000 AUTHENTICATE ", server.DequeueRequest());

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
      }
    }

    [Test]
    public void TestAuthenticateReissueCapabilityResponseCodeAdvertised()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0000 OK [CAPABILITY IMAP4rev1] authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(credential, ImapAuthenticationMechanism.Login, true));

        StringAssert.StartsWith("0000 AUTHENTICATE ", server.DequeueRequest());

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
      }
    }

    [Test]
    public void TestAuthenticateReissueCapabilityNotAdvertised()
    {
      server.EnqueueResponse("* OK ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0000 OK authenticated\r\n");
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        Assert.IsTrue((bool)session.Authenticate(credential, ImapAuthenticationMechanism.Login, true));

        StringAssert.StartsWith("0000 AUTHENTICATE ", server.DequeueRequest());
        server.DequeueRequest(); // username
        server.DequeueRequest(); // password
        Assert.AreEqual("0001 CAPABILITY\r\n", server.DequeueRequest());

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
      }
    }

    [Test]
    public void TestAuthenticateOkWithCapabilityResponse()
    {
      server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 AUTH=PLAIN SASL-IR] ImapSimulatedServer ready\r\n");

      using (var session = new ImapSession(host, port)) {
        Assert.AreEqual(3, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.SaslIR));
        Assert.IsTrue(session.ServerCapabilities.IsCapable(ImapAuthenticationMechanism.Plain));

        //server.EnqueueResponse();
        server.EnqueueResponse("0000 OK [CAPABILITY IMAP4rev1] authenticated\r\n");

        Assert.IsTrue((bool)session.Authenticate(new NetworkCredential("test", "test", "test"),
                                                 ImapAuthenticationMechanism.Plain));

        Assert.AreEqual("0000 AUTHENTICATE PLAIN dGVzdAB0ZXN0AHRlc3Q=\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(new Uri(string.Format("imap://test;AUTH=PLAIN@{0}:{1}/", host, port)), session.Authority);

        Assert.AreEqual(1, session.ServerCapabilities.Count);
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Imap4Rev1));

        try {
          session.ServerCapabilities.Add(ImapCapability.Imap4);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      }
    }

    [Test]
    public void TestLoginInAuthenticatedState()
    {
      using (var session = Authenticate()) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);

        var result = session.Login(credential);

        Assert.IsTrue((bool)result);
        Assert.AreEqual(result.Code, ImapCommandResultCode.RequestDone);

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestAuthenticateInAuthenticatedState()
    {
      using (var session = Authenticate()) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);

        var result = session.Authenticate(credential, ImapAuthenticationMechanism.Login);

        Assert.IsTrue((bool)result);
        Assert.AreEqual(result.Code, ImapCommandResultCode.RequestDone);

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestLoginInSelectedState()
    {
      using (var session = SelectMailbox()) {
        var selectedMailbox = session.SelectedMailbox;

        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.IsNotNull(session.SelectedMailbox);

        var result = session.Login(credential);

        Assert.IsTrue((bool)result);
        Assert.AreEqual(result.Code, ImapCommandResultCode.RequestDone);

        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.IsNotNull(session.SelectedMailbox);

        Assert.AreSame(selectedMailbox, session.SelectedMailbox);
      }
    }

    [Test]
    public void TestAuthenticateInSelectedState()
    {
      using (var session = SelectMailbox()) {
        var selectedMailbox = session.SelectedMailbox;

        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.IsNotNull(session.SelectedMailbox);

        var result = session.Authenticate(credential, ImapAuthenticationMechanism.Login);

        Assert.IsTrue((bool)result);
        Assert.AreEqual(result.Code, ImapCommandResultCode.RequestDone);

        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.IsNotNull(session.SelectedMailbox);

        Assert.AreSame(selectedMailbox, session.SelectedMailbox);
      }
    }

    [Test]
    public void TestStartTls()
    {
      using (var session = Connect()) {
        session.HandlesIncapableAsException = true;
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK CAPABILITY completed\r\n");

        Assert.IsTrue((bool)session.Capability());

        server.DequeueRequest();

        server.EnqueueResponse("0001 OK Begin TLS negotiation now\r\n");

        var prevAuthority = new Uri(session.Authority.ToString());
        var streamUpgraded = false;

        Assert.IsFalse(session.IsSecureConnection);
        Assert.IsTrue((bool)session.StartTls(delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SslStream
        }));

        Assert.IsTrue(streamUpgraded, "stream upgraded");
        Assert.IsTrue(session.IsSecureConnection, "IsSecureConnection");

        Assert.AreEqual("0001 STARTTLS\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(prevAuthority, session.Authority);
      }
    }

    [Test]
    public void TestStartTlsExceptionWhileUpgrading()
    {
      using (var session = Connect()) {
        session.HandlesIncapableAsException = true;
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK CAPABILITY completed\r\n");

        Assert.IsTrue((bool)session.Capability());

        server.DequeueRequest();

        server.EnqueueResponse("0001 OK Begin TLS negotiation now\r\n");

        try {
          session.StartTls(delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException("failed");
          });
          Assert.Fail("ImapUpgradeConnectionException not thrown");
        }
        catch (ImapUpgradeConnectionException) {
        }

        Assert.IsNull(session.Authority);
        Assert.AreEqual(ImapSessionState.NotConnected, session.State);
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestStartTlsIncapable()
    {
      using (var session = Connect()) {
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=PLAIN\r\n" +
                               "0000 OK CAPABILITY completed\r\n");

        Assert.IsTrue((bool)session.Capability());

        server.DequeueRequest();

        Assert.IsFalse(session.IsSecureConnection);

        var streamUpgraded = false;

        session.HandlesIncapableAsException = true;
        session.StartTls(delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SslStream
        });

        Assert.IsFalse(streamUpgraded, "stream upgraded");
        Assert.IsFalse(session.IsSecureConnection, "IsSecureConnection");
      }
    }

    [Test]
    public void TestStartTlsBadResponse()
    {
      using (var session = Connect()) {
        server.EnqueueResponse("0000 BAD command unknown\r\n");

        var prevAuthority = new Uri(session.Authority.ToString());

        Assert.IsFalse(session.IsSecureConnection);

        var streamUpgraded = false;

        Assert.IsFalse((bool)session.StartTls(delegate(ConnectionBase connection, Stream baseStream) {
          streamUpgraded = true;
          return baseStream; // TODO: return SslStream
        }));

        Assert.IsFalse(streamUpgraded, "stream upgraded");

        Assert.AreEqual("0000 STARTTLS\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(prevAuthority, session.Authority);
        Assert.IsFalse(session.IsSecureConnection);
      }
    }
  }
}
