using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol.Client {
  [TestFixture]
  public class PopCommandResultTests {
    [Test]
    public void TestSerializeBinaryDefault()
    {
      var result = new PopCommandResult();

      TestUtils.SerializeBinary(result, delegate(PopCommandResult deserialized) {
        Assert.AreEqual(result.ResponseText, deserialized.ResponseText);
        Assert.AreEqual(result.Code, deserialized.Code);
        Assert.AreEqual(result.Description, deserialized.Description);
        Assert.AreEqual(result.StatusResponse, deserialized.StatusResponse);
        CollectionAssert.AreEqual(result.ReceivedResponses, deserialized.ReceivedResponses);
      });
    }
  }
}

