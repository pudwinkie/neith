using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class PlainMechanismTests {
    [Test]
    public void TestCreate()
    {
      foreach (var name in new[] {
        "PLAIN",
        "plain",
      }) {
        using (var mechanism = SaslClientMechanism.Create(name)) {
          Assert.IsInstanceOfType(typeof(PlainMechanism), mechanism);
          Assert.AreEqual("PLAIN", mechanism.Name);
        }
      }
    }

    [Test]
    public void TestIsPlainText()
    {
      Assert.IsTrue(SaslClientMechanism.IsMechanismPlainText("PLAIN"));

      using (var client = new PlainMechanism()) {
        Assert.IsTrue(client.IsPlainText);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestCredentialNotSet()
    {
      using (var client = new PlainMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test]
    public void TestExchangeWithoutAuthcid()
    {
      using (var client = new PlainMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("tim", "tanstaaftanstaaf");

        byte[] clientResponse;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(new byte[0], out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);
        
        BytesAssert.AreEqual("\0tim\0tanstaaftanstaaf", clientResponse);
      }
    }

    [Test]
    public void TestExchangeWithAuthcid()
    {
      using (var client = new PlainMechanism()) {
        client.Credential = new NetworkCredential("Kurt", "xipj3plmq", "Ursel");

        byte[] serverChallenge = new byte[0];
        byte[] clientResponse;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual("Ursel\0Kurt\0xipj3plmq", clientResponse);

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
      using (var client = new PlainMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = credential;

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
    public void TestInitialize()
    {
      using (var client = new AnonymousMechanism()) {
        client.Credential = new NetworkCredential("test", "test", "test");

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
      using (var client = new PlainMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        Assert.IsTrue(client.ClientFirst);

        client.Credential = new NetworkCredential("test", "test", "test");

        byte[] initialResponse;

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.GetInitialResponse(out initialResponse));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);

        BytesAssert.AreEqual(Convert.FromBase64String("dGVzdAB0ZXN0AHRlc3Q="),
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
