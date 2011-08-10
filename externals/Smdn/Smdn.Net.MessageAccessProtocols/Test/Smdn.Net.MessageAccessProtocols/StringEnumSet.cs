using System;
using System.Collections.Generic;
using NUnit.Framework;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.MessageAccessProtocols {
  [TestFixture]
  public class StringEnumSetTests {
    private class StringEnum : IStringEnum  {
      public string Value {
        get; private set;
      }

      public StringEnum(string val)
      {
        this.Value = val;
      }

      public override bool Equals(object obj)
      {
        if (obj == null)
          return false;
        else if (obj is IStringEnum)
          return Equals((obj as IStringEnum).Value);
        else if (obj is string)
          return Equals(obj as string);
        else
          return false;
      }

      public bool Equals(IStringEnum other)
      {
        return Equals(other.Value);
      }

      public bool Equals(string other)
      {
        return string.Equals(this.Value, other, StringComparison.OrdinalIgnoreCase);
      }

      public override int GetHashCode()
      {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value);
      }
    }

    private class TestStringEnumSet : StringEnumSet<StringEnum> {
      public TestStringEnumSet()
        : base(false, StringComparer.OrdinalIgnoreCase)
      {
      }

      public TestStringEnumSet(bool readOnly, IEnumerable<StringEnum> values)
        : base(readOnly, values, StringComparer.OrdinalIgnoreCase)
      {
      }
    }

    [Test]
    public void TestConstruct()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsFalse(set.IsReadOnly);
      Assert.AreEqual(3, set.Count);
    }

    [Test]
    public void TestConstructContainsSameKey()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("IMAP4rev1"),
      });

      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(set.Contains("IMAP4rev1"));
    }

    [Test]
    public void TestContains()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(set.Contains(new StringEnum("IMAP4rev1")));
      Assert.IsTrue(set.Contains("IMAP4rev1"));
      Assert.IsTrue(set.Contains(new StringEnum("imap4REV1")));
      Assert.IsTrue(set.Contains("imap4REV1"));
      Assert.IsFalse(set.Contains(new StringEnum("AUTH=DIGEST-MD5")));
      Assert.IsFalse(set.Contains("AUTH=DIGEST-MD5"));
    }

    [Test]
    public void TestFind()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var set = new TestStringEnumSet(false, values);

      Assert.AreEqual(values[0], set.Find("imap4rev1"));
      Assert.AreEqual(values[1], set.Find(values[1].Value));
      Assert.IsNull(set.Find("AUTH=digest-md5"));
    }

    [Test]
    public void TestClear()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var set = new TestStringEnumSet(false, values);

      Assert.IsNotNull(set.Find("IMAP4rev1"));
      Assert.AreEqual(3, set.Count);

      set.Clear();

      Assert.IsNull(set.Find("IMAP4rev1"));
      Assert.AreEqual(0, set.Count);
    }

    [Test]
    public void TestToArray()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var set = new TestStringEnumSet(false, values);

      CollectionAssert.AreEquivalent(values, set.ToArray());
    }

    [Test]
    public void TestCopyTo()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var set = new TestStringEnumSet(false, values);

      var copied = new StringEnum[3];

      set.CopyTo(copied, 0);

      CollectionAssert.AreEquivalent(values, copied);
    }

    [Test]
    public void TestToStringArray()
    {
      var values = new[] {
        "IMAP4rev1",
        "UIDPLUS",
        "AUTH=PLAIN",
      };

      var set = new TestStringEnumSet(false, Array.ConvertAll(values, delegate(string val) {
        return new StringEnum(val);
      }));

      CollectionAssert.AreEquivalent(values, set.ToStringArray());
    }

    [Test]
    public void TestToString()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.AreEqual("IMAP4rev1, UIDPLUS, AUTH=PLAIN", set.ToString());
    }

    [Test]
    public void TestAddSameKey()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
      });

      Assert.AreEqual(1, set.Count);

      Assert.IsFalse(set.Add(new StringEnum("IMAP4rev1")));
      Assert.AreEqual(1, set.Count);
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestAddToReadOnlyList()
    {
      var set = new TestStringEnumSet(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(set.IsReadOnly);

      set.Add(new StringEnum("STARTTLS"));
    }

    [Test]
    public void TestRemove()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.AreEqual(3, set.Count, "initial count");

      Assert.IsTrue(set.Remove(new StringEnum("imap4rev1")), "Remove #0");
      Assert.AreEqual(2, set.Count, "Count #0");

      Assert.IsFalse(set.Remove(new StringEnum("imap4rev1")), "Remove #1");
      Assert.AreEqual(2, set.Count, "Count #1");

      CollectionAssert.AreEquivalent(ToStringEnums(new[] {"UIDPLUS", "AUTH=PLAIN"}), set, "equivalent Remove #1");
    }

    [Test]
    public void TestRemoveWhere()
    {
      var set = new TestStringEnumSet(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsFalse(set.IsReadOnly);

      Assert.AreEqual(1, set.RemoveWhere(delegate(StringEnum s) {
        return s.Value.StartsWith("AUTH=");
      }));

      Assert.AreEqual(2, set.Count);
      Assert.IsFalse(set.Contains("AUTH=PLAIN"));
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestRemoveWhereFromReadOnlyList()
    {
      var set = new TestStringEnumSet(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(set.IsReadOnly);

      set.RemoveWhere(delegate(StringEnum s) {
        return 0 < s.Value.Length;
      });
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestRemoveFromReadOnlyList()
    {
      var set = new TestStringEnumSet(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(set.IsReadOnly);

      set.Remove(new StringEnum("IMAP4rev1"));
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestClearReadOnlyList()
    {
      var set = new TestStringEnumSet(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(set.IsReadOnly);

      set.Clear();
    }

    [Test]
    public void TestGetEnumerator()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var set = new TestStringEnumSet(false, values);
      var count = 0;

      foreach (var val in set) {
        count++;
      }

      Assert.AreEqual(3, count);
    }

    private IEnumerable<StringEnum> ToStringEnums(IEnumerable<string> enumerable)
    {
      return enumerable.Select(delegate(string s) {
        return new StringEnum(s);
      });
    }

    [Test]
    public void TestExceptWith()
    {
      var setInitial = ToStringEnums(new[] {"a", "c", "d", "b", "e"}).ToArray();
      var s = new TestStringEnumSet(false, setInitial);

      s.ExceptWith(new StringEnum[0]);

      Assert.AreEqual(5, s.Count, "Count ExceptWith #0");
      CollectionAssert.AreEquivalent(setInitial, s, "equivalent ExceptWith #0");

      s = new TestStringEnumSet(false, setInitial);
      s.ExceptWith(ToStringEnums(new[] {"b", "d", "e", "x", "y"}));

      Assert.AreEqual(2, s.Count, "Count ExceptWith #1");
      CollectionAssert.AreEquivalent(ToStringEnums(new[] {"a", "c"}), s, "equivalent ExceptWith #1");
    }

    [Test]
    public void TestIntersectWith()
    {
      var setInitial = ToStringEnums(new[] {"a", "c", "d", "b", "e"}).ToArray();
      var s = new TestStringEnumSet(false, setInitial);

      s.IntersectWith(new StringEnum[0]);

      Assert.AreEqual(0, s.Count, "Count IntersectWith #0");
      CollectionAssert.AreEquivalent(new int[] {}, s, "equivalent IntersectWith #0");

      s = new TestStringEnumSet(false, setInitial);
      s.IntersectWith(ToStringEnums(new[] {"b", "d", "e", "x", "y"}));

      Assert.AreEqual(3, s.Count, "Count IntersectWith #1");
      CollectionAssert.AreEquivalent(ToStringEnums(new[] {"b", "d", "e"}).ToArray(), s, "equivalent IntersectWith #1");
    }

    [Test]
    public void TestSymmetricExceptWith()
    {
      var setInitial = ToStringEnums(new[] {"a", "c", "d", "b", "e"}).ToArray();
      var s = new TestStringEnumSet(false, setInitial);

      s.SymmetricExceptWith(new StringEnum[0]);

      Assert.AreEqual(5, s.Count, "Count SymmetricExceptWith #0");
      CollectionAssert.AreEquivalent(setInitial, s, "equivalent SymmetricExceptWith #0");

      s.SymmetricExceptWith(ToStringEnums(new[] {"b", "d", "e", "x", "y"}));

      Assert.AreEqual(4, s.Count, "Count SymmetricExceptWith #1");
      CollectionAssert.AreEquivalent(ToStringEnums(new[] {"a", "c", "x", "y"}).ToArray(), s, "equivalent SymmetricExceptWith #1");
    }

    [Test]
    public void TestUnionWith()
    {
      var setInitial = ToStringEnums(new[] {"a", "c", "d", "b", "e"}).ToArray();
      var s = new TestStringEnumSet(false, setInitial);

      s.UnionWith(new StringEnum[0]);

      Assert.AreEqual(5, s.Count, "Count UnionWith #0");
      CollectionAssert.AreEquivalent(setInitial, s, "equivalent UnionWith #0");

      s.UnionWith(ToStringEnums(new[] {"b", "d", "e", "x", "y"}));

      Assert.AreEqual(7, s.Count, "Count UnionWith #1");
      CollectionAssert.AreEquivalent(ToStringEnums(new[] {"a", "b", "c", "d", "e", "x", "y"}).ToArray(), s, "equivalent UnionWith #1");
    }

    [Test]
    public void TestIsSubsetOf()
    {
      var s = new TestStringEnumSet(false, ToStringEnums(new[] {"a", "c", "b"}));

      Assert.IsTrue(s.IsSubsetOf(ToStringEnums(new[] {"A", "b", "C", "d"})), "IsSubsetOf #0");
      Assert.IsTrue(s.IsSubsetOf(ToStringEnums(new[] {"A", "b", "C"})), "IsSubsetOf #1");
      Assert.IsFalse(s.IsSubsetOf(ToStringEnums(new[] {"A", "b", "d", "x"})), "IsSubsetOf #2");
      Assert.IsFalse(s.IsSubsetOf(ToStringEnums(new[] {"A", "b", "d"})), "IsSubsetOf #3");
    }

    [Test]
    public void TestIsSubsetOfEmpty()
    {
      var s = new TestStringEnumSet();

      Assert.IsTrue(s.IsSubsetOf(ToStringEnums(new[] {"x"})), "IsSubsetOf #0");
      Assert.IsTrue(s.IsSubsetOf(ToStringEnums(new string[0])), "IsSubsetOf #1");

      s = new TestStringEnumSet(false, ToStringEnums(new[] {"x"}));

      Assert.IsFalse(s.IsSubsetOf(ToStringEnums(new string[0])), "IsSubsetOf #2");
    }

    [Test]
    public void TestIsProperSubsetOf()
    {
      var s = new TestStringEnumSet(false, ToStringEnums(new[] {"a", "c", "b"}));

      Assert.IsTrue(s.IsProperSubsetOf(ToStringEnums(new[] {"A", "b", "C", "d"})), "IsProperSubsetOf #0");
      Assert.IsFalse(s.IsProperSubsetOf(ToStringEnums(new[] {"A", "b", "C"})), "IsProperSubsetOf #1");
      Assert.IsFalse(s.IsProperSubsetOf(ToStringEnums(new[] {"A", "b", "d", "x"})), "IsProperSubsetOf #2");
      Assert.IsFalse(s.IsProperSubsetOf(ToStringEnums(new[] {"A", "b", "d"})), "IsProperSubsetOf #3");
    }

    [Test]
    public void TestIsProperSubsetOfEmpty()
    {
      var s = new TestStringEnumSet();

      Assert.IsFalse(s.IsProperSubsetOf(ToStringEnums(new[] {"x"})), "IsProperSubsetOf #0");
      Assert.IsTrue(s.IsProperSubsetOf(ToStringEnums(new string[0])), "IsProperSubsetOf #1");

      s = new TestStringEnumSet(false, ToStringEnums(new[] {"x"}));

      Assert.IsFalse(s.IsProperSubsetOf(ToStringEnums(new string[0])), "IsProperSubsetOf #2");
    }

    [Test]
    public void TestIsSupersetOf()
    {
      var s = new TestStringEnumSet(false, ToStringEnums(new[] {"a", "c", "b"}));

      Assert.IsTrue(s.IsSupersetOf(ToStringEnums(new[] {"A", "b"})), "IsSupersetOf #0");
      Assert.IsTrue(s.IsSupersetOf(ToStringEnums(new[] {"A", "b", "C"})), "IsSupersetOf #1");
      Assert.IsFalse(s.IsSupersetOf(ToStringEnums(new[] {"A", "b", "d", "x"})), "IsSupersetOf #2");
      Assert.IsFalse(s.IsSupersetOf(ToStringEnums(new[] {"A", "b", "d"})), "IsSupersetOf #3");
    }

    [Test]
    public void TestIsSupersetOfEmpty()
    {
      var s = new TestStringEnumSet();

      Assert.IsFalse(s.IsSupersetOf(ToStringEnums(new[] {"x"})), "IsSupersetOf #0");
      Assert.IsTrue(s.IsSupersetOf(ToStringEnums(new string[0])), "IsSupersetOf #1");

      s = new TestStringEnumSet(false, ToStringEnums(new[] {"x"}));

      Assert.IsTrue(s.IsSupersetOf(ToStringEnums(new string[0])), "IsSupersetOf #2");
    }

    [Test]
    public void TestIsProperSupersetOf()
    {
      var s = new TestStringEnumSet(false, ToStringEnums(new[] {"a", "c", "b"}));

      Assert.IsTrue(s.IsProperSupersetOf(ToStringEnums(new[] {"A", "b"})), "IsProperSupersetOf #0");
      Assert.IsFalse(s.IsProperSupersetOf(ToStringEnums(new[] {"A", "b", "C"})), "IsProperSupersetOf #1");
      Assert.IsFalse(s.IsProperSupersetOf(ToStringEnums(new[] {"A", "b", "d", "x"})), "IsProperSupersetOf #2");
      Assert.IsFalse(s.IsProperSupersetOf(ToStringEnums(new[] {"A", "b", "d"})), "IsProperSupersetOf #3");
    }

    [Test]
    public void TestIsProperSupersetOfEmpty()
    {
      var s = new TestStringEnumSet();

      Assert.IsFalse(s.IsProperSupersetOf(ToStringEnums(new[] {"x"})), "IsProperSupersetOf #0");
      Assert.IsFalse(s.IsProperSupersetOf(ToStringEnums(new string[0])), "IsProperSupersetOf #1");

      s = new TestStringEnumSet(false, ToStringEnums(new[] {"x"}));

      Assert.IsTrue(s.IsProperSupersetOf(ToStringEnums(new string[0])), "IsProperSupersetOf #2");
    }

    [Test]
    public void TestOverlaps()
    {
      var s = new TestStringEnumSet(false, ToStringEnums(new[] {"a", "c", "b"}));

      Assert.IsTrue(s.Overlaps(ToStringEnums(new[] {"a", "B", "d"})), "Overlaps #0");
      Assert.IsFalse(s.Overlaps(ToStringEnums(new[] {"D", "e"})), "Overlaps #1");
      Assert.IsFalse(s.Overlaps(ToStringEnums(new string[0])), "Overlaps #2");
    }

    [Test]
    public void TestOverlapsEmpty()
    {
      var s = new TestStringEnumSet();

      Assert.IsFalse(s.Overlaps(ToStringEnums(new[] {"x"})), "Overlaps #1");
      Assert.IsFalse(s.Overlaps(ToStringEnums(new string[0])), "Overlaps #2");
    }

    [Test]
    public void TestSetEquals()
    {
      var s = new TestStringEnumSet(false, ToStringEnums(new[] {"a", "b", "c"}));

      Assert.IsTrue(s.SetEquals(ToStringEnums(new[] {"A", "b", "C"})), "SetEquals #0");
      Assert.IsFalse(s.SetEquals(ToStringEnums(new[] {"A", "b", "D"})), "SetEquals #1");
      Assert.IsFalse(s.SetEquals(ToStringEnums(new[] {"A", "b"})), "SetEquals #2");
      Assert.IsFalse(s.SetEquals(ToStringEnums(new string[0])), "SetEquals #3");
    }
  }
}
