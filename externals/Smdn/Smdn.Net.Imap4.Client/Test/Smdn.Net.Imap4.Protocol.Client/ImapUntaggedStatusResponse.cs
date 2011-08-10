using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapUntaggedStatusResponseTests {
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
      receiver.SetResponse("* OK [UIDVALIDITY 988028003] UID validity status\r\n");

      var resp = receiver.ReceiveResponse() as ImapUntaggedStatusResponse;

      TestUtils.SerializeBinary(resp, delegate(ImapUntaggedStatusResponse deserialized) {
        Assert.AreEqual(ImapResponseCondition.Ok, deserialized.Condition);
        Assert.IsNotNull(deserialized.ResponseText);
        Assert.AreEqual(ImapResponseCode.UidValidity, deserialized.ResponseText.Code);
        Assert.IsNotNull(deserialized.ResponseText.Arguments);
        Assert.AreEqual(1, deserialized.ResponseText.Arguments.Length);
        Assert.AreEqual("988028003", deserialized.ResponseText.Arguments[0].GetTextAsString());
        Assert.AreEqual("UID validity status", deserialized.ResponseText.Text);
      });
    }

    private ImapPseudoResponseReceiver receiver;
  }
}