using System;
using System.Net;
using NUnit.Framework;

using PopPseudoServer = Smdn.Net.Pop3.Client.Session.PopPseudoServer;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopMailboxWebRequestTests {
    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    private void Request(PopPseudoServer server, string method, string methodResponse, Action<PopWebRequest, PopWebResponse> responseAction)
    {
      server.Start();

      var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

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

      server.EnqueueResponse(methodResponse);

      // QUIT
      server.EnqueueResponse("+OK\r\n");

      try {
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
    public void TestGetListResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK\r\n" +
                             "1 120\r\n" +
                             "2 210\r\n" +
                             ".\r\n";

        Request(server, PopWebRequestMethods.List, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.ScanLists);
          Assert.AreEqual(2, response.ScanLists.Length);

          Assert.AreEqual(1L, response.ScanLists[0].MessageNumber);
          Assert.AreEqual(120L, response.ScanLists[0].SizeInOctets);

          Assert.AreEqual(2L, response.ScanLists[1].MessageNumber);
          Assert.AreEqual(210L, response.ScanLists[1].SizeInOctets);
        });

        StringAssert.StartsWith("LIST", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetUidlResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK\r\n" +
                             "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                             "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                             ".\r\n";

        Request(server, PopWebRequestMethods.Uidl, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.UniqueIdLists);
          Assert.AreEqual(2, response.UniqueIdLists.Length);

          Assert.AreEqual(1L, response.UniqueIdLists[0].MessageNumber);
          Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", response.UniqueIdLists[0].UniqueId);

          Assert.AreEqual(2L, response.UniqueIdLists[1].MessageNumber);
          Assert.AreEqual("QhdPYR:00WBw1Ph7x7", response.UniqueIdLists[1].UniqueId);
        });

        StringAssert.StartsWith("UIDL", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetRsetResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK\r\n";

        Request(server, PopWebRequestMethods.Rset, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);
        });

        StringAssert.StartsWith("RSET", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetStatResponse()
    {
      using (var server = new PopPseudoServer()) {
        var methodResponse = "+OK 2 320\r\n";

        Request(server, PopWebRequestMethods.Stat, methodResponse, delegate(PopWebRequest request, PopWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.DropList);

          Assert.AreEqual(2L, response.DropList.MessageCount);
          Assert.AreEqual(320L, response.DropList.SizeInOctets);
        });

        StringAssert.StartsWith("STAT", server.DequeueRequest());
      }
    }
  }
}