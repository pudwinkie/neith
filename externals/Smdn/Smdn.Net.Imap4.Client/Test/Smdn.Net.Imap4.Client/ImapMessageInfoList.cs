using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapMessageInfoListTests {
    private string GetFetchResponses(int count)
    {
      return GetFetchResponses(1, count);
    }

    private string GetFetchResponses(int start, int count)
    {
      var resp = new StringBuilder();

      for (int n = 0, seq = start; n < count; n++, seq++) {
        var uid = seq;

        resp.AppendFormat("* FETCH {0} (UID {1})\r\n", seq, uid);
      }

      resp.Append("$tag OK done\r\n");

      return resp.ToString();
    }

    [Test]
    public void TestGetEnumerator()
    {
      var selectResp =
        "* 50 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(50L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages();

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse(GetFetchResponses(50));

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:50 (UID)\r\n"));

          for (var i = 1L;; i++) {
            Assert.AreEqual(i, enumerator.Current.Uid, "uid of #{0}", i);
            Assert.AreEqual(i, enumerator.Current.Sequence, "seq of #{0}", i);

            if (i == 50L) {
              Assert.IsFalse(enumerator.MoveNext());
              break;
            }
            else {
              Assert.IsTrue(enumerator.MoveNext());
            }
          }
        }
      });
    }

    [Test]
    public void TestGetEnumeratorEmptySet1()
    {
      var selectResp =
        "* 0 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(0L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages();

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");

          Assert.IsFalse(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
        }
      });
    }

    [Test]
    public void TestGetEnumeratorEmptySet2()
    {
      var selectResp =
        "* 1 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(1L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages();

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsFalse(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
        }

        Assert.AreEqual(0L, mailbox.ExistMessageCount);
      });
    }

    [Test]
    public void TestGetEnumeratorSplitted()
    {
      var selectResp =
        "* 250 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(250L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages();

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse(GetFetchResponses(1, 100));

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:100 (UID)\r\n"));

          for (var i = 1L;; i++) {
            Assert.AreEqual(i, enumerator.Current.Uid, "uid of #{0}", i);
            Assert.AreEqual(i, enumerator.Current.Sequence, "seq of #{0}", i);

            if (i == 100L)
              break;
            else
              Assert.IsTrue(enumerator.MoveNext());
          }

          // FETCH
          server.EnqueueTaggedResponse(GetFetchResponses(101, 100));

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 101:200 (UID)\r\n"));

          for (var i = 101L;; i++) {
            Assert.AreEqual(i, enumerator.Current.Uid, "uid of #{0}", i);
            Assert.AreEqual(i, enumerator.Current.Sequence, "seq of #{0}", i);

            if (i == 200L)
              break;
            else
              Assert.IsTrue(enumerator.MoveNext());
          }

          // FETCH
          server.EnqueueTaggedResponse(GetFetchResponses(201, 50));

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 201:250 (UID)\r\n"));

          for (var i = 201L;; i++) {
            Assert.AreEqual(i, enumerator.Current.Uid, "uid of #{0}", i);
            Assert.AreEqual(i, enumerator.Current.Sequence, "seq of #{0}", i);

            if (i == 250L) {
              Assert.IsFalse(enumerator.MoveNext());
              break;
            }
            else {
              Assert.IsTrue(enumerator.MoveNext());
            }
          }
        }
      });
    }

    [Test]
    public void TestGetEnumeratorFetchStaticAttributes()
    {
      var selectResp =
        "* 3 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.StaticAttributes);

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          var bodystructure = "(\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL)";
          var envelope = "(" +
                         "\"Tue, 23 Mar 2010 09:46:52 +0900\" " +
                         "\"test message\" " + 
                         "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                         "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                         "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                         "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                         "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                         "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                         "\"<in.reply.to@mail.example.com>\" " + 
                         "\"<message.id@mail.example.com>\" " + 
                         ")";
          var internaldate = "\"23-Mar-2010 09:46:52 +0900\"";

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse(string.Format("* FETCH 1 (UID 2 BODYSTRUCTURE {0} ENVELOPE {1} INTERNALDATE {2} RFC822.SIZE 1024)\r\n", bodystructure, envelope, internaldate) + 
                                       string.Format("* FETCH 2 (UID 4 BODYSTRUCTURE {0} ENVELOPE {1} INTERNALDATE {2} RFC822.SIZE 2048)\r\n", bodystructure, envelope, internaldate) + 
                                       string.Format("* FETCH 3 (UID 6 BODYSTRUCTURE {0} ENVELOPE {1} INTERNALDATE {2} RFC822.SIZE 3072)\r\n", bodystructure, envelope, internaldate) + 
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:3 (UID BODYSTRUCTURE ENVELOPE INTERNALDATE RFC822.SIZE)\r\n"));

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(1, enumerator.Current.Sequence, "seq of #1");
          Assert.AreEqual(2, enumerator.Current.Uid, "uid of #1");
          Assert.IsNotNull(enumerator.Current.BodyStructure, "bodystructure of #1 is not null");
          Assert.IsNotNull(enumerator.Current.Envelope, "envelope of #1 is not null");
          Assert.AreEqual(new DateTimeOffset(2010, 3, 23, 9, 46, 52, TimeSpan.FromHours(+9.0)), enumerator.Current.InternalDate, "internaldate of #1");
          Assert.AreEqual(1024, enumerator.Current.Length, "length of #1");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(2, enumerator.Current.Sequence, "seq of #2");
          Assert.AreEqual(4, enumerator.Current.Uid, "uid of #2");
          Assert.IsNotNull(enumerator.Current.BodyStructure, "bodystructure of #2 is not null");
          Assert.IsNotNull(enumerator.Current.Envelope, "envelope of #2 is not null");
          Assert.AreEqual(new DateTimeOffset(2010, 3, 23, 9, 46, 52, TimeSpan.FromHours(+9.0)), enumerator.Current.InternalDate, "internaldate of #2");
          Assert.AreEqual(2048, enumerator.Current.Length, "length of #2");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(3, enumerator.Current.Sequence, "seq of #3");
          Assert.AreEqual(6, enumerator.Current.Uid, "uid of #3");
          Assert.IsNotNull(enumerator.Current.BodyStructure, "bodystructure of #3 is not null");
          Assert.IsNotNull(enumerator.Current.Envelope, "envelope of #3 is not null");
          Assert.AreEqual(new DateTimeOffset(2010, 3, 23, 9, 46, 52, TimeSpan.FromHours(+9.0)), enumerator.Current.InternalDate, "internaldate of #3");
          Assert.AreEqual(3072, enumerator.Current.Length, "length of #3");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetEnumeratorFetchDynamicAttributes()
    {
      var selectResp =
        "* 3 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes);

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 2 FLAGS (\\Seen))\r\n" +
                                       "* FETCH 2 (UID 4 FLAGS (\\Recent))\r\n" +
                                       "* FETCH 3 (UID 6 FLAGS (\\Answered \\Seen))\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:3 (UID FLAGS)\r\n"));

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(1, enumerator.Current.Sequence, "seq of #1");
          Assert.AreEqual(2, enumerator.Current.Uid, "uid of #1");
          Assert.IsNotNull(enumerator.Current.Flags, "flags of #1 is not null");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(2, enumerator.Current.Sequence, "seq of #2");
          Assert.AreEqual(4, enumerator.Current.Uid, "uid of #2");
          Assert.IsNotNull(enumerator.Current.Flags, "flags of #2 is not null");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(3, enumerator.Current.Sequence, "seq of #3");
          Assert.AreEqual(6, enumerator.Current.Uid, "uid of #3");
          Assert.IsNotNull(enumerator.Current.Flags, "flags of #3 is not null");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetEnumeratorFetchDynamicAttributesCondStoreCapable()
    {
      var selectResp =
        "* 1 EXISTS\r\n" +
        "* OK [HIGHESTMODSEQ 1]\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.CondStore}, "INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(1L, mailbox.ExistMessageCount);
        Assert.IsTrue(mailbox.IsModSequencesAvailable);

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes);

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 2 FLAGS (\\Seen) MODSEQ (1))\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1 (UID FLAGS MODSEQ)\r\n"));

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(1, enumerator.Current.Sequence, "seq of #1");
          Assert.AreEqual(2, enumerator.Current.Uid, "uid of #1");
          Assert.IsNotNull(enumerator.Current.Flags, "flags of #1 is not null");
          Assert.AreEqual(1UL, enumerator.Current.ModSeq, "modseq of #1");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetEnumeratorFetchDynamicAttributesCondStoreCapableNoModSeq()
    {
      var selectResp =
        "* 1 EXISTS\r\n" +
        "* OK [NOMODSEQ]\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.CondStore}, "INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(1L, mailbox.ExistMessageCount);
        Assert.IsFalse(mailbox.IsModSequencesAvailable);

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.DynamicAttributes);

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (UID 2 FLAGS (\\Seen))\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1 (UID FLAGS)\r\n"));

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(1, enumerator.Current.Sequence, "seq of #1");
          Assert.AreEqual(2, enumerator.Current.Uid, "uid of #1");
          Assert.IsNotNull(enumerator.Current.Flags, "flags of #1 is not null");
          Assert.AreEqual(0UL, enumerator.Current.ModSeq, "modseq of #1");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetEnumeratorFetchAllAttributes()
    {
      var selectResp =
        "* 1 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(1L, mailbox.ExistMessageCount);

        var messages = mailbox.GetMessages(ImapMessageFetchAttributeOptions.AllAttributes);

        using (var enumerator = messages.GetEnumerator()) {
          Assert.IsNotNull(enumerator);

          var bodystructure = "(\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL)";
          var envelope = "(" +
                         "\"Tue, 23 Mar 2010 09:46:52 +0900\" " +
                         "\"test message\" " + 
                         "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                         "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                         "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                         "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                         "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                         "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                         "\"<in.reply.to@mail.example.com>\" " + 
                         "\"<message.id@mail.example.com>\" " + 
                         ")";
          var internaldate = "\"23-Mar-2010 09:46:52 +0900\"";

          // NOOP
          server.EnqueueTaggedResponse("$tag OK done\r\n");
          // FETCH
          server.EnqueueTaggedResponse("* FETCH 1 (" +
                                       "UID 2 " +
                                       "BODYSTRUCTURE " + bodystructure +
                                       "ENVELOPE " + envelope +
                                       "INTERNALDATE " + internaldate +
                                       "RFC822.SIZE 1024 " +
                                       "FLAGS (\\Seen)" +
                                       ")\r\n" +
                                       "$tag OK done\r\n");

          Assert.IsTrue(enumerator.MoveNext());

          Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
          Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1 (UID BODYSTRUCTURE ENVELOPE INTERNALDATE RFC822.SIZE FLAGS)\r\n"));

          Assert.IsNotNull(enumerator.Current);
          Assert.AreEqual(1, enumerator.Current.Sequence, "seq of #1");
          Assert.AreEqual(2, enumerator.Current.Uid, "uid of #1");
          Assert.IsNotNull(enumerator.Current.BodyStructure, "bodystructure of #1 is not null");
          Assert.IsNotNull(enumerator.Current.Envelope, "envelope of #1 is not null");
          Assert.AreEqual(new DateTimeOffset(2010, 3, 23, 9, 46, 52, TimeSpan.FromHours(+9.0)), enumerator.Current.InternalDate, "internaldate of #1");
          Assert.AreEqual(1024, enumerator.Current.Length, "length of #1");
          Assert.IsNotNull(enumerator.Current.Flags, "flags of #1 is not null");
          Assert.AreEqual(0UL, enumerator.Current.ModSeq, "modseq of #1");

          Assert.IsFalse(enumerator.MoveNext());
        }
      });
    }

    [Test]
    public void TestGetEnumeratorMailboxClosed()
    {
      TestUtils.TestClosedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        var messages = mailbox.GetMessages();

        using (var enumerator = messages.GetEnumerator()) {
          try {
            enumerator.MoveNext();
            Assert.Fail("ImapMailboxClosedException not thrown");
          }
          catch (ImapMailboxClosedException ex) {
            Assert.IsNotNull(ex.Mailbox);

            Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMailboxClosedException deserialized) {
              Assert.IsNotNull(deserialized.Mailbox);
              Assert.AreEqual(ex.Mailbox, deserialized.Mailbox);
            });
          }
        }
      });
    }

    [Test]
    public void TestGetEnumeratorStatusUpdated()
    {
      var selectResp =
        "* 3 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.AreEqual(3L, mailbox.ExistMessageCount);

        // NOOP
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                     "* FETCH 2 (UID 2)\r\n" +
                                     "* FETCH 3 (UID 3)\r\n" +
                                     "* EXISTS 6\r\n" +
                                     "$tag OK done\r\n");

        var messages = mailbox.GetMessages().ToArray();

        Assert.That(server.DequeueRequest(), Text.EndsWith("NOOP\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("FETCH 1:3 (UID)\r\n"));

        Assert.AreEqual(6L, mailbox.ExistMessageCount);

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);

        Assert.AreEqual(3L, messages[2].Uid);
        Assert.AreEqual(3L, messages[2].Sequence);
      });
    }

    [Test]
    public void TestCopyToDestinationMailboxOfDifferentSession()
    {
      CopyOrMoveToDestinationMailboxOfDifferentSession(false);
    }

    [Test]
    public void TestMoveToDestinationMailboxOfDifferentSession()
    {
      CopyOrMoveToDestinationMailboxOfDifferentSession(true);
    }

    private void CopyOrMoveToDestinationMailboxOfDifferentSession(bool move)
    {
      var selectResp =
        "* 3 EXISTS\r\n" +
        "* OK [UIDVALIDITY 23]\r\n" +
        "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox("INBOX", selectResp, delegate(ImapPseudoServer serverSource, ImapOpenedMailboxInfo mailboxSource) {
        Assert.AreEqual(3L, mailboxSource.ExistMessageCount);

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
            mailboxSource.GetMessages().MoveTo(mailboxDest);
          else
            mailboxSource.GetMessages().CopyTo(mailboxDest);

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
