using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopStatusResponseTests {
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
      receiver.SetResponse("-ERR [IN-USE] Do you have another POP session running?\r\n");

      var resp = receiver.ReceiveResponse() as PopStatusResponse;

      TestUtils.SerializeBinary(resp, delegate(PopStatusResponse deserialized) {
        Assert.AreEqual(PopStatusIndicator.Negative, deserialized.Status);
        Assert.IsNotNull(deserialized.ResponseText.Code);
        Assert.AreEqual(PopResponseCode.InUse, deserialized.ResponseText.Code);
        Assert.AreEqual("Do you have another POP session running?", deserialized.Text);
      });
    }

    private PopPseudoResponseReceiver receiver;
  }
}
