using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Security.Authentication.Sasl.Client {
  [TestFixture]
  public class SaslClientMechanismTests {
    [Test]
    public void TestGetAvailableMechanisms()
    {
      var mechanisms = SaslClientMechanism.GetAvailableMechanisms();

      Assert.IsNotNull(mechanisms);
      Assert.Greater(mechanisms.Length, 0);
      CollectionAssert.Contains(mechanisms, "LOGIN");
      CollectionAssert.Contains(mechanisms, "PLAIN");
      CollectionAssert.Contains(mechanisms, "ANONYMOUS");
      CollectionAssert.Contains(mechanisms, "DIGEST-MD5");
      CollectionAssert.Contains(mechanisms, "CRAM-MD5");
    }

    [Test]
    public void TestCreate()
    {
      var mechanism = SaslClientMechanism.Create("PLAIN");

      Assert.IsNotNull(mechanism);
      Assert.IsInstanceOfType(typeof(PlainMechanism), mechanism);

      Assert.IsTrue(mechanism.IsPlainText);
      Assert.AreEqual(SaslExchangeStatus.None, mechanism.ExchangeStatus);
    }

    [Test, ExpectedException(typeof(SaslMechanismNotSupportedException))]
    public void TestCreateNotSupported()
    {
      SaslClientMechanism.Create("X-UNKNOWN");
    }

    [SaslMechanism("X-THROWS-EXCEPTION", true)]
    private class XThrowsExceptionMechanism : SaslClientMechanism {
      public XThrowsExceptionMechanism()
      {
        throw new InvalidOperationException();
      }

      protected override SaslExchangeStatus Exchange (ByteString serverChallenge, out ByteString clientResponse)
      {
        throw new NotImplementedException();
      }
    }

    [Test]
    public void TestCreateNotThrowTargetInvocationException()
    {
      SaslClientMechanism.RegisterMechanism(typeof(XThrowsExceptionMechanism));

      try {
        SaslClientMechanism.Create("X-THROWS-EXCEPTION");
        Assert.Fail("NotImplementedException not thrown");
      }
      catch (SaslException ex) {
        Assert.IsNotNull(ex.InnerException);
        Assert.IsInstanceOfType(typeof(InvalidOperationException), ex.InnerException);
      }
    }

    [Test]
    public void TestCreateNtlm()
    {
      try {
        var mechanism = SaslClientMechanism.Create("NTLM");

        Assert.IsNotNull(mechanism);
        Assert.IsInstanceOfType(typeof(NTLMMechanism), mechanism);

        Assert.IsFalse(mechanism.IsPlainText);
        Assert.AreEqual(SaslExchangeStatus.None, mechanism.ExchangeStatus);
      }
      catch (SaslMechanismNotSupportedException) {
        Assert.Ignore("NTLM not supported");
      }
      catch (SaslException) {
        Assert.Ignore("NTLM unavailable");
      }
    }

    [Test, ExpectedException(typeof(ObjectDisposedException))]
    public void TestGetCredentialAfterDisposed()
    {
      var mechanism = SaslClientMechanism.Create("PLAIN");

      mechanism.Credential = new NetworkCredential("test", "test");

      mechanism.Dispose();

      Assert.IsNull(mechanism.Credential);
    }

    [Test, ExpectedException(typeof(ObjectDisposedException))]
    public void TestGetExchangeStatusAfterDisposed()
    {
      var mechanism = SaslClientMechanism.Create("PLAIN");

      mechanism.Credential = new NetworkCredential("test", "test");

      mechanism.Dispose();

      Assert.AreEqual(SaslExchangeStatus.None, mechanism.ExchangeStatus);
    }

    [Test, ExpectedException(typeof(ObjectDisposedException))]
    public void TestExchangeAfterDisposed()
    {
      var mechanism = SaslClientMechanism.Create("PLAIN");

      mechanism.Credential = new NetworkCredential("test", "test");

      mechanism.Dispose();

      byte[] clientResponse;

      mechanism.Exchange(null, out clientResponse);
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestRegisterNull()
    {
      SaslClientMechanism.RegisterMechanism(null);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRegisterUnassignableType()
    {
      SaslClientMechanism.RegisterMechanism(typeof(int));
    }

    private abstract class AbstractCustomMechanism : SaslClientMechanism {
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRegisterAbstractClass()
    {
      SaslClientMechanism.RegisterMechanism(typeof(AbstractCustomMechanism));
    }

    private class UnnamedCustomMechanism : SaslClientMechanism {
      protected override SaslExchangeStatus Exchange (ByteString serverChallenge, out ByteString clientResponse)
      {
        throw new NotImplementedException();
      }
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestRegisterNameAttributeNotApplied()
    {
      SaslClientMechanism.RegisterMechanism(typeof(UnnamedCustomMechanism));
    }

    [SaslMechanism("X-CUSTOM", false)]
    private class CustomMechanism : SaslClientMechanism {
      public CustomMechanism()
      {
      }

      protected override SaslExchangeStatus Exchange (ByteString serverChallenge, out ByteString clientResponse)
      {
        throw new NotImplementedException();
      }
    }

    [Test]
    public void TestRegister()
    {
      SaslClientMechanism.RegisterMechanism(typeof(CustomMechanism));

      Assert.Contains("X-CUSTOM", SaslClientMechanism.GetAvailableMechanisms());

      var inst = SaslClientMechanism.Create("X-CUSTOM");

      Assert.IsNotNull(inst);
      Assert.IsInstanceOfType(typeof(CustomMechanism), inst);
    }

    [SaslMechanism("LOGIN", true)]
    private class XLoginMechanism : SaslClientMechanism {
      public XLoginMechanism()
      {
      }

      protected override SaslExchangeStatus Exchange (ByteString serverChallenge, out ByteString clientResponse)
      {
        throw new NotImplementedException();
      }
    }

    [Test]
    public void TestRegisterReplace()
    {
      SaslClientMechanism inst;

      Assert.Contains("LOGIN", SaslClientMechanism.GetAvailableMechanisms());

      inst = SaslClientMechanism.Create("LOGIN");

      Assert.IsNotNull(inst);
      Assert.IsNotInstanceOfType(typeof(XLoginMechanism), inst);

      SaslClientMechanism.RegisterMechanism(typeof(XLoginMechanism));

      Assert.Contains("LOGIN", SaslClientMechanism.GetAvailableMechanisms());

      inst = SaslClientMechanism.Create("LOGIN");

      Assert.IsNotNull(inst);
      Assert.IsInstanceOfType(typeof(XLoginMechanism), inst);
    }
  }
}
