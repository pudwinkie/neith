using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapDataResponseConverterTests {
    [SetUp]
    public void Setup()
    {
      receiver = new ImapPseudoResponseReceiver();
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private ImapDataResponse GetSingleDataResponse(string response)
    {
      receiver.SetResponse(response);

      for (;;) {
        var r = receiver.ReceiveResponse();

        if (r == null)
          continue;

        Assert.IsTrue(r is ImapDataResponse, "response type");

        return r as ImapDataResponse;
      }
    }

    [Test]
    public void TestCapabilityResponse()
    {
      var response =
        GetSingleDataResponse("* CAPABILITY IMAP4rev1 STARTTLS AUTH=GSSAPI XPIG-LATIN\r\n");

      var caps = ImapDataResponseConverter.FromCapability(response);

      Assert.AreEqual(4, caps.Count, "capability count");
      Assert.IsTrue(caps.Contains(ImapCapability.Imap4Rev1));
      Assert.IsTrue(caps.Contains(ImapCapability.StartTls));
      Assert.IsTrue(caps.Contains("IMAP4rev1"));
      Assert.IsTrue(caps.Contains("STARTTLS"));
      Assert.IsTrue(caps.Contains("AUTH=GSSAPI"));
      Assert.IsTrue(caps.Contains("XPIG-LATIN"));
    }

    [Test]
    public void TestCapabilityResponseDuplicated()
    {
      var response =
        GetSingleDataResponse("* CAPABILITY IMAP4rev1 IMAP4rev1 XPIG-LATIN XPIG-LATIN\r\n");

      var caps = ImapDataResponseConverter.FromCapability(response);

      Assert.AreEqual(2, caps.Count, "capability count");
      Assert.IsTrue(caps.Contains(ImapCapability.Imap4Rev1));
      Assert.IsTrue(caps.Contains("XPIG-LATIN"));
    }

    [Test]
    public void TestListResponse()
    {
      var response =
        GetSingleDataResponse("* LIST (\\Noselect) \"/\" ~/Mail/foo\r\n");

      var mailboxList = ImapDataResponseConverter.FromList(response);

      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.NoSelect));
      Assert.IsTrue(mailboxList.NameAttributes.Contains(@"\Noselect"));
      Assert.AreEqual("/", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("~/Mail/foo", mailboxList.Name);

      Assert.IsNull(mailboxList.ChildInfo);

      try {
        var collection = mailboxList.NameAttributes as System.Collections.Generic.ICollection<ImapMailboxFlag>;

        collection.Add(ImapMailboxFlag.HasChildren);

        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }

    [Test]
    public void TestListResponseDuplicated()
    {
      var response =
        GetSingleDataResponse("* LIST (\\Noselect \\Noselect) \"/\" ~/Mail/foo\r\n");

      var mailboxList = ImapDataResponseConverter.FromList(response);

      Assert.AreEqual(1, mailboxList.NameAttributes.Count);
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.NoSelect));

      Assert.IsNull(mailboxList.ChildInfo);
    }

    [Test]
    public void TestListExtendedResponseWithEmptyExtendedData()
    {
      var response =
        GetSingleDataResponse("* LIST (\\NonExistent) \"/\" \"Foo\" ()\r\n");

      var mailboxList = ImapDataResponseConverter.FromList(response);

      Assert.AreEqual(1, mailboxList.NameAttributes.Count);
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.NonExistent));
      Assert.AreEqual("/", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("Foo", mailboxList.Name);

      Assert.IsNull(mailboxList.ChildInfo);
    }

    [Test]
    public void TestListExtendedResponseWithUnknownExtension()
    {
      var response =
        GetSingleDataResponse("* LIST (\\NonExistent) \"/\" \"Foo\" (\"X-EXTENSION\" ())\r\n");

      var mailboxList = ImapDataResponseConverter.FromList(response);

      Assert.AreEqual(1, mailboxList.NameAttributes.Count);
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.NonExistent));
      Assert.AreEqual("/", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("Foo", mailboxList.Name);

      Assert.IsNull(mailboxList.ChildInfo);
    }

    [Test]
    public void TestListExtendedResponseWithChildInfo()
    {
      var response =
        GetSingleDataResponse("* LIST (\\NonExistent) \"/\" \"Foo\" (\"CHILDINFO\" (\"SUBSCRIBED\"))\r\n");

      var mailboxList = ImapDataResponseConverter.FromList(response);

      Assert.AreEqual(1, mailboxList.NameAttributes.Count);
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.NonExistent));
      Assert.AreEqual("/", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("Foo", mailboxList.Name);

      Assert.IsNotNull(mailboxList.ChildInfo);
      Assert.IsTrue(mailboxList.ChildInfo.Subscribed);
    }

    [Test]
    public void TestListExtendedResponseWithUnknownChildInfoExtension()
    {
      var response =
        GetSingleDataResponse("* LIST (\\NonExistent) \"/\" \"Foo\" (\"CHILDINFO\" (\"X-EXTENSION\"))\r\n");

      var mailboxList = ImapDataResponseConverter.FromList(response);

      Assert.AreEqual(1, mailboxList.NameAttributes.Count);
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.NonExistent));
      Assert.AreEqual("/", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("Foo", mailboxList.Name);

      Assert.IsNotNull(mailboxList.ChildInfo);
      Assert.IsFalse(mailboxList.ChildInfo.Subscribed);
    }

    [Test]
    public void TestLsubResponse()
    {
      var response =
        GetSingleDataResponse("* LSUB () \".\" #news.comp.mail.misc\r\n");

      var mailboxList = ImapDataResponseConverter.FromLsub(response);

      Assert.AreEqual(0, mailboxList.NameAttributes.Count);
      Assert.AreEqual(".", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("#news.comp.mail.misc", mailboxList.Name);

      Assert.IsNull(mailboxList.ChildInfo);
    }

    [Test]
    public void TestStatusResponse()
    {
      var response =
        GetSingleDataResponse("* STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)\r\n");

      string name;
      var statusAttribute = ImapDataResponseConverter.FromStatus(response, out name);

      Assert.AreEqual("blurdybloop", name);
      Assert.IsNotNull(statusAttribute.Messages);
      Assert.AreEqual(231, statusAttribute.Messages.Value);
      Assert.IsNull(statusAttribute.Recent);
      Assert.IsNull(statusAttribute.Unseen);
      Assert.IsNotNull(statusAttribute.UidNext);
      Assert.AreEqual(44292, statusAttribute.UidNext.Value);
      Assert.IsNull(statusAttribute.UidValidity);
    }

    [Test]
    public void TestSearchResponse()
    {
      var response =
        GetSingleDataResponse("* SEARCH 2 3 6\r\n");

      var messages = ImapDataResponseConverter.FromSearch(response, false);

      Assert.IsNull(messages.HighestModSeq);

      var arr = messages.ToArray();

      Assert.AreEqual(3, arr.Length);
      Assert.AreEqual(2, arr[0]);
      Assert.AreEqual(3, arr[1]);
      Assert.AreEqual(6, arr[2]);
    }

    [Test]
    public void TestSortResponse()
    {
      var response =
        GetSingleDataResponse("* SORT 2 84 882\r\n");

      var messages = ImapDataResponseConverter.FromSort(response, false);

      Assert.IsNull(messages.HighestModSeq);

      var arr = messages.ToArray();

      Assert.AreEqual(3, arr.Length);
      Assert.AreEqual(2, arr[0]);
      Assert.AreEqual(84, arr[1]);
      Assert.AreEqual(882, arr[2]);
    }

    [Test]
    public void TestSearchNoMatchResponse()
    {
      var response =
        GetSingleDataResponse("* SEARCH\r\n");

      var messages = ImapDataResponseConverter.FromSearch(response, false);

      Assert.IsTrue(messages.IsEmpty);
    }

    [Test]
    public void TestSortNoMatchResponse()
    {
      var response =
        GetSingleDataResponse("* SORT\r\n");

      var messages = ImapDataResponseConverter.FromSort(response, false);

      Assert.IsTrue(messages.IsEmpty);
    }

    [Test]
    public void TestSearchResponseWithModSeq()
    {
      var response =
        GetSingleDataResponse("* SEARCH 2 5 6 7 11 12 18 19 20 23 (MODSEQ 917162500)\r\n");

      var messages = ImapDataResponseConverter.FromSearch(response, false);

      Assert.AreEqual(917162500UL, messages.HighestModSeq);

      var arr = messages.ToArray();

      Assert.AreEqual(10, arr.Length);
      Assert.AreEqual(2, arr[0]);
      Assert.AreEqual(5, arr[1]);
      Assert.AreEqual(6, arr[2]);
      Assert.AreEqual(7, arr[3]);
      Assert.AreEqual(11, arr[4]);
      Assert.AreEqual(12, arr[5]);
      Assert.AreEqual(18, arr[6]);
      Assert.AreEqual(19, arr[7]);
      Assert.AreEqual(20, arr[8]);
      Assert.AreEqual(23, arr[9]);
    }

    [Test]
    public void TestThreadResponse1()
    {
      var response =
        GetSingleDataResponse("* THREAD (2)(3 6 (4 23)(44 7 96))\r\n");

      var threadList = ImapDataResponseConverter.FromThread(response, false);

      var traversed   = new[] {2, 3, 6, 4, 23, 44, 7, 96};
      var childCounts = new[] {0, 1, 2, 1, 0,  1,  1, 0};
      var index = 0;

      threadList.Traverse(delegate(ImapThreadList list) {
        Assert.AreEqual(traversed[index], list.Number, "traversed {0}", index);
        Assert.AreEqual(childCounts[index], list.Children.Length, "children {0}", index);

        index++;
      });

      Assert.AreEqual(8, index, "count");

      var sequenceSet = threadList.ToSequenceSet().ToArray();

      Assert.AreEqual(index, sequenceSet.Length, "sequence set length");

      for (index = 0; index < sequenceSet.Length; index++) {
        Assert.AreEqual(traversed[index], sequenceSet[index], "sequence set {0}", index);
      }
    }

    [Test]
    public void TestThreadResponse2()
    {
      var response =
        GetSingleDataResponse("* THREAD (166)(167)(168)(169)(172)(170)(171)(173)(174 (175)(176)(178)(181)(180))(179)(177 (183)(182)(188)(184)" +
                              "(185)(186)(187)(189))(190)(191)(192)(193)(194 195)(196 (197)(198))(199)(200 202)(201)(203)(204)(205)(206 207)(208)\r\n");

      var threadList = ImapDataResponseConverter.FromThread(response, false);

      var traversed   = new[] {
        166, 167, 168, 169, 172, 170, 171, 173, 174, 175, 176, 178, 181, 180, 179, 177, 183, 182, 188, 184,
        185, 186, 187, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 202, 201, 203, 204, 205, 206, 207, 208,
      };
      var childCounts = new[] {
          0,   0,   0,   0,   0,   0,   0,   0,   5,   0,   0,   0,   0,   0,   0,   8,   0,   0,   0,   0,
          0,   0,   0,   0,   0,   0,   0,   0,   1,   0,   2,   0,   0,   0,   1,   0,   0,   0,   0,   0,   1,   0,   0,
      };
      var index = 0;

      threadList.Traverse(delegate(ImapThreadList list) {
        Assert.AreEqual(traversed[index], list.Number, "traversed {0}", index);
        Assert.AreEqual(childCounts[index], list.Children.Length, "children {0}", index);

        index++;
      });

      Assert.AreEqual(43, index, "count");
    }

    [Test]
    public void TestThreadNoMatchResponse()
    {
      var response =
        GetSingleDataResponse("* THREAD\r\n");

      var threadList = ImapDataResponseConverter.FromThread(response, false);

      Assert.AreEqual(0, threadList.Children.Length);
    }

    [Test]
    public void TestFlagsResponse()
    {
      var response =
        GetSingleDataResponse("* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)\r\n");

      var flags = ImapDataResponseConverter.FromFlags(response);

      Assert.AreEqual(5, flags.Count);
      Assert.IsTrue(flags.Contains(ImapMessageFlag.Answered));
      Assert.IsTrue(flags.Contains(ImapMessageFlag.Flagged));
      Assert.IsTrue(flags.Contains(ImapMessageFlag.Deleted));
      Assert.IsTrue(flags.Contains(ImapMessageFlag.Seen));
      Assert.IsTrue(flags.Contains(ImapMessageFlag.Draft));

      try {
        var collection = flags as System.Collections.Generic.ICollection<ImapMessageFlag>;

        collection.Add(ImapMessageFlag.Recent);

        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }

    [Test]
    public void TestFlagsResponseDuplicated()
    {
      var response =
        GetSingleDataResponse("* FLAGS (\\Answered \\Answered $label2 $label2)\r\n");

      var flags = ImapDataResponseConverter.FromFlags(response);

      Assert.AreEqual(2, flags.Count);
      Assert.IsTrue(flags.Contains(ImapMessageFlag.Answered));
      Assert.IsTrue(flags.Contains("$label2"));
    }

    [Test]
    public void TestExistsResponse()
    {
      var response =
        GetSingleDataResponse("* 23 EXISTS\r\n");

      Assert.AreEqual(23, ImapDataResponseConverter.FromExists(response));
    }

    [Test]
    public void TestRecentResponse()
    {
      var response =
        GetSingleDataResponse("* 5 RECENT\r\n");

      Assert.AreEqual(5, ImapDataResponseConverter.FromRecent(response));
    }

    [Test]
    public void TestExpungeResponse()
    {
      var response =
        GetSingleDataResponse("* 44 EXPUNGE\r\n");

      Assert.AreEqual(44, ImapDataResponseConverter.FromExpunge(response));
    }

    [Test]
    public void TestFetchResponse()
    {
      var response =
        GetSingleDataResponse("* 23 FETCH (FLAGS (\\Seen) RFC822.SIZE 44827)\r\n");

      var message = ImapDataResponseConverter.FromFetch<ImapMessage>(response);

      Assert.AreEqual(23, message.Sequence);
      Assert.AreEqual(1, message.Flags.Count);
      Assert.IsTrue(message.Flags.Contains(ImapMessageFlag.Seen));
      Assert.AreEqual(44827, message.Rfc822Size);

      Assert.IsNull(message.BodyStructure);
      Assert.IsNull(message.Envelope);
      Assert.AreEqual(0, message.Uid);
      Assert.IsNull(message.InternalDate);

      try {
        var collection = message.Flags as System.Collections.Generic.ICollection<ImapMessageFlag>;

        collection.Add(ImapMessageFlag.Recent);

        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }

      try {
        message.GetFirstBody();
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {
      }

      try {
        message.GetFirstBodyAsString();
        Assert.Fail("InvalidOperationException not thrown");
      }
      catch (InvalidOperationException) {
      }

      try {
        message.GetBody("non-existent section");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestFetchResponseBodyStructure()
    {
      var response =
        GetSingleDataResponse("* 7 FETCH (UID 23 BODYSTRUCTURE (" +
                              "(\"text\" \"plain\" (\"format\" \"flowed\" \"charset\" \"iso-2022-jp\" \"reply-type\" \"original\") "+
                              "NIL NIL \"7bit\" 75 3 NIL NIL NIL NIL)" +
                              "(\"application\" \"x-zip-compressed\" (\"name\" \"tmp.zip\") " +
                              "NIL NIL \"base64\" 8200 NIL (\"attachment\" (\"filename\" \"tmp.zip\")) NIL NIL) " +
                              "\"mixed\" (\"boundary\" \"----=_NextPart_000_0018_01CA37E0.F7885730\") NIL NIL NIL))\r\n");

      var message = ImapDataResponseConverter.FromFetch<ImapMessageAttribute>(response);

      Assert.AreEqual(7L, message.Sequence);
      Assert.AreEqual(23L, message.Uid);

      var bodystructure = message.BodyStructure as ImapMultiPartBodyStructure;

      Assert.IsNotNull(bodystructure);
      Assert.IsNull(bodystructure.ParentStructure);
      Assert.AreEqual(2, bodystructure.NestedStructures.Length);
      Assert.AreEqual(string.Empty, bodystructure.Section);
      Assert.AreEqual("multipart/mixed", (string)bodystructure.MediaType);

      var extBodyStructure = bodystructure as ImapExtendedMultiPartBodyStructure;

      Assert.IsNotNull(extBodyStructure);
      Assert.AreEqual(1, extBodyStructure.Parameters.Count);
      Assert.AreEqual("----=_NextPart_000_0018_01CA37E0.F7885730", extBodyStructure.Parameters["boundary"]);
      Assert.IsNull(extBodyStructure.Disposition);
      Assert.IsNull(extBodyStructure.Languages);
      Assert.IsNull(extBodyStructure.Location);
      Assert.IsNull(extBodyStructure.Extensions);

      var nested = bodystructure.NestedStructures;

      var part1 = nested[0] as ImapSinglePartBodyStructure;

      Assert.IsNotNull(part1);
      Assert.AreSame(bodystructure, part1.ParentStructure);
      Assert.AreEqual("1", part1.Section);
      Assert.AreEqual("text/plain", (string)part1.MediaType);
      Assert.AreEqual(3, part1.Parameters.Count);
      Assert.AreEqual("flowed", part1.Parameters["format"]);
      Assert.AreEqual("iso-2022-jp", part1.Parameters["charset"]);
      Assert.AreEqual("original", part1.Parameters["reply-type"]);
      Assert.IsNull(part1.Id);
      Assert.IsNull(part1.Description);
      Assert.AreEqual("7bit", part1.Encoding);
      Assert.AreEqual(75L, part1.Size);
      Assert.AreEqual(3L, part1.LineCount);

      var part1ext = part1 as ImapExtendedSinglePartBodyStructure;

      Assert.IsNotNull(part1ext);
      Assert.IsNull(part1ext.MD5);
      Assert.IsNull(part1ext.Disposition);
      Assert.IsNull(part1ext.Languages);
      Assert.IsNull(part1ext.Location);
      Assert.IsNull(part1ext.Extensions);

      var part2 = nested[1] as ImapSinglePartBodyStructure;

      Assert.IsNotNull(part2);
      Assert.AreSame(bodystructure, part2.ParentStructure);
      Assert.AreEqual("2", part2.Section);
      Assert.AreEqual("application/x-zip-compressed", (string)part2.MediaType);
      Assert.AreEqual(1, part2.Parameters.Count);
      Assert.AreEqual("tmp.zip", part2.Parameters["name"]);
      Assert.IsNull(part2.Id);
      Assert.IsNull(part2.Description);
      Assert.AreEqual("base64", part2.Encoding);
      Assert.AreEqual(8200, part2.Size);
      Assert.AreEqual(0L, part2.LineCount);

      var part2ext = part2 as ImapExtendedSinglePartBodyStructure;

      Assert.IsNotNull(part2ext);
      Assert.IsNull(part2ext.MD5);
      Assert.IsNotNull(part2ext.Disposition);
      Assert.AreEqual("attachment", part2ext.Disposition.Type);
      Assert.IsTrue(part2ext.Disposition.IsAttachment);
      Assert.IsFalse(part2ext.Disposition.IsInline);
      Assert.AreEqual(1, part2ext.Disposition.Parameters.Count);
      Assert.AreEqual("tmp.zip", part2ext.Disposition.Parameters["filename"]);
      Assert.AreEqual("tmp.zip", part2ext.Disposition.Filename);
      Assert.IsNull(part2ext.Languages);
      Assert.IsNull(part2ext.Location);
      Assert.IsNull(part2ext.Extensions);

      /*
       * IEnumerable
       */
      var expectingEnumeratedStructures = new IImapBodyStructure[] {
        part1,
        part2,
      };
      var index = 0;

      foreach (var part in bodystructure) {
        Assert.AreSame(expectingEnumeratedStructures[index++], part, "enumerated instances are same #{0}", index);
      }

      /*
       * ImapBodyStructureUtils
       */
      var expectingTraversedStructures = new IImapBodyStructure[] {
        part1,
        part2,
      };

      index = 0;

      bodystructure.Traverse(delegate(IImapBodyStructure part) {
        Assert.AreSame(expectingTraversedStructures[index++], part, "traversed instances are same #{0}", index);
      });
    }

    [Test]
    public void TestFetchResponseBodyStructureMessageRfc822()
    {
      var response =
        GetSingleDataResponse("* 14 FETCH (UID 30 BODYSTRUCTURE (" +
                              "(\"text\" \"plain\" (\"charset\" \"ISO-2022-JP\") NIL NIL \"7bit\" 6 1 NIL NIL NIL NIL)" +
                              "(\"message\" \"rfc822\" (\"name\" \"=?ISO-2022-JP?B?GyRCRTpJVSVhJUMlOyE8JTgbKEI=?=\") " +
                              "NIL NIL \"7bit\" 188 (NIL \"test mail\" NIL NIL NIL NIL NIL NIL NIL NIL) " +
                              "(\"text\" \"plain\" (\"charset\" \"us-ascii\") NIL NIL \"7bit\" 121 6 NIL NIL NIL NIL) 10 NIL " +
                              "(\"inline\" (\"filename\" \"ISO-2022-JP''%1B%24%42%45%3A%49%55%25%61%25%43%25%3B%21%3C%25%38%1B%28%42\")) NIL NIL) " +
                              "\"mixed\" (\"boundary\" \"------------040401080108050302040809\") NIL NIL NIL))\r\n");
      var message = ImapDataResponseConverter.FromFetch<ImapMessageStaticAttribute>(response);

      Assert.AreEqual(14L, message.Sequence);
      Assert.AreEqual(30L, message.Uid);

      var bodystructure = message.BodyStructure as ImapMultiPartBodyStructure;

      Assert.IsNotNull(bodystructure);
      Assert.IsNull(bodystructure.ParentStructure);
      Assert.AreEqual(2, bodystructure.NestedStructures.Length);
      Assert.AreEqual(string.Empty, bodystructure.Section);
      Assert.AreEqual("multipart/mixed", (string)bodystructure.MediaType);

      var extBodyStructure = bodystructure as ImapExtendedMultiPartBodyStructure;

      Assert.IsNotNull(extBodyStructure);
      Assert.AreEqual(1, extBodyStructure.Parameters.Count);
      Assert.AreEqual("------------040401080108050302040809", extBodyStructure.Parameters["boundary"]);
      Assert.IsNull(extBodyStructure.Disposition);
      Assert.IsNull(extBodyStructure.Languages);
      Assert.IsNull(extBodyStructure.Location);
      Assert.IsNull(extBodyStructure.Extensions);

      var nested = bodystructure.NestedStructures;
      var part1 = nested[0] as ImapSinglePartBodyStructure;

      Assert.IsNotNull(part1);
      Assert.AreSame(bodystructure, part1.ParentStructure);
      Assert.AreEqual("1", part1.Section);
      Assert.AreEqual("text/plain", (string)part1.MediaType);
      Assert.AreEqual(1, part1.Parameters.Count);
      Assert.AreEqual("ISO-2022-JP", part1.Parameters["charset"]);
      Assert.IsNull(part1.Id);
      Assert.IsNull(part1.Description);
      Assert.AreEqual("7bit", part1.Encoding);
      Assert.AreEqual(6L, part1.Size);
      Assert.AreEqual(1L, part1.LineCount);

      var part1ext = part1 as ImapExtendedSinglePartBodyStructure;

      Assert.IsNotNull(part1ext);
      Assert.IsNull(part1ext.MD5);
      Assert.IsNull(part1ext.Disposition);
      Assert.IsNull(part1ext.Languages);
      Assert.IsNull(part1ext.Location);
      Assert.IsNull(part1ext.Extensions);

      var part2 = nested[1] as ImapMessageRfc822BodyStructure;

      Assert.IsNotNull(part2);
      Assert.AreSame(bodystructure, part1.ParentStructure);
      Assert.AreEqual("2", part2.Section);
      Assert.AreEqual("message/rfc822", (string)part2.MediaType);
      Assert.AreEqual(1, part2.Parameters.Count);
      Assert.AreEqual("=?ISO-2022-JP?B?GyRCRTpJVSVhJUMlOyE8JTgbKEI=?=", part2.Parameters["name"]);
      Assert.IsNull(part2.Id);
      Assert.IsNull(part2.Description);
      Assert.AreEqual("7bit", part2.Encoding);
      Assert.AreEqual(188L, part2.Size);

      Assert.IsNotNull(part2.Envelope);
      Assert.IsNull(part2.Envelope.Date);
      Assert.AreEqual("test mail", part2.Envelope.Subject);
      Assert.IsEmpty(part2.Envelope.From);
      Assert.IsEmpty(part2.Envelope.Sender);
      Assert.IsEmpty(part2.Envelope.ReplyTo);
      Assert.IsEmpty(part2.Envelope.To);
      Assert.IsEmpty(part2.Envelope.Cc);
      Assert.IsEmpty(part2.Envelope.Bcc);
      Assert.IsNull(part2.Envelope.InReplyTo);
      Assert.IsNull(part2.Envelope.MessageId);

      var part2encapsulated = part2.BodyStructure as ImapSinglePartBodyStructure;

      Assert.IsNotNull(part2encapsulated);
      Assert.AreSame(part2, part2encapsulated.ParentStructure);
      Assert.AreEqual("2.1", part2encapsulated.Section);
      Assert.AreEqual("text/plain", (string)part2encapsulated.MediaType);
      Assert.AreEqual(1, part2encapsulated.Parameters.Count);
      Assert.AreEqual("us-ascii", part2encapsulated.Parameters["charset"]);
      Assert.IsNull(part2encapsulated.Id);
      Assert.IsNull(part2encapsulated.Description);
      Assert.AreEqual("7bit", part2encapsulated.Encoding);
      Assert.AreEqual(121L, part2encapsulated.Size);
      Assert.AreEqual(6L, part2encapsulated.LineCount);

      var part2encapsulatedExt = part2encapsulated as ImapExtendedSinglePartBodyStructure;

      Assert.IsNotNull(part2encapsulatedExt);
      Assert.IsNull(part2encapsulatedExt.MD5);
      Assert.IsNull(part2encapsulatedExt.Disposition);
      Assert.IsNull(part2encapsulatedExt.Languages);
      Assert.IsNull(part2encapsulatedExt.Location);
      Assert.IsNull(part2encapsulatedExt.Extensions);

      Assert.AreEqual(10L, part2.LineCount);

      var part2ext = part2 as ImapExtendedMessageRfc822BodyStructure;

      Assert.IsNotNull(part2ext);
      Assert.IsNull(part2ext.MD5);
      Assert.IsNotNull(part2ext.Disposition);
      Assert.AreEqual("inline", part2ext.Disposition.Type);
      Assert.IsTrue(part2ext.Disposition.IsInline);
      Assert.AreEqual(1, part2ext.Disposition.Parameters.Count);
      Assert.AreEqual("ISO-2022-JP''%1B%24%42%45%3A%49%55%25%61%25%43%25%3B%21%3C%25%38%1B%28%42",
                      part2ext.Disposition.Parameters["filename"]);
      Assert.AreEqual("ISO-2022-JP''%1B%24%42%45%3A%49%55%25%61%25%43%25%3B%21%3C%25%38%1B%28%42",
                      part2ext.Disposition.Filename);
      Assert.IsNull(part2ext.Languages);
      Assert.IsNull(part2ext.Location);
      Assert.IsNull(part2ext.Extensions);

      /*
       * IEnumerable
       */
      var expectingEnumeratedStructures = new IImapBodyStructure[] {
        part1,
        part2,
      };
      var index = 0;

      foreach (var part in bodystructure) {
        Assert.AreSame(expectingEnumeratedStructures[index++], part, "enumerated instances are same #{0}", index);
      }

      var expectingEnumeratedStructuresInPart2 = new IImapBodyStructure[] {
        part2encapsulated,
      };

      index = 0;

      foreach (var part in part2) {
        Assert.AreSame(expectingEnumeratedStructuresInPart2[index++], part, "enumerated instances are same #{0}", index);
      }

      /*
       * ImapBodyStructureUtils
       */
      var expectingTraversedStructures = new IImapBodyStructure[] {
        part1,
        part2,
        part2encapsulated,
      };

      index = 0;

      bodystructure.Traverse(delegate(IImapBodyStructure part) {
        Assert.AreSame(expectingTraversedStructures[index++], part, "traversed instances are same #{0}", index);
      });
    }

    [Test]
    public void TestFetchResponseMultipleContent()
    {
      var response =
        GetSingleDataResponse("* 10 FETCH (UID 26 " +
                              "BODY[1.1] {10}\r\nsection1.1 " +
                              "BODY[1.2] {10}\r\nsection1.2 " +
                              "BODY[1.3] NIL " +
                              "BINARY[2.1] {10}\r\nsection2.1 " +
                              "RFC822 {6}\r\nrfc822 " +
                              "RFC822.HEADER {6}\r\nheader " +
                              "RFC822.TEXT {4}\r\ntext" +
                              ")\r\n");

      var message = ImapDataResponseConverter.FromFetch<ImapMessage>(response);

      Assert.AreEqual(10L, message.Sequence);
      Assert.AreEqual(26L, message.Uid);
      Assert.AreEqual(0, message.Flags.Count);

      Assert.IsNotNull(message.GetFirstBody());
      Assert.IsNotNull(message.GetFirstBodyAsString());

      var section11 = message.GetBody("body[1.1]");

      Assert.IsNotNull(section11);
      Assert.AreEqual(10L, section11.Length);
      Assert.AreEqual("section1.1", message.GetBodyAsString("BODY[1.1]"));
      Assert.AreEqual("section1.1", message.GetBodyAsString("body[1.1]"));
      Assert.AreEqual("section1.1", message.GetBodyAsString("Body[1.1]"));

      var section12 = message.GetBody("BODY[1.2]");

      Assert.IsNotNull(section12);
      Assert.AreEqual(10L, section12.Length);
      Assert.AreEqual("section1.2", message.GetBodyAsString("BODY[1.2]"));

      Assert.IsNull(message.GetBody("BODY[1.3]"));

      var section21 = message.GetBody("binary[2.1]");

      Assert.IsNotNull(section21);
      Assert.AreEqual(10L, section21.Length);
      Assert.AreEqual("section2.1", message.GetBodyAsString("BINARY[2.1]"));

      var rfc822 = message.GetBody("RFC822");

      Assert.IsNotNull(rfc822);
      Assert.AreEqual(6L, rfc822.Length);
      Assert.AreEqual("rfc822", message.GetBodyAsString("RFC822"));

      var rfc822header = message.GetBody("RFC822.HEADER");

      Assert.IsNotNull(rfc822header);
      Assert.AreEqual(6L, rfc822header.Length);
      Assert.AreEqual("header", message.GetBodyAsString("RFC822.HEADER"));

      var rfc822text = message.GetBody("RFC822.TEXT");

      Assert.IsNotNull(rfc822text);
      Assert.AreEqual(4L, rfc822text.Length);
      Assert.AreEqual("text", message.GetBodyAsString("RFC822.TEXT"));

      try {
        message.GetBody("non-existent section");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestFetchResponseBinarySize()
    {
      var response =
        GetSingleDataResponse("* 10 FETCH (UID 26 BINARY.SIZE[1.1] 12 BINARY.SIZE[1.2] 456)\r\n");

      var message = ImapDataResponseConverter.FromFetch<ImapMessageStaticAttribute>(response);

      Assert.AreEqual(10L, message.Sequence);
      Assert.AreEqual(26L, message.Uid);

      Assert.AreEqual(12L, message.GetBinarySizeOf("1.1"));
      Assert.AreEqual(456L, message.GetBinarySizeOf("1.2"));

      try {
        message.GetBinarySizeOf("non-existent section");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestNamespaceResponse1()
    {
      var response =
        GetSingleDataResponse("* NAMESPACE ((\"\" \"/\")) NIL ((\"Public Folders/\" \"/\"))\r\n");

      var namespaces = ImapDataResponseConverter.FromNamespace(response);

      Assert.AreEqual(1, namespaces.PersonalNamespaces.Length);
      Assert.AreEqual(string.Empty, namespaces.PersonalNamespaces[0].Prefix);
      Assert.AreEqual("/", namespaces.PersonalNamespaces[0].HierarchyDelimiter);
      Assert.AreEqual(0, namespaces.PersonalNamespaces[0].Extensions.Count);

      Assert.AreEqual(0, namespaces.OtherUsersNamespaces.Length);

      Assert.AreEqual(1, namespaces.SharedNamespaces.Length);
      Assert.AreEqual("Public Folders/", namespaces.SharedNamespaces[0].Prefix);
      Assert.AreEqual("/", namespaces.SharedNamespaces[0].HierarchyDelimiter);
      Assert.AreEqual(0, namespaces.SharedNamespaces[0].Extensions.Count);
    }

    [Test]
    public void TestNamespaceResponse2()
    {
      var response =
        GetSingleDataResponse("* NAMESPACE ((\"\" \"/\")(\"#mh/\" \"/\" \"X-PARAM\" (\"FLAG1\" \"FLAG2\"))) NIL NIL\r\n");

      var namespaces = ImapDataResponseConverter.FromNamespace(response);

      Assert.AreEqual(2, namespaces.PersonalNamespaces.Length);
      Assert.AreEqual(string.Empty, namespaces.PersonalNamespaces[0].Prefix);
      Assert.AreEqual("/", namespaces.PersonalNamespaces[0].HierarchyDelimiter);
      Assert.AreEqual(0, namespaces.PersonalNamespaces[0].Extensions.Count);
      Assert.AreEqual("#mh/", namespaces.PersonalNamespaces[1].Prefix);
      Assert.AreEqual("/", namespaces.PersonalNamespaces[1].HierarchyDelimiter);
      Assert.AreEqual(1, namespaces.PersonalNamespaces[1].Extensions.Count);
      Assert.IsTrue(namespaces.PersonalNamespaces[1].Extensions.ContainsKey("X-PARAM"));
      Assert.AreEqual("FLAG1", namespaces.PersonalNamespaces[1].Extensions["X-PARAM"][0]);
      Assert.AreEqual("FLAG2", namespaces.PersonalNamespaces[1].Extensions["X-PARAM"][1]);

      Assert.AreEqual(0, namespaces.OtherUsersNamespaces.Length);
      Assert.AreEqual(0, namespaces.SharedNamespaces.Length);
    }

    [Test]
    public void TestEnabledResponse()
    {
      var response =
        GetSingleDataResponse("* ENABLED CONDSTORE X-GOOD-IDEA\r\n");

      var caps = ImapDataResponseConverter.FromEnabled(response);

      Assert.AreEqual(2, caps.Count, "capability count");
      Assert.IsTrue(caps.Contains("CONDSTORE"));
      Assert.IsTrue(caps.Contains("X-GOOD-IDEA"));
    }

    [Test]
    public void TestIdResponse()
    {
      var response =
        GetSingleDataResponse("* ID (\"name\" \"Cyrus\" \"version\" \"1.5\" \"os\" \"sunos\" \"os-version\" \"5.5\" \"support-url\" \"mailto:cyrus-bugs+@andrew.cmu.edu\")\r\n");

      var paramList = ImapDataResponseConverter.FromId(response);

      Assert.AreEqual(5, paramList.Count, "param count");
      Assert.AreEqual("Cyrus", paramList["name"]);
      Assert.AreEqual("1.5", paramList["version"]);
      Assert.AreEqual("sunos", paramList["os"]);
      Assert.AreEqual("5.5", paramList["os-version"]);
      Assert.AreEqual("mailto:cyrus-bugs+@andrew.cmu.edu", paramList["support-url"]);
    }

    [Test]
    public void TestIdResponseNil()
    {
      var response =
        GetSingleDataResponse("* ID NIL\r\n");

      var paramList = ImapDataResponseConverter.FromId(response);

      Assert.AreEqual(0, paramList.Count, "param count");
    }

    [Test]
    public void TestESearchResponse1()
    {
      var response =
        GetSingleDataResponse("* ESEARCH UID COUNT 5 ALL 4:19,21,28\r\n");

      var matched = ImapDataResponseConverter.FromESearch(response);

      Assert.IsTrue(matched.IsUidSet);
      Assert.IsNull(matched.Max);
      Assert.IsNull(matched.Min);
      Assert.IsNull(matched.HighestModSeq);

      Assert.IsNotNull(matched.Count);
      Assert.AreEqual(5, matched.Count);

      var arrActual = matched.ToArray();
      var arrExpected = new long[] {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 28};

      Assert.AreEqual(arrExpected.Length, arrActual.Length);

      for (var i = 0; i < arrActual.Length; i++) {
        Assert.AreEqual(arrExpected[i], arrActual[i]);
      }
    }

    [Test]
    public void TestESearchResponse2()
    {
      var response =
        GetSingleDataResponse("* ESEARCH (TAG \"a567\") UID COUNT 5 ALL 4:19,21,28\r\n");

      var matched = ImapDataResponseConverter.FromESearch(response);

      Assert.IsTrue(matched.IsUidSet);
      Assert.IsNull(matched.Max);
      Assert.IsNull(matched.Min);
      Assert.IsNull(matched.HighestModSeq);

      Assert.IsNotNull(matched.Count);
      Assert.AreEqual(5, matched.Count);
      Assert.AreEqual("a567", matched.Tag);
    }

    [Test]
    public void TestESearchResponse3()
    {
      var response =
        GetSingleDataResponse("* ESEARCH COUNT 5 ALL 1:17,21\r\n");

      var matched = ImapDataResponseConverter.FromESearch(response);

      Assert.IsFalse(matched.IsUidSet);
      Assert.IsNull(matched.Max);
      Assert.IsNull(matched.Min);
      Assert.IsNull(matched.HighestModSeq);
      Assert.IsNull(matched.Tag);

      var arrActual = matched.ToArray();
      var arrExpected = new long[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 21};

      Assert.AreEqual(arrExpected.Length, arrActual.Length);

      for (var i = 0; i < arrActual.Length; i++) {
        Assert.AreEqual(arrExpected[i], arrActual[i]);
      }
    }

    [Test]
    public void TestQuotaResponse()
    {
      var response =
        GetSingleDataResponse("* QUOTA \"\" (STORAGE 10 512)\r\n");

      var quota = ImapDataResponseConverter.FromQuota(response);

      Assert.IsNotNull(quota);
      Assert.AreEqual(string.Empty, quota.Root);
      Assert.AreEqual(1, quota.Resources.Length);

      Assert.IsNotNull(quota.Resources[0]);
      Assert.AreEqual("STORAGE", quota.Resources[0].Name);
      Assert.AreEqual(10L, quota.Resources[0].Usage);
      Assert.AreEqual(512L, quota.Resources[0].Limit);
    }

    [Test]
    public void TestQuotaRootResponse1()
    {
      var response =
        GetSingleDataResponse("* QUOTAROOT INBOX \"\"\r\n");

      string[] quotaRootNames;

      var mailbox = ImapDataResponseConverter.FromQuotaRoot(response, out quotaRootNames);

      Assert.AreEqual("INBOX", mailbox);
      Assert.IsNotNull(quotaRootNames);
      Assert.AreEqual(1, quotaRootNames.Length);
      Assert.AreEqual(string.Empty, quotaRootNames[0]);
    }

    [Test]
    public void TestQuotaRootResponse2()
    {
      var response =
        GetSingleDataResponse("* QUOTAROOT comp.mail.mime\r\n");

      string[] quotaRootNames;

      var mailbox = ImapDataResponseConverter.FromQuotaRoot(response, out quotaRootNames);

      Assert.AreEqual("comp.mail.mime", mailbox);
      Assert.IsNotNull(quotaRootNames);
      Assert.AreEqual(0, quotaRootNames.Length);
    }

    [Test]
    public void TestLanguageResponse1()
    {
      var response =
        GetSingleDataResponse("* LANGUAGE (DE)\r\n");

      var langTagList = ImapDataResponseConverter.FromLanguage(response);

      Assert.IsNotNull(langTagList);
      Assert.AreEqual(1, langTagList.Length);
      Assert.AreEqual("DE", langTagList[0]);
    }

    [Test]
    public void TestLanguageResponse2()
    {
      var response =
        GetSingleDataResponse("* LANGUAGE (EN DE IT i-default)\r\n");

      var langTagList = ImapDataResponseConverter.FromLanguage(response);

      Assert.IsNotNull(langTagList);
      Assert.AreEqual(4, langTagList.Length);
      Assert.AreEqual("EN", langTagList[0]);
      Assert.AreEqual("DE", langTagList[1]);
      Assert.AreEqual("IT", langTagList[2]);
      Assert.AreEqual("i-default", langTagList[3]);
    }

    [Test]
    public void TestComparatorResponse()
    {
      var response =
        GetSingleDataResponse("* COMPARATOR i;basic\r\n");

      ImapCollationAlgorithm[] matchingComparators;

      var activeComparator = ImapDataResponseConverter.FromComparator(response, out matchingComparators);

      Assert.AreEqual(new ImapCollationAlgorithm("i;basic"), activeComparator);
      Assert.IsNull(matchingComparators);
    }

    [Test]
    public void TestComparatorResponseWithMatchingComparators()
    {
      var response =
        GetSingleDataResponse("* COMPARATOR i;unicode-casemap (i;unicode-casemap i;ascii-casemap)\r\n");

      ImapCollationAlgorithm[] matchingComparators;

      var activeComparator = ImapDataResponseConverter.FromComparator(response, out matchingComparators);

      Assert.AreEqual(ImapCollationAlgorithm.UnicodeCasemap, activeComparator);

      Assert.IsNotNull(matchingComparators);
      Assert.AreEqual(2, matchingComparators.Length);
      Assert.AreEqual(ImapCollationAlgorithm.UnicodeCasemap, matchingComparators[0]);
      Assert.AreEqual(ImapCollationAlgorithm.AsciiCasemap, matchingComparators[1]);
    }

    [Test]
    public void TestXListResponse()
    {
      var response =
        GetSingleDataResponse("* XLIST (\\HasNoChildren \\AllMail) \"/\" \"[Gmail]/&MFkweTBmMG4w4TD8MOs-\"\r\n");

      var mailboxList = ImapDataResponseConverter.FromXList(response);

      Assert.AreEqual(2, mailboxList.NameAttributes.Count);
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.HasNoChildren));
      Assert.IsTrue(mailboxList.NameAttributes.Contains(ImapMailboxFlag.GimapAllMail));
      Assert.AreEqual("/", mailboxList.HierarchyDelimiter);
      Assert.AreEqual("[Gmail]/すべてのメール", mailboxList.Name);

      Assert.IsNull(mailboxList.ChildInfo);
    }

    [Test]
    public void TestMetadataResponseWithValues1()
    {
      var response =
        GetSingleDataResponse("* METADATA \"\" (/shared/comment \"My comment\")\r\n");

      string mailbox;
      var metadata = ImapDataResponseConverter.FromMetadataEntryValues(response, out mailbox);

      Assert.AreEqual(string.Empty, mailbox);
      Assert.AreEqual(1, metadata.Length);
      Assert.IsTrue(metadata[0].IsShared);
      Assert.AreEqual("/shared/comment", metadata[0].EntryName);
      //Assert.AreEqual("My comment", metadata[0].Value);
      Assert.IsTrue(metadata[0].Value.Equals("My comment"));
    }

    [Test]
    public void TestMetadataResponseWithValues2()
    {
      var response =
        GetSingleDataResponse("* METADATA \"INBOX\" (/private/comment \"My comment\"" +
                              "/shared/comment \"Its sunny outside!\")\r\n");

      string mailbox;
      var metadata = ImapDataResponseConverter.FromMetadataEntryValues(response, out mailbox);

      Assert.AreEqual("INBOX", mailbox);
      Assert.AreEqual(2, metadata.Length);

      Assert.IsTrue(metadata[0].IsPrivate);
      Assert.AreEqual("/private/comment", metadata[0].EntryName);
      //Assert.AreEqual("My comment", metadata[0].Value);
      Assert.IsTrue(metadata[0].Value.Equals("My comment"));

      Assert.IsTrue(metadata[1].IsShared);
      Assert.AreEqual("/shared/comment", metadata[1].EntryName);
      //Assert.AreEqual("Its sunny outside!", metadata[1].Value);
      Assert.IsTrue(metadata[1].Value.Equals("Its sunny outside!"));
    }

    [Test]
    public void TestMetadataResponseWithoutValues1()
    {
      var response =
        GetSingleDataResponse("* METADATA \"\" /shared/comment\r\n");

      string mailbox;
      var list = ImapDataResponseConverter.FromMetadataEntryList(response, out mailbox);

      Assert.AreEqual(string.Empty, mailbox);

      Assert.AreEqual(1, list.Length);
      Assert.AreEqual("/shared/comment", list[0]);
    }

    [Test]
    public void TestMetadataResponseWithoutValues2()
    {
      var response =
        GetSingleDataResponse("* METADATA \"INBOX\" /shared/comment /private/comment\r\n");

      string mailbox;
      var list = ImapDataResponseConverter.FromMetadataEntryList(response, out mailbox);

      Assert.AreEqual("INBOX", mailbox);

      Assert.AreEqual(2, list.Length);
      Assert.AreEqual("/shared/comment", list[0]);
      Assert.AreEqual("/private/comment", list[1]);
    }

    private ImapPseudoResponseReceiver receiver;
  }
}
