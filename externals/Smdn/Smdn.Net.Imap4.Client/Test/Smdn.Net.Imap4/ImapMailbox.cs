using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapMailboxTests {
    [Test]
    public void TestNameEquals()
    {
      Assert.IsTrue (ImapMailbox.NameEquals("INBOX", "INBOX"));
      Assert.IsTrue (ImapMailbox.NameEquals("INBOX", "inbox"));
      Assert.IsTrue (ImapMailbox.NameEquals("inbox", "INBOX"));

      Assert.IsFalse(ImapMailbox.NameEquals("INBOX", "hoge"));
      Assert.IsFalse(ImapMailbox.NameEquals("hoge", "INBOX"));

      Assert.IsTrue (ImapMailbox.NameEquals("hoge", "hoge"));
      Assert.IsFalse(ImapMailbox.NameEquals("Hoge", "hoge"));

      Assert.IsTrue (ImapMailbox.NameEquals("INBOX\u3007", "INBOX\u3007"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("INBOX\u3007", "INBOX"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("INBOX", "INBOX\u3007"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("INBOX\u3007", "\u3007INBOX"), "StringComparison");

      Assert.IsTrue (ImapMailbox.NameEquals("hoge\u3007", "hoge\u3007"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("HOGE\u3007", "hoge\u3007"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("hoge\u3007", "hoge"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("hoge", "hoge\u3007"), "StringComparison");
      Assert.IsFalse(ImapMailbox.NameEquals("hoge\u3007", "\u3007hoge"), "StringComparison");
    }
  }
}
