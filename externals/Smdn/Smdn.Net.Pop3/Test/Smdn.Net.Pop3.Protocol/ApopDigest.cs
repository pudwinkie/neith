using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class ApopDigestTests {
    [Test]
    public void TestCalculate()
    {
      Assert.AreEqual("c4c9334bac560ecc979e58001b3e22fb", ApopDigest.Calculate("<1896.697170952@dbc.mtview.ca.us>", "tanstaaf"));
    }
  }
}
