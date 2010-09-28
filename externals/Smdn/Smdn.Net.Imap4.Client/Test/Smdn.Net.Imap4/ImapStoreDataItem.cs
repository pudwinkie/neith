using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapStoreDataItemTest {
    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestConstructEmpty1()
    {
      ImapStoreDataItem.ReplaceFlags(new ImapMessageFlagList());
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestConstructEmpty2()
    {
      ImapStoreDataItem.ReplaceFlags(new string[] {}, new ImapMessageFlag[] {});
    }

    [Test]
    public void TestReplaceFlags()
    {
      Assert.AreEqual("FLAGS (\\Answered \\Deleted)",
                      ImapStoreDataItem.ReplaceFlags(ImapMessageFlag.Answered, ImapMessageFlag.Deleted).ToString());

      Assert.AreEqual("FLAGS ($label1)",
                      ImapStoreDataItem.ReplaceFlags("$label1").ToString());

      Assert.AreEqual("FLAGS (\\Flagged $label3)",
                      ImapStoreDataItem.ReplaceFlags(new[] {"$label3"}, ImapMessageFlag.Flagged).ToString());

      Assert.AreEqual("FLAGS (\\Seen \\Draft $label1 $label2)",
                      ImapStoreDataItem.ReplaceFlags(new ImapMessageFlagList(new[] {"$label1", "$label2"}, ImapMessageFlag.Seen, ImapMessageFlag.Draft)).ToString());
    }

    [Test]
    public void TestRemoveNonApplicableFlags1()
    {
      Assert.AreEqual("FLAGS ($label2)",
                      ImapStoreDataItem.ReplaceFlags(new[] {"$label2"}, ImapMessageFlag.Recent).ToString());
    }

    [Test]
    public void TestRejectNonApplicableFlags2()
    {
      Assert.AreEqual("FLAGS ($label2)",
                      ImapStoreDataItem.ReplaceFlags(new[] {"$label2"}, ImapMessageFlag.AllowedCreateKeywords).ToString());
    }

    private void TestInvalidKeyword(string keyword)
    {
      try {
        ImapStoreDataItem.ReplaceFlagsSilent(keyword);
        Assert.Fail("no exception with " + keyword);
      }
      catch (ArgumentException ex) {
        Console.WriteLine(ex.Message);
      }
    }

    [Test]
    public void TestInvalidKeyword()
    {
      TestInvalidKeyword("$la bel1");
      TestInvalidKeyword("$label1\r");
      TestInvalidKeyword("\0$label1\r");
      TestInvalidKeyword("[label1]\r");
      TestInvalidKeyword("(label1)\r");
      TestInvalidKeyword("label:)\r");
      TestInvalidKeyword("label%\r");
      TestInvalidKeyword("label%\r");
    }
  }
}