using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapTaggedStatusResponseTests {
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
      receiver.SetResponse("A142 OK [READ-WRITE] SELECT completed\r\n");

      var resp = receiver.ReceiveResponse() as ImapTaggedStatusResponse;

      TestUtils.SerializeBinary(resp, delegate(ImapTaggedStatusResponse deserialized) {
        Assert.AreEqual("A142", deserialized.Tag);
        Assert.AreEqual(ImapResponseCondition.Ok, deserialized.Condition);
        Assert.IsNotNull(deserialized.ResponseText);
        Assert.AreEqual(ImapResponseCode.ReadWrite, deserialized.ResponseText.Code);
        Assert.IsNotNull(deserialized.ResponseText.Arguments);
        Assert.AreEqual(0, deserialized.ResponseText.Arguments.Length);
        Assert.AreEqual("SELECT completed", deserialized.ResponseText.Text);
      });
    }

    private ImapPseudoResponseReceiver receiver;
  }
}