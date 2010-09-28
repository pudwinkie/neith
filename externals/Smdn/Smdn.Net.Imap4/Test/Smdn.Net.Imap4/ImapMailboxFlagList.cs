using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapMailboxFlagListTests {
    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestCreateReadOnlyEmpty()
    {
      var list = ImapMailboxFlagList.CreateReadOnlyEmpty();

      Assert.AreEqual(0, list.Count);
      Assert.IsTrue(list.IsReadOnly);

      list.Add(ImapMailboxFlag.Marked);
    }

    [Test]
    public void TestAsReadOnly()
    {
      var list = new ImapMailboxFlagList(new[] {
        ImapMailboxFlag.NoInferiors,
        ImapMailboxFlag.Subscribed,
      });

      var readOnlyList = list.AsReadOnly();

      Assert.AreNotSame(list, readOnlyList);
      Assert.AreEqual(2, readOnlyList.Count);
      Assert.IsTrue(readOnlyList.Has(ImapMailboxFlag.NoInferiors));
      Assert.IsTrue(readOnlyList.Has("\\Noinferiors"));
      Assert.IsTrue(readOnlyList.Has(ImapMailboxFlag.Subscribed));

      try {
        var collection = readOnlyList as System.Collections.Generic.ICollection<ImapMailboxFlag>;

        collection.Add(ImapMailboxFlag.HasChildren);

        Assert.Fail("NotSupportedException not thrown");
      }
      catch (NotSupportedException) {
      }
    }
  }
}
