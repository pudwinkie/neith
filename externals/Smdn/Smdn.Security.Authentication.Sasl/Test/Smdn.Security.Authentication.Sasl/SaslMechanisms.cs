using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl {
  [TestFixture]
  public class SaslMechanismsTests {
    [Test]
    public void TestAllMechanisms()
    {
      var mechanisms = new List<string>(SaslMechanisms.AllMechanisms);

      Assert.Greater(mechanisms.Count, 0);
      Assert.IsTrue(mechanisms.Contains(SaslMechanisms.Anonymous));
      Assert.IsTrue(mechanisms.Contains(SaslMechanisms.DigestMD5));
    }
  }
}
