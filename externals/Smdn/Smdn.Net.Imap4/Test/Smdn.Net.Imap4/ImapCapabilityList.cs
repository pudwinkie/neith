using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapCapabilityListTests {
    [Test]
    public void TestFind()
    {
      var list = new ImapCapabilityList(new[] {
        ImapCapability.LiteralNonSync,
        ImapCapability.Binary,
        new ImapCapability("AUTH=CRAM-MD5"),
        new ImapCapability("AUTH=PLAIN"),
      });

      Assert.AreSame(ImapCapability.LiteralNonSync, list.Find("LITERAL+"));
      Assert.AreSame(ImapCapability.LiteralNonSync, list.Find("literal+"));

      var authPlain = list.Find("AUTH=PLAIN");

      Assert.IsNotNull(authPlain);
      Assert.AreEqual(authPlain, list.Find("auth=plain"));
    }

    [Test]
    public void TestHas()
    {
      var list = new ImapCapabilityList(new[] {
        ImapCapability.LiteralNonSync,
        ImapCapability.Binary,
        new ImapCapability("AUTH=CRAM-MD5"),
        new ImapCapability("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.Has(ImapCapability.LiteralNonSync));
      Assert.IsTrue(list.Has("LITERAL+"));
      Assert.IsTrue(list.Has("literal+"));

      Assert.IsTrue(list.Has("AUTH=CRAM-MD5"));
      Assert.IsTrue(list.Has(new ImapCapability("auth=cram-md5")));

      Assert.IsFalse(list.Has(ImapCapability.Imap4Rev1));
      Assert.IsFalse(list.Has("imap4rev1"));
      Assert.IsFalse(list.Has(new ImapCapability("auth=login")));
      Assert.IsFalse(list.Has("auth=login"));
    }

    private class Extension : IImapExtension {
      public ImapCapability RequiredCapability {
        get; private set;
      }

      public Extension(ImapCapability capa)
      {
        this.RequiredCapability = capa;
      }
    }

    [Test]
    public void TestIsCapable()
    {
      var list = new ImapCapabilityList(new[] {
        ImapCapability.LiteralNonSync,
        ImapCapability.Binary,
        new ImapCapability("AUTH=CRAM-MD5"),
        new ImapCapability("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.IsCapable(new Extension(ImapCapability.LiteralNonSync)));
      Assert.IsTrue(list.IsCapable(new Extension(ImapCapability.Binary)));
      Assert.IsFalse(list.IsCapable(new Extension(ImapCapability.Imap4Rev1)));
      Assert.IsFalse(list.IsCapable(new Extension(ImapCapability.Notify)));
    }
  }
}
