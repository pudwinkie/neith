using System;
using NUnit.Framework;

namespace Smdn.Formats.Mime {
  [TestFixture]
  public class CharsetsTest {
    [Test]
    public void TestFromString()
    {
      Assert.AreEqual(System.Text.Encoding.UTF8, Charsets.FromString("utf-8"));
      Assert.AreEqual(System.Text.Encoding.UTF8, Charsets.FromString("UTF-8"));
      Assert.AreEqual(System.Text.Encoding.UTF8, Charsets.FromString("utf8"));
      Assert.AreEqual(System.Text.Encoding.UTF8, Charsets.FromString("utf_8"));

      Assert.AreEqual(Charsets.ShiftJIS, Charsets.FromString("shift-jis"));
      Assert.AreEqual(Charsets.ShiftJIS, Charsets.FromString("shift_jis"));
      Assert.AreEqual(Charsets.ShiftJIS, Charsets.FromString("x-sjis"));
    }
  }
}
