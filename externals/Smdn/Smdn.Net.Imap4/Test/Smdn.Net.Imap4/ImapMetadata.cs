using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapMetadataTests {
    [Test]
    public void TestCreateNil()
    {
      ImapMetadata nil;

      nil = ImapMetadata.CreateNil("/private/filters/values/test");

      Assert.IsNull(nil.Value);
      Assert.AreEqual("/private/filters/values/test", nil.EntryName);

      nil = ImapMetadata.CreatePrivateVendorMetadata(null, "foo", "bar");

      Assert.IsNull(nil.Value);
      Assert.AreEqual("/private/vendor/foo/bar", nil.EntryName);
    }

    [Test]
    public void TestJoinEntryName()
    {
      Assert.AreEqual("/shared/comment", ImapMetadata.JoinEntryName("shared", "comment"));
      Assert.AreEqual("/shared/vendor/foo/bar", ImapMetadata.JoinEntryName("shared", "vendor", "foo", "bar"));
    }

    [Test]
    public void TestSplitEntryName()
    {
      Assert.AreEqual(new[] {"shared", "comment"},
                      ImapMetadata.SplitEntryName("/shared/comment"));
      Assert.AreEqual(new[] {"shared", "vendor", "foo", "bar"},
                      ImapMetadata.SplitEntryName("/shared/vendor/foo/bar"));

      try {
        ImapMetadata.SplitEntryName("shared/vendor");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestIsShared()
    {
      Assert.IsTrue((new ImapMetadata("/shared/admin", "admin@example.net")).IsShared);
      Assert.IsTrue((new ImapMetadata("/SHARED/ADMIN", "admin@example.net")).IsShared);
      Assert.IsFalse((new ImapMetadata("/private/admin", "admin@example.net")).IsShared);
    }

    [Test]
    public void TestIsPrivate()
    {
      Assert.IsTrue((new ImapMetadata("/private/comment", "Really useful mailbox")).IsPrivate);
      Assert.IsTrue((new ImapMetadata("/PRIVATE/COMMENT", "Really useful mailbox")).IsPrivate);
      Assert.IsFalse((new ImapMetadata("/shared/comment", "Really useful mailbox")).IsPrivate);
    }
  }
}
