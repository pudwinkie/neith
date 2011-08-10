using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

namespace Smdn.Formats.Ini {
  [TestFixture()]
  public class IniSectionTests {
    private IniDocument document = null;
    private IniSection defaultSection = null;
    private string testDocument = @"
keyString = valueString
keyInt    = 23
keyDouble = 3.14
";

    [SetUp]
    public void Setup()
    {
      document = IniDocument.Load(new StringReader(testDocument));

      defaultSection = document.DefaultSection;
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetNullEntry1()
    {
      defaultSection.Get(null);
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetNullEntry2()
    {
      defaultSection.Get<int>(null, (s) => int.Parse(s));
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetNullConverter()
    {
      defaultSection.Get<int>("entry", (Converter<string, int>)null);
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetThrowExceptionNullEntry()
    {
      defaultSection.GetThrowException(null, (s) => int.Parse(s));
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetThrowExceptionNullConverter()
    {
      defaultSection.GetThrowException<int>("entry", (Converter<string, int>)null);
    }

    [Test]
    public void TestGetAsString()
    {
      Assert.AreEqual("valueString", defaultSection.Get("keyString"));
      Assert.AreEqual("23", defaultSection.Get("keyInt"));
      Assert.AreEqual("3.14", defaultSection.Get("keyDouble"));
      Assert.AreEqual(null, defaultSection.Get("keyNotExist"));
    }

    [Test]
    public void TestGetAsStringWithDefault()
    {
      Assert.AreEqual("valueString", defaultSection.Get("keyString", "default"));
      Assert.AreEqual("23", defaultSection.Get("keyInt", "default"));
      Assert.AreEqual("3.14", defaultSection.Get("keyDouble", "default"));
      Assert.AreEqual("default", defaultSection.Get("keyNotExist", "default"));
    }

    [Test]
    public void TestGet()
    {
      Assert.AreEqual("valueString", defaultSection.Get("keyString", (s) => s));
      Assert.AreEqual(23, defaultSection.Get("keyInt", (s) => int.Parse(s)));
      Assert.AreEqual(3.14, defaultSection.Get("keyDouble", (s) => double.Parse(s)));

      Assert.AreEqual(default(double), defaultSection.Get("keyString", (s) => double.Parse(s)));
      Assert.AreEqual(default(bool), defaultSection.Get("keyInt", (s) => bool.Parse(s)));
      Assert.AreEqual(default(int), defaultSection.Get("keyDouble", (s) => int.Parse(s)));

      Assert.AreEqual(null, defaultSection.Get("keyNotExist", (s) => s));
      Assert.AreEqual(0, defaultSection.Get("keyNotExist", (s) => int.Parse(s)));
      Assert.AreEqual(0.0, defaultSection.Get("keyNotExist", (s) => double.Parse(s)));
    }

    [Test]
    public void TestGetWithDefault()
    {
      Assert.AreEqual("valueString", defaultSection.Get("keyString", "default", (s) => s));
      Assert.AreEqual(23, defaultSection.Get("keyInt", 60, (s) => int.Parse(s)));
      Assert.AreEqual(3.14, defaultSection.Get("keyDouble", 2.718, (s) => double.Parse(s)));

      Assert.AreEqual(2.718, defaultSection.Get("keyString", 2.718, (s) => double.Parse(s)));
      Assert.AreEqual(false, defaultSection.Get("keyInt", false, (s) => bool.Parse(s)));
      Assert.AreEqual(60, defaultSection.Get("keyDouble", 60, (s) => int.Parse(s)));

      Assert.AreEqual("default", defaultSection.Get("keyNotExist", "default", (s) => s));
      Assert.AreEqual(60, defaultSection.Get("keyNotExist", 60, (s) => int.Parse(s)));
      Assert.AreEqual(2.718, defaultSection.Get("keyNotExist", 2.718, (s) => double.Parse(s)));
    }

    [Test]
    public void TestGetThrowException()
    {
      Assert.AreEqual(3.14, defaultSection.GetThrowException("keyDouble", 2.718, (s) => double.Parse(s)));
      Assert.AreEqual(2.718, defaultSection.GetThrowException("keyNotExist", 2.718, (s) => double.Parse(s)));

      try {
        defaultSection.GetThrowException("keyString", 2.718, (s) => double.Parse(s));
        Assert.Fail("FormatException not thrown");
      }
      catch (FormatException) {
      }
    }

    [Test]
    public void TestSet()
    {
      var document = new IniDocument();
      var section = document.DefaultSection;

      Assert.AreEqual(0, section.Entries.Count);

      section["sec"] = "hoge";

      Assert.AreEqual(1, section.Entries.Count);
      Assert.AreEqual("hoge", section["sec"]);

      section["sec"] = "hoge";

      Assert.AreEqual(1, section.Entries.Count);
      Assert.AreEqual("hoge", section["sec"]);
    }
  }
}