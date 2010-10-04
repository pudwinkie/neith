using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopMalformedResponseExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new PopMalformedResponseException();

      Assert.IsNull(ex1.CausedResponse);

      TestUtils.SerializeBinary(ex1, delegate(PopMalformedResponseException deserialized) {
        Assert.IsNull(deserialized.CausedResponse);
      });

      var ex2 = new PopMalformedResponseException("invalid response", "* BAD");

      Assert.IsNotNull(ex2.CausedResponse);

      TestUtils.SerializeBinary(ex2, delegate(PopMalformedResponseException deserialized) {
        Assert.IsNotNull(deserialized.CausedResponse);
        Assert.AreEqual(ex2.CausedResponse, deserialized.CausedResponse);
      });
    }
  }
}

