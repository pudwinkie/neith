using System;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsListTests : ImapSessionTestsBase {
    [Test]
    public void TestListRoot()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsFalse(session.HierarchyDelimiters.ContainsKey(""));
        Assert.IsFalse(session.HierarchyDelimiters.ContainsKey("#news.comp.mail.misc"));

        // LIST transaction 1
        server.EnqueueResponse("* LIST (\\Noselect) \"/\" \"\"\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailboxList root;

        Assert.IsTrue((bool)session.ListRoot(out root));

        Assert.AreEqual("0002 LIST \"\" \"\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(root);
        Assert.IsEmpty(root.Name);
        Assert.AreEqual("/", root.HierarchyDelimiter);
        Assert.AreEqual(1, root.NameAttributes.Count);
        Assert.IsTrue(root.NameAttributes.Contains(ImapMailboxFlag.NoSelect));

        Assert.IsTrue(session.HierarchyDelimiters.ContainsKey(""));
        Assert.AreEqual("/", session.HierarchyDelimiters[""]);

        // LIST transaction 2
        server.EnqueueResponse("* LIST (\\Noselect) \".\" #news.\r\n" +
                               "0003 OK LIST Completed\r\n");

        Assert.IsTrue((bool)session.ListRoot("#news.comp.mail.misc", out root));

        Assert.AreEqual("0003 LIST #news.comp.mail.misc \"\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(root);
        Assert.AreEqual("#news.", root.Name);
        Assert.AreEqual(".", root.HierarchyDelimiter);
        Assert.AreEqual(1, root.NameAttributes.Count);
        Assert.IsTrue(root.NameAttributes.Contains(ImapMailboxFlag.NoSelect));

        Assert.IsTrue(session.HierarchyDelimiters.ContainsKey("#news.comp.mail.misc"));
        Assert.AreEqual(".", session.HierarchyDelimiters["#news.comp.mail.misc"]);

        try {
          session.HierarchyDelimiters.Add("#refname", "/");
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      });
    }

    [Test]
    public void TestList()
    {
      ListRList(false);
    }

    [Test]
    public void TestRList()
    {
      ListRList(true);
    }

    private void ListRList(bool mailboxReferral)
    {
      Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0],
                   delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" +
                               "* LIST (\\Noinferiors) \"\" Draft\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RList(out mailboxes));

          Assert.AreEqual("0002 RLIST \"\" *\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.List(out mailboxes));

          Assert.AreEqual("0002 LIST \"\" *\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(4, mailboxes.Length);

        Assert.AreEqual("blurdybloop", mailboxes[0].Name, "mailbox list #1 name");
        Assert.IsTrue(mailboxes[0].NameEquals("blurdybloop"), "mailbox list #1 name comparison");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual(string.Empty, mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("blurdybloop", mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox list #1 flag count");
        Assert.IsFalse(mailboxes[0].IsUnselectable, "mailbox list #1 IsUnselectable");

        Assert.AreEqual("foo", mailboxes[1].Name, "mailbox list #2 name");
        Assert.IsTrue(mailboxes[1].NameEquals("foo"), "mailbox list #2 name comparison");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox list #2 delimiter");
        Assert.AreEqual(string.Empty, mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("foo", mailboxes[1].LeafName, "mailbox list #2 leaf name");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox list #2 flag count");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NoSelect), "mailbox list #2 flags");
        Assert.IsTrue(mailboxes[1].IsUnselectable, "mailbox list #2 IsUnselectable");

        Assert.AreEqual("foo/bar", mailboxes[2].Name, "mailbox list #3 name");
        Assert.IsTrue(mailboxes[2].NameEquals("foo/bar"), "mailbox list #3 name comparison");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox list #3 delimiter");
        Assert.AreEqual("foo", mailboxes[2].SuperiorName, "mailbox list #3 superior name");
        Assert.AreEqual("bar", mailboxes[2].LeafName, "mailbox list #3 leaf name");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox list #3 flag count");

        Assert.AreEqual("Draft", mailboxes[3].Name, "mailbox list #4 name");
        Assert.IsTrue(mailboxes[3].NameEquals("Draft"), "mailbox list #4 name comparison");
        Assert.AreEqual("", mailboxes[3].HierarchyDelimiter, "mailbox list #4 delimiter");
        Assert.AreEqual("", mailboxes[3].SuperiorName, "mailbox list #4 superior name");
        Assert.AreEqual("Draft", mailboxes[3].LeafName, "mailbox list #4 leaf name");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox list #4 flag count");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.NoInferiors));
        Assert.IsTrue(mailboxes[3].IsNonHierarchical, "mailbox list #4 IsNonHierarchical");
      });
    }

    [Test]
    public void TestListNonEmptyReferenceNameAndEmptyMailboxName()
    {
      ListRListNonEmptyReferenceNameAndEmptyMailboxName(false);
    }

    [Test]
    public void TestRListNonEmptyReferenceNameAndEmptyMailboxName()
    {
      ListRListNonEmptyReferenceNameAndEmptyMailboxName(true);
    }

    private void ListRListNonEmptyReferenceNameAndEmptyMailboxName(bool mailboxReferral)
    {
      Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0],
                   delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \".\" #news.\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RList("#news.comp.mail.misc", string.Empty, out mailboxes));

          Assert.AreEqual("0002 RLIST #news.comp.mail.misc \"\"\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.List("#news.comp.mail.misc", string.Empty, out mailboxes));

          Assert.AreEqual("0002 LIST #news.comp.mail.misc \"\"\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(1, mailboxes.Length);

        Assert.AreEqual("#news.", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual("#news", mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual(string.Empty, mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox list #1 flag count");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NoSelect), "mailbox list #1 flags");
      });
    }

    [Test]
    public void TestListNonEmptyReferenceNameAndWildcardMailboxName()
    {
      ListRListNonEmptyReferenceNameAndWildcardMailboxName(false);
    }

    [Test]
    public void TestRListNonEmptyReferenceNameAndWildcardMailboxName()
    {
      ListRListNonEmptyReferenceNameAndWildcardMailboxName(true);
    }

    private void ListRListNonEmptyReferenceNameAndWildcardMailboxName(bool mailboxReferral)
    {
      Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0],
                   delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \"/\" ~/Mail/foo\r\n" +
                               "* LIST () \"/\" ~/Mail/meetings\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RList("~/Mail/", "%", out mailboxes));

          Assert.AreEqual("0002 RLIST ~/Mail/ %\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.List("~/Mail/", "%", out mailboxes));

          Assert.AreEqual("0002 LIST ~/Mail/ %\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.AreEqual("~/Mail/foo", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual("~/Mail", mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("foo", mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox list #1 flag count");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NoSelect), "mailbox list #1 flags");

        Assert.AreEqual("~/Mail/meetings", mailboxes[1].Name, "mailbox list #2 name");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox list #2 delimiter");
        Assert.AreEqual("~/Mail", mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("meetings", mailboxes[1].LeafName, "mailbox list #2 leaf name");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox list #2 flag count");
      });
    }

    [Test]
    public void TestLsub()
    {
      LsubRLsub(false);
    }

    [Test]
    public void TestRLsub()
    {
      LsubRLsub(true);
    }

    public void LsubRLsub(bool mailboxReferral)
    {
      Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0],
                   delegate(ImapSession session, ImapPseudoServer server) {
        // LSUB transaction
        server.EnqueueResponse("* LSUB () \"/\" INBOX\r\n" +
                               "* LSUB () \"/\" INBOX/Sent\r\n" +
                               "* LSUB () \"/\" INBOX/Drafts\r\n" +
                               "* LSUB () \"/\" INBOX/Junk\r\n" +
                                "0002 OK LSUB Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RLsub(out mailboxes));

          Assert.AreEqual("0002 RLSUB \"\" *\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.Lsub(out mailboxes));

          Assert.AreEqual("0002 LSUB \"\" *\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(4, mailboxes.Length);

        Assert.AreEqual("INBOX", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual(string.Empty, mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("INBOX", mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox list #1 flag count");

        Assert.AreEqual("INBOX/Sent", mailboxes[1].Name, "mailbox list #2 name");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox list #2 delimiter");
        Assert.AreEqual("INBOX", mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("Sent", mailboxes[1].LeafName, "mailbox list #2 leaf name");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox list #2 flag count");

        Assert.AreEqual("INBOX/Drafts", mailboxes[2].Name, "mailbox list #3 name");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox list #3 delimiter");
        Assert.AreEqual("INBOX", mailboxes[2].SuperiorName, "mailbox list #3 superior name");
        Assert.AreEqual("Drafts", mailboxes[2].LeafName, "mailbox list #3 leaf name");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox list #3 flag count");

        Assert.AreEqual("INBOX/Junk", mailboxes[3].Name, "mailbox list #4 name");
        Assert.AreEqual("/", mailboxes[3].HierarchyDelimiter, "mailbox list #4 delimiter");
        Assert.AreEqual("INBOX", mailboxes[3].SuperiorName, "mailbox list #4 superior name");
        Assert.AreEqual("Junk", mailboxes[3].LeafName, "mailbox list #4 leaf name");
        Assert.AreEqual(0, mailboxes[3].Flags.Count, "mailbox list #4 flag count");
      });
    }

    [Test]
    public void TestLsubEmptyReferenceNameAndNonEmptyMailboxName()
    {
      LsubRLsubEmptyReferenceNameAndNonEmptyMailboxName(false);
    }

    [Test]
    public void TestRLsubEmptyReferenceNameAndNonEmptyMailboxName()
    {
      LsubRLsubEmptyReferenceNameAndNonEmptyMailboxName(true);
    }

    private void LsubRLsubEmptyReferenceNameAndNonEmptyMailboxName(bool mailboxReferral)
    {
      Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0],
                   delegate(ImapSession session, ImapPseudoServer server) {
        // LSUB transaction
        server.EnqueueResponse("* LSUB () \"/\" INBOX/Sent\r\n" +
                               "* LSUB () \"/\" INBOX/Drafts\r\n" +
                               "* LSUB () \"/\" INBOX/Junk\r\n" +
                                "0002 OK LSUB Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RLsub("INBOX/*", out mailboxes));

          Assert.AreEqual("0002 RLSUB \"\" INBOX/*\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.Lsub("INBOX/*", out mailboxes));

          Assert.AreEqual("0002 LSUB \"\" INBOX/*\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(3, mailboxes.Length);

        Assert.AreEqual("INBOX/Sent", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual("INBOX", mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("Sent", mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox list #1 flag count");

        Assert.AreEqual("INBOX/Drafts", mailboxes[1].Name, "mailbox list #2 name");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox list #2 delimiter");
        Assert.AreEqual("INBOX", mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("Drafts", mailboxes[1].LeafName, "mailbox list #2 leaf name");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox list #2 flag count");

        Assert.AreEqual("INBOX/Junk", mailboxes[2].Name, "mailbox list #3 name");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox list #3 delimiter");
        Assert.AreEqual("INBOX", mailboxes[2].SuperiorName, "mailbox list #3 superior name");
        Assert.AreEqual("Junk", mailboxes[2].LeafName, "mailbox list #3 leaf name");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox list #3 flag count");
      });
    }

    [Test]
    public void TestLsubNonEmptyReferenceNameAndWildcardMailboxName()
    {
      LsubRLsubNonEmptyReferenceNameAndWildcardMailboxName(false);
    }

    [Test]
    public void TestRLsubNonEmptyReferenceNameAndWildcardMailboxName()
    {
      LsubRLsubNonEmptyReferenceNameAndWildcardMailboxName(true);
    }

    private void LsubRLsubNonEmptyReferenceNameAndWildcardMailboxName(bool mailboxReferral)
    {
      Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0],
                   delegate(ImapSession session, ImapPseudoServer server) {
        // LSUB transaction
        server.EnqueueResponse("* LSUB () \".\" #news.comp.mail.mime\r\n" +
                               "* LSUB () \".\" #news.comp.mail.misc\r\n" +
                                "0002 OK LSUB Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RLsub("#news.", "comp.mail.*", out mailboxes));

          Assert.AreEqual("0002 RLSUB #news. comp.mail.*\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.Lsub("#news.", "comp.mail.*", out mailboxes));

          Assert.AreEqual("0002 LSUB #news. comp.mail.*\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.AreEqual("#news.comp.mail.mime", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual("#news.comp.mail", mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("mime", mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox list #1 flag count");

        Assert.AreEqual("#news.comp.mail.misc", mailboxes[1].Name, "mailbox list #2 name");
        Assert.AreEqual(".", mailboxes[1].HierarchyDelimiter, "mailbox list #2 delimiter");
        Assert.AreEqual("#news.comp.mail", mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("misc", mailboxes[1].LeafName, "mailbox list #2 leaf name");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox list #2 flag count");
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestXListIncapable()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.GimapXlist));

        session.HandlesIncapableAsException = true;

        ImapMailbox[] mailboxes;

        session.XList(out mailboxes);
      });
    }

    [Test]
    public void TestXList()
    {
      Authenticate(new[] {"XLIST"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* XLIST (\\HasNoChildren \\Inbox) \"/\" \"&U9dP4TDIMOwwpA-\"\r\n" +
                               "* XLIST (\\Noselect \\HasChildren) \"/\" \"[Gmail]\"\r\n" +
                               "* XLIST (\\HasNoChildren \\AllMail) \"/\" \"[Gmail]/&MFkweTBmMG4w4TD8MOs-\"\r\n" +
                               "* XLIST (\\HasNoChildren \\Trash) \"/\" \"[Gmail]/&MLQw33ux-\"\r\n" +
                               "* XLIST (\\HasNoChildren \\Starred) \"/\" \"[Gmail]/&MLkwvzD8TtgwTQ-\"\r\n" +
                               "* XLIST (\\HasNoChildren \\Drafts) \"/\" \"[Gmail]/&Tgtm+DBN-\"\r\n" +
                               "* XLIST (\\HasNoChildren \\Spam) \"/\" \"[Gmail]/&j,dg0TDhMPww6w-\"\r\n" +
                               "* XLIST (\\HasNoChildren \\Sent) \"/\" \"[Gmail]/&kAFP4W4IMH8w4TD8MOs-\"\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.XList(out mailboxes));

        Assert.AreEqual("0002 XLIST \"\" *\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(8, mailboxes.Length);

        Assert.AreEqual("受信トレイ", mailboxes[0].Name, "mailbox list #1 name");
        Assert.IsTrue(mailboxes[0].NameEquals("受信トレイ"), "mailbox list #1 name comparison");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.GimapInbox), "mailbox list #1 flags");
        Assert.AreEqual(string.Empty, mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("受信トレイ", mailboxes[0].LeafName, "mailbox list #1 leaf name");

        Assert.AreEqual("[Gmail]", mailboxes[1].Name, "mailbox list #2 name");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NoSelect), "mailbox list #2 flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.HasChildren), "mailbox list #2 flags");
        Assert.IsFalse(mailboxes[1].IsNonHierarchical, "mailbox list #2 IsNonHierarchical");
        Assert.AreEqual(string.Empty, mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("[Gmail]", mailboxes[1].LeafName, "mailbox list #2 leaf name");

        Assert.AreEqual("[Gmail]/すべてのメール", mailboxes[2].Name, "mailbox list #3 name");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.GimapAllMail), "mailbox list #3 flags");
        Assert.AreEqual("[Gmail]", mailboxes[2].SuperiorName, "mailbox list #3 superior name");
        Assert.AreEqual("すべてのメール", mailboxes[2].LeafName, "mailbox list #3 leaf name");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.HasNoChildren), "mailbox list #3 flags");
        Assert.IsTrue(mailboxes[2].IsNonHierarchical, "mailbox list #3 IsNonHierarchical");

        Assert.AreEqual("[Gmail]/ゴミ箱", mailboxes[3].Name, "mailbox list #4 name");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.Trash), "mailbox list #4 flags");
        Assert.AreEqual("[Gmail]", mailboxes[3].SuperiorName, "mailbox list #4 superior name");
        Assert.AreEqual("ゴミ箱", mailboxes[3].LeafName, "mailbox list #4 leaf name");

        Assert.AreEqual("[Gmail]/スター付き", mailboxes[4].Name, "mailbox list #5 name");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.GimapStarred), "mailbox list #5 flags");

        Assert.AreEqual("[Gmail]/下書き", mailboxes[5].Name, "mailbox list #6 name");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.Drafts), "mailbox list #6 flags");

        Assert.AreEqual("[Gmail]/迷惑メール", mailboxes[6].Name, "mailbox list #7 name");
        Assert.IsTrue(mailboxes[6].Flags.Contains(ImapMailboxFlag.GimapSpam), "mailbox list #7 flags");

        Assert.AreEqual("[Gmail]/送信済みメール", mailboxes[7].Name, "mailbox list #8 name");
        Assert.IsTrue(mailboxes[7].Flags.Contains(ImapMailboxFlag.Sent), "mailbox list #8 flags");
      });
    }

    [Test]
    public void TestListNonAsciiMailboxName()
    {
      ListNonAsciiMailboxName("LIST");
    }

    [Test]
    public void TestLsubNonAsciiMailboxName()
    {
      ListNonAsciiMailboxName("LSUB");
    }

    [Test]
    public void TestRListNonAsciiMailboxName()
    {
      ListNonAsciiMailboxName("RLIST");
    }

    [Test]
    public void TestRLsubNonAsciiMailboxName()
    {
      ListNonAsciiMailboxName("RLSUB");
    }

    [Test]
    public void TestXListNonAsciiMailboxName()
    {
      ListNonAsciiMailboxName("XLIST");
    }

    private void ListNonAsciiMailboxName(string command)
    {
      Authenticate(new[] {"XLIST", "MAILBOX-REFERRAL"}, delegate(ImapSession session, ImapPseudoServer server) {
        // list transaction
        server.EnqueueResponse("0002 OK completed\r\n");

        ImapMailbox[] mailboxes = null;
        ImapCommandResult result = null;

        switch (command) {
          case "LIST":  result = session.List("INBOX.メールボックス.*", out mailboxes); break;
          case "LSUB":  result = session.Lsub("INBOX.メールボックス.*", out mailboxes); break;
          case "RLIST": result = session.RList("INBOX.メールボックス.*", out mailboxes); break;
          case "RLSUB": result = session.RLsub("INBOX.メールボックス.*", out mailboxes); break;
          case "XLIST": result = session.XList("INBOX.メールボックス.*", out mailboxes); break;
          default:
            return;
        }

        Assert.IsTrue((bool)result);

        Assert.AreEqual(string.Format("0002 {0} \"\" INBOX.&MOEw,DDrMNwwwzCvMLk-.*\r\n", command),
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(0, mailboxes.Length);
      });
    }

    [Test]
    public void TestListExtendedMailboxPatternNonAscii()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \".\" \"INBOX.&MOEw,DDrMNwwwzCvMLk-\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \".\" \"INBOX.&MOEw,DDrMNwwwzCvMLk-.1\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("INBOX.メールボックス.*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" INBOX.&MOEw,DDrMNwwwzCvMLk-.*\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.AreEqual("INBOX.メールボックス", mailboxes[0].Name, "mailbox #1");
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter, "mailbox #1 delimiter");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #1 flags");
        Assert.IsFalse(mailboxes[0].IsUnselectable, "mailbox #1 IsUnselectable");

        Assert.AreEqual("INBOX.メールボックス.1", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(".", mailboxes[1].HierarchyDelimiter, "mailbox #2 delimiter");
        Assert.AreEqual(2, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #2 flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NonExistent), "mailbox #2 flags");
        Assert.IsTrue(mailboxes[1].IsUnselectable, "mailbox #2 IsUnselectable");
      });
    }

    [Test]
    public void TestListExtendedSelectSubscribed()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Marked \\NoInferiors \\Subscribed) \"/\" \"inbox\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"Vegetable\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"Vegetable/Broccoli\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" *\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(5, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.IsTrue(mailboxes[0].NameEquals("inbox"), "mailbox list #1 name comparison");
        Assert.IsTrue(mailboxes[0].NameEquals("INBOX"), "mailbox list #1 name comparison (ignore case)");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox #1 delimiter");
        Assert.AreEqual(3, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #1 flags");

        Assert.AreEqual("Fruit/Banana", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox #2 delimiter");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #2 flags");
        Assert.IsFalse(mailboxes[1].IsUnselectable, "mailbox #2 IsUnselectable");

        Assert.AreEqual("Fruit/Peach", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox #3 delimiter");
        Assert.AreEqual(2, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #3 flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.NonExistent), "mailbox #3 flags");
        Assert.IsTrue(mailboxes[2].IsUnselectable, "mailbox #3 IsUnselectable");

        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual("/", mailboxes[3].HierarchyDelimiter, "mailbox #4 delimiter");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #4 flags");

        Assert.AreEqual("Vegetable/Broccoli", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual("/", mailboxes[4].HierarchyDelimiter, "mailbox #5 delimiter");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #5 flags");
      });
    }

    [Test]
    public void TestListExtendedReturnChildren()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Marked \\NoInferiors) \"/\" \"inbox\"\r\n" +
                               "* LIST (\\HasChildren) \"/\" \"Fruit\"\r\n" +
                               "* LIST (\\HasNoChildren) \"/\" \"Tofu\"\r\n" +
                               "* LIST (\\HasChildren) \"/\" \"Vegetable\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListReturnOptions.Children, out mailboxes));

        //Assert.AreEqual("0002 LIST () \"\" \"%\" RETURN (CHILDREN)\r\n",
        Assert.AreEqual("0002 LIST \"\" % RETURN (CHILDREN)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(4, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox #1 delimiter");
        Assert.AreEqual(2, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");

        Assert.AreEqual("Fruit", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox #2 delimiter");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.HasChildren), "mailbox #2 flags");

        Assert.AreEqual("Tofu", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox #3 delimiter");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.HasNoChildren), "mailbox #3 flags");

        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual("/", mailboxes[3].HierarchyDelimiter, "mailbox #4 delimiter");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.HasChildren), "mailbox #4 flags");
      });
    }

    [Test]
    public void TestListExtendedSelectRemoteReturnChildren()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Marked \\NoInferiors) \"/\" \"inbox\"\r\n" +
                               "* LIST (\\HasChildren) \"/\" \"Fruit\"\r\n" +
                               "* LIST (\\HasNoChildren) \"/\" \"Tofu\"\r\n" +
                               "* LIST (\\HasChildren) \"/\" \"Vegetable\"\r\n" +
                               "* LIST (\\Remote) \"/\" \"Bread\"\r\n" +
                               "* LIST (\\HasChildren \\Remote) \"/\" \"Meat\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListSelectionOptions.Remote,
                                                 ImapListReturnOptions.Children,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (REMOTE) \"\" % RETURN (CHILDREN)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(6, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(2, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");

        Assert.AreEqual("Fruit", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.HasChildren), "mailbox #2 flags");

        Assert.AreEqual("Tofu", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.HasNoChildren), "mailbox #3 flags");

        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.HasChildren), "mailbox #4 flags");

        Assert.AreEqual("Bread", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.Remote), "mailbox #5 flags");

        Assert.AreEqual("Meat", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(2, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.HasChildren), "mailbox #6 flags");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.Remote), "mailbox #6 flags");
      });
    }

    [Test]
    public void TestListExtendedSelectRemoteSubscribed()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Marked \\NoInferiors \\Subscribed) \"/\" \"inbox\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"Vegetable\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"Vegetable/Broccoli\"\r\n" +
                               "* LIST (\\Remote \\Subscribed) \"/\" \"Bread\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("*",
                                                 ImapListSelectionOptions.Remote + ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (REMOTE SUBSCRIBED) \"\" *\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(6, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(3, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #1 flags");

        Assert.AreEqual("Fruit/Banana", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #2 flags");

        Assert.AreEqual("Fruit/Peach", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(2, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #3 flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.NonExistent), "mailbox #3 flags");
        
        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #4 flags");

        Assert.AreEqual("Vegetable/Broccoli", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #5 flags");

        Assert.AreEqual("Bread", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(2, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.Remote), "mailbox #6 flags");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #6 flags");
      });
    }

    [Test]
    public void TestListExtendedSelectRecursiveMatchSubscribed()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" \"foo2\" (\"CHILDINFO\" (\"SUBSCRIBED\"))\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"foo2/bar2\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"baz2/bar2\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"baz2/bar22\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"baz2/bar222\"\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"eps2\" (\"CHILDINFO\" (\"SUBSCRIBED\"))\r\n" +
                               "* LIST (\\Subscribed) \"/\" \"qux2/bar2\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("*2",
                                                 ImapListSelectionOptions.RecursiveMatch + ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (RECURSIVEMATCH SUBSCRIBED) \"\" *2\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(7, mailboxes.Length);

        Assert.AreEqual("foo2", mailboxes[0].Name, "mailbox #1");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        // TODO: child info

        Assert.AreEqual("foo2/bar2", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #2 flags");

        Assert.AreEqual("baz2/bar2", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #3 flags");

        Assert.AreEqual("baz2/bar22", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #4 flags");

        Assert.AreEqual("baz2/bar222", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #5 flags");

        Assert.AreEqual("eps2", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(1, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #6 flags");
        // TODO: child info

        Assert.AreEqual("qux2/bar2", mailboxes[6].Name, "mailbox #6");
        Assert.AreEqual(1, mailboxes[6].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[6].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #6 flags");
      });
    }

    [Test]
    public void TestListExtendedMailboxPatternMultiple()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" \"INBOX\"\r\n" +
                               "* LIST (\\NoInferiors) \"/\" \"Drafts\"\r\n" +
                               "* LIST () \"/\" \"Sent/March2004\"\r\n" +
                               "* LIST (\\Marked) \"/\" \"Sent/December2003\"\r\n" +
                               "* LIST () \"/\" \"Sent/August2004\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended(out mailboxes,
                                                 "INBOX",
                                                 "Drafts",
                                                 "Sent/%"));

        Assert.AreEqual("0002 LIST \"\" (INBOX Drafts Sent/%)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(5, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox #1 count of flags");

        Assert.AreEqual("Drafts", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NoInferiors), "mailbox #2 flags");

        Assert.AreEqual("Sent/March2004", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox #3 count of flags");

        Assert.AreEqual("Sent/December2003", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #4 flags");

        Assert.AreEqual("Sent/August2004", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(0, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestListExtendedReturnStatusIncapable()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.ListStatus));

        session.HandlesIncapableAsException = true;

        ImapMailbox[] mailboxes;

        session.ListExtended("%", ImapListReturnOptions.StatusDataItems(ImapStatusDataItem.Messages), out mailboxes);
      });
    }

    [Test]
    public void TestListExtendedReturnStatus()
    {
      Authenticate(new[] {"LIST-EXTENDED", "LIST-STATUS"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListStatus));

        // LIST transaction
        server.EnqueueResponse("* LIST () \".\"  \"INBOX\"\r\n" +
                               "* STATUS \"INBOX\" (MESSAGES 17 UNSEEN 16)\r\n" +
                               "* LIST () \".\" \"foo\"\r\n" +
                               "* STATUS \"foo\" (MESSAGES 30 UNSEEN 29)\r\n" +
                               "* LIST (\\NoSelect) \".\" \"bar\"\r\n" +
                               "0002 OK List completed.\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListReturnOptions.StatusDataItems(ImapStatusDataItem.Messages +
                                                                                       ImapStatusDataItem.Unseen),
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST \"\" % RETURN (STATUS (MESSAGES UNSEEN))\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(3, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.AreEqual(17L, mailboxes[0].ExistsMessage, "mailbox #1 exists messages");
        Assert.AreEqual(16L, mailboxes[0].UnseenMessage, "mailbox #1 unseen messages");

        Assert.AreEqual("foo", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.AreEqual(30L, mailboxes[1].ExistsMessage, "mailbox #2 exists messages");
        Assert.AreEqual(29L, mailboxes[1].UnseenMessage, "mailbox #2 unseen messages");

        Assert.AreEqual("bar", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.NoSelect), "mailbox #3 flags");
        Assert.AreEqual(0L, mailboxes[2].ExistsMessage, "mailbox #3 exists messages");
        Assert.AreEqual(0L, mailboxes[2].UnseenMessage, "mailbox #3 unseen messages");
      });
    }

    [Test]
    public void TestListExtendedReturnStatusSelectSubscribedRecursiveMatch()
    {
      Authenticate(new[] {"LIST-EXTENDED", "LIST-STATUS"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListStatus));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \".\"  \"INBOX\"\r\n" +
                               "* STATUS \"INBOX\" (MESSAGES 17)\r\n" +
                               "* LIST () \".\" \"foo\" (CHILDINFO (\"SUBSCRIBED\"))\r\n" +
                               "0002 OK List completed.\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListSelectionOptions.Subscribed + ImapListSelectionOptions.RecursiveMatch,
                                                 ImapListReturnOptions.StatusDataItems(ImapStatusDataItem.Messages),
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED RECURSIVEMATCH) \"\" % RETURN (STATUS (MESSAGES))\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Subscribed), "mailbox #1 flags");
        Assert.AreEqual(17L, mailboxes[0].ExistsMessage, "mailbox #1 exists messages");

        Assert.AreEqual("foo", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        // child info
      });
    }

    [Test]
    public void TestListExtendedReturnSpecialUse()
    {
      Authenticate(new[] {"LIST-EXTENDED", "SPECIAL-USE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.SpecialUse));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Marked) \"/\" Inbox\r\n" +
                               "* LIST () \"/\" ToDo\r\n" +
                               "* LIST () \"/\" Projects\r\n" +
                               "* LIST (\\Sent) \"/\" SentMail\r\n" +
                               "* LIST (\\Marked \\Drafts) \"/\" MyDrafts\r\n" +
                               "* LIST (\\Trash) \"/\" Trash\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListReturnOptions.SpecialUse,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST \"\" % RETURN (SPECIAL-USE)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(6, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #1 \\Marked");

        Assert.AreEqual("ToDo", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox #2 count of flags");

        Assert.AreEqual("Projects", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox #3 count of flags");

        Assert.AreEqual("SentMail", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Contains(ImapMailboxFlag.Sent), "mailbox #4 \\Sent");

        Assert.AreEqual("MyDrafts", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(2, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #5 \\Marked");
        Assert.IsTrue(mailboxes[4].Flags.Contains(ImapMailboxFlag.Drafts), "mailbox #5 \\Drafts");

        Assert.AreEqual("Trash", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(1, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Contains(ImapMailboxFlag.Trash), "mailbox #6 \\Trash");
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestListExtendedReturnSpecialUseIncapable()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.SpecialUse));

        session.HandlesIncapableAsException = true;

        ImapMailbox[] mailboxes;

        session.ListExtended("%",
                             ImapListReturnOptions.SpecialUse,
                             out mailboxes);
      });
    }

    [Test]
    public void TestListExtendedSelectSpecialUse()
    {
      Authenticate(new[] {"LIST-EXTENDED", "SPECIAL-USE"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.SpecialUse));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Sent) \"/\" SentMail\r\n" +
                               "* LIST (\\Marked \\Drafts) \"/\" MyDrafts\r\n" +
                               "* LIST (\\Trash) \"/\" Trash\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("*",
                                                 ImapListSelectionOptions.SpecialUse,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SPECIAL-USE) \"\" *\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(3, mailboxes.Length);

        Assert.AreEqual("SentMail", mailboxes[0].Name, "mailbox #1");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.Sent), "mailbox #1 \\Sent");

        Assert.AreEqual("MyDrafts", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(2, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.Marked), "mailbox #1 \\Marked");
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.Drafts), "mailbox #1 \\Drafts");

        Assert.AreEqual("Trash", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Contains(ImapMailboxFlag.Trash), "mailbox #1 \\Trash");
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestListExtendedSelectSpecialUseIncapable()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.ListExtended));
        Assert.IsFalse(session.ServerCapabilities.Contains(ImapCapability.SpecialUse));

        session.HandlesIncapableAsException = true;

        ImapMailbox[] mailboxes;

        session.ListExtended("*",
                             ImapListSelectionOptions.SpecialUse,
                             out mailboxes);
      });
    }
  }
}

