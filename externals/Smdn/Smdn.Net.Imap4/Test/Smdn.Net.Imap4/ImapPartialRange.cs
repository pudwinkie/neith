using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapPartialRangeTests {
    [Test]
    public void TestConstruct()
    {
      ImapPartialRange r;

      r = new ImapPartialRange(0L);

      Assert.AreEqual(0L, r.Start);
      Assert.IsNull(r.Length);
      Assert.IsFalse(r.IsLengthSpecified);
      Assert.AreEqual("<0>", r.ToString());
      Assert.AreEqual("/;PARTIAL=0", r.ToString("u"));

      r = new ImapPartialRange(0L, 1024L);

      Assert.AreEqual(0L, r.Start);
      Assert.AreEqual(1024L, r.Length);
      Assert.IsTrue(r.IsLengthSpecified);
      Assert.AreEqual("<0.1024>", r.ToString());
      Assert.AreEqual("<0.1024>", r.ToString("f"));
      Assert.AreEqual("/;PARTIAL=0.1024", r.ToString("u"));

      r = new ImapPartialRange(0L, (long)0xffffffff);

      Assert.AreEqual(0L, r.Start);
      Assert.AreEqual(4294967295L, r.Length);
      Assert.IsTrue(r.IsLengthSpecified);
      Assert.AreEqual("<0.4294967295>", r.ToString());
      Assert.AreEqual("<0.4294967295>", r.ToString("f"));
      Assert.AreEqual("/;PARTIAL=0.4294967295", r.ToString("u"));

      r = new ImapPartialRange((long)0xfffffffe, 1L);

      Assert.AreEqual(4294967294L, r.Start);
      Assert.AreEqual(1L, r.Length);
      Assert.IsTrue(r.IsLengthSpecified);
      Assert.AreEqual("<4294967294.1>", r.ToString());
      Assert.AreEqual("<4294967294.1>", r.ToString("f"));
      Assert.AreEqual("/;PARTIAL=4294967294.1", r.ToString("u"));
    }

    [Test]
    public void TestSetStartLength()
    {
      var r = new ImapPartialRange(0L);

      Assert.AreEqual(0L, r.Start);
      Assert.IsNull(r.Length);
      Assert.AreEqual("<0>", r.ToString());
      Assert.AreEqual("/;PARTIAL=0", r.ToString("u"));

      r.Start = 1024L;

      Assert.AreEqual(1024L, r.Start);
      Assert.IsNull(r.Length);
      Assert.AreEqual("<1024>", r.ToString());
      Assert.AreEqual("/;PARTIAL=1024", r.ToString("u"));

      r.Length = 1024L;

      Assert.AreEqual(1024L, r.Start);
      Assert.AreEqual(1024L, r.Length);
      Assert.AreEqual("<1024.1024>", r.ToString());
      Assert.AreEqual("<1024.1024>", r.ToString("f"));
      Assert.AreEqual("/;PARTIAL=1024.1024", r.ToString("u"));

      r.Start = 2048L;
      r.Length = null;

      Assert.AreEqual(2048L, r.Start);
      Assert.IsNull(r.Length);
      Assert.AreEqual("<2048>", r.ToString());
      Assert.AreEqual("/;PARTIAL=2048", r.ToString("u"));
    }

    [Test]
    public void TestStartOutOfRange()
    {
      foreach (var start in new[] {
        -1L, 0x100000000,
      }) {
        try {
          new ImapPartialRange(start, 1L);

          Assert.Fail("ArgumentOutOfRangeException not thrown");
        }
        catch (ArgumentOutOfRangeException) {
        }

        try {
          var r = new ImapPartialRange(0L, 1L);

          r.Start = start;

          Assert.Fail("ArgumentOutOfRangeException not thrown");
        }
        catch (ArgumentOutOfRangeException) {
        }
      }
    }

    [Test]
    public void TestLengthOutOfRange()
    {
      foreach (var length in new[] {
        -1L, 0L, 0x100000000,
      }) {
        try {
          new ImapPartialRange(0L, length);

          Assert.Fail("ArgumentOutOfRangeException not thrown");
        }
        catch (ArgumentOutOfRangeException) {
        }

        try {
          var r = new ImapPartialRange(0L, length);

          r.Length = length;

          Assert.Fail("ArgumentOutOfRangeException not thrown");
        }
        catch (ArgumentOutOfRangeException) {
        }
      }
    }

    [Test]
    public void TestStartLengthOutOfRange()
    {
      foreach (var pair in new[] {
        new {Start = 0L,                Length = (long)0x100000000},
        new {Start = 1L,                Length = (long)0x0ffffffff},
        new {Start = (long)0x0ffffffff, Length = 1L},
      }) {
        try {
          new ImapPartialRange(pair.Start, pair.Length);

          Assert.Fail("ArgumentOutOfRangeException not thrown");
        }
        catch (ArgumentOutOfRangeException) {
        }

        var r = new ImapPartialRange(0L, 1L);

        r.Start = pair.Start;

        try {
          r.Length = pair.Length;

          Assert.Fail("ArgumentOutOfRangeException not thrown");
        }
        catch (ArgumentOutOfRangeException) {
        }
      }
    }

    [Test]
    public void TestEquals()
    {
      var r = new ImapPartialRange(0L, 1024L);

      Assert.IsTrue(r.Equals(r));
      Assert.IsTrue(r.Equals(new ImapPartialRange(0L, 1024L)));
      Assert.IsTrue((new ImapPartialRange(0L, 1024L)).Equals(r));

      Assert.IsFalse(r.Equals(0));
      Assert.IsFalse(r.Equals("<0.1024>"));
      Assert.IsFalse(r.Equals(new ImapPartialRange(1L, 1024L)));
      Assert.IsFalse(r.Equals(new ImapPartialRange(0L, 1023L)));
    }

    [Test]
    public void TestToStringFormatException()
    {
      foreach (var pair in new[] {
        new {Format = "f", Range = new ImapPartialRange(1024L)},
        new {Format = "f", Range = default(ImapPartialRange)},
      }) {
        try {
          pair.Range.ToString(pair.Format);
          Assert.Fail("FormatException not thrown: format = '{0}', range = {1}", pair.Format, pair.Range);
        }
        catch (FormatException) {
        }
      }
    }
  }
}