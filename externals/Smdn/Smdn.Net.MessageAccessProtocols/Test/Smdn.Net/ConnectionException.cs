using System;
using NUnit.Framework;

namespace Smdn.Net {
  [TestFixture]
  public class ConnectionExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new ConnectionException("test"));
    }
  }
}
