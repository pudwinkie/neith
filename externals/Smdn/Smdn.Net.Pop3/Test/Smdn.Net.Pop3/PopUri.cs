using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopUriTests {
    [Test]
    public void TestRegisterParser()
    {
      Assert.IsFalse(UriParser.IsKnownScheme("pop"));
      Assert.IsFalse(UriParser.IsKnownScheme("pops"));

      Assert.AreEqual(-1, (new Uri("pop://localhost")).Port);
      Assert.AreEqual(-1, (new Uri("pops://localhost")).Port);

      PopUri.RegisterParser();

      Assert.IsTrue(UriParser.IsKnownScheme("pop"));
      Assert.IsTrue(UriParser.IsKnownScheme("pops"));

      Assert.AreEqual(110, (new Uri("pop://localhost")).Port);
      Assert.AreEqual(995, (new Uri("pops://localhost")).Port);
    }

    [Test]
    public void TestIsPop()
    {
      Assert.IsTrue(PopUri.IsPop(new Uri("pop://localhost/")));
      Assert.IsTrue(PopUri.IsPop(new Uri("pops://localhost/")));
      Assert.IsTrue(PopUri.IsPop(new Uri("POP://localhost/")));
      Assert.IsTrue(PopUri.IsPop(new Uri("POPS://localhost/")));
      Assert.IsFalse(PopUri.IsPop(new Uri("http://localhost")));
    }

    [Test]
    public void TestGetDefaultPortFromScheme()
    {
      Assert.AreEqual(110, PopUri.GetDefaultPortFromScheme("pop"));
      Assert.AreEqual(110, PopUri.GetDefaultPortFromScheme("POP"));
      Assert.AreEqual(995, PopUri.GetDefaultPortFromScheme("pops"));
      Assert.AreEqual(995, PopUri.GetDefaultPortFromScheme("POPS"));
      Assert.AreEqual(110, PopUri.GetDefaultPortFromScheme(new Uri("pop://localhost/")));
      Assert.AreEqual(995, PopUri.GetDefaultPortFromScheme(new Uri("pops://localhost/")));
      Assert.AreEqual(110, PopUri.GetDefaultPortFromScheme(new Uri("pop://localhost:10143/")));
      Assert.AreEqual(995, PopUri.GetDefaultPortFromScheme(new Uri("pops://localhost:10993/")));

      try {
        PopUri.GetDefaultPortFromScheme("http");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        PopUri.GetDefaultPortFromScheme(new Uri("http://localhost/"));
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }
  }
}
