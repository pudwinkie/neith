using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Imap4.Client {
  [TestFixture]
  public class ImapClientProfileTests {
    [Test]
    public void TestClone()
    {
      var original = new ImapClientProfile(new Uri("imap://user@localhost/"));

      original.Timeout = 1;
      original.ReceiveTimeout = 2;
      original.SendTimeout = 3;
      original.UseTlsIfAvailable = false;
      original.UsingSaslMechanisms = new string[] {"X-SASL-EXT"};
      original.AllowInsecureLogin = true;

      var cloned = original.Clone();

      Assert.AreNotSame(cloned, original);
      Assert.AreNotSame(original.Authority, cloned.Authority);
      Assert.AreEqual(original.Authority, cloned.Authority);
      Assert.AreEqual(1, cloned.Timeout);
      Assert.AreEqual(2, cloned.ReceiveTimeout);
      Assert.AreEqual(3, cloned.SendTimeout);
      Assert.IsFalse(cloned.UseTlsIfAvailable);
      Assert.AreNotSame(original.UsingSaslMechanisms, cloned.UsingSaslMechanisms);
      CollectionAssert.AreEqual(original.UsingSaslMechanisms, cloned.UsingSaslMechanisms);
      Assert.IsTrue(cloned.AllowInsecureLogin);
    }

    [Test]
    public void TestCloneUsingSaslMechanismsNull()
    {
      var original = new ImapClientProfile(new Uri("imap://user@localhost/"));

      original.UsingSaslMechanisms = null;

      var cloned = original.Clone();

      Assert.AreNotSame(cloned, original);
      Assert.AreNotSame(original.Authority, cloned.Authority);
      Assert.AreEqual(original.Authority, cloned.Authority);
      Assert.IsNull(cloned.UsingSaslMechanisms);
    }
  }
}
