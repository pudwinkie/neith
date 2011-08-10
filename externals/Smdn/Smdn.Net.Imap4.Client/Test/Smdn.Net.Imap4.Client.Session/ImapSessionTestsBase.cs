using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Client.Session {
  public abstract class ImapSessionTestsBase {
    private class SessionContext : IDisposable {
      public ImapPseudoServer Server {
        get; private set;
      }

      public ImapSession Session {
        get; private set;
      }

      public Uri ExpectedAuthority {
        get; private set;
      }

      public SessionContext()
      {
        Server = new ImapPseudoServer();
        Server.Start();
      }

      public void Dispose()
      {
        if (Server != null) {
          Server.EnqueueResponse("* BYE Logging out\r\n");
          Server.Dispose();
          Server = null;
        }

        if (Session != null) {
          (Session as IDisposable).Dispose();
          Session = null;
        }
      }

      internal void SetSession(ImapSession session)
      {
        this.Session = session;
      }

      internal void SetExpectedAuthority(Uri authority)
      {
        this.ExpectedAuthority = authority;
      }

      public const string UserName = "imapuser";
      public const string Password = "password";
    }

    protected ImapPseudoServer CreateServer()
    {
      var server = new ImapPseudoServer();

      server.Start();

      return server;
    }

    private void Connect(string[] capabilities, Action<SessionContext> action)
    {
      using (var ctx = new SessionContext()) {
        var capa = (capabilities == null || capabilities.Length == 0)
          ? string.Empty
          : string.Format(" [CAPABILITY IMAP4REV1 {0}]", string.Join(" ", capabilities));

        ctx.Server.EnqueueResponse(string.Format("* OK{0} ImapPseudoServer ready\r\n", capa));

        ctx.SetSession(new ImapSession(ctx.Server.Host, ctx.Server.Port));

        action(ctx);
      }
    }

    protected void Connect(Action<ImapSession> action)
    {
      Connect((string[])null, delegate(SessionContext ctx) {
        action(ctx.Session);
      });
    }

    protected void Connect(string[] capabilities, Action<ImapSession> action)
    {
      Connect(capabilities, delegate(SessionContext ctx) {
        action(ctx.Session);
      });
    }

    protected void Connect(Action<ImapSession, ImapPseudoServer> action)
    {
      Connect((string[])null, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }

    protected void Connect(string[] capabilities, Action<ImapSession, ImapPseudoServer> action)
    {
      Connect(capabilities, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }

    protected void Authenticate(Action<ImapSession, ImapPseudoServer> action)
    {
      Authenticate((string[])null, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }

    protected void Authenticate(string[] additionalCapabilities, Action<ImapSession, ImapPseudoServer> action)
    {
      Authenticate(additionalCapabilities, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server);
      });
    }

    protected void Authenticate(Action<ImapSession, ImapPseudoServer, Uri> action)
    {
      Authenticate((string[])null, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server, ctx.ExpectedAuthority);
      });
    }

    protected void Authenticate(string[] additionalCapabilities, Action<ImapSession, ImapPseudoServer, Uri> action)
    {
      Authenticate(additionalCapabilities, delegate(SessionContext ctx) {
        action(ctx.Session, ctx.Server, ctx.ExpectedAuthority);
      });
    }

    private void Authenticate(string[] additionalCapabilities, Action<SessionContext> action)
    {
      Connect((string[])null, delegate(SessionContext ctx) {
        ctx.Server.EnqueueResponse("0000 OK authenticated\r\n");

        ctx.Session.Login(new NetworkCredential(SessionContext.UserName, SessionContext.Password));

        ctx.SetExpectedAuthority(new Uri(string.Format("{0}://{1}@{2}/",
                                                       ImapUri.UriSchemeImap,
                                                       SessionContext.UserName,
                                                       ctx.Server.HostPort)));

        Assert.AreEqual(ctx.ExpectedAuthority, ctx.Session.Authority);

        ctx.Server.DequeueRequest();

        // CAPABILITY transaction
        if (additionalCapabilities == null)
          additionalCapabilities = new string[0];

        ctx.Server.EnqueueResponse(string.Format("* CAPABILITY {0} IMAP4rev1\r\n", string.Join(" ", additionalCapabilities)) +
                               "0001 OK CAPABILITY completed\r\n");

        ctx.Session.Capability();

        ctx.Server.DequeueRequest();

        action(ctx);
      });
    }

    private const int defaultExistMessageCount = 15;

    protected void SelectMailbox(Func<ImapSession, ImapPseudoServer, int> func)
    {
      SelectMailbox(defaultExistMessageCount, (string[])null, delegate(SessionContext ctx) {
        return func(ctx.Session, ctx.Server);
      });
    }

    protected void SelectMailbox(int existMessageCount, Func<ImapSession, ImapPseudoServer, int> func)
    {
      SelectMailbox(existMessageCount, (string[])null, delegate(SessionContext ctx) {
        return func(ctx.Session, ctx.Server);
      });
    }

    protected void SelectMailbox(string[] additionalCapabilities, Func<ImapSession, ImapPseudoServer, int> func)
    {
      SelectMailbox(defaultExistMessageCount, additionalCapabilities, delegate(SessionContext ctx) {
        return func(ctx.Session, ctx.Server);
      });
    }

    protected void SelectMailbox(int existMessageCount, string[] additionalCapabilities, Func<ImapSession, ImapPseudoServer, int> func)
    {
      SelectMailbox(existMessageCount, additionalCapabilities, delegate(SessionContext ctx) {
        return func(ctx.Session, ctx.Server);
      });
    }

    protected void SelectMailbox(Func<ImapSession, ImapPseudoServer, Uri, int> func)
    {
      SelectMailbox(defaultExistMessageCount, (string[])null, delegate(SessionContext ctx) {
        return func(ctx.Session, ctx.Server, ctx.ExpectedAuthority);
      });
    }

    protected void SelectMailbox(string[] additionalCapabilities, Func<ImapSession, ImapPseudoServer, Uri, int> func)
    {
      SelectMailbox(defaultExistMessageCount, additionalCapabilities, delegate(SessionContext ctx) {
        return func(ctx.Session, ctx.Server, ctx.ExpectedAuthority);
      });
    }

    private void SelectMailbox(int existMessageCount, string[] additionalCapabilities, Func<SessionContext, int> func)
    {
      Authenticate(additionalCapabilities, delegate(SessionContext ctx) {
        // LIST transaction
        ctx.Server.EnqueueResponse("* LIST () \".\" \"INBOX\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        ctx.Session.List(out mailboxes);

        ctx.Server.DequeueRequest();

        Assert.AreEqual(new Uri(ctx.ExpectedAuthority, "./INBOX"), mailboxes[0].Url);

        // SELECT transaction
        Assert.IsNull(ctx.Session.SelectedMailbox);

        ctx.Server.EnqueueResponse("* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft custom1 custom2)\r\n" +
                                   "* OK [PERMANENTFLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft custom1 \\*)] Flags permitted\r\n" +
                                   string.Format("* {0} EXISTS\r\n", existMessageCount) +
                                   "* 2 RECENT\r\n" +
                                   "* OK [UIDVALIDITY 1202674433] UIDs valid\r\n" +
                                   "* OK [UIDNEXT 16]\r\n" +
                                   "* OK [UNSEEN 13]\r\n" +
                                   "0003 OK [READ-WRITE] SELECT completed\r\n");

        ctx.Session.Select(mailboxes[0]);

        Assert.AreEqual(new Uri(ctx.ExpectedAuthority, "./INBOX;UIDVALIDITY=1202674433"), mailboxes[0].Url);
        Assert.AreEqual(mailboxes[0], ctx.Session.SelectedMailbox);

        ctx.Server.DequeueRequest();

        ctx.Session.HandlesIncapableAsException = true;

        var sentCommandCount = func(ctx);

        if (0 <= sentCommandCount) {
          var tag = (4 + sentCommandCount).ToString("D4");

          // CLOSE transaction
          ctx.Server.EnqueueResponse(tag + " OK CLOSE completed\r\n");

          Assert.IsTrue((bool)ctx.Session.Close());

          Assert.AreEqual(tag + " CLOSE\r\n",
                          ctx.Server.DequeueRequest());

          ctx.Session.Disconnect(false);
        }
      });
    }
  }
}
