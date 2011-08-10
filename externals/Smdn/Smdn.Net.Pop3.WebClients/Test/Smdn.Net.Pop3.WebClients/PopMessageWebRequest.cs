using System;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;

using PopPseudoServer = Smdn.Net.Pop3.Client.Session.PopPseudoServer;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopMessageWebRequestTests {
    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    private void Request(PopPseudoServer server, string method, string methodResponse, Action<PopWebRequest, PopWebResponse> responseAction)
    {
      Request(server, method, new[] {methodResponse}, null, responseAction);
    }

    private void Request(PopPseudoServer server, string method, string[] methodResponses, Action<PopWebRequest> presetRequest, Action<PopWebRequest, PopWebResponse> responseAction)
    {
      server.Start();

      var request = WebRequest.Create(string.Format("pop://{0}/;MSG=1", server.HostPort)) as PopWebRequest;

      request.KeepAlive = false;
      request.Timeout = 1000;
      request.Method = method;

      // greeting
      server.EnqueueResponse("+OK\r\n");
      // CAPA
      server.EnqueueResponse("+OK\r\n" +
                             ".\r\n");
      // USER
      server.EnqueueResponse("+OK\r\n");
      // PASS
      server.EnqueueResponse("+OK\r\n");

      foreach (var resp in methodResponses) {
        server.EnqueueResponse(resp);
      }

      // QUIT
      server.EnqueueResponse("+OK\r\n");

      try {
        if (presetRequest != null)
          presetRequest(request);

        using (var response = request.GetResponse() as PopWebResponse) {
          responseAction(request, response);
        }
      }
      catch (WebException ex) {
        if (ex.Status == WebExceptionStatus.ProtocolError)
          responseAction(request, ex.Response as PopWebResponse);
        else
          throw ex;
      }

      server.DequeueRequest(); // CAPA
      server.DequeueRequest(); // USER
      server.DequeueRequest(); // PASS
    }

    [Test]
    public void TestMethodNoOp()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK\r\n";

        Request(server, PopWebRequestMethods.NoOp, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.StartsWith("NOOP", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetRetrResponse()
    {
      GetRetrResponse(false);
    }

    [Test]
    public void TestGetRetrResponseDeleteAfterRetrieve()
    {
      GetRetrResponse(true);
    }

    private void GetRetrResponse(bool deleAfterRetr)
    {
      var message = @"MIME-Version: 1.0
From: from
To: to
Subect: subject
Date: Sat, 16 Jan 2010 00:41:20 +0900

1st line
2nd line
3rd line
".Replace("\r\n", "\n").Replace("\n", "\r\n");

      using (var server = new PopPseudoServer()) {
        var methodResponses = deleAfterRetr
          ? new[] {
            // RETR
            "+OK\r\n" +
            message +
            ".\r\n",
            // DELE
            "+OK\r\n",
          }
          : new[] {
            // RETR
            "+OK\r\n" +
            message +
            ".\r\n",
          };

        Action<PopWebRequest> presetRequest = delegate(PopWebRequest req) {
          req.DeleteAfterRetrieve = deleAfterRetr;
        };

        Request(server, PopWebRequestMethods.Retr, methodResponses, presetRequest, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.AreEqual(Encoding.ASCII.GetByteCount(message), response.ContentLength);

          using (var responseStream = response.GetResponseStream()) {
            Assert.IsNotNull(responseStream);

            Assert.AreEqual(message, (new StreamReader(responseStream, Encoding.ASCII)).ReadToEnd());
          }
        });

        StringAssert.StartsWith("RETR 1", server.DequeueRequest());

        if (deleAfterRetr)
          StringAssert.StartsWith("DELE 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetRetrResponseErr()
    {
      GetRetrResponseErr(false);
    }

    [Test]
    public void TestGetRetrResponseErrDeleteAfterRetrieve()
    {
      GetRetrResponseErr(true);
    }

    private void GetRetrResponseErr(bool deleAfterRetr)
    {
      using (var server = new PopPseudoServer()) {
        var methodResponses = new [] {
          // RETR
          "-ERR\r\n",
        };

        Action<PopWebRequest> presetRequest = delegate(PopWebRequest req) {
          req.DeleteAfterRetrieve = deleAfterRetr;
        };

        Request(server, PopWebRequestMethods.Retr, methodResponses, presetRequest, delegate(PopWebRequest request, PopWebResponse response) {
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

        StringAssert.StartsWith("RETR 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetTopResponse()
    {
      var message = @"MIME-Version: 1.0
From: from
To: to
Subect: subject
Date: Sat, 16 Jan 2010 00:41:20 +0900
".Replace("\r\n", "\n").Replace("\n", "\r\n");

      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK\r\n" +
          message +
          ".\r\n";

        Request(server, PopWebRequestMethods.Top, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.AreEqual(Encoding.ASCII.GetByteCount(message), response.ContentLength);

          using (var responseStream = response.GetResponseStream()) {
            Assert.IsNotNull(responseStream);

            Assert.AreEqual(message, (new StreamReader(responseStream, Encoding.ASCII)).ReadToEnd());
          }
        });

        StringAssert.StartsWith("TOP 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetTopResponseErr()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "-ERR\r\n";

        Request(server, PopWebRequestMethods.Top, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
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

        StringAssert.StartsWith("TOP 1 0", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetDeleResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK\r\n";

        Request(server, PopWebRequestMethods.Dele, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.StartsWith("DELE 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetDeleResponseErr()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "-ERR\r\n";

        Request(server, PopWebRequestMethods.Dele, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);
        });

        StringAssert.StartsWith("DELE 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetListResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK 1 120\r\n";

        Request(server, PopWebRequestMethods.List, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.ScanLists);
          Assert.AreEqual(1, response.ScanLists.Length);

          Assert.AreEqual(1L, response.ScanLists[0].MessageNumber);
          Assert.AreEqual(120L, response.ScanLists[0].SizeInOctets);
        });

        StringAssert.StartsWith("LIST 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetListResponseErr()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "-ERR\r\n";

        Request(server, PopWebRequestMethods.List, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsEmpty(response.ScanLists);
        });

        StringAssert.StartsWith("LIST 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetUidlResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK 1 QhdPYR:00WBw1Ph7x7\r\n";

        Request(server, PopWebRequestMethods.Uidl, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.UniqueIdLists);
          Assert.AreEqual(1, response.UniqueIdLists.Length);

          Assert.AreEqual(1L, response.UniqueIdLists[0].MessageNumber);
          Assert.AreEqual("QhdPYR:00WBw1Ph7x7", response.UniqueIdLists[0].UniqueId);
        });

        StringAssert.StartsWith("UIDL 1", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetUidlResponseErr()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "-ERR\r\n";

        Request(server, PopWebRequestMethods.Uidl, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsEmpty(response.UniqueIdLists);
        });

        StringAssert.StartsWith("UIDL 1", server.DequeueRequest());
      }
    }
  }
}