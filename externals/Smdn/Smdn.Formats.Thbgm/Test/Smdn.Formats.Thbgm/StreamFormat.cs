using System;
using NUnit.Framework;

namespace Smdn.Formats.Thbgm {
  [TestFixture]
  public class StreamFormatTests {
    private class TestStreamFormat : StreamFormat {
      public override int BitsPerSample {
        get { return 16; }
      }

      public override int Channels {
        get { return 2; }
      }

      public override int SamplesPerSecond {
        get { return 44100; }
      }

      protected override string GetStreamFile(string thbgmPath, int trackNumber)
      {
        return thbgmPath;
      }
    }

    [Test]
    public void TestBlockAlign()
    {
      var format = new TestStreamFormat();

      Assert.AreEqual(4, format.BlockAlign);
    }

    [Test]
    public void TestBytesPerSecond()
    {
      var format = new TestStreamFormat();

      Assert.AreEqual(176400, format.BytesPerSecond);
    }

    [Test]
    public void TestAlignOffset()
    {
      var format = new TestStreamFormat();

      foreach (var pair in new[] {
        new {Expected = 0L, Offset = 0L},
        new {Expected = 0L, Offset = 1L},
        new {Expected = 0L, Offset = 2L},
        new {Expected = 0L, Offset = 3L},
        new {Expected = 4L, Offset = 4L},
        new {Expected = 4L, Offset = 5L},
        new {Expected = 4L, Offset = 6L},
        new {Expected = 4L, Offset = 7L},
        new {Expected = 8L, Offset = 8L},
      }) {
        Assert.AreEqual(pair.Expected, format.AlignOffset(pair.Offset), "offset={0}L", pair.Offset);
      }

      foreach (var pair in new[] {
        new {Expected = 0L, Offset = 0.0},
        new {Expected = 0L, Offset = 1.0},
        new {Expected = 0L, Offset = 2.0},
        new {Expected = 0L, Offset = 3.0},
        new {Expected = 0L, Offset = 3.75},
        new {Expected = 4L, Offset = 4.0},
        new {Expected = 4L, Offset = 4.25},
        new {Expected = 4L, Offset = 5.0},
        new {Expected = 4L, Offset = 6.0},
        new {Expected = 4L, Offset = 7.0},
        new {Expected = 4L, Offset = 7.75},
        new {Expected = 8L, Offset = 8.0},
        new {Expected = 8L, Offset = 8.25},
      }) {
        Assert.AreEqual(pair.Expected, format.AlignOffset(pair.Offset), "offset={0:F}", pair.Offset);
      }
    }

    [Test]
    public void TestToAlignedByteCount()
    {
      var format = new TestStreamFormat();

      foreach (var pair in new[] {
        new {Expected = 4408L, Length = TimeSpan.FromMilliseconds(25)},
        new {Expected = 44100L, Length = TimeSpan.FromMilliseconds(250)},
        new {Expected = 176400L, Length = TimeSpan.FromSeconds(1.0)},
      }) {
        Assert.AreEqual(pair.Expected, format.ToAlignedByteCount(pair.Length), "length={0}", pair.Length);
      }
    }

    [Test]
    public void TestToTimeSpan()
    {
      var format = new TestStreamFormat();

      foreach (var pair in new[] {
        new {Expected = TimeSpan.FromMilliseconds(25), Length = 4410L},
        new {Expected = TimeSpan.FromMilliseconds(250), Length = 44100L},
        new {Expected = TimeSpan.FromSeconds(1.0), Length = 176400L},
      }) {
        Assert.AreEqual(pair.Expected, format.ToTimeSpan(pair.Length), "length={0}", pair.Length);
      }
    }

    [Test]
    public void TestCreateGeneric()
    {
      var format = StreamFormat.Create("foo", 48000, 8, 1);

      Assert.AreEqual(48000, format.SamplesPerSecond);
      Assert.AreEqual(8, format.BitsPerSample);
      Assert.AreEqual(1, format.Channels);
      Assert.AreEqual(1, format.BlockAlign);
    }
  }
}