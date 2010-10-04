using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Client.Session {
  public abstract class ImapSessionTestsBase {
    [SetUp]
    public void Setup()
    {
      server = new ImapPseudoServer();
      server.Start();

      host = server.ServerEndPoint.Address.ToString();
      port = server.ServerEndPoint.Port;

      credential = new NetworkCredential(username, password, host);
      uri = new Uri(string.Format("{0}://{1}@{2}:{3}",
                                  ImapUri.UriSchemeImap,
                                  username,
                                  host,
                                  port));
    }

    [TearDown]
    public void TearDown()
    {
      server.EnqueueResponse("* BYE Logging out\r\n");
      server.Stop();
    }

    protected int port;
    protected string host;
    protected Uri uri;
    protected const string username = "imapuser";
    protected const string password = "password";
    protected NetworkCredential credential;

    protected ImapPseudoServer server;

    protected ImapSession Connect(params string[] capabilities)
    {
      var capa = (capabilities == null || capabilities.Length == 0)
        ? string.Empty
        : string.Format(" [CAPABILITY IMAP4REV1 {0}]", string.Join(" ", capabilities));

      server.EnqueueResponse(string.Format("* OK{0} ImapPseudoServer ready\r\n", capa));

      return new ImapSession(host, port);
    }

    protected ImapSession Authenticate(params string[] additionalCapabilities)
    {
      // greeting and login transaction
      var session = Connect();

      server.EnqueueResponse("0000 OK authenticated\r\n");

      session.Login(credential);

      Assert.AreEqual(new Uri(string.Format("imap://{0}@{1}:{2}/", username, host, port)), session.Authority);

      server.DequeueRequest();

      // CAPABILITY transaction
      server.EnqueueResponse(string.Format("* CAPABILITY {0} IMAP4rev1\r\n", string.Join(" ", additionalCapabilities)) +
                             "0001 OK CAPABILITY completed\r\n");

      session.Capability();

      server.DequeueRequest();

      return session;
    }

    protected ImapSession SelectMailbox(params string[] additionalCapabilities)
    {
      return SelectMailbox(15, additionalCapabilities);
    }

    protected ImapSession SelectMailbox(int existMessageCount, params string[] additionalCapabilities)
    {
      // greeting and login transaction
      var session = Connect();

      server.EnqueueResponse("0000 OK authenticated\r\n");

      session.Login(credential);

      var expectedAuthority = new Uri(string.Format("imap://{0};AUTH=LOGIN@{1}:{2}/", username, host, port));

      Assert.AreEqual(expectedAuthority, session.Authority);

      server.DequeueRequest();

      // CAPABILITY transaction
      if (additionalCapabilities.Length == 0)
        server.EnqueueResponse("* CAPABILITY IMAP4rev1\r\n" +
                               "0001 OK CAPABILITY completed\r\n");
      else
        server.EnqueueResponse(string.Format("* CAPABILITY IMAP4rev1 {0}\r\n", string.Join(" ", additionalCapabilities)) +
                               "0001 OK CAPABILITY completed\r\n");

      session.Capability();

      server.DequeueRequest();

      // LIST transaction
      server.EnqueueResponse("* LIST () \".\" \"INBOX\"\r\n" +
                             "0002 OK LIST completed\r\n");

      ImapMailbox[] mailboxes;

      session.List(out mailboxes);

      server.DequeueRequest();

      Assert.AreEqual(new Uri(expectedAuthority, "./INBOX"), mailboxes[0].Url);

      // SELECT transaction
      Assert.IsNull(session.SelectedMailbox);

      server.EnqueueResponse("* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft custom1 custom2)\r\n" +
                             "* OK [PERMANENTFLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft custom1 \\*)] Flags permitted\r\n" +
                             string.Format("* {0} EXISTS\r\n", existMessageCount) +
                             "* 2 RECENT\r\n" +
                             "* OK [UIDVALIDITY 1202674433] UIDs valid\r\n" +
                             "* OK [UIDNEXT 16]\r\n" +
                             "* OK [UNSEEN 13]\r\n" +
                             "0003 OK [READ-WRITE] SELECT completed\r\n");

      session.Select(mailboxes[0]);

      Assert.AreEqual(new Uri(expectedAuthority, "./INBOX;UIDVALIDITY=1202674433"), mailboxes[0].Url);
      Assert.AreEqual(mailboxes[0], session.SelectedMailbox);

      server.DequeueRequest();

      session.HandlesIncapableAsException = true;

      return session;
    }

    protected void CloseMailbox(ImapSession session)
    {
      CloseMailbox(session, "0005");
    }

    protected void CloseMailbox(ImapSession session, string tag)
    {
      // CLOSE transaction
      server.EnqueueResponse(tag + " OK CLOSE completed\r\n");

      session.Close();

      Assert.AreEqual(tag + " CLOSE\r\n",
                      server.DequeueRequest());

      session.Disconnect(false);
    }
  }
}
