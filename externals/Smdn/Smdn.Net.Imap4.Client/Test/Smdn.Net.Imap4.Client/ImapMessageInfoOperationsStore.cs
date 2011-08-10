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
  public class ImapMessageInfoOperationsStoreTests : ImapMessageInfoTestsBase {
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
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
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
        Assert.IsTrue(messages[2].Flags.Contains(ImapMessageFlag.Recent));
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
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(message.IsSeen);
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
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
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
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
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
    public void TestToggleFlags()
    {
      var fetchResp =
        "* FETCH 1 (UID 1 FLAGS (\\Seen $label1))\r\n" +
        "$tag OK done\r\n";

      TestMessage(fetchResp, ImapMessageFetchAttributeOptions.DynamicAttributes, delegate(ImapPseudoServer server, ImapMessageInfo message) {
        Assert.AreEqual(2, message.Flags.Count);
        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue(message.Flags.Contains("$label1"));

        // STORE
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS ($label1))\r\n" +
                                     "$tag OK done\r\n");
        server.EnqueueTaggedResponse("* 1 FETCH (FLAGS (\\Draft $label1))\r\n" +
                                     "$tag OK done\r\n");

        message.ToggleFlags(ImapMessageFlag.Seen, ImapMessageFlag.Draft);

        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 -FLAGS (\\Seen)\r\n"));
        Assert.That(server.DequeueRequest(), Text.EndsWith("UID STORE 1 +FLAGS (\\Draft)\r\n"));

        Assert.AreEqual(2, message.Flags.Count);
        Assert.IsFalse(message.Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsTrue (message.Flags.Contains(ImapMessageFlag.Draft));
        Assert.IsTrue (message.Flags.Contains("$label1"));
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
        Assert.IsFalse(message.Flags.Contains("$label1"));
        Assert.IsTrue (message.Flags.Contains("$label2"));
        Assert.IsTrue (message.Flags.Contains("$label3"));
        Assert.IsTrue (message.Flags.Contains(ImapMessageFlag.Draft));
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
        Assert.IsFalse(message.Flags.Contains(ImapMessageFlag.Seen));
        Assert.IsFalse(message.IsSeen);
      });
    }
  }
}

