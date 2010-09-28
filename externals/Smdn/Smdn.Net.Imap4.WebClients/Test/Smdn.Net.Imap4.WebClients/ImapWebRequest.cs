using System;
using System.Net;
using System.Threading;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapWebRequestTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private void UsingAsyncStartedRequest(Action<ImapWebRequest> action)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

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
      var request = WebRequest.Create("imap://localhost/");

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
      var serverRequest = WebRequest.Create("imap://localhost/");

      SetAndAssertMethodEquals(serverRequest, ImapWebRequestMethods.List);
      SetAndAssertMethodEquals(serverRequest, ImapWebRequestMethods.Lsub);
      SetAndAssertMethodEquals(serverRequest, ImapWebRequestMethods.NoOp);
      SetAndAssertMethodEquals(serverRequest, ImapWebRequestMethods.XList);

      var mailboxRequest = WebRequest.Create("imap://localhost/INBOX");

      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Append);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Check);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Create);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Delete);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Examine);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Expunge);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Fetch);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.NoOp);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Rename);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Status);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Subscribe);
      SetAndAssertMethodEquals(mailboxRequest, ImapWebRequestMethods.Unsubscribe);

      var fetchMessageRequest = WebRequest.Create("imap://localhost/INBOX/;UID=1");

      SetAndAssertMethodEquals(fetchMessageRequest, ImapWebRequestMethods.Copy);
      SetAndAssertMethodEquals(fetchMessageRequest, ImapWebRequestMethods.Expunge);
      SetAndAssertMethodEquals(fetchMessageRequest, ImapWebRequestMethods.Fetch);
      SetAndAssertMethodEquals(fetchMessageRequest, ImapWebRequestMethods.NoOp);
      SetAndAssertMethodEquals(fetchMessageRequest, ImapWebRequestMethods.Store);

      var searchMessageRequest = WebRequest.Create("imap://localhost/INBOX?UID 1");

      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.Copy);
      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.Expunge);
      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.NoOp);
      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.Search);
      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.Sort);
      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.Store);
      SetAndAssertMethodEquals(searchMessageRequest, ImapWebRequestMethods.Thread);
    }

    private void SetAndAssertMethodEquals(WebRequest request, string method)
    {
      request.Method = method;

      Assert.AreEqual(method, request.Method);
    }

    [Test]
    public void TestSetMethodInvalid()
    {
      var request = WebRequest.Create("imap://localhost/");

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

      var serverRequest = WebRequest.Create("imap://localhost/");

      try {
        serverRequest.Method = ImapWebRequestMethods.Fetch;
        Assert.Fail("ArgumentException not thrown (set invalid)");
      }
      catch (ArgumentException) {
      }

      var mailboxRequest = WebRequest.Create("imap://localhost/INBOX");

      try {
        mailboxRequest.Method = ImapWebRequestMethods.List;
        Assert.Fail("ArgumentException not thrown (set invalid)");
      }
      catch (ArgumentException) {
      }

      var fetchMessageRequest = WebRequest.Create("imap://localhost/INBOX/;UID=1");

      try {
        fetchMessageRequest.Method = ImapWebRequestMethods.Append;
        Assert.Fail("ArgumentException not thrown (set invalid)");
      }
      catch (ArgumentException) {
      }

      var searchMessageRequest = WebRequest.Create("imap://localhost/INBOX?UID 1");

      try {
        searchMessageRequest.Method = ImapWebRequestMethods.Delete;
        Assert.Fail("ArgumentException not thrown (set invalid)");
      }
      catch (ArgumentException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetMethodRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.Method = ImapWebRequestMethods.Fetch;
      });
    }

    [Test]
    public void TestSetDestinationUri()
    {
      var request = WebRequest.Create("imap://user;auth=digest-md5@localhost:10143/") as ImapWebRequest;

      request.DestinationUri = new Uri("imap://user;auth=digest-md5@localhost:10143/INBOX");

      try {
        request.DestinationUri = null;
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }

      try {
        request.DestinationUri = new Uri("imaps://user;auth=digest-md5@localhost:10143/");
        Assert.Fail("ArgumentException not thrown (different scheme)");
      }
      catch (ArgumentException) {
      }

      try {
        request.DestinationUri = new Uri("imap://anotheruser;auth=digest-md5@localhost:10143/");
        Assert.Fail("ArgumentException not thrown (different username)");
      }
      catch (ArgumentException) {
      }

      try {
        request.DestinationUri = new Uri("imap://user;auth=ntlm@localhost:10143/");
        Assert.Fail("ArgumentException not thrown (different auth-type)");
      }
      catch (ArgumentException) {
      }

      try {
        request.DestinationUri = new Uri("imap://user;auth=digest-md5@imap.example.com:10143/");
        Assert.Fail("ArgumentException not thrown (different host)");
      }
      catch (ArgumentException) {
      }

      try {
        request.DestinationUri = new Uri("imap://user;auth=digest-md5@localhost:143/");
        Assert.Fail("ArgumentException not thrown (different port)");
      }
      catch (ArgumentException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetDestinationUriRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.DestinationUri = request.RequestUri;
      });
    }

    [Test]
    public void TestSetTimeout()
    {
      var request = WebRequest.Create("imap://localhost/");

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
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.Timeout = 1000;
      });
    }

    [Test]
    public void TestSetReadWriteTimeout()
    {
      var request = WebRequest.Create("imap://localhost/") as ImapWebRequest;

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
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.ReadWriteTimeout = 1000;
      });
    }

    [Test]
    public void TestSetContentLength()
    {
      var request = WebRequest.Create("imap://localhost/");

      request.ContentLength = 0L;
      request.ContentLength = long.MaxValue;

      try {
        request.ContentLength = -1;
        Assert.Fail("ArgumentOutOfRangeException not thrown");
      }
      catch (ArgumentOutOfRangeException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetContentLengthRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.ContentLength = 1024L;
      });
    }

    [Test]
    public void TestSetFetchBlockSize()
    {
      var request = WebRequest.Create("imap://localhost/") as ImapWebRequest;

      request.FetchBlockSize = 1;
      request.FetchBlockSize = int.MaxValue;

      try {
        request.FetchBlockSize = 0;
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetFetchBlockSizeRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.FetchBlockSize = 8192;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetFetchPeekRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.FetchPeek = false;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetFetchDataItemRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.FetchDataItem = ImapFetchDataItemMacro.Fast;
      });
    }

    [Test]
    public void TestSetStoreDataItem()
    {
      var request = WebRequest.Create("imap://localhost/") as ImapWebRequest;

      request.StoreDataItem = ImapStoreDataItem.AddFlags(ImapMessageFlag.Deleted);

      try {
        request.StoreDataItem = null;
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetStoreDataItemRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.StoreDataItem = ImapStoreDataItem.AddFlags(ImapMessageFlag.Deleted);
      });
    }

    [Test]
    public void TestSetStatusDataItem()
    {
      var request = WebRequest.Create("imap://localhost/INBOX/") as ImapWebRequest;

      request.StatusDataItem = ImapStatusDataItem.UidNext;

      try {
        request.StatusDataItem = null;
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetStatusDataItemRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.StatusDataItem = ImapStatusDataItem.Messages;
      });
    }

    [Test]
    public void TestSetSortCriteria()
    {
      var request = WebRequest.Create("imap://localhost/INBOX?UNSEEN") as ImapWebRequest;

      request.SortCriteria = ImapSortCriteria.Subject;

      try {
        request.SortCriteria = null;
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetSortCriteriaRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.SortCriteria = ImapSortCriteria.Subject;
      });
    }

    [Test]
    public void TestSetThreadingAlgorithm()
    {
      var request = WebRequest.Create("imap://localhost/INBOX?UNSEEN") as ImapWebRequest;

      request.ThreadingAlgorithm = ImapThreadingAlgorithm.References;

      try {
        request.ThreadingAlgorithm = null;
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetThreadingAlgorithmRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.ThreadingAlgorithm = ImapThreadingAlgorithm.References;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetKeepAliveRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.KeepAlive = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetReadOnlyRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.ReadOnly = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetExpectedErrorResponseCodesRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.ExpectedErrorResponseCodes = new[] {ImapResponseCode.AlreadyExists};
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetSubscriptionRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.Subscription = false;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetAllowCreateMailboxRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.AllowCreateMailbox = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetUseTlsIfAvailableRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.UseTlsIfAvailable = true;
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestSetInsecureLoginRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.AllowInsecureLogin = true;
      });
    }

    [Test, ExpectedException(typeof(NotImplementedException))]
    public void TestGetPreAuthenticate()
    {
      Assert.IsFalse(WebRequest.Create("imap://localhost/").PreAuthenticate);
    }

    [Test, ExpectedException(typeof(NotImplementedException))]
    public void TestSetPreAuthenticate()
    {
      WebRequest.Create("imap://localhost/").PreAuthenticate = false;
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestGetRequestStream()
    {
      using (var stream = WebRequest.Create("imap://localhost/").GetRequestStream()) {
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetRequestStreamRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.GetRequestStream();
      });
    }

    [Test]
    public void TestBeginGetResponse()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Method = "NOOP";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ready\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // NOOP
        server.EnqueueResponse("0001 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0002 OK done\r\n");

        var asyncState = "async state";
        var asyncResult = request.BeginGetResponse(null, asyncState);

        Assert.IsNotNull(asyncResult);
        Assert.AreEqual(asyncResult.AsyncState, asyncState);

        using (var response = request.EndGetResponse(asyncResult)) {
        }

        StringAssert.Contains("LOGIN", server.DequeueRequest());
        StringAssert.Contains("NOOP", server.DequeueRequest());
        StringAssert.Contains("LOGOUT", server.DequeueRequest());
      }
    }

    [Test]
    public void TestBeginGetResponseCallback()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Method = "NOOP";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK [CAPABILITY IMAP4rev1] ready\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");

        var asyncResult = request.BeginGetResponse(BeginGetResponseCallbackProc, request);

        Assert.IsNotNull(asyncResult);
        Assert.AreEqual(asyncResult.AsyncState, request);

        Thread.Sleep(1000);

        StringAssert.Contains("LOGIN", server.DequeueRequest());

        // NOOP
        server.EnqueueResponse("0001 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0002 OK done\r\n");

        if (!asyncResult.AsyncWaitHandle.WaitOne(5000))
          Assert.Fail("not completed");

        StringAssert.Contains("NOOP", server.DequeueRequest());
        StringAssert.Contains("LOGOUT", server.DequeueRequest());
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
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
        request.GetRequestStream();
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestEndGetResponseInvalidAsyncResult()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request1 = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;
        var request2 = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        var asyncResult1 = request1.BeginGetResponse(null, null);
        var asyncResult2 = request2.BeginGetResponse(null, null);

        request1.EndGetResponse(asyncResult2);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetResponseRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Timeout = timeout;
        request.ReadWriteTimeout = readWriteTimeout;
        request.Method = "NOOP";

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // not respond to NOOP

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.Timeout, ex.Status);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseOk()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Timeout = 1000;
        request.Method = "NOOP";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // NOOP
        server.EnqueueResponse("0002 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        using (var response = request.GetResponse()) {
          Assert.IsNotNull(response);
          Assert.IsInstanceOfType(typeof(ImapWebResponse), response);

          var r = response as ImapWebResponse;

          Assert.IsNotNull(r.Result);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseNo()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Timeout = 1000;
        request.Method = "SUBSCRIBE";
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SUBSCRIBE
        server.EnqueueResponse("0002 NO failure\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);

          Assert.IsNotNull(ex.Response);
          Assert.IsInstanceOfType(typeof(ImapWebResponse),ex.Response);

          var r = ex.Response as ImapWebResponse;

          Assert.IsNotNull(r.Result);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("SUBSCRIBE", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseNoWithExpectedResponseCode()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Timeout = 1000;
        request.Method = "SELECT";
        request.ExpectedErrorResponseCodes = new[] {ImapResponseCode.PrivacyRequired};
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0002 NO [PRIVACYREQUIRED] failed\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsTrue(response.Result.Failed);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseNoWithUnxpectedResponseCode()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Timeout = 1000;
        request.Method = "SELECT";
        request.ExpectedErrorResponseCodes = new[] {ImapResponseCode.PrivacyRequired};
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0002 NO [NOPERM] failed\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        try {
          using (var response = request.GetResponse() as ImapWebResponse) {
            Assert.Fail("WebException not thrown");
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);

          var response = ex.Response as ImapWebResponse;

          Assert.IsNotNull(response);
          Assert.AreEqual(response.ResponseCode, ImapResponseCode.NoPerm);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.EndsWith("SELECT \"INBOX\"\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetResponseKeepAliveFalse()
    {
      using (var server = new ImapPseudoServer()) {
        for (var req = 0; req < 3; req++) {
          server.Start();

          /*
           * request/response
           */
          var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

          request.Timeout = 1000;
          request.Method = "NOOP";
          request.KeepAlive = false;

          // greeting
          server.EnqueueResponse("* OK ready\r\n");
          // CAPABILITY
          server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                                 "0000 OK done\r\n");
          // LOGIN
          server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                                 "0001 OK done\r\n");
          // NOOP
          server.EnqueueResponse("0002 OK done\r\n");
          // LOGOUT
          server.EnqueueResponse("* BYE logging out\r\n" +
                                 "0003 OK done\r\n");

          using (var response = request.GetResponse()) {
          }

          server.DequeueRequest(); // CAPABILITY
          server.DequeueRequest(); // LOGIN
          StringAssert.Contains("NOOP", server.DequeueRequest(), "request #{0}", req);
          StringAssert.Contains("LOGOUT", server.DequeueRequest(), "request #{0}", req);

          server.Stop();
        }
      }
    }

    [Test]
    public void TestGetResponseKeepAliveTrue()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        for (var req = 0; req < 3; req++) {
          /*
           * request/response
           */
          var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

          request.Timeout = 1000;
          request.Method = "NOOP";
          request.KeepAlive = true;

          if (req == 0) {
            // greeting
            server.EnqueueResponse("* OK ready\r\n");
            // CAPABILITY
            server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                                   "0000 OK done\r\n");
            // LOGIN
            server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                                   "0001 OK done\r\n");
            // NOOP
            server.EnqueueResponse("0002 OK done\r\n");
          }
          else if (req == 1) {
            // NOOP
            server.EnqueueResponse("0003 OK done\r\n");
          }
          else if (req == 2) {
            // NOOP
            server.EnqueueResponse("0004 OK done\r\n");
          }

          using (var response = request.GetResponse()) {
          }

          if (req == 0) {
            server.DequeueRequest(); // CAPABILITY
            server.DequeueRequest(); // LOGIN
          }

          StringAssert.Contains("NOOP", server.DequeueRequest(), "request #{0}", req);
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
      using (var server = new ImapPseudoServer()) {
        for (var req = 0; req < 2; req++) {
          server.Start();

          /*
           * request/response
           */
          var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

          request.Timeout = 1000;
          request.Method = "NOOP";
          request.KeepAlive = true;

          // greeting
          server.EnqueueResponse("* OK ready\r\n");
          // CAPABILITY
          server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                                 "0000 OK done\r\n");
          // LOGIN
          server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                                 "0001 OK done\r\n");
          // NOOP
          server.EnqueueResponse("0002 OK done\r\n");

          using (var response = request.GetResponse()) {
          }

          server.DequeueRequest(); // CAPABILITY
          server.DequeueRequest(); // LOGIN
          StringAssert.Contains("NOOP", server.DequeueRequest(), "request #{0}", req);

          if (disconnectFromServer) {
            server.Stop();
          }
          else {
            // LOGOUT
            server.EnqueueResponse("* BYE logging out\r\n" +
                                   "0003 OK done\r\n");

            ImapSessionManager.DisconnectFrom(request.RequestUri);

            server.DequeueRequest(); // LOGOUT

            server.Stop();
          }
        }
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestUsingSaslMechanismsRequestStarted()
    {
      UsingAsyncStartedRequest(delegate(ImapWebRequest request) {
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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.UsingSaslMechanisms = mechanisms;
        request.Credentials = credential;
        request.Timeout = 1000;
        request.Method = "NOOP";
        request.KeepAlive = true;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5 AUTH=NTLM AUTH=CRAM-MD5 AUTH=PLAIN AUTH=LOGIN\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // NOOP
        server.EnqueueResponse("0002 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        using (var response = request.GetResponse()) {
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestUsingSaslMechanismsNonEmpty()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.UsingSaslMechanisms = new string[] {"X-UNKWNON-MECHANISM", "PLAIN", "login"};
        request.Credentials = credential;
        request.Timeout = 1000;
        request.Method = "NOOP";
        request.KeepAlive = true;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1 AUTH=DIGEST-MD5 AUTH=PLAIN AUTH=NTLM AUTH=CRAM-MD5 AUTH=LOGIN\r\n" +
                               "0000 OK done\r\n");
        // AUTHENTICATE PLAIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0001 NO failure\r\n");
        // AUTHENTICATE LOGIN response
        server.EnqueueResponse("+ \r\n");
        server.EnqueueResponse("0002 NO failure\r\n");
        // LOGIN response
        server.EnqueueResponse("0003 NO failure\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("AUTHENTICATE PLAIN", server.DequeueRequest());
        server.DequeueRequest(); // AUTHENTICATE PLAIN client response
        StringAssert.Contains("AUTHENTICATE LOGIN", server.DequeueRequest());
      }
    }

    [Test]
    public void TestSelectRequestMailbox()
    {
      SelectRequestMailbox(false);
    }

    [Test]
    public void TestSelectRequestMailboxAsReadOnly()
    {
      SelectRequestMailbox(true);
    }

    private void SelectRequestMailbox(bool readOnly)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.Credentials = credential;
        request.Timeout = 1000;
        request.Method = "CHECK";
        request.KeepAlive = false;
        request.ReadOnly = readOnly;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT/EXAMINE
        if (readOnly)
          server.EnqueueResponse("0002 OK [READ-ONLY] done\r\n");
        else
          server.EnqueueResponse("0002 OK [READ-WRITE] done\r\n");
        // CHECK
        server.EnqueueResponse("0003 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0004 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0005 OK done\r\n");

        using (var response = request.GetResponse()) {
          Assert.AreEqual(response.ResponseUri, request.RequestUri);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("LOGIN", server.DequeueRequest());
        if (readOnly)
          StringAssert.Contains("EXAMINE \"INBOX\"", server.DequeueRequest());
        else
          StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest());
        StringAssert.Contains("CHECK", server.DequeueRequest());
        StringAssert.Contains("CLOSE", server.DequeueRequest());
        StringAssert.Contains("LOGOUT", server.DequeueRequest());
      }
    }

    [Test]
    public void TestSelectRequestMailboxReselectReadOnlyToReadWrite()
    {
      SelectRequestMailboxReselect(true);
    }

    [Test]
    public void TestSelectRequestMailboxReselectReadWriteToReadOnly()
    {
      SelectRequestMailboxReselect(false);
    }

    private void SelectRequestMailboxReselect(bool readOnlyFirst)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var firstRequest = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX", credential.UserName, server.HostPort)) as ImapWebRequest;

        firstRequest.Credentials = credential;
        firstRequest.Timeout = 1000;
        firstRequest.Method = "CHECK";
        firstRequest.KeepAlive = true;
        firstRequest.ReadOnly = readOnlyFirst;
        firstRequest.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT/EXAMINE
        if (readOnlyFirst)
          server.EnqueueResponse("0002 OK [READ-ONLY] done\r\n");
        else
          server.EnqueueResponse("0002 OK [READ-WRITE] done\r\n");
        // CHECK
        server.EnqueueResponse("0003 OK done\r\n");

        using (var response = firstRequest.GetResponse()) {
          Assert.AreEqual(response.ResponseUri, firstRequest.RequestUri);
        }

        server.DequeueRequest(); // CAPABILITY
        StringAssert.Contains("LOGIN", server.DequeueRequest());
        if (readOnlyFirst)
          StringAssert.Contains("EXAMINE \"INBOX\"", server.DequeueRequest());
        else
          StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest());
        StringAssert.Contains("CHECK", server.DequeueRequest());

        var secondRequest = WebRequest.Create(firstRequest.RequestUri) as ImapWebRequest;

        secondRequest.Credentials = credential;
        secondRequest.Timeout = 1000;
        secondRequest.Method = "CHECK";
        secondRequest.KeepAlive = false;
        secondRequest.ReadOnly = !readOnlyFirst;

        // NOOP
        server.EnqueueResponse("0004 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0005 OK done\r\n");
        // SELECT/EXAMINE
        if (readOnlyFirst)
          server.EnqueueResponse("0006 OK [READ-WRITE] done\r\n");
        else
          server.EnqueueResponse("0006 OK [READ-ONLY] done\r\n");
        // CHECK
        server.EnqueueResponse("0007 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0008 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0009 OK done\r\n");

        using (var response = secondRequest.GetResponse()) {
          Assert.AreEqual(response.ResponseUri, secondRequest.RequestUri);
        }

        server.DequeueRequest(); // NOOP
        StringAssert.Contains("CLOSE", server.DequeueRequest());
        if (readOnlyFirst)
          StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest());
        else
          StringAssert.Contains("EXAMINE \"INBOX\"", server.DequeueRequest());
        StringAssert.Contains("CHECK", server.DequeueRequest());
      }
    }

    [Test]
    public void TestSelectRequestMailboxFailure()
    {
      SelectRequestMailboxFailure(false);
    }

    [Test]
    public void TestSelectRequestMailboxAsReadOnlyFailure()
    {
      SelectRequestMailboxFailure(true);
    }

    private void SelectRequestMailboxFailure(bool readOnly)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX", credential.UserName, server.HostPort)) as ImapWebRequest;

        request.Credentials = credential;
        request.Timeout = 1000;
        request.Method = "CHECK";
        request.KeepAlive = false;
        request.ReadOnly = readOnly;
        request.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT/EXAMINE
        server.EnqueueResponse("0002 NO can't select\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

        try {
          using (var response = request.GetResponse()) {
            Assert.AreEqual(response.ResponseUri, request.RequestUri);
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        if (readOnly)
          StringAssert.Contains("EXAMINE \"INBOX\"", server.DequeueRequest());
        else
          StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCloseMailbox()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request1 = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX", credential.UserName, server.HostPort)) as ImapWebRequest;

        request1.Credentials = credential;
        request1.Timeout = 1000;
        request1.Method = "CHECK";
        request1.KeepAlive = true;
        request1.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0002 OK [READ-WRITE] done\r\n");
        // CHECK
        server.EnqueueResponse("0003 OK done\r\n");

        using (var response = request1.GetResponse()) {
          Assert.AreEqual(response.ResponseUri, request1.RequestUri);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest()); // SELECT
        server.DequeueRequest(); // CHECK

        var request2 = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX/child", credential.UserName, server.HostPort)) as ImapWebRequest;

        request2.Credentials = credential;
        request2.Timeout = 1000;
        request2.Method = "CHECK";
        request2.KeepAlive = false;

        // NOOP
        server.EnqueueResponse("0004 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0005 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0006 OK [READ-WRITE] done\r\n");
        // CHECK
        server.EnqueueResponse("0007 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0008 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0009 OK done\r\n");

        using (var response = request2.GetResponse()) {
          Assert.AreEqual(response.ResponseUri, request2.RequestUri);
        }

        server.DequeueRequest(); // NOOP

        StringAssert.Contains("CLOSE", server.DequeueRequest());
        StringAssert.Contains("SELECT \"INBOX/child\"", server.DequeueRequest());
        StringAssert.Contains("CHECK", server.DequeueRequest());
        StringAssert.Contains("CLOSE", server.DequeueRequest());
        StringAssert.Contains("LOGOUT", server.DequeueRequest());
      }
    }

    [Test]
    public void TestCloseMailboxFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var credential = new NetworkCredential("user", "pass");
        var request1 = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX", credential.UserName, server.HostPort)) as ImapWebRequest;

        request1.Credentials = credential;
        request1.Timeout = 1000;
        request1.Method = "CHECK";
        request1.KeepAlive = true;
        request1.AllowInsecureLogin = true;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0002 OK [READ-WRITE] done\r\n");
        // CHECK
        server.EnqueueResponse("0003 OK done\r\n");

        using (var response = request1.GetResponse()) {
          Assert.AreEqual(response.ResponseUri, request1.RequestUri);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest()); // SELECT
        server.DequeueRequest(); // CHECK

        var request2 = WebRequest.Create(string.Format("imap://{0}@{1}/INBOX/child", credential.UserName, server.HostPort)) as ImapWebRequest;

        request2.Credentials = credential;
        request2.Timeout = 1000;
        request2.Method = "CHECK";
        request2.KeepAlive = false;

        // NOOP
        server.EnqueueResponse("0004 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0005 OK done\r\n");
        // SELECT
        server.EnqueueResponse("0006 OK [READ-WRITE] done\r\n");
        // CHECK
        server.EnqueueResponse("0007 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0008 BAD invalid argument\r\n");

        try {
          using (var response = request2.GetResponse()) {
            Assert.AreEqual(response.ResponseUri, request2.RequestUri);
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex);
        }

        server.DequeueRequest(); // NOOP

        StringAssert.Contains("CLOSE", server.DequeueRequest());
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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;

        request.Method = ImapWebRequestMethods.NoOp;
        request.KeepAlive = false;

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // NOOP
        server.EnqueueResponse("0002 OK done\r\n");

        var asyncResult = request.BeginGetResponse(null, null);

        Thread.Sleep(500);

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("NOOP", server.DequeueRequest());

        if (test == "not responding")
          // not respond to LOGOUT
          ;
        else if (test == "disconnected")
          server.Stop();

        using (var response = request.EndGetResponse(asyncResult) as ImapWebResponse) {
          Assert.IsTrue(response.Result.Succeeded);
        }
      }
    }
  }
}
