using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Client.Session {
  public abstract class PopSessionTestsBase {
    protected class NullCredential : ICredentialsByHost {
      public NetworkCredential GetCredential(string host, int port, string authType)
      {
        return null;
      }
    }

    [SetUp]
    public void Setup()
    {
      server = new PopPseudoServer();
      server.Start();

      host = server.ServerEndPoint.Address.ToString();
      port = server.ServerEndPoint.Port;

      credential = new NetworkCredential(username, password, host);
      authority = new Uri(string.Format("{0}://{1}@{2}:{3}",
                                  PopUri.UriSchemePop,
                                  username,
                                  host,
                                  port));
    }

    [TearDown]
    public void TearDown()
    {
      server.Stop();
    }

    protected PopSession Connect(string timestamp)
    {
      if (string.IsNullOrEmpty(timestamp))
        server.EnqueueResponse("+OK POP3 server ready\r\n");
      else
        server.EnqueueResponse(string.Format("+OK POP3 server ready {0}\r\n", timestamp));

      var session = new PopSession(host, port);

      Assert.AreEqual(PopSessionState.Authorization, session.State);
      Assert.AreEqual(authority, session.Authority);

      return session;
    }

    protected PopSession Login(params PopCapability[] capabilities)
    {
      var session = Connect(null);

      if (0 < capabilities.Length) {
        var resp = "+OK Capability list follows\r\n" + string.Join("\r\n", (new PopCapabilityList(capabilities)).ToStringArray()) + "\r\n.\r\n";

        server.EnqueueResponse(resp);

        Assert.IsTrue((bool)session.Capa());

        StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                          server.DequeueRequest());
      }

      server.EnqueueResponse("+OK\r\n");
      server.EnqueueResponse("+OK\r\n");

      Assert.IsTrue((bool)session.Login(credential));

      StringAssert.AreEqualIgnoringCase(string.Format("USER {0}\r\n", credential.UserName),
                                        server.DequeueRequest());
      StringAssert.AreEqualIgnoringCase(string.Format("PASS {0}\r\n", credential.Password),
                                        server.DequeueRequest());

      Assert.AreEqual(PopSessionState.Transaction, session.State);
      Assert.AreEqual(authority, session.Authority);

      session.HandlesIncapableAsException = true;

      return session;
    }

    protected int port;
    protected string host;
    protected Uri authority;
    protected const string username = "popuser";
    protected const string password = "password";
    protected NetworkCredential credential;

    protected PopPseudoServer server;
  }
}
