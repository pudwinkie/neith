using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapCapabilityTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(ImapCapability.Imap4Rev1, delegate(ImapCapability deserialized) {
        Assert.AreNotSame(ImapCapability.Imap4Rev1, deserialized);
        Assert.AreEqual(ImapCapability.Imap4Rev1, deserialized);
      });
    }
  }
}
