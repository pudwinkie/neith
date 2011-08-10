using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopStyleUriParserTests {
    private void ExpectArgumentException(Action action)
    {
      try {
        action();
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    private void ExpectArgumentNullException(Action action)
    {
      try {
        action();
        Assert.Fail("ArgumentNullException not thrown");
      }
      catch (ArgumentNullException) {
      }
    }

    [Test]
    public void TestCheckUriScheme()
    {
      try {
        PopStyleUriParser.GetUser(new Uri("pop://localhost/"));
        PopStyleUriParser.GetUser(new Uri("pops://localhost/"));
        PopStyleUriParser.GetUser(new Uri("POP://localhost/"));
        PopStyleUriParser.GetUser(new Uri("POPS://localhost/"));
      }
      catch (ArgumentException) {
        Assert.Fail("ArgumentException thrown");
      }
    }

    [Test]
    public void TestGetAuthority()
    {
      Assert.AreEqual("localhost:110", PopStyleUriParser.GetAuthority(new Uri("pop://localhost")));
      Assert.AreEqual("localhost:995", PopStyleUriParser.GetAuthority(new Uri("pops://localhost")));
      Assert.AreEqual("localhost:10110", PopStyleUriParser.GetAuthority(new Uri("pop://localhost:10110")));
      Assert.AreEqual("localhost:10110", PopStyleUriParser.GetAuthority(new Uri("pops://localhost:10110")));
      Assert.AreEqual("user@localhost:10110", PopStyleUriParser.GetAuthority(new Uri("pop://user@localhost:10110")));
      Assert.AreEqual("localhost:10110", PopStyleUriParser.GetAuthority(new Uri("pop://;auth=*@localhost:10110")));
      Assert.AreEqual("user@localhost:10110", PopStyleUriParser.GetAuthority(new Uri("pop://user;auth=*@localhost:10110")));

      ExpectArgumentException(delegate {PopStyleUriParser.GetAuthority(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {PopStyleUriParser.GetAuthority(null);});
    }

    [Test]
    public void TestGetStrongAuthority()
    {
      Assert.AreEqual("localhost:110", PopStyleUriParser.GetStrongAuthority(new Uri("pop://localhost")));
      Assert.AreEqual("user@localhost:110", PopStyleUriParser.GetStrongAuthority(new Uri("pop://user@localhost")));
      Assert.AreEqual(";auth=*@localhost:110", PopStyleUriParser.GetStrongAuthority(new Uri("pop://;auth=*@localhost")));
      Assert.AreEqual("user;auth=*@localhost:110", PopStyleUriParser.GetStrongAuthority(new Uri("pop://user;auth=*@localhost")));

      ExpectArgumentException(delegate {PopStyleUriParser.GetStrongAuthority(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {PopStyleUriParser.GetStrongAuthority(null);});
    }

    [Test]
    public void TestGetUser()
    {
      Assert.AreEqual(string.Empty, PopStyleUriParser.GetUser(new Uri("pop://localhost:10110")));
      Assert.AreEqual("user", PopStyleUriParser.GetUser(new Uri("pop://user@localhost:10110")));
      Assert.AreEqual(string.Empty, PopStyleUriParser.GetUser(new Uri("pop://;auth=*@localhost:10110")));
      Assert.AreEqual("user", PopStyleUriParser.GetUser(new Uri("pop://user;auth=*@localhost:10110")));
      Assert.AreEqual("user;auth", PopStyleUriParser.GetUser(new Uri("pop://user%3bauth@localhost:10110")));
      Assert.AreEqual("user@host", PopStyleUriParser.GetUser(new Uri("pop://user%40host@localhost:10110")));

      Assert.AreEqual("user", PopStyleUriParser.GetUser(new Uri("pop://user:pass@localhost:10110")));
      Assert.AreEqual("user", PopStyleUriParser.GetUser(new Uri("pop://user;auth=*:pass@localhost:10110")));
      Assert.AreEqual("user", PopStyleUriParser.GetUser(new Uri("pop://user:pass;auth=*@localhost:10110")));
      Assert.AreEqual("user", PopStyleUriParser.GetUser(new Uri("pop://user;auth=*%3apass@localhost:10110")));
      Assert.AreEqual("user:pass", PopStyleUriParser.GetUser(new Uri("pop://user%3apass;auth=*@localhost:10110")));

      ExpectArgumentException(delegate {PopStyleUriParser.GetUser(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {PopStyleUriParser.GetUser(null);});
    }

    [Test]
    public void TestGetAuthType()
    {
      Assert.AreEqual(null, PopStyleUriParser.GetAuthType(new Uri("pop://localhost:10110")));
      Assert.AreEqual(null, PopStyleUriParser.GetAuthType(new Uri("pop://user@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.Apop, PopStyleUriParser.GetAuthType(new Uri("pop://;auth=+APOP@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.Apop, PopStyleUriParser.GetAuthType(new Uri("pop://;AUTH=+apop@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.SelectAppropriate, PopStyleUriParser.GetAuthType(new Uri("pop://;auth=*@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.SelectAppropriate, PopStyleUriParser.GetAuthType(new Uri("pop://;AUTH=*@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.SelectAppropriate, PopStyleUriParser.GetAuthType(new Uri("pop://user;auth=*@localhost:10110")));

      Assert.AreEqual(null, PopStyleUriParser.GetAuthType(new Uri("pop://%3bauth=*@localhost:10110")));
      Assert.AreEqual(new PopAuthenticationMechanism("%2a"), PopStyleUriParser.GetAuthType(new Uri("pop://;auth=%2a@localhost:10110")));
      Assert.AreEqual(new PopAuthenticationMechanism("%2bAPOP"), PopStyleUriParser.GetAuthType(new Uri("pop://;auth=%2bAPOP@localhost:10110")));

      Assert.AreEqual(null, PopStyleUriParser.GetAuthType(new Uri("pop://user:pass@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.SelectAppropriate, PopStyleUriParser.GetAuthType(new Uri("pop://user;auth=*:pass@localhost:10110")));
      Assert.AreEqual(null, PopStyleUriParser.GetAuthType(new Uri("pop://user:pass;auth=*@localhost:10110")));
      Assert.AreEqual(new PopAuthenticationMechanism("*:pass"), PopStyleUriParser.GetAuthType(new Uri("pop://user;auth=*%3apass@localhost:10110")));
      Assert.AreEqual(PopAuthenticationMechanism.SelectAppropriate, PopStyleUriParser.GetAuthType(new Uri("pop://user%3apass;auth=*@localhost:10110")));

      ExpectArgumentException(delegate {PopStyleUriParser.GetAuthType(new Uri("http://localhost/"));});
      ExpectArgumentNullException(delegate {PopStyleUriParser.GetAuthType(null);});
    }

    [Test()]
    public void TestPopUrlExamples1()
    {
      var uri = new Uri("pop://rg@mailsrv.qualcomm.com");

      Assert.AreEqual("rg", PopStyleUriParser.GetUser(uri));
      Assert.AreEqual("rg@mailsrv.qualcomm.com:110", PopStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("rg@mailsrv.qualcomm.com:110", PopStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(null, PopStyleUriParser.GetAuthType(uri));
    }

    [Test()]
    public void TestImapUrlExamples2()
    {
      var uri = new Uri("pop://rg;AUTH=+APOP@mail.eudora.com:8110");

      Assert.AreEqual("rg", PopStyleUriParser.GetUser(uri));
      Assert.AreEqual("rg@mail.eudora.com:8110", PopStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("rg;AUTH=+APOP@mail.eudora.com:8110", PopStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(PopAuthenticationMechanism.Apop, PopStyleUriParser.GetAuthType(uri));
    }

    [Test()]
    public void TestImapUrlExamples3()
    {
      var uri = new Uri("pop://baz;AUTH=SCRAM-MD5@foo.bar");

      Assert.AreEqual("baz", PopStyleUriParser.GetUser(uri));
      Assert.AreEqual("baz@foo.bar:110", PopStyleUriParser.GetAuthority(uri));
      Assert.AreEqual("baz;AUTH=SCRAM-MD5@foo.bar:110", PopStyleUriParser.GetStrongAuthority(uri));
      Assert.AreEqual(new PopAuthenticationMechanism("SCRAM-MD5"), PopStyleUriParser.GetAuthType(uri));
    }
  }
}
