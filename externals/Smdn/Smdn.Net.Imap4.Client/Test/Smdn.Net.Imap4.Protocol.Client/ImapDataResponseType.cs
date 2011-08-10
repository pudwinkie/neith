using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapDataResponseTypeTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(ImapDataResponseType.Fetch, delegate(ImapDataResponseType deserialized) {
        Assert.AreNotSame(ImapDataResponseType.Fetch, deserialized);
        Assert.AreEqual(ImapDataResponseType.Fetch, deserialized);
      });
    }
  }
}
