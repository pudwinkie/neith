using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapMessageFlagTests {
    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetValidatedKeywordNull()
    {
      ImapMessageFlag.GetValidatedKeyword(null);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestGetValidatedKeywordEmpty()
    {
      ImapMessageFlag.GetValidatedKeyword(string.Empty);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestGetValidatedKeywordContainsCtrl()
    {
      ImapMessageFlag.GetValidatedKeyword("keyword\n");
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestGetValidatedKeywordSystemFlags()
    {
      ImapMessageFlag.GetValidatedKeyword("\\Deleted");
    }

    [Test]
    public void TestIsValidKeyword()
    {
      Assert.IsTrue(ImapMessageFlag.IsValidKeyword("a"));
      Assert.IsTrue(ImapMessageFlag.IsValidKeyword("$label"));
      Assert.IsTrue(ImapMessageFlag.IsValidKeyword("<label>"));
      Assert.IsFalse(ImapMessageFlag.IsValidKeyword(string.Empty));
      Assert.IsFalse(ImapMessageFlag.IsValidKeyword("keyword\n"));
      Assert.IsFalse(ImapMessageFlag.IsValidKeyword("\\Deleted"));
    }

    [Test]
    public void TestGetValidatedKeywordValid()
    {
      Assert.AreEqual("a", ImapMessageFlag.GetValidatedKeyword("a"));
      Assert.AreEqual("$label", ImapMessageFlag.GetValidatedKeyword("$label"));
      Assert.AreEqual("<label>", ImapMessageFlag.GetValidatedKeyword("<label>"));
    }
  }
}
