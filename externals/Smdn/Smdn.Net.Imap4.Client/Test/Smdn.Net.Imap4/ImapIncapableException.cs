using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapIncapableExceptionTests {
    [Test]
    public void TestSerializeBinary1()
    {
      var ex = new ImapIncapableException();

      Assert.IsNull(ex.RequiredCapabilities);

      TestUtils.SerializeBinary(ex, delegate(ImapIncapableException deserialized) {
        Assert.IsNull(deserialized.RequiredCapabilities);
      });
    }

    [Test]
    public void TestSerializeBinary2()
    {
      var ex = new ImapIncapableException(ImapCapability.Imap4Rev1);

      Assert.IsNotNull(ex.RequiredCapabilities);

      TestUtils.SerializeBinary(ex, delegate(ImapIncapableException deserialized) {
        Assert.IsNotNull(deserialized.RequiredCapabilities);
        CollectionAssert.AreEquivalent(new[] {ImapCapability.Imap4Rev1}, 
                                       deserialized.RequiredCapabilities);
      });
    }

    [Test]
    public void TestSerializeBinary3()
    {
      var ex = new ImapIncapableException(new[] {ImapCapability.Imap4Rev1, ImapCapability.SaslIR});

      Assert.IsNotNull(ex.RequiredCapabilities);

      TestUtils.SerializeBinary(ex, delegate(ImapIncapableException deserialized) {
        Assert.IsNotNull(deserialized.RequiredCapabilities);
        CollectionAssert.AreEquivalent(new[] {ImapCapability.Imap4Rev1, ImapCapability.SaslIR}, 
                                       deserialized.RequiredCapabilities);
      });
    }
  }
}

