using System;
using System.IO;
using System.Net;
using NUnit.Framework;

using Smdn.Net.Pop3.Client.Session;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopSessionManagerTests {
    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    private void WebExceptionAssertion(WebExceptionStatus expectedStatus, WebException actual)
    {
      if (expectedStatus != actual.Status)
        throw actual;

      if (expectedStatus == WebExceptionStatus.ProtocolError) {
        Assert.IsNotNull(actual.Response);
        Assert.IsInstanceOfType(typeof(PopWebResponse), actual.Response);

        var response = actual.Response as PopWebResponse;

        Assert.IsTrue(response.Result.Failed);
      }
    }

    [Test]
    public void TestCreateSessionConnectFailure()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

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
    public void TestCreateSessionSecureChannelFailureSecurePort()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pops://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        var defaultCallback = PopSessionManager.CreateSslStreamCallback;

        try {
          PopSessionManager.CreateSslStreamCallback = delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          };

          try {
            using (var response = request.GetResponse()) {
            }
          }
          catch (WebException ex) {
            WebExceptionAssertion(WebExceptionStatus.SecureChannelFailure, ex);
            Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException), ex.InnerException);
          }
        }
        finally {
          PopSessionManager.CreateSslStreamCallback = defaultCallback;
        }
      }
    }

    [Test]
    public void TestCreateSessionSecureChannelFailureStartTlsException()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // STLS response
        server.EnqueueResponse("+OK\r\n");

        var defaultCallback = PopSessionManager.CreateSslStreamCallback;

        try {
          PopSessionManager.CreateSslStreamCallback = delegate(ConnectionBase connection, Stream baseStream) {
            throw new System.Security.Authentication.AuthenticationException();
          };

          try {
            using (var response = request.GetResponse()) {
            }
          }
          catch (WebException ex) {
            WebExceptionAssertion(WebExceptionStatus.SecureChannelFailure, ex);
            Assert.IsInstanceOfType(typeof(System.Security.Authentication.AuthenticationException), ex.InnerException);
          }
        }
        finally {
          PopSessionManager.CreateSslStreamCallback = defaultCallback;
        }
      }
    }

    [Test]
    public void TestCreateSessionSecureChannelFailureStartTlsErr()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "STLS\r\n" +
                               ".\r\n");
        // STLS response
        server.EnqueueResponse("-ERR\r\n");

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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0};auth=digest-md5@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.Credentials = credential;
        request.KeepAlive = false;
        request.UsingSaslMechanisms = new[] {"DIGEST-MD5"};
        request.Timeout = 1000;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 CRAM-MD5 NTLM PLAIN LOGIN\r\n" +
                               ".\r\n");
        // AUTH DIGEST-MD5 response
        server.EnqueueResponse("+ xxx-invalid-response-xxx\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.UnknownError, ex);
          Assert.IsInstanceOfType(typeof(PopException), ex.InnerException);
        }
      }
    }

    [Test]
    public void TestCreateSessionTimeout()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 500;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPABILITY response (not respond termination response)
        server.EnqueueResponse("+OK\r\n");

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
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var requestUri = new Uri(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort));
        var request = WebRequest.Create(requestUri) as PopWebRequest;

        request.Credentials = credential;
        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Method = "NOOP";
        request.Timeout = 1000;

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
        // NOOP response
        server.EnqueueResponse("+OK\r\n");
        // QUIT response
        server.EnqueueResponse("+OK\r\n");

        var defaultCallback = PopSessionManager.CreateSslStreamCallback;

        try {
          var streamUpgraded = false;

          PopSessionManager.CreateSslStreamCallback = delegate(ConnectionBase connection, Stream baseStream) {
            streamUpgraded = true;
            return baseStream; // TODO: return SSL stream
          };

          using (var response = request.GetResponse()) {
            Assert.AreEqual(response.ResponseUri, requestUri);
          }

          Assert.IsTrue(streamUpgraded, "stream upgraded");

          server.DequeueRequest(); // CAPA
          StringAssert.StartsWith("STLS", server.DequeueRequest());
        }
        finally {
          PopSessionManager.CreateSslStreamCallback = defaultCallback;
        }
      }
    }

    [Test]
    public void TestCreateSessionAppropriateMechanismNotFound()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var requestUri = new Uri(string.Format("pop://user@{0}/", server.HostPort));
        var request = WebRequest.Create(requestUri) as PopWebRequest;

        request.Credentials = credential;
        request.UseTlsIfAvailable = true;
        request.KeepAlive = false;
        request.Timeout = 1000;
        request.AllowInsecureLogin = false; // LOGINDISABLE equiv

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA response
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          WebExceptionAssertion(WebExceptionStatus.RequestCanceled, ex);
        }
      }
    }
  }
}
