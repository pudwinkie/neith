using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  [TestFixture]
  public class StreamFileWriterTests {
    [Test]
    public void TestSeekToHeader()
    {
      using (var writer = new StreamFileWriter(new MemoryStream())) {
        writer.BaseStream.Position = 8L;

        Assert.AreEqual(0L, writer.SeekToHeader());

        Assert.AreEqual(0L, writer.BaseStream.Position);

        writer.Write(new StreamFileHeaderData());

        Assert.AreNotEqual(0L, writer.BaseStream.Position);

        Assert.AreEqual(0L, writer.SeekToHeader());

        Assert.AreEqual(0L, writer.BaseStream.Position);
      }
    }

    [Test]
    public void TestSeekToFirstFrame()
    {
      using (var writer = new StreamFileWriter(new MemoryStream())) {
        writer.Write(new StreamFileHeaderData());

        var expectedPosition = writer.BaseStream.Position;

        Assert.AreEqual(expectedPosition, writer.SeekToFirstFrame());

#if false
        var frameData = new StreamFileFrameData();

        frameData.AudioSampleCount = 801;
        frameData.VideoSize = new uint[] {4096, 4096, 4096, 4096};

        writer.Write(frameData);

        Assert.AreNotEqual(0L, writer.BaseStream.Position);

        Assert.AreEqual(expectedPosition, writer.SeekToFirstFrame());
#endif
      }
    }

    [Test]
    public void TestSeekToFrame()
    {
      using (var indexReader = new IndexFileReader("test-1280x720p-10f.dvi")) {
        var entries = indexReader.ReadAllEntry();

        using (var streamWriter = new StreamFileWriter(new MemoryStream())) {
          foreach (var entry in entries) {
            streamWriter.BaseStream.Position = 8L;

            Assert.AreEqual(entry.FrameOffset, streamWriter.SeekToFrame(entry));
            Assert.AreEqual(entry.FrameOffset, streamWriter.BaseStream.Position);
          }
        }
      }
    }

    [Test]
    public void TestWriteStreamFileHeaderData()
    {
      var header = new StreamFileHeaderData();

      header.CodecVersion = 2;
      header.HorizontalPixels = 1280 / 16;
      header.VerticalPixels = 720 / 8;
      header.FrameScanning = FrameScanning.Progressive;

      var luminanceQuantizerTable = new ushort[] {
        0x0000, 0x0010, 0x0011, 0x0012, 0x0012, 0x0013, 0x002a, 0x002c,
        0x0010, 0x0011, 0x0012, 0x0012, 0x0013, 0x0026, 0x002b, 0x002d,
        0x0011, 0x0012, 0x0013, 0x0013, 0x0028, 0x0029, 0x002d, 0x0030,
        0x0012, 0x0012, 0x0013, 0x0028, 0x0029, 0x002a, 0x002e, 0x0031,
        0x0012, 0x0013, 0x0028, 0x0029, 0x002a, 0x002b, 0x0030, 0x0065,
        0x0013, 0x0026, 0x0029, 0x002a, 0x002b, 0x002c, 0x0062, 0x0068,
        0x002a, 0x002b, 0x002d, 0x002e, 0x0030, 0x0062, 0x006d, 0x0074,
        0x002c, 0x002d, 0x0030, 0x0031, 0x0065, 0x0068, 0x0074, 0x007b,
      };

      for (var i = 0; i < luminanceQuantizerTable.Length; i++) {
        header.LuminanceQuantizerTable[i] = luminanceQuantizerTable[i];
      }

      var chrominanceQuantizerTable = new ushort[] {
        0x0000, 0x0010, 0x0011, 0x0019, 0x001a, 0x001a, 0x002a, 0x002c,
        0x0010, 0x0011, 0x0019, 0x0019, 0x001a, 0x0026, 0x002b, 0x005b,
        0x0011, 0x0019, 0x001a, 0x001b, 0x0028, 0x0029, 0x005b, 0x0060,
        0x0019, 0x0019, 0x001b, 0x0028, 0x0029, 0x0054, 0x005d, 0x00c5,
        0x001a, 0x001a, 0x0028, 0x0029, 0x0054, 0x0056, 0x00bf, 0x00cb,
        0x001a, 0x0026, 0x0029, 0x0054, 0x0056, 0x00b1, 0x00c5, 0x00d1,
        0x002a, 0x002b, 0x005b, 0x005d, 0x00bf, 0x00c5, 0x00db, 0x00e8,
        0x002c, 0x005b, 0x0060, 0x00c5, 0x00cb, 0x00d1, 0x00e8, 0x00f6,
      };

      for (var i = 0; i < chrominanceQuantizerTable.Length; i++) {
        header.ChrominanceQuantizerTable[i] = chrominanceQuantizerTable[i];
      }

      using (var stream = new MemoryStream()) {
        const long offset = 8L;

        var writer = new StreamFileWriter(stream);

        writer.BaseStream.Position = offset;

        writer.Write(header);

        Assert.AreEqual(offset + StreamFileHeaderData.Size, writer.BaseStream.Position);

        writer.Close();

        using (var expectedDataStream = File.OpenRead("test-1280x720p-10f.dv")) {
          CollectionAssert.AreEqual((new Smdn.IO.BinaryReader(expectedDataStream)).ReadExactBytes(StreamFileHeaderData.Size),
                                    stream.ToArray().Slice((int)offset));
        }
      }
    }

    [Test]
    public void TestWriteStreamFileFrameData()
    {
      const string testFile = "test-out.dv";

      try {
        if (File.Exists(testFile))
          File.Delete(testFile);

        using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
          using (var stream = File.OpenWrite(testFile)) {
            stream.SetLength(0L);

            var writer = new StreamFileWriter(stream);

            writer.Write(reader.ReadHeader());

            Assert.AreEqual(reader.BaseStream.Position,
                            writer.BaseStream.Position);

            for (var frameNumber = 0;; frameNumber++) {
              var frame = reader.ReadFrameData();

              if (frame == null)
                break;

              writer.Write(frame);

              if (reader.EndOfStream)
                Assert.GreaterOrEqual(writer.BaseStream.Position,
                                      reader.BaseStream.Position, "frame {0}", frameNumber);
              else
                Assert.AreEqual(reader.BaseStream.Position,
                                writer.BaseStream.Position, "frame {0}", frameNumber);
            }
          }
        }

#if false
        FileAssert.AreEqual("test-1280x720p-10f.dv", testFile);
#else
        using (var expectedDataStream = File.OpenRead("test-1280x720p-10f.dv")) {
          var alignment = new byte[4096 - expectedDataStream.Length % 4096];
          var expectedAlignedDataStream = new Smdn.IO.ExtendStream(expectedDataStream, new byte[0], alignment, true);

          using (var actualDataStream = File.OpenRead(testFile)) {
            FileAssert.AreEqual(expectedAlignedDataStream, actualDataStream);
          }
        }
#endif
      }
      finally {
        if (File.Exists(testFile))
          File.Delete(testFile);
      }
    }
  }
}
