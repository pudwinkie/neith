using System;
using NUnit.Framework;

using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopErrorResponseExceptionTests {
    [Test]
    public void TestSerializeBinary1()
    {
      var ex = new PopErrorResponseException();

      Assert.IsNull(ex.Result);

      TestUtils.SerializeBinary(ex, delegate(PopErrorResponseException deserialized) {
        Assert.IsNull(deserialized.Result);
      });
    }

    [Test]
    public void TestSerializeBinary2()
    {
      var ex = new PopErrorResponseException(new PopCommandResult());

      Assert.IsNotNull(ex.Result);

      TestUtils.SerializeBinary(ex, delegate(PopErrorResponseException deserialized) {
        Assert.IsNotNull(deserialized.Result);
      });
    }
  }
}

