using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Thbgm {
  [TestFixture]
  public class BgmStreamTests {
    private const string testStreamFile = "test.thbgm.dat";

    private Smdn.IO.PartialStream GetInnerStreamAsPartialStream(BgmStream stream)
    {
      return stream.InnerStream as Smdn.IO.PartialStream;
    }

    private Smdn.IO.PersistentCachedStream GetInnerStreamAsMemoryStream(BgmStream stream)
    {
      return stream.InnerStream as Smdn.IO.PersistentCachedStream;
    }

    [SetUp]
    public void Setup()
    {
      TearDown();

      using (var stream = File.OpenWrite(testStreamFile)) {
        // data stream (total 16 + 32 + 32 = 80 bytes)
        var writer = new BinaryWriter(stream);

        // header (16bytes, 0x00 - 0x10)
        writer.Write(new byte[] {
          0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
          0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd, 0xcd,
        });

        // intro (32 bytes, 0x10 - 0x30)
        writer.Write(new byte[] {
          0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
          0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
          0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
          0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
        });

        // repeat (32 bytes, 0x30 - 0x50)
        writer.Write(new byte[] {
          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
          0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
          0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
          0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
        });

        writer.Flush();
      }
    }

    [TearDown]
    public void TearDown()
    {
      if (File.Exists(testStreamFile))
        File.Delete(testStreamFile);
    }

    [Test]
    public void TestConstruct()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = trackInfo.GetStream(testStreamFile, 2)) {
        Assert.IsInstanceOfType(typeof(Smdn.IO.PartialStream), stream.InnerStream);

        ConstructedStreamAssertion(stream);
      }
    }

    [Test]
    public void TestConstructLoadOnMemory()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = trackInfo.GetStream(testStreamFile, 2, true)) {
        Assert.IsInstanceOfType(typeof(Smdn.IO.PersistentCachedStream), stream.InnerStream);

        ConstructedStreamAssertion(stream);
      }
    }

    private void ConstructedStreamAssertion(BgmStream stream)
    {
      Assert.IsTrue(stream.CanRead);
      Assert.IsTrue(stream.CanSeek);
      Assert.IsFalse(stream.CanWrite);
      //Assert.IsFalse(stream.LeaveInnerStreamOpen);
      Assert.AreEqual(StreamFormat.ThXX, stream.Format);
      Assert.AreEqual(0, stream.RepeatedTimes);
      Assert.AreEqual(2, stream.TimesToRepeat);
      Assert.AreEqual(0x20 + 2 * 0x20, stream.Length);
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestConstructOffsetGreaterThanStreamLength()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 80, 4, 4);

      using (var stream = trackInfo.GetStream(testStreamFile, 0)) {
      }
    }

    [Test]
    public void TestConstructLengthGreaterThanStreamLength()
    {
      try {
        var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0, 40, 40);

        using (var stream = trackInfo.GetStream(testStreamFile, 0)) {
        }
      }
      catch (ArgumentException) {
        Assert.Fail("ArgumentException thrown");
      }

      try {
        var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0, 40, 41);

        using (var stream = trackInfo.GetStream(testStreamFile, 0)) {
          Assert.Fail("ArgumentException not thrown");
        }
      }
      catch (ArgumentException) {
      }
    }

    [Test]
    public void TestSeekToIntroStart()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = trackInfo.GetStream(testStreamFile, 2)) {
        stream.Position = 48;

        Assert.AreEqual(0L, stream.SeekToIntroStart());
        Assert.AreEqual(0L, stream.Position);
        Assert.AreEqual(0, stream.RepeatedTimes);
        Assert.AreEqual(16L, GetInnerStreamAsPartialStream(stream).InnerStream.Position);
      }
    }

    [Test]
    public void TestSeekToLoopStart()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = trackInfo.GetStream(testStreamFile, 2)) {
        stream.Position = 48;

        Assert.AreEqual(32L, stream.SeekToLoopStart(0));
        Assert.AreEqual(32L, stream.Position);
        Assert.AreEqual(0, stream.RepeatedTimes);
        Assert.AreEqual(16L + 32L, GetInnerStreamAsPartialStream(stream).InnerStream.Position);

        Assert.AreEqual(64L, stream.SeekToLoopStart(1));
        Assert.AreEqual(64L, stream.Position);
        Assert.AreEqual(1, stream.RepeatedTimes);
        Assert.AreEqual(16L + 32L, GetInnerStreamAsPartialStream(stream).InnerStream.Position);

        Assert.AreEqual(96L, stream.SeekToLoopStart(2));
        Assert.AreEqual(96L, stream.Position);
        Assert.AreEqual(2, stream.RepeatedTimes);
        Assert.AreEqual(16L + 32L, GetInnerStreamAsPartialStream(stream).InnerStream.Position);
      }
    }

    [Test]
    public void TestRead()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = trackInfo.GetStream(testStreamFile, 2)) {
        ReadAssertion(stream, false);
      }
    }

    [Test]
    public void TestReadLoadOnMemory()
    {
      var trackInfo = new TrackInfo(null, "test", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = trackInfo.GetStream(testStreamFile, 2, true)) {
        ReadAssertion(stream, true);
      }
    }

    private void ReadAssertion(BgmStream stream, bool loadedOnMemory)
    {
      var buffer = new byte[20];

      // offset
      Assert.AreEqual(0, stream.Position);
      //Assert.AreEqual(16, stream.InnerStream.Position);

      // intro (0 to 19 of 32 bytes)
      Assert.AreEqual(20, stream.Read(buffer, 0, 20));
      Assert.AreEqual(new byte[] {
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
        0x10, 0x11, 0x12, 0x13,
      }, buffer);
      Assert.AreEqual(0, stream.RepeatedTimes);
      Assert.AreEqual(20, stream.Position);

      if (loadedOnMemory)
        Assert.AreEqual(0  + 20, GetInnerStreamAsMemoryStream(stream).Position);
      else
        Assert.AreEqual(16 + 20, GetInnerStreamAsPartialStream(stream).InnerStream.Position);

      // intro (20 to 31 of 32 bytes) + repeat[0] (0 to 7 of 32 bytes)
      Assert.AreEqual(20, stream.Read(buffer, 0, 20));
      Assert.AreEqual(new byte[] {
        0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b,
        0x1c, 0x1d, 0x1e, 0x1f,
        0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
      }, buffer);
      Assert.AreEqual(0, stream.RepeatedTimes);
      Assert.AreEqual(40, stream.Position);

      if (loadedOnMemory)
        Assert.AreEqual(0  + 32 + 8, GetInnerStreamAsMemoryStream(stream).Position);
      else
        Assert.AreEqual(16 + 32 + 8, GetInnerStreamAsPartialStream(stream).InnerStream.Position);

      // repeat[0] (8 to 27 of 32 bytes)
      Assert.AreEqual(20, stream.Read(buffer, 0, 20));
      Assert.AreEqual(new byte[] {
        0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
        0x38, 0x39, 0x3a, 0x3b,
      }, buffer);
      Assert.AreEqual(0, stream.RepeatedTimes);
      Assert.AreEqual(60, stream.Position);

      if (loadedOnMemory)
        Assert.AreEqual(0  + 32 + 28, GetInnerStreamAsMemoryStream(stream).Position);
      else
        Assert.AreEqual(16 + 32 + 28, GetInnerStreamAsPartialStream(stream).InnerStream.Position);

      // repeat[0] (28 to 31 of 32 bytes) + repeat[1] (0 to 15 of 32 bytes)
      Assert.AreEqual(20, stream.Read(buffer, 0, 20));
      Assert.AreEqual(new byte[] {
        0x3c, 0x3d, 0x3e, 0x3f,
        0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
        0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
      }, buffer);
      Assert.AreEqual(1, stream.RepeatedTimes);
      Assert.AreEqual(80, stream.Position);

      if (loadedOnMemory)
        Assert.AreEqual(0  + 32 + 16, GetInnerStreamAsMemoryStream(stream).Position);
      else
        Assert.AreEqual(16 + 32 + 16, GetInnerStreamAsPartialStream(stream).InnerStream.Position);

      // repeat[1] (16 to 31 of 32 bytes)
      Assert.AreEqual(16, stream.Read(buffer, 0, 20));
      Assert.AreEqual(new byte[] {
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
        0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
      }, Smdn.ArrayExtensions.Slice(buffer, 0, 16));
      Assert.AreEqual(2, stream.RepeatedTimes);
      Assert.AreEqual(96, stream.Position);

      if (loadedOnMemory)
        Assert.AreEqual(0  + 32, GetInnerStreamAsMemoryStream(stream).Position);
      else
        Assert.AreEqual(16 + 32, GetInnerStreamAsPartialStream(stream).InnerStream.Position);
    }
  }
}
