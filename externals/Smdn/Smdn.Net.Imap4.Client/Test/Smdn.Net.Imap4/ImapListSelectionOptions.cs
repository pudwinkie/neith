using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapListSelectionOptionsTest {
    [Test]
    public void TestOperatorAdd()
    {
      var options = ImapListSelectionOptions.RecursiveMatch + ImapListSelectionOptions.Subscribed;

      Assert.AreEqual("(RECURSIVEMATCH SUBSCRIBED)", options.ToString());
    }
  }
}