using System;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapSearchMessageWebRequestTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private void Request(ImapPseudoServer server, string method, bool searchresCapable, string query, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server,
              method,
              searchresCapable
                ? new[] {"IMAP4REV1", "SEARCHRES"}
                : new[] {"IMAP4REV1"},
              query,
              methodResponses,
              null,
              responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, string query, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server,
              method,
              capabilities,
              query,
              methodResponses,
              null,
              responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, string query, string[] methodResponses, Action<ImapWebRequest> presetRequest, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server,
              method,
              capabilities,
              new Uri(string.Format("imap://user@{0}/INBOX{1}", server.HostPort, query)),
              methodResponses,
              presetRequest,
              responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, Uri requestUri, string[] methodResponses, Action<ImapWebRequest> presetRequest, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      server.Start();

      var request = WebRequest.Create(requestUri) as ImapWebRequest;

      Assert.AreEqual("ImapSearchMessageWebRequest", request.GetType().Name);

      request.Credentials = new NetworkCredential("user", "pass");
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
      server.EnqueueResponse(string.Format("* CAPABILITY {0}\r\n", string.Join(" ", capabilities)) +
                             "0001 OK done\r\n");
      // SELECT
      server.EnqueueResponse("0002 OK done\r\n");

      int commandTag = 3;

      foreach (var methodResponse in methodResponses) {
        server.EnqueueResponse(methodResponse);

        commandTag++;
      }

      // CLOSE
      server.EnqueueResponse(string.Format("{0:x4} OK done\r\n", commandTag++));
      // LOGOUT
      server.EnqueueResponse("* BYE logging out\r\n" + 
                             string.Format("{0:x4} OK done\r\n", commandTag++));

      if (presetRequest != null)
        presetRequest(request);

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

      server.DequeueRequest(); // CAPABILITY
      server.DequeueRequest(); // LOGIN
      server.DequeueRequest(); // SELECT
    }

    [Test]
    public void TestMethodNoOp()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://user@{0}/INBOX?UID 1", server.HostPort)) as ImapWebRequest;

        Assert.AreEqual("ImapSearchMessageWebRequest", request.GetType().Name);

        request.Credentials = new NetworkCredential("user", "pass");
        request.KeepAlive = false;
        request.Timeout = 1000;
        request.Method = "NOOP";
        request.AllowInsecureLogin = true;

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
        // CLOSE
        server.EnqueueResponse("0003 OK done\r\n");
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" + 
                               "0004 OK done\r\n");

        using (var response = request.GetResponse() as ImapWebResponse) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN
        StringAssert.Contains("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSearchResponseSelectFailure()
    {
      GetResponseSelectFailure(ImapWebRequestMethods.Search);
    }

    [Test]
    public void TestGetSortResponseSelectFailure()
    {
      GetResponseSelectFailure(ImapWebRequestMethods.Sort);
    }

    [Test]
    public void TestGetThreadResponseSelectFailure()
    {
      GetResponseSelectFailure(ImapWebRequestMethods.Thread);
    }

    [Test]
    public void TestGetCopyResponseSelectFailure()
    {
      GetResponseSelectFailure(ImapWebRequestMethods.Copy);
    }

    [Test]
    public void TestGetStoreResponseSelectFailure()
    {
      GetResponseSelectFailure(ImapWebRequestMethods.Store);
    }

    [Test]
    public void TestGetExpungeResponseSelectFailure()
    {
      GetResponseSelectFailure(ImapWebRequestMethods.Expunge);
    }

    private void GetResponseSelectFailure(string method)
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/INBOX?UID 1", server.HostPort)) as ImapWebRequest;

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

          Assert.AreEqual(request.RequestUri, resp.ResponseUri);

          Assert.IsNotNull(resp);
          Assert.IsTrue(resp.Result.Failed);
        }

        server.DequeueRequest(); // CAPABILITY
        server.DequeueRequest(); // LOGIN

        StringAssert.Contains("SELECT \"INBOX\"", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSearchResponseUidSearchFailure()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Search, false);
    }

    [Test]
    public void TestGetSearchResponseUidSearchFailureSearchresCapable()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Search, true);
    }

    [Test]
    public void TestGetCopyResponseUidSearchFailure()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Copy, false);
    }

    [Test]
    public void TestGetCopyResponseUidSearchFailureSearchresCapable()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Copy, true);
    }

    [Test]
    public void TestGetStoreResponseUidSearchFailure()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Store, false);
    }

    [Test]
    public void TestGetStoreResponseUidSearchFailureSearchresCapable()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Store, true);
    }

    [Test]
    public void TestGetExpungeResponseUidSearchFailure()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Expunge, false);
    }

    [Test]
    public void TestGetExpungeResponseUidSearchFailureSearchresCapable()
    {
      GetResponseUidSearchFailure(ImapWebRequestMethods.Expunge, true);
    }

    private void GetResponseUidSearchFailure(string method, bool searchresCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "0003 NO failed\r\n",
        };

        Request(server, method, searchresCapable, "?UID 1", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        if (searchresCapable)
          StringAssert.EndsWith("UID SEARCH RETURN (SAVE) UID 1\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("UID SEARCH UID 1\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSearchResponseUidSearchNothingMatched()
    {
      GetResponseUidSearchNothingMatched(ImapWebRequestMethods.Search);
    }

    [Test]
    public void TestGetCopyResponseUidSearchNothingMatched()
    {
      GetResponseUidSearchNothingMatched(ImapWebRequestMethods.Copy);
    }

    [Test]
    public void TestGetStoreResponseUidSearchNothingMatched()
    {
      GetResponseUidSearchNothingMatched(ImapWebRequestMethods.Store);
    }

    [Test]
    public void TestGetExpungeResponseUidSearchNothingMatched()
    {
      GetResponseUidSearchNothingMatched(ImapWebRequestMethods.Expunge);
    }

    private void GetResponseUidSearchNothingMatched(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH\r\n" +
          "0003 OK done\r\n",
        };

        Request(server, method, false, "?UNSEEN", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSearchResponseUidSearchFailureBadCharset()
    {
      GetResponseUidSearchFailureBadCharset(ImapWebRequestMethods.Search);
    }

    [Test]
    public void TestGetCopyResponseUidSearchFailureBadCharset()
    {
      GetResponseUidSearchFailureBadCharset(ImapWebRequestMethods.Copy);
    }

    [Test]
    public void TestGetStoreResponseUidSearchFailureBadCharset()
    {
      GetResponseUidSearchFailureBadCharset(ImapWebRequestMethods.Store);
    }

    [Test]
    public void TestGetExpungeResponseUidSearchFailureBadCharset()
    {
      GetResponseUidSearchFailureBadCharset(ImapWebRequestMethods.Expunge);
    }

    private void GetResponseUidSearchFailureBadCharset(string method)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "0003 NO [BADCHARSET (SHIFT_JIS X-UNKNOWN-CHARSET)] failed\r\n",
        };

        Request(server, method, false, "?UID 1", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.AreEqual(1, response.SupportedCharsets.Length);
          Assert.AreEqual(Encoding.GetEncoding("shift_jis").WebName, response.SupportedCharsets[0].WebName);
        });

        StringAssert.EndsWith("UID SEARCH UID 1\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSortResponse()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SORT
          "* SORT 5 3 4\r\n" + 
          "0003 OK done\r\n",
          // UID FETCH
          "* 1 FETCH (UID 3)\r\n" +
          "* 2 FETCH (UID 4)\r\n" +
          "* 3 FETCH (UID 5)\r\n" +
          "0004 OK done\r\n"
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.SortCriteria = ImapSortCriteria.Date + ImapSortCriteria.From;
        };

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(3, response.MessageAttributes.Length);

          var mailbox = new Uri(request.RequestUri.GetLeftPart(UriPartial.Path));

          Assert.AreEqual(new Uri(mailbox + "/;UID=5"), response.MessageAttributes[0].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=3"), response.MessageAttributes[1].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=4"), response.MessageAttributes[2].Url);
        });

        StringAssert.EndsWith("UID SORT (DATE FROM) UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID FETCH 5,3,4 ALL\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSortResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SORT
          "* SORT 5 3 4\r\n" + 
          "0003 OK done\r\n",
          // UID FETCH
          "0004 NO failed\r\n"
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.SortCriteria = ImapSortCriteria.Date + ImapSortCriteria.From;
          req.FetchDataItem = ImapFetchDataItemMacro.Fast;
        };

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID SORT (DATE FROM) UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID FETCH 5,3,4 FAST\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSortResponseUidSortFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SORT
          "0003 NO failed\r\n",
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.SortCriteria = ImapSortCriteria.Date;
        };

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID SORT (DATE) utf-8 SUBJECT hoge\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSortResponseUidSortFailureBadCharset()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SORT
          "0003 NO [BADCHARSET (SHIFT_JIS X-UNKNOWN-CHARSET)] failed\r\n",
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.SortCriteria = ImapSortCriteria.Date;
        };

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.AreEqual(1, response.SupportedCharsets.Length);
          Assert.AreEqual(Encoding.GetEncoding("shift_jis").WebName, response.SupportedCharsets[0].WebName);
        });

        StringAssert.EndsWith("UID SORT (DATE) utf-8 SUBJECT hoge\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSortResponseUidSortNothingMatched()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SORT
          "* SORT\r\n" +
          "0003 OK done\r\n",
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.SortCriteria = ImapSortCriteria.Date;
        };

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID SORT (DATE) utf-8 SUBJECT hoge\r\n", server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetSortResponseSortCriteriaNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?SUBJECT hoge", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNull(request.SortCriteria);
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test]
    public void TestGetThreadResponse()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID THREAD
          "* THREAD (5)(3 4 (8)(9 7))\r\n" + 
          "0003 OK done\r\n",
          // UID FETCH
          "* 1 FETCH (UID 3)\r\n" +
          "* 2 FETCH (UID 4)\r\n" +
          "* 3 FETCH (UID 5)\r\n" +
          "* 4 FETCH (UID 7)\r\n" +
          "* 5 FETCH (UID 8)\r\n" +
          "* 6 FETCH (UID 9)\r\n" +
          "0004 OK done\r\n"
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.ThreadingAlgorithm = ImapThreadingAlgorithm.OrderedSubject;
        };

        Request(server, ImapWebRequestMethods.Thread, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(6, response.MessageAttributes.Length);

          var mailbox = new Uri(request.RequestUri.GetLeftPart(UriPartial.Path));

          Assert.AreEqual(new Uri(mailbox + "/;UID=3"), response.MessageAttributes[0].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=4"), response.MessageAttributes[1].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=5"), response.MessageAttributes[2].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=7"), response.MessageAttributes[3].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=8"), response.MessageAttributes[4].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=9"), response.MessageAttributes[5].Url);

          Assert.IsNotNull(response.ThreadTree);

          var expectedResults = new[] {
            new {Url = new Uri(mailbox + "/;UID=5"), Message = response.MessageAttributes[2]},
            new {Url = new Uri(mailbox + "/;UID=3"), Message = response.MessageAttributes[0]},
            new {Url = new Uri(mailbox + "/;UID=4"), Message = response.MessageAttributes[1]},
            new {Url = new Uri(mailbox + "/;UID=8"), Message = response.MessageAttributes[4]},
            new {Url = new Uri(mailbox + "/;UID=9"), Message = response.MessageAttributes[5]},
            new {Url = new Uri(mailbox + "/;UID=7"), Message = response.MessageAttributes[3]},
          };
          var index = 0;

          response.ThreadTree.Traverse(delegate(ImapThreadTree tree) {
            Assert.AreEqual(expectedResults[index].Url, tree.MessageAttribute.Url);
            Assert.AreSame(expectedResults[index].Message, tree.MessageAttribute);
            index++;
          });

          Assert.AreEqual(2,  response.ThreadTree.Children.Length);
          Assert.AreEqual(5L, response.ThreadTree.Children[0].MessageAttribute.Uid);
          Assert.AreEqual(3L, response.ThreadTree.Children[1].MessageAttribute.Uid);
          Assert.AreEqual(1,  response.ThreadTree.Children[1].Children.Length);
          Assert.AreEqual(4L, response.ThreadTree.Children[1].Children[0].MessageAttribute.Uid);
          Assert.AreEqual(2,  response.ThreadTree.Children[1].Children[0].Children.Length);
          Assert.AreEqual(8L, response.ThreadTree.Children[1].Children[0].Children[0].MessageAttribute.Uid);
          Assert.AreEqual(9L, response.ThreadTree.Children[1].Children[0].Children[1].MessageAttribute.Uid);
          Assert.AreEqual(1,  response.ThreadTree.Children[1].Children[0].Children[1].Children.Length);
          Assert.AreEqual(7L, response.ThreadTree.Children[1].Children[0].Children[1].Children[0].MessageAttribute.Uid);
        });

        StringAssert.EndsWith("UID THREAD ORDEREDSUBJECT UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID FETCH 5,3,4,8,9,7 ALL\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetThreadResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID THREAD
          "* THREAD (5)(3 4 (8)(9 7))\r\n" + 
          "0003 OK done\r\n",
          // UID FETCH
          "0004 NO failed\r\n"
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.ThreadingAlgorithm = ImapThreadingAlgorithm.OrderedSubject;
          req.FetchDataItem = ImapFetchDataItemMacro.Fast;
        };

        Request(server, ImapWebRequestMethods.Thread, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.ThreadTree);
          Assert.IsEmpty(response.ThreadTree.Children);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID THREAD ORDEREDSUBJECT UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID FETCH 5,3,4,8,9,7 FAST\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetThreadResponseUidThreadFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID THREAD
          "0003 NO failed\r\n",
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.ThreadingAlgorithm = ImapThreadingAlgorithm.OrderedSubject;
        };

        Request(server, ImapWebRequestMethods.Thread, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.ThreadTree);
          Assert.IsEmpty(response.ThreadTree.Children);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID THREAD ORDEREDSUBJECT UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetThreadResponseUidThreadFailureBadCharset()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID THREAD
          "0003 NO [BADCHARSET (SHIFT_JIS X-UNKNOWN-CHARSET)] failed\r\n",
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.ThreadingAlgorithm = ImapThreadingAlgorithm.OrderedSubject;
        };

        Request(server, ImapWebRequestMethods.Thread, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.ThreadTree);
          Assert.IsEmpty(response.ThreadTree.Children);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.AreEqual(1, response.SupportedCharsets.Length);
          Assert.AreEqual(Encoding.GetEncoding("shift_jis").WebName, response.SupportedCharsets[0].WebName);
        });

        StringAssert.EndsWith("UID THREAD ORDEREDSUBJECT UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetThreadResponseUidThreadNothingMatched()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID THREAD
          "* THREAD\r\n" + 
          "0003 OK done\r\n",
        };

        Action<ImapWebRequest> presetRequest = delegate(ImapWebRequest req) {
          req.ThreadingAlgorithm = ImapThreadingAlgorithm.OrderedSubject;
        };

        Request(server, ImapWebRequestMethods.Thread, new[] {"IMAP4rev1"}, "?CHARSET UTF-8 SUBJECT hoge", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.ThreadTree);
          Assert.IsEmpty(response.ThreadTree.Children);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);

          Assert.IsNotNull(response.SupportedCharsets);
          Assert.IsEmpty(response.SupportedCharsets);
        });

        StringAssert.EndsWith("UID THREAD ORDEREDSUBJECT UTF-8 SUBJECT hoge\r\n", server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetThreadResponseThreadingAlgorithmNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[] {};

        Request(server, ImapWebRequestMethods.Sort, new[] {"IMAP4rev1"}, "?SUBJECT hoge", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNull(request.ThreadingAlgorithm);
          Assert.Fail("exception not thrown");
        });
      }
    }

    [Test]
    public void TestGetSearchResponse()
    {
      GetSearchResponse(false);
    }

    [Test]
    public void TestGetSearchResponseSearchresCapable()
    {
      GetSearchResponse(true);
    }

    private void GetSearchResponse(bool searchresCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          searchresCapable
            ? "0003 OK done\r\n"
            : "* SEARCH 3 4 5\r\n" + 
              "0003 OK done\r\n",
          // UID FETCH
          "* 1 FETCH (UID 3)\r\n" +
          "* 2 FETCH (UID 4)\r\n" +
          "* 3 FETCH (UID 5)\r\n" +
          "0004 OK done\r\n"
        };

        Request(server, ImapWebRequestMethods.Search, searchresCapable, "?UNSEEN", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(3, response.MessageAttributes.Length);

          var mailbox = new Uri(request.RequestUri.GetLeftPart(UriPartial.Path));

          Assert.AreEqual(new Uri(mailbox + "/;UID=3"), response.MessageAttributes[0].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=4"), response.MessageAttributes[1].Url);
          Assert.AreEqual(new Uri(mailbox + "/;UID=5"), response.MessageAttributes[2].Url);
        });

        if (searchresCapable) {
          StringAssert.EndsWith("UID SEARCH RETURN (SAVE) UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID FETCH $ ALL\r\n", server.DequeueRequest());
        }
        else {
          StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID FETCH 3,4,5 ALL\r\n", server.DequeueRequest());
        }
      }
    }

    [Test]
    public void TestGetSearchResponseCriteriaWithLiteral()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
          // UID FETCH
          "* 1 FETCH (UID 3)\r\n" +
          "* 2 FETCH (UID 4)\r\n" +
          "* 3 FETCH (UID 5)\r\n" +
          "0004 OK done\r\n"
        };

        var b = new ImapUriBuilder(new Uri(string.Format("imap://user@{0}/", server.HostPort)));

        b.Mailbox = "INBOX";
        b.SearchCriteria = ImapSearchCriteria.From("差出人") & (ImapSearchCriteria.Seen | ImapSearchCriteria.Subject("件名"));
        b.Charset = Encoding.UTF8;

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.FetchDataItem = ImapFetchDataItemMacro.Full;
        };

        Request(server, ImapWebRequestMethods.Search, new string[] {"IMAP4rev1", "LITERAL+"}, b.Uri, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.AreEqual(3, response.MessageAttributes.Length);

          b.SearchCriteria = null;

          b.Uid = 3L; Assert.AreEqual(b.Uri, response.MessageAttributes[0].Url);
          b.Uid = 4L; Assert.AreEqual(b.Uri, response.MessageAttributes[1].Url);
          b.Uid = 5L; Assert.AreEqual(b.Uri, response.MessageAttributes[2].Url);
        });

        StringAssert.EndsWith("UID SEARCH CHARSET utf-8 FROM {9+}\r\n",
                              server.DequeueRequest(NetworkTransferEncoding.Transfer8Bit));
        Assert.AreEqual("\x00E5\x00B7\x00AE\x00E5\x0087\x00BA\x00E4\x00BA\x00BA OR (SEEN) (SUBJECT {6+}\r\n",
                        server.DequeueRequest(NetworkTransferEncoding.Transfer8Bit));
        Assert.AreEqual("\x00E4\x00BB\x00B6\x00E5\x0090\x008D)\r\n",
                        server.DequeueRequest(NetworkTransferEncoding.Transfer8Bit));
        StringAssert.EndsWith("UID FETCH 3,4,5 FULL\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetSearchResponseCriteriaWithLiteralNonSyncLiteralIncapable()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new string[0];
        var b = new ImapUriBuilder(new Uri(string.Format("imap://user@{0}/", server.HostPort)));

        b.Mailbox = "INBOX";
        b.SearchCriteria = ImapSearchCriteria.From("差出人") & (ImapSearchCriteria.Seen | ImapSearchCriteria.Subject("件名"));
        b.Charset = Encoding.UTF8;

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          // nothing to do
        };

        try {
          Request(server, ImapWebRequestMethods.Search, new string[] {"IMAP4rev1"}, b.Uri, methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
            Assert.Fail("ProtocolViolationException not thrown");
          });
        }
        catch (ProtocolViolationException) {
        }
      }
    }

    [Test]
    public void TestGetSearchResponseFailure()
    {
      GetSearchResponseFailure(false);
    }

    [Test]
    public void TestGetSearchResponseFailureSearchresCapable()
    {
      GetSearchResponseFailure(true);
    }

    private void GetSearchResponseFailure(bool searchresCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          searchresCapable
            ? "0003 OK done\r\n"
            : "* SEARCH 3 4 5\r\n" + 
              "0003 OK done\r\n",
          // UID FETCH
          "0004 NO failed\r\n"
        };

        Request(server, ImapWebRequestMethods.Search, searchresCapable, "?UNSEEN", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.MessageAttributes);
          Assert.IsEmpty(response.MessageAttributes);
        });

        if (searchresCapable) {
          StringAssert.EndsWith("UID SEARCH RETURN (SAVE) UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID FETCH $ ALL\r\n", server.DequeueRequest());
        }
        else {
          StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID FETCH 3,4,5 ALL\r\n", server.DequeueRequest());
        }
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetCopyResponseDestinationUriNotSet()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
        };

        Request(server, ImapWebRequestMethods.Copy, false, "?UNSEEN", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("InvalidOperationException not thrown");
        });
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetCopyResponseInvalidDestinationUriForm1()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.DestinationUri = new Uri(new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority)), "copyto-mailbox/;UID=1");
        };

        Request(server, ImapWebRequestMethods.Copy, new[] {"IMAP4REV1"}, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("InvalidOperationException not thrown");
        });
      }
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetCopyResponseInvalidDestinationUriForm2()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Authority));
        };

        Request(server, ImapWebRequestMethods.Copy, new[] {"IMAP4REV1"}, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.Fail("InvalidOperationException not thrown");
        });
      }
    }

    [Test]
    public void TestGetCopyResponseFailure()
    {
      GetCopyResponseFailure(false);
    }

    [Test]
    public void TestGetCopyResponseFailureSearchresCapable()
    {
      GetCopyResponseFailure(true);
    }

    private void GetCopyResponseFailure(bool searchresCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          searchresCapable
            ? "0003 OK done\r\n"
            : "* SEARCH 3 4 5\r\n" + 
              "0003 OK done\r\n",
          // UID COPY
          "0004 NO [TRYCREATE] failed\r\n",
        };

        var capabilities = searchresCapable
          ? new[] {"IMAP4REV1", "SEARCHRES"}
          : new[] {"IMAP4REV1"};

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = false;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, capabilities, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        if (searchresCapable) {
          StringAssert.EndsWith("UID SEARCH RETURN (SAVE) UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID COPY $ \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        }
        else {
          StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID COPY 3,4,5 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        }
      }
    }

    [Test]
    public void TestGetCopyResponse()
    {
      GetCopyResponse(false);
    }

    [Test]
    public void TestGetCopyResponseSearchresCapable()
    {
      GetCopyResponse(true);
    }

    private void GetCopyResponse(bool searchresCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          searchresCapable
            ? "0003 OK done\r\n"
            : "* SEARCH 3 4 5\r\n" + 
              "0003 OK done\r\n",
          // UID COPY
          "0004 OK done\r\n"
        };

        var capabilities = searchresCapable
          ? new[] {"IMAP4REV1", "SEARCHRES"}
          : new[] {"IMAP4REV1"};

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = false;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, capabilities, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);
        });

        if (searchresCapable) {
          StringAssert.EndsWith("UID SEARCH RETURN (SAVE) UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID COPY $ \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        }
        else {
          StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID COPY 3,4,5 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        }
      }
    }

    [Test]
    public void TestGetCopyResponseCopyUidResponseExists()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
          // UID COPY
          "0004 OK [COPYUID 38505 3,4,5 3956:3958] done\r\n"
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = false;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, new[] {"IMAP4rev1"}, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(new Uri(string.Format("{0}/INBOX/コピー先メールボックス;UIDVALIDITY=38505?UID 3956:3958", request.RequestUri.GetLeftPart(UriPartial.Authority))),
                response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);
        });

        StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID COPY 3,4,5 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
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
            // UID SEARCH
            "* SEARCH 3 4 5\r\n" + 
            "0003 OK done\r\n",
            // UID COPY
            "0004 NO [TRYCREATE] failed\r\n",
            // CREATE
            "0005 OK done\r\n",
            // UID COPY
            "0006 OK done\r\n",
            // SUBSCRIBE
            "0007 OK done\r\n",
            // LSUB
            "* LSUB () \".\" \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n" + 
            "0008 OK done\r\n"
          }
          : new[] {
            // UID SEARCH
            "* SEARCH 3 4 5\r\n" + 
            "0003 OK done\r\n",
            // UID COPY
            "0004 NO [TRYCREATE] failed\r\n",
            // CREATE
            "0005 OK done\r\n",
            // UID COPY
            "0006 OK done\r\n",
            // LIST
            "* LIST () \".\" \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n" + 
            "0007 OK done\r\n"
          };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = true;
          req.Subscription = subscription;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, new[] {"IMAP4REV1"}, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);
          Assert.AreEqual(new Uri(new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), "INBOX/コピー先メールボックス"),
                          response.Mailboxes[0].Url);
        });

        StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID COPY 3,4,5 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID COPY 3,4,5 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());

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
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
          // UID COPY
          "0004 NO [TRYCREATE] failed\r\n",
          // CREATE
          "0005 NO failed\r\n",
        };

        var presetRequest = (Action<ImapWebRequest>)delegate(ImapWebRequest req) {
          req.AllowCreateMailbox = true;
          req.DestinationUri = new Uri(req.RequestUri.GetLeftPart(UriPartial.Path) + "/コピー先メールボックス");
        };

        Request(server, ImapWebRequestMethods.Copy, new[] {"IMAP4REV1"}, "?UNSEEN", methodResponses, presetRequest, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID COPY 3,4,5 \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
        StringAssert.EndsWith("CREATE \"INBOX/&MLMw1DD8UUgw4TD8MOsw3DDDMK8wuQ-\"\r\n", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetExpungeResponseStoreDeletedFlagFailure()
    {
      GetExpungeResponseStoreDeletedFlagFailure(false);
    }

    [Test]
    public void TestGetExpungeResponseStoreDeletedFlagFailureSearchresCapable()
    {
      GetExpungeResponseStoreDeletedFlagFailure(true);
    }

    private void GetExpungeResponseStoreDeletedFlagFailure(bool searchresCapable)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          searchresCapable
            ? "0003 OK done\r\n"
            : "* SEARCH 3 4 5\r\n" + 
              "0003 OK done\r\n",
          // UID STORE
          "0004 NO failed\r\n"
        };

        Request(server, ImapWebRequestMethods.Expunge, searchresCapable, "?UNSEEN", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        if (searchresCapable) {
          StringAssert.EndsWith("UID SEARCH RETURN (SAVE) UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID STORE $ +FLAGS.SILENT (\\Deleted)\r\n", server.DequeueRequest());
        }
        else {
          StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
          StringAssert.EndsWith("UID STORE 3,4,5 +FLAGS.SILENT (\\Deleted)\r\n", server.DequeueRequest());
        }
      }
    }

    [Test]
    public void TestGetExpungeResponseFailure()
    {
      const bool uidplusCapable = false;
      const bool expungeFailure = true;

      GetExpungeResponse(uidplusCapable, expungeFailure);
    }

    [Test]
    public void TestGetExpungeResponseFailureUidplusCapable()
    {
      const bool uidplusCapable = true;
      const bool expungeFailure = true;

      GetExpungeResponse(uidplusCapable, expungeFailure);
    }

    [Test]
    public void TestGetExpungeResponse()
    {
      const bool uidplusCapable = false;
      const bool expungeFailure = false;

      GetExpungeResponse(uidplusCapable, expungeFailure);
    }

    [Test]
    public void TestGetExpungeResponseUidplusCapable()
    {
      const bool uidplusCapable = true;
      const bool expungeFailure = false;

      GetExpungeResponse(uidplusCapable, expungeFailure);
    }

    private void GetExpungeResponse(bool uidplusCapable, bool expungeFailure)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponses = new[] {
          // UID SEARCH
          "* SEARCH 3 4 5\r\n" + 
          "0003 OK done\r\n",
          // UID STORE
          "0004 OK done\r\n",
          // EXPUNGE/UID EXPUNGE
          expungeFailure
          ? "0005 NO failed\r\n"
          : "* 1 EXPUNGE\r\n" +
            "* 1 EXPUNGE\r\n" +
            "* 1 EXPUNGE\r\n" +
            "0005 OK done\r\n",
        };
        var capabilities = uidplusCapable
          ? new[] {"IMAP4REV1", "UIDPLUS"}
          : new[] {"IMAP4REV1"};

        Request(server, ImapWebRequestMethods.Expunge, capabilities, "?UNSEEN", methodResponses, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.AreEqual(request.RequestUri, response.ResponseUri);

          Assert.IsNotNull(response);
          Assert.IsNotNull(response.Result);
          Assert.AreNotEqual(response.Result.Succeeded, expungeFailure);
        });

        StringAssert.EndsWith("UID SEARCH UNSEEN\r\n", server.DequeueRequest());
        StringAssert.EndsWith("UID STORE 3,4,5 +FLAGS.SILENT (\\Deleted)\r\n", server.DequeueRequest());

        if (uidplusCapable)
          StringAssert.EndsWith("UID EXPUNGE 3,4,5\r\n", server.DequeueRequest());
        else
          StringAssert.EndsWith("EXPUNGE\r\n", server.DequeueRequest());
      }
    }
  }
}
