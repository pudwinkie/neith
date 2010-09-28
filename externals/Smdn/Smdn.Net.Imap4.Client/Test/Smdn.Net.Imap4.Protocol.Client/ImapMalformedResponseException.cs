using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol.Client {
  [TestFixture]
  public class ImapMalformedResponseExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new ImapMalformedResponseException();

      Assert.IsNull(ex1.CausedResponse);

      TestUtils.SerializeBinary(ex1, delegate(ImapMalformedResponseException deserialized) {
        Assert.IsNull(deserialized.CausedResponse);
      });

      var ex2 = new ImapMalformedResponseException("invalid response", "+ERR");

      Assert.IsNotNull(ex2.CausedResponse);

      TestUtils.SerializeBinary(ex2, delegate(ImapMalformedResponseException deserialized) {
        Assert.IsNotNull(deserialized.CausedResponse);
        Assert.AreEqual(ex2.CausedResponse, deserialized.CausedResponse);
      });
    }
  }
}

