using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class PopConnectionExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new PopConnectionException());
    }
  }
}