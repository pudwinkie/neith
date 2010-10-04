using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapMessageFlagListTests {
    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestCreateReadOnlyEmpty()
    {
      var list = ImapMessageFlagList.CreateReadOnlyEmpty();

      Assert.AreEqual(0, list.Count);
      Assert.IsTrue(list.IsReadOnly);

      list.Add(ImapMessageFlag.Answered);
    }

    [Test]
    public void TestContainNonApplicableFlags()
    {
      Assert.IsTrue((new ImapMessageFlagList(new[] {
        ImapMessageFlag.Recent,
      })).ContainNonApplicableFlags);

      Assert.IsTrue((new ImapMessageFlagList(new[] {
        ImapMessageFlag.AllowedCreateKeywords,
      })).ContainNonApplicableFlags);

      Assert.IsFalse((new ImapMessageFlagList(new[] {
        new ImapMessageFlag("$label1"),
      })).ContainNonApplicableFlags);
    }

    [Test]
    public void TestGetNonApplicableFlagsRemoved()
    {
      var removed = (new ImapMessageFlagList(new[] {
        ImapMessageFlag.Recent,
        ImapMessageFlag.AllowedCreateKeywords,
        ImapMessageFlag.Seen,
      })).GetNonApplicableFlagsRemoved();

      Assert.IsFalse(removed.ContainNonApplicableFlags);
      Assert.AreEqual(1, removed.Count);
      Assert.IsTrue(removed.Has(ImapMessageFlag.Seen));
    }

    [Test]
    public void TestAsReadOnly()
    {
      var list = new ImapMessageFlagList(new[] {
        ImapMessageFlag.Recent,
        ImapMessageFlag.Answered,
        new ImapMessageFlag("$label1"),
      });

      var readOnlyList = list.AsReadOnly();

      Assert.AreNotSame(list, readOnlyList);
      Assert.AreEqual(3, readOnlyList.Count);
      Assert.IsTrue(readOnlyList.Has(ImapMessageFlag.Recent));
      Assert.IsTrue(readOnlyList.Has("\\Answered"));
      Assert.IsTrue(readOnlyList.Has("$label1"));

      try {
        var collection = readOnlyList as System.Collections.Generic.ICollection<ImapMessageFlag>;

        collection.Add(ImapMessageFlag.Seen);

        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }
  }
}
