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

    private class SessionContext : IDisposable {
      public PopPseudoServer Server {
        get; private set;
      }

      public PopSession Session {
        get; private set;
      }

      public Uri ExpectedAuthority {
        get; private set;
      }

      public SessionContext()
      {
        Server = new PopPseudoServer();
        Server.Start();
      }

      public void Dispose()
      {
        if (Server != null) {
          Server.Dispose();
          Server = null;
        }

        if (Session != null) {
          (Session as IDisposable).Dispose();
          Session = null;
        }
      }

      internal void SetSession(PopSession session)
      {
        this.Session = session;
      }

      internal void SetExpectedAuthority(Uri authority)
      {
        this.ExpectedAuthority = authority;
      }

      public const string UserName = "popuser";
      public const string Password = "password";
    }

    protected PopPseudoServer CreateServer()
    {
      var server = new PopPseudoServer();

      server.Start();

      return server;
    }

    private void Connect(Action<SessionContext> action)
    {
      using (var ctx = new SessionContext()) {
        ctx.Server.EnqueueResponse("+OK POP3 server ready\r\n");

        ctx.SetSession(new PopSession(ctx.Server.Host, ctx.Server.Port));
        ctx.SetExpectedAuthority(new Uri(string.Format("{0}://{1}@{2}:{3}",
                                                       PopUri.UriSchemePop,
                                                       SessionContext.UserName,
                                                       ctx.Server.Host,
                                                       ctx.Server.Port)));


        Assert.AreEqual(PopSessionState.Authorization, ctx.Session.State);
        Assert.AreEqual(ctx.ExpectedAuthority, ctx.Session.Authority);

        action(ctx);
      }
    }

    protected void Connect(Action<PopSession> action)
    {
      Connect(delegate(SessionContext ctx) {
        action(ctx.Session);
      });
    }

    protected void Connect(Action<PopSession, PopPseudoServer> action)
    {
      Connect(delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }

    protected void Connect(Action<PopSession, PopPseudoServer, Uri> action)
    {
      Connect(delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server, ctx.ExpectedAuthority);
      });
    }

    private void Login(PopCapability[] capabilities, Action<SessionContext> action)
    {
      Connect(delegate(SessionContext ctx) {
        if (capabilities != null && 0 < capabilities.Length) {
          var resp = "+OK Capability list follows\r\n" +
                     string.Join("\r\n", (new PopCapabilitySet(capabilities)).ToStringArray()) +
                     "\r\n.\r\n";

          ctx.Server.EnqueueResponse(resp);

          Assert.IsTrue((bool)ctx.Session.Capa());

          StringAssert.AreEqualIgnoringCase("CAPA\r\n",
                                            ctx.Server.DequeueRequest());
        }

        ctx.Server.EnqueueResponse("+OK\r\n");
        ctx.Server.EnqueueResponse("+OK\r\n");

        var credential = new NetworkCredential(SessionContext.UserName,
                                               SessionContext.Password,
                                               ctx.Server.Host);

        Assert.IsTrue((bool)ctx.Session.Login(credential));

        StringAssert.AreEqualIgnoringCase(string.Format("USER {0}\r\n", credential.UserName),
                                          ctx.Server.DequeueRequest());
        StringAssert.AreEqualIgnoringCase(string.Format("PASS {0}\r\n", credential.Password),
                                          ctx.Server.DequeueRequest());

        Assert.AreEqual(PopSessionState.Transaction, ctx.Session.State);
        Assert.AreEqual(ctx.ExpectedAuthority, ctx.Session.Authority);

        ctx.Session.HandlesIncapableAsException = true;

        action(ctx);
      });
    }

    protected void Login(Action<PopSession, PopPseudoServer> action)
    {
      Login(null, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }

    protected void Login(PopCapability[] capabilities, Action<PopSession, PopPseudoServer> action)
    {
      Login(capabilities, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }
  }
}
