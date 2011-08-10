using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapListReturnOptionsTest {
    [Test]
    public void TestOperatorAdd1()
    {
      var options = ImapListReturnOptions.Children + ImapListReturnOptions.Subscribed;

      Assert.AreEqual("(CHILDREN SUBSCRIBED)", options.ToString());
    }

    [Test]
    public void TestOperatorAdd2()
    {
      var options = ImapListReturnOptions.Subscribed;

      CollectionAssert.AreEquivalent((options as IImapExtension).RequiredCapabilities,
                                     new[] {ImapCapability.ListExtended});

      Assert.AreEqual("(SUBSCRIBED)", options.ToString());

      options += ImapListReturnOptions.SpecialUse;

      CollectionAssert.AreEquivalent((options as IImapExtension).RequiredCapabilities,
                                     new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse});

      Assert.AreEqual("(SUBSCRIBED SPECIAL-USE)", options.ToString());
    }
  }
}