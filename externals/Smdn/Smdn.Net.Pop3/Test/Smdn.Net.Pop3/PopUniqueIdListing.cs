using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopUniqueIdListingTests {
    [Test]
    public void TestEquatableLong()
    {
      var uidl = new PopUniqueIdListing(1L, "xxx");

      Assert.IsTrue (uidl.Equals(1L));
      Assert.IsFalse(uidl.Equals(0L));
    }

    [Test]
    public void TestEquatableString()
    {
      var uidl = new PopUniqueIdListing(1L, "xxx");

      Assert.IsTrue (uidl.Equals("xxx"));
      Assert.IsFalse(uidl.Equals("xxX"));
      Assert.IsFalse(uidl.Equals("XXX"));
      Assert.IsFalse(uidl.Equals("unique-id"));
    }
  }
}
