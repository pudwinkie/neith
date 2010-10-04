using System;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsAuthenticatedStateTests : ImapSessionTestsBase {
    [Test]
    public void TestSelect()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0002 SELECT \"INBOX\"\r\n",
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
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Answered));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Draft));

        Assert.IsNotNull(session.SelectedMailbox.PermanentFlags);
        Assert.AreEqual(3, session.SelectedMailbox.PermanentFlags.Count);
        Assert.IsTrue(session.SelectedMailbox.PermanentFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.PermanentFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.PermanentFlags.Has(ImapMessageFlag.AllowedCreateKeywords));

        Assert.IsFalse(session.SelectedMailbox.ReadOnly);

        Assert.AreEqual(new Uri(uri, "./INBOX;UIDVALIDITY=3857529045"), session.SelectedMailbox.Url);
      }
    }

    [Test]
    public void TestSelectNo()
    {
      using (var session = Authenticate()) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        // SELECT transaction
        server.EnqueueResponse("0002 NO Mailbox doesn't exist: foo\r\n");

        Assert.IsFalse((bool)session.Select("foo"));

        Assert.AreEqual("0002 SELECT \"foo\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      }
    }

    [Test]
    public void TestExamine()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0002 EXAMINE \"INBOX\"\r\n",
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
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Answered));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Flagged));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(session.SelectedMailbox.ApplicableFlags.Has(ImapMessageFlag.Draft));

        Assert.IsNotNull(session.SelectedMailbox.PermanentFlags);
        Assert.AreEqual(0, session.SelectedMailbox.PermanentFlags.Count);

        Assert.IsTrue(session.SelectedMailbox.ReadOnly);

        Assert.AreEqual(new Uri(uri, "./INBOX;UIDVALIDITY=3857529045"), session.SelectedMailbox.Url);
      }
    }

    [Test]
    public void TestExamineNo()
    {
      using (var session = Authenticate()) {
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);

        // EXAMINE transaction
        server.EnqueueResponse("0002 NO Mailbox doesn't exist: foo\r\n");

        Assert.IsFalse((bool)session.Examine("foo"));

        Assert.AreEqual("0002 EXAMINE \"foo\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      }
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
      using (var session = Authenticate()) {
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
          Assert.AreEqual("0002 EXAMINE \"REMOTE\"\r\n",
                          server.DequeueRequest());
        else
          Assert.AreEqual("0002 SELECT \"REMOTE\"\r\n",
                          server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
        Assert.IsNull(session.SelectedMailbox);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestSelectNoSelectMailbox()
    {
      using (var session = Authenticate()) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \"/\" foo\r\n" +
                               "* LIST () \"/\" foo/bar\r\n" + 
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.List("*", out mailboxes));

        Assert.AreEqual("0002 LIST \"\" \"*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[0].IsUnselectable);

        // SELECT transaction
        session.Select(mailboxes[0]);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestSelectNonExistentMailbox()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" \"Fruit/*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].IsUnselectable);

        // SELECT transaction
        session.Select(mailboxes[1]);
      }
    }

    [Test]
    public void TestSelectUidNotSticky()
    {
      using (var session = Authenticate()) {
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
  
        Assert.AreEqual("0002 SELECT \"funny\"\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.SelectedMailbox.UidPersistent);
        Assert.AreEqual(2, session.SelectedMailbox.UidNext);
        Assert.AreEqual(3857529045, session.SelectedMailbox.UidValidity);
        Assert.IsFalse(session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(new Uri(uri, "./funny;UIDVALIDITY=3857529045"), session.SelectedMailbox.Url);
      }
    }

    [Test]
    public void TestSelectHighestModSeq()
    {
      using (var session = Authenticate("CONDSTORE")) {
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
  
        Assert.AreEqual("0002 SELECT \"funny\"\r\n",
                        server.DequeueRequest());

        Assert.IsTrue(session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(715194045007UL, session.SelectedMailbox.HighestModSeq);
        Assert.AreEqual(new Uri(uri, "./funny;UIDVALIDITY=3857529045"), session.SelectedMailbox.Url);
      }
    }

    [Test]
    public void TestSelectNoModSeq()
    {
      using (var session = Authenticate("CONDSTORE")) {
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
  
        Assert.AreEqual("0002 SELECT \"funny\"\r\n",
                        server.DequeueRequest());

        Assert.IsFalse(session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(0UL, session.SelectedMailbox.HighestModSeq);
        Assert.AreEqual(new Uri(uri, "./funny;UIDVALIDITY=3857529045"), session.SelectedMailbox.Url);
      }
    }

    [Test]
    public void TestSelectCondStore()
    {
      using (var session = Authenticate("CONDSTORE")) {
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
  
        Assert.AreEqual("0002 SELECT \"INBOX\" (CONDSTORE)\r\n",
                        server.DequeueRequest());

        Assert.IsTrue((bool)session.SelectedMailbox.ModificationSequences);
        Assert.AreEqual(715194045007UL, session.SelectedMailbox.HighestModSeq);
        Assert.AreEqual(new Uri(uri, "./INBOX;UIDVALIDITY=3857529045"), session.SelectedMailbox.Url);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestSelectCondStoreIncapable()
    {
      using (var session = Authenticate()) {
        session.HandlesIncapableAsException = true;

        // SELECT transaction
        session.SelectCondStore("INBOX");
      }
    }

    [Test]
    public void TestListRoot()
    {
      using (var session = Authenticate()) {
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
        Assert.IsTrue(root.NameAttributes.Has(ImapMailboxFlag.NoSelect));

        Assert.IsTrue(session.HierarchyDelimiters.ContainsKey(""));
        Assert.AreEqual("/", session.HierarchyDelimiters[""]);

        // LIST transaction 2
        server.EnqueueResponse("* LIST (\\Noselect) \".\" #news.\r\n" +
                               "0003 OK LIST Completed\r\n");

        Assert.IsTrue((bool)session.ListRoot("#news.comp.mail.misc", out root));

        Assert.AreEqual("0003 LIST \"#news.comp.mail.misc\" \"\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(root);
        Assert.AreEqual("#news.", root.Name);
        Assert.AreEqual(".", root.HierarchyDelimiter);
        Assert.AreEqual(1, root.NameAttributes.Count);
        Assert.IsTrue(root.NameAttributes.Has(ImapMailboxFlag.NoSelect));

        Assert.IsTrue(session.HierarchyDelimiters.ContainsKey("#news.comp.mail.misc"));
        Assert.AreEqual(".", session.HierarchyDelimiters["#news.comp.mail.misc"]);

        try {
          session.HierarchyDelimiters.Add("#refname", "/");
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }
      }
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
      using (var session = Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0])) {
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
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NoSelect), "mailbox list #2 flags");
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
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.NoInferiors));
        Assert.IsTrue(mailboxes[3].IsNonHierarchical, "mailbox list #4 IsNonHierarchical");
      }
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
      using (var session = Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0])) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \".\" #news.\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RList("#news.comp.mail.misc", string.Empty, out mailboxes));

          Assert.AreEqual("0002 RLIST \"#news.comp.mail.misc\" \"\"\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.List("#news.comp.mail.misc", string.Empty, out mailboxes));

          Assert.AreEqual("0002 LIST \"#news.comp.mail.misc\" \"\"\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(1, mailboxes.Length);

        Assert.AreEqual("#news.", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual("#news", mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual(string.Empty, mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox list #1 flag count");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NoSelect), "mailbox list #1 flags");
      }
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
      using (var session = Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0])) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Noselect) \"/\" ~/Mail/foo\r\n" +
                               "* LIST () \"/\" ~/Mail/meetings\r\n" +
                               "0002 OK LIST Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RList("~/Mail/", "%", out mailboxes));

          Assert.AreEqual("0002 RLIST \"~/Mail/\" \"%\"\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.List("~/Mail/", "%", out mailboxes));

          Assert.AreEqual("0002 LIST \"~/Mail/\" \"%\"\r\n",
                          server.DequeueRequest());
        }

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.AreEqual("~/Mail/foo", mailboxes[0].Name, "mailbox list #1 name");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox list #1 delimiter");
        Assert.AreEqual("~/Mail", mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("foo", mailboxes[0].LeafName, "mailbox list #1 leaf name");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox list #1 flag count");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NoSelect), "mailbox list #1 flags");

        Assert.AreEqual("~/Mail/meetings", mailboxes[1].Name, "mailbox list #2 name");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox list #2 delimiter");
        Assert.AreEqual("~/Mail", mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("meetings", mailboxes[1].LeafName, "mailbox list #2 leaf name");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox list #2 flag count");
      }
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
      using (var session = Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0])) {
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
      }
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
      using (var session = Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0])) {
        // LSUB transaction
        server.EnqueueResponse("* LSUB () \"/\" INBOX/Sent\r\n" +
                               "* LSUB () \"/\" INBOX/Drafts\r\n" +
                               "* LSUB () \"/\" INBOX/Junk\r\n" +
                                "0002 OK LSUB Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RLsub("INBOX/*", out mailboxes));

          Assert.AreEqual("0002 RLSUB \"\" \"INBOX/*\"\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.Lsub("INBOX/*", out mailboxes));

          Assert.AreEqual("0002 LSUB \"\" \"INBOX/*\"\r\n",
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
      }
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
      using (var session = Authenticate(mailboxReferral ? new[] {"MAILBOX-REFERRAL"} : new string[0])) {
        // LSUB transaction
        server.EnqueueResponse("* LSUB () \".\" #news.comp.mail.mime\r\n" +
                               "* LSUB () \".\" #news.comp.mail.misc\r\n" +
                                "0002 OK LSUB Completed\r\n");

        ImapMailbox[] mailboxes;

        if (mailboxReferral) {
          Assert.IsTrue((bool)session.RLsub("#news.", "comp.mail.*", out mailboxes));

          Assert.AreEqual("0002 RLSUB \"#news.\" \"comp.mail.*\"\r\n",
                          server.DequeueRequest());
        }
        else {
          Assert.IsTrue((bool)session.Lsub("#news.", "comp.mail.*", out mailboxes));

          Assert.AreEqual("0002 LSUB \"#news.\" \"comp.mail.*\"\r\n",
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
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestXListIncapable()
    {
      using (var session = Authenticate()) {
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.GimapXlist));

        session.HandlesIncapableAsException = true;

        ImapMailbox[] mailboxes;

        session.XList(out mailboxes);
      }
    }

    [Test]
    public void TestXList()
    {
      using (var session = Authenticate("XLIST")) {
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
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Inbox), "mailbox list #1 flags");
        Assert.AreEqual(string.Empty, mailboxes[0].SuperiorName, "mailbox list #1 superior name");
        Assert.AreEqual("受信トレイ", mailboxes[0].LeafName, "mailbox list #1 leaf name");

        Assert.AreEqual("[Gmail]", mailboxes[1].Name, "mailbox list #2 name");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NoSelect), "mailbox list #2 flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.HasChildren), "mailbox list #2 flags");
        Assert.IsFalse(mailboxes[1].IsNonHierarchical, "mailbox list #2 IsNonHierarchical");
        Assert.AreEqual(string.Empty, mailboxes[1].SuperiorName, "mailbox list #2 superior name");
        Assert.AreEqual("[Gmail]", mailboxes[1].LeafName, "mailbox list #2 leaf name");

        Assert.AreEqual("[Gmail]/すべてのメール", mailboxes[2].Name, "mailbox list #3 name");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.GimapAllMail), "mailbox list #3 flags");
        Assert.AreEqual("[Gmail]", mailboxes[2].SuperiorName, "mailbox list #3 superior name");
        Assert.AreEqual("すべてのメール", mailboxes[2].LeafName, "mailbox list #3 leaf name");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.HasNoChildren), "mailbox list #3 flags");
        Assert.IsTrue(mailboxes[2].IsNonHierarchical, "mailbox list #3 IsNonHierarchical");

        Assert.AreEqual("[Gmail]/ゴミ箱", mailboxes[3].Name, "mailbox list #4 name");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.Trash), "mailbox list #4 flags");
        Assert.AreEqual("[Gmail]", mailboxes[3].SuperiorName, "mailbox list #4 superior name");
        Assert.AreEqual("ゴミ箱", mailboxes[3].LeafName, "mailbox list #4 leaf name");

        Assert.AreEqual("[Gmail]/スター付き", mailboxes[4].Name, "mailbox list #5 name");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.GimapStarred), "mailbox list #5 flags");

        Assert.AreEqual("[Gmail]/下書き", mailboxes[5].Name, "mailbox list #6 name");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.Drafts), "mailbox list #6 flags");

        Assert.AreEqual("[Gmail]/迷惑メール", mailboxes[6].Name, "mailbox list #7 name");
        Assert.IsTrue(mailboxes[6].Flags.Has(ImapMailboxFlag.GimapSpam), "mailbox list #7 flags");

        Assert.AreEqual("[Gmail]/送信済みメール", mailboxes[7].Name, "mailbox list #8 name");
        Assert.IsTrue(mailboxes[7].Flags.Has(ImapMailboxFlag.Sent), "mailbox list #8 flags");
      }
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
      using (var session = Authenticate("XLIST", "MAILBOX-REFERRAL")) {
        // list transaction
        server.EnqueueResponse("0002 OK completed\r\n");

        ImapMailbox[] mailboxes;
        ImapCommandResult result;

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

        Assert.AreEqual(string.Format("0002 {0} \"\" \"INBOX.&MOEw,DDrMNwwwzCvMLk-.*\"\r\n", command),
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(0, mailboxes.Length);
      }
    }

    [Test]
    public void TestListExtendedMailboxPatternNonAscii()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \".\" \"INBOX.&MOEw,DDrMNwwwzCvMLk-\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \".\" \"INBOX.&MOEw,DDrMNwwwzCvMLk-.1\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("INBOX.メールボックス.*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" \"INBOX.&MOEw,DDrMNwwwzCvMLk-.*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.AreEqual("INBOX.メールボックス", mailboxes[0].Name, "mailbox #1");
        Assert.AreEqual(".", mailboxes[0].HierarchyDelimiter, "mailbox #1 delimiter");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #1 flags");
        Assert.IsFalse(mailboxes[0].IsUnselectable, "mailbox #1 IsUnselectable");

        Assert.AreEqual("INBOX.メールボックス.1", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(".", mailboxes[1].HierarchyDelimiter, "mailbox #2 delimiter");
        Assert.AreEqual(2, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #2 flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NonExistent), "mailbox #2 flags");
        Assert.IsTrue(mailboxes[1].IsUnselectable, "mailbox #2 IsUnselectable");
      }
    }

    [Test]
    public void TestListExtendedSelectSubscribed()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" \"*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(5, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.IsTrue(mailboxes[0].NameEquals("inbox"), "mailbox list #1 name comparison");
        Assert.IsTrue(mailboxes[0].NameEquals("INBOX"), "mailbox list #1 name comparison (ignore case)");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox #1 delimiter");
        Assert.AreEqual(3, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #1 flags");

        Assert.AreEqual("Fruit/Banana", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox #2 delimiter");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #2 flags");
        Assert.IsFalse(mailboxes[1].IsUnselectable, "mailbox #2 IsUnselectable");

        Assert.AreEqual("Fruit/Peach", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox #3 delimiter");
        Assert.AreEqual(2, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #3 flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.NonExistent), "mailbox #3 flags");
        Assert.IsTrue(mailboxes[2].IsUnselectable, "mailbox #3 IsUnselectable");

        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual("/", mailboxes[3].HierarchyDelimiter, "mailbox #4 delimiter");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #4 flags");

        Assert.AreEqual("Vegetable/Broccoli", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual("/", mailboxes[4].HierarchyDelimiter, "mailbox #5 delimiter");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #5 flags");
      }
    }

    [Test]
    public void TestListExtendedReturnChildren()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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
        Assert.AreEqual("0002 LIST \"\" \"%\" RETURN (CHILDREN)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(4, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual("/", mailboxes[0].HierarchyDelimiter, "mailbox #1 delimiter");
        Assert.AreEqual(2, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");

        Assert.AreEqual("Fruit", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual("/", mailboxes[1].HierarchyDelimiter, "mailbox #2 delimiter");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.HasChildren), "mailbox #2 flags");

        Assert.AreEqual("Tofu", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual("/", mailboxes[2].HierarchyDelimiter, "mailbox #3 delimiter");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.HasNoChildren), "mailbox #3 flags");

        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual("/", mailboxes[3].HierarchyDelimiter, "mailbox #4 delimiter");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.HasChildren), "mailbox #4 flags");
      }
    }

    [Test]
    public void TestListExtendedSelectRemoteReturnChildren()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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

        Assert.AreEqual("0002 LIST (REMOTE) \"\" \"%\" RETURN (CHILDREN)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(6, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(2, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");

        Assert.AreEqual("Fruit", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.HasChildren), "mailbox #2 flags");

        Assert.AreEqual("Tofu", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.HasNoChildren), "mailbox #3 flags");

        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.HasChildren), "mailbox #4 flags");

        Assert.AreEqual("Bread", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.Remote), "mailbox #5 flags");

        Assert.AreEqual("Meat", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(2, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.HasChildren), "mailbox #6 flags");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.Remote), "mailbox #6 flags");
      }
    }

    [Test]
    public void TestListExtendedSelectRemoteSubscribed()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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

        Assert.AreEqual("0002 LIST (REMOTE SUBSCRIBED) \"\" \"*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(6, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(3, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Marked), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NoInferiors), "mailbox #1 flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #1 flags");

        Assert.AreEqual("Fruit/Banana", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #2 flags");

        Assert.AreEqual("Fruit/Peach", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(2, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #3 flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.NonExistent), "mailbox #3 flags");
        
        Assert.AreEqual("Vegetable", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #4 flags");

        Assert.AreEqual("Vegetable/Broccoli", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #5 flags");

        Assert.AreEqual("Bread", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(2, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.Remote), "mailbox #6 flags");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #6 flags");
      }
    }

    [Test]
    public void TestListExtendedSelectRecursiveMatchSubscribed()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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

        Assert.AreEqual("0002 LIST (RECURSIVEMATCH SUBSCRIBED) \"\" \"*2\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(7, mailboxes.Length);

        Assert.AreEqual("foo2", mailboxes[0].Name, "mailbox #1");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        // TODO: child info

        Assert.AreEqual("foo2/bar2", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #2 flags");

        Assert.AreEqual("baz2/bar2", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(1, mailboxes[2].Flags.Count, "mailbox #3 count of flags");
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #3 flags");

        Assert.AreEqual("baz2/bar22", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #4 flags");

        Assert.AreEqual("baz2/bar222", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(1, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #5 flags");

        Assert.AreEqual("eps2", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(1, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #6 flags");
        // TODO: child info

        Assert.AreEqual("qux2/bar2", mailboxes[6].Name, "mailbox #6");
        Assert.AreEqual(1, mailboxes[6].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[6].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #6 flags");
      }
    }

    [Test]
    public void TestListExtendedMailboxPatternMultiple()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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

        Assert.AreEqual("0002 LIST \"\" (\"INBOX\" \"Drafts\" \"Sent/%\")\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(5, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(0, mailboxes[0].Flags.Count, "mailbox #1 count of flags");

        Assert.AreEqual("Drafts", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(1, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NoInferiors), "mailbox #2 flags");

        Assert.AreEqual("Sent/March2004", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox #3 count of flags");

        Assert.AreEqual("Sent/December2003", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.Marked), "mailbox #4 flags");

        Assert.AreEqual("Sent/August2004", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(0, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
      }
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestListExtendedReturnStatusIncapable()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));
        Assert.IsFalse(session.ServerCapabilities.Has(ImapCapability.ListStatus));

        session.HandlesIncapableAsException = true;

        ImapMailbox[] mailboxes;

        session.ListExtended("%", ImapListReturnOptions.StatusDataItems(ImapStatusDataItem.Messages), out mailboxes);
      }
    }

    [Test]
    public void TestListExtendedReturnStatus()
    {
      using (var session = Authenticate("LIST-EXTENDED", "LIST-STATUS")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListStatus));

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

        Assert.AreEqual("0002 LIST \"\" \"%\" RETURN (STATUS (MESSAGES UNSEEN))\r\n",
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
        Assert.IsTrue(mailboxes[2].Flags.Has(ImapMailboxFlag.NoSelect), "mailbox #3 flags");
        Assert.AreEqual(0L, mailboxes[2].ExistsMessage, "mailbox #3 exists messages");
        Assert.AreEqual(0L, mailboxes[2].UnseenMessage, "mailbox #3 unseen messages");
      }
    }

    [Test]
    public void TestListExtendedReturnStatusSelectSubscribedRecursiveMatch()
    {
      using (var session = Authenticate("LIST-EXTENDED", "LIST-STATUS")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListStatus));

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

        Assert.AreEqual("0002 LIST (SUBSCRIBED RECURSIVEMATCH) \"\" \"%\" RETURN (STATUS (MESSAGES))\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Subscribed), "mailbox #1 flags");
        Assert.AreEqual(17L, mailboxes[0].ExistsMessage, "mailbox #1 exists messages");

        Assert.AreEqual("foo", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox #2 count of flags");
        // child info
      }
    }

    [Test]
    public void TestListExtendedReturnSpecialUse()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.ListExtended));

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

        Assert.AreEqual("0002 LIST \"\" \"%\" RETURN (SPECIAL-USE)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(6, mailboxes.Length);

        Assert.IsTrue(mailboxes[0].IsInbox, "mailbox #1 (inbox)");
        Assert.AreEqual(1, mailboxes[0].Flags.Count, "mailbox #1 count of flags");
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.Marked), "mailbox #1 \\Marked");

        Assert.AreEqual("ToDo", mailboxes[1].Name, "mailbox #2");
        Assert.AreEqual(0, mailboxes[1].Flags.Count, "mailbox #2 count of flags");

        Assert.AreEqual("Projects", mailboxes[2].Name, "mailbox #3");
        Assert.AreEqual(0, mailboxes[2].Flags.Count, "mailbox #3 count of flags");

        Assert.AreEqual("SentMail", mailboxes[3].Name, "mailbox #4");
        Assert.AreEqual(1, mailboxes[3].Flags.Count, "mailbox #4 count of flags");
        Assert.IsTrue(mailboxes[3].Flags.Has(ImapMailboxFlag.Sent), "mailbox #4 \\Sent");

        Assert.AreEqual("MyDrafts", mailboxes[4].Name, "mailbox #5");
        Assert.AreEqual(2, mailboxes[4].Flags.Count, "mailbox #5 count of flags");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.Marked), "mailbox #5 \\Marked");
        Assert.IsTrue(mailboxes[4].Flags.Has(ImapMailboxFlag.Drafts), "mailbox #5 \\Drafts");

        Assert.AreEqual("Trash", mailboxes[5].Name, "mailbox #6");
        Assert.AreEqual(1, mailboxes[5].Flags.Count, "mailbox #6 count of flags");
        Assert.IsTrue(mailboxes[5].Flags.Has(ImapMailboxFlag.Trash), "mailbox #6 \\Trash");
      }
    }

    [Test]
    public void TestStatus()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0003 STATUS \"INBOX\" (UIDNEXT UNSEEN)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(4L, mailboxes[0].UidNext);
        Assert.AreEqual(2L, mailboxes[0].UnseenMessage);
      }
    }

    [Test]
    public void TestStatusUnmanagedMailbox1()
    {
      using (var session = Authenticate()) {
        // STATUS transaction
        server.EnqueueResponse("* STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)\r\n" +
                               "0002 OK STATUS completed\r\n");

        ImapMailbox statusMailbox;

        Assert.IsTrue((bool)session.Status("blurdybloop",
                                           ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext,
                                           out statusMailbox));

        Assert.AreEqual("0002 STATUS \"blurdybloop\" (MESSAGES UIDNEXT)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(statusMailbox);
        Assert.IsTrue(statusMailbox.NameEquals("blurdybloop"));
        Assert.AreEqual(44292L, statusMailbox.UidNext);
        Assert.AreEqual(231L, statusMailbox.ExistsMessage);
        Assert.IsNotNull(statusMailbox.Flags);
        Assert.IsNotNull(statusMailbox.ApplicableFlags);
        Assert.IsNotNull(statusMailbox.PermanentFlags);
      }
    }

    [Test]
    public void TestStatusUnmanagedMailbox2()
    {
      using (var session = Authenticate()) {
        // STATUS transaction
        server.EnqueueResponse("* STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)\r\n" +
                               "0002 OK STATUS completed\r\n");

        ImapStatusAttributeList statusAttributes;

        Assert.IsTrue((bool)session.Status("blurdybloop",
                                           ImapStatusDataItem.Messages + ImapStatusDataItem.UidNext,
                                           out statusAttributes));

        Assert.AreEqual("0002 STATUS \"blurdybloop\" (MESSAGES UIDNEXT)\r\n",
                        server.DequeueRequest());

        Assert.IsNull(statusAttributes.HighestModSeq);
        Assert.IsNull(statusAttributes.Recent);
        Assert.IsNull(statusAttributes.UidValidity);
        Assert.IsNull(statusAttributes.Unseen);
        Assert.AreEqual(44292L, statusAttributes.UidNext);
        Assert.AreEqual(231L, statusAttributes.Messages);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStatusSelectedMailbox()
    {
      using (var session = SelectMailbox()) {
        Assert.IsNotNull(session.SelectedMailbox);

        session.Status(session.SelectedMailbox, ImapStatusDataItem.UidNext);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStatusNoSelectMailbox()
    {
      using (var session = Authenticate()) {
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
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestStatusNonExistentMailbox()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" \"Fruit/*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].IsUnselectable);

        // STATUS transaction
        session.Status(mailboxes[1], ImapStatusDataItem.UidValidity);
      }
    }

    [Test]
    public void TestStatusHighestModSeq()
    {
      using (var session = Authenticate("CONDSTORE")) {
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

        Assert.AreEqual("0002 STATUS \"blurdybloop\" (MESSAGES UIDNEXT HIGHESTMODSEQ)\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestCreate()
    {
      using (var session = Authenticate()) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK CREATE completed\r\n");

        ImapMailbox createdMailbox;

        Assert.IsTrue((bool)session.Create("INBOX.日本語", out createdMailbox));

        Assert.AreEqual("0002 CREATE \"INBOX.&ZeVnLIqe-\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(createdMailbox);
        Assert.AreEqual("INBOX.日本語", createdMailbox.Name);
        Assert.AreEqual(new Uri(uri, "./INBOX.日本語"), createdMailbox.Url);
        Assert.IsNotNull(createdMailbox.Flags);
        Assert.IsNotNull(createdMailbox.ApplicableFlags);
        Assert.IsNotNull(createdMailbox.PermanentFlags);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCreateInbox()
    {
      using (var session = Authenticate()) {
        ImapMailbox createdMailbox;

        session.Create("INBOX", out createdMailbox);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestCreateExistentMailbox()
    {
      using (var session = Authenticate()) {
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
      }
    }

    [Test]
    public void TestCreateNonExistentMailbox()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" \"Fruit/*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NonExistent));

        // CREATE transaction
        server.EnqueueResponse("0003 OK CREATE completed\r\n");

        ImapMailbox createdMailbox;

        Assert.IsTrue((bool)session.Create("Fruit/Peach", out createdMailbox));

        Assert.AreEqual("0003 CREATE \"Fruit/Peach\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(createdMailbox);
        Assert.AreSame(mailboxes[1], createdMailbox);
      }
    }

    [Test]
    public void TestCreateSpecialUse()
    {
      using (var session = Authenticate("CREATE-SPECIAL-USE")) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK MySpecial created\r\n");

        ImapMailbox createdMailbox;

        Assert.IsTrue((bool)session.CreateSpecialUse("MySpecial", out createdMailbox, ImapMailboxFlag.Drafts, ImapMailboxFlag.Sent));

        Assert.AreEqual("0002 CREATE \"MySpecial\" (USE (\\Drafts \\Sent))\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(createdMailbox);
        Assert.AreEqual("MySpecial", createdMailbox.Name);
        Assert.AreEqual(2, createdMailbox.Flags.Count);
        Assert.IsTrue(createdMailbox.Flags.Has(ImapMailboxFlag.Drafts));
        Assert.IsTrue(createdMailbox.Flags.Has(ImapMailboxFlag.Sent));
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestCreateSpecialUseInvalidUseFlag()
    {
      using (var session = Authenticate("CREATE-SPECIAL-USE")) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK MySpecial created\r\n");

        ImapMailbox createdMailbox;

        session.CreateSpecialUse("MySpecial", out createdMailbox, ImapMailboxFlag.NoInferiors);
      }
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestCreateSpecialUseIncapable()
    {
      using (var session = Authenticate()) {
        session.HandlesIncapableAsException = true;

        // CREATE transaction
        session.CreateSpecialUse("Everything", ImapMailboxFlag.All);
      }
    }

    [Test]
    public void TestDelete()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0003 DELETE \"blurdybloop\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestDeleteUnmanagedMailbox()
    {
      using (var session = Authenticate()) {
        // DELETE transaction
        server.EnqueueResponse("0002 OK DELETE completed\r\n");

        Assert.IsTrue((bool)session.Delete("INBOX.☺"));

        Assert.AreEqual("0002 DELETE \"INBOX.&Jjo-\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteInbox1()
    {
      using (var session = Authenticate()) {
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
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteInbox2()
    {
      using (var session = Authenticate()) {
        session.Delete("inbox");
      }
    }

    [Test]
    public void TestDeleteSelectedMailbox()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0002 SELECT \"blurdybloop\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(ImapSessionState.Selected, session.State);

        // DELETE transaction
        server.EnqueueResponse("0003 OK DELETE completed\r\n");

        Assert.IsNotNull(session.SelectedMailbox);
        Assert.IsTrue((bool)session.Delete(session.SelectedMailbox));

        Assert.AreEqual("0003 DELETE \"blurdybloop\"\r\n",
                        server.DequeueRequest());

        Assert.IsNull(session.SelectedMailbox);
        Assert.AreEqual(ImapSessionState.Authenticated, session.State);
      }
    }

    [Test]
    public void TestDeleteHierarchy()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0003 DELETE \"foo/bar\"\r\n",
                        server.DequeueRequest());

        server.EnqueueResponse("0004 OK DELETE completed\r\n");

        Assert.IsTrue((bool)session.Delete(mailboxes[1]));

        Assert.AreEqual("0004 DELETE \"foo\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteParentMailboxNoSelect()
    {
      using (var session = Authenticate()) {
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
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestDeleteParentMailboxNonExistentHasChildren()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\NonExistent \\HasChildren) \"/\" music\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("%",
                                                 ImapListReturnOptions.Children,
                                                 out mailboxes));

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.NonExistent));
        Assert.IsTrue(mailboxes[0].Flags.Has(ImapMailboxFlag.HasChildren));

        session.Delete(mailboxes[0]);
      }
    }

    [Test]
    public void TestRename()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual(new Uri(uri, "./INBOX.日本語"), renamingMailbox.Url);

        Assert.IsTrue((bool)session.Rename(renamingMailbox, "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0003 RENAME \"INBOX.&ZeVnLIqe-\" \"INBOX.&Jjo-\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsTrue(object.ReferenceEquals(renamingMailbox, renamedMailbox));
        Assert.AreEqual(new Uri(uri, "./INBOX.☺"), renamedMailbox.Url);
      }
    }

    [Test]
    public void TestRenameInbox1()
    {
      using (var session = Authenticate()) {
        // REMANE transaction
        server.EnqueueResponse("0002 OK RENAME completed\r\n");

        ImapMailbox renamedMailbox;

        Assert.IsTrue((bool)session.Rename("INBOX", "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0002 RENAME \"INBOX\" \"INBOX.&Jjo-\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsFalse(renamedMailbox.IsInbox);
      }
    }

    [Test]
    public void TestRenameInbox2()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0003 RENAME \"INBOX\" \"INBOX.&Jjo-\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsTrue(object.ReferenceEquals(inbox, renamedMailbox));
        Assert.IsFalse(renamedMailbox.IsInbox);
      }
    }

    [Test]
    public void TestRenameUnmanagedMailbox()
    {
      using (var session = Authenticate()) {
        // REMANE transaction
        server.EnqueueResponse("0002 OK RENAME completed\r\n");

        ImapMailbox renamedMailbox;

        Assert.IsTrue((bool)session.Rename("INBOX", "INBOX.☺", out renamedMailbox));

        Assert.AreEqual("0002 RENAME \"INBOX\" \"INBOX.&Jjo-\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("INBOX.☺", renamedMailbox.Name);
        Assert.IsNotNull(renamedMailbox.Flags);
        Assert.IsNotNull(renamedMailbox.ApplicableFlags);
        Assert.IsNotNull(renamedMailbox.PermanentFlags);
      }
    }

    [Test]
    public void TestRenameHierarchy()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0003 RENAME \"foo\" \"zowie\"\r\n",
                        server.DequeueRequest());

        Assert.AreEqual("blurdybloop", mailboxes[0].Name);
        Assert.AreEqual("zowie", mailboxes[1].Name);
        Assert.AreEqual("zowie/bar", mailboxes[2].Name);
      }
    }

    [Test]
    public void TestRenameSelectedMailbox()
    {
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0002 SELECT \"blurdybloop\"\r\n",
                        server.DequeueRequest());

        // REMANE transaction
        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        Assert.IsNotNull(session.SelectedMailbox);

        var selectedMailbox = session.SelectedMailbox;

        Assert.IsTrue((bool)session.Rename(selectedMailbox, "foo"));

        Assert.AreEqual("0003 RENAME \"blurdybloop\" \"foo\"\r\n",
                        server.DequeueRequest());

        Assert.AreSame(selectedMailbox, session.SelectedMailbox);
        Assert.AreEqual("foo", selectedMailbox.Name);
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestRenameToExistMailbox()
    {
      using (var session = Authenticate()) {
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
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRenameToSameName1()
    {
      using (var session = Authenticate()) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);

        session.Rename(mailboxes[0], "blurdybloop");
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRenameToSameName2()
    {
      using (var session = Authenticate()) {
        session.Rename("blurdybloop", "blurdybloop");
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestRenameToInbox()
    {
      using (var session = Authenticate()) {
        // LIST transaction
        server.EnqueueResponse("* LIST () \"/\" blurdybloop\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.List(out mailboxes);

        server.DequeueRequest();

        Assert.AreEqual(1, mailboxes.Length);
        Assert.AreEqual("blurdybloop", mailboxes[0].Name);

        session.Rename("blurdybloop", "INBOX");
      }
    }

    [Test]
    public void TestRenameToNonExistentMailbox()
    {
      using (var session = Authenticate("LIST-EXTENDED")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \"/\" \"Fruit/Banana\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \"/\" \"Fruit/Peach\"\r\n" +
                               "0002 OK done\r\n");

        ImapMailbox[] mailboxes;

        Assert.IsTrue((bool)session.ListExtended("Fruit/*",
                                                 ImapListSelectionOptions.Subscribed,
                                                 out mailboxes));

        Assert.AreEqual("0002 LIST (SUBSCRIBED) \"\" \"Fruit/*\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(mailboxes);
        Assert.AreEqual(2, mailboxes.Length);
        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NonExistent));

        // RENAME transaction
        ImapMailbox renamedMailbox;

        server.EnqueueResponse("0003 OK RENAME completed\r\n");

        Assert.IsTrue((bool)session.Rename("Fruit/Unsubscribed", "Fruit/Peach", out renamedMailbox));

        Assert.AreEqual("0003 RENAME \"Fruit/Unsubscribed\" \"Fruit/Peach\"\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(renamedMailbox);
        Assert.AreEqual("Fruit/Peach", renamedMailbox.Name);
        Assert.AreNotSame(mailboxes[1], renamedMailbox);
      }
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
      using (var session = Authenticate()) {
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

        Assert.AreEqual("0002 RENAME \"FOO\" \"BAR\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestSubscribe()
    {
      using (var session = Authenticate()) {
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
  
        Assert.AreEqual("0003 SUBSCRIBE \"INBOX.&ZeVnLIqe-\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestSubscribeCreated()
    {
      using (var session = Authenticate()) {
        // CREATE transaction
        server.EnqueueResponse("0002 OK CREATE completed\r\n");

        ImapMailbox created;

        Assert.IsTrue((bool)session.Create("Sent", out created));

        Assert.AreEqual("0002 CREATE \"Sent\"\r\n",
                        server.DequeueRequest());

        // SUBSCRIBE transaction
        server.EnqueueResponse("0003 OK SUBSCRIBE completed\r\n");

        Assert.IsTrue((bool)session.Subscribe(created));

        Assert.AreEqual("0003 SUBSCRIBE \"Sent\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test, ExpectedException(typeof(ImapProtocolViolationException))]
    public void TestSubscribeNonExistent()
    {
      using (var session = Authenticate("LIST-EXTEND")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\HasChildren) \".\" \"INBOX\"\r\n" +
                               "* LIST (\\HasChildren \\NonExistent) \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.ListExtended("%", out mailboxes);

        server.DequeueRequest();

        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NonExistent));

        session.Subscribe(mailboxes[1]);
      }
    }

    [Test]
    public void TestUnsubscribe()
    {
      using (var session = Authenticate()) {
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
  
        Assert.AreEqual("0003 UNSUBSCRIBE \"INBOX.&ZeVnLIqe-\"\r\n",
                        server.DequeueRequest());
      }
    }

    [Test]
    public void TestUnsubscribeNonExistent()
    {
      using (var session = Authenticate("LIST-EXTEND")) {
        // LIST transaction
        server.EnqueueResponse("* LIST (\\Subscribed) \".\" \"INBOX\"\r\n" +
                               "* LIST (\\Subscribed \\NonExistent) \".\" \"INBOX.&ZeVnLIqe-\"\r\n" +
                               "0002 OK LIST completed\r\n");

        ImapMailbox[] mailboxes;

        session.ListExtended("*", ImapListSelectionOptions.Subscribed, out mailboxes);

        server.DequeueRequest();

        // UNSUBSCRIBE transaction
        server.EnqueueResponse("0003 OK UNSUBSCRIBE completed\r\n");

        Assert.IsTrue(mailboxes[1].Flags.Has(ImapMailboxFlag.NonExistent));

        Assert.IsTrue((bool)session.Unsubscribe(mailboxes[1]));

        Assert.AreEqual("0003 UNSUBSCRIBE \"INBOX.&ZeVnLIqe-\"\r\n",
                        server.DequeueRequest());
      }
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
      using (var session = Authenticate()) {
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
      }
    }

    [Test]
    public void TestEnable()
    {
      using (var session = Authenticate("ENABLE", "X-GOOD-IDEA")) {
        Assert.IsTrue(session.ServerCapabilities.Has(ImapCapability.Enable));
  
        // ENABLE transaction
        server.EnqueueResponse("* ENABLED X-GOOD-IDEA\r\n" +
                               "0002 OK ENABLE completed\r\n");
  
        ImapCapabilityList enabledCapas;
  
        Assert.IsTrue((bool)session.Enable(out enabledCapas, "CONDSTORE", "X-GOOD-IDEA"));
  
        Assert.AreEqual("0002 ENABLE CONDSTORE X-GOOD-IDEA\r\n",
                        server.DequeueRequest());
  
        Assert.AreEqual(1, enabledCapas.Count);
        Assert.IsTrue(enabledCapas.Has("X-GOOD-IDEA"));
      }
    }
  }
}
