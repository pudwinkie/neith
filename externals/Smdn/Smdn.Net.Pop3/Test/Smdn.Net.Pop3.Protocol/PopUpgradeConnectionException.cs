using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class PopUpgradeConnectionExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new PopUpgradeConnectionException());
    }
  }
}