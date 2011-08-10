using System;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class CRAMMD5MechanismTests {
    [Test]
    public void TestCreate()
    {
      foreach (var name in new[] {
        "CRAM-MD5",
        "cram-md5",
      }) {
        using (var mechanism = SaslClientMechanism.Create(name)) {
          Assert.IsInstanceOfType(typeof(CRAMMD5Mechanism), mechanism);
          Assert.AreEqual("CRAM-MD5", mechanism.Name);
        }
      }
    }

    [Test]
    public void TestIsPlainText()
    {
      Assert.IsFalse(SaslClientMechanism.IsMechanismPlainText("CRAM-MD5"));

      using (var client = new CRAMMD5Mechanism()) {
        Assert.IsFalse(client.IsPlainText);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestCredentialNotSet()
    {
      using (var client = new CRAMMD5Mechanism()) {
        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test]
    public void TestExchange()
    {
      using (var client = new CRAMMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("tim", "tanstaaftanstaaf");

        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = Convert.FromBase64String("PDE4OTYuNjk3MTcwOTUyQHBvc3RvZmZpY2UucmVzdG9uLm1jaS5uZXQ+");

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponse));

        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual(Convert.FromBase64String("dGltIGI5MTNhNjAyYzdlZGE3YTQ5NWI0ZTZlNzMzNGQzODkw"),
                             clientResponse);

        try {
          client.Exchange(serverChallenge, out clientResponse);
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }
      }
    }

    [Test]
    public void TestExchangeCredentialUsernameEmpty()
    {
      ExchangeEmptyCredentialProperty(new NetworkCredential(null, "tanstaaftanstaaf"));
    }

    [Test]
    public void TestExchangeCredentialPasswordEmpty()
    {
      ExchangeEmptyCredentialProperty(new NetworkCredential("tim", (string)null));
    }

    private void ExchangeEmptyCredentialProperty(NetworkCredential credential)
    {
      using (var client = new CRAMMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = credential;

        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = Convert.FromBase64String("PDE4OTYuNjk3MTcwOTUyQHBvc3RvZmZpY2UucmVzdG9uLm1jaS5uZXQ+");

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
      using (var client = new CRAMMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("tim", "tanstaaftanstaaf");

        byte[] serverChallenge;
        byte[] clientResponseFirst;

        serverChallenge = Convert.FromBase64String("PDE4OTYuNjk3MTcwOTUyQHBvc3RvZmZpY2UucmVzdG9uLm1jaS5uZXQ+");

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponseFirst));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        client.Initialize();

        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] clientResponseSecond;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponseSecond));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        Assert.AreEqual(clientResponseFirst, clientResponseSecond);
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestGetInitialResponse()
    {
      using (var client = new CRAMMD5Mechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        Assert.IsFalse(client.ClientFirst);

        client.Credential = new NetworkCredential("tim", "tanstaaftanstaaf");

        byte[] initialResponse;

        client.GetInitialResponse(out initialResponse);
      }
    }
  }
}
