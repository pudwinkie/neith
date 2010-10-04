using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class LoginMechanismTests {
    [Test]
    public void TestCreate()
    {
      foreach (var name in new[] {
        "LOGIN",
        "login",
      }) {
        using (var mechanism = SaslClientMechanism.Create(name)) {
          Assert.IsInstanceOfType(typeof(LoginMechanism), mechanism);
          Assert.AreEqual("LOGIN", mechanism.Name);
        }
      }
    }

    [Test]
    public void TestIsPlainText()
    {
      Assert.IsTrue(SaslClientMechanism.IsMechanismPlainText("LOGIN"));

      using (var client = new LoginMechanism()) {
        Assert.IsTrue(client.IsPlainText);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestCredentialNotSet()
    {
      using (var client = new LoginMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test]
    public void TestExchange()
    {
      using (var client = new LoginMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("user", "pass");

        byte[] serverChallenge = new byte[0];
        byte[] clientResponse;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);

        BytesAssert.AreEqual(client.Credential.UserName, clientResponse);

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual(client.Credential.Password, clientResponse);

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
      using (var client = new LoginMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential(null, "pass");

        byte[] serverChallenge = new byte[0];
        byte[] clientResponse;

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
    public void TestExchangeCredentialPasswordEmpty()
    {
      using (var client = new LoginMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("user", (string)null);

        byte[] serverChallenge = new byte[0];
        byte[] clientResponse;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);
        
        BytesAssert.AreEqual(client.Credential.UserName, clientResponse);

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
      using (var client = new AnonymousMechanism()) {
        client.Credential = new NetworkCredential("user", "pass");

        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] initialResponseFirst;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.GetInitialResponse(out initialResponseFirst));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        client.Initialize();

        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] initialResponseSecond;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.GetInitialResponse(out initialResponseSecond));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        Assert.AreEqual(initialResponseFirst, initialResponseSecond);
      }
    }

    [Test]
    public void TestGetInitialResponse()
    {
      using (var client = new LoginMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        Assert.IsTrue(client.ClientFirst);

        client.Credential = new NetworkCredential("user", "pass");

        byte[] initialResponse;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.GetInitialResponse(out initialResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);

        BytesAssert.AreEqual(client.Credential.UserName, initialResponse);
      }
    }
  }
}
