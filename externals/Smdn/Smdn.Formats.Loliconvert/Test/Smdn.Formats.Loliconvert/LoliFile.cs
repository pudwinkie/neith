using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Loliconvert {
  [TestFixture]
  public class LoliFileTests {
    [Test]
    public void TestIsLolizedFile()
    {
      Assert.IsFalse(LoliFile.IsLolizedFile("test.png"));
      Assert.IsTrue(LoliFile.IsLolizedFile("test.png.txt"));
      Assert.IsFalse(LoliFile.IsLolizedFile("test.txt"));
      Assert.IsTrue(LoliFile.IsLolizedFile("test.txt.txt"));
    }

    [Test]
    public void TestGetEncodedPathOf()
    {
      Assert.AreEqual("test.txt", LoliFile.GetEncodedPathOf("test"));
      Assert.AreEqual("test.txt.txt", LoliFile.GetEncodedPathOf("test.txt"));
      Assert.AreEqual(@"C:\test.txt.txt", LoliFile.GetEncodedPathOf(@"C:\test.txt"));
      Assert.AreEqual(@"/tmp/test.txt.txt", LoliFile.GetEncodedPathOf(@"/tmp/test.txt"));

      if (Environment.OSVersion.Platform != PlatformID.Unix) {
        try {
          LoliFile.GetEncodedPathOf(@"C:\");
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }
      }

      try {
        LoliFile.GetEncodedPathOf(@"/tmp/");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestGetDecodedPathOf()
    {
      Console.WriteLine(System.IO.Path.DirectorySeparatorChar);
      Console.WriteLine(System.IO.Path.AltDirectorySeparatorChar);

      Assert.AreEqual("test", LoliFile.GetDecodedPathOf("testfile"));
      Assert.AreEqual("test", LoliFile.GetDecodedPathOf("test.txt"));
      Assert.AreEqual("test.txt", LoliFile.GetDecodedPathOf("test.txt.txt"));
      Assert.AreEqual(@"C:\test.txt", LoliFile.GetDecodedPathOf(@"C:\test.txt.txt"));
      Assert.AreEqual(@"/tmp/test.txt", LoliFile.GetDecodedPathOf(@"/tmp/test.txt.txt"));

      if (Environment.OSVersion.Platform != PlatformID.Unix) {
        try {
          LoliFile.GetDecodedPathOf(@"C:\");
          Assert.Fail("ArgumentException not thrown");
        }
        catch (ArgumentException) {
        }
      }

      try {
        LoliFile.GetDecodedPathOf(@"/tmp/");
        Assert.Fail("ArgumentException not thrown");
      }
      catch (ArgumentException) {
      }

      try {
        LoliFile.GetDecodedPathOf("test");
        Assert.Fail("LolisticException not thrown");
      }
      catch (LolisticException) {
      }
    }

    [Test]
    public void TestOpenRead()
    {
      using (var expected = File.OpenRead("test.txt"))
      using (var actual = LoliFile.OpenRead("test.txt.txt")) {
        Assert.AreEqual(Smdn.IO.StreamExtensions.ReadToEnd(expected),
                        Smdn.IO.StreamExtensions.ReadToEnd(actual),
                        "content test.txt");
      }

      using (var expected = File.OpenRead("test.png"))
      using (var actual = LoliFile.OpenRead("test.png.txt")) {
        Assert.AreEqual(Smdn.IO.StreamExtensions.ReadToEnd(expected),
                        Smdn.IO.StreamExtensions.ReadToEnd(actual),
                        "content test.png");
      }

      try {
        LoliFile.OpenRead("test.png");
        Assert.Fail("LolisticException not thrown");
      }
      catch (LolisticException) {
      }

      try {
        LoliFile.OpenRead("test.txt");
        Assert.Fail("LolisticException not thrown");
      }
      catch (LolisticException) {
      }
    }

    [Test]
    public void TestOpenWrite()
    {
    }

    [Test]
    public void TestConvertDecode()
    {
      var file = "converted.png.txt";

      try {
        File.Copy("test.png.txt", file, true);

        string converted = null;

        try {
          converted = LoliFile.Convert(file);

          Assert.AreEqual(LoliFile.GetDecodedPathOf(file), Path.GetFileName(converted));
          FileAssert.AreEqual("test.png", converted);
        }
        finally {
          if (converted != null && File.Exists(converted))
            File.Delete(converted);
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test]
    public void TestConvertEncode()
    {
      var file = "converted.png";

      try {
        File.Copy("test.png", file, true);

        string converted = null;

        try {
          converted = LoliFile.Convert(file);

          Assert.AreEqual(LoliFile.GetEncodedPathOf(file), Path.GetFileName(converted));
          FileAssert.AreEqual("test.png.txt", converted);
        }
        finally {
          if (converted != null && File.Exists(converted))
            File.Delete(converted);
        }
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test]
    public void TestEncode()
    {
    }

    [Test]
    public void TestDecode()
    {
    }
  }
}
