using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapFormatExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new ImapFormatException());
    }
  }
}