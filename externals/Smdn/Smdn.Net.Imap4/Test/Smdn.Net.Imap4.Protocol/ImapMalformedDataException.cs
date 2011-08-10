using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapMalformedDataExceptionTests {
    [Test]
    public void TestSerializeBinaryCausedDataNull()
    {
      var ex = new ImapMalformedDataException();

      Assert.IsNull(ex.CausedData);

      TestUtils.SerializeBinary(ex, delegate(ImapMalformedDataException deserialized) {
        Assert.IsNull(deserialized.CausedData);
      });
    }

    [Test]
    public void TestSerializeBinaryCausedDataNotNull()
    {
      var ex = new ImapMalformedDataException(ImapData.CreateTextData(ByteString.CreateImmutable("exception")));

      Assert.IsNotNull(ex.CausedData);

      TestUtils.SerializeBinary(ex, delegate(ImapMalformedDataException deserialized) {
        Assert.IsNotNull(deserialized.CausedData);
        Assert.AreEqual("exception", deserialized.CausedData.GetTextAsString());
      });
    }
  }
}