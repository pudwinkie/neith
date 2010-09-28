using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapUriTests {
    [Test]
    public void TestRegisterParser()
    {
      Assert.IsFalse(UriParser.IsKnownScheme("imap"));
      Assert.IsFalse(UriParser.IsKnownScheme("imaps"));

      Assert.AreEqual(-1, (new Uri("imap://localhost")).Port);
      Assert.AreEqual(-1, (new Uri("imaps://localhost")).Port);

      ImapUri.RegisterParser();

      Assert.IsTrue(UriParser.IsKnownScheme("imap"));
      Assert.IsTrue(UriParser.IsKnownScheme("imaps"));

      Assert.AreEqual(143, (new Uri("imap://localhost")).Port);
      Assert.AreEqual(993, (new Uri("imaps://localhost")).Port);
    }

    [Test]
    public void TestIsImap()
    {
      Assert.IsTrue(ImapUri.IsImap(new Uri("imap://localhost/")));
      Assert.IsTrue(ImapUri.IsImap(new Uri("imaps://localhost/")));
      Assert.IsTrue(ImapUri.IsImap(new Uri("IMAP://localhost/")));
      Assert.IsTrue(ImapUri.IsImap(new Uri("IMAPS://localhost/")));
      Assert.IsFalse(ImapUri.IsImap(new Uri("http://localhost")));
    }

    [Test]
    public void TestGetDefaultPortFromScheme()
    {
      Assert.AreEqual(143, ImapUri.GetDefaultPortFromScheme("imap"));
      Assert.AreEqual(143, ImapUri.GetDefaultPortFromScheme("IMAP"));
      Assert.AreEqual(993, ImapUri.GetDefaultPortFromScheme("imaps"));
      Assert.AreEqual(993, ImapUri.GetDefaultPortFromScheme("IMAPS"));
      Assert.AreEqual(143, ImapUri.GetDefaultPortFromScheme(new Uri("imap://localhost/")));
      Assert.AreEqual(993, ImapUri.GetDefaultPortFromScheme(new Uri("imaps://localhost/")));
      Assert.AreEqual(143, ImapUri.GetDefaultPortFromScheme(new Uri("imap://localhost:10143/")));
      Assert.AreEqual(993, ImapUri.GetDefaultPortFromScheme(new Uri("imaps://localhost:10993/")));

      try {
        ImapUri.GetDefaultPortFromScheme("http");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        ImapUri.GetDefaultPortFromScheme(new Uri("http://localhost/"));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }
  }
}
