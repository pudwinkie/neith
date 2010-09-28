using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopScanListingTests {
    [Test]
    public void TestEquatableLong()
    {
      var scan = new PopScanListing(1L, 120L);

      Assert.IsTrue (scan.Equals(1L));
      Assert.IsFalse(scan.Equals(0L));
    }
  }
}
