using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Client {
  [TestFixture]
  public class PopMessageNotFoundExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      var ex1 = new PopMessageNotFoundException();

      Assert.AreEqual(0L, ex1.MessageNumber);
      Assert.IsNull(ex1.UniqueId);

      Smdn.Net.TestUtils.SerializeBinary(ex1, delegate(PopMessageNotFoundException deserialized) {
        Assert.AreEqual(0L, deserialized.MessageNumber);
        Assert.IsNull(deserialized.UniqueId);
      });

      var ex2 = new PopMessageNotFoundException(1L);

      Assert.AreEqual(1L, ex2.MessageNumber);
      Assert.IsNull(ex2.UniqueId);

      Smdn.Net.TestUtils.SerializeBinary(ex2, delegate(PopMessageNotFoundException deserialized) {
        Assert.AreEqual(ex2.MessageNumber, deserialized.MessageNumber);
        Assert.IsNull(deserialized.UniqueId);
      });

      var ex3 = new PopMessageNotFoundException("unique-id");

      Assert.AreEqual(0L, ex3.MessageNumber);
      Assert.AreEqual("unique-id", ex3.UniqueId);

      Smdn.Net.TestUtils.SerializeBinary(ex3, delegate(PopMessageNotFoundException deserialized) {
        Assert.AreEqual(0L, deserialized.MessageNumber);
        Assert.AreEqual(ex3.UniqueId, deserialized.UniqueId);
      });
    }
  }
}

