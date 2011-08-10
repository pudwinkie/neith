using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3.Protocol {
  [TestFixture]
  public class PopTextConverterTests {
    [Test]
    public void TestToCapability()
    {
      Assert.AreEqual(PopCapability.Top, PopTextConverter.ToCapability(new[] {
        ByteString.CreateImmutable("TOP"),
      }));
      Assert.AreEqual(new PopCapability("LOGIN-DELAY", "900"), PopTextConverter.ToCapability(new[] {
        ByteString.CreateImmutable("LOGIN-DELAY"),
        ByteString.CreateImmutable("900")
      }));
    }

    [Test]
    public void TestToDropListing()
    {
      var dropListing = PopTextConverter.ToDropListing(new[] {
        ByteString.CreateImmutable("2"),
        ByteString.CreateImmutable("320"),
        ByteString.CreateImmutable("undocumented-extension-value"),
      });

      Assert.AreEqual(2L, dropListing.MessageCount);
      Assert.AreEqual(320L, dropListing.SizeInOctets);

      try {
        PopTextConverter.ToDropListing(new[] {
          ByteString.CreateImmutable("2"),
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
        ByteString.CreateImmutable("1"),
        ByteString.CreateImmutable("120"),
        ByteString.CreateImmutable("undocumented-extension-value"),
      });

      Assert.AreEqual(1L, scanListing.MessageNumber);
      Assert.AreEqual(120L, scanListing.SizeInOctets);

      try {
        PopTextConverter.ToDropListing(new[] {
          ByteString.CreateImmutable("1"),
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
        ByteString.CreateImmutable("1"),
        ByteString.CreateImmutable("whqtswO00WBw418f9t5JxYwZ"),
        ByteString.CreateImmutable("undocumented-extension-value"),
      });

      Assert.AreEqual(1L, uniqueIdListing.MessageNumber);
      Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", uniqueIdListing.UniqueId);

      try {
        PopTextConverter.ToDropListing(new[] {
          ByteString.CreateImmutable("1"),
        });
        Assert.Fail("PopMalformedTextException not thrown");
      }
      catch (PopMalformedTextException) {
      }
    }

    [Test]
    public void TestToMessageNumber()
    {
      Assert.AreEqual(1L, PopTextConverter.ToMessageNumber(ByteString.CreateImmutable("1")));
      Assert.AreEqual(1L, PopTextConverter.ToMessageNumber("1"));

      try {
        PopTextConverter.ToMessageNumber(ByteString.CreateImmutable("0"));
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
      Assert.AreEqual(1L, PopTextConverter.ToNumber(ByteString.CreateImmutable("1")));
      Assert.AreEqual(1234567890L, PopTextConverter.ToNumber(ByteString.CreateImmutable("1234567890")));

      Assert.AreEqual(1L, PopTextConverter.ToNumber("1"));
      Assert.AreEqual(1234567890L, PopTextConverter.ToNumber("1234567890"));

      try {
        PopTextConverter.ToNumber(ByteString.CreateImmutable("123a"));
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
