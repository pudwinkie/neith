using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class PopMalformedTextExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new PopMalformedTextException();

      Assert.IsNull(ex1.CausedText);

      TestUtils.SerializeBinary(ex1, delegate(PopMalformedTextException deserialized) {
        Assert.IsNull(deserialized.CausedText);
      });

      var ex2 = new PopMalformedTextException(ByteString.CreateImmutable("+ERR"));

      Assert.IsNotNull(ex2.CausedText);

      TestUtils.SerializeBinary(ex2, delegate(PopMalformedTextException deserialized) {
        Assert.IsNotNull(deserialized.CausedText);
        Assert.AreEqual(ex2.CausedText, deserialized.CausedText);
      });
    }
  }
}