using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapServerWebRequestTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    private void Request(ImapPseudoServer server, string method, string methodResponse, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, new[] {"IMAP4rev1"}, new[] {methodResponse}, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, string methodResponse, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      Request(server, method, capabilities, new[] {methodResponse}, responseAction);
    }

    private void Request(ImapPseudoServer server, string method, string[] capabilities, string[] methodResponses, Action<ImapWebRequest, ImapWebResponse> responseAction)
    {
      server.Start();

      var request = WebRequest.Create(string.Format("imap://user@{0}/", server.HostPort)) as ImapWebRequest;

      Assert.AreEqual("ImapServerWebRequest", request.GetType().Name);

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

      int commandTag = 2;

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

      server.DequeueRequest(); // CAPABILITY
      server.DequeueRequest(); // LOGIN
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
    public void TestDefaultMethod()
    {
      WebRequest request;

      var defaultSubscription = ImapWebRequestDefaults.Subscription;

      try {
        ImapWebRequestDefaults.Subscription = true;

        request = WebRequest.Create("imap://localhost/");

        Assert.AreEqual(ImapWebRequestMethods.Lsub, request.Method);

        ImapWebRequestDefaults.Subscription = false;

        request = WebRequest.Create("imap://localhost/");

        Assert.AreEqual(ImapWebRequestMethods.List, request.Method);
      }
      finally {
        ImapWebRequestDefaults.Subscription = defaultSubscription;
      }
    }

    [Test]
    public void TestGetListResponse()
    {
      GetListLsubResponse("LIST");
    }

    [Test]
    public void TestGetLsubResponse()
    {
      GetListLsubResponse("LSUB");
    }

    private void GetListLsubResponse(string command)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = string.Format("* {0} (\\Noselect) \"/\" ~/Mail/foo\r\n", command) +
                             "0002 OK done\r\n";

        Request(server, command, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);

          Assert.AreEqual(new Uri(request.RequestUri, "~/Mail/foo"), response.Mailboxes[0].Url);
        });

        StringAssert.Contains(command + " \"\" *", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetListResponseFailure()
    {
      GetListLsubResponseFailure("LIST");
    }

    [Test]
    public void TestGetLsubResponseFailure()
    {
      GetListLsubResponseFailure("LSUB");
    }

    private void GetListLsubResponseFailure(string command)
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = "0002 NO failed\r\n";

        Request(server, command, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);
        });

        StringAssert.Contains(command + " \"\" *", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetXListResponse()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = "* XLIST (\\HasNoChildren \\Starred) \"/\" \"[Gmail]/&MLkwvzD8TtgwTQ-\"\r\n" +
                             "0002 OK done\r\n";

        Request(server, ImapWebRequestMethods.XList, new[] {"IMAP4rev1", "XLIST"}, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Succeeded);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(1, response.Mailboxes.Length);

          Assert.AreEqual(new Uri(request.RequestUri, "[Gmail]/スター付き"), response.Mailboxes[0].Url);
        });

        StringAssert.Contains("XLIST \"\" *", server.DequeueRequest());
      }
    }

    [Test]
    public void TestGetXListResponseFailure()
    {
      using (var server = new ImapPseudoServer()) {
        var methodResponse = "0002 NO failed\r\n";

        Request(server, ImapWebRequestMethods.XList, new[] {"IMAP4rev1", "XLIST"}, methodResponse, delegate(ImapWebRequest request, ImapWebResponse response) {
          Assert.IsNotNull(response);

          Assert.AreEqual(response.ResponseUri, request.RequestUri);

          Assert.IsNotNull(response.Result);
          Assert.IsTrue(response.Result.Failed);

          Assert.IsNotNull(response.Mailboxes);
          Assert.AreEqual(0, response.Mailboxes.Length);
        });

        StringAssert.Contains("XLIST \"\" *", server.DequeueRequest());
      }
    }

    [Test, Ignore("XLIST incapable")]
    public void TestGetXListResponseXListIncapable()
    {
    }
  }
}