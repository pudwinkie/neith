using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapSearchResultOptionsTest {
    [Test]
    public void TestThreadingAlgorithm()
    {
      var options = ImapSearchResultOptions.ThreadingAlgorithm(ImapThreadingAlgorithm.Refs);

      Assert.AreEqual("(THREAD=REFS)", options.ToString());

      var caps = (options as IImapMultipleExtension).RequiredCapabilities;

      Assert.AreEqual(4, caps.Length);

      Assert.Contains(ImapCapability.ESearch, caps);
      Assert.Contains(ImapCapability.ESort, caps);
      Assert.Contains(ImapCapability.SearchInThread, caps);
      Assert.Contains(ImapCapability.ThreadRefs, caps);
    }
  }
}