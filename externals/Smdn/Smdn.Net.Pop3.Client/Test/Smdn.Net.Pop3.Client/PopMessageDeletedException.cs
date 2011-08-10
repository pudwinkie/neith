using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Client {
  [TestFixture]
  public class PopMessageDeletedExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new PopMessageDeletedException();

      Assert.AreEqual(0L, ex1.MessageNumber);

      Smdn.Net.TestUtils.SerializeBinary(ex1, delegate(PopMessageDeletedException deserialized) {
        Assert.AreEqual(0L, deserialized.MessageNumber);
      });

      var ex2 = new PopMessageDeletedException(1L);

      Assert.AreEqual(1L, ex2.MessageNumber);

      Smdn.Net.TestUtils.SerializeBinary(ex2, delegate(PopMessageDeletedException deserialized) {
        Assert.AreEqual(ex2.MessageNumber, deserialized.MessageNumber);
      });
    }
  }
}

