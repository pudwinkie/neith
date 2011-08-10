using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NUnit.Framework;

using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapSessionManagerTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private void WebExceptionAssertion(WebExceptionStatus expectedStatus, WebException actual)
    {
      if (expectedStatus != actual.Status)
        throw actual;

      if (expectedStatus == WebExceptionStatus.ProtocolError) {
        Assert.IsNotNull(actual.Response);
        Assert.IsInstanceOfType(typeof(ImapWebResponse), actual.Response);

        var response = actual.Response as ImapWebResponse;

        Assert.IsTrue(response.Result.Failed);
      }
    }

    [Test]
    public void TestCreateSessionConnectFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.Timeout = 500;

        server.Stop();

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.ConnectFailure, ex);
        }
      }
    }

    [Test]
    public void TestCreateSessionRequestCanceled()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response (IMAP4rev1 incapable)
        server.EnqueueResponse("* CAPABILITY IMAP4\r\n" +
                               "0000 OK done\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.RequestCanceled, ex);
        }
      }
    }

    [Test]
    public void TestCreateSessionSecureChannelFailureSecurePort()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imaps://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        var defaultCallback = ImapSessionManager.CreateSslStreamCallback;

        try {
          ImapSessionManager.CreateSslStreamCallback = delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          };

          try {
            using (var response = request.GetResponse()) {
            }
          }
          catch (WebException ex) {
            WebExceptionAssertion(WebExceptionStatus.SecureChannelFailure, ex);

            var upgradeException = ex.InnerException as Smdn.Net.Imap4.Protocol.ImapUpgradeConnectionException;

            Assert.IsNotNull(upgradeException);
            Assert.IsNotNull(upgradeException.InnerException);
            Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                    upgradeException.InnerException);
          }
        }
        finally {
          ImapSessionManager.CreateSslStreamCallback = defaultCallback;
        }
      }
    }

    [Test]
    public void TestCreateSessionSecureChannelFailureStartTlsException()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");
        // STARTTLS response
        server.EnqueueResponse("0001 OK done\r\n");

        var defaultCallback = ImapSessionManager.CreateSslStreamCallback;

        try {
          ImapSessionManager.CreateSslStreamCallback = delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          };

          try {
            using (var response = request.GetResponse()) {
            }
          }
          catch (WebException ex) {
            WebExceptionAssertion(WebExceptionStatus.SecureChannelFailure, ex);

            var upgradeException = ex.InnerException as Smdn.Net.Imap4.Protocol.ImapUpgradeConnectionException;

            Assert.IsNotNull(upgradeException);
            Assert.IsNotNull(upgradeException.InnerException);
            Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException),
                                    upgradeException.InnerException);
          }
        }
        finally {
          ImapSessionManager.CreateSslStreamCallback = defaultCallback;
        }
      }
    }

    [Test]
    public void TestCreateSessionSecureChannelFailureStartTlsBad()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");
        // STARTTLS response
        server.EnqueueResponse("0001 BAD done\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.SecureChannelFailure, ex);
          Assert.IsNull(ex.InnerException);
        }
      }
    }

    [Test]
    public void TestCreateSessionInternalError()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0};auth=digest-md5@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.Credentials = credential;
        request.KeepAlive = false;
        request.UsingSaslMechanisms = new[] {"DIGEST-MD5"};
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5 AUTH=CRAM-MD5 AUTH=NTLM AUTH=PLAIN AUTH=LOGIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE DIGEST-MD5 response
        server.EnqueueResponse("+ xxx-invalid-response-xxx\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.UnknownError, ex);
          Assert.IsInstanceOfType(typeof(ImapException), ex.InnerException);
        }
      }
    }

    [Test]
    public void TestCreateSessionTimeout()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 500;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response (not respond tagged response)
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.Timeout, ex);
        }
      }
    }

    [Test]
    public void TestCreateSessionUseTlsIfAvailable()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var requestUri = new Uri(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort));
        var request = WebRequest.Create(requestUri) as ImapWebRequest;

        request.Credentials = credential;
        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

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
        // NOOP response
        server.EnqueueResponse("0004 OK done\r\n");
        // LOGOUT response
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0005 OK done\r\n");

        var defaultCallback = ImapSessionManager.CreateSslStreamCallback;

        try {
          var streamUpgraded = false;

          ImapSessionManager.CreateSslStreamCallback = delegate(ConnectionBase connection, Stream baseStream) {
            streamUpgraded = true;
            return baseStream; // TODO: return SSL stream
          };

          using (var response = request.GetResponse()) {
            Assert.AreEqual(response.ResponseUri, requestUri);
          }

          Assert.IsTrue(streamUpgraded, "stream upgraded");

          server.DequeueRequest(); // CAPABILITY
          StringAssert.StartsWith("0001 STARTTLS", server.DequeueRequest());
        }
        finally {
          ImapSessionManager.CreateSslStreamCallback = defaultCallback;
        }
      }
    }

    [Test]
    public void TestCreateSessionAppropriateMechanismNotFound()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var requestUri = new Uri(string.Format("imap://{0}/", server.HostPort));
        var request = WebRequest.Create(requestUri) as ImapWebRequest;

        request.Credentials = credential;
        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY response
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 LOGINDISABLED\r\n" +
                               "0000 OK done\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.RequestCanceled, ex);
        }

        server.DequeueRequest(); // CAPABILITY
      }
    }

    [Test]
    public void TestCreateSessionSendReceiveID()
    {
      var defaultClientId = new Dictionary<string, string>(ImapWebRequestDefaults.ClientID);

      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.Credentials = credential;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1 ID] ready\r\n");
        // LOGIN
        server.EnqueueResponse("0000 OK [CAPABILITY IMAP4rev1 ID] done\r\n");
        // ID
        server.EnqueueResponse("* ID (\"name\" \"ImapPseudoServer\")\r\n" +
                               "0001 OK done\r\n");
        // NOOP response
        server.EnqueueResponse("0002 OK done\r\n");
        // LOGOUT response
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        try {
          ImapWebRequestDefaults.ClientID.Clear();
          ImapWebRequestDefaults.ClientID["name"] = "test-client";
          ImapWebRequestDefaults.ClientID["version"] = "1.0";

          using (var response = request.GetResponse() as ImapWebResponse) {
            Assert.IsNotNull(response.ServerID);
            Assert.IsTrue(response.ServerID.IsReadOnly);
            Assert.AreEqual(1, response.ServerID.Count);
            Assert.AreEqual("ImapPseudoServer", response.ServerID["name"]);
            Assert.AreEqual("ImapPseudoServer", response.ServerID["NAME"]);

            server.DequeueRequest(); // LOGIN
            StringAssert.StartsWith("0001 ID (\"name\" \"test-client\" \"version\" \"1.0\")",
                                    server.DequeueRequest());
          }
        }
        finally {
          ImapWebRequestDefaults.ClientID.Clear();

          foreach (var pair in defaultClientId) {
            ImapWebRequestDefaults.ClientID.Add(pair.Key, pair.Value);
          }
        }
      }
    }
  }
}
