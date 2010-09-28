using System;
using System.Net;
using System.Threading;
using NUnit.Framework;

using Smdn.Net.Pop3.Protocol;

using PopPseudoServer = Smdn.Net.Pop3.Client.Session.PopPseudoServer;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopWebRequestTests {
    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    private void UsingAsyncStartedRequest(Action<PopWebRequest> action)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Method = "NOOP";
        request.KeepAlive = false;

        var asyncResult = request.BeginGetResponse(null, null);

        Assert.IsNotNull(asyncResult);

        action(request);

        using (var response = request.EndGetResponse(asyncResult)) {
        }
      }
    }

    class Credentials : ICredentials {
      public NetworkCredential GetCredential(Uri uri, string authType)
      {
        throw new NotImplementedException();
      }
    }

    class CredentialsByHost : Credentials, ICredentialsByHost {
      public NetworkCredential GetCredential(string host, int port, string authType)
      {
        throw new NotImplementedException();
      }
    }

    [Test]
    public void TestSetCredentials()
    {
      var request = WebRequest.Create("pop://localhost/");

      request.Credentials = null;
      request.Credentials = new NetworkCredential();
      request.Credentials = new CredentialCache();

      request.Credentials = new CredentialsByHost();

      try {
        request.Credentials = new Credentials();
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestSetMethod()
    {
      var mailboxRequest = WebRequest.Create("pop://localhost/");

      SetAndAssertMethodEquals(mailboxRequest, PopWebRequestMethods.List);
      SetAndAssertMethodEquals(mailboxRequest, PopWebRequestMethods.NoOp);
      SetAndAssertMethodEquals(mailboxRequest, PopWebRequestMethods.Rset);
      SetAndAssertMethodEquals(mailboxRequest, PopWebRequestMethods.Stat);
      SetAndAssertMethodEquals(mailboxRequest, PopWebRequestMethods.Uidl);

      var messageRequest = WebRequest.Create("pop://localhost/;MSG=1");

      SetAndAssertMethodEquals(messageRequest, PopWebRequestMethods.Dele);
      SetAndAssertMethodEquals(messageRequest, PopWebRequestMethods.List);
      SetAndAssertMethodEquals(messageRequest, PopWebRequestMethods.NoOp);
      SetAndAssertMethodEquals(messageRequest, PopWebRequestMethods.Retr);
      SetAndAssertMethodEquals(messageRequest, PopWebRequestMethods.Top);
      SetAndAssertMethodEquals(messageRequest, PopWebRequestMethods.Uidl);
    }

    private void SetAndAssertMethodEquals(WebRequest request, string method)
    {
      request.Method = method;

      Assert.AreEqual(method, request.Method);
    }

    [Test]
    public void TestSetMethodInvalid()
    {
      var request = WebRequest.Create("pop://localhost/");

      try {
        request.Method = null;
        Assert.Fail("ArgumentException not thrown (set null)");
      }
      catch (ArgumentException) {
      }

      try {
        request.Method = string.Empty;
        Assert.Fail("ArgumentException not thrown (set empty)");
      }
      catch (ArgumentException) {
      }

      var mailboxRequest = WebRequest.Create("pop://localhost/");

      try {
        mailboxRequest.Method = PopWebRequestMethods.Retr;
        Assert.Fail("ArgumentException not thrown (set invalid)");
      }
      catch (ArgumentException) {
      }

      var messageRequest = WebRequest.Create("pop://localhost/;MSG=1");

      try {
        messageRequest.Method = PopWebRequestMethods.Stat;
        Assert.Fail("ArgumentException not thrown (set invalid)");
      }
      catch (ArgumentException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetMethodRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.Method = PopWebRequestMethods.Stat;
      });
    }

    [Test]
    public void TestSetTimeout()
    {
      var request = WebRequest.Create("pop://localhost/");

      request.Timeout = 0;
      request.Timeout = int.MaxValue;
      request.Timeout = System.Threading.Timeout.Infinite;

      try {
        request.Timeout = -2;
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetTimeoutRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.Timeout = 1000;
      });
    }

    [Test]
    public void TestSetReadWriteTimeout()
    {
      var request = WebRequest.Create("pop://localhost/") as PopWebRequest;

      request.ReadWriteTimeout = 0;
      request.ReadWriteTimeout = int.MaxValue;
      request.ReadWriteTimeout = System.Threading.Timeout.Infinite;

      try {
        request.ReadWriteTimeout = -2;
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetReadWriteTimeoutRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.ReadWriteTimeout = 1000;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetDeleteAfterRetrieveRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.DeleteAfterRetrieve = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetKeepAliveRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.KeepAlive = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetExpectedErrorResponseCodesRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.ExpectedErrorResponseCodes = new[] {PopResponseCode.SysTemp};
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetUseTlsIfAvailableRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.UseTlsIfAvailable = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetAllowInsecureLoginRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.AllowInsecureLogin = true;
      });
    }

    [Test, ExpectedException(typeof(NotImplementedException))]
    public void TestGetPreAuthenticate()
    {
      Assert.IsFalse(WebRequest.Create("pop://localhost/").PreAuthenticate);
    }

    [Test, ExpectedException(typeof(NotImplementedException))]
    public void TestSetPreAuthenticate()
    {
      WebRequest.Create("pop://localhost/").PreAuthenticate = false;
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestGetRequestStream()
    {
      using (var stream = WebRequest.Create("pop://localhost/").GetRequestStream()) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetRequestStreamRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.GetRequestStream();
      });
    }

    [Test]
    public void TestBeginGetResponse()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Method = "NOOP";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // NOOP
        server.EnqueueResponse("+OK\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        var asyncState = "async state";
        var asyncResult = request.BeginGetResponse(null, asyncState);

        Assert.IsNotNull(asyncResult);
        Assert.AreEqual(asyncResult.AsyncState, asyncState);

        using (var response = request.EndGetResponse(asyncResult)) {
        }

        StringAssert.StartsWith("CAPA", server.DequeueRequest());
        StringAssert.StartsWith("USER", server.DequeueRequest());
        StringAssert.StartsWith("PASS", server.DequeueRequest());
        StringAssert.StartsWith("NOOP", server.DequeueRequest());
        StringAssert.StartsWith("QUIT", server.DequeueRequest());
      }
    }

    [Test]
    public void TestBeginGetResponseCallback()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Method = "NOOP";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");

        var asyncResult = request.BeginGetResponse(BeginGetResponseCallbackProc, request);

        Assert.IsNotNull(asyncResult);
        Assert.AreEqual(asyncResult.AsyncState, request);

        Thread.Sleep(1000);

        StringAssert.StartsWith("CAPA", server.DequeueRequest());
        StringAssert.StartsWith("USER", server.DequeueRequest());
        StringAssert.StartsWith("PASS", server.DequeueRequest());

        // NOOP
        server.EnqueueResponse("+OK\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        if (!asyncResult.AsyncWaitHandle.WaitOne(5000))
          Assert.Fail("not completed");

        StringAssert.StartsWith("NOOP", server.DequeueRequest());
        StringAssert.StartsWith("QUIT", server.DequeueRequest());
      }
    }

    private void BeginGetResponseCallbackProc(IAsyncResult asyncResult)
    {
      var request = asyncResult.AsyncState as WebRequest;

      for (;;) {
        if (asyncResult.IsCompleted)
          break;

        System.Threading.Thread.Sleep(50);
      }

      using (var response = request.EndGetResponse(asyncResult)) {
      }
    }


    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestBeginGetResponseRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.GetRequestStream();
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestEndGetResponseInvalidAsyncResult()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request1 = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;
        var request2 = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        var asyncResult1 = request1.BeginGetResponse(null, null);
        var asyncResult2 = request2.BeginGetResponse(null, null);

        request1.EndGetResponse(asyncResult2);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetResponseRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.GetResponse();
      });
    }

    [Test]
    public void TestGetResponseTimeout()
    {
      GetResponseTimeout(500, System.Threading.Timeout.Infinite);
    }

    [Test]
    public void TestGetResponseReadWriteTimeout()
    {
      GetResponseTimeout(System.Threading.Timeout.Infinite, 500);
    }

    private void GetResponseTimeout(int timeout, int readWriteTimeout)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Timeout = timeout;
        request.ReadWriteTimeout = readWriteTimeout;
        request.Method = "NOOP";

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // not respond to NOOP

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.Timeout, ex.Status);
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseOk()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Timeout = 1000;
        request.Method = "NOOP";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // NOOP
        server.EnqueueResponse("+OK\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        using (var response = request.GetResponse()) {
          Assert.IsNotNull(response);
          Assert.IsInstanceOfType(typeof(PopWebResponse), response);

          var r = response as PopWebResponse;

          Assert.IsNotNull(r.Result);
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseErr()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/;MSG=1", server.HostPort)) as PopWebRequest;

        request.Timeout = 1000;
        request.Method = "DELE";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // DELE
        server.EnqueueResponse("-ERR\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);

          Assert.IsNotNull(ex.Response);
          Assert.IsInstanceOfType(typeof(PopWebResponse),ex.Response);

          var r = ex.Response as PopWebResponse;

          Assert.IsNotNull(r.Result);
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("DELE", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseErrWithExpectedResponseCode()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/;MSG=1", server.HostPort)) as PopWebRequest;

        request.Timeout = 1000;
        request.Method = "DELE";
        request.ExpectedErrorResponseCodes = new[] {PopResponseCode.SysTemp};
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // DELE
        server.EnqueueResponse("-ERR [SYS/TEMP]\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        using (var response = request.GetResponse() as PopWebResponse) {
          Assert.IsTrue(response.Result.Failed);
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("DELE", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseErrWithUnxpectedResponseCode()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/;MSG=1", server.HostPort)) as PopWebRequest;

        request.Timeout = 1000;
        request.Method = "DELE";
        request.ExpectedErrorResponseCodes = new[] {PopResponseCode.SysTemp};
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // DELE
        server.EnqueueResponse("-ERR [SYS/PERM]\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        try {
          using (var response = request.GetResponse()) {
            Assert.Fail("WebException not thrown");
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);

          var response = ex.Response as PopWebResponse;

          Assert.IsNotNull(response);
          Assert.AreEqual(response.ResponseCode, PopResponseCode.SysPerm);
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("DELE", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseKeepAliveFalse()
    {
      using (var server = new PopPseudoServer()) {
        for (var req = 0; req < 3; req++) {
          server.Start();

          /*
           * request/response
           */
          var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

          request.Timeout = 1000;
          request.Method = "NOOP";
          request.KeepAlive = false;

          // greeting
          server.EnqueueResponse("+OK\r\n");
          // CAPA
          server.EnqueueResponse("+OK\r\n" +
                                 ".\r\n");
          // USER
          server.EnqueueResponse("+OK\r\n");
          // PASS
          server.EnqueueResponse("+OK\r\n");
          // NOOP
          server.EnqueueResponse("+OK\r\n");
          // QUIT
          server.EnqueueResponse("+OK\r\n");

          using (var response = request.GetResponse()) {
          }

          server.DequeueRequest(); // CAPA
          server.DequeueRequest(); // USER
          server.DequeueRequest(); // PASS
          StringAssert.StartsWith("NOOP", server.DequeueRequest(), "request #{0}", req);
          StringAssert.StartsWith("QUIT", server.DequeueRequest(), "request #{0}", req);

          server.Stop();
        }
      }
    }

    [Test]
    public void TestGetResponseKeepAliveTrue()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        for (var req = 0; req < 3; req++) {
          /*
           * request/response
           */
          var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

          request.Timeout = 1000;
          request.Method = "NOOP";
          request.KeepAlive = true;

          if (req == 0) {
            // greeting
            server.EnqueueResponse("+OK\r\n");
            // CAPA
            server.EnqueueResponse("+OK\r\n" +
                                   ".\r\n");
            // USER
            server.EnqueueResponse("+OK\r\n");
            // PASS
            server.EnqueueResponse("+OK\r\n");
          }

          // NOOP
          server.EnqueueResponse("+OK\r\n");

          using (var response = request.GetResponse()) {
          }

          if (req == 0) {
            server.DequeueRequest(); // CAPA
            server.DequeueRequest(); // USER
            server.DequeueRequest(); // PASS
          }

          StringAssert.StartsWith("NOOP", server.DequeueRequest(), "request #{0}", req);
        }
      }
    }

    [Test]
    public void TestGetResponseKeepAliveTrueDisconnectedFromServer()
    {
      KeepAliveAndDisconnected(true);
    }

    [Test]
    public void TestGetResponseKeepAliveTrueDisconnectedFromClient()
    {
      KeepAliveAndDisconnected(false);
    }

    private void KeepAliveAndDisconnected(bool disconnectFromServer)
    {
      using (var server = new PopPseudoServer()) {
        for (var req = 0; req < 2; req++) {
          server.Start();

          /*
           * request/response
           */
          var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

          request.Timeout = 1000;
          request.Method = "NOOP";
          request.KeepAlive = true;

          // greeting
          server.EnqueueResponse("+OK\r\n");
          // CAPA
          server.EnqueueResponse("+OK\r\n" +
                                 ".\r\n");
          // USER
          server.EnqueueResponse("+OK\r\n");
          // PASS
          server.EnqueueResponse("+OK\r\n");
          // NOOP
          server.EnqueueResponse("+OK\r\n");

          using (var response = request.GetResponse()) {
          }

          server.DequeueRequest(); // CAPA
          server.DequeueRequest(); // USER
          server.DequeueRequest(); // PASS
          StringAssert.StartsWith("NOOP", server.DequeueRequest(), "request #{0}", req);

          if (disconnectFromServer) {
            server.Stop();
          }
          else {
            // QUIT
            server.EnqueueResponse("+OK\r\n");
            PopSessionManager.DisconnectFrom(request.RequestUri);

            server.DequeueRequest(); // QUIT

            server.Stop();
          }
        }
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestUsingSaslMechanismsRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(PopWebRequest request) {
        request.UsingSaslMechanisms = new[] {"PLAIN", "NTLM"};
      });
    }

    [Test]
    public void TestUsingSaslMechanismsNull()
    {
      AuthenticateWithSpecifiedSaslMechanisms(null);
    }

    [Test]
    public void TestUsingSaslMechanismsEmpty()
    {
      AuthenticateWithSpecifiedSaslMechanisms(new string[0]);
    }

    private void AuthenticateWithSpecifiedSaslMechanisms(string[] mechanisms)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.UsingSaslMechanisms = mechanisms;
        request.Credentials = credential;
        request.Timeout = 1000;
        request.Method = "NOOP";
        request.KeepAlive = true;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 NTLM CRAM-MD5 PLAIN LOGIN\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // NOOP
        server.EnqueueResponse("+OK\r\n");
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        using (var response = request.GetResponse()) {
        }

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestUsingSaslMechanismsNonEmpty()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("pop://{0}@{1}/", credential.UserName, server.HostPort)) as PopWebRequest;

        request.UsingSaslMechanisms = new string[] {"X-UNKWNON-MECHANISM", "PLAIN", "login"};
        request.Credentials = credential;
        request.Timeout = 1000;
        request.Method = "NOOP";
        request.KeepAlive = true;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               "SASL DIGEST-MD5 PLAIN NTLM CRAM-MD5 LOGIN\r\n" +
                               ".\r\n");
        // AUTH PLAIN response
        server.EnqueueResponse("-ERR\r\n");
        // AUTH LOGIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("-ERR\r\n");
        // USER response
        server.EnqueueResponse("-ERR\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);
        }

        server.DequeueRequest(); // CAPA
        StringAssert.StartsWith("AUTH PLAIN", server.DequeueRequest());
        StringAssert.StartsWith("AUTH LOGIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCloseSessionNotResponding()
    {
      TestExceptionWhileCloseSession("not responding");
    }

    [Test]
    public void TestCloseSessionDisconnected()
    {
      TestExceptionWhileCloseSession("disconnected");
    }

    private void TestExceptionWhileCloseSession(string test)
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;

        request.Method = PopWebRequestMethods.NoOp;
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("+OK\r\n");
        // CAPA
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");
        // USER
        server.EnqueueResponse("+OK\r\n");
        // PASS
        server.EnqueueResponse("+OK\r\n");
        // NOOP
        server.EnqueueResponse("+OK\r\n");

        var asyncResult = request.BeginGetResponse(null, null);

        Thread.Sleep(500);

        server.DequeueRequest(); // CAPA
        server.DequeueRequest(); // USER
        server.DequeueRequest(); // PASS
        StringAssert.StartsWith("NOOP", server.DequeueRequest());

        if (test == "not responding")
          // not respond to QUIT
          ;
        else if (test == "disconnected")
          server.Stop();

        using (var response = request.EndGetResponse(asyncResult) as PopWebResponse) {
          Assert.IsTrue(response.Result.Succeeded);
        }
      }
    }
  }
}
