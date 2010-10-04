using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapSequenceSetTest {
    [Test]
    public void TestSet()
    {
      ImapSequenceSet s;

      s = ImapSequenceSet.CreateSet(2);

      Assert.AreEqual("2", s.ToString());
      Assert.IsTrue(s.IsSingle);

      s = ImapSequenceSet.CreateSet(2, 4, 5, 7);

      Assert.AreEqual("2,4,5,7", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);

      s = ImapSequenceSet.CreateSet(new long[] {1, 2, 3, 4, 5, 6, 7, 8});

      Assert.AreEqual("1,2,3,4,5,6,7,8", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);

      s = ImapSequenceSet.CreateSet(new long[] {1, 2, 3, 4, 5, 6, 7, 8}, 3, 4);

      Assert.AreEqual("4,5,6,7", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);

      s = ImapSequenceSet.CreateSet(new long[] {1, 2, 3, 4, 5, 6, 7, 8}, 7, 1);

      Assert.AreEqual("8", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsTrue(s.IsSingle);
    }

    [Test]
    public void TestSetFromArray()
    {
      var arr = new long[] {1, 2, 3, 4, 5, 6, 7, 8};
      var s = ImapSequenceSet.CreateSet(arr);

      Assert.AreEqual("1,2,3,4,5,6,7,8", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);

      Array.Clear(arr, 0, arr.Length);

      Assert.AreEqual("1,2,3,4,5,6,7,8", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);
    }

    [Test]
    public void TestSetToArray()
    {
      var s = ImapSequenceSet.CreateSet(2, 4, 5, 7).ToArray();

      Assert.AreEqual(4, s.Length);
      Assert.AreEqual(2, s[0]);
      Assert.AreEqual(4, s[1]);
      Assert.AreEqual(5, s[2]);
      Assert.AreEqual(7, s[3]);
    }

    [Test]
    public void TestSetGetEnumerator()
    {
      var expected = new[] {2L, 4L, 5L, 7L};
      var index = 0;

      foreach (var num in ImapSequenceSet.CreateSet(2, 4, 5, 7)) {
        Assert.AreEqual(expected[index++], num);
      }
    }

    [Test]
    public void TestSetToNumberSingle()
    {
      Assert.AreEqual(3L, ImapSequenceSet.CreateSet(3).ToNumber());
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestSetToNumberNotSingle()
    {
      ImapSequenceSet.CreateSet(2, 4, 5, 7).ToNumber();
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestSetToNumberEmpty()
    {
      ImapSequenceSet.CreateSet(new long[0]).ToNumber();
    }

    [Test]
    public void TestRange()
    {
      ImapSequenceSet s;

      s = ImapSequenceSet.CreateRangeSet(15, 18);

      Assert.AreEqual("15:18", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);

      s = ImapSequenceSet.CreateRangeSet(15L, 15L);

      Assert.AreEqual("15", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsTrue(s.IsSingle);
    }

    [Test]
    public void TestRangeToArray()
    {
      var s = ImapSequenceSet.CreateRangeSet(15, 18).ToArray();

      Assert.AreEqual(4, s.Length);
      Assert.AreEqual(15, s[0]);
      Assert.AreEqual(16, s[1]);
      Assert.AreEqual(17, s[2]);
      Assert.AreEqual(18, s[3]);

      s = ImapSequenceSet.CreateRangeSet(15L, 15L).ToArray();

      Assert.AreEqual(1, s.Length);
      Assert.AreEqual(15L, s[0]);
    }

    [Test]
    public void TestRangeGetEnumerator()
    {
      var expected = new[] {15L, 16L, 17L, 18L};
      var index = 0;

      foreach (var num in ImapSequenceSet.CreateRangeSet(15, 18)) {
        Assert.AreEqual(expected[index++], num);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestRangeToNumber()
    {
      ImapSequenceSet.CreateRangeSet(15, 18).ToNumber();
    }

    [Test]
    public void TestFrom()
    {
      var s = ImapSequenceSet.CreateFromSet(72);

      Assert.AreEqual("72:*", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestFromToArray()
    {
      ImapSequenceSet.CreateFromSet(72).ToArray();
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestFromGetEnumerator()
    {
      foreach (var num in ImapSequenceSet.CreateFromSet(72)) {
        Assert.Fail("enumerated: {0}", num);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestFromToNumber()
    {
      ImapSequenceSet.CreateFromSet(72).ToNumber();
    }

    [Test]
    public void TestTo()
    {
      var s = ImapSequenceSet.CreateToSet(25);

      Assert.AreEqual("*:25", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestToToArray()
    {
      ImapSequenceSet.CreateToSet(25).ToArray();
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestToGetEnumerator()
    {
      foreach (var num in ImapSequenceSet.CreateToSet(25)) {
        Assert.Fail("enumerated: {0}", num);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestToToNumber()
    {
      ImapSequenceSet.CreateToSet(25).ToNumber();
    }

    [Test]
    public void TestAll()
    {
      var s = ImapSequenceSet.CreateAllSet();

      Assert.AreEqual("*", s.ToString());
      Assert.IsFalse(s.IsEmpty);
      Assert.IsFalse(s.IsSingle);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestAllToArray()
    {
      ImapSequenceSet.CreateAllSet().ToArray();
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestAllGetEnumerator()
    {
      foreach (var num in ImapSequenceSet.CreateAllSet()) {
        Assert.Fail("enumerated: {0}", num);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestAllToNumber()
    {
      ImapSequenceSet.CreateAllSet().ToNumber();
    }

    [Test]
    public void TestCombine1()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(1, 3);
      var set2 = ImapSequenceSet.CreateSet(5, 6, 8, 9);

      ImapSequenceSet c;

      c = ImapSequenceSet.Combine(set1, set2);

      Assert.AreEqual("1:3,5,6,8,9", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsFalse(c.IsSingle);

      c = set1 + set2;

      Assert.AreEqual("1:3,5,6,8,9", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsFalse(c.IsSingle);

      c = set1.CombineWith(set2);

      try {
        c.ToNumber();
        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }

      Assert.AreEqual("1:3,5,6,8,9", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsFalse(c.IsSingle);

      c = set2 + set1;

      Assert.AreEqual("5,6,8,9,1:3", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsFalse(c.IsSingle);

      try {
        c.ToNumber();
        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }

    [Test]
    public void TestCombineNonEmptyWithEmpty()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(2, 5);
      var set2 = ImapSequenceSet.CreateSet(new long[0]);

      Assert.IsFalse(set1.IsEmpty);
      Assert.IsFalse(set1.IsSingle);
      Assert.IsTrue(set2.IsEmpty);
      Assert.IsFalse(set2.IsSingle);

      ImapSequenceSet c;

      c = set1 + set2;

      Assert.AreEqual("2:5", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsFalse(c.IsSingle);

      try {
        c.ToNumber();
        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }

      c = set2 + set1;

      Assert.AreEqual("2:5", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsFalse(c.IsSingle);

      try {
        c.ToNumber();
        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }

    [Test]
    public void TestCombineSingleWithEmpty()
    {
      var set1 = ImapSequenceSet.CreateSet(3);
      var set2 = ImapSequenceSet.CreateSet(new long[0]);

      Assert.IsFalse(set1.IsEmpty);
      Assert.IsTrue(set1.IsSingle);
      Assert.IsTrue(set2.IsEmpty);
      Assert.IsFalse(set2.IsSingle);

      ImapSequenceSet c;

      c = set1 + set2;

      Assert.AreEqual("3", c.ToString());
      Assert.IsFalse(c.IsEmpty);
      Assert.IsTrue(c.IsSingle);

      Assert.AreEqual(3L, c.ToNumber());
    }

    [Test]
    public void TestCombineWithAll()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(1, 3);
      var set2 = ImapSequenceSet.CreateAllSet();

      var combined = set1 + set2;

      Assert.AreEqual("*", combined.ToString());
      Assert.IsFalse(combined.IsEmpty);
      Assert.IsFalse(combined.IsSingle);

      try {
        combined.ToNumber();
        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }

    [Test]
    public void TestCombineWithSequenceSetAndUidSet()
    {
      foreach (var test in new[] {
        new {Set1 = ImapSequenceSet.CreateAllSet(),         Set2 = ImapSequenceSet.CreateUidAllSet()},
        new {Set1 = ImapSequenceSet.CreateAllSet(),         Set2 = ImapSequenceSet.CreateUidFromSet(2)},
        new {Set1 = ImapSequenceSet.CreateFromSet(3),       Set2 = ImapSequenceSet.CreateUidToSet(5)},
        new {Set1 = ImapSequenceSet.CreateRangeSet(1, 3),   Set2 = ImapSequenceSet.CreateUidRangeSet(5, 7)},
        new {Set1 = ImapSequenceSet.CreateToSet(10),        Set2 = ImapSequenceSet.CreateUidSet(3)},
      }) {
        try {
          ImapSequenceSet.Combine(test.Set1, test.Set2);
          Assert.Fail("ArgumentException not thrown: {0} {1}", test.Set1, test.Set2);
        }
        catch (ArgumentException) {
        }

        try {
          ImapSequenceSet.Combine(test.Set2, test.Set1);
          Assert.Fail("ArgumentException not thrown: {0} {1}", test.Set1, test.Set2);
        }
        catch (ArgumentException) {
        }
      }
    }

    [Test]
    public void TestCombinedToArray()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(1, 3);
      var set2 = ImapSequenceSet.CreateSet(5, 6, 8, 9);

      var combined = set1 + set2;

      var arr = combined.ToArray();

      Assert.AreEqual(7, arr.Length);
      Assert.AreEqual(1, arr[0]);
      Assert.AreEqual(2, arr[1]);
      Assert.AreEqual(3, arr[2]);
      Assert.AreEqual(5, arr[3]);
      Assert.AreEqual(6, arr[4]);
      Assert.AreEqual(8, arr[5]);
      Assert.AreEqual(9, arr[6]);
    }

    [Test]
    public void TestCombinedGetEnumerator()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(1, 3);
      var set2 = ImapSequenceSet.CreateSet(5, 6, 8, 9);

      var combined = set1 + set2;

      var expected = new[] {1L, 2L, 3L, 5L, 6L, 8L, 9L};
      var index = 0;

      foreach (var num in combined) {
        Assert.AreEqual(expected[index++], num);
      }
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestCombinedWithFromSetToArray()
    {
      var set1 = ImapSequenceSet.CreateFromSet(10);
      var set2 = ImapSequenceSet.CreateSet(5, 6, 8, 9);

      (set1 + set2).ToArray();
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestCombinedWithFromSetGetEnumerator()
    {
      var set1 = ImapSequenceSet.CreateFromSet(10);
      var set2 = ImapSequenceSet.CreateSet(5, 6, 8, 9);

      foreach (var num in (set1 + set2)) {
        Assert.Fail("enumerated: {0}", num);
      }
    }

    [Test]
    public void TestIsEmpty()
    {
      Assert.IsTrue(ImapSequenceSet.CreateSet(new long[] {}).IsEmpty);

      Assert.IsFalse(ImapSequenceSet.CreateFromSet(1).IsEmpty);
      Assert.IsFalse(ImapSequenceSet.CreateUidFromSet(1).IsEmpty);

      Assert.IsFalse(ImapSequenceSet.CreateToSet(3).IsEmpty);
      Assert.IsFalse(ImapSequenceSet.CreateUidToSet(3).IsEmpty);

      Assert.IsFalse(ImapSequenceSet.CreateRangeSet(1, 3).IsEmpty);
      Assert.IsFalse(ImapSequenceSet.CreateUidRangeSet(1, 3).IsEmpty);

      Assert.IsFalse(ImapSequenceSet.CreateAllSet().IsEmpty);
      Assert.IsFalse(ImapSequenceSet.CreateUidAllSet().IsEmpty);

      Assert.IsFalse(ImapSequenceSet.Combine(ImapSequenceSet.CreateSet(new long[] {}), ImapSequenceSet.CreateSet(new long[] {1})).IsEmpty);
    }

    [Test]
    public void TestSplitIntoEachSequential1()
    {
      var range = ImapSequenceSet.CreateRangeSet(1L, 345L);
      var expected = new[] {
        "1:100",
        "101:200",
        "201:300",
        "301:345",
      };
      var index = 0;

      foreach (var splitted in range.SplitIntoEach(100)) {
        Assert.AreEqual(expected[index++], splitted.ToString());

        Assert.IsFalse(splitted.IsUidSet);
        Assert.IsFalse(splitted.IsSingle);
        Assert.IsFalse(splitted.IsEmpty);
      }

      Assert.AreEqual(4, index);
    }

    [Test]
    public void TestSplitIntoEachSequential2()
    {
      var range = ImapSequenceSet.CreateRangeSet(3L, 12L);
      var expected = new[] {
        "3:6",
        "7:10",
        "11:12",
      };
      var index = 0;

      foreach (var splitted in range.SplitIntoEach(4)) {
        Assert.AreEqual(expected[index++], splitted.ToString());

        Assert.IsFalse(splitted.IsUidSet);
        Assert.IsFalse(splitted.IsSingle);
        Assert.IsFalse(splitted.IsEmpty);
      }

      Assert.AreEqual(3, index);
    }

    [Test]
    public void TestSplitIntoEachSequential3()
    {
      var set1 = ImapSequenceSet.CreateUidRangeSet(3L, 8L);
      var set2 = ImapSequenceSet.CreateUidRangeSet(9L, 15L);
      var combined = set1 + set2;
      var expected = new[] {
        "3:7",
        "8:12",
        "13:15",
      };
      var index = 0;

      foreach (var splitted in combined.SplitIntoEach(5)) {
        Assert.AreEqual(expected[index++], splitted.ToString());

        Assert.IsTrue(splitted.IsUidSet);
        Assert.IsFalse(splitted.IsSingle);
        Assert.IsFalse(splitted.IsEmpty);
      }

      Assert.AreEqual(3, index);
    }

    [Test]
    public void TestSplitIntoEachNonSequential1()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(7L, 10L);
      var set2 = ImapSequenceSet.CreateSet(1L, 3L, 5L);
      var set3 = ImapSequenceSet.CreateRangeSet(20L, 25L);
      var set4 = ImapSequenceSet.CreateSet(30L);
      var combined = set1 + set2 + set3 + set4;
      var expected = new[] {
        "7:9",
        "10,1,3",
        "5,20,21",
        "22:24",
        "25,30",
      };
      var index = 0;

      foreach (var splitted in combined.SplitIntoEach(3)) {
        Assert.AreEqual(expected[index++], splitted.ToString());

        Assert.IsFalse(splitted.IsUidSet);
        Assert.IsFalse(splitted.IsSingle);
        Assert.IsFalse(splitted.IsEmpty);
      }

      Assert.AreEqual(5, index);
    }

    [Test]
    public void TestSplitIntoEachNonSequential2()
    {
      var set1 = ImapSequenceSet.CreateRangeSet(7L, 10L);
      var set2 = ImapSequenceSet.CreateSet(1L, 3L, 5L);
      var set3 = ImapSequenceSet.CreateRangeSet(20L, 25L);
      var set4 = ImapSequenceSet.CreateSet(30L);
      var combined = set1 + set2 + set3 + set4;
      var expected = new[] {
        "7,8,9,10,1",
        "3,5,20,21,22",
        "23,24,25,30",
      };
      var index = 0;

      foreach (var splitted in combined.SplitIntoEach(5)) {
        Assert.AreEqual(expected[index++], splitted.ToString());

        Assert.IsFalse(splitted.IsUidSet);
        Assert.IsFalse(splitted.IsSingle);
        Assert.IsFalse(splitted.IsEmpty);
      }

      Assert.AreEqual(3, index);
    }

    [Test]
    public void TestFromUri()
    {
      var arr = ImapSequenceSet.FromUri(new Uri("imap://localhost/INBOX/;UID=1")).ToArray();

      Assert.AreEqual(1, arr.Length);
      Assert.AreEqual(1L, arr[0]);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestFromUriInvalidForm()
    {
      ImapSequenceSet.FromUri(new Uri("imap://localhost/INBOX?UID 1"));
    }
  }
}