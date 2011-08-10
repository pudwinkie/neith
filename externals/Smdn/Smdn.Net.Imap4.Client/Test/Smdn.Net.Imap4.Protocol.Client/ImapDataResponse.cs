using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapDataResponseTests {
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
    public void TestSerializeBinary()
    {
      receiver.SetResponse("* LIST () \"\" INBOX\r\n");

      var resp = receiver.ReceiveResponse() as ImapDataResponse;

      TestUtils.SerializeBinary(resp, delegate(ImapDataResponse deserialized) {
        Assert.AreEqual(ImapDataResponseType.List, deserialized.Type);
        Assert.IsNotNull(deserialized.Data);
        Assert.AreEqual(3, deserialized.Data.Length);
        Assert.AreEqual(ImapDataFormat.List, deserialized.Data[0].Format);
        Assert.AreEqual(0, deserialized.Data[0].List.Length);
        Assert.AreEqual(ImapDataFormat.Text, deserialized.Data[1].Format);
        Assert.AreEqual(string.Empty, deserialized.Data[1].GetTextAsString());
        Assert.AreEqual(ImapDataFormat.Text, deserialized.Data[2].Format);
        Assert.AreEqual("INBOX", deserialized.Data[2].GetTextAsString());
      });
    }

    private ImapPseudoResponseReceiver receiver;
  }
}