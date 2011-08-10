using System;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsAuthenticatedStateTests : ImapSessionTestsBase {
    [Test]
    public void TestSelect()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        // SELECT transaction
        server.EnqueueResponse("* 172 EXISTS\r\n" +
                               "* 1 RECENT\r\n" +
                               "* OK [UNSEEN 12] Message 12 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS (\\Deleted \\Seen \\*)] Limited\r\n" +
                               "0002 OK [READ-WRITE] SELECT completed\r\n");

        Assert.IsTrue((bool)session.Select("INBOX"));

        Assert.AreEqual("0002 SELECT INBOX\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.IsNotNull(session.SelectedMailbox);

        Assert.AreEqual("INBOX", session.SelectedMailbox.Name);
        Assert.IsTrue(session.SelectedMailbox.IsInbox);
        Assert.IsTrue(session.SelectedMailbox.NameEquals("INBOX"));
        Assert.IsTrue(session.SelectedMailbox.NameEquals("inbox"));

        Assert.AreEqual(172L, session.SelectedMailbox.ExistsMessage);
        Assert.AreEqual(1L, session.SelectedMailbox.RecentMessage);
        Assert.AreEqual(12L, session.SelectedMailbox.FirstUnseen);
        Assert.AreEqual(0L, session.SelectedMailbox.UnseenMessage);
        Assert.AreEqual(3857529045L, session.SelectedMailbox.UidValidity);
        Assert.AreEqual(4392L, session.SelectedMailbox.UidNext);

        Assert.IsTrue(session.SelectedMailbox.UidPersistent);
        Assert.IsFalse(session.SelectedMailbox.ModificationSequences);

        Assert.IsNotNull(session.SelectedMailbox.Flags);
        Assert.AreEqual(0L, session.SelectedMailbox.Flags.Count);

        Assert.IsNotNull(session.SelectedMailbox.ApplicableFlags);
        Assert.AreEqual(5, session.SelectedMailbox.ApplicableFlags.Count);
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Draft));

        Assert.IsNotNull(session.SelectedMailbox.PermanentFlags);
        Assert.AreEqual(3, session.SelectedMailbox.PermanentFlags.Count);
        Assert.IsTrue(session.SelectedMailbox.PermanentFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.PermanentFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.PermanentFlags.Contains(ImapMessageFlag.AllowedCreateKeywords));

        Assert.IsFalse(session.SelectedMailbox.ReadOnly);

        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX;UIDVALIDITY=3857529045"),
                        session.SelectedMailbox.Url);
      });
    }

    [Test]
    public void TestSelectNo()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        // SELECT transaction
        server.EnqueueResponse("0002 NO Mailbox doesn't exist: foo\r\n");

        Assert.IsFalse((bool)session.Select("foo"));

        Assert.AreEqual("0002 SELECT foo\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      });
    }

    [Test]
    public void TestExamine()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        // EXAMINE transaction
        server.EnqueueResponse("* 17 EXISTS\r\n" +
                               "* 2 RECENT\r\n" +
                               "* OK [UNSEEN 8] Message 8 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS ()] No permanent flags permitted\r\n" +
                               "0002 OK [READ-ONLY] EXAMINE completed\r\n");

        Assert.IsTrue((bool)session.Examine("INBOX"));

        Assert.AreEqual("0002 EXAMINE INBOX\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Selected, session.State);
        Assert.IsNotNull(session.SelectedMailbox);

        Assert.AreEqual("INBOX", session.SelectedMailbox.Name);
        Assert.IsTrue(session.SelectedMailbox.IsInbox);
        Assert.IsTrue(session.SelectedMailbox.NameEquals("INBOX"));
        Assert.IsTrue(session.SelectedMailbox.NameEquals("inbox"));

        Assert.AreEqual(17L, session.SelectedMailbox.ExistsMessage);
        Assert.AreEqual(2L, session.SelectedMailbox.RecentMessage);
        Assert.AreEqual(8L, session.SelectedMailbox.FirstUnseen);
        Assert.AreEqual(0L, session.SelectedMailbox.UnseenMessage);
        Assert.AreEqual(3857529045L, session.SelectedMailbox.UidValidity);
        Assert.AreEqual(4392L, session.SelectedMailbox.UidNext);

        Assert.IsTrue(session.SelectedMailbox.UidPersistent);
        Assert.IsFalse(session.SelectedMailbox.ModificationSequences);

        Assert.IsNotNull(session.SelectedMailbox.Flags);
        Assert.AreEqual(0L, session.SelectedMailbox.Flags.Count);

        Assert.IsNotNull(session.SelectedMailbox.ApplicableFlags);
        Assert.AreEqual(5, session.SelectedMailbox.ApplicableFlags.Count);
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Flagged));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Contains(ImapMessageFlag.Draft));

        Assert.IsNotNull(session.SelectedMailbox.PermanentFlags);
        Assert.AreEqual(0, session.SelectedMailbox.PermanentFlags.Count);

        Assert.IsTrue(session.SelectedMailbox.ReadOnly);

        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX;UIDVALIDITY=3857529045"),
                        session.SelectedMailbox.Url);
      });
    }

    [Test]
    public void TestExamineNo()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        // EXAMINE transaction
        server.EnqueueResponse("0002 NO Mailbox doesn't exist: foo\r\n");

        Assert.IsFalse((bool)session.Examine("foo"));

        Assert.AreEqual("0002 EXAMINE foo\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      });
    }

    [Test]
    public void TestSelectNoReferralResponseCodeAsError()
    {
      SelectExamineNoRefferalResponseCode(false, false);
    }

    [Test]
    public void TestExamineNoRefferalResponseCodeAsError()
    {
      SelectExamineNoRefferalResponseCode(true, false);
    }

    [Test]
    public void TestSelectNoReferralResponseCodeAsException()
    {
      SelectExamineNoRefferalResponseCode(false, true);
    }

    [Test]
    public void TestExamineNoRefferalResponseCodeAsException()
    {
      SelectExamineNoRefferalResponseCode(true, true);
    }

    private void SelectExamineNoRefferalResponseCode(bool examine, bool asException)
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        session.HandlesReferralAsException = asException;

        // SELECT/EXAMINE transaction
        server.EnqueueResponse("0002 NO [REFERRAL IMAP://user;AUTH=*@SERVER2/REMOTE IMAP://user;AUTH=*@SERVER3/REMOTE] Remote mailbox. Try SERVER2 or SERVER3.\r\n");

        try {
          Assert.IsFalse((bool)(examine ? session.Examine("REMOTE") : session.Select("REMOTE")));

          if (asException)
            Assert.Fail("ImapMailboxReferralException not thrown");
        }
        catch (ImapMailboxReferralException ex) {
          Assert.IsNotNull(ex.Referrals);
          Assert.AreEqual(2, ex.Referrals.Length);
          Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER2/REMOTE"),
                          ex.Referrals[0]);
          Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER3/REMOTE"),
                          ex.Referrals[1]);
        }

        if (examine)
          Assert.AreEqual("0002 EXAMINE REMOTE\r\n",
                          server.DequeueRequest());
        else
          Assert.AreEqual("0002 SELECT REMOTE\r\n",
                          server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestSelectNoSelectMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" + 
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.List("*", out mailboxes));

        Assert.AreEqual("0002 LIST \"\" *\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[0].IsUnselectable);

        // SELECT transaction
        session.Select(mailboxes[0]);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestSelectNonExistentMailbox()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" Fruit/*\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].IsUnselectable);

        // SELECT transaction
        session.Select(mailboxes[1]);
      });
    }

    [Test]
    public void TestSelectUidNotSticky()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // SELECT transaction
        server.EnqueueResponse("* 1 EXISTS\r\n" +
                               "* 1 RECENT\r\n" +
                               "* OK [UNSEEN 1] Message 1 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] Validity session-only\r\n" +
                               "* OK [UIDNEXT 2] Predicted next UID\r\n" +
                               "* NO [UIDNOTSTICKY] Non-persistent UIDs\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS (\\Deleted \\Seen)] Limited\r\n" +
                               "0002 OK [READ-WRITE] SELECT completed\r\n");

        Assert.IsTrue((bool)session.Select("funny"));
  
        Assert.AreEqual("0002 SELECT funny\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.SelectedMailbox.UidPersistent);
        Assert.AreEqual(2, session.SelectedMailbox.UidNext);
        Assert.AreEqual(3857529045, session.SelectedMailbox.UidValidity);
        Assert.IsFalse(session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(new Uri(expectedAuthority, "./funny;UIDVALIDITY=3857529045"),
                        session.SelectedMailbox.Url);
      });
    }

    [Test]
    public void TestSelectHighestModSeq()
    {
      Authenticate(new[] {"CONDSTORE"}, delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // SELECT transaction
        server.EnqueueResponse("* 172 EXISTS\r\n" +
                               "* 1 RECENT\r\n" +
                               "* OK [UNSEEN 12] Message 12 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS (\\Deleted \\Seen \\*)] Limited\r\n" +
                               "* OK [HIGHESTMODSEQ 715194045007]\r\n" +
                               "0002 OK [READ-WRITE] SELECT completed\r\n");

        Assert.IsTrue((bool)session.Select("funny"));
  
        Assert.AreEqual("0002 SELECT funny\r\n",
                        server.DequeueRequest());

        Assert.IsTrue(session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(715194045007UL, session.SelectedMailbox.HighestModSeq);
        Assert.AreEqual(new Uri(expectedAuthority, "./funny;UIDVALIDITY=3857529045"),
                        session.SelectedMailbox.Url);
      });
    }

    [Test]
    public void TestSelectNoModSeq()
    {
      Authenticate(new[] {"CONDSTORE"}, delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // SELECT transaction
        server.EnqueueResponse("* 172 EXISTS\r\n" +
                               "* 1 RECENT\r\n" +
                               "* OK [UNSEEN 12] Message 12 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS (\\Deleted \\Seen \\*)] Limited\r\n" +
                               "* OK [NOMODSEQ] Sorry, this mailbox format doesn't support modsequences\r\n" +
                               "0002 OK [READ-WRITE] SELECT completed\r\n");

        Assert.IsTrue((bool)session.Select("funny"));
  
        Assert.AreEqual("0002 SELECT funny\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(0UL, session.SelectedMailbox.HighestModSeq);
        Assert.AreEqual(new Uri(expectedAuthority, "./funny;UIDVALIDITY=3857529045"),
                        session.SelectedMailbox.Url);
      });
    }

    [Test]
    public void TestSelectCondStore()
    {
      Authenticate(new[] {"CONDSTORE"}, delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // SELECT transaction
        server.EnqueueResponse("* 172 EXISTS\r\n" +
                               "* 1 RECENT\r\n" +
                               "* OK [UNSEEN 12] Message 12 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS (\\Deleted \\Seen \\*)] Limited\r\n" +
                               "* OK [HIGHESTMODSEQ 715194045007]\r\n" +
                               "0002 OK [READ-WRITE] SELECT completed, CONDSTORE is now enabled\r\n");

        Assert.IsTrue((bool)session.SelectCondStore("INBOX"));
  
        Assert.AreEqual("0002 SELECT INBOX (CONDSTORE)\r\n",
                        server.DequeueRequest());

        Assert.IsTrue((bool)session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(715194045007UL, session.SelectedMailbox.HighestModSeq);
        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX;UIDVALIDITY=3857529045"),
                        session.SelectedMailbox.Url);
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSelectCondStoreIncapable()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;

        // SELECT transaction
        session.SelectCondStore("INBOX");
      });
    }

    [Test]
    public void TestStatus()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" INBOX\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);

        // STATUS transaction
        server.EnqueueResponse("* STATUS \"INBOX\" (UIDNEXT 4 UNSEEN 2)\r\n" +
                               "0003 OK STATUS completed\r\n");

        Assert.IsTrue((bool)session.Status(mailboxes[0],
                                           ImapStatusDataItem.UidNext + ImapStatusDataItem.Unseen));

        Assert.AreEqual("0003 STATUS INBOX (UIDNEXT UNSEEN)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(4L, mailboxes[0].UidNext);
        Assert.AreEqual(2L, mailboxes[0].UnseenMessage);
      });
    }

    [Test]
    public void TestStatusUnmanagedMailbox1()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // STATUS transaction
        server.EnqueueResponse("* STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)\r\n" +
                               "0002 OK STATUS completed\r\n");

        ImapMailbox statusMailbox;

        Assert.IsTrue((bool)session.Status("blurdybloop",
                                           ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext,
                                           out statusMailbox));

        Assert.AreEqual("0002 STATUS blurdybloop (MESSAGES UIDNEXT)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(statusMailbox);
        Assert.IsTrue(statusMailbox.NameEquals("blurdybloop"));
        Assert.AreEqual(44292L, statusMailbox.UidNext);
        Assert.AreEqual(231L, statusMailbox.ExistsMessage);
        Assert.IsNotNull(statusMailbox.Flags);
        Assert.IsNotNull(statusMailbox.ApplicableFlags);
        Assert.IsNotNull(statusMailbox.PermanentFlags);
      });
    }

    [Test]
    public void TestStatusUnmanagedMailbox2()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // STATUS transaction
        server.EnqueueResponse("* STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)\r\n" +
                               "0002 OK STATUS completed\r\n");

        ImapStatusAttributeList statusAttributes;

        Assert.IsTrue((bool)session.Status("blurdybloop",
                                           ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext,
                                           out statusAttributes));

        Assert.AreEqual("0002 STATUS blurdybloop (MESSAGES UIDNEXT)\r\n",
                        server.DequeueRequest());

        Assert.IsNull(statusAttributes.HighestModSeq);
        Assert.IsNull(statusAttributes.Recent);
        Assert.IsNull(statusAttributes.UidValidity);
        Assert.IsNull(statusAttributes.Unseen);
        Assert.AreEqual(44292L, statusAttributes.UidNext);
        Assert.AreEqual(231L, statusAttributes.Messages);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStatusSelectedMailbox()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsNotNull(session.SelectedMailbox);

        session.Status(session.SelectedMailbox, ImapStatusDataItem.UidNext);

        return -1;
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStatusNoSelectMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(2, mailboxes.Length);
        Assert.AreEqual("foo", mailboxes[0].Name);
        Assert.AreEqual("foo/bar", mailboxes[1].Name);

        session.Status(mailboxes[0], ImapStatusDataItem.UidNext);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStatusNonExistentMailbox()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" Fruit/*\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].IsUnselectable);

        // STATUS transaction
        session.Status(mailboxes[1], ImapStatusDataItem.UidValidity);
      });
    }

    [Test]
    public void TestStatusHighestModSeq()
    {
      Authenticate(new[] {"CONDSTORE"}, delegate(ImapSession session, ImapPseudoServer server) {
        // STATUS transaction
        server.EnqueueResponse("* STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292 HIGHESTMODSEQ 7011231777)\r\n" +
                               "0002 OK STATUS completed\r\n");

        ImapStatusAttributeList statusAttributes;

        Assert.IsTrue((bool)session.Status("blurdybloop", ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext + ImapStatusDataItem.HighestModSeq, out statusAttributes));

        Assert.IsNull(statusAttributes.Recent);
        Assert.IsNull(statusAttributes.Unseen);
        Assert.IsNull(statusAttributes.UidValidity);

        Assert.IsNotNull(statusAttributes.Messages);
        Assert.AreEqual(231, statusAttributes.Messages);
        Assert.IsNotNull(statusAttributes.UidNext);
        Assert.AreEqual(44292, statusAttributes.UidNext);
        Assert.IsNotNull(statusAttributes.HighestModSeq);
        Assert.AreEqual(7011231777UL, statusAttributes.HighestModSeq);

        Assert.AreEqual("0002 STATUS blurdybloop (MESSAGES UIDNEXT HIGHESTMODSEQ)\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestCreate()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK CREATE completed\r\n");

        ImapMailbox createdMailbox;

        Assert.IsTrue((bool)session.Create("INBOX.日本語", out createdMailbox));

        Assert.AreEqual("0002 CREATE INBOX.&ZeVnLIqe-\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(createdMailbox);
        Assert.AreEqual("INBOX.日本語", createdMailbox.Name);
        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX.日本語"),
                        createdMailbox.Url);
        Assert.IsNotNull(createdMailbox.Flags);
        Assert.IsNotNull(createdMailbox.ApplicableFlags);
        Assert.IsNotNull(createdMailbox.PermanentFlags);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCreateInbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        ImapMailbox createdMailbox;

        session.Create("INBOX", out createdMailbox);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCreateExistentMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("INBOX.日本語", mailboxes[0].Name);

        ImapMailbox createdMailbox;

        session.Create(mailboxes[0].Name, out createdMailbox);
      });
    }

    [Test]
    public void TestCreateNonExistentMailbox()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" Fruit/*\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NonExistent));

        // CREATE transaction
        server.EnqueueResponse("0003 OK CREATE completed\r\n");

        ImapMailbox createdMailbox;

        Assert.IsTrue((bool)session.Create("Fruit/Peach", out createdMailbox));

        Assert.AreEqual("0003 CREATE Fruit/Peach\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(createdMailbox);
        Assert.AreSame(mailboxes[1], createdMailbox);
      });
    }

    [Test]
    public void TestCreateSpecialUse()
    {
      Authenticate(new[] {"CREATE-SPECIAL-USE"}, delegate(ImapSession session, ImapPseudoServer server) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK MySpecial created\r\n");

        ImapMailbox createdMailbox;

        Assert.IsTrue((bool)session.CreateSpecialUse("MySpecial", out createdMailbox, ImapMailboxFlag.Drafts, ImapMailboxFlag.Sent));

        Assert.AreEqual("0002 CREATE MySpecial (USE (\\Drafts \\Sent))\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(createdMailbox);
        Assert.AreEqual("MySpecial", createdMailbox.Name);
        Assert.AreEqual(2, createdMailbox.Flags.Count);
        Assert.IsTrue(createdMailbox.Flags.Contains(ImapMailboxFlag.Drafts));
        Assert.IsTrue(createdMailbox.Flags.Contains(ImapMailboxFlag.Sent));
      });
    }

    [Test]
    public void TestCreateSpecialUseNoUseAttr()
    {
      Authenticate(new[] {"CREATE-SPECIAL-USE"}, delegate(ImapSession session, ImapPseudoServer server) {
        // CREATE transaction
        server.EnqueueResponse("0002 NO [USEATTR] \\All not supported\r\n");

        ImapMailbox createdMailbox;

        Assert.IsFalse((bool)session.CreateSpecialUse("Everything", out createdMailbox, ImapMailboxFlag.All));

        Assert.AreEqual("0002 CREATE Everything (USE (\\All))\r\n",
                        server.DequeueRequest());

        Assert.IsNull(createdMailbox);
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCreateSpecialUseInvalidUseFlag()
    {
      Authenticate(new[] {"CREATE-SPECIAL-USE"}, delegate(ImapSession session, ImapPseudoServer server) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK MySpecial created\r\n");

        ImapMailbox createdMailbox;

        session.CreateSpecialUse("MySpecial", out createdMailbox, ImapMailboxFlag.NoInferiors);
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestCreateSpecialUseIncapable()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;

        // CREATE transaction
        session.CreateSpecialUse("Everything", ImapMailboxFlag.All);
      });
    }

    [Test]
    public void TestDelete()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);

        // DELETE transaction
        server.EnqueueResponse("0003 OK DELETE completed\r\n");

        Assert.IsTrue((bool)session.Delete(mailboxes[0]));

        Assert.AreEqual("0003 DELETE blurdybloop\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestDeleteUnmanagedMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // DELETE transaction
        server.EnqueueResponse("0002 OK DELETE completed\r\n");

        Assert.IsTrue((bool)session.Delete("INBOX.☺"));

        Assert.AreEqual("0002 DELETE INBOX.&Jjo-\r\n",
                        server.DequeueRequest());
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteInbox1()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \".\" \"INBOX\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("INBOX", mailboxes[0].Name);
        Assert.IsTrue(mailboxes[0].IsInbox);
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter);

        session.Delete(mailboxes[0]);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteInbox2()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.Delete("inbox");
      });
    }

    [Test]
    public void TestDeleteSelectedMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // SELECT transaction
        Assert.IsNull(session.SelectedMailbox);

        server.EnqueueResponse("* 17 EXISTS\r\n" +
                               "* 2 RECENT\r\n" +
                               "* OK [UNSEEN 8] Message 8 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS ()] No permanent flags permitted\r\n" +
                               "0002 OK [READ-WRITE] EXAMINE completed\r\n");

        Assert.IsTrue((bool)session.Select("blurdybloop"));

        Assert.AreEqual("0002 SELECT blurdybloop\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Selected, session.State);

        // DELETE transaction
        server.EnqueueResponse("0003 OK DELETE completed\r\n");

        Assert.IsNotNull(session.SelectedMailbox);
        Assert.IsTrue((bool)session.Delete(session.SelectedMailbox));

        Assert.AreEqual("0003 DELETE blurdybloop\r\n",
                        server.DequeueRequest());

        Assert.IsNull(session.SelectedMailbox);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      });
    }

    [Test]
    public void TestDeleteHierarchy()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(3, mailboxes.Length);
        Assert.AreEqual("foo", mailboxes[1].Name);
        Assert.AreEqual("foo/bar", mailboxes[2].Name);

        // DELETE transaction
        server.EnqueueResponse("0003 OK DELETE completed\r\n");

        Assert.IsTrue((bool)session.Delete(mailboxes[2]));

        Assert.AreEqual("0003 DELETE foo/bar\r\n",
                        server.DequeueRequest());

        server.EnqueueResponse("0004 OK DELETE completed\r\n");

        Assert.IsTrue((bool)session.Delete(mailboxes[1]));

        Assert.AreEqual("0004 DELETE foo\r\n",
                        server.DequeueRequest());
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteParentMailboxNoSelect()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(3, mailboxes.Length);
        Assert.AreEqual("foo", mailboxes[1].Name);

        session.Delete(mailboxes[1]);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteParentMailboxNonExistentHasChildren()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\NonExistent \\HasChildren) \"/\" music\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListReturnOptions.Children,
                                                 out mailboxes));

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.NonExistent));
        Assert.IsTrue(mailboxes[0].Flags.Contains(ImapMailboxFlag.HasChildren));

        session.Delete(mailboxes[0]);
      });
    }

    [Test]
    public void TestRename()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server, Uri expectedAuthority) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \".\" \"INBOX\"\r\n" +
                               "* LIST () \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(2, mailboxes.Length);
        Assert.AreEqual("INBOX", mailboxes[0].Name);
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter);
        Assert.AreEqual("INBOX.日本語", mailboxes[1].Name);
        Assert.AreEqual(".", mailboxes[1].HierarchyDelimiter);

        // REMANE transaction
        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        ImapMailbox renamingMailbox = mailboxes[1];
        ImapMailbox renamedMailbox;

        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX.日本語"),
                        renamingMailbox.Url);

        Assert.IsTrue((bool)session.Rename(renamingMailbox, "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0003 RENAME INBOX.&ZeVnLIqe- INBOX.&Jjo-\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsTrue(object.ReferenceEquals(renamingMailbox, renamedMailbox));
        Assert.AreEqual(new Uri(expectedAuthority, "./INBOX.☺"),
                        renamedMailbox.Url);
      });
    }

    [Test]
    public void TestRenameInbox1()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // REMANE transaction
        server.EnqueueResponse("0002 OK RENAME completed\r\n");

        ImapMailbox renamedMailbox;

        Assert.IsTrue((bool)session.Rename("INBOX", "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0002 RENAME INBOX INBOX.&Jjo-\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsFalse(renamedMailbox.IsInbox);
      });
    }

    [Test]
    public void TestRenameInbox2()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \".\" \"INBOX\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("INBOX", mailboxes[0].Name);
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter);

        // REMANE transaction
        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        ImapMailbox inbox = mailboxes[0];
        ImapMailbox renamedMailbox;

        Assert.IsTrue(inbox.IsInbox);
        Assert.IsTrue((bool)session.Rename(inbox, "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0003 RENAME INBOX INBOX.&Jjo-\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsTrue(object.ReferenceEquals(inbox, renamedMailbox));
        Assert.IsFalse(renamedMailbox.IsInbox);
      });
    }

    [Test]
    public void TestRenameUnmanagedMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // REMANE transaction
        server.EnqueueResponse("0002 OK RENAME completed\r\n");

        ImapMailbox renamedMailbox;

        Assert.IsTrue((bool)session.Rename("INBOX", "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0002 RENAME INBOX INBOX.&Jjo-\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsNotNull(renamedMailbox.Flags);
        Assert.IsNotNull(renamedMailbox.ApplicableFlags);
        Assert.IsNotNull(renamedMailbox.PermanentFlags);
      });
    }

    [Test]
    public void TestRenameHierarchy()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(3, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);
        Assert.AreEqual("foo", mailboxes[1].Name);
        Assert.AreEqual("foo/bar", mailboxes[2].Name);

        // REMANE transaction
        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        Assert.IsTrue((bool)session.Rename("foo", "zowie"));

        Assert.AreEqual("0003 RENAME foo zowie\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("blurdybloop", mailboxes[0].Name);
        Assert.AreEqual("zowie", mailboxes[1].Name);
        Assert.AreEqual("zowie/bar", mailboxes[2].Name);
      });
    }

    [Test]
    public void TestRenameSelectedMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // SELECT transaction
        Assert.IsNull(session.SelectedMailbox);

        server.EnqueueResponse("* 17 EXISTS\r\n" +
                               "* 2 RECENT\r\n" +
                               "* OK [UNSEEN 8] Message 8 is first unseen\r\n" +
                               "* OK [UIDVALIDITY 3857529045] UIDs valid\r\n" +
                               "* OK [UIDNEXT 4392] Predicted next UID\r\n" +
                               "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
                               "* OK [PERMANENTFLAGS ()] No permanent flags permitted\r\n" +
                               "0002 OK [READ-WRITE] EXAMINE completed\r\n");

        Assert.IsTrue((bool)session.Select("blurdybloop"));

        Assert.AreEqual("0002 SELECT blurdybloop\r\n",
                        server.DequeueRequest());

        // REMANE transaction
        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        Assert.IsNotNull(session.SelectedMailbox);

        var selectedMailbox = session.SelectedMailbox;

        Assert.IsTrue((bool)session.Rename(selectedMailbox, "foo"));

        Assert.AreEqual("0003 RENAME blurdybloop foo\r\n",
                        server.DequeueRequest());

        Assert.AreSame(selectedMailbox, session.SelectedMailbox);
        Assert.AreEqual("foo", selectedMailbox.Name);
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestRenameToExistMailbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(3, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);
        Assert.AreEqual("foo", mailboxes[1].Name);

        session.Rename("blurdybloop", "foo");
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRenameToSameName1()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);

        session.Rename(mailboxes[0], "blurdybloop");
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRenameToSameName2()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.Rename("blurdybloop", "blurdybloop");
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestRenameToInbox()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);

        session.Rename("blurdybloop", "INBOX");
      });
    }

    [Test]
    public void TestRenameToNonExistentMailbox()
    {
      Authenticate(new[] {"LIST-EXTENDED"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" Fruit/*\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NonExistent));

        // RENAME transaction
        ImapMailbox renamedMailbox;

        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        Assert.IsTrue((bool)session.Rename("Fruit/Unsubscribed", "Fruit/Peach", out renamedMailbox));

        Assert.AreEqual("0003 RENAME Fruit/Unsubscribed Fruit/Peach\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("Fruit/Peach", renamedMailbox.Name);
        Assert.AreNotSame(mailboxes[1], renamedMailbox);
      });
    }

    [Test]
    public void TestRenameNoReferralResponseCodeAsError()
    {
      RenameNoReferralResponseCode(false);
    }

    [Test]
    public void TestRenameNoRefferalResponseCodeAsException()
    {
      RenameNoReferralResponseCode(true);
    }

    private void RenameNoReferralResponseCode(bool asException)
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesReferralAsException = asException;

        server.EnqueueResponse("0002 NO [REFERRAL IMAP://user;AUTH=*@SERVER1/FOO IMAP://user;AUTH=*@SERVER2/BAR] Unable to rename mailbox across servers\r\n");

        try {
          Assert.IsFalse((bool)session.Rename("FOO", "BAR"));

          if (asException)
            Assert.Fail("ImapMailboxReferralException not thrown");
        }
        catch (ImapMailboxReferralException ex) {
          Assert.IsNotNull(ex.Referrals);
          Assert.AreEqual(2, ex.Referrals.Length);
          Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER1/FOO"),
                          ex.Referrals[0]);
          Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER2/BAR"),
                          ex.Referrals[1]);
        }

        Assert.AreEqual("0002 RENAME FOO BAR\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestSubscribe()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \".\" \"INBOX\"\r\n" +
                               "* LIST () \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        // SUBSCRIBE transaction
        server.EnqueueResponse("0003 OK SUBSCRIBE completed\r\n");

        Assert.IsTrue((bool)session.Subscribe(mailboxes[1]));

        Assert.AreEqual("0003 SUBSCRIBE INBOX.&ZeVnLIqe-\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestSubscribeCreated()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK CREATE completed\r\n");

        ImapMailbox created;

        Assert.IsTrue((bool)session.Create("Sent", out created));

        Assert.AreEqual("0002 CREATE Sent\r\n",
                        server.DequeueRequest());

        // SUBSCRIBE transaction
        server.EnqueueResponse("0003 OK SUBSCRIBE completed\r\n");

        Assert.IsTrue((bool)session.Subscribe(created));

        Assert.AreEqual("0003 SUBSCRIBE Sent\r\n",
                        server.DequeueRequest());
      });
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestSubscribeNonExistent()
    {
      Authenticate(new[] {"LIST-EXTEND"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\HasChildren) \".\" \"INBOX\"\r\n" +
                               "* LIST (\\HasChildren \\NonExistent) \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.ListExtended("%", out mailboxes);

        server.DequeueRequest();

        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NonExistent));

        session.Subscribe(mailboxes[1]);
      });
    }

    [Test]
    public void TestUnsubscribe()
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        // LSUB transaction
        server.EnqueueResponse("* LSUB () \".\" \"INBOX\"\r\n" +
                               "* LSUB () \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LSUB completed\r\n");

        ImapMailbox[] mailboxes;

        session.Lsub("INBOX*", out mailboxes);

        server.DequeueRequest();

        // UNSUBSCRIBE transaction
        server.EnqueueResponse("0003 OK UNSUBSCRIBE completed\r\n");

        Assert.IsTrue((bool)session.Unsubscribe(mailboxes[1]));

        Assert.AreEqual("0003 UNSUBSCRIBE INBOX.&ZeVnLIqe-\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestUnsubscribeNonExistent()
    {
      Authenticate(new[] {"LIST-EXTEND"}, delegate(ImapSession session, ImapPseudoServer server) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \".\" \"INBOX\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.ListExtended("*", ImapListSelectionOptions.Subscribed, out mailboxes);

        server.DequeueRequest();

        // UNSUBSCRIBE transaction
        server.EnqueueResponse("0003 OK UNSUBSCRIBE completed\r\n");

        Assert.IsTrue(mailboxes[1].Flags.Contains(ImapMailboxFlag.NonExistent));

        Assert.IsTrue((bool)session.Unsubscribe(mailboxes[1]));

        Assert.AreEqual("0003 UNSUBSCRIBE INBOX.&ZeVnLIqe-\r\n",
                        server.DequeueRequest());
      });
    }

    [Test]
    public void TestCreateNoReferralResponseCodeAsError()
    {
      RemoteMailboxNoReferralResponseCode(false, delegate(ImapSession session) {
        return session.Create("SHARED/FOO");
      });
    }

    [Test]
    public void TestCreateNoReferralResponseCodeAsException()
    {
      RemoteMailboxNoReferralResponseCode(true, delegate(ImapSession session) {
        return session.Create("SHARED/FOO");
      });
    }

    [Test]
    public void TestDeleteNoReferralResponseCodeAsError()
    {
      RemoteMailboxNoReferralResponseCode(false, delegate(ImapSession session) {
        return session.Delete("SHARED/FOO");
      });
    }

    [Test]
    public void TestDeleteNoReferralResponseCodeAsException()
    {
      RemoteMailboxNoReferralResponseCode(true, delegate(ImapSession session) {
        return session.Delete("SHARED/FOO");
      });
    }

    [Test]
    public void TestSubscribeNoReferralResponseCodeAsError()
    {
      RemoteMailboxNoReferralResponseCode(false, delegate(ImapSession session) {
        return session.Subscribe("SHARED/FOO");
      });
    }

    [Test]
    public void TestSubscribeNoReferralResponseCodeAsException()
    {
      RemoteMailboxNoReferralResponseCode(true, delegate(ImapSession session) {
        return session.Subscribe("SHARED/FOO");
      });
    }

    [Test]
    public void TestUnsubscribeNoReferralResponseCodeAsError()
    {
      RemoteMailboxNoReferralResponseCode(false, delegate(ImapSession session) {
        return session.Unsubscribe("SHARED/FOO");
      });
    }

    [Test]
    public void TestUnsubscribeNoReferralResponseCodeAsException()
    {
      RemoteMailboxNoReferralResponseCode(true, delegate(ImapSession session) {
        return session.Unsubscribe("SHARED/FOO");
      });
    }

    [Test]
    public void TestStatusNoReferralResponseCodeAsError()
    {
      RemoteMailboxNoReferralResponseCode(false, delegate(ImapSession session) {
        return session.Status("SHARED/FOO", ImapStatusDataItem.StandardAll);
      });
    }

    [Test]
    public void TestStatusNoReferralResponseCodeAsException()
    {
      RemoteMailboxNoReferralResponseCode(true, delegate(ImapSession session) {
        return session.Status("SHARED/FOO", ImapStatusDataItem.StandardAll);
      });
    }

    private void RemoteMailboxNoReferralResponseCode(bool asException,
                                                     Func<ImapSession, ImapCommandResult> operation)
    {
      Authenticate(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesReferralAsException = asException;

        server.EnqueueResponse("0002 NO [REFERRAL IMAP://user;AUTH=*@SERVER2/SHARED/FOO] Remote mailbox. Try SERVER2.\r\n");

        try {
          Assert.IsFalse((bool)operation(session));

          if (asException)
            Assert.Fail("ImapMailboxReferralException not thrown");
        }
        catch (ImapMailboxReferralException ex) {
          Assert.IsNotNull(ex.Referrals);
          Assert.AreEqual(1, ex.Referrals.Length);
          Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER2/SHARED/FOO"),
                          ex.Referrals[0]);
        }
      });
    }

    [Test]
    public void TestEnable()
    {
      Authenticate(new[] {"ENABLE", "X-GOOD-IDEA"}, delegate(ImapSession session, ImapPseudoServer server) {
        Assert.IsTrue(session.ServerCapabilities.Contains(ImapCapability.Enable));

        // ENABLE transaction
        server.EnqueueResponse("* ENABLED X-GOOD-IDEA\r\n" +
                               "0002 OK ENABLE completed\r\n");

        ImapCapabilitySet enabledCapas;

        Assert.IsTrue((bool)session.Enable(out enabledCapas, "CONDSTORE", "X-GOOD-IDEA"));

        Assert.AreEqual("0002 ENABLE CONDSTORE X-GOOD-IDEA\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, enabledCapas.Count);
        Assert.IsTrue(enabledCapas.Contains("X-GOOD-IDEA"));
      });
    }
  }
}
