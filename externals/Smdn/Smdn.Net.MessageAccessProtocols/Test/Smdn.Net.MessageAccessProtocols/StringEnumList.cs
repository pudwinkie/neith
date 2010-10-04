using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Smdn.Net.MessageAccessProtocols {
  [TestFixture]
  public class StringEnumListTests {
    private class StringEnum : IStringEnum {
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

    private class StringEnumList : StringEnumList<StringEnum> {
      public StringEnumList(bool readOnly, IEnumerable<StringEnum> values)
        : base(readOnly, values)
      {
      }
    }

    [Test]
    public void TestConstruct()
    {
      var list = new StringEnumList(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsFalse(list.IsReadOnly);
      Assert.AreEqual(3, list.Count);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestConstructContainsSameKey()
    {
      new StringEnumList(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("IMAP4rev1"),
      });
    }

    [Test]
    public void TestHas()
    {
      var list = new StringEnumList(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.Has(new StringEnum("IMAP4rev1")));
      Assert.IsTrue(list.Has("IMAP4rev1"));
      Assert.IsTrue(list.Has(new StringEnum("imap4REV1")));
      Assert.IsTrue(list.Has("imap4REV1"));
      Assert.IsFalse(list.Has(new StringEnum("AUTH=DIGEST-MD5")));
      Assert.IsFalse(list.Has("AUTH=DIGEST-MD5"));
    }

    [Test]
    public void TestFind()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var list = new StringEnumList(false, values);

      Assert.AreEqual(values[0], list.Find("imap4rev1"));
      Assert.AreEqual(values[1], list.Find(values[1].Value));
      Assert.IsNull(list.Find("AUTH=digest-md5"));
    }

    [Test]
    public void TestClear()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var list = new StringEnumList(false, values);

      Assert.IsNotNull(list.Find("IMAP4rev1"));
      Assert.AreEqual(3, list.Count);

      list.Clear();

      Assert.IsNull(list.Find("IMAP4rev1"));
      Assert.AreEqual(0, list.Count);
    }

    [Test]
    public void TestToArray()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var list = new StringEnumList(false, values);

      CollectionAssert.AreEquivalent(values, list.ToArray());
    }

    [Test]
    public void TestCopyTo()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var list = new StringEnumList(false, values);

      var copied = new StringEnum[3];

      list.CopyTo(copied, 0);

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

      var list = new StringEnumList(false, Array.ConvertAll(values, delegate(string val) {
        return new StringEnum(val);
      }));

      CollectionAssert.AreEquivalent(values, list.ToStringArray());
    }

    [Test]
    public void TestToString()
    {
      var list = new StringEnumList(false, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.AreEqual("IMAP4rev1, UIDPLUS, AUTH=PLAIN", list.ToString());
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestAddSameKey()
    {
      var list = new StringEnumList(false, new[] {
        new StringEnum("IMAP4rev1"),
      });

      list.Add(new StringEnum("IMAP4rev1"));
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestAddToReadOnlyList()
    {
      var list = new StringEnumList(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.IsReadOnly);

      list.Add(new StringEnum("STARTTLS"));
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestRemoveFromReadOnlyList()
    {
      var list = new StringEnumList(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.IsReadOnly);

      list.Remove(new StringEnum("IMAP4rev1"));
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestClearReadOnlyList()
    {
      var list = new StringEnumList(true, new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.IsReadOnly);

      list.Clear();
    }

    [Test]
    public void TestGetEnumerator()
    {
      var values = new[] {
        new StringEnum("IMAP4rev1"),
        new StringEnum("UIDPLUS"),
        new StringEnum("AUTH=PLAIN"),
      };

      var list = new StringEnumList(false, values);
      var count = 0;

      foreach (var val in list) {
        count++;
      }

      Assert.AreEqual(3, count);
    }
  }
}
