using System;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class DigestMD5MechanismTests {
    [Test]
    public void TestCreate()
    {
      foreach (var name in new[] {
        "DIGEST-MD5",
        "digest-md5",
      }) {
        using (var mechanism = SaslClientMechanism.Create(name)) {
          Assert.IsInstanceOfType(typeof(DigestMD5Mechanism), mechanism);
          Assert.AreEqual("DIGEST-MD5", mechanism.Name);
        }
      }
    }

    [Test]
    public void TestIsPlainText()
    {
      Assert.IsFalse(SaslClientMechanism.IsMechanismPlainText("DIGEST-MD5"));

      using (var client = new DigestMD5Mechanism()) {
        Assert.IsFalse(client.IsPlainText);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestCredentialNotSet()
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.ServiceName = "imap";

        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestServiceNameNotSet()
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("user", "pass");

        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test]
    public void TestExchange()
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("chris", "secret", "elwood.innosoft.com");
        client.ServiceName = "imap";
        client.Cnonce = Encoding.ASCII.GetBytes("OA6MHXh6VqTrRk");

        /*
         * step 1, 2
         */
        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = Convert.FromBase64String("cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                                                   "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                                                   "cnNldD11dGYtOA==");

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);
        
        BytesAssert.AreEqual(Convert.FromBase64String("Y2hhcnNldD11dGYtOCx1c2VybmFtZT0iY2hyaXMiLHJlYWxtPSJlbHdvb2" +
                                                      "QuaW5ub3NvZnQuY29tIixub25jZT0iT0E2TUc5dEVRR20yaGgiLG5jPTAw" +
                                                      "MDAwMDAxLGNub25jZT0iT0E2TUhYaDZWcVRyUmsiLGRpZ2VzdC11cmk9Im" +
                                                      "ltYXAvZWx3b29kLmlubm9zb2Z0LmNvbSIscmVzcG9uc2U9ZDM4OGRhZDkw" +
                                                      "ZDRiYmQ3NjBhMTUyMzIxZjIxNDNhZjcscW9wPWF1dGg="),
                             clientResponse);

        /*
         * step 3
         */
        serverChallenge = Convert.FromBase64String("cnNwYXV0aD1lYTQwZjYwMzM1YzQyN2I1NTI3Yjg0ZGJhYmNkZmZmZA==");

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual(new byte[0], clientResponse);

        try {
          client.Exchange(serverChallenge, out clientResponse);
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestExchangeServiceNameNotSet()
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential();
        client.ServiceName = null;

        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = Convert.FromBase64String("cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                                                   "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                                                   "cnNldD11dGYtOA==");

        client.Exchange(serverChallenge, out clientResponse);
      }
    }

    [Test]
    public void TestExchangeCredentialUsernameEmpty()
    {
      ExchangeEmptyCredentialProperty(new NetworkCredential(null, "secret", "elwood.innosoft.com"));
    }

    [Test]
    public void TestExchangeCredentialPasswordEmpty()
    {
      ExchangeEmptyCredentialProperty(new NetworkCredential("chris", (string)null, "elwood.innosoft.com"));
    }

    private void ExchangeEmptyCredentialProperty(NetworkCredential cred)
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = cred;
        client.ServiceName = "imap";

        /*
         * step 1, 2
         */
        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = Convert.FromBase64String("cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                                                   "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                                                   "cnNldD11dGYtOA==");

        Assert.AreEqual(SaslExchangeStatus.Failed,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Failed, client.ExchangeStatus);

        Assert.IsNull(clientResponse);

        try {
          client.Exchange(serverChallenge, out clientResponse);
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }
      }
    }

    [Test]
    public void TestInitialize()
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("chris", "secret", "elwood.innosoft.com");
        client.ServiceName = "imap";
        client.Cnonce = Encoding.ASCII.GetBytes("OA6MHXh6VqTrRk");

        byte[] serverChallenge;
        byte[] clientResponseFirst;

        serverChallenge = Convert.FromBase64String("cmVhbG09ImVsd29vZC5pbm5vc29mdC5jb20iLG5vbmNlPSJPQTZNRzl0" +
                                                   "RVFHbTJoaCIscW9wPSJhdXRoIixhbGdvcml0aG09bWQ1LXNlc3MsY2hh" +
                                                   "cnNldD11dGYtOA==");

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponseFirst));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);

        client.Initialize();

        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] clientResponseSecond;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponseSecond));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);

        Assert.AreNotEqual(clientResponseFirst, clientResponseSecond);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestGetInitialResponse()
    {
      using (var client = new DigestMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        Assert.IsFalse(client.ClientFirst);

        client.Credential = new NetworkCredential("chris", "secret", "elwood.innosoft.com");

        byte[] initialResponse;

        client.GetInitialResponse(out initialResponse);
      }
    }
  }
}
