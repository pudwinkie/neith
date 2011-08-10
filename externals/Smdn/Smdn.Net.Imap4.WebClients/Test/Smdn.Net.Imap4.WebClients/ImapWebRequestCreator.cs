using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapWebRequestCreatorTests {
    [SetUp]
    public void Setup()
    {
      ImapWebRequestCreator.RegisterPrefix();
    }

    [Test]
    public void TestRegisterPrefix()
    {
      var imapRequest = WebRequest.Create("imap://localhost/");

      Assert.IsInstanceOfType(typeof(ImapWebRequest), imapRequest);

      var imapsRequest = WebRequest.Create("imaps://localhost/");

      Assert.IsInstanceOfType(typeof(ImapWebRequest), imapsRequest);
    }

    [Test]
    public void TestIWebRequestCreate()
    {
      var imapServerRequest = WebRequest.Create("imap://localhost/");

      Assert.IsInstanceOfType(typeof(ImapWebRequest), imapServerRequest);
      StringAssert.Contains("ImapServerWebRequest", imapServerRequest.GetType().FullName);

      var imapMailboxRequest = WebRequest.Create("imap://localhost/INBOX");

      Assert.IsInstanceOfType(typeof(ImapWebRequest), imapMailboxRequest);
      StringAssert.Contains("ImapMailboxWebRequest", imapMailboxRequest.GetType().FullName);

      var imapFetchMessageRequest = WebRequest.Create("imap://localhost/INBOX/;UID=1");

      Assert.IsInstanceOfType(typeof(ImapWebRequest), imapFetchMessageRequest);
      StringAssert.Contains("ImapFetchMessageWebRequest", imapFetchMessageRequest.GetType().FullName);

      var imapSearchMessageRequest = WebRequest.Create("imap://localhost/INBOX?UID 1");

      Assert.IsInstanceOfType(typeof(ImapWebRequest), imapSearchMessageRequest);
      StringAssert.Contains("ImapSearchMessageWebRequest", imapSearchMessageRequest.GetType().FullName);

      foreach (var uri in new[] {
        "imap://localhost/?",
        "imap://localhost/;uid=1",
        "imap://localhost/;uidvalidity=1",
      }) {
        try {
          WebRequest.Create(uri);
          Assert.Fail("ArgumentException not thrown: {0}", uri);
        }
        catch (ArgumentException) {
        }
      }
    }
  }
}
