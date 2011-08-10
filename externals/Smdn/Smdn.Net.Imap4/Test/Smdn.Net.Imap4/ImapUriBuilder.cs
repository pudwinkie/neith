using System;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapUriBuilderTests {
    private class SearchQuery : IImapUrlSearchQuery {
      public static SearchQuery FromUri(Uri uri)
      {
        var q = uri.Query;

        if (q == string.Empty || q == "?")
          return null;
        else
          return new SearchQuery(Uri.UnescapeDataString(q.Substring(1)));
      }

      public SearchQuery(string query)
      {
        this.query = query;
      }

      public byte[] GetEncodedQuery(Encoding charset, out bool charsetSpecified)
      {
        charsetSpecified = false;

        return (charset ?? Encoding.ASCII).GetBytes(query);
      }

      private string query;
    }

    [Test]
    public void TestConstructDefault()
    {
      var b = new ImapUriBuilder();

      Assert.AreEqual(new Uri("imap://localhost/"), b.Uri);
    }

    [Test]
    public void TestConstructFromUri()
    {
      var uri = "IMAPS://user;auth=cram-md5@localhost:10143/INBOX;uidvalidity=12345/;uid=1/;section=1.2/;partial=0.1024/";
      var b = new ImapUriBuilder(new Uri(uri));

      Assert.AreEqual(new Uri("imaps://user;AUTH=CRAM-MD5@localhost:10143/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=1.2/;PARTIAL=0.1024"), b.Uri);
      Assert.AreEqual("imaps", b.Scheme);
      Assert.AreEqual("user", b.UserName);
      Assert.AreEqual(ImapAuthenticationMechanism.CRAMMD5, b.AuthType);
      Assert.AreEqual("localhost", b.Host);
      Assert.AreEqual(10143, b.Port);
      Assert.AreEqual("INBOX", b.Mailbox);
      Assert.AreEqual(12345L, b.UidValidity);
      Assert.AreEqual(1L, b.Uid);
      Assert.AreEqual("1.2", b.Section);
      Assert.AreEqual(new ImapPartialRange(0L, 1024L), b.Partial);
      Assert.IsNull(b.SearchCriteria);
    }

    [Test]
    public void TestConstructFromUriWithoutQueryParser()
    {
      var uri = "imap://localhost/INBOX?FROM {12+}\r\nfrom address";
      var b = new ImapUriBuilder(uri);

      Assert.IsNull(b.SearchCriteria);
      Assert.AreEqual(new Uri("imap://localhost/INBOX"), b.Uri);
    }

    [Test]
    public void TestConstructFromUriWithQueryParser()
    {
      var uri = "imap://localhost/INBOX?FROM {12+}\r\nfrom address";
      var b = new ImapUriBuilder(uri, SearchQuery.FromUri);

      Assert.IsNotNull(b.SearchCriteria);
      Assert.IsInstanceOfType(typeof(SearchQuery), b.SearchCriteria);
      Assert.AreEqual(new Uri("imap://localhost/INBOX?FROM%20%7B12+%7D%0D%0Afrom%20address"), b.Uri);
    }

    [Test]
    public void TestGetUriThrowsUriFormatExceptionIfMailboxNotSpecified()
    {
      foreach (var b in new[] {
        new ImapUriBuilder() {Host = "localhost", Uid = 1L},
        new ImapUriBuilder() {Host = "localhost", SearchCriteria = new SearchQuery("ALL")},
      }) {
        try {
          Assert.IsNotNull(b.Uri);
          Assert.Fail("UriFormatException not thrown; '{0}'", b.ToString());
        }
        catch (UriFormatException) {
        }
      }
    }

    [Test]
    public void TestGetUriThrowsUriFormatExceptionIfUidNotSpecified()
    {
      foreach (var b in new[] {
        new ImapUriBuilder() {Host = "localhost", Mailbox = "INBOX", Section = "1.2"},
        new ImapUriBuilder() {Host = "localhost", Mailbox = "INBOX", Partial = new ImapPartialRange(0L, 1024L)},
        new ImapUriBuilder() {Host = "localhost", Mailbox = "INBOX", Section = "1.2", Partial = new ImapPartialRange(0L, 1024L)},
      }) {
        try {
          Assert.IsNotNull(b.Uri);
          Assert.Fail("UriFormatException not thrown; '{0}'", b.ToString());
        }
        catch (UriFormatException) {
        }
      }
    }

    [Test]
    public void TestClone()
    {
      var origin = new ImapUriBuilder("imaps://user;auth=cram-md5@localhost:10143/INBOX;uidvalidity=12345/;uid=1/;section=1.2/;partial=0.1024/");
      var cloned = origin.Clone();

      Assert.AreEqual(cloned, origin);
      Assert.AreEqual("imaps", cloned.Scheme);
      Assert.AreEqual("user", cloned.UserName);
      Assert.AreEqual(ImapAuthenticationMechanism.CRAMMD5, cloned.AuthType);
      Assert.AreEqual("localhost", cloned.Host);
      Assert.AreEqual(10143, cloned.Port);
      Assert.AreEqual("INBOX", cloned.Mailbox);
      Assert.AreEqual(12345L, cloned.UidValidity);
      Assert.AreEqual(1L, cloned.Uid);
      Assert.AreEqual("1.2", cloned.Section);
      Assert.AreEqual(new ImapPartialRange(0L, 1024L), cloned.Partial);
      Assert.IsNull(cloned.SearchCriteria);
    }

    [Test]
    public void TestCloneShallowCopy()
    {
      var origin = new ImapUriBuilder("imap://localhost/", "INBOX", 12345L, new SearchQuery("ALL"));

      origin.Charset = Encoding.UTF8;

      var cloned = origin.Clone();

      Assert.AreSame(cloned.SearchCriteria, origin.SearchCriteria);
      Assert.AreSame(cloned.Charset, origin.Charset);
    }

    [Test]
    public void TestBuildServerFormUri()
    {
      var b = new ImapUriBuilder();

      b.Host = "imap.example.net";

      Assert.AreEqual(new Uri("imap://imap.example.net/"), b.Uri);

      b.Port = 10143;

      Assert.AreEqual(new Uri("imap://imap.example.net:10143/"), b.Uri);

      b.UserName = "user";

      Assert.AreEqual(new Uri("imap://user@imap.example.net:10143/"), b.Uri);

      b.AuthType = ImapAuthenticationMechanism.CRAMMD5;

      Assert.AreEqual(new Uri("imap://user;AUTH=CRAM-MD5@imap.example.net:10143/"), b.Uri);

      b.Scheme = "imaps";

      Assert.AreEqual(new Uri("imaps://user;AUTH=CRAM-MD5@imap.example.net:10143/"), b.Uri);

      b.Scheme = "IMAP";

      Assert.AreEqual(new Uri("imap://user;AUTH=CRAM-MD5@imap.example.net:10143/"), b.Uri);

      b.UserName = null;

      Assert.AreEqual(new Uri("imap://;AUTH=CRAM-MD5@imap.example.net:10143/"), b.Uri);

      b.Port = 143;

      Assert.AreEqual(new Uri("imap://;AUTH=CRAM-MD5@imap.example.net:143/"), b.Uri);

      b.Port = 0;

      Assert.AreEqual(new Uri("imap://;AUTH=CRAM-MD5@imap.example.net:0/"), b.Uri);

      b.Port = -1;

      Assert.AreEqual(new Uri("imap://;AUTH=CRAM-MD5@imap.example.net/"), b.Uri);
    }

    [Test]
    public void TestBuildListsOfMessagesFormUri()
    {
      var b = new ImapUriBuilder("imap://localhost/", "INBOX");

      Assert.AreEqual(new Uri("imap://localhost/INBOX"), b.Uri);

      b.SearchCriteria = new SearchQuery("ALL");

      Assert.AreEqual(new Uri("imap://localhost/INBOX?ALL"), b.Uri);

      b.UidValidity = 12345L;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?ALL"), b.Uri);

      b.SearchCriteria = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"), b.Uri);

      b.UidValidity = 0L;

      Assert.AreEqual(new Uri("imap://localhost/INBOX"), b.Uri);
    }

    [Test]
    public void TestBuildListsOfMessagesFormUriNonAsciiMailboxName()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "psicorp.example.org";
      b.Mailbox = "~peter/日本語/台北";

      Assert.AreEqual(new Uri("imap://psicorp.example.org/~peter/%E6%97%A5%E6%9C%AC%E8%AA%9E/%E5%8F%B0%E5%8C%97"),
                      b.Uri);

      b.Scheme = "imaps";
      b.Host = "localhost";
      b.Mailbox = "INBOX.新しいメールボックス";

      Assert.AreEqual(new Uri("imaps://localhost/INBOX.%E6%96%B0%E3%81%97%E3%81%84%E3%83%A1%E3%83%BC%E3%83%AB%E3%83%9C%E3%83%83%E3%82%AF%E3%82%B9"),
                      b.Uri);
    }

    [Test]
    public void TestBuildListsOfMessagesFormUriSearchCriteria()
    {
      var b = new ImapUriBuilder("imap://localhost/", "INBOX", 12345L, new SearchQuery("ALL"));

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?ALL"), b.Uri);

      b.Charset = Encoding.UTF8;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345?ALL"), b.Uri);

      b.SearchCriteria = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"), b.Uri);

      b.Charset = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345"), b.Uri);
    }

    [Test]
    public void TestBuildSpecificMessageOrMessagePartFormUri()
    {
      var b = new ImapUriBuilder("imap://localhost/", "INBOX");

      b.Uid = 1L;

      Assert.AreEqual(new Uri("imap://localhost/INBOX/;UID=1"), b.Uri);

      b.UidValidity = 12345L;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1"), b.Uri);

      b.Section = "1.2";

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=1.2"), b.Uri);

      b.Partial = new ImapPartialRange(0L, 1024L);

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=1.2/;PARTIAL=0.1024"), b.Uri);

      b.Section = "HEADER.FIELDS (DATE FROM)";

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=HEADER.FIELDS%20(DATE%20FROM)/;PARTIAL=0.1024"), b.Uri);

      b.Partial = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=HEADER.FIELDS%20(DATE%20FROM)"), b.Uri);

      b.Section = "1.3.HEADER.FIELDS.NOT (DATE FROM)";

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=1.3.HEADER.FIELDS.NOT%20(DATE%20FROM)"), b.Uri);

      b.Partial = new ImapPartialRange(1024L);

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;SECTION=1.3.HEADER.FIELDS.NOT%20(DATE%20FROM)/;PARTIAL=1024"), b.Uri);

      b.Section = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX;UIDVALIDITY=12345/;UID=1/;PARTIAL=1024"), b.Uri);

      b.UidValidity = 0L;

      Assert.AreEqual(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=1024"), b.Uri);

      b.Partial = null;

      Assert.AreEqual(new Uri("imap://localhost/INBOX/;UID=1"), b.Uri);
    }

    [Test]
    public void TestBuildMailboxStartsWithSlash()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imaps";
      b.Host = "localhost";
      b.Mailbox = "/INBOX.child";

      Assert.AreEqual(new Uri("imaps://localhost/%2fINBOX.child"),
                      b.Uri);
    }

    [Test]
    public void TestBuildExample1()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "minbari.example.org";
      b.Mailbox = "gray-council";
      b.UidValidity = 385759045L;
      b.Uid = 20L;
      b.Partial = new ImapPartialRange(0L, 1024L);

      Assert.AreEqual(new Uri("imap://minbari.example.org/gray-council;UIDVALIDITY=385759045/;UID=20/;PARTIAL=0.1024"),
                      b.Uri);
    }

    [Test]
    public void TestBuildExample2()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "psicorp.example.org";
      b.Mailbox = "~peter/日本語/台北";

      Assert.AreEqual(new Uri("imap://psicorp.example.org/~peter/%E6%97%A5%E6%9C%AC%E8%AA%9E/%E5%8F%B0%E5%8C%97"),
                      b.Uri);
    }

    [Test]
    public void TestBuildExample3()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "minbari.example.org";
      b.AuthType = ImapAuthenticationMechanism.Gssapi;
      b.Mailbox = "gray-council";
      b.Uid = 20;
      b.Section = "1.2";

      Assert.AreEqual(new Uri("imap://;AUTH=GSSAPI@minbari.example.org/gray-council/;UID=20/;SECTION=1.2"),
                      b.Uri);
    }

    [Test]
    public void TestBuildExample4()
    {
      var b = new ImapUriBuilder();

      b.Scheme = "imap";
      b.Host = "minbari.example.org";
      b.AuthType = ImapAuthenticationMechanism.SelectAppropriate;
      b.Mailbox = "gray council";
      b.SearchCriteria = new SearchQuery("SUBJECT shadows");

      Assert.AreEqual(new Uri("imap://;AUTH=*@minbari.example.org/gray%20council?SUBJECT%20shadows"),
                      b.Uri);
    }
  }
}
