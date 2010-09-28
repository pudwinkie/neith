using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapResponseTextConverterTests {
    [SetUp]
    public void Setup()
    {
      baseStream = new MemoryStream();
      stream = new LineOrientedBufferedStream(baseStream);
      receiver = new ImapResponseReceiver(stream);
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private ImapStatusResponse GetSingleStatusResponse(string response)
    {
      var resp = Encoding.ASCII.GetBytes(response);

      baseStream.Seek(0, SeekOrigin.Begin);
      baseStream.Write(resp, 0, resp.Length);
      baseStream.Seek(0, SeekOrigin.Begin);

      var r = receiver.ReceiveResponse();

      Assert.IsTrue(r is ImapStatusResponse, "response type");

      return r as ImapStatusResponse;
    }

    [Test]
    public void TestAlertResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [ALERT] System shutdown in 10 minutes\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.Alert);

      Assert.AreEqual("System shutdown in 10 minutes", response.ResponseText.Text);
    }

    [Test]
    public void TestBadCharsetResponse()
    {
      var response =
        GetSingleStatusResponse("a111 NO [BADCHARSET (UTF-8 SHIFT-JIS)] EUC-JP is not supported\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.BadCharset);

      var supportedCharsets = ImapResponseTextConverter.FromBadCharset(response.ResponseText);

      Assert.AreEqual("EUC-JP is not supported", response.ResponseText.Text);
      Assert.AreEqual(2, supportedCharsets.Length);
      Assert.AreEqual("UTF-8", supportedCharsets[0]);
      Assert.AreEqual("SHIFT-JIS", supportedCharsets[1]);
    }

    [Test]
    public void TestBadCharsetResponseWithoutCharsets()
    {
      var response =
        GetSingleStatusResponse("a111 NO [BADCHARSET] EUC-JP is not supported\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.BadCharset);

      var supportedCharsets = ImapResponseTextConverter.FromBadCharset(response.ResponseText);

      Assert.AreEqual("EUC-JP is not supported", response.ResponseText.Text);
      Assert.AreEqual(0, supportedCharsets.Length);
    }

    [Test]
    public void TestCapabilityResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [CAPABILITY IMAP4rev1 AUTH=CRAM-MD5 AUTH=PLAIN CHILDREN THREAD=REFERENCES X-EXTENSION1 X-EXTENSION2] ready\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.Capability);

      var caps = ImapResponseTextConverter.FromCapability(response.ResponseText);

      Assert.AreEqual("ready", response.ResponseText.Text);

      Assert.AreEqual(7, caps.Count);

      Assert.IsTrue(caps.Has(ImapCapability.Imap4Rev1));
      Assert.IsTrue(caps.Has(ImapCapability.Children));
      Assert.IsTrue(caps.IsCapable(ImapAuthenticationMechanism.CRAMMD5));
      Assert.IsTrue(caps.IsCapable(ImapAuthenticationMechanism.Plain));

      Assert.IsTrue(caps.Has("THREAD=REFERENCES"));
      Assert.IsTrue(caps.Has("X-EXTENSION1"));
      Assert.IsTrue(caps.Has("X-EXTENSION2"));
    }

    [Test]
    public void TestPermanentFlagsResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [PERMANENTFLAGS (\\Deleted \\Seen custom \\*)] Limited\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.PermanentFlags);

      var flags = ImapResponseTextConverter.FromPermanentFlags(response.ResponseText);

      Assert.AreEqual("Limited", response.ResponseText.Text);

      Assert.IsNotNull(flags);
      Assert.AreEqual(4, flags.Count);
      Assert.IsTrue(flags.Has(ImapMessageFlag.Deleted));
      Assert.IsTrue(flags.Has(ImapMessageFlag.Seen));
      Assert.IsTrue(flags.Has(ImapMessageFlag.AllowedCreateKeywords));
      Assert.IsTrue(flags.Has("custom"));
    }


    [Test]
    public void TestReadOnlyResponse()
    {
      var response =
        GetSingleStatusResponse("A932 OK [READ-ONLY] EXAMINE completed\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.ReadOnly);

      Assert.AreEqual("EXAMINE completed", response.ResponseText.Text);
    }


    [Test]
    public void TestReadWriteResponse()
    {
      var response =
        GetSingleStatusResponse("A142 OK [READ-WRITE] SELECT completed\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.ReadWrite);

      Assert.AreEqual("SELECT completed", response.ResponseText.Text);
    }

    [Test]
    public void TestTryCreateResponse()
    {
      var response =
        GetSingleStatusResponse("9835 NO [TRYCREATE] UID COPY failed: No such destination mailbox\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.TryCreate);

      Assert.AreEqual("UID COPY failed: No such destination mailbox", response.ResponseText.Text);
    }

    [Test]
    public void TestUidNextResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [UIDNEXT 6] Predicted next UID\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.UidNext);

      var uidNext = ImapResponseTextConverter.FromUidNext(response.ResponseText);

      Assert.AreEqual("Predicted next UID", response.ResponseText.Text);
      Assert.AreEqual(6, uidNext);
    }

    [Test]
    public void TestUidValidityResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [UIDVALIDITY 988028003] UID validity status\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.UidValidity);

      var uidValidity = ImapResponseTextConverter.FromUidValidity(response.ResponseText);

      Assert.AreEqual("UID validity status", response.ResponseText.Text);
      Assert.AreEqual(988028003, uidValidity);
    }

    [Test]
    public void TestUnseenResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [UNSEEN 13] first unseen message in /var/spool/mail/user\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.Unseen);

      var unseen = ImapResponseTextConverter.FromUnseen(response.ResponseText);

      Assert.AreEqual("first unseen message in /var/spool/mail/user", response.ResponseText.Text);
      Assert.AreEqual(13, unseen);
    }

    [Test]
    public void TestReferralResponse()
    {
      var response =
        GetSingleStatusResponse("A001 NO [REFERRAL IMAP://user;AUTH=*@SERVER1/FOO IMAP://user;AUTH=*@SERVER2/BAR] Unable to rename mailbox across servers\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.Referral);

      var referrals = ImapResponseTextConverter.FromReferral(response.ResponseText);

      Assert.AreEqual("Unable to rename mailbox across servers", response.ResponseText.Text);
      Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER1/FOO"), referrals[0]);
      Assert.AreEqual(new Uri("IMAP://user;AUTH=*@SERVER2/BAR"), referrals[1]);
    }

    [Test]
    public void TestAppendUidResponse()
    {
      var response =
        GetSingleStatusResponse("A003 OK [APPENDUID 38505 3955] APPEND completed\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.AppendUid);

      var appendUid = ImapResponseTextConverter.FromAppendUid(response.ResponseText);

      Assert.AreEqual(38505, appendUid.UidValidity);
      Assert.AreEqual(3955, appendUid.ToArray()[0]);
    }

    [Test]
    public void TestAppendUidSequenceSetResponse()
    {
      var response =
        GetSingleStatusResponse("A003 OK [APPENDUID 12345 3950:3952,3954,3955,3958:3960] APPEND completed\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.AppendUid);

      var appendUid = ImapResponseTextConverter.FromAppendUid(response.ResponseText);

      Assert.AreEqual(12345, appendUid.UidValidity);
      Assert.IsTrue(appendUid.IsUidSet);

      var uids = appendUid.ToArray();

      Assert.AreEqual(8, uids.Length);
      Assert.AreEqual(3950, uids[0]);
      Assert.AreEqual(3951, uids[1]);
      Assert.AreEqual(3952, uids[2]);
      Assert.AreEqual(3954, uids[3]);
      Assert.AreEqual(3955, uids[4]);
      Assert.AreEqual(3958, uids[5]);
      Assert.AreEqual(3959, uids[6]);
      Assert.AreEqual(3960, uids[7]);
    }

    [Test]
    public void TestCopyUidResponse()
    {
      var response =
        GetSingleStatusResponse("A004 OK [COPYUID 38505 304,319:320 3956:3958] Done\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.CopyUid);

      var copyUid = ImapResponseTextConverter.FromCopyUid(response.ResponseText);

      Assert.AreEqual(38505, copyUid.UidValidity);

      Assert.IsTrue(copyUid.IsUidSet);
      Assert.IsFalse(copyUid.IsEmpty);
      Assert.IsTrue(copyUid.CopiedUidSet.IsUidSet);
      Assert.IsFalse(copyUid.CopiedUidSet.IsEmpty);

      var copied = copyUid.CopiedUidSet.ToArray();

      Assert.AreEqual(3, copied.Length);
      Assert.AreEqual(304, copied[0]);
      Assert.AreEqual(319, copied[1]);
      Assert.AreEqual(320, copied[2]);

      var assigned = copyUid.AssignedUidSet.ToArray();

      Assert.AreEqual(3, assigned.Length);
      Assert.AreEqual(3956, assigned[0]);
      Assert.AreEqual(3957, assigned[1]);
      Assert.AreEqual(3958, assigned[2]);
    }

    [Test]
    public void TestHighestModSeqResponse()
    {
      var response =
        GetSingleStatusResponse("* OK [HIGHESTMODSEQ 715194045007]\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.HighestModSeq);

      Assert.AreEqual(715194045007UL, ImapResponseTextConverter.FromHighestModSeq(response.ResponseText));
    }

    [Test]
    public void TestMetadataLongEntriesResponse()
    {
      var response =
        GetSingleStatusResponse("a OK [METADATA LONGENTRIES 2199] GETMETADATA complete\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.MetadataLongEntries);

      Assert.AreEqual(2199L, ImapResponseTextConverter.FromMetadataLongEntries(response.ResponseText));
    }

    [Test]
    public void TestMetadataMaxSizeResponse()
    {
      var response =
        GetSingleStatusResponse("a NO [METADATA MAXSIZE 1024] SETMETADATA failed\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.MetadataMaxSize);

      Assert.AreEqual(1024L, ImapResponseTextConverter.FromMetadataMaxSize(response.ResponseText));
    }

    [Test]
    public void TestUndefinedFilterResponse()
    {
      var response =
        GetSingleStatusResponse("a NO [UNDEFINED-FILTER on-the-road] search failed\r\n");

      Assert.IsTrue(response.ResponseText.Code == ImapResponseCode.UndefinedFilter);

      Assert.AreEqual("on-the-road", ImapResponseTextConverter.FromUndefinedFilter(response.ResponseText));
    }

    private ImapResponseReceiver receiver;
    private MemoryStream baseStream;
    private LineOrientedBufferedStream stream;
  }
}