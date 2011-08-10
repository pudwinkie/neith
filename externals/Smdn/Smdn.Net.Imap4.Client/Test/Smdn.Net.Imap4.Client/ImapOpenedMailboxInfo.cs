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
        Assert.IsFalse(mailbox.ApplicableFlags.Contains("$label1"));

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
        Assert.IsTrue(mailbox.ApplicableFlags.Contains("$label1"));
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
        Assert.IsFalse(mailbox.PermanentFlags.Contains(ImapMessageFlag.AllowedCreateKeywords));
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
    public void TestDispose()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK [READ-WRITE] done\r\n");

        var mailbox = client.OpenInbox();

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" INBOX\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT INBOX\r\n"));

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        (mailbox as IDisposable).Dispose();

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));

        Assert.IsFalse(mailbox.IsOpen);
        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestDisposeAlreadyClosed()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK [READ-WRITE] done\r\n");

        var mailbox = client.OpenInbox();

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" INBOX\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT INBOX\r\n"));

        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        client.CloseMailbox();

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));

        Assert.IsFalse(mailbox.IsOpen);
        Assert.IsNull(client.OpenedMailbox);

        (mailbox as IDisposable).Dispose();

        Assert.IsFalse(mailbox.IsOpen);
        Assert.IsNull(client.OpenedMailbox);
      });
    }

    [Test]
    public void TestDisposeAlreadyLoggedOut()
    {
      TestUtils.TestAuthenticated(delegate(ImapPseudoServer server, ImapClient client) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX\r\n" +
                                     "$tag OK done\r\n");
        // SELECT
        server.EnqueueTaggedResponse("$tag OK [READ-WRITE] done\r\n");

        var mailbox = client.OpenInbox();

        Assert.That(server.DequeueRequest(), Text.EndsWith("LIST \"\" INBOX\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("SELECT INBOX\r\n"));

        // LOGOUT
        server.EnqueueTaggedResponse("* BYE stopped\r\n");
        server.EnqueueTaggedResponse("$tag OK disconnected\r\n");

        server.Stop(true);

        client.Logout();

        Assert.IsFalse(client.IsConnected);

        (mailbox as IDisposable).Dispose(); // throws no exception
      });
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
    public void TestMoveMessagesTo()
    {
      CopyOrMoveMessagesTo(true);
    }

    [Test]
    public void TestCopyMessagesTo()
    {
      CopyOrMoveMessagesTo(false);
    }

    private void CopyOrMoveMessagesTo(bool move)
    {
      var selectResp =
        "* 5 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = mailbox.Client.GetMailbox("dest");

        server.DequeueRequest();

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        // COPY
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        if (move)
          // STORE
          server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 2 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 3 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 4 (FLAGS (\\Deleted))\r\n" +
                                       "* FETCH 5 (FLAGS (\\Deleted))\r\n" +
                                       "$tag OK done\r\n");

        if (move)
          mailbox.MoveMessagesTo(destMailbox);
        else
          mailbox.CopyMessagesTo(destMailbox);

        Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("COPY 1:5 dest\r\n"));

        if (move)
          Assert.That(server.DequeueRequest(), Text.EndsWith("STORE 1:5 +FLAGS (\\Deleted)\r\n"));
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestMoveMessagesToDestinationNull()
    {
      var selectResp =
        "* 5 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.MoveMessagesTo(null);
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestCopyMessagesToDestinationNull()
    {
      var selectResp =
        "* 5 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        mailbox.CopyMessagesTo(null);
      });
    }

    [Test]
    public void TestMoveMessagesToMailboxOfDifferentSession()
    {
      CopyOrMoveMessagesToMailboxOfDifferentSession(true);
    }

    [Test]
    public void TestCopyMessagesToMailboxOfDifferentSession()
    {
      CopyOrMoveMessagesToMailboxOfDifferentSession(false);
    }

    private void CopyOrMoveMessagesToMailboxOfDifferentSession(bool move)
    {
      var selectResp =
        "* 3 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer serverSource, ImapOpenedMailboxInfo mailboxSource) {
        TestUtils.TestAuthenticated(delegate(ImapPseudoServer serverDest, ImapClient clientDest) {
          // LIST
          serverDest.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                           "$tag OK done\r\n");

          var mailboxDest = clientDest.GetMailbox("dest");

          serverDest.DequeueRequest();

          Assert.AreNotSame(mailboxSource.Client, mailboxDest.Client);

          // NOOP (source)
          serverSource.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH (source)
          serverSource.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                             "* FETCH 2 (UID 2)\r\n" +
                                             "* FETCH 3 (UID 3)\r\n" +
                                             "$tag OK done\r\n");
          serverSource.EnqueueTaggedResponse("* FETCH 1 (" +
                                             "FLAGS (\\Answered $label1) " +
                                             "INTERNALDATE \"25-Jan-2011 15:29:06 +0900\" " +
                                             "RFC822.SIZE 13 " +
                                             "BODY[] {13}\r\ntest message1)\r\n" +
                                             "$tag OK done\r\n");
          serverSource.EnqueueTaggedResponse("* FETCH 2 (" +
                                             "FLAGS (\\Answered $label2) " +
                                             "INTERNALDATE \"25-Jan-2011 15:29:06 +0900\" " +
                                             "RFC822.SIZE 13 " +
                                             "BODY[] {13}\r\ntest message2)\r\n" +
                                             "$tag OK done\r\n");
          serverSource.EnqueueTaggedResponse("* FETCH 3 (" +
                                             "FLAGS (\\Answered $label3) " +
                                             "INTERNALDATE \"25-Jan-2011 15:29:06 +0900\" " +
                                             "RFC822.SIZE 13 " +
                                             "BODY[] {13}\r\ntest message3)\r\n" +
                                             "$tag OK done\r\n");

          if (move)
            // UID STORE (source)
            serverSource.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                               "* FETCH 2 (FLAGS (\\Deleted))\r\n" +
                                               "* FETCH 3 (FLAGS (\\Deleted))\r\n" +
                                               "$tag OK done\r\n");

          // APPEND (dest)
          serverDest.EnqueueResponse("+ OK continue\r\n");
          serverDest.EnqueueResponse(string.Empty);
          serverDest.EnqueueTaggedResponse("$tag OK [APPENDUID 38505 3955] APPEND completed\r\n");
          serverDest.EnqueueResponse("+ OK continue\r\n");
          serverDest.EnqueueResponse(string.Empty);
          serverDest.EnqueueTaggedResponse("$tag OK [APPENDUID 38505 3956] APPEND completed\r\n");
          serverDest.EnqueueResponse("+ OK continue\r\n");
          serverDest.EnqueueResponse(string.Empty);
          serverDest.EnqueueTaggedResponse("$tag OK [APPENDUID 38505 3957] APPEND completed\r\n");

          if (move)
            mailboxSource.MoveMessagesTo(mailboxDest);
          else
            mailboxSource.CopyMessagesTo(mailboxDest);

          Assert.That(serverSource.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(serverSource.DequeueRequest(), Text.EndsWith("FETCH 1:3 (UID)\r\n"));
          Assert.That(serverSource.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE INTERNALDATE FLAGS BODY.PEEK[]<0.10240>)\r\n"));
          Assert.That(serverSource.DequeueRequest(), Text.EndsWith("UID FETCH 2 (RFC822.SIZE INTERNALDATE FLAGS BODY.PEEK[]<0.10240>)\r\n"));
          Assert.That(serverSource.DequeueRequest(), Text.EndsWith("UID FETCH 3 (RFC822.SIZE INTERNALDATE FLAGS BODY.PEEK[]<0.10240>)\r\n"));

          if (move)
            Assert.That(serverSource.DequeueRequest(), Text.EndsWith("STORE 1:3 +FLAGS (\\Deleted)\r\n"));

          Assert.That(serverDest.DequeueRequest(),
                      Text.EndsWith("APPEND dest (\\Answered $label1) \"25-Jan-2011 15:29:06 +0900\" {13}\r\n"));
          Assert.That(serverDest.DequeueRequest(),
                      Text.StartsWith("test message1"));
          Assert.That(serverDest.DequeueRequest(),
                      Text.EndsWith("APPEND dest (\\Answered $label2) \"25-Jan-2011 15:29:06 +0900\" {13}\r\n"));
          Assert.That(serverDest.DequeueRequest(),
                      Text.StartsWith("test message2"));
          Assert.That(serverDest.DequeueRequest(),
                      Text.EndsWith("APPEND dest (\\Answered $label3) \"25-Jan-2011 15:29:06 +0900\" {13}\r\n"));
          Assert.That(serverDest.DequeueRequest(),
                      Text.StartsWith("test message3"));
        });
      });
    }
  }
}
