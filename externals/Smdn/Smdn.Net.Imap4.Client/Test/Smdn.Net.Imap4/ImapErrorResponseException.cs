using System;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapErrorResponseExceptionTests {
    [Test]
    public void TestSerializeBinary1()
    {
      var ex = new ImapErrorResponseException();

      Assert.IsNull(ex.Result);

      TestUtils.SerializeBinary(ex, delegate(ImapErrorResponseException deserialized) {
        Assert.IsNull(deserialized.Result);
      });
    }

    [Test]
    public void TestSerializeBinary2()
    {
      var ex = new ImapErrorResponseException(new ImapCommandResult());

      Assert.IsNotNull(ex.Result);

      TestUtils.SerializeBinary(ex, delegate(ImapErrorResponseException deserialized) {
        Assert.IsNotNull(deserialized.Result);
      });
    }
  }
}

