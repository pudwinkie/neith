using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopCapabilityTests {
    [Test]
    public void TestContainsAllArguments()
    {
      var capa = new PopCapability("sasl", "cram-md5", "plain");

      Assert.IsTrue(capa.ContainsAllArguments());
      Assert.IsTrue(capa.ContainsAllArguments("cram-md5"));
      Assert.IsTrue(capa.ContainsAllArguments("plain"));
      Assert.IsTrue(capa.ContainsAllArguments("cram-md5", "plain"));
      Assert.IsTrue(capa.ContainsAllArguments("plain", "cram-md5"));
      Assert.IsFalse(capa.ContainsAllArguments("digest-md5"));
    }

    [Test]
    public void TestOpExplicit()
    {
      var capa = new PopCapability("TOP");

      Assert.AreEqual("TOP", (string)capa);
      Assert.AreEqual(capa.ToString(), (string)capa);

      capa = null;

      Assert.IsNull((string)capa);
    }

    [Test]
    public void TestSerializeBinary()
    {
      var capa = new PopCapability("sasl", "cram-md5", "plain");

      Assert.IsTrue(capa.ContainsAllArguments());
      Assert.IsTrue(capa.ContainsAllArguments("cram-md5"));
      Assert.IsTrue(capa.ContainsAllArguments("plain"));
      Assert.IsTrue(capa.ContainsAllArguments("cram-md5", "plain"));
      Assert.IsTrue(capa.ContainsAllArguments("plain", "cram-md5"));
      Assert.IsFalse(capa.ContainsAllArguments("digest-md5"));

      TestUtils.SerializeBinary(capa, delegate(PopCapability deserialized) {
        Assert.AreNotEqual(PopCapability.Sasl, deserialized);
        Assert.AreEqual(capa, deserialized);
        Assert.IsTrue(deserialized.ContainsAllArguments("cram-md5", "plain"));
      });
    }
  }
}
