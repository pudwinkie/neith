using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class PopResponseCodeTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(PopResponseCode.Auth, delegate(PopResponseCode deserialized) {
        Assert.AreNotSame(PopResponseCode.Auth, deserialized);
        Assert.AreEqual(PopResponseCode.Auth, deserialized);
      });
    }
  }
}

