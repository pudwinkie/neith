using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class PopTextConverterTests {
    [Test]
    public void TestToCapability()
    {
      Assert.AreEqual(PopCapability.Top, PopTextConverter.ToCapability(new[] {
        new ByteString("TOP"),
      }));
      Assert.AreEqual(new PopCapability("LOGIN-DELAY", "900"), PopTextConverter.ToCapability(new[] {
        new ByteString("LOGIN-DELAY"),
        new ByteString("900")
      }));
    }

    [Test]
    public void TestToDropListing()
    {
      var dropListing = PopTextConverter.ToDropListing(new[] {
        new ByteString("2"),
        new ByteString("320"),
        new ByteString("undocumented-extension-value"),
      });

      Assert.AreEqual(2L, dropListing.MessageCount);
      Assert.AreEqual(320L, dropListing.SizeInOctets);

      try {
        PopTextConverter.ToDropListing(new[] {
          new ByteString("2"),
        });
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }
    }

    [Test]
    public void TestToScanListing()
    {
      var scanListing = PopTextConverter.ToScanListing(new[] {
        new ByteString("1"),
        new ByteString("120"),
        new ByteString("undocumented-extension-value"),
      });

      Assert.AreEqual(1L, scanListing.MessageNumber);
      Assert.AreEqual(120L, scanListing.SizeInOctets);

      try {
        PopTextConverter.ToDropListing(new[] {
          new ByteString("1"),
        });
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }
    }

    [Test]
    public void TestToUniqueIdListing()
    {
      var uniqueIdListing = PopTextConverter.ToUniqueIdListing(new[] {
        new ByteString("1"),
        new ByteString("whqtswO00WBw418f9t5JxYwZ"),
        new ByteString("undocumented-extension-value"),
      });

      Assert.AreEqual(1L, uniqueIdListing.MessageNumber);
      Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", uniqueIdListing.UniqueId);

      try {
        PopTextConverter.ToDropListing(new[] {
          new ByteString("1"),
        });
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }
    }

    [Test]
    public void TestToMessageNumber()
    {
      Assert.AreEqual(1L, PopTextConverter.ToMessageNumber(new ByteString("1")));
      Assert.AreEqual(1L, PopTextConverter.ToMessageNumber("1"));

      try {
        PopTextConverter.ToMessageNumber(new ByteString("0"));
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }

      try {
        PopTextConverter.ToMessageNumber("0");
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }
    }

    [Test]
    public void TestToNumber()
    {
      Assert.AreEqual(1L, PopTextConverter.ToNumber(new ByteString("1")));
      Assert.AreEqual(1234567890L, PopTextConverter.ToNumber(new ByteString("1234567890")));

      Assert.AreEqual(1L, PopTextConverter.ToNumber("1"));
      Assert.AreEqual(1234567890L, PopTextConverter.ToNumber("1234567890"));

      try {
        PopTextConverter.ToNumber(new ByteString("123a"));
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }

      try {
        PopTextConverter.ToNumber("123a");
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }
    }
  }
}
