using System;
using System.Net;
using NUnit.Framework;

using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopWebRequestDefaultsTests {
    private static bool defaultKeepAlive = PopWebRequestDefaults.KeepAlive;
    private static int defaultTimeout = PopWebRequestDefaults.Timeout;
    private static int defaultReadWriteTimeout = PopWebRequestDefaults.ReadWriteTimeout;
    private static bool defaultDeleteAfterRetrieve = PopWebRequestDefaults.DeleteAfterRetrieve;
    private static bool defaultUseTlsIfAvailable = PopWebRequestDefaults.UseTlsIfAvailable;
    private static string[] defaultUsingSaslMechanisms = (string[])PopWebRequestDefaults.UsingSaslMechanisms.Clone();
    private static bool defaultAllowInsecureLogin = PopWebRequestDefaults.AllowInsecureLogin;
    private static PopResponseCode[] defaultExpectedErrorResponseCode = (PopResponseCode[])PopWebRequestDefaults.ExpectedErrorResponseCodes.Clone();

    [TearDown]
    public void TearDown()
    {
      PopWebRequestDefaults.KeepAlive = defaultKeepAlive;
      PopWebRequestDefaults.Timeout = defaultTimeout;
      PopWebRequestDefaults.ReadWriteTimeout = defaultReadWriteTimeout;
      PopWebRequestDefaults.DeleteAfterRetrieve = defaultDeleteAfterRetrieve;
      PopWebRequestDefaults.UseTlsIfAvailable = defaultUseTlsIfAvailable;
      PopWebRequestDefaults.UsingSaslMechanisms = defaultUsingSaslMechanisms;
      PopWebRequestDefaults.AllowInsecureLogin = defaultAllowInsecureLogin;
      PopWebRequestDefaults.ExpectedErrorResponseCodes = defaultExpectedErrorResponseCode;
    }

    [Test]
    public void TestDefaults()
    {
      Assert.IsTrue(defaultKeepAlive);
      Assert.AreEqual(-1, defaultTimeout);
      Assert.AreEqual(300000, defaultReadWriteTimeout);
      Assert.IsFalse(defaultDeleteAfterRetrieve);
      Assert.IsTrue(defaultUseTlsIfAvailable);
      Assert.AreEqual(new[] {"DIGEST-MD5", "CRAM-MD5", "NTLM"}, defaultUsingSaslMechanisms);
      Assert.IsFalse(defaultAllowInsecureLogin);
      Assert.AreEqual(new PopResponseCode[0], defaultExpectedErrorResponseCode);
    }

    [Test]
    public void TestGetSetTimeout()
    {
      PopWebRequestDefaults.Timeout = 1000;

      Assert.AreEqual(1000, PopWebRequestDefaults.Timeout);
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestSetTimeoutOutOfRange()
    {
      PopWebRequestDefaults.Timeout = -2;
    }

    [Test]
    public void TestGetSetReadWriteTimeout()
    {
      PopWebRequestDefaults.ReadWriteTimeout = 1000;

      Assert.AreEqual(1000, PopWebRequestDefaults.ReadWriteTimeout);
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestSetReadWriteTimeoutOutOfRange()
    {
      PopWebRequestDefaults.ReadWriteTimeout = -2;
    }

    [Test]
    public void TestGetSetUsingSaslMechanisms()
    {
      Assert.IsNotNull(PopWebRequestDefaults.UsingSaslMechanisms, "default non null");

      PopWebRequestDefaults.UsingSaslMechanisms = new[] {"NTLM", "X-AUTH"};

      Assert.AreEqual(new[] {"NTLM", "X-AUTH"}, PopWebRequestDefaults.UsingSaslMechanisms);

      PopWebRequestDefaults.UsingSaslMechanisms = null;

      Assert.IsNotNull(PopWebRequestDefaults.UsingSaslMechanisms);
      Assert.AreEqual(new string[0], PopWebRequestDefaults.UsingSaslMechanisms);
    }

    [Test]
    public void TestGetSetExpectedErrorResponseCodes()
    {
      Assert.IsNotNull(PopWebRequestDefaults.ExpectedErrorResponseCodes, "default non null");

      PopWebRequestDefaults.ExpectedErrorResponseCodes = new[] {PopResponseCode.InUse};

      Assert.AreEqual(new[] {PopResponseCode.InUse}, PopWebRequestDefaults.ExpectedErrorResponseCodes);

      PopWebRequestDefaults.ExpectedErrorResponseCodes = null;

      Assert.IsNotNull(PopWebRequestDefaults.ExpectedErrorResponseCodes);
      Assert.AreEqual(new PopResponseCode[0], PopWebRequestDefaults.ExpectedErrorResponseCodes);
    }

    [Test]
    public void TestCreateDefaultValueAppliedRequest()
    {
      PopWebRequestCreator.RegisterPrefix();

      PopWebRequest req;

      PopWebRequestDefaults.KeepAlive = true;
      PopWebRequestDefaults.Timeout = -1;
      PopWebRequestDefaults.ReadWriteTimeout = -1;
      PopWebRequestDefaults.DeleteAfterRetrieve = false;
      PopWebRequestDefaults.UseTlsIfAvailable = true;
      PopWebRequestDefaults.UsingSaslMechanisms = new[] {"DIGEST-MD5", "CRAM-MD5"};
      PopWebRequestDefaults.AllowInsecureLogin = false;
      PopWebRequestDefaults.ExpectedErrorResponseCodes = new[] {PopResponseCode.InUse, PopResponseCode.SysTemp};

      req = WebRequest.Create("pop://localhost/") as PopWebRequest;

      Assert.IsTrue(req.KeepAlive);
      Assert.AreEqual(-1, req.Timeout);
      Assert.AreEqual(-1, req.ReadWriteTimeout);
      Assert.IsFalse(req.DeleteAfterRetrieve);
      Assert.IsTrue(req.UseTlsIfAvailable);
      Assert.AreNotSame(req.UsingSaslMechanisms, PopWebRequestDefaults.UsingSaslMechanisms);
      Assert.AreEqual(new[] {"DIGEST-MD5", "CRAM-MD5"}, req.UsingSaslMechanisms);
      Assert.IsFalse(req.AllowInsecureLogin);
      Assert.AreNotSame(req.ExpectedErrorResponseCodes, PopWebRequestDefaults.ExpectedErrorResponseCodes);
      Assert.AreEqual(new[] {PopResponseCode.InUse, PopResponseCode.SysTemp}, req.ExpectedErrorResponseCodes);

      PopWebRequestDefaults.KeepAlive = false;
      PopWebRequestDefaults.Timeout = 5000;
      PopWebRequestDefaults.ReadWriteTimeout = 3000;
      PopWebRequestDefaults.DeleteAfterRetrieve = true;
      PopWebRequestDefaults.UseTlsIfAvailable = false;
      PopWebRequestDefaults.UsingSaslMechanisms = null;
      PopWebRequestDefaults.AllowInsecureLogin = true;
      PopWebRequestDefaults.ExpectedErrorResponseCodes = null;

      req = WebRequest.Create("pop://localhost/") as PopWebRequest;

      Assert.IsFalse(req.KeepAlive);
      Assert.AreEqual(5000, req.Timeout);
      Assert.AreEqual(3000, req.ReadWriteTimeout);
      Assert.IsTrue(req.DeleteAfterRetrieve);
      Assert.IsFalse(req.UseTlsIfAvailable);
      Assert.AreNotSame(req.UsingSaslMechanisms, PopWebRequestDefaults.UsingSaslMechanisms);
      Assert.AreEqual(new string[0], req.UsingSaslMechanisms);
      Assert.IsTrue(req.AllowInsecureLogin);
      Assert.AreNotSame(req.ExpectedErrorResponseCodes, PopWebRequestDefaults.ExpectedErrorResponseCodes);
      Assert.AreEqual(new PopResponseCode[0], req.ExpectedErrorResponseCodes);
    }
  }
}
