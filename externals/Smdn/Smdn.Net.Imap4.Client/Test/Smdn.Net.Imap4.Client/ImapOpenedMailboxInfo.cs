using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapOpenedMailboxInfoTests {
    [Test]
    public void TestRefresh()
    {
      var selectResp =
        "* EXISTS 3\r\n" +
        "* RECENT 1\r\n" +
        "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);
        Assert.AreEqual(1L, mailbox.RecentMessageCount);
        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(5, mailbox.ApplicableFlags.Count);
        Assert.IsFalse(mailbox.ApplicableFlags.Has("$label1"));

        // NOOP
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "* EXISTS 1\r\n" +
                                     "* RECENT 2\r\n" +
                                     "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft $label1)\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));

        Assert.AreEqual(1L, mailbox.ExistMessageCount);
        Assert.AreEqual(2L, mailbox.RecentMessageCount);
        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(6, mailbox.ApplicableFlags.Count);
        Assert.IsTrue(mailbox.ApplicableFlags.Has("$label1"));
      });
    }

    [Test]
    public void TestRefreshClosed()
    {
      TestUtils.TestClosedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        try {
          mailbox.Refresh();
          Assert.Fail("ImapMailboxClosedException not thrown");
        }
        catch (ImapMailboxClosedException ex) {
          Assert.IsNotNull(ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxClosedException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestOpen()
    {
      TestUtils.TestOpenedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.IsOpen);

        Assert.AreSame(mailbox, mailbox.Open());
        Assert.AreSame(mailbox, mailbox.Client.OpenedMailbox);

        Assert.IsTrue(mailbox.IsOpen);
      });
    }

    [Test]
    public void TestOpenAsReadOnly()
    {
      var selectResp =
        "$tag OK [READ-ONLY] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.IsOpen);

        Assert.AreSame(mailbox, mailbox.Open());
        Assert.AreSame(mailbox, mailbox.Client.OpenedMailbox);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.IsTrue(mailbox.IsReadOnly);
      });
    }

    [Test]
    public void TestOpenUidNotSticky()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "* NO [UIDNOTSTICKY]\r\n" +
        "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n" +
        "* OK [PERMANENTFLAGS (\\Deleted \\Seen)] Limited\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.IsOpen);

        Assert.AreSame(mailbox, mailbox.Open());
        Assert.AreSame(mailbox, mailbox.Client.OpenedMailbox);
        Assert.IsTrue(mailbox.IsOpen);
        Assert.IsFalse(mailbox.IsReadOnly);
        Assert.AreEqual(23L, mailbox.UidValidity);
        Assert.IsFalse(mailbox.IsUidPersistent);

        Assert.IsNotNull(mailbox.ApplicableFlags);
        Assert.AreEqual(5, mailbox.ApplicableFlags.Count);

        Assert.IsNotNull(mailbox.PermanentFlags);
        Assert.AreEqual(2, mailbox.PermanentFlags.Count);
        Assert.IsFalse(mailbox.PermanentFlags.Has(ImapMessageFlag.AllowedCreateKeywords));
        Assert.IsFalse(mailbox.IsAllowedToCreateKeywords);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestOpenNewly()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestOpenReopenClosed()
    {
    }

    [Test, Ignore("not implemented")]
    public void TestOpenReopenReadWriteChange()
    {
    }

    [Test]
    public void TestClose()
    {
      TestUtils.TestOpenedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.IsOpen);

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        mailbox.Close();

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));

        Assert.IsFalse(mailbox.IsOpen);
        Assert.IsNull(mailbox.Client.OpenedMailbox);
      });
    }

    [Test]
    public void TestCloseAlreadyClosed()
    {
      TestUtils.TestClosedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // no exceptions
        mailbox.Close();

        Assert.IsFalse(mailbox.IsOpen);
        Assert.IsNull(mailbox.Client.OpenedMailbox);
      });
    }

    [Test, Ignore("not implemented")]
    public void TestCloseAnotherMailboxOpened()
    {
    }

    [Test]
    public void TestExpunge()
    {
      var selectResp =
        "* EXISTS 3\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Deleted))\r\n" +
                                     "* FETCH 2 (UID 2 FLAGS (\\Deleted))\r\n" +
                                     "* FETCH 3 (UID 3 FLAGS (\\Deleted))\r\n" +
                                     "$tag OK done\r\n");

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes).ToArray();

        server.DequeueRequest(); // NOOP
        server.DequeueRequest(); // FETCH

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsTrue(messages[0].IsMarkedAsDeleted);
        Assert.IsFalse(messages[0].IsDeleted);

        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsTrue(messages[1].IsMarkedAsDeleted);
        Assert.IsFalse(messages[1].IsDeleted);

        Assert.AreEqual(3L, messages[2].Sequence);
        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsTrue(messages[2].IsMarkedAsDeleted);
        Assert.IsFalse(messages[2].IsDeleted);

        // EXPUNGE
        server.EnqueueTaggedResponse("* EXPUNGE 3\r\n" +
                                     "* EXPUNGE 2\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "* EXISTS 0\r\n" +
                                     "$tag OK done\r\n");

        mailbox.Expunge();

        Assert.That(server.DequeueRequest(), Text.EndsWith("EXPUNGE\r\n"));

        Assert.AreEqual(0L, mailbox.ExistMessageCount);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[0].Sequence);
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsTrue(messages[0].IsMarkedAsDeleted);
        Assert.IsTrue(messages[0].IsDeleted);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[1].Sequence);
        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsTrue(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue(messages[1].IsDeleted);

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[2].Sequence);
        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsTrue(messages[2].IsMarkedAsDeleted);
        Assert.IsTrue(messages[2].IsDeleted);
      });
    }

    [Test]
    public void TestExpungeClosed()
    {
      TestUtils.TestClosedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        try {
          mailbox.Expunge();
          Assert.Fail("ImapMailboxClosedException not thrown");
        }
        catch (ImapMailboxClosedException ex) {
          Assert.IsNotNull(ex.Mailbox);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxClosedException deserialized) {
            Assert.IsNotNull(deserialized.Mailbox);
            Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
          });
        }
      });
    }

    [Test]
    public void TestGetMessages()
    {
      var selectResp =
        "* 5 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetMessages();

        Assert.AreEqual(5L, mailbox.ExistMessageCount);

        var enumeratedMessages = new ImapMessageInfo[4];

        using (var enumerator = messages.GetEnumerator()) {
          // NOOP
          server.EnqueueTaggedResponse("* 5 EXPUNGE\r\n" +
                                       "* 4 EXISTS\r\n" + 
                                       "$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 3)\r\n" +
                                       "* FETCH 2 (UID 5)\r\n" +
                                       "* FETCH 3 (UID 7)\r\n" +
                                       "* FETCH 4 (UID 9)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:4 (UID)\r\n"));

          Assert.AreEqual(4L, mailbox.ExistMessageCount);

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(3L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(1L, enumerator.Current.Sequence, "#0 seq");

          enumeratedMessages[0] = enumerator.Current;

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#1 non-null");
          Assert.AreEqual(5L, enumerator.Current.Uid, "#1 uid");
          Assert.AreEqual(2L, enumerator.Current.Sequence, "#1 seq");

          enumeratedMessages[1] = enumerator.Current;

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#2 non-null");
          Assert.AreEqual(7L, enumerator.Current.Uid, "#2 uid");
          Assert.AreEqual(3L, enumerator.Current.Sequence, "#2 seq");

          enumeratedMessages[2] = enumerator.Current;

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#3 non-null");
          Assert.AreEqual(9L, enumerator.Current.Uid, "#3 uid");
          Assert.AreEqual(4L, enumerator.Current.Sequence, "#3 seq");

          enumeratedMessages[3] = enumerator.Current;

          Assert.IsFalse(enumerator.MoveNext());
        }

        /*
         * re-enumerate
         */
        Assert.AreEqual(4L, mailbox.ExistMessageCount);

        using (var enumerator = messages.GetEnumerator()) {
          // NOOP
          server.EnqueueTaggedResponse("* 2 EXPUNGE\r\n" +
                                       "* 1 EXPUNGE\r\n" +
                                       "$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 7)\r\n" +
                                       "* FETCH 2 (UID 9)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:2 (UID)\r\n"));

          Assert.AreEqual(2L, mailbox.ExistMessageCount);

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(7L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(1L, enumerator.Current.Sequence, "#0 seq");

          Assert.AreSame(enumeratedMessages[2], enumerator.Current, "returns same instance #0");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#1 non-null");
          Assert.AreEqual(9L, enumerator.Current.Uid, "#1 uid");
          Assert.AreEqual(2L, enumerator.Current.Sequence, "#1 seq");

          Assert.AreSame(enumeratedMessages[3], enumerator.Current, "returns same instance #1");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetMessagesNoMessageExists()
    {
      var selectResp =
        "* 2 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // NOOP
        server.EnqueueTaggedResponse("* 1 EXPUNGE\r\n" +
                                     "* 1 EXPUNGE\r\n" +
                                     "$tag OK done\r\n");

        var messages = new List<ImapMessageInfo>(mailbox.GetMessages());

        Assert.AreEqual(0L, mailbox.ExistMessageCount);

        Assert.AreEqual(0, messages.Count);

        Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
      });
    }

    [Test]
    public void TestGetMessageByUid()
    {
      GetMessageByUidOrSequence(true);
    }

    [Test]
    public void TestGetMessageBySequence()
    {
      GetMessageByUidOrSequence(false);
    }

    private void GetMessageByUidOrSequence(bool uid)
    {
      var selectResp =
        "* 1 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 3)\r\n" +
                                     "$tag OK done\r\n");

        var message = uid
          ? mailbox.GetMessageByUid(3L)
          : mailbox.GetMessageBySequence(1L);

        Assert.IsNotNull(message);
        Assert.AreEqual(3L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.AreEqual(23L, message.UidValidity);
        Assert.AreSame(mailbox, message.Mailbox);

        if (uid)
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 3 (UID)\r\n"));
        else
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1 (UID)\r\n"));

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 3)\r\n" +
                                     "$tag OK done\r\n");

        var messageSecond = uid
          ? mailbox.GetMessageByUid(3L)
          : mailbox.GetMessageBySequence(1L);

        Assert.AreSame(message, messageSecond, "returns same instance");

        if (uid)
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 3 (UID)\r\n"));
        else
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1 (UID)\r\n"));
      });
    }

    [Test]
    public void TestGetMessageByUidNotFound()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // FETCH
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          mailbox.GetMessageByUid(3L);
          Assert.Fail("ImapMessageNotFoundException not thrown");
        }
        catch (ImapMessageNotFoundException ex) {
          Assert.IsNotNull(ex.SequenceOrUidSet);
          Assert.AreEqual("3", ex.SequenceOrUidSet.ToString());

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageNotFoundException deserialized) {
            Assert.IsNull(deserialized.SequenceOrUidSet);
          });
        }
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetMessageBySequenceGreaterThanExistCount()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.GetMessageBySequence(1L);
      });
    }

    [Test, ExpectedException(typeof(ImapErrorResponseException))]
    public void TestGetMessageBySequenceNoResponse()
    {
      var selectResp =
        "* 1 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // FETCH
        server.EnqueueTaggedResponse("$tag NO such message\r\n");

        mailbox.GetMessageBySequence(1L);
      });
    }

    [Test]
    public void TestGetMessagesByUids()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetMessages(1L, 3L, 5L);

        using (var enumerator = messages.GetEnumerator()) {
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                       "* FETCH 3 (UID 3)\r\n" +
                                       "* FETCH 5 (UID 5)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1,3,5 (UID)\r\n"));

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(1L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(1L, enumerator.Current.Sequence, "#0 seq");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#1 non-null");
          Assert.AreEqual(3L, enumerator.Current.Uid, "#1 uid");
          Assert.AreEqual(3L, enumerator.Current.Sequence, "#1 seq");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#2 non-null");
          Assert.AreEqual(5L, enumerator.Current.Uid, "#2 uid");
          Assert.AreEqual(5L, enumerator.Current.Sequence, "#2 seq");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetMessagesWithSearchCriteria()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetMessages(ImapSearchCriteria.Recent);

        using (var enumerator = messages.GetEnumerator()) {
          // SEARCH
          server.EnqueueTaggedResponse("* SEARCH 1 2 3\r\n" +
                                       "$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 3)\r\n" +
                                       "* FETCH 2 (UID 5)\r\n" +
                                       "* FETCH 3 (UID 7)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("SEARCH RECENT\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:3 (UID)\r\n"));

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(3L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(1L, enumerator.Current.Sequence, "#0 seq");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#1 non-null");
          Assert.AreEqual(5L, enumerator.Current.Uid, "#1 uid");
          Assert.AreEqual(2L, enumerator.Current.Sequence, "#1 seq");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#2 non-null");
          Assert.AreEqual(7L, enumerator.Current.Uid, "#2 uid");
          Assert.AreEqual(3L, enumerator.Current.Sequence, "#2 seq");

          Assert.IsFalse(enumerator.MoveNext());
        }

        /*
         * re-enumerate
         */
        using (var enumerator = messages.GetEnumerator()) {
          // SEARCH
          server.EnqueueTaggedResponse("* SEARCH 4\r\n" +
                                       "$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 4 (UID 8)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("SEARCH RECENT\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 4 (UID)\r\n"));

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(8L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(4L, enumerator.Current.Sequence, "#0 seq");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetMessagesWithSearchCriteriaSearchresCapable()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.ESearch, ImapCapability.Searchres},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetMessages(ImapSearchCriteria.Recent);

        using (var enumerator = messages.GetEnumerator()) {
          // SEARCH
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 3)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("SEARCH RETURN (SAVE) RECENT\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH $ (UID)\r\n"));

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(3L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(1L, enumerator.Current.Sequence, "#0 seq");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetMessagesWithSearchCriteriaNothingMatched()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetMessages(ImapSearchCriteria.Recent);

        using (var enumerator = messages.GetEnumerator()) {
          // SEARCH
          server.EnqueueTaggedResponse("* SEARCH\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsFalse(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("SEARCH RECENT\r\n"));
        }
      });
    }

    [Test]
    public void TestGetSortedMessages()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Sort},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetSortedMessages(ImapSortCriteria.Arrival, ImapSearchCriteria.Recent);

        using (var enumerator = messages.GetEnumerator()) {
          // SORT
          server.EnqueueTaggedResponse("* SORT 1 4 2\r\n" +
                                       "$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 2)\r\n" +
                                       "* FETCH 2 (UID 4)\r\n" +
                                       "* FETCH 4 (UID 8)\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("SORT (ARRIVAL) utf-8 RECENT\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1,4,2 (UID)\r\n"));

          Assert.IsNotNull(enumerator.Current, "#0 non-null");
          Assert.AreEqual(2L, enumerator.Current.Uid, "#0 uid");
          Assert.AreEqual(1L, enumerator.Current.Sequence, "#0 seq");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#1 non-null");
          Assert.AreEqual(4L, enumerator.Current.Uid, "#1 uid");
          Assert.AreEqual(2L, enumerator.Current.Sequence, "#1 seq");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current, "#2 non-null");
          Assert.AreEqual(8L, enumerator.Current.Uid, "#2 uid");
          Assert.AreEqual(4L, enumerator.Current.Sequence, "#2 seq");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test, ExpectedException(typeof(ImapIncapableException))]
    public void TestGetSortedMessagesSortIncapable()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.GetSortedMessages(ImapSortCriteria.Subject, ImapSearchCriteria.Recent);
      });
    }


    [Test]
    public void TestGetSortedMessagesNothingMatched()
    {
      var selectResp =
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.Sort},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetSortedMessages(ImapSortCriteria.Arrival, ImapSearchCriteria.Recent);

        using (var enumerator = messages.GetEnumerator()) {
          // SORT
          server.EnqueueTaggedResponse("* SORT\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsFalse(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("SORT (ARRIVAL) utf-8 RECENT\r\n"));
        }
      });
    }

    [Test, Ignore("to be added")]
    public void TestWaitForMessageArrival()
    {
    }

    [Test, Ignore("to be added")]
    public void TestBeginWaitForMessageArrival()
    {
    }

    [Test, Ignore("to be added")]
    public void TestEndWaitForMessageArrival()
    {
    }
  }
}
