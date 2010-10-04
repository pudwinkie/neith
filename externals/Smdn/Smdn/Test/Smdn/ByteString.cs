using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Smdn {
  [TestFixture]
  public class ByteStringTests {
    [Test]
    public void TestConstructFromByteArray()
    {
      Assert.AreEqual(new byte[] {0x61, 0x62, 0x63}, (new ByteString(new byte[] {0x61, 0x62, 0x63})).ByteArray);
      Assert.AreEqual(new byte[] {0x62, 0x63}, (new ByteString(new byte[] {0x61, 0x62, 0x63}, 1)).ByteArray);
      Assert.AreEqual(new byte[] {0x62}, (new ByteString(new byte[] {0x61, 0x62, 0x63}, 1, 1)).ByteArray);
      Assert.AreEqual(new byte[] {}, (new ByteString(new byte[] {0x61, 0x62, 0x63}, 1, 0)).ByteArray);

      try {
        new ByteString(new byte[] {0x61, 0x62, 0x63}, 2, 2);
        Assert.Fail("exception not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        new ByteString(new byte[] {0x61, 0x62, 0x63}, 2, 2);
        Assert.Fail("exception not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        new ByteString(new byte[] {0x61, 0x62, 0x63}, -1, 4);
        Assert.Fail("exception not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestConstructFromString()
    {
      var s = new ByteString("abc");

      Assert.IsFalse(s.IsEmpty);
      Assert.AreEqual(3, s.Length);
      Assert.AreEqual(new byte[] {0x61, 0x62, 0x63}, s.ByteArray);
    }

    [Test]
    public void TestCreateEmpty()
    {
      var e = ByteString.CreateEmpty();

      Assert.IsTrue(e.IsEmpty);
      Assert.AreEqual(0, e.Length);

      Assert.IsFalse(Object.ReferenceEquals(e, ByteString.CreateEmpty()));
    }

    [Test]
    public void TestToByteArray()
    {
      var bytes = ByteString.ToByteArray("abc");

      Assert.AreEqual(new byte[] {0x61, 0x62, 0x63}, bytes);
    }

    [Test]
    public void TestContains()
    {
      var str = new ByteString("ababdabdbdabcab");

      Assert.IsTrue(str.Contains(new ByteString("abc")));
      Assert.IsTrue(str.Contains(new ByteString("abd")));
      Assert.IsFalse(str.Contains(new ByteString("abe")));
    }

    [Test]
    public void TestIndexOf()
    {
      var str = new ByteString("ababdabdbdabcab");

      Assert.AreEqual(10, str.IndexOf(new ByteString("abc")));
      Assert.AreEqual("abc", str.Substring(10, 3).ToString());
      Assert.AreEqual(2, str.IndexOf(new ByteString("abd")));
      Assert.AreEqual("abd", str.Substring(2, 3).ToString());
      Assert.AreEqual(-1, str.IndexOf(new ByteString("abe")));

      var substr = new ByteString("ab");

      Assert.AreEqual(0, str.IndexOf(substr));
      Assert.AreEqual(0, str.IndexOf(substr, 0));
      Assert.AreEqual(2, str.IndexOf(substr, 1));
      Assert.AreEqual(2, str.IndexOf(substr, 2));
      Assert.AreEqual(5, str.IndexOf(substr, 3));
      Assert.AreEqual(5, str.IndexOf(substr, 4));
      Assert.AreEqual(5, str.IndexOf(substr, 5));
    }

    [Test]
    public void TestIndexOfIgnoreCase()
    {
      var str = new ByteString("abcDEF");

      Assert.AreEqual(0, str.IndexOfIgnoreCase(new ByteString("ABC")));
      Assert.AreEqual(0, str.IndexOfIgnoreCase(new ByteString("Abc")));
      Assert.AreEqual(0, str.IndexOfIgnoreCase(new ByteString("abC")));
      Assert.AreEqual(0, str.IndexOfIgnoreCase(new ByteString("abc")));

      Assert.AreEqual(2, str.IndexOfIgnoreCase(new ByteString("cde")));
      Assert.AreEqual(2, str.IndexOfIgnoreCase(new ByteString("CDE")));
      Assert.AreEqual(2, str.IndexOfIgnoreCase(new ByteString("cdE")));
      Assert.AreEqual(2, str.IndexOfIgnoreCase(new ByteString("cDE")));
      Assert.AreEqual(2, str.IndexOfIgnoreCase(new ByteString("CDe")));
      Assert.AreEqual(2, str.IndexOfIgnoreCase(new ByteString("Cde")));
    }

    [Test]
    public void TestIndexOfString()
    {
      var str = new ByteString("ababdabdbdabcab");

      Assert.AreEqual(10, str.IndexOf("abc"));
      Assert.AreEqual(2, str.IndexOf("abd"));
      Assert.AreEqual(-1, str.IndexOf("abe"));

      Assert.AreEqual(0, str.IndexOf("ab"));
      Assert.AreEqual(0, str.IndexOf("ab", 0));
      Assert.AreEqual(2, str.IndexOf("ab", 1));
      Assert.AreEqual(2, str.IndexOf("ab", 2));
      Assert.AreEqual(5, str.IndexOf("ab", 3));
      Assert.AreEqual(5, str.IndexOf("ab", 4));
      Assert.AreEqual(5, str.IndexOf("ab", 5));
    }

    [Test]
    public void TestIndexOfNot()
    {
      ByteString str;

      str = new ByteString("aaabbb");

      Assert.AreEqual(3, str.IndexOfNot('a'));
      Assert.AreEqual(3, str.IndexOfNot(0x61));
      Assert.AreEqual(3, str.IndexOfNot('a', 3));
      Assert.AreEqual(3, str.IndexOfNot(0x61, 3));
      Assert.AreEqual(5, str.IndexOfNot('a', 5));
      Assert.AreEqual(5, str.IndexOfNot(0x61, 5));
      Assert.AreEqual(0, str.IndexOfNot('b'));
      Assert.AreEqual(0, str.IndexOfNot(0x62));
      Assert.AreEqual(-1, str.IndexOfNot('b', 3));
      Assert.AreEqual(-1, str.IndexOfNot(0x62, 3));
      Assert.AreEqual(-1, str.IndexOfNot('b', 5));
      Assert.AreEqual(-1, str.IndexOfNot(0x62, 5));
    }

    [Test]
    public void TestStartsWith()
    {
      var str = new ByteString("abcde");

      Assert.IsTrue(str.StartsWith(new ByteString("abc")));
      Assert.IsTrue(str.StartsWith(new ByteString("abcde")));
      Assert.IsFalse(str.StartsWith(new ByteString("abd")));
      Assert.IsFalse(str.StartsWith(new ByteString("abcdef")));
    }

    [Test]
    public void TestStartsWithIgnoreCase()
    {
      var str = new ByteString("aBC");

      Assert.IsTrue(str.StartsWithIgnoreCase(new ByteString("abc")));
      Assert.IsTrue(str.StartsWithIgnoreCase(new ByteString("aBc")));
      Assert.IsTrue(str.StartsWithIgnoreCase(new ByteString("aBC")));
      Assert.IsTrue(str.StartsWithIgnoreCase(new ByteString("ABc")));
      Assert.IsTrue(str.StartsWithIgnoreCase(new ByteString("AbC")));
      Assert.IsFalse(str.StartsWithIgnoreCase(new ByteString("abd")));
      Assert.IsFalse(str.StartsWithIgnoreCase(new ByteString("abcdef")));
    }

    [Test]
    public void TestStartsWithString()
    {
      var str = new ByteString("abcde");

      Assert.IsTrue(str.StartsWith("abc"));
      Assert.IsTrue(str.StartsWith("abcde"));
      Assert.IsFalse(str.StartsWith("abd"));
      Assert.IsFalse(str.StartsWith("abcdef"));
    }

    [Test]
    public void TestEndsWith()
    {
      var str = new ByteString("abcde");

      Assert.IsTrue(str.EndsWith(new ByteString("cde")));
      Assert.IsTrue(str.EndsWith(new ByteString("abcde")));
      Assert.IsFalse(str.EndsWith(new ByteString("cdd")));
      Assert.IsFalse(str.EndsWith(new ByteString("abcdef")));
    }

    [Test]
    public void TestEndsWithString()
    {
      var str = new ByteString("abcde");

      Assert.IsTrue(str.EndsWith("cde"));
      Assert.IsTrue(str.EndsWith("abcde"));
      Assert.IsFalse(str.EndsWith("cdd"));
      Assert.IsFalse(str.EndsWith("abcdef"));
    }

    [Test]
    public void TestIsPrefixOf()
    {
      var str = new ByteString("abc");

      Assert.IsTrue(str.IsPrefixOf(new ByteString("abcd")));
      Assert.IsTrue(str.IsPrefixOf(new byte[] {0x61, 0x62, 0x63, 0x64}));
      Assert.IsTrue(str.IsPrefixOf(new ByteString("abc")));
      Assert.IsTrue(str.IsPrefixOf(new byte[] {0x61, 0x62, 0x63}));
      Assert.IsFalse(str.IsPrefixOf(new ByteString("abd")));
      Assert.IsFalse(str.IsPrefixOf(new byte[] {0x61, 0x62, 0x64}));
      Assert.IsFalse(str.IsPrefixOf(new ByteString("ab")));
      Assert.IsFalse(str.IsPrefixOf(new byte[] {0x61, 0x62}));
    }

    [Test]
    public void TestSubstring()
    {
      var str = new ByteString("abcde");

      Assert.AreEqual(new ByteString("abcde"), str.Substring(0));
      Assert.AreEqual(new ByteString("abcde"), str.Substring(0, 5));
      Assert.AreEqual(new ByteString("cde"), str.Substring(2));
      Assert.AreEqual(new ByteString("cd"), str.Substring(2, 2));
    }

    [Test]
    public void TestSplit()
    {
      var splitted = (new ByteString(" a bc  def g ")).Split(' ');

      Assert.AreEqual(new[] {
        new ByteString(string.Empty),
        new ByteString("a"),
        new ByteString("bc"),
        new ByteString(string.Empty),
        new ByteString("def"),
        new ByteString("g"),
        new ByteString(string.Empty),
      }, splitted);
    }

    [Test]
    public void TestSplitNoDelimiters()
    {
      var splitted = (new ByteString("abcde")).Split(' ');

      Assert.AreEqual(new[] {new ByteString("abcde")}, splitted);
    }

    [Test]
    public void TestToUpper()
    {
      var str = new ByteString("`abcdefghijklmnopqrstuvwxyz{");

      Assert.AreEqual(new ByteString("`ABCDEFGHIJKLMNOPQRSTUVWXYZ{"), str.ToUpper());
    }

    [Test]
    public void TestToLower()
    {
      var str = new ByteString("@ABCDEFGHIJKLMNOPQRSTUVWXYZ[");

      Assert.AreEqual(new ByteString("@abcdefghijklmnopqrstuvwxyz["), str.ToLower());
    }

    [Test]
    public void TestToUInt32()
    {
      Assert.AreEqual(0U, (new ByteString("0")).ToUInt64());
      Assert.AreEqual(1234567890U, (new ByteString("1234567890")).ToUInt32());
    }

    [Test]
    public void TestToUInt32ContainsNonNumberCharacter()
    {
      ToNumberContainsNonNumberCharacter(32);
    }

    [Test, ExpectedException(typeof(OverflowException))]
    public void TestToUInt32Overflow()
    {
      (new ByteString("4294967296")).ToUInt32();
    }

    [Test]
    public void TestToUInt64()
    {
      Assert.AreEqual(0UL, (new ByteString("0")).ToUInt64());
      Assert.AreEqual(1234567890UL, (new ByteString("1234567890")).ToUInt64());
    }

    [Test]
    public void TestToUInt64ContainsNonNumberCharacter()
    {
      ToNumberContainsNonNumberCharacter(64);
    }

    private void ToNumberContainsNonNumberCharacter(int bits)
    {
      foreach (var test in new[] {
        "-1",
        "+1",
        "0x0123456",
        "1234567890a",
      }) {
        try {
          if (bits == 32)
            (new ByteString(test)).ToUInt32();
          else if (bits == 64)
            (new ByteString(test)).ToUInt64();
          Assert.Fail("FormatException not thrown");
        }
        catch (FormatException) {
        }
      }
    }

    [Test, ExpectedException(typeof(OverflowException))]
    public void TestToUInt64Overflow()
    {
      (new ByteString("18446744073709551616")).ToUInt64();
    }

    [Test]
    public void TestTrimStart()
    {
      var expected = new ByteString("abc");

      Assert.AreEqual(expected, (new ByteString("\u0020abc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\u00a0abc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\u0009abc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\u000aabc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\u000babc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\u000cabc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\u000dabc")).TrimStart());
      Assert.AreEqual(expected, (new ByteString("\r\n   abc")).TrimStart());
      Assert.AreEqual(new ByteString("!abc"), (new ByteString("!abc")).TrimStart());
      Assert.AreEqual(new ByteString("abc "), (new ByteString("abc ")).TrimStart());
      Assert.AreEqual(ByteString.CreateEmpty(), (new ByteString("   \r\n")).TrimStart());
    }

    [Test]
    public void TestTrimEnd()
    {
      var expected = new ByteString("abc");

      Assert.AreEqual(expected, (new ByteString("abc\u0020")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc\u00a0")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc\u0009")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc\u000a")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc\u000b")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc\u000c")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc\u000d")).TrimEnd());
      Assert.AreEqual(expected, (new ByteString("abc  \r\n")).TrimEnd());
      Assert.AreEqual(new ByteString("abc!"), (new ByteString("abc!")).TrimEnd());
      Assert.AreEqual(new ByteString(" abc"), (new ByteString(" abc")).TrimEnd());
      Assert.AreEqual(ByteString.CreateEmpty(), (new ByteString("   \r\n")).TrimEnd());
    }

    [Test]
    public void TestTrim()
    {
      Assert.AreEqual(new ByteString("abc"), (new ByteString("\n\nabc  ")).Trim());
      Assert.AreEqual(ByteString.CreateEmpty(), (new ByteString("   \r\n")).Trim());
    }

    [Test]
    public void TestEquals()
    {
      var str = new ByteString("abc");

      Assert.IsTrue(str.Equals(str));
      Assert.IsTrue(str.Equals(new ByteString("abc")));
      Assert.IsTrue(str.Equals(new byte[] {0x61, 0x62, 0x63}));

      Assert.IsTrue(str.Equals((object)str));
      Assert.IsTrue(str.Equals((object)new ByteString("abc")));
      Assert.IsTrue(str.Equals((object)new byte[] {0x61, 0x62, 0x63}));

      Assert.IsFalse(str.Equals((ByteString)null));
      Assert.IsFalse(str.Equals((string)null));
      Assert.IsFalse(str.Equals((byte[])null));
      Assert.IsFalse(str.Equals((object)1));
      Assert.IsFalse(str.Equals(new ByteString("ABC")));
    }

    [Test]
    public void TestEqualsIgnoreCase()
    {
      var a = new ByteString("abc");
      var b = new ByteString("ABC");
      var c = new ByteString("Abc");
      var d = new ByteString("AbcD");

      Assert.IsTrue(a.EqualsIgnoreCase(b));
      Assert.IsTrue(a.EqualsIgnoreCase(c));
      Assert.IsFalse(a.EqualsIgnoreCase(d));
    }

    [Test]
    public void TestOperatorEquality()
    {
      var x = new ByteString("abc");
      var y = x;

      Assert.IsTrue(x == y);
      Assert.IsTrue(x == new ByteString("abc"));
      Assert.IsFalse(x == new ByteString("ABC"));
      Assert.IsTrue(new ByteString("abc") == x);
      Assert.IsFalse(new ByteString("ABC") == x);
      Assert.IsFalse(x == null);
      Assert.IsFalse(null == x);
    }

    [Test]
    public void TestOperatorInquality()
    {
      var x = new ByteString("abc");
      var y = x;

      Assert.IsFalse(x != y);
      Assert.IsFalse(x != new ByteString("abc"));
      Assert.IsTrue(x != new ByteString("ABC"));
      Assert.IsFalse(new ByteString("abc") != x);
      Assert.IsTrue(new ByteString("ABC") != x);
      Assert.IsTrue(x != null);
      Assert.IsTrue(null != x);
    }

    [Test]
    public void TestOperatorAddition()
    {
      var x = new ByteString("abc");
      var y = new ByteString("xyz");
      var z = (ByteString)null;

      Assert.AreEqual(new ByteString("abcxyz"), x + y);
      Assert.AreEqual(new ByteString("xyzabc"), y + x);

      try {
        Assert.Fail("x + z = " + (x + z).ToString());
      }
      catch (Exception) {
      }

      try {
        Assert.Fail("z + x = " + (z + x).ToString());
      }
      catch (Exception) {
      }
    }

    [Test]
    public void TestOperatorMultiply()
    {
      var x = new ByteString("abc");

      Assert.AreEqual(new ByteString(""), x * 0);
      Assert.AreEqual(new ByteString("abc"), x * 1);
      Assert.AreEqual(new ByteString("abcabc"), x * 2);

      try {
        Assert.Fail("x * -1 = " + (x * -1).ToString());
      }
      catch (Exception) {
      }
    }

    [Test]
    public void TestConcat()
    {
      Assert.AreEqual(new ByteString("abcdefghi"), ByteString.Concat(new ByteString("abc"), new ByteString("def"), new ByteString("ghi")));
      Assert.AreEqual(new ByteString("abcghi"), ByteString.Concat(new ByteString("abc"), null, new ByteString("ghi")));
      Assert.AreEqual(new ByteString(""), ByteString.Concat(null, null, null));
    }

    [Test]
    public void TestGetHashCode()
    {
      Assert.AreEqual((new ByteString("abc")).GetHashCode(), (new ByteString(new byte[] {0x61, 0x62, 0x63})).GetHashCode());
      Assert.AreEqual((new ByteString("abc")).GetHashCode(), (new ByteString("abc")).GetHashCode());
      Assert.AreNotEqual((new ByteString("abc")).GetHashCode(), (new ByteString("abd")).GetHashCode());
      Assert.AreNotEqual((new ByteString("abc")).GetHashCode(), (new ByteString("abcd")).GetHashCode());
      Assert.AreNotEqual((new ByteString("abc")).GetHashCode(), ByteString.CreateEmpty().GetHashCode());
    }

    [Test]
    public void TestToString()
    {
      var str = new ByteString(0x61, 0x62, 0x63);

      Assert.AreEqual("abc", str.ToString());
    }

    [Test]
    public void TestToStringPartial()
    {
      var str = new ByteString("abcdefghi");

      Assert.AreEqual("abcdefghi", str.ToString(0));
      Assert.AreEqual("", str.ToString(0, 0));
      Assert.AreEqual("abc", str.ToString(0, 3));
      Assert.AreEqual("abcdefghi", str.ToString(0, 9));
      Assert.AreEqual("defghi", str.ToString(3));
      Assert.AreEqual("", str.ToString(3, 0));
      Assert.AreEqual("def", str.ToString(3, 3));
      Assert.AreEqual("defghi", str.ToString(3, 6));
      Assert.AreEqual("i", str.ToString(8, 1));
    }

    [Test]
    public void TestBinarySerialization()
    {
      var toSerialize = new ByteString("abc");
      var serializeFormatter = new BinaryFormatter();
      var stream = new MemoryStream();

      serializeFormatter.Serialize(stream, toSerialize);

      stream.Position = 0L;

      var deserializeFormatter = new BinaryFormatter();
      var deserialized = (ByteString)deserializeFormatter.Deserialize(stream);

      Assert.AreNotSame(toSerialize, deserialized);
      Assert.AreEqual(toSerialize, deserialized);
      Assert.IsTrue(toSerialize.Equals(deserialized));
      Assert.IsTrue(deserialized.Equals(toSerialize));
    }
  }
}