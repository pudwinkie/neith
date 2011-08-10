using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapNamespaceTests {
    [Test]
    public void TestClone()
    {
      //(("" "/")) (("Other Users/" "/")) NIL
      var ns = new ImapNamespace(new[] {new ImapNamespaceDesc("", "/")},
                                 new[] {new ImapNamespaceDesc("Other Users/", "/")},
                                 new ImapNamespaceDesc[0]);
      var cloned = ns.Clone();

      Assert.AreNotSame(ns, cloned);

      Assert.AreNotSame(ns.PersonalNamespaces, cloned.PersonalNamespaces);
      Assert.AreEqual(ns.PersonalNamespaces.Length, cloned.PersonalNamespaces.Length);
      Assert.AreNotSame(ns.PersonalNamespaces[0], cloned.PersonalNamespaces[0]);
      Assert.AreEqual(ns.PersonalNamespaces[0].Prefix, cloned.PersonalNamespaces[0].Prefix);
      Assert.AreEqual(ns.PersonalNamespaces[0].HierarchyDelimiter, cloned.PersonalNamespaces[0].HierarchyDelimiter);

      Assert.AreNotSame(ns.OtherUsersNamespaces, cloned.OtherUsersNamespaces);
      Assert.AreEqual(ns.OtherUsersNamespaces.Length, cloned.OtherUsersNamespaces.Length);
      Assert.AreNotSame(ns.OtherUsersNamespaces[0], cloned.OtherUsersNamespaces[0]);

      Assert.AreNotSame(ns.SharedNamespaces, cloned.SharedNamespaces);
      Assert.AreEqual(ns.SharedNamespaces.Length, cloned.SharedNamespaces.Length);
    }
  }
}
