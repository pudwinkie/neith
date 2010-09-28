using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapNamespaceDescTests {
    [Test]
    public void TestClone()
    {
      var desc = new ImapNamespaceDesc("#mh/", "/", new Dictionary<string, string[]>() {
        {"X-PARAM", new[] {"FLAG1", "FLAG2"}},
      });
      var cloned = desc.Clone();

      Assert.AreNotSame(desc, cloned);
      Assert.AreEqual(desc.Prefix, cloned.Prefix);
      Assert.AreEqual(desc.HierarchyDelimiter, cloned.HierarchyDelimiter);
      Assert.AreNotSame(desc.Extensions, cloned.Extensions);
      Assert.AreEqual(desc.Extensions.Count, cloned.Extensions.Count);
      Assert.AreNotSame(desc.Extensions["X-PARAM"], cloned.Extensions["X-PARAM"]);
      Assert.AreEqual(desc.Extensions["X-PARAM"].Length, cloned.Extensions["X-PARAM"].Length);
      Assert.AreEqual(desc.Extensions["X-PARAM"][0], cloned.Extensions["X-PARAM"][0]);
      Assert.AreEqual(desc.Extensions["X-PARAM"][1], cloned.Extensions["X-PARAM"][1]);
    }

    [Test]
    public void TestTranslatedPrefix()
    {
      var desc = new ImapNamespaceDesc("Other Users/", "/", new Dictionary<string, string[]>() {
        {"TRANSLATION", new[] {"Andere Ben&APw-tzer/"}},
      });

      Assert.AreEqual("Other Users/", desc.Prefix);
      Assert.AreEqual("Andere Benützer/", desc.TranslatedPrefix);

      desc = new ImapNamespaceDesc("Other Users/", "/");

      Assert.AreEqual("Other Users/", desc.Prefix);
      Assert.AreEqual("Other Users/", desc.TranslatedPrefix);
    }
  }
}
