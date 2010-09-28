using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapStatusDataItemTest {
    [Test]
    public void TestCombine()
    {
      var status1 = ImapStatusDataItem.Recent;
      var status2 = ImapStatusDataItem.UidNext;

      Assert.AreEqual("(RECENT UIDNEXT)", ImapStatusDataItem.Combine(status1, status2).ToString());
      Assert.AreEqual("(RECENT UIDNEXT)", (status1 + status2).ToString());
      Assert.AreEqual("(RECENT UIDNEXT)", status1.CombineWith(status2).ToString());

      Assert.AreEqual("(UIDNEXT RECENT)", (status2 + status1).ToString());
    }

    [Test]
    public void TestCombineWithContainsRequiredCapability()
    {
      var status1 = ImapStatusDataItem.HighestModSeq;
      var status2 = ImapStatusDataItem.UidValidity;

      var combined = status1 + status2;

      Assert.AreEqual("(HIGHESTMODSEQ UIDVALIDITY)", combined.ToString());
      Assert.AreEqual(ImapCapability.CondStore, (combined as IImapExtension).RequiredCapability);
    }
  }
}