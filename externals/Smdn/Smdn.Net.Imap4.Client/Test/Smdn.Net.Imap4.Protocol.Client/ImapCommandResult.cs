using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapCommandResultTests {
    [Test]
    public void TestSerializeBinaryDefault()
    {
      var result = new ImapCommandResult();

      TestUtils.SerializeBinary(result, delegate(ImapCommandResult deserialized) {
        Assert.AreEqual(result.ResponseText, deserialized.ResponseText);
        Assert.AreEqual(result.Code, deserialized.Code);
        Assert.AreEqual(result.Description, deserialized.Description);
        Assert.AreEqual(result.TaggedStatusResponse, deserialized.TaggedStatusResponse);
        CollectionAssert.AreEqual(result.ReceivedResponses, deserialized.ReceivedResponses);
      });
    }
  }
}

