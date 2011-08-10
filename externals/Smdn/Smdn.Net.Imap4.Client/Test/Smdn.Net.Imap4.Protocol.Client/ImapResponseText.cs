using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapResponseTextTests {
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

    [Test]
    public void TestSerializeBinary1()
    {
      receiver.SetResponse("A142 OK [READ-WRITE] SELECT completed\r\n");

      var resp = receiver.ReceiveResponse() as ImapStatusResponse;

      TestUtils.SerializeBinary(resp.ResponseText, delegate(ImapResponseText deserialized) {
        Assert.AreEqual(ImapResponseCode.ReadWrite, deserialized.Code);
        Assert.AreEqual("SELECT completed", deserialized.Text);
        Assert.IsNotNull(deserialized.Arguments);
        Assert.AreEqual(0, deserialized.Arguments.Length);
      });
    }

    [Test]
    public void TestSerializeBinary2()
    {
      receiver.SetResponse("* OK [UIDVALIDITY 988028003] UID validity status\r\n");

      var resp = receiver.ReceiveResponse() as ImapStatusResponse;

      TestUtils.SerializeBinary(resp.ResponseText, delegate(ImapResponseText deserialized) {
        Assert.AreEqual(ImapResponseCode.UidValidity, deserialized.Code);
        Assert.AreEqual("UID validity status", deserialized.Text);
        Assert.IsNotNull(deserialized.Arguments);
        Assert.AreEqual(1, deserialized.Arguments.Length);
        Assert.AreEqual("988028003", deserialized.Arguments[0].GetTextAsString());
      });
    }

    [Test]
    public void TestSerializeBinary3()
    {
      receiver.SetResponse("a OK [METADATA LONGENTRIES 2199] GETMETADATA complete\r\n");

      var resp = receiver.ReceiveResponse() as ImapStatusResponse;

      TestUtils.SerializeBinary(resp.ResponseText, delegate(ImapResponseText deserialized) {
        Assert.AreEqual(ImapResponseCode.MetadataLongEntries, deserialized.Code);
        Assert.AreEqual("GETMETADATA complete", deserialized.Text);
        Assert.IsNotNull(deserialized.Arguments);
        Assert.AreEqual(2, deserialized.Arguments.Length);
        Assert.AreEqual("LONGENTRIES", deserialized.Arguments[0].GetTextAsString());
        Assert.AreEqual("2199", deserialized.Arguments[1].GetTextAsString());
      });
    }

    private ImapPseudoResponseReceiver receiver;
  }
}
