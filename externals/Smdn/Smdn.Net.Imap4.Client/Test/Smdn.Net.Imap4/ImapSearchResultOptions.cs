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

      var caps = (options as IImapExtension).RequiredCapabilities;

      CollectionAssert.AreEquivalent(new[] {
                                      ImapCapability.ESearch,
                                      ImapCapability.ESort,
                                      ImapCapability.SearchInThread,
                                      ImapCapability.ThreadRefs,
                                     },
                                     caps);
    }
  }
}