using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopCapabilitySetTests {
    [Test]
    public void TestFindByTag()
    {
      var list = new PopCapabilitySet(new[] {
        PopCapability.Uidl,
        PopCapability.Pipelining,
        new PopCapability("SASL", "CRAM-MD5"),
        new PopCapability("SASL", "PLAIN"),
      });

      Assert.AreEqual(PopCapability.Uidl, list.FindByTag(PopCapability.Uidl));
      Assert.AreEqual(PopCapability.Uidl, list.FindByTag("UIDL"));
      Assert.AreEqual(PopCapability.Uidl, list.FindByTag("uidl"));
      Assert.IsNotNull(list.FindByTag(PopCapability.Sasl));
      Assert.IsNull(list.FindByTag(PopCapability.Expire));
    }

    [Test]
    public void TestIsCapable()
    {
      var list = new PopCapabilitySet(new[] {
        PopCapability.Uidl,
        PopCapability.Pipelining,
        new PopCapability("SASL", "CRAM-MD5"),
        new PopCapability("SASL", "PLAIN"),
      });

      Assert.IsTrue(list.IsCapable(PopCapability.Uidl));
      Assert.IsTrue(list.IsCapable(PopCapability.Sasl));
      Assert.IsTrue(list.IsCapable(new PopCapability("uidl")));
      Assert.IsTrue(list.IsCapable(new PopCapability("sasl")));
      Assert.IsFalse(list.IsCapable(new PopCapability("SASL", "DIGEST-MD5")));
      Assert.IsFalse(list.IsCapable(PopCapability.Expire));
    }
  }
}
