using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Protocol {
  [TestFixture]
  public class ImapMalformedDataExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new ImapMalformedDataException();

      Assert.IsNull(ex1.CausedData);

      TestUtils.SerializeBinary(ex1, delegate(ImapMalformedDataException deserialized) {
        Assert.IsNull(deserialized.CausedData);
      });

      var ex2 = new ImapMalformedDataException(ImapData.CreateNilData());

      Assert.IsNotNull(ex2.CausedData);

      TestUtils.SerializeBinary(ex2, delegate(ImapMalformedDataException deserialized) {
        Assert.IsNull(deserialized.CausedData);
      });
    }
  }
}