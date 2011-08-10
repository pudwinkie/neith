using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  [TestFixture]
  public class IndexFileReaderTests {
    [Test]
    public void TestReadEntry()
    {
      using (var reader = new IndexFileReader("test-1280x720p-10f.dvi")) {
        int frameNumber = 0;

        foreach (var expected in new[] {
          new {FrameOffset = 0x00000004 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000000000, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x00000013 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000000321, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x00000022 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000000642, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x00000031 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000000963, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x00000040 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000000c84, AudioSampleCount = (ushort)0x0320, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x0000004f * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000000fa4, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x0000005e * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x000000000012c5, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x0000006d * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x000000000015e6, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x0000007c * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000001907, AudioSampleCount = (ushort)0x0321, EncodingQuality = (byte)0xff},
          new {FrameOffset = 0x0000008b * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = (UInt48)0x00000000001c28, AudioSampleCount = (ushort)0x0320, EncodingQuality = (byte)0xff},
        }) {
          var entry = reader.ReadEntry();

          Assert.IsNotNull(entry, "frame {0}", frameNumber);
          Assert.AreEqual(expected.FrameOffset, entry.FrameOffset, "frame {0} FrameOffset", frameNumber);
          Assert.AreEqual(expected.FrameSize, entry.FrameSize, "frame {0} FrameSize", frameNumber);
          Assert.AreEqual(expected.PrecedentAudioSampleCount, entry.PrecedentAudioSampleCount, "frame {0} PrecedentAudioSampleCount", frameNumber);
          Assert.AreEqual(expected.AudioSampleCount, entry.AudioSampleCount, "frame {0} AudioSampleCount", frameNumber);
          Assert.AreEqual(expected.EncodingQuality, entry.EncodingQuality, "frame {0} EncodingQuality", frameNumber);

          frameNumber++;

          Assert.AreEqual(IndexFileEntry.Size * frameNumber, reader.BaseStream.Position, "frame {0} BaseStream.Position", frameNumber);
        }

        Assert.IsTrue(IndexFileEntry.Empty == reader.ReadEntry());
      }
    }

    [Test]
    public void TestReadAllEntry()
    {
      using (var reader = new IndexFileReader("test-1280x720p-10f.dvi")) {
        reader.BaseStream.Position = 8L;

        var entries = reader.ReadAllEntry();

        Assert.IsNotNull(entries);

        var count = 0;

        foreach (var e in entries) {
          Assert.IsNotNull(e, "frame {0}", count);
          Assert.AreEqual(0xff, e.EncodingQuality, "frame {0} EncodingQuality", count);
          count++;
        }

        Assert.AreEqual(10, count);
      }
    }
  }
}

