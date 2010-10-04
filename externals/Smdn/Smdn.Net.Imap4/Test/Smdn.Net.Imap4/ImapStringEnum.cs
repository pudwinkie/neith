using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapStringEnumTests {
    private class StringEnum : ImapStringEnum {
      public StringEnum(string val)
        : base(val)
      {
      }
    }

    [Test]
    public void TestEquals()
    {
      var str = new StringEnum("IMAP4rev1");
      var str2 = str;

      Assert.IsTrue(str.Equals(str2));
      Assert.IsTrue(str.Equals("IMAP4REV1"));
      Assert.IsTrue(str.Equals("imap4rev1"));
      Assert.IsTrue(str.Equals(new StringEnum("IMAP4rev1")));
      Assert.IsTrue(str.Equals(new StringEnum("IMAP4REV1")));
      Assert.IsTrue(str.Equals(new StringEnum("imap4rev1")));

      var str3 = new StringEnum("IMAP4");

      Assert.IsFalse(str.Equals(null));
      Assert.IsFalse(str.Equals(str3));
      Assert.IsFalse(str.Equals("IMAP4"));
      Assert.IsFalse(str.Equals("imap4"));
      Assert.IsFalse(str.Equals(new StringEnum("IMAP4")));
      Assert.IsFalse(str.Equals(new StringEnum("imap4")));
      Assert.IsFalse(str.Equals(ImapCapability.Imap4Rev1));
      Assert.IsFalse(str.Equals(new ImapCapability("IMAP4rev1")));
      Assert.IsFalse(str.Equals(new ImapCapability("IMAP4REV1")));
      Assert.IsFalse(str.Equals(new ImapCapability("imap4rev1")));
    }

    [Test]
    public void TestOpEquality()
    {
      var str = new StringEnum("IMAP4rev1");
      var c = str;

      Assert.IsTrue(str == c);
      Assert.IsTrue(c == str);
      Assert.IsTrue(str == new StringEnum("IMAP4rev1"));
      Assert.IsTrue(str == new StringEnum("IMAP4REV1"));
      Assert.IsTrue(str == new StringEnum("imap4rev1"));
      Assert.IsTrue(new StringEnum("IMAP4rev1") == str);
      Assert.IsTrue(new StringEnum("IMAP4REV1") == str);
      Assert.IsTrue(new StringEnum("imap4rev1") == str);

      c = new StringEnum("IMAP4");

      Assert.IsFalse(str == c);
      Assert.IsFalse(c == str);
      Assert.IsFalse(str == null);
      Assert.IsFalse(null == str);
      Assert.IsFalse(str == new StringEnum("IMAP4"));
      Assert.IsFalse(str == new StringEnum("imap4"));
      Assert.IsFalse(new StringEnum("IMAP4") == str);
      Assert.IsFalse(new StringEnum("imap4") == str);
      Assert.IsFalse(str == new ImapCapability("IMAP4rev1"));
      Assert.IsFalse(str == new ImapCapability("IMAP4REV1"));
      Assert.IsFalse(str == new ImapCapability("imap4rev1"));
    }
  }
}
