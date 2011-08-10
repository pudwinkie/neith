using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopContinuationRequestTests {
    [SetUp]
    public void Setup()
    {
      receiver = new PopPseudoResponseReceiver();
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    [Test]
    public void TestSerializeBinary()
    {
      receiver.SetResponse("+ TlRMTVNTUAACAAAAAAAAAAAAAAABAoAAfvE6HR6OGggAAAAAAAAAABQAFAAwAAAAAwAMAGgAYQB5AGEAdABlAAAAAAA=\r\n");

      var resp = receiver.ReceiveResponse() as PopContinuationRequest;

      TestUtils.SerializeBinary(resp, delegate(PopContinuationRequest deserialized) {
        Assert.AreEqual(ByteString.CreateImmutable("TlRMTVNTUAACAAAAAAAAAAAAAAABAoAAfvE6HR6OGggAAAAAAAAAABQAFAAwAAAAAwAMAGgAYQB5AGEAdABlAAAAAAA="),
                        deserialized.Base64Text);
      });
    }

    private PopPseudoResponseReceiver receiver;
  }
}

