using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopTerminationResponseTests {
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
      receiver.HandleAsMultiline = true;

      receiver.SetResponse("+OK XXX octets\r\n" +
                           "end of message\r\n" +
                           ".\r\n");

      receiver.ReceiveResponse();
      receiver.ReceiveResponse();

      var resp = receiver.ReceiveResponse() as PopTerminationResponse;

      TestUtils.SerializeBinary(resp);
    }

    private PopPseudoResponseReceiver receiver;
  }
}
