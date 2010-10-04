using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapListReturnOptionsTest {
    [Test]
    public void TestOperatorAdd()
    {
      var options = ImapListReturnOptions.Children + ImapListReturnOptions.Subscribed;

      Assert.AreEqual("(CHILDREN SUBSCRIBED)", options.ToString());
    }
  }
}