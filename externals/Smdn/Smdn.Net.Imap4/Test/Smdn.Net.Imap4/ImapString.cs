using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapStringTests {
    [Test]
    public void TestOpExplicit()
    {
      var str = new ImapString("hoge");

      Assert.AreEqual("hoge", (string)str);
      Assert.AreEqual(str.ToString(), (string)str);

      str = null;

      Assert.IsNull((string)str);
    }
  }
}
