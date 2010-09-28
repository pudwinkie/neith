using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapIncapableExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new ImapIncapableException();

      Assert.IsNull(ex1.RequiredCapability);

      TestUtils.SerializeBinary(ex1, delegate(ImapIncapableException deserialized) {
        Assert.IsNull(deserialized.RequiredCapability);
      });

      var ex2 = new ImapIncapableException(ImapCapability.Imap4Rev1);

      Assert.IsNotNull(ex2.RequiredCapability);

      TestUtils.SerializeBinary(ex2, delegate(ImapIncapableException deserialized) {
        Assert.IsNull(deserialized.RequiredCapability);
      });
    }
  }
}

