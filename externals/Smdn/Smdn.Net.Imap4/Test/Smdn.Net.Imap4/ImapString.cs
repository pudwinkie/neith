using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapStringTests {
    [Test]
    public void TestValue()
    {
      var str = new ImapString("hoge");

      Assert.AreEqual("hoge", str.Value);
      Assert.AreEqual(str.ToString(), str.Value);
    }

    [Test]
    public void TestSerializeBinary()
    {
      var str = new ImapString("hoge");

      TestUtils.SerializeBinary(str, delegate(ImapString deserialized) {
        Assert.AreNotSame(str, deserialized);
        Assert.AreEqual(str, deserialized);
        Assert.AreEqual("hoge", deserialized.Value);
      });
    }
  }
}
