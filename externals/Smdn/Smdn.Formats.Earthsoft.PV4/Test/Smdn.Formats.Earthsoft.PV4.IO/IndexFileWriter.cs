using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  [TestFixture]
  public class IndexFileWriterTests {
    [Test]
    public void TestWriteEntry()
    {
      using (var stream = new MemoryStream()) {
        var w = new IndexFileWriter(stream);

        var entry = new IndexFileEntry();

        entry.FrameOffset = 4 * 4096;
        entry.FrameSize = 15 * 4096;
        entry.PrecedentAudioSampleCount = 0;
        entry.AudioSampleCount = 801;
        entry.EncodingQuality = 255;
        entry.Reserved = 0;

        w.Write(entry);

        w.Close();

        CollectionAssert.AreEqual(new byte[] {
          0x00, 0x00, 0x00, 0x04, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x21, 0xff, 0x00,
        }, stream.ToArray());
      }
    }

    [Test]
    public void TestWriteEntries()
    {
      using (var stream = new MemoryStream()) {
        var w = new IndexFileWriter(stream);

        w.Write(new[] {
          new IndexFileEntry() {FrameOffset = 0x0004 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00000000, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x0013 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00000321, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x0022 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00000642, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x0031 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00000963, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x0040 * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00000c84, AudioSampleCount = 0x0320, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x004f * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00000fa4, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x005e * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x000012c5, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x006d * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x000015e6, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x007c * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00001907, AudioSampleCount = 0x0321, EncodingQuality = 0xff},
          new IndexFileEntry() {FrameOffset = 0x008b * 4096, FrameSize = 0x000f * 4096, PrecedentAudioSampleCount = 0x00001c28, AudioSampleCount = 0x0320, EncodingQuality = 0xff},
        });

        w.Close();

        CollectionAssert.AreEqual(File.ReadAllBytes("test-1280x720p-10f.dvi"), stream.ToArray());
      }
    }
  }
}

