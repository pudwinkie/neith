using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net {
  [TestFixture]
  public class ICredentialsByHostExtensionsTests {
    [Test]
    public void TestLookupCredentialWithCredentialCache()
    {
      var credentials = new CredentialCache();
      var credPopuser1 = new NetworkCredential("popuser1", "password");
      var credPopuser2 = new NetworkCredential("popuser2", "password");
      var credImapuser1 = new NetworkCredential("imapuser1", "password");
      var credImapuser2 = new NetworkCredential("imapuser2", "password");

      credentials.Add("localhost", 110, string.Empty, credPopuser1);
      credentials.Add("localhost", 110, "cram-md5", credPopuser2);
      credentials.Add("localhost", 143, string.Empty, credImapuser1);
      credentials.Add("localhost", 143, "digest-md5", credImapuser2);

      Assert.AreEqual(credPopuser1, credentials.LookupCredential("localhost", 110, "popuser1", null));
      Assert.AreEqual(credPopuser2, credentials.LookupCredential("localhost", 110, "popuser2", "CRAM-MD5"));
      Assert.AreEqual(credImapuser1, credentials.LookupCredential("localhost", 143, "imapuser1", null));
      Assert.AreEqual(credImapuser2, credentials.LookupCredential("localhost", 143, "imapuser2", "DIGEST-MD5"));

      Assert.AreEqual(credPopuser1, credentials.LookupCredential("localhost", 110, null, null));
      Assert.AreEqual(credPopuser2, credentials.LookupCredential("localhost", 110, null, "CRAM-MD5"));
      Assert.IsNull(credentials.LookupCredential("localhost", 110, null, "DIGEST-MD5"));

      Assert.AreEqual(credImapuser1, credentials.LookupCredential("localhost", 143, null, null));
      Assert.AreEqual(credImapuser2, credentials.LookupCredential("localhost", 143, null, "DIGEST-MD5"));
      Assert.IsNull(credentials.LookupCredential("localhost", 143, null, "CRAM-MD5"));
    }

    [Test]
    public void TestLookupCredentialNetworkCredential()
    {
      var cred = new NetworkCredential("user", "pass");

      Assert.AreEqual(cred, cred.LookupCredential("localhost", 110, null, null));
      Assert.AreEqual(cred, cred.LookupCredential("localhost", 143, null, null));
      Assert.AreEqual(cred, cred.LookupCredential("localhost", 143, "user", null));
      Assert.AreEqual(cred, cred.LookupCredential("localhost", 110, null, "+apop"));
      Assert.AreEqual(cred, cred.LookupCredential("localhost", 143, "user", "digest-md5"));
    }
  }
}
