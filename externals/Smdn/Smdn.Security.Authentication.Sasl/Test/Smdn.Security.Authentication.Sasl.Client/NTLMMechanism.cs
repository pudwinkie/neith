using System;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class NTLMMechanismTests {
    [Test]
    public void TestCreate()
    {
      foreach (var name in new[] {
        "NTLM",
        "ntlm",
      }) {
        using (var mechanism = SaslClientMechanism.Create(name)) {
          Assert.IsInstanceOfType(typeof(NTLMMechanism), mechanism);
          Assert.AreEqual("NTLM", mechanism.Name);
        }
      }
    }

    [Test]
    public void TestIsPlainText()
    {
      Assert.IsFalse(SaslClientMechanism.IsMechanismPlainText("NTLM"));

      using (var client = new NTLMMechanism()) {
        Assert.IsFalse(client.IsPlainText);
      }
    }

    [Test, ExpectedException(typeof(SaslException))]
    public void TestCredentialNotSet()
    {
      using (var client = new NTLMMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] clientResponse;

        client.Exchange(new byte[0], out clientResponse);
      }
    }

    [Test]
    public void TestExchange()
    {
      /*
       * http://davenport.sourceforge.net/ntlm.html
       * Appendix B: Application Protocol Usage of NTLM - NTLM IMAP Authentication
       */
      using (var client = new NTLMMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = new NetworkCredential("user", "SecREt01", "DOMAIN");
        client.TargetHost = "WORKSTATION";

        /*
         * Type-1
         */
        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = new byte[0];

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);
        
        BytesAssert.AreEqual(Convert.FromBase64String("TlRMTVNTUAABAAAABzIAAAYABgArAAAACwALACAAAABXT1JLU1RBVElPTkRPTUFJTg=="),
                             clientResponse);

        /*
         * Type-3
         */
        serverChallenge = Convert.FromBase64String("TlRMTVNTUAACAAAADAAMADAAAAABAoEAASNFZ4mrze8AAAAAAAAAAGIAYgA8AAAA" +
                                                   "RABPAE0AQQBJAE4AAgAMAEQATwBNAEEASQBOAAEADABTAEUAUgBWAEUAUgAEABQAZA" +
                                                   "BvAG0AYQBpAG4ALgBjAG8AbQADACIAcwBlAHIAdgBlAHIALgBkAG8AbQBhAGkAbgAu" +
                                                   "AGMAbwBtAAAAAAA=");

        Assert.AreEqual(SaslExchangeStatus.Succeeded,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Succeeded, client.ExchangeStatus);
        
        BytesAssert.AreEqual(Convert.FromBase64String("TlRMTVNTUAADAAAAGAAYAGoAAAAYABgAggAAAAwADABAAAAACAAIAEwAAAAWABYAVA" +
                                                      "AAAAAAAACaAAAAAQIAAEQATwBNAEEASQBOAHUAcwBlAHIAVwBPAFIASwBTAFQAQQBU" +
                                                      "AEkATwBOAMM3zVy9RPyXgqZnr21CfG3mfCDC0+d8ViWpjBwx6BhHRmspst9GgPOZWP" +
                                                      "uMITqcxg=="),
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
      ExchangeEmptyCredentialProperty(new NetworkCredential(null, "pass", "DOMAIN"));
    }

    [Test]
    public void TestExchangeCredentialPaswordEmpty()
    {
      ExchangeEmptyCredentialProperty(new NetworkCredential("user", (string)null, "DOMAIN"));
    }

    private void ExchangeEmptyCredentialProperty(NetworkCredential credential)
    {
      using (var client = new NTLMMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        client.Credential = credential;
        client.TargetHost = "WORKSTATION";

        /*
         * Type-1
         */
        byte[] serverChallenge;
        byte[] clientResponse;

        serverChallenge = new byte[0];

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.Exchange(serverChallenge, out clientResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);
        
        BytesAssert.AreEqual(Convert.FromBase64String("TlRMTVNTUAABAAAABzIAAAYABgArAAAACwALACAAAABXT1JLU1RBVElPTkRPTUFJTg=="),
                             clientResponse);

        /*
         * Type-3
         */
        serverChallenge = Convert.FromBase64String("TlRMTVNTUAACAAAADAAMADAAAAABAoEAASNFZ4mrze8AAAAAAAAAAGIAYgA8AAAA" +
                                                   "RABPAE0AQQBJAE4AAgAMAEQATwBNAEEASQBOAAEADABTAEUAUgBWAEUAUgAEABQAZA" +
                                                   "BvAG0AYQBpAG4ALgBjAG8AbQADACIAcwBlAHIAdgBlAHIALgBkAG8AbQBhAGkAbgAu" +
                                                   "AGMAbwBtAAAAAAA=");

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
      using (var client = new NTLMMechanism()) {
        client.Credential = new NetworkCredential("user", "pass", "DOMAIN");
        client.TargetHost = "WORKSTATION";

        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] initialResponseFirst;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.GetInitialResponse(out initialResponseFirst));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);

        client.Initialize();

        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        byte[] initialResponseSecond;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.GetInitialResponse(out initialResponseSecond));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);

        Assert.AreEqual(initialResponseFirst, initialResponseSecond);
      }
    }

    [Test]
    public void TestGetInitialResponse()
    {
      using (var client = new NTLMMechanism()) {
        Assert.AreEqual(SaslExchangeStatus.None, client.ExchangeStatus);

        Assert.IsTrue(client.ClientFirst);

        client.Credential = new NetworkCredential("user", "pass", "DOMAIN");
        client.TargetHost = "WORKSTATION";

        byte[] initialResponse;

        Assert.AreEqual(SaslExchangeStatus.Continuing,
                        client.GetInitialResponse(out initialResponse));
        Assert.AreEqual(SaslExchangeStatus.Continuing, client.ExchangeStatus);
      }
    }
  }
}
