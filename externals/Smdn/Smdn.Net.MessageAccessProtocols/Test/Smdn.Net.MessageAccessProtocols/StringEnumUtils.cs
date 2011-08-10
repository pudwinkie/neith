using System;
using NUnit.Framework;

namespace Smdn.Net.MessageAccessProtocols {
  [TestFixture]
  public class StringEnumUtilsTests {
    private class StringEnum : IStringEnum {
      public static readonly StringEnum Imap4Rev1 = new StringEnum("IMAP4rev1");
      public static readonly StringEnum UidPlus = new StringEnum("UIDPLUS");
      public static readonly StringEnum AuthPlain = new StringEnum("AUTH=PLAIN");

      private static readonly StringEnum PrivateClassValue = new StringEnum("PrivateClassValue");
      public StringEnum PublicInstanceValue = PrivateClassValue;

      public string Value {
        get; private set;
      }

      public StringEnum(string val)
      {
        this.Value = val;
      }

      public bool Equals(IStringEnum other)
      {
        return Equals(other.Value);
      }

      public bool Equals(string other)
      {
        return string.Equals(this.Value, other, StringComparison.OrdinalIgnoreCase);
      }
    }

    [Test]
    public void TestGetDefinedConstants()
    {
      var expected = new StringEnumSet<StringEnum>(new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });
      var actual = new StringEnumSet<StringEnum>(StringEnumUtils.GetDefinedConstants<StringEnum>());

      Assert.AreEqual(expected.Count, actual.Count);

      foreach (var val in expected) {
        Assert.IsTrue(actual.Contains(val));
      }
    }
  }
}
