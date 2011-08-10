using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.IO;

namespace Smdn.Net.Imap4.Client {
  public class ImapMessageInfoTestsBase {
    internal protected void TestMessage(Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      TestUtils.TestOpenedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1)\r\n" +
                                     "$tag OK done\r\n");

        var message = mailbox.GetMessageByUid(1L);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (UID)\r\n"));

        Assert.AreEqual(1L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.IsFalse(message.IsDeleted);
        Assert.IsNotNull(message.Url);
        Assert.AreSame(mailbox, message.Mailbox);

        action(server, message);
      });
    }

    internal protected void TestMessageClosedMailbox(Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        // CLOSE
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        message.Mailbox.Close();

        Assert.That(server.DequeueRequest(), Text.EndsWith("CLOSE\r\n"));

        Assert.IsFalse(message.Mailbox.IsOpen);

        action(server, message);
      });
    }

    internal protected void TestMessageDeleted(Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        // NOOP
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "$tag OK done\r\n");

        message.Client.Refresh();

        server.DequeueRequest(); // NOOP

        Assert.IsTrue(message.IsDeleted);

        action(server, message);
      });
    }

    internal protected void TestMessage(string fetchResponse, ImapMessageFetchAttributeOptions options, Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      TestMessages(fetchResponse, options, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        action(server, messages.First());
      });
    }

    internal protected void TestMessages(string fetchResponse, ImapMessageFetchAttributeOptions options, Action<ImapPseudoServer, ImapMessageInfo[]> action)
    {
      var selectResp = "$tag OK [READ-WRITE] done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.ESearch, ImapCapability.Searchres},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // SEARCH
        server.EnqueueTaggedResponse("$tag OK done\r\n");
        // FETCH
        server.EnqueueTaggedResponse(fetchResponse);

        var messages = new List<ImapMessageInfo>(mailbox.GetMessages(ImapSearchCriteria.All, options));

        server.DequeueRequest(); // SEARCH
        server.DequeueRequest(); // FETCH

        action(server, messages.ToArray());
      });
    }
  }

  [TestFixture]
  public class ImapMessageInfoTests : ImapMessageInfoTestsBase {
    [Test]
    public void TestIsSeen()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS ())\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsFalse(messages[0].IsSeen);
        Assert.IsFalse(messages[0].Flags.Contains(ImapMessageFlag.Seen));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsSeen);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
      });
    }

    [Test]
    public void TestIsRecent()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS ())\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Recent))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsFalse(messages[0].IsRecent);
        Assert.IsFalse(messages[0].Flags.Contains(ImapMessageFlag.Recent));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsRecent);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Recent));
      });
    }

    [Test]
    public void TestIsMarkedAsDeleted()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS ())\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Deleted))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsFalse(messages[0].IsMarkedAsDeleted);
        Assert.IsFalse(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsFalse(messages[0].IsDeleted);

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsFalse(messages[0].IsDeleted);
      });
    }

    [Test]
    public void TestIsDeleted()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        // UID STORE
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted))\r\n" +
                                     "$tag OK done\r\n");
        // EXPUNGE
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "$tag OK done\r\n");

        messages[0].Delete();

        server.DequeueRequest(); // UID STORE
        server.DequeueRequest(); // EXPUNGE

        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, messages[0].Sequence);
        Assert.AreEqual(2, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[0].IsMarkedAsDeleted);
        Assert.IsTrue(messages[0].IsSeen);
        Assert.IsTrue(messages[0].IsDeleted);

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(1L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsFalse(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue(messages[1].IsSeen);
        Assert.IsFalse(messages[1].IsDeleted);
      });
    }

    [Test]
    public void TestIsAnswered()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS ())\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Answered))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsFalse(messages[0].IsAnswered);
        Assert.IsFalse(messages[0].Flags.Contains(ImapMessageFlag.Answered));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsAnswered);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Answered));
      });
    }

    [Test]
    public void TestIsDraft()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS ())\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Draft))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsFalse(messages[0].IsDraft);
        Assert.IsFalse(messages[0].Flags.Contains(ImapMessageFlag.Draft));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsDraft);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Draft));
      });
    }

    [Test]
    public void TestIsFlagged()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS ())\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Flagged))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsFalse(messages[0].IsFlagged);
        Assert.IsFalse(messages[0].Flags.Contains(ImapMessageFlag.Flagged));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsFlagged);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Flagged));
      });
    }

    [Test]
    public void TestEnvelopeSubject()
    {
      var envelope = "(" +
                     "\"Tue, 23 Mar 2010 09:46:52 +0900\" " +
                     "{0} " + 
                     "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                     "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                     "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                     "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                     "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                     "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                     "\"<in.reply.to@mail.example.com>\" " + 
                     "\"<message.id@mail.example.com>\" " + 
                     ")";

      var respLine = "* FETCH {0} (UID {0} BODYSTRUCTURE (\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL) " +
                     "ENVELOPE {1} " +
                     "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024)\r\n";

      var fetchResp =
        string.Format(respLine, 1L, string.Format(envelope, "\"not encoded\"")) +
        string.Format(respLine, 2L, string.Format(envelope, "NIL")) +
        string.Format(respLine, 3L, string.Format(envelope, "\"=?utf-8?b?bWltZSBlbmNvZGVk?=\"")) +
        string.Format(respLine, 4L, string.Format(envelope, "\"=?x?x?invalid format?=\"")) +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.AreEqual("not encoded", messages[0].EnvelopeSubject);
        Assert.AreEqual("not encoded", messages[0].EnvelopeSubject); // must return same value

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(null, messages[1].EnvelopeSubject);
        Assert.AreEqual(null, messages[1].EnvelopeSubject); // must return same value

        Assert.AreEqual(3L, messages[2].Uid);
        Assert.AreEqual("mime encoded", messages[2].EnvelopeSubject);
        Assert.AreEqual("mime encoded", messages[2].EnvelopeSubject); // must return same value

        Assert.AreEqual(4L, messages[3].Uid);
        Assert.AreEqual("=?x?x?invalid format?=", messages[3].EnvelopeSubject);
        Assert.AreEqual("=?x?x?invalid format?=", messages[3].EnvelopeSubject); // must return same value
      });
    }

    [Test]
    public void TestEnvelopeDate()
    {
      var envelope = "(" +
                     "{0} " +
                     "\"subject\" " + 
                     "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                     "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                     "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                     "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                     "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                     "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                     "\"<in.reply.to@mail.example.com>\" " + 
                     "\"<message.id@mail.example.com>\" " + 
                     ")";

      var respLine = "* FETCH {0} (UID {0} BODYSTRUCTURE (\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL) " +
                     "ENVELOPE {1} " +
                     "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024)\r\n";

      var fetchResp =
        string.Format(respLine, 1L, string.Format(envelope, "\"Tue, 23 Mar 2010 11:22:33 +0900\"")) +
        string.Format(respLine, 2L, string.Format(envelope, "NIL")) +
        string.Format(respLine, 3L, string.Format(envelope, "\"invalid date time format\"")) +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsTrue(messages[0].EnvelopeDate.HasValue);

        var expected = new DateTimeOffset(2010, 3, 23, 11, 22, 33, TimeSpan.FromHours(+9.0));

        Assert.AreEqual(expected, messages[0].EnvelopeDate.Value);
        Assert.AreEqual(expected, messages[0].EnvelopeDate.Value); // must return same value

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsFalse(messages[1].EnvelopeDate.HasValue);

        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsTrue(messages[2].EnvelopeDate.HasValue);

        expected = default(DateTimeOffset);

        Assert.AreEqual(expected, messages[2].EnvelopeDate.Value);
        Assert.AreEqual(expected, messages[2].EnvelopeDate.Value); // must return same value
      });
    }

    [Test]
    public void TestIsMultiPart()
    {
      var envelope = "(" +
                     "\"Thu, 25 Mar 2010 13:43:30 +0900\" " +
                     "\"subject\" " + 
                     "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                     "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                     "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                     "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                     "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                     "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                     "\"<in.reply.to@mail.example.com>\" " + 
                     "\"<message.id@mail.example.com>\" " + 
                     ") ";
      var respLine = "* FETCH {0} (UID {0} BODYSTRUCTURE {1} " +
                     "ENVELOPE " + envelope +
                     "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024)\r\n";

      var fetchResp =
        string.Format(respLine, 1L, "(\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL)") +
        string.Format(respLine, 2L, "((\"text\" \"plain\" (\"charset\" \"GB2312\") NIL NIL \"quoted-printable\" 1634 51 NIL NIL NIL NIL)" +
                                    "(\"text\" \"html\" (\"charset\" \"GB2312\") NIL NIL \"quoted-printable\" 5650 96 NIL NIL NIL NIL) " +
                                    "\"alternative\" (\"boundary\" \"----000000000000000000000000000000000000000000000000000000000000000\") NIL NIL NIL)") +
        string.Format(respLine, 3L, "((\"text\" \"plain\" (\"charset\" \"ISO-2022-JP\") NIL NIL \"7bit\" 1950 62 NIL NIL NIL NIL)" +
                                    "(\"text\" \"x-csrc\" (\"name\" \"polish.c\" \"charset\" \"us-ascii\") NIL NIL \"7bit\" 4752 246 NIL (\"inline\" (\"filename\" \"test.c\")) NIL NIL) " +
                                    "\"mixed\" (\"boundary\" \"------------070006080802040707000608\") NIL NIL NIL)") +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsNotNull(messages[0].BodyStructure);
        Assert.IsFalse(messages[0].IsMultiPart);
        Assert.IsTrue(MimeType.TextPlain.EqualsIgnoreCase(messages[0].MediaType));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsNotNull(messages[1].BodyStructure);
        Assert.IsTrue(messages[1].IsMultiPart);
        Assert.IsTrue(MimeType.MultipartAlternative.EqualsIgnoreCase(messages[1].MediaType));

        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsNotNull(messages[2].BodyStructure);
        Assert.IsTrue(messages[2].IsMultiPart);
        Assert.IsTrue(MimeType.MultipartMixed.EqualsIgnoreCase(messages[2].MediaType));
      });
    }

    [Test]
    public void TestMediaType()
    {
      var envelope = "(" +
                     "\"Thu, 25 Mar 2010 13:43:30 +0900\" " +
                     "\"subject\" " + 
                     "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                     "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                     "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                     "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                     "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                     "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                     "\"<in.reply.to@mail.example.com>\" " + 
                     "\"<message.id@mail.example.com>\" " + 
                     ") ";
      var respLine = "* FETCH {0} (UID {0} BODYSTRUCTURE (\"{1}\" \"{2}\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL) " +
                     "ENVELOPE " + envelope +
                     "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024)\r\n";

      var fetchResp =
        string.Format(respLine, 1L, "text", "plain") +
        string.Format(respLine, 2L, "TEXT", "PLAIN") +
        string.Format(respLine, 3L, "application", "octet-stream") +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1L, messages[0].Uid);
        Assert.IsNotNull(messages[0].BodyStructure);
        Assert.IsNotNull(messages[0].MediaType);
        Assert.AreEqual("text/plain", messages[0].MediaType.ToString());
        Assert.IsTrue(MimeType.TextPlain.EqualsIgnoreCase(messages[0].MediaType));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.IsNotNull(messages[1].BodyStructure);
        Assert.IsNotNull(messages[1].MediaType);
        Assert.AreEqual("TEXT/PLAIN", messages[1].MediaType.ToString());
        Assert.IsTrue(MimeType.TextPlain.EqualsIgnoreCase(messages[1].MediaType));

        Assert.AreEqual(3L, messages[2].Uid);
        Assert.IsNotNull(messages[2].BodyStructure);
        Assert.IsNotNull(messages[2].MediaType);
        Assert.AreEqual("application/octet-stream", messages[2].MediaType.ToString());
        Assert.IsTrue(MimeType.ApplicationOctetStream.EqualsIgnoreCase(messages[2].MediaType));
      });
    }

    [Test]
    public void TestCopyToDestinationNameDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          message.CopyTo("INBOX.Trash");
          Assert.Fail("ImapMessageDeletedException not thrown");
        }
        catch (ImapMessageDeletedException ex) {
          Assert.IsNotNull(ex.DeletedMessage);
          Assert.AreSame(message, ex.DeletedMessage);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageDeletedException deserialized) {
            Assert.IsNull(deserialized.DeletedMessage);
          });
        }
      });
    }

    [Test]
    public void TestCopyToDestinationMailboxDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        // LIST
        server.EnqueueTaggedResponse("* LIST () \"\" INBOX.Trash\r\n" +
                                     "$tag OK done\r\n");

        var destMailbox = message.Mailbox.Client.GetMailbox("INBOX.Trash");

        server.DequeueRequest();

        try {
          message.CopyTo(destMailbox);
          Assert.Fail("ImapMessageDeletedException not thrown");
        }
        catch (ImapMessageDeletedException ex) {
          Assert.IsNotNull(ex.DeletedMessage);
          Assert.AreSame(message, ex.DeletedMessage);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageDeletedException deserialized) {
            Assert.IsNull(deserialized.DeletedMessage);
          });
        }
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
      TestMessage(delegate(ImapPseudoServer serverSource, ImapMessageInfo messageSource) {
        TestUtils.TestAuthenticated(delegate(ImapPseudoServer serverDest, ImapClient clientDest) {
          // LIST
          serverDest.EnqueueTaggedResponse("* LIST () \"\" dest\r\n" +
                                           "$tag OK done\r\n");

          var mailboxDest = clientDest.GetMailbox("dest");

          serverDest.DequeueRequest();

          Assert.AreNotSame(messageSource.Client, mailboxDest.Client);

          // FETCH (source)
          serverSource.EnqueueTaggedResponse("* FETCH 1 (" +
                                             "FLAGS (\\Answered $label1) " +
                                             "INTERNALDATE \"25-Jan-2011 15:29:06 +0900\" " +
                                             "RFC822.SIZE 12 " +
                                             "BODY[] {12}\r\ntest message)\r\n" +
                                             "$tag OK done\r\n");

          if (move)
            // UID STORE (source)
            serverSource.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Deleted))\r\n" +
                                               "$tag OK done\r\n");

          // APPEND (dest)
          serverDest.EnqueueResponse("+ OK continue\r\n");
          serverDest.EnqueueTaggedResponse("$tag OK [APPENDUID 38505 3955] APPEND completed\r\n");

          if (move)
            messageSource.MoveTo(mailboxDest);
          else
            messageSource.CopyTo(mailboxDest);

          Assert.That(serverSource.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE INTERNALDATE FLAGS BODY.PEEK[]<0.10240>)\r\n"));

          if (move) {
            Assert.That(serverSource.DequeueRequest(), Text.EndsWith("UID STORE 1 +FLAGS (\\Deleted)\r\n"));

            Assert.IsNotNull(messageSource.Flags);
            Assert.AreEqual(1, messageSource.Flags.Count);
            Assert.IsTrue(messageSource.Flags.Contains(ImapMessageFlag.Deleted));
            Assert.IsTrue(messageSource.IsMarkedAsDeleted);
          }

          Assert.That(serverDest.DequeueRequest(),
                      Text.EndsWith("APPEND dest (\\Answered $label1) \"25-Jan-2011 15:29:06 +0900\" {12}\r\n"));
          Assert.That(serverDest.DequeueAll(),
                      Text.StartsWith("test message"));
        });
      });
    }
  }
}
