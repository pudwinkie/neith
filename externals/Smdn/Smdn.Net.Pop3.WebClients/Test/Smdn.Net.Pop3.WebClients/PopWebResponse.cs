using System;
using System.Net;
using NUnit.Framework;

using PopPseudoServer = Smdn.Net.Pop3.Client.Session.PopPseudoServer;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopWebResposeTests {
    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    [Test]
    public void TestGetResponseStreamResponseHasNoStream()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Timeout = 1000;
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
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        using (var response = request.GetResponse()) {
          try {
            response.GetResponseStream();
            Assert.Fail("InvalidOperationException not thrown");
          }
          catch (InvalidOperationException) {
          }
        }
      }
    }

    [Test]
    public void TestGetResponseStreamAfterClose()
    {
      using (var server = new PopPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("pop://{0}/", server.HostPort)) as PopWebRequest;

        request.Timeout = 1000;
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
        // QUIT
        server.EnqueueResponse("+OK\r\n");

        var response = request.GetResponse();

        response.Close();

        try {
          response.GetResponseStream();
          Assert.Fail("ObjectDisposedException not thrown");
        }
        catch (ObjectDisposedException) {
        }
      }
    }
  }
}
