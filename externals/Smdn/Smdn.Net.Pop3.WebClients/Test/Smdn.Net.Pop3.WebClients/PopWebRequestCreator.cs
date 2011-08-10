using System;
using System.Net;
using NUnit.Framework;

namespace Smdn.Net.Pop3.WebClients {
  [TestFixture]
  public class PopWebRequestCreatorTests {
    [SetUp]
    public void Setup()
    {
      PopWebRequestCreator.RegisterPrefix();
    }

    [Test]
    public void TestRegisterPrefix()
    {
      var popRequest = WebRequest.Create("pop://localhost/");

      Assert.IsInstanceOfType(typeof(PopWebRequest), popRequest);

      var popsRequest = WebRequest.Create("pops://localhost/");

      Assert.IsInstanceOfType(typeof(PopWebRequest), popsRequest);
    }

    [Test]
    public void TestIWebRequestCreate()
    {
      var popMailboxRequest = WebRequest.Create("pop://localhost/");

      Assert.IsInstanceOfType(typeof(PopWebRequest), popMailboxRequest);
      StringAssert.Contains("PopMailboxWebRequest", popMailboxRequest.GetType().FullName);

      var popMessageRequest = WebRequest.Create("pop://localhost/;MSG=1");

      Assert.IsInstanceOfType(typeof(PopWebRequest), popMessageRequest);
      StringAssert.Contains("PopMessageWebRequest", popMessageRequest.GetType().FullName);
    }
  }
}