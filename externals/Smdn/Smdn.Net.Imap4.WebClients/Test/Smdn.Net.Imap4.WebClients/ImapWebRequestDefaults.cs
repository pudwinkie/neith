using System;
using System.Net;
using NUnit.Framework;

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.WebClients {
  [TestFixture]
  public class ImapWebRequestDefaultsTests {
    private static bool defaultKeepAlive = ImapWebRequestDefaults.KeepAlive;
    private static int defaultTimeout = ImapWebRequestDefaults.Timeout;
    private static int defaultReadWriteTimeout = ImapWebRequestDefaults.ReadWriteTimeout;
    private static bool defaultReadOnly = ImapWebRequestDefaults.ReadOnly;
    private static bool defaultUseTilsIfAvailable = ImapWebRequestDefaults.UseTlsIfAvailable;
    private static bool defaultSubscription = ImapWebRequestDefaults.Subscription;
    private static bool defaultAllowCreateMailbox = ImapWebRequestDefaults.AllowCreateMailbox;
    private static int defaultFetchBlockSize = ImapWebRequestDefaults.FetchBlockSize;
    private static bool defaultFetchPeek = ImapWebRequestDefaults.FetchPeek;
    private static ImapFetchDataItemMacro defaultFetchDataItem = ImapWebRequestDefaults.FetchDataItem;
    private static string[] defaultUsingSaslMechanisms = (string[])ImapWebRequestDefaults.UsingSaslMechanisms.Clone();
    private static bool defaultAllowInsecureLogin = ImapWebRequestDefaults.AllowInsecureLogin;
    private static ImapResponseCode[] defaultExpectedErrorResponseCodes = (ImapResponseCode[])ImapWebRequestDefaults.ExpectedErrorResponseCodes.Clone();

    [TearDown]
    public void TearDown()
    {
      ImapWebRequestDefaults.KeepAlive = defaultKeepAlive;
      ImapWebRequestDefaults.Timeout = defaultTimeout;
      ImapWebRequestDefaults.ReadWriteTimeout = defaultReadWriteTimeout;
      ImapWebRequestDefaults.ReadOnly = defaultReadOnly;
      ImapWebRequestDefaults.UseTlsIfAvailable = defaultUseTilsIfAvailable;
      ImapWebRequestDefaults.Subscription = defaultSubscription;
      ImapWebRequestDefaults.AllowCreateMailbox = defaultAllowCreateMailbox;
      ImapWebRequestDefaults.FetchBlockSize = defaultFetchBlockSize;
      ImapWebRequestDefaults.FetchPeek = defaultFetchPeek;
      ImapWebRequestDefaults.FetchDataItem = defaultFetchDataItem;
      ImapWebRequestDefaults.UsingSaslMechanisms = defaultUsingSaslMechanisms;
      ImapWebRequestDefaults.AllowInsecureLogin = defaultAllowInsecureLogin;
      ImapWebRequestDefaults.ExpectedErrorResponseCodes = defaultExpectedErrorResponseCodes;
    }

    [Test]
    public void TestDefaults()
    {
      Assert.IsTrue(defaultKeepAlive);
      Assert.AreEqual(-1, defaultTimeout);
      Assert.AreEqual(300000, defaultReadWriteTimeout);
      Assert.IsFalse(defaultReadOnly);
      Assert.IsTrue(defaultUseTilsIfAvailable);
      Assert.IsTrue(defaultSubscription);
      Assert.IsTrue(defaultAllowCreateMailbox);
      Assert.AreEqual(10240, defaultFetchBlockSize);
      Assert.IsTrue(defaultFetchPeek);
      Assert.AreEqual(ImapFetchDataItemMacro.All, defaultFetchDataItem);
      Assert.IsFalse(defaultAllowInsecureLogin);
      Assert.AreEqual(new[] {"DIGEST-MD5", "CRAM-MD5", "NTLM"}, defaultUsingSaslMechanisms);
      Assert.AreEqual(new ImapResponseCode[0], defaultExpectedErrorResponseCodes);
    }

    [Test]
    public void TestGetSetTimeout()
    {
      ImapWebRequestDefaults.Timeout = 1000;

      Assert.AreEqual(1000, ImapWebRequestDefaults.Timeout);
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestSetTimeoutOutOfRange()
    {
      ImapWebRequestDefaults.Timeout = -2;
    }

    [Test]
    public void TestGetSetReadWriteTimeout()
    {
      ImapWebRequestDefaults.ReadWriteTimeout = 1000;

      Assert.AreEqual(1000, ImapWebRequestDefaults.ReadWriteTimeout);
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestSetReadWriteTimeoutOutOfRange()
    {
      ImapWebRequestDefaults.ReadWriteTimeout = -2;
    }

    [Test]
    public void TestGetSetFetchBlockSize()
    {
      ImapWebRequestDefaults.FetchBlockSize = 1024;

      Assert.AreEqual(1024, ImapWebRequestDefaults.FetchBlockSize);
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestSetFetchBlockSizeOutOfRange()
    {
      ImapWebRequestDefaults.FetchBlockSize = 0;
    }

    [Test]
    public void TestGetSetUsingSaslMechanisms()
    {
      Assert.IsNotNull(ImapWebRequestDefaults.UsingSaslMechanisms, "default non null");

      ImapWebRequestDefaults.UsingSaslMechanisms = new[] {"NTLM", "X-AUTH"};

      Assert.AreEqual(new[] {"NTLM", "X-AUTH"}, ImapWebRequestDefaults.UsingSaslMechanisms);

      ImapWebRequestDefaults.UsingSaslMechanisms = null;

      Assert.IsNotNull(ImapWebRequestDefaults.UsingSaslMechanisms);
      Assert.AreEqual(new string[0], ImapWebRequestDefaults.UsingSaslMechanisms);
    }

    [Test]
    public void TestGetSetExpectedErrorResponseCodes()
    {
      Assert.IsNotNull(ImapWebRequestDefaults.ExpectedErrorResponseCodes, "default non null");

      ImapWebRequestDefaults.ExpectedErrorResponseCodes = new[] {ImapResponseCode.Modified};

      Assert.AreEqual(new[] {ImapResponseCode.Modified}, ImapWebRequestDefaults.ExpectedErrorResponseCodes);

      ImapWebRequestDefaults.ExpectedErrorResponseCodes = null;

      Assert.IsNotNull(ImapWebRequestDefaults.ExpectedErrorResponseCodes);
      Assert.AreEqual(new ImapResponseCode[0], ImapWebRequestDefaults.ExpectedErrorResponseCodes);
    }

    [Test]
    public void TestGetClientID()
    {
      Assert.IsNotNull(ImapWebRequestDefaults.ClientID, "default non null");

      ImapWebRequestDefaults.ClientID["Version"] = "1.0";

      Assert.AreEqual("1.0", ImapWebRequestDefaults.ClientID["version"]);
      Assert.AreEqual("1.0", ImapWebRequestDefaults.ClientID["VERSION"]);
    }

    [Test]
    public void TestCreateDefaultValueAppliedRequest()
    {
      ImapWebRequestCreator.RegisterPrefix();

      ImapWebRequest req;

      ImapWebRequestDefaults.KeepAlive = true;
      ImapWebRequestDefaults.Timeout = -1;
      ImapWebRequestDefaults.ReadWriteTimeout = -1;
      ImapWebRequestDefaults.ReadOnly = false;
      ImapWebRequestDefaults.UseTlsIfAvailable = true;
      ImapWebRequestDefaults.Subscription = true;
      ImapWebRequestDefaults.AllowCreateMailbox = true;
      ImapWebRequestDefaults.FetchBlockSize = 10240;
      ImapWebRequestDefaults.FetchPeek = true;
      ImapWebRequestDefaults.FetchDataItem = ImapFetchDataItemMacro.All;
      ImapWebRequestDefaults.UsingSaslMechanisms = new[] {"DIGEST-MD5", "CRAM-MD5"};
      ImapWebRequestDefaults.AllowInsecureLogin = false;
      ImapWebRequestDefaults.ExpectedErrorResponseCodes = new[] {ImapResponseCode.AlreadyExists, ImapResponseCode.NonExistent};

      req = WebRequest.Create("imap://localhost/") as ImapWebRequest;

      Assert.IsTrue(req.KeepAlive);
      Assert.AreEqual(-1, req.Timeout);
      Assert.AreEqual(-1, req.ReadWriteTimeout);
      Assert.IsFalse(req.ReadOnly);
      Assert.IsTrue(req.UseTlsIfAvailable);
      Assert.IsTrue(req.Subscription);
      Assert.IsTrue(req.AllowCreateMailbox);
      Assert.AreEqual(10240, req.FetchBlockSize);
      Assert.IsTrue(req.FetchPeek);
      Assert.AreEqual(ImapFetchDataItemMacro.All, req.FetchDataItem);
      Assert.AreNotSame(req.UsingSaslMechanisms, ImapWebRequestDefaults.UsingSaslMechanisms);
      Assert.AreEqual(new[] {"DIGEST-MD5", "CRAM-MD5"}, req.UsingSaslMechanisms);
      Assert.IsFalse(req.AllowInsecureLogin);
      Assert.AreNotSame(req.ExpectedErrorResponseCodes, ImapWebRequestDefaults.ExpectedErrorResponseCodes);
      Assert.AreEqual(new[] {ImapResponseCode.AlreadyExists, ImapResponseCode.NonExistent}, req.ExpectedErrorResponseCodes);

      ImapWebRequestDefaults.KeepAlive = false;
      ImapWebRequestDefaults.Timeout = 5000;
      ImapWebRequestDefaults.ReadWriteTimeout = 3000;
      ImapWebRequestDefaults.ReadOnly = true;
      ImapWebRequestDefaults.UseTlsIfAvailable = false;
      ImapWebRequestDefaults.Subscription = false;
      ImapWebRequestDefaults.AllowCreateMailbox = false;
      ImapWebRequestDefaults.FetchBlockSize = 10;
      ImapWebRequestDefaults.FetchPeek = false;
      ImapWebRequestDefaults.FetchDataItem = ImapFetchDataItemMacro.Fast;
      ImapWebRequestDefaults.UsingSaslMechanisms = null;
      ImapWebRequestDefaults.AllowInsecureLogin = true;
      ImapWebRequestDefaults.ExpectedErrorResponseCodes = null;

      req = WebRequest.Create("imap://localhost/") as ImapWebRequest;

      Assert.IsFalse(req.KeepAlive);
      Assert.AreEqual(5000, req.Timeout);
      Assert.AreEqual(3000, req.ReadWriteTimeout);
      Assert.IsTrue(req.ReadOnly);
      Assert.IsFalse(req.UseTlsIfAvailable);
      Assert.IsFalse(req.Subscription);
      Assert.IsFalse(req.AllowCreateMailbox);
      Assert.AreEqual(10, req.FetchBlockSize);
      Assert.IsFalse(req.FetchPeek);
      Assert.AreEqual(ImapFetchDataItemMacro.Fast, req.FetchDataItem);
      Assert.AreNotSame(req.UsingSaslMechanisms, ImapWebRequestDefaults.UsingSaslMechanisms);
      Assert.AreEqual(new string[0], req.UsingSaslMechanisms);
      Assert.IsTrue(req.AllowInsecureLogin);
      Assert.AreNotSame(req.ExpectedErrorResponseCodes, ImapWebRequestDefaults.ExpectedErrorResponseCodes);
      Assert.AreEqual(new ImapResponseCode[0], req.ExpectedErrorResponseCodes);
    }
  }
}
