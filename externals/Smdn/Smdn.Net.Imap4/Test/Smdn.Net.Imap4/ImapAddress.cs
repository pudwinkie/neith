using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapAddressTests {
    [Test]
    public void TestToMailAddressWithDisplayName()
    {
      var address = (new ImapAddress("=?ISO-2022-JP?B?GyRCJE8kRiRKJSIlcyVGJUobKEI=?=", null, "info", "hatena.ne.jp")).ToMailAddress();

      Assert.AreEqual("はてなアンテナ", address.DisplayName);
      Assert.AreEqual("info", address.User);
      Assert.AreEqual("hatena.ne.jp", address.Host);

      address = (new ImapAddress("sender", null, "test", "example.com")).ToMailAddress();

      Assert.AreEqual("sender", address.DisplayName);
      Assert.AreEqual("test", address.User);
      Assert.AreEqual("example.com", address.Host);
    }

    [Test]
    public void TestToMailAddressWithoutDisplayName()
    {
      var address = (new ImapAddress(null, null, "santamarta", "mail.invisiblefulmoon.net")).ToMailAddress();

      Assert.AreEqual("santamarta", address.User);
      Assert.AreEqual("mail.invisiblefulmoon.net", address.Host);
    }

    [Test]
    public void TestToMailAddressInvalidMimeEncoding()
    {
      var address = (new ImapAddress("=?ISO-2022-JP?X?xxxxxx?=", null, "test", "example.com")).ToMailAddress();

      Assert.AreEqual("=?ISO-2022-JP?X?xxxxxx?=", address.DisplayName);
      Assert.AreEqual("test", address.User);
      Assert.AreEqual("example.com", address.Host);
    }

    [Test]
    public void TestToMailAddressInvalidCharset()
    {
      var address = (new ImapAddress("=?XXXXX?q?test?=", null, "test", "example.com")).ToMailAddress();

      Assert.AreEqual("=?XXXXX?q?test?=", address.DisplayName);
      Assert.AreEqual("test", address.User);
      Assert.AreEqual("example.com", address.Host);
    }

    [Test]
    public void TestToMailAddressCollection()
    {
      var col = ImapAddress.ToMailAddressCollection(null);

      Assert.IsNull(col);

      col = ImapAddress.ToMailAddressCollection(new[] {
        new ImapAddress("=?ISO-2022-JP?B?GyRCJE8kRiRKJSIlcyVGJUobKEI=?=", null, "info", "hatena.ne.jp"),
        new ImapAddress("=?ISO-2022-JP?X?xxxxxx?=", null, "test", "example.com"),
        new ImapAddress(null, null, "test", "example.com"),
      });

      Assert.IsNotNull(col);
      Assert.AreEqual(3, col.Count);

      Assert.AreEqual("はてなアンテナ", col[0].DisplayName);
      Assert.AreEqual("info@hatena.ne.jp", col[0].Address);

      Assert.AreEqual("=?ISO-2022-JP?X?xxxxxx?=", col[1].DisplayName);
      Assert.AreEqual("test@example.com", col[1].Address);

      Assert.IsEmpty(col[2].DisplayName);
      Assert.AreEqual("test@example.com", col[2].Address);
    }
  }
}
