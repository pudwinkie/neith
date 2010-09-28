using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopIncapableExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new PopIncapableException();

      Assert.IsNull(ex1.RequiredCapability);

      TestUtils.SerializeBinary(ex1, delegate(PopIncapableException deserialized) {
        Assert.IsNull(deserialized.RequiredCapability);
      });

      var ex2 = new PopIncapableException(PopCapability.Utf8);

      Assert.IsNotNull(ex2.RequiredCapability);

      TestUtils.SerializeBinary(ex2, delegate(PopIncapableException deserialized) {
        Assert.IsNull(deserialized.RequiredCapability);
      });
    }
  }
}

