using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapSecureConnectionExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new ImapSecureConnectionException());
    }
  }
}