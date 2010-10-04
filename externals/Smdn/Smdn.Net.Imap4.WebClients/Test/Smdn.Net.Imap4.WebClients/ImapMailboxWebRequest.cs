using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapMailboxWebRequestTests {
    private bool mailboxSelected = false;

    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();

      mailboxSelected = false;
    }

    private void SelectMailbox(ImapPseudoServer server)
    {
      server.Start();

      var request = WebRequest.Create(string.Format("imap://user@{0}/INBOX", server.HostPort)) as ImapWebRequest;

      request.Credentials = new NetworkCredential("user", "pass");
      request.KeepAlive = true;
      request.Timeout = 1000;
      request.Method = ImapWebRequestMethods.Select;
      request.AllowInsecureLogin = true;

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

      using (var response = request.GetResponse()) {
      }

      mailboxSelected = true;

      server.DequeueRequest(); // CAPABILITY
      server.DequeueRequest(); // LOGIN
      server.DequeueRequest(); // SELECT
    }

    private void Request(ImapPseudoServer server, string method, string methodResponse, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, "INBOX", new[] {methodResponse}, null, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, "INBOX", methodResponses, null, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string mailbox, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, mailbox, methodResponses, null, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string mailbox, string[] methodResponses, Action<ImapWebRequest> presetRequest, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      if (!server.IsStarted)
        server.Start();

      var request = WebRequest.Create(string.Format("imap://user@{0}/{1}", server.HostPort, mailbox)) as ImapWebRequest;

      Assert.AreEqual("ImapMailboxWebRequest", request.GetType().Name);

      request.Credentials = new NetworkCredential("user", "pass");
      request.KeepAlive = false;
      request.Timeout = 1000;
      request.Method = method;
      request.AllowInsecureLogin = true;

      if (presetRequest != null)
        presetRequest(request);

      int commandTag;

      if (mailboxSelected) {
        // NOOP
        server.EnqueueResponse("0003 OK done\r\n");

        commandTag = 4;
      }
      else {
        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        commandTag = 2;
      }

      foreach (var methodResponse in methodResponses) {
        server.EnqueueResponse(methodResponse);

        commandTag++;
      }

      // LOGOUT
      server.EnqueueResponse("* BYE logging out\r\n" + 
                             string.Format("{0:x4} OK done\r\n", commandTag));

      try {
        using (var response = request.GetResponse() as ImapWebResponse) {
          responseAction(request, response);
        }
      }
      catch (WebException ex) {
        if (ex.Status == WebExceptionStatus.ProtocolError)
          responseAction(request, ex.Response as ImapWebResponse);
        else
          throw ex;
      }

      if (mailboxSelected) {
        server.DequeueRequest(); // NOOP
      }
      else {
        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
      }
    }

    [Test]
    public void TestBeginGetRequestStreamCallback()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse(string.Empty);

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;
        request.AllowInsecureLogin = true;

        var asyncResult = request.BeginGetRequestStream((AsyncCallback)BeginGetRequestStreamCallbackProc, request);

        Thread.Sleep(100); // XXX

        if (!asyncResult.AsyncWaitHandle.WaitOne(5000))
          Assert.Fail("wait time out");

        server.EnqueueResponse("0002 OK done\r\n");

        using (var response = request.GetResponse()) {
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        var appendCommand = server.DequeueAll();

        StringAssert.StartsWith("0002 APPEND \"INBOX\" ", appendCommand);
        StringAssert.EndsWith(" {12}\r\ntest message\r\n", appendCommand);
      }
    }

    private void BeginGetRequestStreamCallbackProc(IAsyncResult asyncResult)
    {
      var request = asyncResult.AsyncState as ImapWebRequest;

      var stream = request.EndGetRequestStream(asyncResult);

      var data = Encoding.ASCII.GetBytes("test message");

      stream.Write(data, 0, data.Length);
    }

    [Test, ExpectedException(typeof(ProtocolViolationException))]
    public void TestBeginGetRequestStreamInvalidMethod()
    {
      var request = WebRequest.Create("imap://localhost/INBOX");

      request.Method = "NOOP";
      request.BeginGetRequestStream(null, null);
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestBeginGetRequestStreamRequestStarted()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("0001 OK done\r\n");

        var request = WebRequest.Create("imap://localhost/INBOX") as ImapWebRequest;

        request.Method = "NOOP";
        request.KeepAlive = false;
        request.AllowInsecureLogin = true;

        request.BeginGetResponse(null, null);

        request.BeginGetRequestStream(null, null);
      }
    }

    [Test]
    public void TestGetRequestStreamGetSessionFailed()
    {
      string hostPort;

      using (var server = new ImapPseudoServer()) {
        server.Start();

        hostPort = server.HostPort;

        server.Stop();
      }

      var request = WebRequest.Create(string.Format("imap://{0}/INBOX", hostPort));

      request.Method = "APPEND";
      request.Timeout = 1000;

      try {
        request.GetRequestStream();
      }
      catch (WebException ex) {
        if (ex.Status != WebExceptionStatus.ConnectFailure)
          throw ex;
      }
    }

    [Test]
    public void TestGetRequestStreamContentLengthSpecified()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.ContentLength = 80L;
        request.Timeout = 1000;
        request.ReadWriteTimeout = 1000;

        var stream = GetRequestStream(server, request);

        Assert.IsNotNull(stream);
        Assert.AreEqual(80L, stream.Length);
      }
    }

    [Test]
    public void TestGetRequestStreamContentLengthNotSpecified()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = 1000;
        request.ReadWriteTimeout = 1000;

        var stream = GetRequestStream(server, request);

        Assert.IsNotNull(stream);
        //Assert.AreEqual(0L, stream.Length); // this will be locked
      }
    }

    private Stream GetRequestStream(ImapPseudoServer server, ImapWebRequest request)
    {
      // greeting
      server.EnqueueResponse("* OK ready\r\n");
      // CAPABILITY
      server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                             "0000 OK done\r\n");
      // LOGIN
      server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                             "0001 OK done\r\n");
      // APPEND
      server.EnqueueResponse("+ continue\r\n");

      return request.GetRequestStream();
    }

    [Test]
    public void TestGetRequestStreamCloseContentLengthSpecified()
    {
      GetRequestStreamClose(true);
    }

    [Test]
    public void TestGetRequestStreamCloseContentLengthNotSpecified()
    {
      GetRequestStreamClose(false);
    }

    private void GetRequestStreamClose(bool setContentLength)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse(string.Empty);

        var message = "test message";
        var data = Encoding.ASCII.GetBytes(message);
        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;
        request.AllowInsecureLogin = true;

        if (setContentLength)
          request.ContentLength = data.Length;

        using (var stream = request.GetRequestStream()) {
          stream.Write(data, 0, data.Length);
        }

        server.EnqueueResponse("0002 OK done\r\n");

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response.GetResponseStream());

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        var appendCommand = server.DequeueAll();

        StringAssert.StartsWith("0002 APPEND \"INBOX\" ", appendCommand);
        StringAssert.EndsWith(string.Format(" {{{0}}}\r\n{1}\r\n", Encoding.ASCII.GetByteCount(message), message), appendCommand);
      }
    }

    [Test]
    public void TestGetRequestStreamWriteOverContentLength()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse(string.Empty);

        var message = "test message";
        var data = Encoding.ASCII.GetBytes(message);
        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;
        request.ContentLength = data.Length;
        request.AllowInsecureLogin = true;

        using (var stream = request.GetRequestStream()) {
          stream.Write(data, 0, data.Length);

          // extra data
          stream.WriteByte(0x40);
          stream.WriteByte(0x40);
          stream.WriteByte(0x40);
          stream.WriteByte(0x40);
        }

        server.EnqueueResponse("0002 OK done\r\n");

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response.GetResponseStream());

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        var appendCommand = server.DequeueAll();

        StringAssert.StartsWith("0002 APPEND \"INBOX\" ", appendCommand);
        StringAssert.EndsWith(string.Format(" {{{0}}}\r\n{1}\r\n", Encoding.ASCII.GetByteCount(message), message), appendCommand);
      }
    }

    [Test]
    public void TestGetRequestStreamBufferUnderrun()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse(string.Empty);
        server.EnqueueResponse("0002 OK done\r\n");

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = -1;
        request.ReadWriteTimeout = 1000;
        request.ContentLength = 1;

        request.GetRequestStream();

        // stream.ReadTimeout = 1000; // not works

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.Timeout, ex.Status);
        }
      }
    }

    [Test]
    public void TestGetRequestStreamWriteShortFragments()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse(string.Empty);

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = -1;
        request.ReadWriteTimeout = 1000;
        request.AllowInsecureLogin = true;

        using (var stream = request.GetRequestStream()) {
          for (var i = 0; i < 0x100; i++) {
            stream.WriteByte((byte)i);
            Thread.Sleep(1);
          }

          server.EnqueueResponse("0002 OK done\r\n");
        }

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response.GetResponseStream());

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        }
      }
    }

    [Test]
    public void TestGetAppendResponse()
    {
      GetAppendResponse(false);
    }

    [Test]
    public void TestGetAppendResponseAppendUidResponseCodeExists()
    {
      GetAppendResponse(true);
    }

    private void GetAppendResponse(bool appendUid)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("+ continue\r\n");
        server.EnqueueResponse(string.Empty);

        var message = "test message";
        var data = Encoding.ASCII.GetBytes(message);
        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;
        request.ContentLength = data.Length;
        request.AllowInsecureLogin = true;

        using (var stream = request.GetRequestStream()) {
          stream.Write(data, 0, data.Length);
        }

        if (appendUid)
          server.EnqueueResponse("0002 OK [APPENDUID 38505 3955] done\r\n");
        else
          server.EnqueueResponse("0002 OK done\r\n");

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response.GetResponseStream());

          if (appendUid)
            Assert.AreEqual(new Uri(string.Format("imap://{0}/INBOX;UIDVALIDITY=38505/;UID=3955", server.HostPort)), response.ResponseUri);
          else
            Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        var appendCommand = server.DequeueAll();

        StringAssert.StartsWith("0002 APPEND \"INBOX\" ", appendCommand);
        StringAssert.EndsWith(string.Format(" {{{0}}}\r\n{1}\r\n", Encoding.ASCII.GetByteCount(message), message), appendCommand);
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetAppendResponseGetRequestStreamNotCalled()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.AllowInsecureLogin = true;

        request.GetResponse();
      }
    }

    [Test]
    public void TestGetAppendResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        // greeting
        server.EnqueueResponse("* OK ready\r\n");
        // CAPABILITY
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0000 OK done\r\n");
        // LOGIN
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK done\r\n");
        // APPEND
        server.EnqueueResponse("0002 NO failed\r\n");

        var message = "test message";
        var data = Encoding.ASCII.GetBytes(message);
        var request = WebRequest.Create(string.Format("imap://{0}/INBOX", server.HostPort)) as ImapWebRequest;

        request.Method = "APPEND";
        request.Timeout = 3000;
        request.ReadWriteTimeout = 3000;
        request.AllowInsecureLogin = true;

        using (var stream = request.GetRequestStream()) {
          stream.Write(data, 0, data.Length);
        }

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);

          var resp = ex.Response as ImapWebResponse;

          Assert.IsNotNull(resp);

          Assert.AreEqual(request.RequestUri, resp.ResponseUri);
          Assert.IsNotNull(resp.Result);
          Assert.IsTrue(resp.Result.Failed);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        var appendCommand = server.DequeueAll();

        StringAssert.StartsWith("0002 APPEND \"INBOX\" ", appendCommand);
      }
    }

    [Test]
    public void TestMethodNoOp()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = "0002 OK done\r\n";

        Request(server, ImapWebRequestMethods.NoOp, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.Contains("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponse()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 OK done\r\n", // SELECT

          "* 1 FETCH (UID 1 FLAGS (\\Seen) INTERNALDATE \"17-Jan-2010 05:45:17 +0900\" RFC822.SIZE 100 ENVELOPE (NIL \"test1\" NIL NIL NIL NIL NIL NIL NIL NIL))\r\n" +
          "* 2 FETCH (UID 2 FLAGS (\\Seen) INTERNALDATE \"17-Jan-2010 05:45:17 +0900\" RFC822.SIZE 300 ENVELOPE (NIL \"test2\" NIL NIL NIL NIL NIL NIL NIL NIL))\r\n" +
          "* 3 FETCH (UID 4 FLAGS (\\Seen) INTERNALDATE \"17-Jan-2010 05:45:17 +0900\" RFC822.SIZE 200 ENVELOPE (NIL \"test3\" NIL NIL NIL NIL NIL NIL NIL NIL))\r\n" +
          "0003 OK done\r\n",
        };

        Request(server, ImapWebRequestMethods.Fetch, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(3, response.MessageAttributes.Length);

          Assert.AreEqual(new Uri(request.RequestUri.AbsoluteUri + "/;UID=1"), response.MessageAttributes[0].Url);
          Assert.AreEqual(new Uri(request.RequestUri.AbsoluteUri + "/;UID=2"), response.MessageAttributes[1].Url);
          Assert.AreEqual(new Uri(request.RequestUri.AbsoluteUri + "/;UID=4"), response.MessageAttributes[2].Url);
        });

        StringAssert.Contains("SELECT", server.DequeueRequest());
        StringAssert.Contains("UID FETCH 1:* ALL", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 OK done\r\n",
          "0003 NO failed\r\n",
        };

        Request(server, ImapWebRequestMethods.Fetch, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(0, response.MessageAttributes.Length);
        });

        StringAssert.Contains("SELECT", server.DequeueRequest());
        StringAssert.Contains("UID FETCH 1:* ALL", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponseSelectFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 NO failed\r\n",
        };

        Request(server, ImapWebRequestMethods.Fetch, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(0, response.MessageAttributes.Length);
        });

        StringAssert.Contains("SELECT", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetCreateResponse()
    {
      GetCreateResponse(false);
    }

    [Test]
    public void TestGetCreateResponseMailboxSelected()
    {
      GetCreateResponse(true);
    }

    private void GetCreateResponse(bool mailboxSelected)
    {
      using (var server = new ImapPseudoServer()) {
        if (mailboxSelected)
          SelectMailbox(server);

        var methodResponses = mailboxSelected
          ? new[] {
              "0004 OK done\r\n", // CLOSE
              "0005 OK done\r\n", // CREATE
              "0006 OK done\r\n", // SUBSCIBE
              "* LSUB () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
              "0007 OK done\r\n", // LSUB
            }
          : new[] {
              "0002 OK done\r\n", // CREATE
              "0003 OK done\r\n", // SUBSCIBE
              "* LSUB () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
              "0004 OK done\r\n", // LSUB
            };

        Request(server, ImapWebRequestMethods.Create, "新しいメールボックス", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          var responseUri = new Uri(new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");

          Assert.AreEqual(responseUri, response.ResponseUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(responseUri, response.Mailboxes[0].Url);
          Assert.AreEqual(".", response.Mailboxes[0].HierarchyDelimiter);
          Assert.AreEqual("新しいメールボックス", response.Mailboxes[0].Name);
        });

        if (mailboxSelected)
          server.DequeueRequest(); // CLOSE

        StringAssert.Contains("CREATE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("SUBSCRIBE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("LSUB \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetCreateResponseNoSubscription()
    {
      using (var server = new ImapPseudoServer()) {
        if (mailboxSelected)
          SelectMailbox(server);

        var methodResponses = new[] {
          "0002 OK done\r\n", // CREATE
          "* LIST () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
          "0003 OK done\r\n", // LIST
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.Subscription = false;
        };

        Request(server, ImapWebRequestMethods.Create, "新しいメールボックス", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          var responseUri = new Uri(new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");

          Assert.AreEqual(responseUri, response.ResponseUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(responseUri, response.Mailboxes[0].Url);
          Assert.AreEqual(".", response.Mailboxes[0].HierarchyDelimiter);
          Assert.AreEqual("新しいメールボックス", response.Mailboxes[0].Name);
        });

        StringAssert.Contains("CREATE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("LIST \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetRenameResponse()
    {
      GetRenameResponse(false);
    }

    [Test]
    public void TestGetRenameResponseMailboxSelected()
    {
      GetRenameResponse(true);
    }

    private void GetRenameResponse(bool mailboxSelected)
    {
      using (var server = new ImapPseudoServer()) {
        if (mailboxSelected)
          SelectMailbox(server);

        var methodResponses = mailboxSelected
          ? new[] {
              "0004 OK done\r\n", // CLOSE
              "0005 OK done\r\n", // RENAME
              "0006 OK done\r\n", // UNSUBSCRIBE
              "0007 OK done\r\n", // SUBSCRIBE
              "* LSUB () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
              "0008 OK done\r\n", // LSUB
              "0009 OK done\r\n", // LSUB children
              "000a OK done\r\n", // LIST children
            }
          : new[] {
              "0002 OK done\r\n", // RENAME
              "0003 OK done\r\n", // UNSUBSCRIBE
              "0004 OK done\r\n", // SUBSCRIBE
              "* LSUB () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
              "0005 OK done\r\n", // LSUB
              "0006 OK done\r\n", // LSUB children
              "0007 OK done\r\n", // LIST children
            };

        Uri destinationUri = null;
        Uri responseUri = null;

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          destinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");
          responseUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");

          req.DestinationUri = destinationUri;
        };

        Request(server, ImapWebRequestMethods.Rename, "古いメールボックス", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(responseUri, response.ResponseUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(responseUri, response.Mailboxes[0].Url);
          Assert.AreEqual(".", response.Mailboxes[0].HierarchyDelimiter);
          Assert.AreEqual("新しいメールボックス", response.Mailboxes[0].Name);
        });

        if (mailboxSelected)
          server.DequeueRequest(); // CLOSE

        StringAssert.Contains("RENAME \"&U+QwRDDhMPww6zDcMMMwrzC5-\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("UNSUBSCRIBE \"&U+QwRDDhMPww6zDcMMMwrzC5-\"", server.DequeueRequest());
        StringAssert.Contains("SUBSCRIBE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("LSUB \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("LSUB \"\" \"&U+QwRDDhMPww6zDcMMMwrzC5-.*\"", server.DequeueRequest());
        StringAssert.Contains("LIST \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-.*\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetRenameResponseNoSubscription()
    {
      using (var server = new ImapPseudoServer()) {
        if (mailboxSelected)
          SelectMailbox(server);

        var methodResponses = new[] {
          "0002 OK done\r\n", // RENAME
          "* LIST () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
          "0003 OK done\r\n", // LIST
        };

        Uri destinationUri = null;
        Uri responseUri = null;

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          destinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");
          responseUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");

          req.DestinationUri = destinationUri;
          req.Subscription = false;
        };

        Request(server, ImapWebRequestMethods.Rename, "古いメールボックス", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(responseUri, response.ResponseUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(responseUri, response.Mailboxes[0].Url);
          Assert.AreEqual(".", response.Mailboxes[0].HierarchyDelimiter);
          Assert.AreEqual("新しいメールボックス", response.Mailboxes[0].Name);
        });

        StringAssert.Contains("RENAME \"&U+QwRDDhMPww6zDcMMMwrzC5-\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("LIST \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetRenameResponseHierarchySubscription()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 OK done\r\n", // RENAME
          "0003 OK done\r\n", // UNSUBSCRIBE
          "0004 OK done\r\n", // SUBSCRIBE
          "* LSUB () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"\r\n" +
          "0005 OK done\r\n", // LSUB

          // LSUB children
          "* LSUB () \".\" \"&U+QwRDDhMPww6zDcMMMwrzC5-.child1\"\r\n" +
          "* LSUB () \".\" \"&U+QwRDDhMPww6zDcMMMwrzC5-.child2\"\r\n" +
          "0006 OK done\r\n",
          // UNSUBSCRIBE children
          "0007 OK done\r\n",
          "0008 OK done\r\n",

          // LIST children
          "* LIST () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-.child1\"\r\n" +
          "* LIST () \".\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-.child2\"\r\n" +
          "0009 OK done\r\n", 
          // SUBSCRIBE children
          "000a OK done\r\n",
          "000b OK done\r\n",
        };

        Uri destinationUri = null;
        Uri responseUri = null;

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          destinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");
          responseUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");

          req.DestinationUri = destinationUri;
        };

        Request(server, ImapWebRequestMethods.Rename, "古いメールボックス", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(responseUri, response.ResponseUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(responseUri, response.Mailboxes[0].Url);
          Assert.AreEqual(".", response.Mailboxes[0].HierarchyDelimiter);
          Assert.AreEqual("新しいメールボックス", response.Mailboxes[0].Name);
        });

        if (mailboxSelected)
          server.DequeueRequest(); // CLOSE

        StringAssert.Contains("RENAME \"&U+QwRDDhMPww6zDcMMMwrzC5-\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("UNSUBSCRIBE \"&U+QwRDDhMPww6zDcMMMwrzC5-\"", server.DequeueRequest());
        StringAssert.Contains("SUBSCRIBE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("LSUB \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());

        // LSUB children
        StringAssert.Contains("LSUB \"\" \"&U+QwRDDhMPww6zDcMMMwrzC5-.*\"", server.DequeueRequest());
        // UNSUBSCRIBE children
        StringAssert.Contains("UNSUBSCRIBE \"&U+QwRDDhMPww6zDcMMMwrzC5-.child1\"", server.DequeueRequest());
        StringAssert.Contains("UNSUBSCRIBE \"&U+QwRDDhMPww6zDcMMMwrzC5-.child2\"", server.DequeueRequest());

        // LIST children
        StringAssert.Contains("LIST \"\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-.*\"", server.DequeueRequest());
        // SUBSCRIBE children
        StringAssert.Contains("SUBSCRIBE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-.child1\"", server.DequeueRequest());
        StringAssert.Contains("SUBSCRIBE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-.child2\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetDeleteResponse()
    {
      GetDeleteResponse(false);
    }

    [Test]
    public void TestGetDeleteResponseMailboxSelected()
    {
      GetDeleteResponse(true);
    }

    private void GetDeleteResponse(bool mailboxSelected)
    {
      using (var server = new ImapPseudoServer()) {
        if (mailboxSelected)
          SelectMailbox(server);

        var methodResponses = mailboxSelected
          ? new[] {
              "0004 OK done\r\n", // CLOSE
              "0005 OK done\r\n", // DELETE
              "0006 OK done\r\n", // UNSUBSCRIBE
            }
          : new[] {
              "0002 OK done\r\n", // DELETE
              "0003 OK done\r\n", // UNSUBSCRIBE
            };

        Request(server, ImapWebRequestMethods.Delete, "新しいメールボックス", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);
        });

        if (mailboxSelected)
          server.DequeueRequest(); // CLOSE

        StringAssert.Contains("DELETE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
        StringAssert.Contains("UNSUBSCRIBE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetDeleteResponseNoSubscription()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 OK done\r\n", // DELETE
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.Subscription = false;
        };

        Request(server, ImapWebRequestMethods.Delete, "新しいメールボックス", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);
        });

        StringAssert.Contains("DELETE \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetCreateResponseFailure()
    {
      GetCreateRenameDeleteResponseFailure(ImapWebRequestMethods.Create);
    }

    [Test]
    public void TestGetRenameResponseFailure()
    {
      GetCreateRenameDeleteResponseFailure(ImapWebRequestMethods.Rename);
    }

    [Test]
    public void TestGetDeleteResponseFailure()
    {
      GetCreateRenameDeleteResponseFailure(ImapWebRequestMethods.Delete);
    }

    private void GetCreateRenameDeleteResponseFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 NO failed\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          if (method == "RENAME")
            req.DestinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "新しいメールボックス");
        };

        Request(server, method, "古いメールボックス", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        switch (method) {
          case "DELETE": StringAssert.Contains("DELETE \"&U+QwRDDhMPww6zDcMMMwrzC5-\"", server.DequeueRequest()); break;
          case "RENAME": StringAssert.Contains("RENAME \"&U+QwRDDhMPww6zDcMMMwrzC5-\" \"&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-\"", server.DequeueRequest()); break;
          case "CREATE": StringAssert.Contains("CREATE \"&U+QwRDDhMPww6zDcMMMwrzC5-\"", server.DequeueRequest()); break;
        }
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetRenameResponseDestinationUriNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        Request(server, ImapWebRequestMethods.Rename, "renaming-mailbox", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetRenameResponseInvalidDestinationUriForm1()
    {
      using (var server = new ImapPseudoServer()) {
        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.DestinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "renamed-mailbox/;UID=1");
        };

        Request(server, ImapWebRequestMethods.Rename, "renaming-mailbox", new string[0], presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetRenameResponseInvalidDestinationUriForm2()
    {
      using (var server = new ImapPseudoServer()) {
        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority));
        };

        Request(server, ImapWebRequestMethods.Rename, "renaming-mailbox", new string[0], presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test]
    public void TestGetSelectResponse()
    {
      GetSelectExamineResponse(ImapWebRequestMethods.Select);
    }

    [Test]
    public void TestGetExamineResponse()
    {
      GetSelectExamineResponse(ImapWebRequestMethods.Examine);
    }

    private void GetSelectExamineResponse(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = (method == "SELECT")
          ? "0002 OK [READ-WRITE] done\r\n"
          : "0002 OK [READ-ONLY] done\r\n";

        Request(server, method, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(request.RequestUri, response.Mailboxes[0].Url);

          if (method == "SELECT")
            Assert.IsFalse(response.Mailboxes[0].ReadOnly);
          else
            Assert.IsTrue(response.Mailboxes[0].ReadOnly);
        });

        StringAssert.Contains(method, server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSelectResponseReselectMailbox()
    {
      GetSelectExamineResponseReselectMailbox(ImapWebRequestMethods.Select);
    }

    [Test]
    public void TestGetExamineResponseReselectMailbox()
    {
      GetSelectExamineResponseReselectMailbox(ImapWebRequestMethods.Examine);
    }

    private void GetSelectExamineResponseReselectMailbox(string method)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var firstRequest = WebRequest.Create(string.Format("imap://user@{0}/INBOX", server.HostPort)) as ImapWebRequest;

        firstRequest.Credentials = new NetworkCredential("user", "pass");
        firstRequest.KeepAlive = true;
        firstRequest.Timeout = 1000;
        firstRequest.Method = method;
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
        if (method == "SELECT")
          server.EnqueueResponse("0002 OK [READ-WRITE] done\r\n");
        else
          server.EnqueueResponse("0002 OK [READ-ONLY] done\r\n");

        using (var response = firstRequest.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, firstRequest.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(firstRequest.RequestUri, response.Mailboxes[0].Url);

          if (method == "SELECT")
            Assert.IsFalse(response.Mailboxes[0].ReadOnly);
          else
            Assert.IsTrue(response.Mailboxes[0].ReadOnly);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains(method, server.DequeueRequest());

        var secondRequest = WebRequest.Create(firstRequest.RequestUri) as ImapWebRequest;

        secondRequest.Credentials = new NetworkCredential("user", "pass");
        secondRequest.KeepAlive = false;
        secondRequest.Timeout = 1000;
        secondRequest.Method = method;

        // NOOP
        server.EnqueueResponse("0003 OK done\r\n");
        // CLOSE
        server.EnqueueResponse("0004 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" + 
                               "0005 OK done\r\n");

        try {
          using (var response = secondRequest.GetResponse()) {
            Assert.Fail("WebException not thrown");
          }
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.Success, ex.Status);
          Assert.IsNull(ex.Response);
        }

        StringAssert.Contains("NOOP", server.DequeueRequest());
        StringAssert.Contains("CLOSE", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSelectResponseFailure()
    {
      GetSelectExamineResponseFailure(ImapWebRequestMethods.Select);
    }

    [Test]
    public void TestGetExamineResponseFailure()
    {
      GetSelectExamineResponseFailure(ImapWebRequestMethods.Examine);
    }

    private void GetSelectExamineResponseFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = "0002 NO failed\r\n";

        Request(server, method, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains(method, server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetStatusResponse()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "* STATUS INBOX (MESSAGES 231 UIDNEXT 44292)\r\n" +
          "0002 OK done\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.StatusDataItem = ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext;
        };

        Request(server, ImapWebRequestMethods.Status, "INBOX", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(response.ResponseUri, response.Mailboxes[0].Url);
          Assert.AreEqual("INBOX", response.Mailboxes[0].Name);
          Assert.AreEqual(231L, response.Mailboxes[0].ExistsMessage);
          Assert.AreEqual(44292L, response.Mailboxes[0].UidNext);
        });

        StringAssert.Contains("STATUS \"INBOX\" (MESSAGES UIDNEXT)", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetStatusResponseSelectedMailbox()
    {
      using (var server = new ImapPseudoServer()) {
        SelectMailbox(server);

        var methodResponses = new string[0];

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.StatusDataItem = ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext;
        };

        try {
          Request(server, ImapWebRequestMethods.Status, "INBOX", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
            Assert.Fail("ProtocolViolationException not thrown");
          });
        }
        catch (ProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestGetStatusResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 NO failed\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.StatusDataItem = ImapStatusDataItem.UidNext;
        };

        Request(server, ImapWebRequestMethods.Status, "INBOX", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains("STATUS \"INBOX\" (UIDNEXT)", server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetStatusResponseStatusDataItemNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        Request(server, ImapWebRequestMethods.Status, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test]
    public void TestGetSubscribeResponse()
    {
      GetSubscribeUnsubscribeResponse(ImapWebRequestMethods.Subscribe, false);
    }

    [Test]
    public void TestGetSubscribeResponseMailboxSelected()
    {
      GetSubscribeUnsubscribeResponse(ImapWebRequestMethods.Subscribe, true);
    }

    [Test]
    public void TestGetUnsubscribeResponse()
    {
      GetSubscribeUnsubscribeResponse(ImapWebRequestMethods.Unsubscribe, false);
    }

    [Test]
    public void TestGetUnsubscribeResponseMailboxSelected()
    {
      GetSubscribeUnsubscribeResponse(ImapWebRequestMethods.Unsubscribe, true);
    }

    private void GetSubscribeUnsubscribeResponse(string method, bool mailboxSelected)
    {
      using (var server = new ImapPseudoServer()) {
        if (mailboxSelected)
          SelectMailbox(server);

        var methodResponses = mailboxSelected
          ? new[] {
              "0004 OK done\r\n", // CLOSE
              "0005 OK done\r\n",
            }
          : new[] {
              "0002 OK done\r\n"
            };

        Request(server, method, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        if (mailboxSelected)
          server.DequeueRequest(); // CLOSE

        StringAssert.Contains(method, server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSubscribeResponseFailure()
    {
      GetSubscribeUnsubscribeResponseFailure(ImapWebRequestMethods.Subscribe);
    }

    [Test]
    public void TestGetUnsubscribeResponseFailure()
    {
      GetSubscribeUnsubscribeResponseFailure(ImapWebRequestMethods.Unsubscribe);
    }

    private void GetSubscribeUnsubscribeResponseFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = "0002 NO failed\r\n";

        Request(server, method, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains(method, server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponse()
    {
      GetExpungeCheckResponse(ImapWebRequestMethods.Expunge);
    }

    [Test]
    public void TestGetCheckResponse()
    {
      GetExpungeCheckResponse(ImapWebRequestMethods.Check);
    }

    private void GetExpungeCheckResponse(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 OK done\r\n",
          "0003 OK done\r\n",
        };

        Request(server, method, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.Contains("SELECT", server.DequeueRequest());
        StringAssert.Contains(method, server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponseFailure()
    {
      GetExpungeCheckResponseFailure(ImapWebRequestMethods.Expunge);
    }

    [Test]
    public void TestGetCheckResponseFailure()
    {
      GetExpungeCheckResponseFailure(ImapWebRequestMethods.Check);
    }

    private void GetExpungeCheckResponseFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 OK done\r\n",
          "0003 NO failed\r\n",
        };

        Request(server, method, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains("SELECT", server.DequeueRequest());
        StringAssert.Contains(method, server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponseSelectFailure()
    {
      GetExpungeCheckResponseSelectFailure(ImapWebRequestMethods.Expunge);
    }

    [Test]
    public void TestGetCheckResponseSelectFailure()
    {
      GetExpungeCheckResponseSelectFailure(ImapWebRequestMethods.Check);
    }

    private void GetExpungeCheckResponseSelectFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0002 NO failed\r\n",
        };

        Request(server, method, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains("SELECT", server.DequeueRequest());
      }
    }
  }
}
