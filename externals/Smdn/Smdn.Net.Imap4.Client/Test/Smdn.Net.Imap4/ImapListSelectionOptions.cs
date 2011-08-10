using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapListSelectionOptionsTest {
    [Test]
    public void TestOperatorAdd1()
    {
      var options = ImapListSelectionOptions.RecursiveMatch + ImapListSelectionOptions.Subscribed;

      Assert.AreEqual("(RECURSIVEMATCH SUBSCRIBED)", options.ToString());
    }

    [Test]
    public void TestOperatorAdd2()
    {
      var options = ImapListSelectionOptions.Subscribed;

      CollectionAssert.AreEquivalent((options as IImapExtension).RequiredCapabilities,
                                     new[] {ImapCapability.ListExtended});

      Assert.AreEqual("(SUBSCRIBED)", options.ToString());

      options += ImapListSelectionOptions.SpecialUse;

      CollectionAssert.AreEquivalent((options as IImapExtension).RequiredCapabilities,
                                     new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse});

      Assert.AreEqual("(SUBSCRIBED SPECIAL-USE)", options.ToString());
    }
  }
}