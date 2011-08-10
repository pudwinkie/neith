using System;
using System.Text;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopUriBuilderTests {
    [Test]
    public void TestConstructDefault()
    {
      var b = new PopUriBuilder();

      Assert.AreEqual(new Uri("pop://localhost/"), b.Uri);
    }

    [Test]
    public void TestConstructFromUri()
    {
      var uri = "POPS://user;auth=cram-md5@localhost:10110";
      var b = new PopUriBuilder(new Uri(uri));

      Assert.AreEqual(new Uri("pops://user;AUTH=CRAM-MD5@localhost:10110"), b.Uri);
      Assert.AreEqual("pops", b.Scheme);
      Assert.AreEqual("user", b.UserName);
      Assert.AreEqual(PopAuthenticationMechanism.CRAMMD5, b.AuthType);
      Assert.AreEqual("localhost", b.Host);
      Assert.AreEqual(10110, b.Port);
    }

    [Test]
    public void TestClone()
    {
      var origin = new PopUriBuilder("pops://user;auth=cram-md5@localhost:10110/");
      var cloned = origin.Clone();

      Assert.AreEqual(cloned, origin);
      Assert.AreEqual("pops", cloned.Scheme);
      Assert.AreEqual("user", cloned.UserName);
      Assert.AreEqual(PopAuthenticationMechanism.CRAMMD5, cloned.AuthType);
      Assert.AreEqual("localhost", cloned.Host);
      Assert.AreEqual(10110, cloned.Port);
    }

    [Test]
    public void TestBuildUri()
    {
      var b = new PopUriBuilder();

      b.Host = "pop.example.net";

      Assert.AreEqual(new Uri("pop://pop.example.net/"), b.Uri);

      b.Port = 10110;

      Assert.AreEqual(new Uri("pop://pop.example.net:10110/"), b.Uri);

      b.UserName = "user";

      Assert.AreEqual(new Uri("pop://user@pop.example.net:10110/"), b.Uri);

      b.AuthType = PopAuthenticationMechanism.CRAMMD5;

      Assert.AreEqual(new Uri("pop://user;AUTH=CRAM-MD5@pop.example.net:10110/"), b.Uri);

      b.Scheme = "pops";

      Assert.AreEqual(new Uri("pops://user;AUTH=CRAM-MD5@pop.example.net:10110/"), b.Uri);

      b.Scheme = "POP";

      Assert.AreEqual(new Uri("pop://user;AUTH=CRAM-MD5@pop.example.net:10110/"), b.Uri);

      b.UserName = null;

      Assert.AreEqual(new Uri("pop://;AUTH=CRAM-MD5@pop.example.net:10110/"), b.Uri);

      b.Port = 110;

      Assert.AreEqual(new Uri("pop://;AUTH=CRAM-MD5@pop.example.net:110/"), b.Uri);

      b.Port = 0;

      Assert.AreEqual(new Uri("pop://;AUTH=CRAM-MD5@pop.example.net:0/"), b.Uri);

      b.Port = -1;

      Assert.AreEqual(new Uri("pop://;AUTH=CRAM-MD5@pop.example.net/"), b.Uri);
    }

    [Test]
    public void TestBuildExample1()
    {
      var b = new PopUriBuilder();

      b.Scheme = "pop";
      b.Host = "mailsrv.qualcomm.com";
      b.UserName = "rg";

      Assert.AreEqual(new Uri("pop://rg@mailsrv.qualcomm.com"),
                      b.Uri);
    }

    [Test]
    public void TestBuildExample2()
    {
      var b = new PopUriBuilder();

      b.Scheme = "pop";
      b.Host = "mail.eudora.com";
      b.Port = 8110;
      b.UserName = "rg";
      b.AuthType = PopAuthenticationMechanism.Apop;

      Assert.AreEqual(new Uri("pop://rg;AUTH=+APOP@mail.eudora.com:8110"),
                      b.Uri);
    }

    [Test]
    public void TestBuildExample3()
    {
      var b = new PopUriBuilder();

      b.Scheme = "pop";
      b.Host = "foo.bar";
      b.UserName = "baz";
      b.AuthType = new PopAuthenticationMechanism("SCRAM-MD5");

      Assert.AreEqual(new Uri("pop://baz;AUTH=SCRAM-MD5@foo.bar"),
                      b.Uri);
    }
  }
}
