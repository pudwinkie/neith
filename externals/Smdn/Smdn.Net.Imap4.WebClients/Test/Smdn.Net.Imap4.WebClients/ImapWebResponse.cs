using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapWebResponseTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    [Test]
    public void TestGetResponseStreamResponseHasNoStream()
    {
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Timeout = 1000;
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
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

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
      using (var server = new ImapPseudoServer()) {
        server.Start();

        var request = WebRequest.Create(string.Format("imap://{0}/", server.HostPort)) as ImapWebRequest;

        request.Timeout = 1000;
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
        // LOGOUT
        server.EnqueueResponse("* BYE logging out\r\n" +
                               "0003 OK done\r\n");

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
