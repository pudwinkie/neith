using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapFetchMessageWebRequestTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private void Request(ImapPseudoServer server, string method, string methodResponse, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, new[] {"IMAP4REV1"}, new[] {methodResponse}, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, new[] {"IMAP4REV1"}, methodResponses, null, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, capabilities, methodResponses, null, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] methodResponses, Action<ImapWebRequest> presetRequest, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, new[] {"IMAP4REV1"}, methodResponses, presetRequest, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, string[] methodResponses, Action<ImapWebRequest> presetRequest, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      server.Start();

      var request = WebRequest.Create(string.Format("imap://user@{0}/INBOX/;UID=1", server.HostPort)) as ImapWebRequest;

      Assert.AreEqual("ImapFetchMessageWebRequest", request.GetType().Name);

      request.Credentials = new NetworkCredential("user", "pass");
      request.KeepAlive = false;
      request.Timeout = 1000;
      request.Method = method;
      request.AllowInsecureLogin = true;

      if (presetRequest != null)
        presetRequest(request);

      // greeting
      server.EnqueueResponse("* OK ready\r\n");
      // CAPABILITY
      server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                             "0000 OK done\r\n");
      // LOGIN
      server.EnqueueResponse(string.Format("* CAPABILITY {0}\r\n", string.Join(" ", capabilities)) +
                             "0001 OK done\r\n");

      int commandTag = 3;

      if (method == "NOOP") {
        commandTag = 2;
      }
      else {
        // SELECT
        server.EnqueueResponse("0002 OK done\r\n");
      }

      foreach (var methodResponse in methodResponses) {
        server.EnqueueResponse(methodResponse);

        commandTag++;
      }

      // LOGOUT
      server.EnqueueResponse("* BYE logging out\r\n" + 
                             string.Format("{0:x4} OK done\r\n", commandTag));

      try {
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
      }
      finally {
        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        if (method != "NOOP")
          server.DequeueRequest(); // SELECT
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
    public void TestGetFetchResponseSelectFailure()
    {
      GetFetchExpungeStoreResponseSelectFailure(ImapWebRequestMethods.Fetch);
    }

    [Test]
    public void TestGetExpungeResponseSelectFailure()
    {
      GetFetchExpungeStoreResponseSelectFailure(ImapWebRequestMethods.Expunge);
    }

    [Test]
    public void TestGetStoreResponseSelectFailure()
    {
      GetFetchExpungeStoreResponseSelectFailure(ImapWebRequestMethods.Store);
    }

    private void GetFetchExpungeStoreResponseSelectFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)) as ImapWebRequest;

        request.KeepAlive = false;
        request.Timeout = 1000;
        request.Method = method;
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
        server.EnqueueResponse("0002 NO failed\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" + 
                               "0003 OK done\r\n");

        try {
          using (var response = request.GetResponse()) {
          }
        }
        catch (WebException ex) {
          if (ex.Status != WebExceptionStatus.ProtocolError)
            throw ex;

          var resp = ex.Response as ImapWebResponse;

          Assert.IsNotNull(resp);
          Assert.IsTrue(resp.Result.Failed);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponse()
    {
      GetFetchResponse(false);
    }

    [Test]
    public void TestGetFetchResponsePeekFetch()
    {
      GetFetchResponse(true);
    }

    private void GetFetchResponse(bool peek)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "* FETCH 1 (RFC822.SIZE 1024 BODYSTRUCTURE (\"text\" \"plain\" (\"charset\" \"us-ascii\") NIL NIL \"7bit\" 1024 5 NIL NIL NIL NIL) BODY[]<0> {4}\r\n" +
          "body)\r\n" +
          "0003 OK done\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.FetchPeek = peek;
          req.FetchBlockSize = 4;
          req.FetchDataItem = ImapFetchDataItemMacro.Full;
        };

        Request(server, ImapWebRequestMethods.Fetch, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.AreEqual(1024L, response.ContentLength);
          Assert.AreEqual("text/plain", response.ContentType);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(1, response.MessageAttributes.Length);
          Assert.AreEqual(1L, response.MessageAttributes[0].Sequence);
          Assert.IsNotNull(response.MessageAttributes[0].BodyStructure);
          Assert.AreEqual(1024, response.MessageAttributes[0].Rfc822Size);

          var stream = response.GetResponseStream();

          Assert.IsNotNull(stream);
          Assert.AreEqual(1024L, stream.Length);
        });

        if (peek)
          StringAssert.Contains("UID FETCH 1 (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY BODY.PEEK[]<0.4>)", server.DequeueRequest());
        else
          StringAssert.Contains("UID FETCH 1 (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY BODY[]<0.4>)", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponseCloseResponseStreamKeepAliveTrue()
    {
      using (var server = new ImapPseudoServer()) {
        using (var response = GetFetchResponseCloseResponseStreamWithKeepAlive(server, true)) {
          var stream = response.GetResponseStream();

          Assert.IsNotNull(stream);

          stream.Close();
        }

        // new request
        var request = WebRequest.Create(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)) as ImapWebRequest;

        request.KeepAlive = true;
        request.Timeout = 1000;
        request.Method = "NOOP";

        // NOOP
        server.EnqueueResponse("0004 OK done\r\n");

        using (var response = request.GetResponse()) {
        }

        StringAssert.Contains("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponseCloseResponseStreamKeepAliveFalse()
    {
      using (var server = new ImapPseudoServer()) {
        using (var response = GetFetchResponseCloseResponseStreamWithKeepAlive(server, false)) {
          var stream = response.GetResponseStream();

          Assert.IsNotNull(stream);

          // CLOSE
          server.EnqueueResponse("0004 OK done\r\n");
          // LOGOUT
          server.EnqueueResponse("* BYE logging out\r\n" + 
                                 "0005 OK done\r\n");

          stream.Close();

          StringAssert.Contains("CLOSE", server.DequeueRequest());
          StringAssert.Contains("LOGOUT", server.DequeueRequest());
        }
      }
    }

    private ImapWebResponse GetFetchResponseCloseResponseStreamWithKeepAlive(ImapPseudoServer server, bool keepAlive)
    {
      server.Start();

      var request = WebRequest.Create(string.Format("imap://{0}/INBOX/;UID=1", server.HostPort)) as ImapWebRequest;

      request.KeepAlive = keepAlive;
      request.Timeout = 1000;
      request.Method = "FETCH";
      request.FetchBlockSize = 4;
      request.FetchDataItem = ImapFetchDataItemMacro.Fast;
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
      server.EnqueueResponse("0002 OK done\r\n");
      // FETCH
      server.EnqueueResponse("* FETCH 1 (RFC822.SIZE 1024 BODYSTRUCTURE (\"text\" \"plain\" (\"charset\" \"us-ascii\") NIL NIL \"7bit\" 1024 5 NIL NIL NIL NIL) BODY[]<0> {4}\r\n" +
                             "body)\r\n" +
                             "0003 OK done\r\n");

      var response = request.GetResponse() as ImapWebResponse;

      Assert.IsNotNull(response.Result);
      Assert.IsTrue(response.Result.Succeeded);

      server.DequeueRequest(); // CAPABILITY
      server.DequeueRequest(); // LOGIN
      server.DequeueRequest(); // SELECT
      StringAssert.Contains("UID FETCH 1 (FLAGS INTERNALDATE RFC822.SIZE BODY.PEEK[]<0.4>)", server.DequeueRequest());

      return response;
    }

    [Test]
    public void TestGetFetchResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0003 NO failed\r\n",
        };

        Request(server, ImapWebRequestMethods.Fetch, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          try {
            response.GetResponseStream();
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }
        });

        StringAssert.Contains("UID FETCH 1 (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY.PEEK[]<0.10240>)", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponseNoSuchMessage()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0003 OK done\r\n",
        };

        try {
          Request(server, ImapWebRequestMethods.Fetch, methodResponses, null);
        }
        catch (WebException ex) {
          Assert.AreEqual(WebExceptionStatus.Success, ex.Status);

          var resp = ex.Response as ImapWebResponse;

          Assert.IsNotNull(resp);

          Assert.IsNotNull(resp.Result);
          Assert.IsTrue(resp.Result.Succeeded);

          try {
            resp.GetResponseStream();
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }
        }

        StringAssert.Contains("UID FETCH 1 (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE BODY.PEEK[]<0.10240>)", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetFetchResponsePartialFetchSectionSpecified()
    {
      GetFetchResponsePartialFetch("/;UID=1/;SECTION=HEADER", "BODY.PEEK[HEADER]<0.4>");
    }

    [Test]
    public void TestGetFetchResponsePartialFetchStartSpecified()
    {
      GetFetchResponsePartialFetch("/;UID=1/;PARTIAL=256", "BODY.PEEK[]<256.4>");
    }

    [Test]
    public void TestGetFetchResponsePartialFetchRangeSpecified()
    {
      GetFetchResponsePartialFetch("/;UID=1/;PARTIAL=1024.256", "BODY.PEEK[]<1024.4>");
    }

    private void GetFetchResponsePartialFetch(string requestUri, string expectedSpecifier)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX{1}", server.HostPort, requestUri)) as ImapWebRequest;

        request.KeepAlive = false;
        request.Timeout = 1000;
        request.Method = "FETCH";
        request.FetchBlockSize = 4;
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
        server.EnqueueResponse("0002 OK done\r\n");
        // FETCH
        server.EnqueueResponse("* FETCH 1 (BODY[]<0> {4}\r\n" +
                               "body)\r\n" +
                               "0003 OK done\r\n");

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(0, response.MessageAttributes.Length);

          var stream = response.GetResponseStream();

          Assert.IsNotNull(stream);
          Assert.AreEqual(0L, stream.Length);

          // CLOSE
          server.EnqueueResponse("0004 OK done\r\n");
          // LOGOUT
          server.EnqueueResponse("* BYE logging out\r\n" + 
                                 "0005 OK done\r\n");

          stream.Close();
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        server.DequeueRequest(); // SELECT
        StringAssert.Contains(string.Format("UID FETCH 1 ({0})", expectedSpecifier), server.DequeueRequest());
        StringAssert.Contains("CLOSE", server.DequeueRequest());
        StringAssert.Contains("LOGOUT", server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetCopyResponseDestinationUriNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        Request(server, ImapWebRequestMethods.Copy, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("InvalidOperationException not thrown");
        });
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetCopyResponseInvalidDestinationUriForm1()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.DestinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "copyto-mailbox/;UID=1");
        };

        Request(server, ImapWebRequestMethods.Copy, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("InvalidOperationException not thrown");
        });
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetCopyResponseInvalidDestinationUriForm2()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority));
        };

        Request(server, ImapWebRequestMethods.Copy, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("InvalidOperationException not thrown");
        });
      }
    }

    [Test]
    public void TestGetCopyResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID COPY
          "0003 NO [TRYCREATE] failed\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = false;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/../コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.EndsWith("UID COPY 1 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetCopyResponse()
    {
      GetCopyResponse(false);
    }

    [Test]
    public void TestGetCopyResponseCopyUidResponseCodeExists()
    {
      GetCopyResponse(true);
    }

    private void GetCopyResponse(bool copyuid)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID COPY
          copyuid
            ? "0003 OK [COPYUID 38505 1 3956] done\r\n"
            : "0003 OK done\r\n"
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = false;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/../コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          if (copyuid)
            Assert.AreEqual(new Uri(string.Format("{0}/INBOX/コピー先メールボックス;UIDVALIDITY=38505/;UID=3956", request.RequestUri.GetLeftPart(UriPartial.Authority))),
                            response.ResponseUri);
          else
            Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);

          if (copyuid) {
          }
        });

        StringAssert.EndsWith("UID COPY 1 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetCopyResponseAllowCreateMailbox()
    {
      GetCopyResponseAllowCreateMailbox(true);
    }

    [Test]
    public void TestGetCopyResponseAllowCreateMailboxNoSubscription()
    {
      GetCopyResponseAllowCreateMailbox(false);
    }

    private void GetCopyResponseAllowCreateMailbox(bool subscription)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = subscription
          ? new[] {
            // UID COPY
            "0003 NO [TRYCREATE] failed\r\n",
            // CREATE
            "0004 OK done\r\n",
            // UID COPY
            "0005 OK done\r\n",
            // SUBSCRIBE
            "0006 OK done\r\n",
            // LSUB
            "* LSUB () \".\" \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n" + 
            "0007 OK done\r\n"
          }
          : new[] {
            // UID COPY
            "0003 NO [TRYCREATE] failed\r\n",
            // CREATE
            "0004 OK done\r\n",
            // UID COPY
            "0005 OK done\r\n",
            // LIST
            "* LIST () \".\" \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n" + 
            "0006 OK done\r\n"
          };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = true;
          req.Subscription = subscription;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/../コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(new Uri(new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), "INBOX/コピー先メールボックス"),
                          response.Mailboxes[0].Url);
        });

        StringAssert.EndsWith("UID COPY 1 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID COPY 1 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());

        if (subscription) {
          StringAssert.EndsWith("SUBSCRIBE \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
          StringAssert.EndsWith("LSUB \"\" \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        }
        else {
          StringAssert.EndsWith("LIST \"\" \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        }
      }
    }

    [Test]
    public void TestGetCopyResponseCreateDestinationMailboxFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID COPY
          "0003 NO [TRYCREATE] failed\r\n",
          // CREATE
          "0004 NO failed\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = true;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/../コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.EndsWith("UID COPY 1 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponse()
    {
      GetExpungeResponse(false);
    }

    [Test]
    public void TestGetExpungeResponseUidplusCapable()
    {
      GetExpungeResponse(true);
    }

    private void GetExpungeResponse(bool uidplusCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0003 OK done\r\n",

          "* 1 EXPUNGE\r\n" +
          "0004 OK done\r\n",
        };
        var capabilities = uidplusCapable
          ? new[] {
              "IMAP4rev1",
              "UIDPLUS",
            }
          : new[] {
              "IMAP4rev1",
            };

        Request(server, ImapWebRequestMethods.Expunge, capabilities, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.Contains("UID STORE 1 +FLAGS.SILENT (\\Deleted)", server.DequeueRequest());

        if (uidplusCapable)
          StringAssert.Contains("UID EXPUNGE 1\r\n", server.DequeueRequest());
        else
          StringAssert.Contains("EXPUNGE\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponseStoreDeletedFlagFailure()
    {
      GetExpungeResponseStoreDeletedFlagFailure(false);
    }

    [Test]
    public void TestGetExpungeResponseStoreDeletedFlagFailureUidplusCapable()
    {
      GetExpungeResponseStoreDeletedFlagFailure(true);
    }

    private void GetExpungeResponseStoreDeletedFlagFailure(bool uidplusCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0003 NO failed\r\n",
        };
        var capabilities = uidplusCapable
          ? new[] {
              "IMAP4rev1",
              "UIDPLUS",
            }
          : new[] {
              "IMAP4rev1",
            };

        Request(server, ImapWebRequestMethods.Expunge, capabilities, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains("UID STORE 1 +FLAGS.SILENT (\\Deleted)", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponseFailure()
    {
      GetExpungeResponseFailure(false);
    }

    [Test]
    public void TestGetExpungeResponseFailureUidplusCapable()
    {
      GetExpungeResponseFailure(true);
    }

    private void GetExpungeResponseFailure(bool uidplusCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0003 OK done\r\n",
          "0004 NO failed\r\n",
        };
        var capabilities = uidplusCapable
          ? new[] {
              "IMAP4rev1",
              "UIDPLUS",
            }
          : new[] {
              "IMAP4rev1",
            };

        Request(server, ImapWebRequestMethods.Expunge, capabilities, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains("UID STORE 1 +FLAGS.SILENT (\\Deleted)", server.DequeueRequest());

        if (uidplusCapable)
          StringAssert.Contains("UID EXPUNGE 1\r\n", server.DequeueRequest());
        else
          StringAssert.Contains("EXPUNGE\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetStoreResponse()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "* FETCH 1 (FLAGS (\\Deleted $label1))\r\n" + 
          "0003 OK done\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.StoreDataItem = ImapStoreDataItem.AddFlags(new[] {"$label1"}, ImapMessageFlag.Deleted);
        };

        Request(server, ImapWebRequestMethods.Store, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.Contains("UID STORE 1 +FLAGS (\\Deleted $label1)", server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetStoreResponseStoreDataItemNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        Request(server, ImapWebRequestMethods.Store, methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test]
    public void TestGetStoreResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          "0003 NO failed\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.StoreDataItem = ImapStoreDataItem.AddFlags(new[] {"$label1"}, ImapMessageFlag.Deleted);
        };

        Request(server, ImapWebRequestMethods.Store, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.Contains("UID STORE 1 +FLAGS (\\Deleted $label1)", server.DequeueRequest());
      }
    }
  }
}
