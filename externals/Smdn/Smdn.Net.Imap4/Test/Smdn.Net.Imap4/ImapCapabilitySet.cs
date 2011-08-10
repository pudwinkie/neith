using System;
using NUnit.Framework;

using System.Collections.Generic;

#if !NET_3_5
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapCapabilitySetTests {
    [Test]
    public void TestFind()
    {
      var list = new ImapCapabilitySet(new[] {
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
    public void TestContains()
    {
      var list = new ImapCapabilitySet(new[] {
        ImapCapability.LiteralNonSync,
        ImapCapability.Binary,
        new ImapCapability("AUTH=CRAM-MD5"),
        new ImapCapability("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.Contains(ImapCapability.LiteralNonSync));
      Assert.IsTrue(list.Contains("LITERAL+"));
      Assert.IsTrue(list.Contains("literal+"));

      Assert.IsTrue(list.Contains("AUTH=CRAM-MD5"));
      Assert.IsTrue(list.Contains(new ImapCapability("auth=cram-md5")));

      Assert.IsFalse(list.Contains(ImapCapability.Imap4Rev1));
      Assert.IsFalse(list.Contains("imap4rev1"));
      Assert.IsFalse(list.Contains(new ImapCapability("auth=login")));
      Assert.IsFalse(list.Contains("auth=login"));
    }

    private class Extension : IImapExtension {
      public IEnumerable<ImapCapability> RequiredCapabilities {
        get { return requiredCapabilities; }
      }

      public Extension(ImapCapability capa)
        : this(new[] {capa})
      {
      }

      public Extension(IEnumerable<ImapCapability> capas)
      {
        this.requiredCapabilities = new ImapCapabilitySet(true, capas);
      }

      private ImapCapabilitySet requiredCapabilities;
    }

    [Test]
    public void TestIsCapable()
    {
      var list = new ImapCapabilitySet(new[] {
        ImapCapability.LiteralNonSync,
        ImapCapability.Binary,
        new ImapCapability("AUTH=CRAM-MD5"),
        new ImapCapability("AUTH=PLAIN"),
      });

      Assert.IsTrue(list.IsCapable(new Extension(ImapCapability.LiteralNonSync)));
      Assert.IsTrue(list.IsCapable(new Extension(ImapCapability.Binary)));
      Assert.IsTrue(list.IsCapable(new Extension(new[] {ImapCapability.Binary, ImapCapability.LiteralNonSync})));
      Assert.IsTrue(list.IsCapable(new Extension(new[] {new ImapCapability("binary"), new ImapCapability("auth=plain")})));
      Assert.IsFalse(list.IsCapable(new Extension(ImapCapability.Imap4Rev1)));
      Assert.IsFalse(list.IsCapable(new Extension(ImapCapability.Notify)));
      Assert.IsFalse(list.IsCapable(new Extension(new[] {ImapCapability.Imap4Rev1, ImapCapability.LiteralNonSync})));
    }
  }
}
