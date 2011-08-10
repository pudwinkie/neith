using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopIncapableExceptionTests {
    [Test]
    public void TestSerializeBinary1()
    {
      var ex = new PopIncapableException();

      Assert.IsNull(ex.RequiredCapability);

      TestUtils.SerializeBinary(ex, delegate(PopIncapableException deserialized) {
        Assert.IsNull(deserialized.RequiredCapability);
      });
    }

    [Test]
    public void TestSerializeBinary2()
    {
      var ex = new PopIncapableException(PopCapability.Utf8);

      Assert.IsNotNull(ex.RequiredCapability);

      TestUtils.SerializeBinary(ex, delegate(PopIncapableException deserialized) {
        Assert.IsNotNull(deserialized.RequiredCapability);
        Assert.AreEqual(PopCapability.Utf8, deserialized.RequiredCapability);
      });
    }
  }
}

