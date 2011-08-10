using System;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Client.Session {
  [TestFixture]
  public class ImapSessionCommandsFetchTests : ImapSessionTestsBase {
    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestFetchEmptySequenceSet()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        ImapMessage[] messages;

        session.Fetch(ImapSequenceSet.CreateSet(new long[] {}), ImapFetchDataItem.Fast, out messages);

        return -1;
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestFetchEmptyUidSet()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        ImapMessage[] messages;

        session.Fetch(ImapSequenceSet.CreateUidSet(new long[] {}), ImapFetchDataItem.Fast, out messages);

        return -1;
      });
    }

    [Test]
    public void TestFetchResultMustBeInReceivedOrder()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 3 FETCH (UID 3)\r\n" +
                               "* 2 FETCH (UID 2)\r\n" +
                               "* 1 FETCH (UID 1)\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateRangeSet(1, 3), ImapFetchDataItem.Uid, out messages));

        Assert.AreEqual("0004 FETCH 1:3 (UID)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(3L, messages[0].Uid);
        Assert.AreEqual(3L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=3"), messages[0].Url);

        Assert.AreEqual(2L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=2"), messages[1].Url);

        Assert.AreEqual(1L, messages[2].Uid);
        Assert.AreEqual(1L, messages[2].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=1"), messages[2].Url);

        return 1;
      });
    }

    [Test]
    public void TestFetchAttributes()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (" +
                               "UID 4 " + 
                               "FLAGS (\\Answered custom1) " +
                               "INTERNALDATE \"19-Feb-2008 08:15:48 +0900\" " +
                               "RFC822.SIZE 1716" + 
                               ")\r\n" + 
                               "0004 OK FETCH completed\r\n");

        ImapMessageAttribute[] messages;
        ImapFetchDataItem dataItem =
          ImapFetchDataItem.Uid +
          ImapFetchDataItem.Flags +
          ImapFetchDataItem.InternalDate +
          ImapFetchDataItem.Rfc822Size;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), dataItem, out messages));

        Assert.AreEqual("0004 FETCH 1 (UID FLAGS INTERNALDATE RFC822.SIZE)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        ImapMessageAttribute message = messages[0];

        //Assert.AreEqual(session.SelectedMailbox, message.Mailbox);
        Assert.AreEqual(4, message.Uid);
        Assert.AreEqual(1, message.Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=4"), message.Url);

        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(message.Flags.Contains("custom1"));
        Assert.AreEqual(new DateTimeOffset(2008, 2, 19, 8, 15, 48, new TimeSpan(+9, 0, 0)), message.InternalDate);
        Assert.AreEqual(1716, message.Rfc822Size);

        return 1;
      });
    }

    [Test]
    public void TestFetchDynamicAttributes()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (FLAGS (\\Answered custom1))\r\n" + 
                               "0004 OK FETCH completed\r\n");

        ImapMessageDynamicAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.Flags, out messages));

        Assert.AreEqual("0004 FETCH 1 (FLAGS)\r\n",
                        server.DequeueRequest());

        Assert.IsNotNull(messages);
        Assert.AreEqual(1, messages.Length);

        var message = messages[0];

        Assert.AreEqual(1L, message.Sequence);

        Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Answered));
        Assert.IsTrue(message.Flags.Contains("custom1"));

        return 1;
      });
    }

    [Test]
    public void TestFetchEnvelope()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (ENVELOPE (" + 
                               "\"Tue, 19 Feb 2008 08:15:48 +0900\" " +
                               "\"fetch test\" " + 
                               "((\"from address\" NIL \"from\" \"mail.example.com\")) " + 
                               "((\"sender address\" NIL \"sender\" \"mail.example.com\")) " + 
                               "((\"reply-to address\" NIL \"reply-to\" \"mail.example.com\")) " + 
                               "((\"to address\" NIL \"to\" \"mail.example.com\")) " +
                               "((\"cc address\" NIL \"cc\" \"mail.example.com\")) " +
                               "((\"bcc address\" NIL \"bcc\" \"mail.example.com\")) " +
                               "\"<in.reply.to@mail.example.com>\" " + 
                               "\"<message.id@mail.example.com>\" " + 
                               "))\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageStaticAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.Envelope, out messages));

        Assert.AreEqual("0004 FETCH 1 (ENVELOPE)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        ImapMessageStaticAttribute message = messages[0];

        //Assert.AreEqual(session.SelectedMailbox, message.Mailbox);
        Assert.AreEqual(0L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), message.Url);

        // envelope
        ImapEnvelope envelope = message.Envelope;
        Assert.AreEqual("Tue, 19 Feb 2008 08:15:48 +0900", envelope.Date);
        Assert.AreEqual("fetch test", envelope.Subject);
        Assert.AreEqual(1, envelope.From.Length);
        Assert.AreEqual("from address", envelope.From[0].Name);
        Assert.AreEqual(1, envelope.Sender.Length);
        Assert.AreEqual("sender address", envelope.Sender[0].Name);
        Assert.AreEqual(1, envelope.ReplyTo.Length);
        Assert.AreEqual("reply-to address", envelope.ReplyTo[0].Name);
        Assert.AreEqual(1, envelope.To.Length);
        Assert.AreEqual("to address", envelope.To[0].Name);
        Assert.AreEqual(1, envelope.Cc.Length);
        Assert.AreEqual("cc address", envelope.Cc[0].Name);
        Assert.AreEqual(1, envelope.Bcc.Length);
        Assert.AreEqual("bcc address", envelope.Bcc[0].Name);
        Assert.AreEqual("<in.reply.to@mail.example.com>", envelope.InReplyTo);
        Assert.AreEqual("<message.id@mail.example.com>", envelope.MessageId);

        return 1;
      });
    }

    [Test]
    public void TestFetchBodyStructureSinglePart()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (BODY (\"text\" \"plain\" (\"charset\" \"ISO-2022-JP\") NIL NIL \"7bit\" 22 1))\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageStaticAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.Body, out messages));

        Assert.AreEqual("0004 FETCH 1 (BODY)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        ImapMessageStaticAttribute message = messages[0];

        //Assert.AreEqual(session.SelectedMailbox, message.Mailbox);
        Assert.AreEqual(0L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), message.Url);

        // body structure
        Assert.IsNotNull(message.BodyStructure);
        Assert.IsNull(message.BodyStructure.ParentStructure);
        Assert.IsFalse(message.BodyStructure.IsMultiPart);
        Assert.IsTrue(message.BodyStructure is ImapSinglePartBodyStructure);

        var structure = message.BodyStructure as ImapSinglePartBodyStructure;

        Assert.AreEqual(string.Empty, structure.Section);
        Assert.AreEqual("text/plain", (string)structure.MediaType);
        Assert.AreEqual(1, structure.Parameters.Count);
        Assert.AreEqual("ISO-2022-JP", structure.Parameters["charset"]);
        Assert.IsNull(structure.Id);
        Assert.IsNull(structure.Description);
        Assert.AreEqual("7bit", structure.Encoding);
        Assert.AreEqual(22, structure.Size);
        Assert.AreEqual(1, structure.LineCount);

        try {
          Assert.IsNotNull(structure.Url);
          Assert.Fail("NotSupportedException not thrown");
        }
        catch (NotSupportedException) {
        }

        return 1;
      });
    }

    [Test]
    public void TestFetchBodyStructureMultiPart()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (UID 6 BODY "+
                               "((\"TEXT\" \"PLAIN\" (\"CHARSET\" \"US-ASCII\") NIL NIL \"7BIT\" 1152 " + 
                               "23)(\"TEXT\" \"PLAIN\" (\"CHARSET\" \"US-ASCII\" \"NAME\" \"cc.diff\") " + 
                               "\"<960723163407.20117h@cac.washington.edu>\" \"Compiler diff\" " + 
                               "\"BASE64\" 4554 73) \"MIXED\")" + 
                               ")\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageStaticAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.Body, out messages));

        Assert.AreEqual("0004 FETCH 1 (BODY)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        ImapMessageStaticAttribute message = messages[0];

        //Assert.AreEqual(session.SelectedMailbox, message.Mailbox);
        Assert.AreEqual(6L, message.Uid);
        Assert.AreEqual(1L, message.Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=6"), message.Url);

        // body structure
        Assert.IsNotNull(message.BodyStructure);
        Assert.IsNull(message.BodyStructure.ParentStructure);
        Assert.IsTrue(message.BodyStructure.IsMultiPart);
        Assert.IsTrue(message.BodyStructure is ImapMultiPartBodyStructure);

        var structure = message.BodyStructure as ImapMultiPartBodyStructure;

        Assert.AreEqual("multipart/MIXED", (string)structure.MediaType);
        Assert.AreEqual(string.Empty, structure.Section);
        Assert.AreEqual(2, structure.NestedStructures.Length);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=6"), structure.Url);

        var part1 = structure.NestedStructures[0] as ImapSinglePartBodyStructure;

        Assert.IsNotNull(part1);
        Assert.AreSame(structure, part1.ParentStructure);
        Assert.AreEqual("1", part1.Section);
        Assert.AreEqual("TEXT/PLAIN", (string)part1.MediaType);
        Assert.AreEqual(1, part1.Parameters.Count);
        Assert.AreEqual("US-ASCII", part1.Parameters["CHARSET"]);
        Assert.IsNull(part1.Id);
        Assert.IsNull(part1.Description);
        Assert.AreEqual("7BIT", part1.Encoding);
        Assert.AreEqual(1152, part1.Size);
        Assert.AreEqual(23, part1.LineCount);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=6/;SECTION=1"), part1.Url);

        var part2 = structure.NestedStructures[1] as ImapSinglePartBodyStructure;

        Assert.IsNotNull(part2);
        Assert.AreSame(structure, part2.ParentStructure);
        Assert.AreEqual("2", part2.Section);
        Assert.AreEqual("TEXT/PLAIN", (string)part2.MediaType);
        Assert.AreEqual(2, part2.Parameters.Count);
        Assert.AreEqual("US-ASCII", part2.Parameters["CHARSET"]);
        Assert.AreEqual("cc.diff", part2.Parameters["NAME"]);
        Assert.AreEqual("<960723163407.20117h@cac.washington.edu>", part2.Id);
        Assert.AreEqual("Compiler diff", part2.Description);
        Assert.AreEqual("BASE64", part2.Encoding);
        Assert.AreEqual(4554, part2.Size);
        Assert.AreEqual(73, part2.LineCount);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=6/;SECTION=2"), part2.Url);

        return 1;
      });
    }

    [Test]
    public void TestFetchByMacro()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (" +
                               "FLAGS (\\Answered custom1) " +
                               "INTERNALDATE \"19-Feb-2008 08:15:48 +0900\" " +
                               "RFC822.SIZE 1716" + 
                               ")\r\n" + 
                               "0004 OK FETCH completed\r\n");

        ImapMessageAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.Fast, out messages));

        Assert.AreEqual("0004 FETCH 1 FAST\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);
        Assert.AreEqual(new DateTimeOffset(2008, 2, 19, 8, 15, 48, new TimeSpan(+9, 0, 0)), messages[0].InternalDate);

        return 1;
      });
    }

    [Test]
    public void TestFetchBody()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (BODY[] {12}\r\ntest message)\r\n" +
                               "* 2 FETCH (BODY[] \"test message\")\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessage[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateRangeSet(1, 2), ImapFetchDataItem.BodyPeek(), out messages));

        Assert.AreEqual("0004 FETCH 1:2 (BODY.PEEK[])\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(2, messages.Length);

        //Assert.AreEqual(session.SelectedMailbox, messages[0].Mailbox);
        //Assert.AreEqual(session.SelectedMailbox, messages[1].Mailbox);

        Assert.AreEqual("test message", messages[0].GetFirstBodyAsString());
        Assert.AreEqual("test message", messages[1].GetFirstBodyAsString());

        Assert.AreEqual(0L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), messages[0].Url);

        Assert.AreEqual(0L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?2"), messages[1].Url);

        return 1;
      });
    }

    private string CreateLiteral8(string body, Encoding encoding, int maxoctet)
    {
      return CreateLiteral(body, encoding, true, maxoctet);
    }

    private string CreateLiteral(string body, Encoding encoding, int maxoctet)
    {
      return CreateLiteral(body, encoding, false, maxoctet);
    }

    private string CreateLiteral(string body, Encoding encoding, bool literal8, int maxoctet)
    {
      var literalizedBody = encoding.GetBytes(body);

      var octetBody = (maxoctet == 0) ? literalizedBody.Length : maxoctet;

      return string.Format("{0}{{{1}}}\r\n{2}",
                           literal8 ? "~" : string.Empty,
                           octetBody,
                           NetworkTransferEncoding.Transfer8Bit.GetString(literalizedBody, 0, octetBody));
    }

    [Test]
    public void TestFetchSection()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        var body = "From: from\r\n" +
          "To: to\r\n" +
          "Subect: subject\r\n" +
          "Date: Mon, 25 Feb 2008 15:01:12 +0900\r\n";

        // FETCH
        server.EnqueueResponse("* 1 FETCH (BODY[HEADER.FIELDS (FROM TO SUBJECT DATE)] " +
                               CreateLiteral(body, Encoding.ASCII, 0) +
                               ")\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageBody[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.BodyPeek("header.fields (from to subject date)"), out messages));

        Assert.AreEqual("0004 FETCH 1 (BODY.PEEK[header.fields (from to subject date)])\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        //Assert.AreEqual(0L, messages[0].Uid);
        //Assert.AreEqual(1L, messages[0].Sequence);
        //Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), messages[0].Url);

        Assert.AreEqual(body, messages[0].GetFirstBodyAsString());

        return 1;
      });
    }

    [Test]
    public void TestFetchPartial()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        var body = "From: from\r\n" +
          "To: to\r\n" +
          "Subect: subject\r\n" +
          "Date: Mon, 25 Feb 2008 15:01:12 +0900\r\n";
        var octet = 10;

        // FETCH
        server.EnqueueResponse("* 1 FETCH (BODY[HEADER.FIELDS (FROM TO SUBJECT DATE)] " +
                               CreateLiteral(body, Encoding.ASCII, octet) +
                               ")\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessage[] messages;

        var messageDataItems = ImapFetchDataItem.BodyText("header.fields (from to subject date)", 0, octet);

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), messageDataItems, out messages));

        Assert.AreEqual("0004 FETCH 1 (BODY[header.fields (from to subject date)]<0." + octet.ToString() + ">)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        Assert.AreEqual(0L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), messages[0].Url);

        Assert.AreEqual(body.Substring(0, octet), messages[0].GetFirstBodyAsString());

        return 1;
      });
    }

    [Test]
    public void TestFetchReturnedInSeparatedResponse()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 2 FETCH (INTERNALDATE \"24-Jan-2010 23:08:54 +0900\")\r\n" +
                               "* 2 FETCH (UID 399)\r\n" +
                               "* 2 FETCH (FLAGS ())\r\n" +
                               "0004 OK done\r\n");

        ImapMessageAttribute[] messages;

        var messageDataItems = ImapFetchDataItem.Uid + ImapFetchDataItem.Flags + ImapFetchDataItem.InternalDate;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(2), messageDataItems, out messages));

        Assert.AreEqual("0004 FETCH 2 (UID FLAGS INTERNALDATE)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        Assert.AreEqual(399L, messages[0].Uid);
        Assert.AreEqual(2L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=399"), messages[0].Url);

        Assert.AreEqual(0, messages[0].Flags.Count);
        Assert.IsTrue(messages[0].InternalDate.HasValue);
        Assert.AreEqual(new DateTimeOffset(2010, 1, 24, 23, 8, 54, new TimeSpan(9, 0, 0)), messages[0].InternalDate.Value);

        return 1;
      });
    }

    [Test]
    public void TestFetchModSeq()
    {
      SelectMailbox(new[] {"CONDSTORE"}, delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (MODSEQ (624140003))\r\n" +
                               "* 2 FETCH (MODSEQ (624140007))\r\n" +
                               "* 3 FETCH (MODSEQ (624140005))\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateRangeSet(1, 3), ImapFetchDataItem.ModSeq, out messages));

        Assert.AreEqual("0004 FETCH 1:3 (MODSEQ)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(0L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), messages[0].Url);
        Assert.AreEqual(624140003UL, messages[0].ModSeq);

        Assert.AreEqual(0L, messages[1].Uid);
        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?2"), messages[1].Url);
        Assert.AreEqual(624140007UL, messages[1].ModSeq);

        Assert.AreEqual(0L, messages[2].Uid);
        Assert.AreEqual(3L, messages[2].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?3"), messages[2].Url);
        Assert.AreEqual(624140005UL, messages[2].ModSeq);

        Assert.AreEqual(0L, messages[2].Uid);
        Assert.AreEqual(3L, messages[2].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?3"), messages[2].Url);

        return 1;
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestFetchModSeqIncapable()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        session.HandlesIncapableAsException = true;

        // FETCH
        ImapMessage[] messages;

        session.Fetch(ImapSequenceSet.CreateRangeSet(1, 3), ImapFetchDataItem.Fast + ImapFetchDataItem.ModSeq, out messages);

        return -1;
      });
    }

    [Test]
    public void TestFetchChangedSince()
    {
      SelectMailbox(new[] {"CONDSTORE"}, delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (UID 4 MODSEQ (65402) FLAGS (\\Seen))\r\n" +
                               "* 2 FETCH (UID 6 MODSEQ (75403) FLAGS (\\Deleted))\r\n" +
                               "* 4 FETCH (UID 8 MODSEQ (29738) FLAGS ($NoJunk $AutoJunk $MDNSent))\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageAttribute[] messages;

        Assert.IsTrue((bool)session.FetchChangedSince(ImapSequenceSet.CreateUidFromSet(1), ImapFetchDataItem.Flags, 12345UL, out messages));

        Assert.AreEqual("0004 UID FETCH 1:* (FLAGS) (CHANGEDSINCE 12345)\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(4L, messages[0].Uid);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=4"), messages[0].Url);
        Assert.AreEqual(65402UL, messages[0].ModSeq);
        Assert.IsTrue(messages[0].Flags.Contains(ImapMessageFlag.Seen));

        Assert.AreEqual(2L, messages[1].Sequence);
        Assert.AreEqual(6L, messages[1].Uid);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=6"), messages[1].Url);
        Assert.AreEqual(75403UL, messages[1].ModSeq);
        Assert.IsTrue(messages[1].Flags.Contains(ImapMessageFlag.Deleted));

        Assert.AreEqual(4L, messages[2].Sequence);
        Assert.AreEqual(8L, messages[2].Uid);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "/;UID=8"), messages[2].Url);
        Assert.AreEqual(29738UL, messages[2].ModSeq);
        Assert.IsTrue(messages[2].Flags.Contains("$NoJunk"));
        Assert.IsTrue(messages[2].Flags.Contains("$AutoJunk"));
        Assert.IsTrue(messages[2].Flags.Contains("$MDNSent"));

        return 1;
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestFetchChangedSinceIncapable()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        ImapMessage[] messages;
        session.FetchChangedSince(ImapSequenceSet.CreateUidFromSet(1), ImapFetchDataItem.Flags, 12345UL, out messages);

        return -1;
      });
    }

    [Test]
    public void TestFetchBinary()
    {
      SelectMailbox(new[] {"BINARY"}, delegate(ImapSession session, ImapPseudoServer server) {
        var body = "\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f" +
          "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f";

        // FETCH
        server.EnqueueResponse("* 1 FETCH (BINARY[1.1] " +
                               CreateLiteral8(body, Encoding.ASCII, 0) +
                               ")\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessage[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.BinaryPeek("1.1"), out messages));

        Assert.AreEqual("0004 FETCH 1 (BINARY.PEEK[1.1])\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        Assert.AreEqual(0L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), messages[0].Url);

        Assert.AreEqual(body, messages[0].GetFirstBodyAsString());

        return 1;
      });
    }

    [Test]
    public void TestFetchBinarySize()
    {
      SelectMailbox(new[] {"BINARY"}, delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        server.EnqueueResponse("* 1 FETCH (BINARY.SIZE[1.1] 32)\r\n" +
                               "0004 OK FETCH completed\r\n");

        ImapMessageStaticAttribute[] messages;

        Assert.IsTrue((bool)session.Fetch(ImapSequenceSet.CreateSet(1), ImapFetchDataItem.BinarySize("1.1"), out messages));

        Assert.AreEqual("0004 FETCH 1 (BINARY.SIZE[1.1])\r\n",
                        server.DequeueRequest());

        Assert.AreEqual(1, messages.Length);

        Assert.AreEqual(0L, messages[0].Uid);
        Assert.AreEqual(1L, messages[0].Sequence);
        Assert.AreEqual(new Uri(session.SelectedMailbox.Url.AbsoluteUri + "?1"), messages[0].Url);

        Assert.AreEqual(32L, messages[0].GetBinarySizeOf("1.1"));
        Assert.AreEqual(32L, messages[0].BinarySize);

        return 1;
      });
    }

    [Test]
    [ExpectedException(typeof(ImapIncapableException))]
    public void TestFetchBinaryIncapable()
    {
      SelectMailbox(delegate(ImapSession session, ImapPseudoServer server) {
        // FETCH
        ImapMessage[] messages;
        session.Fetch(ImapSequenceSet.CreateUidFromSet(1), ImapFetchDataItem.Binary("1.1"), out messages);

        return -1;
      });
    }
  }
}