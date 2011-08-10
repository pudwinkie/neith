using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopAuthenticationMechanismTests {
    [Test]
    public void TestOpExplicit()
    {
      var authType = new PopAuthenticationMechanism("X-AUTH");

      Assert.AreEqual("X-AUTH", (string)authType);
      Assert.AreEqual(authType.ToString(), (string)authType);

      authType = null;

      Assert.IsNull((string)authType);
    }
  }
}
