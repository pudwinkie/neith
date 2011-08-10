using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapCommandContinuationRequestTests {
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
      receiver.SetResponse("+ Ready for additional command text\r\n");

      var resp = receiver.ReceiveResponse() as ImapCommandContinuationRequest;

      TestUtils.SerializeBinary(resp, delegate(ImapCommandContinuationRequest deserialized) {
        Assert.AreEqual("Ready for additional command text", deserialized.Text);
      });
    }

    private ImapPseudoResponseReceiver receiver;
  }
}