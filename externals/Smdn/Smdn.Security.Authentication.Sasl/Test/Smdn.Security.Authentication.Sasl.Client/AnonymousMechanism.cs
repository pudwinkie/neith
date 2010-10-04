using System;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class AnonymousMechanismTests {
    [Test]
    public void TestCreate()
    {
      foreach (var name in new[] {
        "ANONYMOUS",
        "anonymous",
      }) {
        using (var mechanism = SaslClientMechanism.Create(name)) {
          Assert.IsInstanceOfType(typeof(AnonymousMechanism), mechanism);
          Assert.AreEqual("ANONYMOUS", mechanism.Name);
        }
      }
    }

    [Test]
    public void TestIsPlainText()
    {
      Assert.IsTrue(SaslClientMechanism.IsMechanismPlainText("ANONYMOUS"));

      using (var client = new AnonymousMechanism()) {
        Assert.IsTrue(client.IsPlainText);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestCredentialNotSet()
    {
      using (var client = new AnonymousMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test]
    public void TestExchange()
    {
      using (var client = new AnonymousMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("sirhc", "passwordnotused");

        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = new byte[0];

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponse));

        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual(Convert.FromBase64String("c2lyaGM="),
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
      using (var client = new AnonymousMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential();

        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = new byte[0];

        Assert.AreEqual(SaslExchangeStatus.Failed,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.IsNull(clientResponse);

        Assert.AreEqual(SaslExchangeStatus.Failed, client.ExchangeStatus);

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
        client.Credential = new NetworkCredential("sirhc", "passwordnotused");

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
      using (var client = new AnonymousMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        Assert.IsTrue(client.ClientFirst);

        client.Credential = new NetworkCredential("sirhc", "passwordnotused");

        byte[] initialResponse;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.GetInitialResponse(out initialResponse));

        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual(Convert.FromBase64String("c2lyaGM="),
                             initialResponse);

        try {
          byte[] clientResponse;

          client.Exchange(new byte[0], out clientResponse);
          Assert.Fail("InvalidOperationException not thrown");
        }
        catch (InvalidOperationException) {
        }
      }
    }
  }
}
