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

      var requiredCapabilities = (criteria as IImapMultipleExtension).RequiredCapabilities;

      Assert.AreEqual(2, requiredCapabilities.Length);
      CollectionAssert.Contains(requiredCapabilities, ImapCapability.Sort);
      CollectionAssert.Contains(requiredCapabilities, ImapCapability.SortDisplay);
    }
  }
}
