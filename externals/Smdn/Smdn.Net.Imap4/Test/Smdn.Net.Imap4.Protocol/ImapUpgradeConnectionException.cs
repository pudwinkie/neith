using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapUpgradeConnectionExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new ImapUpgradeConnectionException());
    }
  }
}