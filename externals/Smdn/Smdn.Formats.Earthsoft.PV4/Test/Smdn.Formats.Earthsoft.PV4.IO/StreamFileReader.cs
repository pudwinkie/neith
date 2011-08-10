using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  [TestFixture]
  public class StreamFileReaderTests {
    [Test]
    public void TestSeekToHeader()
    {
      using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
        reader.BaseStream.Position = 8L;

        Assert.AreEqual(0L, reader.SeekToHeader());

        Assert.AreEqual(0L, reader.BaseStream.Position);

        reader.ReadHeader();

        Assert.AreNotEqual(0L, reader.BaseStream.Position);

        reader.ReadFrameData(false, false);

        Assert.AreNotEqual(0L, reader.BaseStream.Position);

        Assert.AreEqual(0L, reader.SeekToHeader());

        Assert.AreEqual(0L, reader.BaseStream.Position);
      }
    }

    [Test]
    public void TestSeekToFirstFrame()
    {
      using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
        reader.ReadHeader();

        var expectedPosition = reader.BaseStream.Position;

        Assert.AreEqual(expectedPosition, reader.SeekToFirstFrame());

        reader.ReadFrameData(false, false);

        Assert.AreNotEqual(0L, reader.BaseStream.Position);

        Assert.AreEqual(expectedPosition, reader.SeekToFirstFrame());
      }
    }

    [Test]
    public void TestSeekToFrame()
    {
      using (var indexReader = new IndexFileReader("test-1280x720p-10f.dvi")) {
        var entries = indexReader.ReadAllEntry();

        using (var streamReader = new StreamFileReader("test-1280x720p-10f.dv")) {
          foreach (var entry in entries) {
            streamReader.BaseStream.Position = 8L;

            Assert.AreEqual(entry.FrameOffset, streamReader.SeekToFrame(entry));
            Assert.AreEqual(entry.FrameOffset, streamReader.BaseStream.Position);
          }
        }
      }
    }

    [Test]
    public void TestReadHeader()
    {
      using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
        reader.BaseStream.Position = 8L;

        var header = reader.ReadHeader();

        Assert.AreEqual(StreamFileHeaderData.Size, reader.BaseStream.Position, "BaseStream.Position");

        Assert.IsNotNull(header);
        Assert.AreEqual(2, header.CodecVersion, "CodecVersion");
        Assert.AreEqual(1280 / 16, header.HorizontalPixels, "HorizontalPixels");
        Assert.AreEqual(720 / 8, header.VerticalPixels, "VerticalPixels");
        Assert.AreEqual(FrameScanning.Progressive, header.FrameScanning, "FrameScanning");

        CollectionAssert.AreEqual(new ushort[] {
                                    0x0000, 0x0010, 0x0011, 0x0012, 0x0012, 0x0013, 0x002a, 0x002c,
                                    0x0010, 0x0011, 0x0012, 0x0012, 0x0013, 0x0026, 0x002b, 0x002d,
                                    0x0011, 0x0012, 0x0013, 0x0013, 0x0028, 0x0029, 0x002d, 0x0030,
                                    0x0012, 0x0012, 0x0013, 0x0028, 0x0029, 0x002a, 0x002e, 0x0031,
                                    0x0012, 0x0013, 0x0028, 0x0029, 0x002a, 0x002b, 0x0030, 0x0065,
                                    0x0013, 0x0026, 0x0029, 0x002a, 0x002b, 0x002c, 0x0062, 0x0068,
                                    0x002a, 0x002b, 0x002d, 0x002e, 0x0030, 0x0062, 0x006d, 0x0074,
                                    0x002c, 0x002d, 0x0030, 0x0031, 0x0065, 0x0068, 0x0074, 0x007b,
                                  },
                                  header.LuminanceQuantizerTable.ToArray(), "LuminanceQuantizerTable");
        CollectionAssert.AreEqual(new ushort[] {
                                    0x0000, 0x0010, 0x0011, 0x0019, 0x001a, 0x001a, 0x002a, 0x002c,
                                    0x0010, 0x0011, 0x0019, 0x0019, 0x001a, 0x0026, 0x002b, 0x005b,
                                    0x0011, 0x0019, 0x001a, 0x001b, 0x0028, 0x0029, 0x005b, 0x0060,
                                    0x0019, 0x0019, 0x001b, 0x0028, 0x0029, 0x0054, 0x005d, 0x00c5,
                                    0x001a, 0x001a, 0x0028, 0x0029, 0x0054, 0x0056, 0x00bf, 0x00cb,
                                    0x001a, 0x0026, 0x0029, 0x0054, 0x0056, 0x00b1, 0x00c5, 0x00d1,
                                    0x002a, 0x002b, 0x005b, 0x005d, 0x00bf, 0x00c5, 0x00db, 0x00e8,
                                    0x002c, 0x005b, 0x0060, 0x00c5, 0x00cb, 0x00d1, 0x00e8, 0x00f6,
                                  },
                                  header.ChrominanceQuantizerTable.ToArray(), "ChrominanceQuantizerTable");
      }
    }

    [Test]
    public void TestReadHeaderAsByteArray()
    {
      using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
        reader.BaseStream.Position = 8L;

        var header = reader.ReadHeaderAsByteArray();

        Assert.AreEqual(StreamFileHeaderData.Size, reader.BaseStream.Position, "BaseStream.Position");

        Assert.IsNotNull(header);
        Assert.AreEqual(StreamFileHeaderData.Size, header.Length);
      }
    }

    [Test]
    public void TestReadFrameDataProgressive()
    {
      using (var reader = new StreamFileReader("test-1280x720p-10f.dv")) {
        reader.ReadHeader();

        Assert.AreEqual((long)0x00000004 << 12, reader.BaseStream.Position, "frame0 BaseStream.Position");

        var frameData = reader.ReadFrameData();

        Assert.IsNotNull(frameData, "frame0");
        Assert.AreEqual(0x000000000000, frameData.Audio.PrecedentSampleCount, "frame0 PrecedentAudioSampleCount");
        Assert.AreEqual(0x0321, frameData.Audio.SampleCount, "frame0 AudioSampleCount");
        Assert.AreEqual(48000, frameData.Audio.SamplingFrequency, "frame0 AudioSamplingFrequency");
        Assert.AreEqual(16, frameData.Video.DisplayAspectHorizontal, "frame0 DisplayAspectHorizontal");
        Assert.AreEqual(9, frameData.Video.DisplayAspectVertical, "frame0 DisplayAspectVertical");
        Assert.AreEqual(0xff, frameData.Video.EncodingQuality, "frame0 EncodingQuality");

        TestAudioData(frameData.Audio, true, 0);
        Assert.AreEqual(0x0321 * 4, frameData.Audio.Block.Count, "frame0 Audio.Block.Count");

        TestVideoData(frameData.Video, true, 0);
        Assert.AreEqual(27296,  frameData.Video.Block0.Count, "frame0 Video.Block0.Count");
        Assert.AreEqual(27360,  frameData.Video.Block1.Count, "frame0 Video.Block1.Count");
        Assert.AreEqual(0,      frameData.Video.Block2.Count, "frame0 Video.Block2.Count");
        Assert.AreEqual(0,      frameData.Video.Block3.Count, "frame0 Video.Block3.Count");

        Assert.AreEqual((long)0x00000013 << 12, reader.BaseStream.Position, "frame1 BaseStream.Position");

        frameData = reader.ReadFrameData();

        Assert.IsNotNull(frameData, "frame1");
        Assert.AreEqual(0x000000000321, frameData.Audio.PrecedentSampleCount, "frame1 PrecedentAudioSampleCount");
        Assert.AreEqual(0x0321, frameData.Audio.SampleCount, "frame1 AudioSampleCount");
        Assert.AreEqual(48000, frameData.Audio.SamplingFrequency, "frame1 AudioSamplingFrequency");
        Assert.AreEqual(16, frameData.Video.DisplayAspectHorizontal, "frame1 DisplayAspectHorizontal");
        Assert.AreEqual(9, frameData.Video.DisplayAspectVertical, "frame1 DisplayAspectVertical");
        Assert.AreEqual(0xff, frameData.Video.EncodingQuality, "frame1 EncodingQuality");

        TestAudioData(frameData.Audio, true, 1);
        Assert.AreEqual(0x0321 * 4, frameData.Audio.Block.Count, "frame1 Audio.Block.Count");

        TestVideoData(frameData.Video, true, 1);
        Assert.AreEqual(27360,  frameData.Video.Block0.Count, "frame1 Video.Block0.Count");
        Assert.AreEqual(27392,  frameData.Video.Block1.Count, "frame1 Video.Block1.Count");
        Assert.AreEqual(0,      frameData.Video.Block2.Count, "frame1 Video.Block2.Count");
        Assert.AreEqual(0,      frameData.Video.Block3.Count, "frame1 Video.Block3.Count");

        Assert.AreEqual((long)0x00000022 << 12, reader.BaseStream.Position, "frame2 BaseStream.Position");

        frameData = reader.ReadFrameData();

        Assert.IsNotNull(frameData, "frame2");
        Assert.AreEqual(0x000000000642, frameData.Audio.PrecedentSampleCount, "frame2 PrecedentAudioSampleCount");
        Assert.AreEqual(0x0321, frameData.Audio.SampleCount, "frame2 AudioSampleCount");
        TestAudioData(frameData.Audio, true, 2);
        TestVideoData(frameData.Video, true, 2);
        Assert.AreEqual(27360, frameData.Video.Block0.Count, "frame2 VideoSize[0]");
        Assert.AreEqual(27296, frameData.Video.Block1.Count, "frame2 VideoSize[1]");
        Assert.AreEqual(0, frameData.Video.Block2.Count, "frame2 VideoSize[2]");
        Assert.AreEqual(0, frameData.Video.Block3.Count, "frame2 VideoSize[3]");

        Assert.AreEqual((long)0x00000031 << 12, reader.BaseStream.Position, "frame3 BaseStream.Position");

        frameData = reader.ReadFrameData();

        Assert.IsNotNull(frameData, "frame3");
        Assert.AreEqual(0x00000000963, frameData.Audio.PrecedentSampleCount, "frame3 PrecedentAudioSampleCount");
        Assert.AreEqual(0x0321, frameData.Audio.SampleCount, "frame3 AudioSampleCount");
        TestAudioData(frameData.Audio, true, 3);
        TestVideoData(frameData.Video, true, 3);
        Assert.AreEqual(27392, frameData.Video.Block0.Count, "frame3 VideoSize[0]");
        Assert.AreEqual(27328, frameData.Video.Block1.Count, "frame3 VideoSize[1]");
        Assert.AreEqual(0, frameData.Video.Block2.Count, "frame3 VideoSize[2]");
        Assert.AreEqual(0, frameData.Video.Block3.Count, "frame3 VideoSize[3]");

        Assert.AreEqual((long)0x00000040 << 12, reader.BaseStream.Position, "frame4 BaseStream.Position");

        frameData = reader.ReadFrameData();

        Assert.IsNotNull(frameData, "frame4");
        Assert.AreEqual(0x000000000c84, frameData.Audio.PrecedentSampleCount, "frame4 PrecedentAudioSampleCount");
        Assert.AreEqual(0x0320, frameData.Audio.SampleCount, "frame4 AudioSampleCount");
        TestAudioData(frameData.Audio, true, 4);
        TestVideoData(frameData.Video, true, 4);
        Assert.AreEqual(27424, frameData.Video.Block0.Count, "frame4 VideoSize[0]");
        Assert.AreEqual(27328, frameData.Video.Block1.Count, "frame4 VideoSize[1]");
        Assert.AreEqual(0, frameData.Video.Block2.Count, "frame4 VideoSize[2]");
        Assert.AreEqual(0, frameData.Video.Block3.Count, "frame4 VideoSize[3]");

        Assert.AreEqual((long)0x0000004f << 12, reader.BaseStream.Position, "frame5 BaseStream.Position");

        frameData = reader.ReadFrameData();

        Assert.IsNotNull(frameData, "frame5");
        Assert.AreEqual(0x000000000fa4, frameData.Audio.PrecedentSampleCount, "frame5 PrecedentAudioSampleCount");
        Assert.AreEqual(0x0321, frameData.Audio.SampleCount, "frame5 AudioSampleCount");
        TestAudioData(frameData.Audio, true, 5);
        TestVideoData(frameData.Video, true, 5);
        Assert.AreEqual(27360, frameData.Video.Block0.Count, "frame5 VideoSize[0]");
        Assert.AreEqual(27360, frameData.Video.Block1.Count, "frame5 VideoSize[1]");
        Assert.AreEqual(0, frameData.Video.Block2.Count, "frame5 VideoSize[2]");
        Assert.AreEqual(0, frameData.Video.Block3.Count, "frame5 VideoSize[3]");

        Assert.AreEqual((long)0x0000005e << 12, reader.BaseStream.Position, "frame6 BaseStream.Position");

        frameData = reader.ReadFrameData(false, false);

        TestAudioData(frameData.Audio, false, 6);
        TestVideoData(frameData.Video, false, 6);

        Assert.AreEqual((long)0x0000006d << 12, reader.BaseStream.Position, "frame7 BaseStream.Position");

        frameData = reader.ReadFrameData(true, false);

        Assert.IsNotNull(frameData, "frame7");
        TestAudioData(frameData.Audio, true, 7);
        TestVideoData(frameData.Video, false, 7);

        Assert.AreEqual((long)0x0000007c << 12, reader.BaseStream.Position, "frame8 BaseStream.Position");

        frameData = reader.ReadFrameData(false, true);

        Assert.IsNotNull(frameData, "frame8");
        TestAudioData(frameData.Audio, false, 8);
        TestVideoData(frameData.Video, true, 8);

        Assert.AreEqual((long)0x0000008b << 12, reader.BaseStream.Position, "frame9 BaseStream.Position");

        frameData = reader.ReadFrameData(true, true);

        Assert.IsNotNull(frameData, "frame9");
        TestAudioData(frameData.Audio, true, 9);
        TestVideoData(frameData.Video, true, 9);

        frameData = reader.ReadFrameData();

        Assert.IsNull(frameData, "frame10");
      }
    }

    [Test, Ignore("to be written")]
    public void TestReadFrameDataInterlaced()
    {
    }

    internal static void TestAudioData(StreamFileAudioData audio, bool available, int frameNumber)
    {
      Assert.IsNotNull(audio, "audui must be non null value (frame {0})", frameNumber);

      var emptySegment = new ArraySegment<byte>();

      if (available) {
        Assert.IsTrue(audio.DataAvailable, "DataAvailable (frame {0})", frameNumber);

        Assert.AreNotEqual(emptySegment, audio.Block, "Block is not empty (frame {0})", frameNumber);
      }
      else {
        Assert.IsFalse(audio.DataAvailable, "DataAvailable (frame {0})", frameNumber);

        try {
          Assert.AreEqual(emptySegment, audio.Block);
          Assert.Fail("InvalidOperationException not thrown at Block (frame {0})", frameNumber);
        }
        catch (InvalidOperationException) {
        }
      }
    }

    internal static void TestVideoData(StreamFileVideoData video, bool available, int frameNumber)
    {
      Assert.IsNotNull(video, "video must be non null value (frame {0})", frameNumber);

      var emptySegment = new ArraySegment<byte>();

      if (available) {
        Assert.IsTrue(video.DataAvailable, "DataAvailable (frame {0})", frameNumber);

        Assert.AreNotEqual(emptySegment, video.Block0, "Block0 is not empty (frame {0})", frameNumber);
        Assert.AreNotEqual(emptySegment, video.Block1, "Block1 is not empty (frame {0})", frameNumber);
        Assert.AreNotEqual(emptySegment, video.Block2, "Block2 is not empty (frame {0})", frameNumber);
        Assert.AreNotEqual(emptySegment, video.Block3, "Block3 is not empty (frame {0})", frameNumber);

        Assert.AreEqual(video.Block0, video.GetBlock(0), "GetBlock(0) equals to Block0 (frame {0})", frameNumber);
        Assert.AreEqual(video.Block1, video.GetBlock(1), "GetBlock(1) equals to Block1 (frame {0})", frameNumber);
        Assert.AreEqual(video.Block2, video.GetBlock(2), "GetBlock(2) equals to Block2 (frame {0})", frameNumber);
        Assert.AreEqual(video.Block3, video.GetBlock(3), "GetBlock(3) equals to Block3 (frame {0})", frameNumber);
      }
      else {
        Assert.IsFalse(video.DataAvailable, "DataAvailable (frame {0})", frameNumber);

        try {
          Assert.AreEqual(emptySegment, video.Block0);
          Assert.Fail("InvalidOperationException not thrown at Block0 (frame {0})", frameNumber);
        }
        catch (InvalidOperationException) {
        }

        try {
          Assert.AreEqual(emptySegment, video.Block1);
          Assert.Fail("InvalidOperationException not thrown at Block1 (frame {0})", frameNumber);
        }
        catch (InvalidOperationException) {
        }

        try {
          Assert.AreEqual(emptySegment, video.Block2);
          Assert.Fail("InvalidOperationException not thrown at Block2 (frame {0})", frameNumber);
        }
        catch (InvalidOperationException) {
        }

        try {
          Assert.AreEqual(emptySegment, video.Block3);
          Assert.Fail("InvalidOperationException not thrown at Block3 (frame {0})", frameNumber);
        }
        catch (InvalidOperationException) {
        }
      }
    }
  }
}
