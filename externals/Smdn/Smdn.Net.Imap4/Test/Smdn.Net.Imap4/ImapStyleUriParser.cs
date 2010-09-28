using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapStyleUriParserTests {
    private void ExpectArgumentException(Action action)
    {
      try {
        action();
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    private void ExpectArgumentNullException(Action action)
    {
      try {
        action();
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test]
    public void TestCheckUriScheme()
    {
      try {
        ImapStyleUriParser.GetUid(new Uri("imap://localhost/"));
        ImapStyleUriParser.GetUid(new Uri("imaps://localhost/"));
        ImapStyleUriParser.GetUid(new Uri("IMAP://localhost/"));
        ImapStyleUriParser.GetUid(new Uri("IMAPS://localhost/"));
      }
      catch (ArgumentException) {
        Assert.Fail("ArgumentException thrown");
      }
    }

    [Test]
    public void TestGetAuthority()
    {
      Assert.AreEqual("localhost:143", ImapStyleUriParser.GetAuthority(new Uri("imap://localhost")));
      Assert.AreEqual("localhost:993", ImapStyleUriParser.GetAuthority(new Uri("imaps://localhost")));
      Assert.AreEqual("localhost:10143", ImapStyleUriParser.GetAuthority(new Uri("imap://localhost:10143")));
      Assert.AreEqual("localhost:10143", ImapStyleUriParser.GetAuthority(new Uri("imaps://localhost:10143")));
      Assert.AreEqual("user@localhost:10143", ImapStyleUriParser.GetAuthority(new Uri("imap://user@localhost:10143")));
      Assert.AreEqual("localhost:10143", ImapStyleUriParser.GetAuthority(new Uri("imap://;auth=*@localhost:10143")));
      Assert.AreEqual("user@localhost:10143", ImapStyleUriParser.GetAuthority(new Uri("imap://user;auth=*@localhost:10143")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetAuthority(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetAuthority(null);});
    }

    [Test]
    public void TestGetStrongAuthority()
    {
      Assert.AreEqual("localhost:143", ImapStyleUriParser.GetStrongAuthority(new Uri("imap://localhost")));
      Assert.AreEqual("user@localhost:143", ImapStyleUriParser.GetStrongAuthority(new Uri("imap://user@localhost")));
      Assert.AreEqual(";auth=*@localhost:143", ImapStyleUriParser.GetStrongAuthority(new Uri("imap://;auth=*@localhost")));
      Assert.AreEqual("user;auth=*@localhost:143", ImapStyleUriParser.GetStrongAuthority(new Uri("imap://user;auth=*@localhost")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetStrongAuthority(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetStrongAuthority(null);});
    }

    [Test]
    public void TestGetUser()
    {
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetUser(new Uri("imap://localhost:10143")));
      Assert.AreEqual("user", ImapStyleUriParser.GetUser(new Uri("imap://user@localhost:10143")));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetUser(new Uri("imap://;auth=*@localhost:10143")));
      Assert.AreEqual("user", ImapStyleUriParser.GetUser(new Uri("imap://user;auth=*@localhost:10143")));
      Assert.AreEqual("user name", ImapStyleUriParser.GetUser(new Uri("imap://user%20name@localhost:10143")));
      Assert.AreEqual("user;auth", ImapStyleUriParser.GetUser(new Uri("imap://user%3bauth@localhost:10143")));
      Assert.AreEqual("user@host", ImapStyleUriParser.GetUser(new Uri("imap://user%40host@localhost:10143")));

      Assert.AreEqual("user", ImapStyleUriParser.GetUser(new Uri("imap://user:pass@localhost:10143")));
      Assert.AreEqual("user", ImapStyleUriParser.GetUser(new Uri("imap://user;auth=*:pass@localhost:10143")));
      Assert.AreEqual("user", ImapStyleUriParser.GetUser(new Uri("imap://user:pass;auth=*@localhost:10143")));
      Assert.AreEqual("user", ImapStyleUriParser.GetUser(new Uri("imap://user;auth=*%3apass@localhost:10143")));
      Assert.AreEqual("user:pass", ImapStyleUriParser.GetUser(new Uri("imap://user%3apass;auth=*@localhost:10143")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetUser(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetUser(null);});
    }

    [Test]
    public void TestGetAuthType()
    {
      Assert.AreEqual(null, ImapStyleUriParser.GetAuthType(new Uri("imap://localhost:10143")));
      Assert.AreEqual(null, ImapStyleUriParser.GetAuthType(new Uri("imap://user@localhost:10143")));
      Assert.AreEqual(ImapAuthenticationMechanism.SelectAppropriate, ImapStyleUriParser.GetAuthType(new Uri("imap://;auth=*@localhost:10143")));
      Assert.AreEqual(ImapAuthenticationMechanism.SelectAppropriate, ImapStyleUriParser.GetAuthType(new Uri("imap://;AUTH=*@localhost:10143")));

      Assert.AreEqual(null, ImapStyleUriParser.GetAuthType(new Uri("imap://%3bauth=*@localhost:10143")));
      Assert.AreEqual(new ImapAuthenticationMechanism("%2a"), ImapStyleUriParser.GetAuthType(new Uri("imap://;auth=%2a@localhost:10143")));

      Assert.AreEqual(null, ImapStyleUriParser.GetAuthType(new Uri("imap://user:pass@localhost:10143")));
      Assert.AreEqual(ImapAuthenticationMechanism.SelectAppropriate, ImapStyleUriParser.GetAuthType(new Uri("imap://user;auth=*:pass@localhost:10143")));
      Assert.AreEqual(null, ImapStyleUriParser.GetAuthType(new Uri("imap://user:pass;auth=*@localhost:10143")));
      Assert.AreEqual(new ImapAuthenticationMechanism("*:pass"), ImapStyleUriParser.GetAuthType(new Uri("imap://user;auth=*%3apass@localhost:10143")));
      Assert.AreEqual(ImapAuthenticationMechanism.SelectAppropriate, ImapStyleUriParser.GetAuthType(new Uri("imap://user%3apass;auth=*@localhost:10143")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetAuthType(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetAuthType(null);});
    }

    [Test]
    public void TestGetMailbox()
    {
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/")));
      Assert.AreEqual("INBOX", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX")));
      Assert.AreEqual("INBOX", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/")));
      Assert.AreEqual("INBOX", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX;UIDVALIDITY=1")));
      Assert.AreEqual("INBOX", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX;UIDVALIDITY=1/")));
      Assert.AreEqual("INBOX/Sent", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/Sent")));
      Assert.AreEqual("INBOX/Sent", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/Sent;UIDVALIDITY=1")));
      Assert.AreEqual("INBOX.Sent", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX.Sent")));
      Assert.AreEqual("INBOX.Sent", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX.Sent;UIDVALIDITY=1")));
      Assert.AreEqual("新しいメールボックス", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/新しいメールボックス")));
      Assert.AreEqual("新しいメールボックス", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/%E6%96%B0%E3%81%97%E3%81%84%E3%83%A1%E3%83%BC%E3%83%AB%E3%83%9C%E3%83%83%E3%82%AF%E3%82%B9")));
      Assert.AreEqual("&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-", ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/&ZbAwVzBEMOEw,DDrMNwwwzCvMLk-")));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/;")));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/?")));

      //TODO: Assert.AreEqual("/INBOX",               ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/%2fINBOX")));
      Assert.AreEqual("INBOX Sent",           ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX%20Sent")));
      Assert.AreEqual("INBOX/Sent",           ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/;uid=1/../Sent")));
      Assert.AreEqual("INBOX/Sent",           ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/./Sent/")));
      Assert.AreEqual("INBOX/../Sent",        ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/%2e%2e/Sent")));
      Assert.AreEqual("INBOX/./Sent",         ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/%2e/Sent/")));
      Assert.AreEqual("INBOX?",               ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX%3f")));
      Assert.AreEqual("?",                    ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/%3f")));
      Assert.AreEqual("INBOX;uidvalidity=1",  ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX%3buidvalidity=1")));
      Assert.AreEqual("INBOX/;uid=1",         ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/INBOX/%3buid=1")));
      Assert.AreEqual(";",                    ImapStyleUriParser.GetMailbox(new Uri("imap://localhost/%3b")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetMailbox(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetMailbox(null);});
    }

    [Test]
    public void TestGetUidValidity()
    {
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/")));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX;UIDVALIDITY=1")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX;uidvalidity=1")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX;UIDVALIDITY=1/")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX;UIDVALIDITY=1?ALL")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX;uidvalidity=1?ALL")));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX/Sent")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX/Sent;UIDVALIDITY=1")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX/Sent;UIDVALIDITY=1?ALL")));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX.Sent")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX.Sent;UIDVALIDITY=1")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX.Sent;UIDVALIDITY=1?ALL")));

      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(new Uri("imap://localhost/INBOX%3bUIDVALIDITY=1")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetUidValidity(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetUidValidity(null);});
    }

    [Test]
    public void TestGetUid()
    {
      Assert.AreEqual(0L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/;UID=1")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/;uid=1")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=0.1024")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/;UID=1?ALL")));
      Assert.AreEqual(1L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/;uid=1?ALL")));

      Assert.AreEqual(0L, ImapStyleUriParser.GetUid(new Uri("imap://localhost/INBOX/%3buid=1")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetUid(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetUid(null);});
    }

    [Test]
    public void TestGetSection()
    {
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1")));
      Assert.AreEqual("1.1",        ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=1.1")));
      Assert.AreEqual("1.1",        ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;section=1.1")));
      Assert.AreEqual("1.1",        ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;section=1%2e1")));
      Assert.AreEqual("1.1",        ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=1.1/")));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=0.1024")));
      Assert.AreEqual("1.1",        ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=1.1/;PARTIAL=0.1024")));
      Assert.AreEqual("HEADER",     ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=HEADER")));
      Assert.AreEqual("3.HEADER",   ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=3.HEADER")));
      Assert.AreEqual("3.HEADER",   ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=3%2eHEADER")));

      Assert.AreEqual("HEADER.FIELDS (DATE FROM)",
                      ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=HEADER.FIELDS (DATE FROM)")));
      Assert.AreEqual("HEADER.FIELDS (DATE FROM)",
                      ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=HEADER%2eFIELDS%20(DATE%20FROM)")));
      Assert.AreEqual("HEADER.FIELDS (DATE FROM)",
                      ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=HEADER.FIELDS (DATE FROM)/;PARTIAL=0.1024")));
      Assert.AreEqual("1.1.HEADER.FIELDS (DATE FROM)",
                      ImapStyleUriParser.GetSection(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=1.1.HEADER.FIELDS (DATE FROM)")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetSection(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetSection(null);});
    }

    [Test]
    public void TestGetPartial()
    {
      Assert.IsNull(                                    ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1")));
      Assert.IsNull(                                    ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=1.1")));
      Assert.AreEqual(new ImapPartialRange(0L, 1024L),  ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=0.1024")));
      Assert.AreEqual(new ImapPartialRange(0L, 1024L),  ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;partial=0.1024")));
      Assert.AreEqual(new ImapPartialRange(0L, 1024L),  ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=0.1024/")));
      Assert.AreEqual(new ImapPartialRange(0L),         ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=0")));
      Assert.AreEqual(new ImapPartialRange(1024L),      ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;PARTIAL=1024")));
      Assert.AreEqual(new ImapPartialRange(0L, 1024L),  ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/;SECTION=1.1/;PARTIAL=0.1024")));

      Assert.IsNull(                ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/%3bPARTIAL=0.1024")));
      Assert.IsNull(                ImapStyleUriParser.GetPartial(new Uri("imap://localhost/INBOX/;UID=1/%3bPARTIAL=0.1024/")));

      ExpectArgumentException(delegate {ImapStyleUriParser.GetPartial(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetPartial(null);});
    }

    [Test]
    public void TestGetPartialMalformed()
    {
      foreach (var uri in new[] {
        "imap://localhost/INBOX/;UID=1/;PARTIAL=0%2e1024",
        "imap://localhost/INBOX/;UID=1/;PARTIAL=0.",
        "imap://localhost/INBOX/;UID=1/;PARTIAL=.1024",
        "imap://localhost/INBOX/;UID=1/;PARTIAL=0.1024.",
        "imap://localhost/INBOX/;UID=1/;PARTIAL=.0.1024",
        "imap://localhost/INBOX/;UID=1/;PARTIAL=.0.1024.",
      }) {
        try {
          ImapStyleUriParser.GetPartial(new Uri(uri));
          Assert.Fail("UriFormatException not thrown");
        }
        catch (UriFormatException) {
        }
      }
    }

    [Test]
    public void TestGetUriForm()
    {
      TestGetUriForm(ImapUriForm.Server, "imap://localhost/");
      TestGetUriForm(ImapUriForm.Server, "imap://localhost:10143/");
      TestGetUriForm(ImapUriForm.Server, "imap://user@localhost:10143/");
      TestGetUriForm(ImapUriForm.Server, "imap://;auth=*@localhost:10143/");
      TestGetUriForm(ImapUriForm.Server, "imap://user;auth=*@localhost:10143/");

      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX/");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX/Sent/");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX.Sent/");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX/Sent;uidvalidity=1/");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX.Sent;uidvalidity=1/");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX/;uid=1/../Sent");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX/./Sent/");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX%3f");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/%3f");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX%3buidvalidity=1");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/INBOX/%3buid=1");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/%3buid=1");
      TestGetUriForm(ImapUriForm.ListMessages, "imap://localhost/%3b");

      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX/;uid=1");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX/;uid=1/;section=1.1");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX/;uid=1/;partial=0.1024");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX/;uid=1/;section=1.1/;partial=0.1024");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX;uidvalidity=1/;uid=1");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX/Sent/;uid=1");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX.Sent/;uid=1");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX/Sent;uidvalidity=1/;uid=1");
      TestGetUriForm(ImapUriForm.FetchMessage, "imap://localhost/INBOX.Sent;uidvalidity=1/;uid=1");

      TestGetUriForm(ImapUriForm.SearchMessages, "imap://localhost/INBOX?");
      TestGetUriForm(ImapUriForm.SearchMessages, "imap://localhost/INBOX?UID 1");
      TestGetUriForm(ImapUriForm.SearchMessages, "imap://localhost/INBOX;uidvalidity=1?UID 1");

      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/?");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/?uid 1");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/;uidvalidity=1");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/;uid=1");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/;section=1.1");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/;partial=0.1024");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/INBOX/../;uid=1");
      TestGetUriForm(ImapUriForm.Unknown, "imap://localhost/INBOX/../?uid 1");

      ExpectArgumentException(delegate {ImapStyleUriParser.GetUriForm(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {ImapStyleUriParser.GetUriForm(null);});
    }

    private void TestGetUriForm(ImapUriForm expected, string uri)
    {
      Assert.AreEqual(expected, ImapStyleUriParser.GetUriForm(new Uri(uri)), uri);
    }

    [Test()]
    public void TestImapUrlExamples1()
    {
      //   imap://<iserver>/
      var uri = new Uri("imap://localhost:993/");

      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetUser(uri));
      Assert.AreEqual("localhost:993", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("localhost:993", ImapStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(null, ImapStyleUriParser.GetAuthType(uri));
      Assert.AreEqual(ImapUriForm.Server, ImapStyleUriParser.GetUriForm(uri));

      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUid(uri));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(uri));
      Assert.IsNull(ImapStyleUriParser.GetPartial(uri));
    }

    [Test()]
    public void TestImapUrlExamples2()
    {
      var uri = new Uri("imap://user;auth=plain@localhost");

      Assert.AreEqual("user", ImapStyleUriParser.GetUser(uri));
      Assert.AreEqual("user@localhost:143", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("user;auth=plain@localhost:143", ImapStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(ImapAuthenticationMechanism.Plain, ImapStyleUriParser.GetAuthType(uri));
      Assert.AreEqual(ImapUriForm.Server, ImapStyleUriParser.GetUriForm(uri));

      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUid(uri));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(uri));
      Assert.IsNull(ImapStyleUriParser.GetPartial(uri));
    }

    [Test()]
    public void TestImapUrlExamples3()
    {
      //   imap://<iserver>/<enc_mailbox>[uidvalidity]<iuid>[isection]
      var uri = new Uri("imap://psicorp.example.org/~peter/%E6%97%A5%E6%9C%AC%E8%AA%9E/%E5%8F%B0%E5%8C%97");

      Assert.AreEqual("psicorp.example.org:143", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("~peter/日本語/台北", ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(ImapUriForm.ListMessages, ImapStyleUriParser.GetUriForm(uri));
    }

    [Test()]
    public void TestImapUrlExamples4()
    {
      var uri = new Uri("imap://minbari.example.org/gray-council;UIDVALIDITY=385759045/;UID=20/;PARTIAL=0.1024");

      Assert.AreEqual("minbari.example.org:143", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("gray-council", ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(385759045L, ImapStyleUriParser.GetUidValidity(uri));
      Assert.AreEqual(20L, ImapStyleUriParser.GetUid(uri));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(uri));
      Assert.AreEqual(new ImapPartialRange(0L, 1024L), ImapStyleUriParser.GetPartial(uri));
      Assert.AreEqual(ImapUriForm.FetchMessage, ImapStyleUriParser.GetUriForm(uri));
    }

    [Test()]
    public void TestImapUrlExamples5()
    {
      var uri = new Uri("imap://;AUTH=GSSAPI@minbari.example.org/gray-council/;uid=20/;section=1.2");

      Assert.AreEqual("minbari.example.org:143", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual(";AUTH=GSSAPI@minbari.example.org:143", ImapStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(ImapAuthenticationMechanism.Gssapi, ImapStyleUriParser.GetAuthType(uri));
      Assert.AreEqual("gray-council", ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(uri));
      Assert.AreEqual(20L, ImapStyleUriParser.GetUid(uri));
      Assert.AreEqual("1.2", ImapStyleUriParser.GetSection(uri));
      Assert.IsNull(ImapStyleUriParser.GetPartial(uri));
      Assert.AreEqual(ImapUriForm.FetchMessage, ImapStyleUriParser.GetUriForm(uri));
    }

    [Test]
    public void TestImapUrlExamples6()
    {
      var uri = new Uri("imap://;AUTH=*@minbari.example.org/gray%20council?SUBJECT%20shadows");

      Assert.AreEqual("minbari.example.org:143", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual(";AUTH=*@minbari.example.org:143", ImapStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(ImapAuthenticationMechanism.SelectAppropriate, ImapStyleUriParser.GetAuthType(uri));
      Assert.AreEqual("gray council", ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUid(uri));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(uri));
      Assert.IsNull(ImapStyleUriParser.GetPartial(uri));
      //Assert.AreEqual("?SUBJECT%20shadows", uri.Query);
      Assert.AreEqual(ImapUriForm.SearchMessages, ImapStyleUriParser.GetUriForm(uri));
    }

    [Test]
    public void TestImapUrlExamples7()
    {
      var uri = new Uri("imap://john;AUTH=*@minbari.example.org/babylon5/personel?" +
                        "charset%20UTF-8%20SUBJECT%20%7B14+%7D%0D%0A%D0%98%D0%B2%" +
                        "D0%B0%D0%BD%D0%BE%D0%B2%D0%B0");

      Assert.AreEqual("john@minbari.example.org:143", ImapStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("john;AUTH=*@minbari.example.org:143", ImapStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(ImapAuthenticationMechanism.SelectAppropriate, ImapStyleUriParser.GetAuthType(uri));
      Assert.AreEqual("babylon5/personel", ImapStyleUriParser.GetMailbox(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUidValidity(uri));
      Assert.AreEqual(0L, ImapStyleUriParser.GetUid(uri));
      Assert.AreEqual(string.Empty, ImapStyleUriParser.GetSection(uri));
      Assert.IsNull(ImapStyleUriParser.GetPartial(uri));
      //Assert.AreEqual("?charset%20UTF-8%20SUBJECT%20%7B14+%7D%0D%0A%D0%98%D0%B2%D0%B0%D0%BD%D0%BE%D0%B2%D0%B0", uri.Query);
      Assert.AreEqual(ImapUriForm.SearchMessages, ImapStyleUriParser.GetUriForm(uri));
    }
  }
}
