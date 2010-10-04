using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapMatchedSequenceSetTests {
    [Test]
    public void TestIsSavedResult()
    {
      Assert.IsFalse((new ImapMatchedSequenceSet(ImapSequenceSet.CreateAllSet())).IsSavedResult);
      Assert.IsTrue(ImapMatchedSequenceSet.CreateSavedResult(new ImapMatchedSequenceSet(ImapSequenceSet.CreateAllSet())).IsSavedResult);
    }

    [Test]
    public void TestIsEmpty()
    {
      Assert.IsTrue(ImapMatchedSequenceSet.CreateEmpty(true).IsEmpty);
      Assert.IsFalse(ImapMatchedSequenceSet.CreateSavedResult(new ImapMatchedSequenceSet(ImapSequenceSet.CreateAllSet())).IsEmpty);
    }

    [Test]
    public void TestIsSingle()
    {
      var singleSet = ImapSequenceSet.CreateSet(1L);

      Assert.IsTrue(singleSet.IsSingle);
      Assert.IsFalse(ImapMatchedSequenceSet.CreateSavedResult(new ImapMatchedSequenceSet(singleSet)).IsSingle);
      Assert.IsTrue((new ImapMatchedSequenceSet(singleSet)).IsSingle);
    }

    [Test]
    public void TestToArray()
    {
      var originalSet = ImapSequenceSet.CreateRangeSet(1, 3);

      Assert.AreEqual(originalSet.ToArray(), (new ImapMatchedSequenceSet(originalSet)).ToArray());
    }

    [Test]
    public void TestGetEnumerator()
    {
      var originalSet = ImapSequenceSet.CreateRangeSet(1, 3);

      var expected = new[] {1L, 2L, 3L};
      var index = 0;

      foreach (var num in (new ImapMatchedSequenceSet(originalSet))) {
        Assert.AreEqual(expected[index++], num);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestToArraySavedResult()
    {
      var matchedSet = new ImapMatchedSequenceSet(ImapSequenceSet.CreateRangeSet(1L, 3L));
      var savedResult = ImapMatchedSequenceSet.CreateSavedResult(matchedSet);

      savedResult.ToArray();
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestGetEnumeratorSavedResult()
    {
      var matchedSet = new ImapMatchedSequenceSet(ImapSequenceSet.CreateRangeSet(1L, 3L));
      var savedResult = ImapMatchedSequenceSet.CreateSavedResult(matchedSet);

      foreach (var num in savedResult) {
        Assert.Fail("enumerated: {0}", num);
      }
    }

    [Test]
    public void TestToNumber()
    {
      Assert.AreEqual(1L, (new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(1L))).ToNumber());
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestToNumberSavedResult()
    {
      ImapMatchedSequenceSet.CreateSavedResult(new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(1L))).ToNumber();
    }

    [Test]
    public void TestSplitIntoEach()
    {
      var matched = new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(1L, 3L, 5L, 7L, 9L));
      var expected = new[] {
        "1,3,5",
        "7,9",
      };
      var index = 0;

      foreach (var splitted in matched.SplitIntoEach(3)) {
        Assert.AreEqual(expected[index++], splitted.ToString());
      }

      Assert.AreEqual(expected.Length, index);
    }

    [Test]
    public void TestSplitIntoEachSavedResult()
    {
      var matched = ImapMatchedSequenceSet.CreateSavedResult(new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(1L)));
      var expected = new[] {
        "$",
      };
      var index = 0;

      foreach (var splitted in matched.SplitIntoEach(3)) {
        Assert.AreEqual(expected[index++], splitted.ToString());
      }

      Assert.AreEqual(expected.Length, index);

      index = 0;

      foreach (var splitted in matched.SplitIntoEach(10)) {
        Assert.AreEqual(expected[index++], splitted.ToString());
      }

      Assert.AreEqual(expected.Length, index);
    }

    [Test]
    public void TestToString()
    {
      Assert.AreEqual("*", (new ImapMatchedSequenceSet(ImapSequenceSet.CreateAllSet())).ToString());
    }

    [Test]
    public void TestToStringSavedResult()
    {
      Assert.AreEqual("$", ImapMatchedSequenceSet.CreateSavedResult(new ImapMatchedSequenceSet(ImapSequenceSet.CreateAllSet())).ToString());
    }
  }
}
