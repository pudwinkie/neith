using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapSortCriteriaTests {
    [Test]
    public void TestOperatorAdd()
    {
      var criteria = ImapSortCriteria.Date + ImapSortCriteria.DisplayFrom;

      Assert.AreEqual("(DATE DISPLAYFROM)", criteria.ToString());

      var requiredCapabilities = (criteria as IImapExtension).RequiredCapabilities;

      CollectionAssert.AreEquivalent(new[] {ImapCapability.Sort, ImapCapability.SortDisplay},
                                     requiredCapabilities);
    }
  }
}
