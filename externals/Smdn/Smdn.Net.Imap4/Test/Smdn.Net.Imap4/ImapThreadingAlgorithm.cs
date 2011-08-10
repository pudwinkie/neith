using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapThreadingAlgorithmTest {
    [Test]
    public void TestEquatable()
    {
      Assert.IsTrue(ImapThreadingAlgorithm.OrderedSubject.Equals(ImapThreadingAlgorithm.OrderedSubject));
      Assert.IsTrue(ImapThreadingAlgorithm.OrderedSubject.Equals(new ImapString(ImapThreadingAlgorithm.OrderedSubject.ToString())));
      Assert.IsTrue(ImapThreadingAlgorithm.OrderedSubject.Equals(ImapThreadingAlgorithm.OrderedSubject.ToString()));
    }
  }
}