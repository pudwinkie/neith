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
  [TestFixture]
  public class ImapMessageInfoTests {
    private void TestMessage(Action<ImapPseudoServer, ImapMessageInfo> action)
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

    private void TestMessageClosedMailbox(Action<ImapPseudoServer, ImapMessageInfo> action)
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

    private void TestMessageDeleted(Action<ImapPseudoServer, ImapMessageInfo> action)
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

    private void TestMessage(string fetchResponse, ImapMessageFetchAttributeOptions options, Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      TestMessages(fetchResponse, options, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        action(server, messages.First());
      });
    }

    private void TestMessages(string fetchResponse, ImapMessageFetchAttributeOptions options, Action<ImapPseudoServer, ImapMessageInfo[]> action)
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

    [Test]
    public void TestEnsureDynamicAttributesFetched()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(message.IsSeen);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(1, message.Flags.Count);
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(message.IsSeen);
      });
    }

    [Test]
    public void TestEnsureDynamicAttributesFetchedMailboxClosed()
    {
      TestMessageClosedMailbox(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          Assert.Fail("expected exception not thrown: {0}", message.IsSeen);
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
    public void TestEnsureDynamicAttributesFetchedDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          Assert.Fail("expected exception not thrown: {0}", message.IsSeen);
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
    public void TestEnsureDynamicAttributesFetchedVanished()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.AreNotEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsFalse(message.IsDeleted);

        // FETCH
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          Assert.Fail("ImapMessageDeletedException not thrown: {0}", message.IsSeen);
        }
        catch (ImapMessageDeletedException ex) {
          Assert.IsNotNull(ex.DeletedMessage);
          Assert.AreSame(message, ex.DeletedMessage);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageDeletedException deserialized) {
            Assert.IsNull(deserialized.DeletedMessage);
          });
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsTrue(message.IsDeleted);
      });
    }

    [Test, Ignore("this will never happen")]
    public void TestEnsureDynamicAttributesFetchedSequenceChanged()
    {
      var fetchResp =
        "* FETCH 1 (UID 1)\r\n" +
        "* FETCH 2 (UID 2)\r\n" +
        "* FETCH 3 (UID 3)\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.None,
                   delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        // FETCH
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "* EXISTS 1\r\n" +
                                     "* FETCH 1 (UID 3 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        Assert.IsTrue(messages[2].IsSeen);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 3 (FLAGS)\r\n"));

        Assert.IsTrue (messages[0].IsDeleted);
        Assert.IsTrue (messages[1].IsDeleted);
        Assert.IsFalse(messages[2].IsDeleted);

        var message = messages[2];

        Assert.AreEqual(3L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(1, message.Flags.Count);
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(message.IsSeen);
      });
    }

    [Test]
    public void TestEnsureStaticAttributesFetched()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
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

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (" +
                                     "UID 1 " +
                                     "BODYSTRUCTURE " + bodystructure +
                                     "ENVELOPE " + envelope +
                                     "INTERNALDATE " + internaldate +
                                     "RFC822.SIZE 1024 " +
                                     ")\r\n" +
                                     "$tag OK done\r\n");

        Assert.AreEqual(1024L, message.Length);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (BODYSTRUCTURE ENVELOPE INTERNALDATE RFC822.SIZE)\r\n"));

        Assert.IsNotNull(message.BodyStructure);
        Assert.IsNotNull(message.Envelope);
        Assert.AreNotEqual(default(DateTimeOffset), message.InternalDate);
      });
    }

    [Test]
    public void TestEnsureStaticAttributesFetchedMailboxClosed()
    {
      TestMessageClosedMailbox(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          Assert.Fail("expected exception not thrown: {0}", message.Length);
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
    public void TestEnsureStaticAttributesFetchedDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          Assert.Fail("expected exception not thrown: {0}", message.Length);
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
    public void TestEnsureStaticAttributesFetchedVanished()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.AreNotEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsFalse(message.IsDeleted);

        // FETCH
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          Assert.Fail("ImapMessageDeletedException not thrown: {0}", message.Length);
        }
        catch (ImapMessageDeletedException ex) {
          Assert.IsNotNull(ex.DeletedMessage);
          Assert.AreSame(message, ex.DeletedMessage);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageDeletedException deserialized) {
            Assert.IsNull(deserialized.DeletedMessage);
          });
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (BODYSTRUCTURE ENVELOPE INTERNALDATE RFC822.SIZE)\r\n"));

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsTrue(message.IsDeleted);
      });
    }

    [Test, Ignore("this will never happen")]
    public void TestEnsureStaticAttributesFetchedSequenceChanged()
    {
      var fetchResp =
        "* FETCH 1 (UID 1)\r\n" +
        "* FETCH 2 (UID 2)\r\n" +
        "* FETCH 3 (UID 3)\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.None,
                   delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
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

        // FETCH
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "* EXISTS 1\r\n" +
                                     "* FETCH 1 (" +
                                     "UID 3 " +
                                     "BODYSTRUCTURE " + bodystructure +
                                     "ENVELOPE " + envelope +
                                     "INTERNALDATE " + internaldate +
                                     "RFC822.SIZE 1024 " +
                                     ")\r\n" +
                                     "$tag OK done\r\n");

        Assert.AreEqual(1024L, messages[2].Length);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 3 (BODYSTRUCTURE ENVELOPE INTERNALDATE RFC822.SIZE)\r\n"));

        Assert.IsTrue (messages[0].IsDeleted);
        Assert.IsTrue (messages[1].IsDeleted);
        Assert.IsFalse(messages[2].IsDeleted);

        var message = messages[2];

        Assert.AreEqual(3L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.IsNotNull(message.BodyStructure);
        Assert.IsNotNull(message.Envelope);
        Assert.AreEqual(new DateTimeOffset(2010, 03, 23, 09, 46, 52, TimeSpan.FromHours(+9)),
                        message.InternalDate);
      });
    }

    [Test]
    public void TestStoreDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          message.Store(ImapStoreDataItem.AddFlags(ImapMessageFlag.Seen));
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
        Assert.IsFalse(messages[0].Flags.Has(ImapMessageFlag.Seen));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsSeen);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Seen));
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
        Assert.IsFalse(messages[0].Flags.Has(ImapMessageFlag.Recent));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsRecent);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Recent));
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
        Assert.IsFalse(messages[0].Flags.Has(ImapMessageFlag.Deleted));
        Assert.IsFalse(messages[0].IsDeleted);

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsMarkedAsDeleted);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Deleted));
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
        Assert.IsTrue(messages[0].Flags.Has(ImapMessageFlag.Deleted));
        Assert.IsTrue(messages[0].Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(messages[0].IsMarkedAsDeleted);
        Assert.IsTrue(messages[0].IsSeen);
        Assert.IsTrue(messages[0].IsDeleted);

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(1L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Seen));
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
        Assert.IsFalse(messages[0].Flags.Has(ImapMessageFlag.Answered));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsAnswered);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Answered));
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
        Assert.IsFalse(messages[0].Flags.Has(ImapMessageFlag.Draft));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsDraft);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Draft));
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
        Assert.IsFalse(messages[0].Flags.Has(ImapMessageFlag.Flagged));

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(1, messages[1].Flags.Count);
        Assert.IsTrue(messages[1].IsFlagged);
        Assert.IsTrue(messages[1].Flags.Has(ImapMessageFlag.Flagged));
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
    public void TestRefresh()
    {
      var envelope = "(" +
                     "\"Tue, 23 Mar 2010 11:22:33 +0900\" " +
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
      var fetchResp = "* FETCH 1 (UID 1 BODYSTRUCTURE (\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL) " +
                      "ENVELOPE " + envelope + " " +
                      "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024 FLAGS (\\Seen))\r\n" +
                      "$tag OK done\r\n";

      TestMessage(fetchResp, ImapMessageFetchAttributeOptions.AllAttributes, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.IsTrue(message.IsSeen);
        Assert.IsFalse(message.IsMarkedAsDeleted);
        Assert.IsNotNull(message.Envelope);
        Assert.IsNotNull(message.BodyStructure);

        var currentEnvelope = message.Envelope;
        var currentBodystructure = message.BodyStructure;

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted))\r\n" +
                                     "$tag OK done\r\n");

        message.Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.IsMarkedAsDeleted);

        Assert.AreSame(currentEnvelope, message.Envelope);
        Assert.AreSame(currentBodystructure, message.BodyStructure);
      });
    }

    [Test]
    public void TestRefreshMailboxClosed()
    {
      TestMessageClosedMailbox(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          message.Refresh();
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
    public void TestRefreshDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          message.Refresh();
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
    public void TestRefreshVanished()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.AreNotEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsFalse(message.IsDeleted);

        // FETCH
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          message.Refresh();
          Assert.Fail("ImapMessageDeletedException not thrown");
        }
        catch (ImapMessageDeletedException ex) {
          Assert.IsNotNull(ex.DeletedMessage);
          Assert.AreSame(message, ex.DeletedMessage);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageDeletedException deserialized) {
            Assert.IsNull(deserialized.DeletedMessage);
          });
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsTrue(message.IsDeleted);
      });
    }

    [Test]
    public void TestRefreshAttributesNotFetched()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        message.Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(1, message.Flags.Count);
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(message.IsSeen);
      });
    }

    [Test]
    public void TestRefreshDynamicAttributeNotFetched()
    {
      var envelope = "(" +
                     "\"Tue, 23 Mar 2010 11:22:33 +0900\" " +
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
      var fetchResp = "* FETCH 1 (UID 1 BODYSTRUCTURE (\"text\" \"plain\" NIL NIL NIL \"7bit\" 2109 54 NIL NIL NIL NIL) " +
                      "ENVELOPE " + envelope + " " +
                      "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024)\r\n" +
                      "$tag OK done\r\n";

      TestMessage(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.IsNotNull(message.Envelope);
        Assert.IsNotNull(message.BodyStructure);

        var currentEnvelope = message.Envelope;
        var currentBodystructure = message.BodyStructure;

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted))\r\n" +
                                     "$tag OK done\r\n");

        message.Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(2, message.Flags.Count);
        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.IsMarkedAsDeleted);

        Assert.AreSame(currentEnvelope, message.Envelope);
        Assert.AreSame(currentBodystructure, message.BodyStructure);
      });
    }

    [Test]
    public void TestRefreshCondStoreCapable()
    {
      var selectResp =
        "* OK [HIGHESTMODSEQ 1]\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.CondStore},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.IsTrue(mailbox.IsModSequencesAvailable);

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen) MODSEQ (1))\r\n" +
                                     "$tag OK done\r\n");

        var message = mailbox.GetMessageByUid(1L, ImapMessageFetchAttributeOptions.DynamicAttributes);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (UID FLAGS MODSEQ)\r\n"));

        Assert.IsTrue(message.IsSeen);
        Assert.IsFalse(message.IsMarkedAsDeleted);
        Assert.AreEqual(1L, message.ModSeq);

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted) MODSEQ (2))\r\n" +
                                     "$tag OK done\r\n");

        message.Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS MODSEQ)\r\n"));

        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.IsMarkedAsDeleted);
        Assert.AreEqual(2L, message.ModSeq);
      });
    }

    [Test]
    public void TestRefreshCondStoreCapableNoModSeq()
    {
      var selectResp =
        "* OK [NOMODSEQ]\r\n" +
        "* OK [READ-WRITE]\r\n" +
        "$tag OK done\r\n";

      TestUtils.TestOpenedMailbox(new[] {ImapCapability.CondStore},
                                  "INBOX",
                                  selectResp,
                                  delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        Assert.IsFalse(mailbox.IsModSequencesAvailable);

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        var message = mailbox.GetMessageByUid(1L, ImapMessageFetchAttributeOptions.DynamicAttributes);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (UID FLAGS)\r\n"));

        Assert.IsTrue(message.IsSeen);
        Assert.IsFalse(message.IsMarkedAsDeleted);

        // FETCH
        server.EnqueueTaggedResponse("* FETCH 1 (FLAGS (\\Seen \\Deleted))\r\n" +
                                     "$tag OK done\r\n");

        message.Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));

        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.IsMarkedAsDeleted);
      });
    }

    [Test, Ignore("this will never happen")]
    public void TestRefreshSequenceChanged()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
        "* FETCH 3 (UID 3 FLAGS (\\Recent))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes,
                   delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.AreEqual(1, messages[2].Flags.Count);
        Assert.IsTrue(messages[2].Flags.Has(ImapMessageFlag.Recent));
        Assert.IsTrue(messages[2].IsRecent);

        // FETCH
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "* EXISTS 1\r\n" +
                                     "* FETCH 1 (UID 3 FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        messages[2].Refresh();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 3 (FLAGS)\r\n"));

        Assert.IsTrue (messages[0].IsDeleted);
        Assert.IsTrue (messages[1].IsDeleted);
        Assert.IsFalse(messages[2].IsDeleted);

        var message = messages[2];

        Assert.AreEqual(3L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.IsNotNull(message.Flags);
        Assert.AreEqual(1, message.Flags.Count);
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(message.IsSeen);
      });
    }

    [Test]
    public void TestToggleFlags()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS (\\Seen $label1))\r\n" +
        "$tag OK done\r\n";

      TestMessage(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.AreEqual(2, message.Flags.Count);
        Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue(message.Flags.Has("$label1"));

        // STORE
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS ($label1))\r\n" +
                                     "$tag OK done\r\n");
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS (\\Draft $label1))\r\n" +
                                     "$tag OK done\r\n");

        message.ToggleFlags(ImapMessageFlag.Seen, ImapMessageFlag.Draft);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 -FLAGS (\\Seen)\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 +FLAGS (\\Draft)\r\n"));

        Assert.AreEqual(2, message.Flags.Count);
        Assert.IsFalse(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsTrue (message.Flags.Has(ImapMessageFlag.Draft));
        Assert.IsTrue (message.Flags.Has("$label1"));
        Assert.IsFalse(message.IsSeen);

        // STORE
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS (\\Draft))\r\n" +
                                     "$tag OK done\r\n");
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS (\\Draft $label2 $label3))\r\n" +
                                     "$tag OK done\r\n");

        message.ToggleKeywords("$label1", "$label2", "$label3");

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 -FLAGS ($label1)\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 +FLAGS ($label2 $label3)\r\n"));

        Assert.AreEqual(3, message.Flags.Count);
        Assert.IsFalse(message.Flags.Has("$label1"));
        Assert.IsTrue (message.Flags.Has("$label2"));
        Assert.IsTrue (message.Flags.Has("$label3"));
        Assert.IsTrue (message.Flags.Has(ImapMessageFlag.Draft));
        Assert.IsFalse(message.IsSeen);
      });
    }

    [Test]
    public void TestToggleFlagsDynamicAttributeNotFetched()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        // FETCH
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");
        // STORE
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS ())\r\n" +
                                     "$tag OK done\r\n");

        message.ToggleFlags(ImapMessageFlag.Seen);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (FLAGS)\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 -FLAGS (\\Seen)\r\n"));

        Assert.AreEqual(0, message.Flags.Count);
        Assert.IsFalse(message.Flags.Has(ImapMessageFlag.Seen));
        Assert.IsFalse(message.IsSeen);
      });
    }

    private void TestFetchMultipartBodystructure(Action<ImapMessageInfo> action)
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
      var bodystructure = "(" +
                          "(\"text\" \"plain\" (\"charset\" \"ISO-2022-JP\") NIL NIL \"7bit\" 6 1 NIL NIL NIL NIL)" +
                          "(\"message\" \"rfc822\" (\"name\" \"=?ISO-2022-JP?B?GyRCRTpJVSVhJUMlOyE8JTgbKEI=?=\") " +
                          "NIL NIL \"7bit\" 188 (NIL \"test mail\" NIL NIL NIL NIL NIL NIL NIL NIL) " +
                          "(\"text\" \"plain\" (\"charset\" \"us-ascii\") NIL NIL \"7bit\" 121 6 NIL NIL NIL NIL) 10 NIL " +
                          "(\"inline\" (\"filename\" \"ISO-2022-JP''%1B%24%42%45%3A%49%55%25%61%25%43%25%3B%21%3C%25%38%1B%28%42\")) NIL NIL) " +
                          "\"mixed\" (\"boundary\" \"------------040401080108050302040809\") NIL NIL NIL" +
                          ") ";

      var fetchResp = "* FETCH 1 (UID 1 " + 
                      "BODYSTRUCTURE " + bodystructure +
                      "ENVELOPE " + envelope +
                      "INTERNALDATE \"23-Mar-2010 11:22:33 +0900\" RFC822.SIZE 1024)\r\n" +
                      "$tag OK done\r\n";

      TestMessage(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        action(message);
      });
    }

    [Test]
    public void TestGetStructureOfSectionInInt()
    {
      TestFetchMultipartBodystructure(delegate(ImapMessageInfo message) {
        Assert.IsNotNull(message.BodyStructure);

        Assert.AreSame(message.BodyStructure, message.GetStructureOf((int[])null));
        Assert.AreSame(message.BodyStructure, message.GetStructureOf(new int[0]));

        Assert.AreSame(message.BodyStructure.FindSection("1"), message.GetStructureOf(1));
        Assert.AreEqual("text/plain", (string)message.GetStructureOf(1).MediaType);
        Assert.AreSame(message.BodyStructure.FindSection("2"), message.GetStructureOf(2));
        Assert.AreEqual("message/rfc822", (string)message.GetStructureOf(2).MediaType);
        Assert.AreSame(message.BodyStructure.FindSection("2.1"), message.GetStructureOf(2, 1));
        Assert.AreEqual("text/plain", (string)message.GetStructureOf(2, 1).MediaType);

        Assert.IsNull(message.GetStructureOf(2, 2));
        Assert.IsNull(message.GetStructureOf(3));
      });
    }

    [Test]
    public void TestGetStructureOfSectionInString()
    {
      TestFetchMultipartBodystructure(delegate(ImapMessageInfo message) {
        Assert.IsNotNull(message.BodyStructure);

        Assert.AreSame(message.BodyStructure, message.GetStructureOf((string)null));
        Assert.AreSame(message.BodyStructure, message.GetStructureOf(string.Empty));

        Assert.AreSame(message.BodyStructure.FindSection("1"), message.GetStructureOf("1"));
        Assert.AreEqual("text/plain", (string)message.GetStructureOf("1").MediaType);
        Assert.AreSame(message.BodyStructure.FindSection("2"), message.GetStructureOf("2"));
        Assert.AreEqual("message/rfc822", (string)message.GetStructureOf("2").MediaType);
        Assert.AreSame(message.BodyStructure.FindSection("2.1"), message.GetStructureOf("2.1"));
        Assert.AreEqual("text/plain", (string)message.GetStructureOf("2.1").MediaType);

        Assert.IsNull(message.GetStructureOf("2.2"));
        Assert.IsNull(message.GetStructureOf("3"));
      });
    }

    [Test]
    public void TestGetStructureOfSectionInIntGeneric()
    {
      TestFetchMultipartBodystructure(delegate(ImapMessageInfo message) {
        Assert.IsNotNull(message.BodyStructure);

        Assert.IsNotNull(message.GetStructureOf<ImapMultiPartBodyStructure>((int[])null));
        Assert.IsNotNull(message.GetStructureOf<ImapSinglePartBodyStructure>(1));
        Assert.IsNotNull(message.GetStructureOf<ImapExtendedMessageRfc822BodyStructure>(2));
        Assert.IsNotNull(message.GetStructureOf<ImapSinglePartBodyStructure>(2, 1));

        TestUtils.ExpectExceptionThrown<InvalidCastException>(delegate {
          message.GetStructureOf<ImapSinglePartBodyStructure>((int[])null);
        });

        TestUtils.ExpectExceptionThrown<InvalidCastException>(delegate {
          message.GetStructureOf<ImapMultiPartBodyStructure>(1);
        });
      });
    }

    [Test]
    public void TestGetStructureOfSectionInStringGeneric()
    {
      TestFetchMultipartBodystructure(delegate(ImapMessageInfo message) {
        Assert.IsNotNull(message.BodyStructure);

        Assert.IsNotNull(message.GetStructureOf<ImapMultiPartBodyStructure>((string)null));
        Assert.IsNotNull(message.GetStructureOf<ImapSinglePartBodyStructure>("1"));
        Assert.IsNotNull(message.GetStructureOf<ImapExtendedMessageRfc822BodyStructure>("2"));
        Assert.IsNotNull(message.GetStructureOf<ImapSinglePartBodyStructure>("2.1"));

        TestUtils.ExpectExceptionThrown<InvalidCastException>(delegate {
          message.GetStructureOf<ImapSinglePartBodyStructure>((string)null);
        });

        TestUtils.ExpectExceptionThrown<InvalidCastException>(delegate {
          message.GetStructureOf<ImapMultiPartBodyStructure>("1");
        });
      });
    }

    private void TestFetchMessage(Action<ImapPseudoServer, ImapMessageInfo, string, int> action)
    {
      TestFetchMessage(false, string.Empty, action);
    }

    private void TestFetchMessage(bool setSeen, string initialFlags, Action<ImapPseudoServer, ImapMessageInfo, string, int> action)
    {
      const string messageBody = @"Return-Path: <test@example.net>
X-Original-To: test@example.net
Delivered-To: test@example.net
Message-ID: <4BAA561E.5000004@test@example.net>
Date: Thu, 25 Mar 2010 03:12:46 +0900
From: test <test@example.net>
Reply-To: test@example.net
User-Agent: Thunderbird 2.0.0.24 (X11/20100317)
MIME-Version: 1.0
To: hoge@example.net
Subject: =?ISO-2022-JP?B?GyRCJUYlOSVIGyhC?=
Content-Type: text/plain; charset=ISO-2022-JP
Content-Transfer-Encoding: 7bit

test message

3 uid fetch 6 (body[])
* 6 FETCH (UID 6 BODY[] {1476}
hogehogehoge
)
3 OK Fetch completed.
";

      TestUtils.TestOpenedMailbox("INBOX", delegate(ImapPseudoServer server, ImapOpenedMailboxInfo mailbox) {
        // FETCH
        server.EnqueueTaggedResponse(string.Format("* FETCH 1 (UID 1 FLAGS ({0}))\r\n", initialFlags ?? string.Empty) +
                                     "$tag OK done\r\n");

        var message = mailbox.GetMessageByUid(1L, ImapMessageFetchAttributeOptions.DynamicAttributes);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (UID FLAGS)\r\n"));

        var octets = NetworkTransferEncoding.Transfer8Bit.GetByteCount(messageBody);

        server.EnqueueTaggedResponse(string.Format("* {0} FETCH (RFC822.SIZE {1} {3}BODY[] {{{1}}}\r\n{2})\r\n",
                                                   message.Uid,
                                                   octets,
                                                   messageBody,
                                                   setSeen ? "FLAGS (\\Seen) " : string.Empty) +
                                     "$tag OK done\r\n");

        action(server, message, messageBody, octets);
      });
    }

    [Test]
    public void TestOpenRead()
    {
      TestFetchMessage(false,
                       null,
                       delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.IsFalse(message.IsSeen);

        using (var stream = message.OpenRead()) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

          Assert.IsNotNull(stream);
          Assert.AreEqual(expectedOctets, stream.Length);
          Assert.AreEqual(expectedMessageBody,
                          (new StreamReader(stream, NetworkTransferEncoding.Transfer8Bit)).ReadToEnd());

          Assert.IsFalse(message.Flags.Has(ImapMessageFlag.Seen));
          Assert.IsFalse(message.IsSeen);
        }
      });
    }

    [Test]
    public void TestOpenReadSetSeen()
    {
      TestFetchMessage(true,
                       null,
                       delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.IsFalse(message.IsSeen);

        using (var stream = message.OpenRead(ImapMessageFetchBodyOptions.SetSeen)) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY[]<0.10240>)\r\n"));

          Assert.IsNotNull(stream);
          Assert.AreEqual(expectedOctets, stream.Length);
          Assert.AreEqual(expectedMessageBody,
                          (new StreamReader(stream, NetworkTransferEncoding.Transfer8Bit)).ReadToEnd());

          Assert.IsTrue(message.Flags.Has(ImapMessageFlag.Seen));
          Assert.IsTrue(message.IsSeen);
        }
      });
    }

    [Test]
    public void TestOpenReadFlagsNotChange()
    {
      TestFetchMessage(false,
                       "\\Seen $label1",
                       delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.AreEqual(2, message.Flags.Count);
        Assert.IsTrue(message.IsSeen);
        Assert.IsTrue(message.Flags.Has("$label1"));

        using (var stream = message.OpenRead(ImapMessageFetchBodyOptions.SetSeen)) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY[]<0.10240>)\r\n"));

          Assert.IsNotNull(stream);
          Assert.AreEqual(expectedOctets, stream.Length);
          Assert.AreEqual(expectedMessageBody,
                          (new StreamReader(stream, NetworkTransferEncoding.Transfer8Bit)).ReadToEnd());

          Assert.AreEqual(2, message.Flags.Count);
          Assert.IsTrue(message.IsSeen);
          Assert.IsTrue(message.Flags.Has("$label1"));
        }
      });
    }

    private void TestMultipartMessage(Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      TestMultipartMessage("utf-8", action);
    }

    private void TestMultipartMessage(string bodystructureCharset, Action<ImapPseudoServer, ImapMessageInfo> action)
    {
      var charset = bodystructureCharset == null
        ? "NIL"
        : string.Format("(\"charset\" \"{0}\")", bodystructureCharset);

      var fetchResp = "* FETCH 1 ( " +
                      "UID 1 " +
                      "BODYSTRUCTURE (" +
                      string.Format("(\"text\" \"plain\" {0} NIL NIL \"base64\" 28 0 NIL NIL NIL NIL)", charset) +
                      string.Format("(\"text\" \"plain\" {0} NIL NIL \"quoted-printable\" 35 0 NIL NIL NIL NIL) ", charset) +
                      "\"mixed\" (\"charset\" \"us-ascii\" \"boundary\" \"----------------634051269909345790\") NIL NIL NIL" +
                      ") " +
                      "INTERNALDATE \"25-Mar-2010 15:16:31 +0900\" RFC822.SIZE 651 ENVELOPE (NIL \"multipart\" NIL NIL NIL NIL NIL NIL NIL NIL))\r\n" +
                      "$tag OK done\r\n";

      TestMessage(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.IsTrue(message.IsMultiPart);

        var multipart = message.BodyStructure as ImapMultiPartBodyStructure;

        Assert.AreEqual("base64",
                        (multipart.NestedStructures[0] as ImapSinglePartBodyStructure).Encoding);
        Assert.AreEqual("quoted-printable",
                        (multipart.NestedStructures[1] as ImapSinglePartBodyStructure).Encoding);

        action(server, message);
      });
    }

    [Test]
    public void TestOpenReadWithMultiPartBodyStructure()
    {
      TestMultipartMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        var bodystructure = message.BodyStructure;

        Assert.IsTrue(bodystructure.IsMultiPart);

        var fetchBodyResp = @"* 1 FETCH (BODY[] {651}
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Type: multipart/mixed; charset=""us-ascii""; boundary=""----------------634051269909345790""
Subject: multipart

------------------634051269909345790
MIME-Version: 1.0
Content-Transfer-Encoding: base64
Content-Type: text/plain; charset=""utf-8""
Subject: =?iso-2022-jp?b?GyRCN29MPhsoQg==?=

YmFzZTY0IGVuY29kZWQg5pys5paH
------------------634051269909345790
MIME-Version: 1.0
Content-Transfer-Encoding: quoted-printable
Content-Type: text/plain; charset=""utf-8""
Subject: =?iso-2022-jp?q?=1B$B7oL>=1B(B?=

quoted-printable =E6=9C=AC=E6=96=87
------------------634051269909345790--
)
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        // FETCH
        server.EnqueueTaggedResponse(fetchBodyResp +
                                     "$tag OK done\r\n");

        var expected = @"MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Type: multipart/mixed; charset=""us-ascii""; boundary=""----------------634051269909345790""
Subject: multipart

------------------634051269909345790
MIME-Version: 1.0
Content-Transfer-Encoding: base64
Content-Type: text/plain; charset=""utf-8""
Subject: =?iso-2022-jp?b?GyRCN29MPhsoQg==?=

YmFzZTY0IGVuY29kZWQg5pys5paH
------------------634051269909345790
MIME-Version: 1.0
Content-Transfer-Encoding: quoted-printable
Content-Type: text/plain; charset=""utf-8""
Subject: =?iso-2022-jp?q?=1B$B7oL>=1B(B?=

quoted-printable =E6=9C=AC=E6=96=87
------------------634051269909345790--
".Replace("\r\n", "\n").Replace("\n", "\r\n");

          Assert.AreEqual(expected,
                          message.ReadAllText(bodystructure));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestOpenReadWithSinglePartBodyStructure()
    {
      TestMultipartMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        var bodystructure = message.GetStructureOf<ImapSinglePartBodyStructure>(1);

        var fetchBodyResp = @"* 1 FETCH (BODY[1] {28}
YmFzZTY0IGVuY29kZWQg5pys5paH)
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        // FETCH
        server.EnqueueTaggedResponse(fetchBodyResp +
                                     "$tag OK done\r\n");

        Assert.AreEqual("YmFzZTY0IGVuY29kZWQg5pys5paH",
                        message.ReadAllText(bodystructure));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (BODY.PEEK[1]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestOpenReadWithSinglePartBodyStructureDecodeContentEncodingBase64()
    {
      TestMultipartMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        var bodystructure = message.GetStructureOf<ImapSinglePartBodyStructure>(1);

        var fetchBodyResp = @"* 1 FETCH (BODY[1] {28}
YmFzZTY0IGVuY29kZWQg5pys5paH)
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        // FETCH
        server.EnqueueTaggedResponse(fetchBodyResp +
                                     "$tag OK done\r\n");

        Assert.AreEqual("base64 encoded 本文",
                        message.ReadAllText(bodystructure, ImapMessageFetchBodyOptions.DecodeContent));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (BODY.PEEK[1]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestOpenReadWithSinglePartBodyStructureDecodeContentEncodingQuotedPrintable()
    {
      TestMultipartMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        var bodystructure = message.GetStructureOf<ImapSinglePartBodyStructure>(2);

        var fetchBodyResp = @"* 1 FETCH (BODY[2] {35}
quoted-printable =E6=9C=AC=E6=96=87)
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        // FETCH
        server.EnqueueTaggedResponse(fetchBodyResp +
                                     "$tag OK done\r\n");

        Assert.AreEqual("quoted-printable 本文",
                        message.ReadAllText(bodystructure, ImapMessageFetchBodyOptions.DecodeContent));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (BODY.PEEK[2]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestOpenReadWithSinglePartBodyStructureDecodeContentCharsetMissing()
    {
      TestMultipartMessage(null, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        var bodystructure = message.GetStructureOf<ImapSinglePartBodyStructure>(1);

        Assert.IsFalse(bodystructure.Parameters.ContainsKey("charset"));

        var fetchBodyResp = @"* 1 FETCH (BODY[1] {28}
YmFzZTY0IGVuY29kZWQg5pys5paH)
".Replace("\r\n", "\n").Replace("\n", "\r\n");

        // FETCH
        server.EnqueueTaggedResponse(fetchBodyResp +
                                     "$tag OK done\r\n");

        Assert.AreEqual(new byte[] {0x62, 0x61, 0x73, 0x65, 0x36, 0x34, 0x20, 0x65, 0x6E, 0x63, 0x6F, 0x64, 0x65, 0x64, 0x20, 0xE6, 0x9C, 0xAC, 0xE6, 0x96, 0x87},
                        message.ReadAllBytes(bodystructure, ImapMessageFetchBodyOptions.DecodeContent));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (BODY.PEEK[1]<0.10240>)\r\n"));
      });
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestOpenReadWithSinglePartBodyStructureDecodeContentCharsetUnsupported()
    {
      TestMultipartMessage("x-unknown-charset", delegate(ImapPseudoServer server, ImapMessageInfo message) {
        var bodystructure = message.GetStructureOf<ImapSinglePartBodyStructure>(1);

        Assert.IsTrue(bodystructure.Parameters.ContainsKey("charset"));
        Assert.AreEqual("x-unknown-charset", bodystructure.Parameters["charset"]);

        // throws exception
        message.OpenRead(bodystructure, ImapMessageFetchBodyOptions.DecodeContent);
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestOpenReadStaticAttributeNotFetched()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        IImapBodyStructure bodyStructure = null;

        message.OpenRead(bodyStructure);
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestOpenReadWithNullBodyStructure()
    {
      TestMultipartMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        IImapBodyStructure bodyStructure = null;

        message.OpenRead(bodyStructure);
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestOpenReadWithInvalidBodyStructure()
    {
      var envelope = "(" +
                     "\"Mon, 29 Mar 2010 22:40:08 +0900\" " +
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
                     "INTERNALDATE \"29-Mar-2010 20:40:08 +0900\" RFC822.SIZE 1024)\r\n";

      var fetchResp =
        string.Format(respLine, 1L, "((\"text\" \"plain\" (\"charset\" \"GB2312\") NIL NIL \"quoted-printable\" 1634 51 NIL NIL NIL NIL)" +
                                    "(\"text\" \"html\" (\"charset\" \"GB2312\") NIL NIL \"quoted-printable\" 5650 96 NIL NIL NIL NIL) " +
                                    "\"alternative\" (\"boundary\" \"----000000000000000000000000000000000000000000000000000000000000000\") NIL NIL NIL)") +
        string.Format(respLine, 2L, "((\"text\" \"plain\" (\"charset\" \"ISO-2022-JP\") NIL NIL \"7bit\" 1950 62 NIL NIL NIL NIL)" +
                                    "(\"text\" \"x-csrc\" (\"name\" \"polish.c\" \"charset\" \"us-ascii\") NIL NIL \"7bit\" 4752 246 NIL (\"inline\" (\"filename\" \"test.c\")) NIL NIL) " +
                                    "\"mixed\" (\"boundary\" \"------------070006080802040707000608\") NIL NIL NIL)") +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.StaticAttributes, delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        var bodyStructure = messages[0].BodyStructure.FindSection(1);

        Assert.IsNotNull(bodyStructure);
        Assert.AreEqual("1", bodyStructure.Section);

        // throws exception
        messages[1].OpenRead(bodyStructure);
      });
    }

    [Test]
    public void TestOpenReadMailboxClosed()
    {
      TestMessageClosedMailbox(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          message.OpenRead();
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
    public void TestOpenReadDeleted()
    {
      TestMessageDeleted(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        try {
          message.OpenRead();
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
    public void TestOpenReadVanished()
    {
      TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.AreNotEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsFalse(message.IsDeleted);

        // FETCH
        server.EnqueueTaggedResponse("$tag OK done\r\n");

        try {
          message.OpenRead();
          Assert.Fail("ImapMessageDeletedException not thrown");
        }
        catch (ImapMessageDeletedException ex) {
          Assert.IsNotNull(ex.DeletedMessage);
          Assert.AreSame(message, ex.DeletedMessage);

          Smdn.Net.TestUtils.SerializeBinary(ex, delegate(ImapMessageDeletedException deserialized) {
            Assert.IsNull(deserialized.DeletedMessage);
          });
        }

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

        Assert.AreEqual(ImapMessageInfo.ExpungedMessageSequenceNumber, message.Sequence);
        Assert.IsTrue(message.IsDeleted);
      });
    }

    [Test, Ignore("this will never happen")]
    public void TestOpenReadSequenceChanged()
    {
      const string messageBody = @"Return-Path: <test@example.net>
X-Original-To: test@example.net
Delivered-To: test@example.net
Message-ID: <4BAA561E.5000004@test@example.net>
Date: Thu, 25 Mar 2010 03:12:46 +0900
From: test <test@example.net>
Reply-To: test@example.net
User-Agent: Thunderbird 2.0.0.24 (X11/20100317)
MIME-Version: 1.0
To: hoge@example.net
Subject: =?ISO-2022-JP?B?GyRCJUYlOSVIGyhC?=
Content-Type: text/plain; charset=ISO-2022-JP
Content-Transfer-Encoding: 7bit

test message

3 uid fetch 6 (body[])
* 6 FETCH (UID 6 BODY[] {1476}
hogehogehoge
)
3 OK Fetch completed.
";

      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS (\\Seen))\r\n" +
        "* FETCH 2 (UID 2 FLAGS (\\Seen))\r\n" +
        "* FETCH 3 (UID 3 FLAGS (\\Recent))\r\n" +
        "$tag OK done\r\n";

      TestMessages(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes,
                   delegate(ImapPseudoServer server, ImapMessageInfo[] messages) {
        Assert.IsFalse(messages[0].IsDeleted);
        Assert.IsFalse(messages[1].IsDeleted);
        Assert.IsFalse(messages[2].IsDeleted);

        // FETCH
        server.EnqueueTaggedResponse("* EXPUNGE 1\r\n" +
                                     "* EXPUNGE 1\r\n" +
                                     "* EXISTS 1\r\n" +
                                     string.Format("* 1 FETCH (RFC822.SIZE {0} BODY[] {{{0}}}\r\n{1})\r\n",
                                                   NetworkTransferEncoding.Transfer8Bit.GetByteCount(messageBody),
                                                   messageBody) +
                                     "* 1 FETCH (FLAGS (\\Seen))\r\n" +
                                     "$tag OK done\r\n");

        var message = messages[2];

        Assert.IsFalse(message.IsSeen);
        Assert.IsTrue(message.IsRecent);

        using (var stream = message.OpenRead(ImapMessageFetchBodyOptions.SetSeen)) {
          Assert.IsNotNull(stream);

          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 3 (RFC822.SIZE BODY[]<0.10240>)\r\n"));

          Assert.IsTrue(messages[0].IsDeleted);
          Assert.IsTrue(messages[1].IsDeleted);

          Assert.AreEqual(1L, message.Sequence);
          Assert.AreEqual(3L, message.Uid);
          Assert.IsTrue(message.IsSeen);
          Assert.IsFalse(message.IsRecent);
        }
      });
    }

    [Test]
    public void TestReadAsStream()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        var ret = message.ReadAs(delegate(Stream stream) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

          Assert.IsNotNull(stream);
          Assert.AreEqual(expectedOctets, stream.Length);

          return stream.ReadToEnd();
        });

        Assert.IsNotNull(ret);
        Assert.AreEqual(expectedOctets, ret.Length);
        Assert.AreEqual(expectedMessageBody,
                        NetworkTransferEncoding.Transfer8Bit.GetString(ret));
      });
    }

    [Test]
    public void TestReadAllBytes()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.AreEqual(NetworkTransferEncoding.Transfer8Bit.GetBytes(expectedMessageBody),
                        message.ReadAllBytes());

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestSave()
    {
      const string outfile = "fetched.eml";

      try {
        if (File.Exists(outfile))
          File.Delete(outfile);

        TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
          message.Save(outfile);

          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

          Assert.IsTrue(File.Exists(outfile));
          Assert.AreEqual(expectedOctets, (new FileInfo(outfile)).Length);
          Assert.AreEqual(NetworkTransferEncoding.Transfer8Bit.GetBytes(expectedMessageBody),
                          File.ReadAllBytes(outfile));
        });
      }
      finally {
        if (File.Exists(outfile))
          File.Delete(outfile);
      }
    }

    [Test]
    public void TestSaveOpenArgumentExceptionFileNotCreate()
    {
      const string outfile = "fetched.eml";

      try {
        if (File.Exists(outfile))
          File.Delete(outfile);

        TestMessage(delegate(ImapPseudoServer server, ImapMessageInfo message) {
          try {
            message.Save(outfile, (IImapBodyStructure)null);
            Assert.Fail("ArgumentException not throw");
          }
          catch (ArgumentException) {
          }

          Assert.IsFalse(File.Exists(outfile));
        });
      }
      finally {
        if (File.Exists(outfile))
          File.Delete(outfile);
      }
    }

    [Test]
    public void TestWriteToStream()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        var outputStream = new MemoryStream();

        message.WriteTo(outputStream);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

        Assert.AreEqual(expectedOctets, outputStream.Length);

        outputStream.Close();

        Assert.AreEqual(NetworkTransferEncoding.Transfer8Bit.GetBytes(expectedMessageBody),
                        outputStream.ToArray());
      });
    }

    [Test]
    public void TestWriteToBinaryWriter()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        var outputStream = new MemoryStream();
        var writer = new BinaryWriter(outputStream);

        message.WriteTo(writer);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

        Assert.AreEqual(expectedOctets, outputStream.Length);

        outputStream.Close();

        Assert.AreEqual(NetworkTransferEncoding.Transfer8Bit.GetBytes(expectedMessageBody),
                        outputStream.ToArray());
      });
    }

    [Test]
    public void TestOpenText()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        using (var reader = message.OpenText()) {
          Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

          Assert.IsNotNull(reader);
          Assert.AreEqual(expectedMessageBody, reader.ReadToEnd());
        }
      });
    }

    [Test, Ignore("to be added")]
    public void TestOpenTextDefaultCharset()
    {
    }

    [Test]
    public void TestReadAsStreamReader()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.AreEqual(expectedMessageBody, message.ReadAs(delegate(StreamReader reader) {
          return reader.ReadToEnd();
        }));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestReadLines()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        var lines = message.ReadLines();

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));

        var reader = new StringReader(expectedMessageBody);

        foreach (var line in lines) {
          var expectedLine = reader.ReadLine();

          if (expectedLine == null)
            break;

          Assert.AreEqual(expectedLine, line);
        }
      });
    }

    [Test]
    public void TestReadAllLines()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.AreEqual(expectedMessageBody.Replace("\r\n", "\n").Replace("\n", "\r\n").TrimEnd(),
                        string.Join("\r\n", message.ReadAllLines()));

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));
      });
    }

    [Test]
    public void TestReadAllText()
    {
      TestFetchMessage(delegate(ImapPseudoServer server, ImapMessageInfo message, string expectedMessageBody, int expectedOctets) {
        Assert.AreEqual(expectedMessageBody,
                        message.ReadAllText());

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID FETCH 1 (RFC822.SIZE BODY.PEEK[]<0.10240>)\r\n"));
      });
    }
  }
}
