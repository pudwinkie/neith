using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4.IO {
  [TestFixture]
  public class UtilsTests {
    [Test]
    public void TestIsStreamFile()
    {
      Assert.IsTrue(Utils.IsStreamFile("test-1280x720p-10f.dv"));

      const string testfile = "test-nondvfile.tmp";

      try {
        using (var stream = File.OpenWrite(testfile)) {
          stream.SetLength(0L);

          using (var writer = new Smdn.IO.BinaryWriter(stream)) {
            writer.WriteZero(StreamFileHeaderData.Size);
          }
        }

        Assert.IsFalse(Utils.IsStreamFile(testfile));
      }
      finally {
        if (File.Exists(testfile))
          File.Delete(testfile);
      }
    }

    [Test]
    public void TestGetFrameListFromStreamFile()
    {
      var expected = new[] {
        new IndexFileEntry() {FrameOffset = 0x0004 * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x0013 * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x0022 * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x0031 * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x0040 * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x004f * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x005e * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x006d * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x007c * 4096, FrameSize = 0x000f * 4096},
        new IndexFileEntry() {FrameOffset = 0x008b * 4096, FrameSize = 0x000f * 4096},
      };

      CollectionAssert.AreEqual(expected, Utils.GetFrameListFromStreamFile("test-1280x720p-10f.dv"));
    }

    [Test]
    public void TestGetFrameListFromIndexFile()
    {
      var expected = new[] {
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
      };

      CollectionAssert.AreEqual(expected, Utils.GetFrameListFromIndexFile("test-1280x720p-10f.dvi"));
    }

    [Test]
    public void TestExtractToFile()
    {
      const string infile = "test-1280x720p-10f.dv";
      const string outfile = "test-extracted.dv";
      const string outfile_index = "test-extracted.dvi";

      try {
        foreach (var test in new[] {
          new {Start = 0, Count = 10, StartFrameOffset = 0x00000004 * 4096L, EndFrameOffset = 0x0000009a * 4096L},
          new {Start = 0, Count =  5, StartFrameOffset = 0x00000004 * 4096L, EndFrameOffset = 0x0000004f * 4096L},
          new {Start = 3, Count =  2, StartFrameOffset = 0x00000031 * 4096L, EndFrameOffset = 0x0000004f * 4096L},
          new {Start = 5, Count =  5, StartFrameOffset = 0x0000004f * 4096L, EndFrameOffset = 0x0000009a * 4096L},
        }) {
          if (File.Exists(outfile))
            File.Delete(outfile);
          if (File.Exists(outfile_index))
            File.Delete(outfile_index);

          Utils.ExtractToFile(infile, test.Start, test.Count, outfile);

          using (var infileStream = File.OpenRead(infile)) {
            using (var outfileStream = File.OpenRead(outfile)) {
              Assert.Greater(outfileStream.Length, StreamFileHeaderData.Size);

              var infileHeader = new byte[StreamFileHeaderData.Size];
              var outfileHeader = new byte[StreamFileHeaderData.Size];

              infileStream.Read(infileHeader, 0, StreamFileHeaderData.Size);
              outfileStream.Read(outfileHeader, 0, StreamFileHeaderData.Size);

              CollectionAssert.AreEqual(infileHeader, outfileHeader, "HeaderData Start = {0}, Count = {1}", test.Start, test.Count);

              var infileFrameDataStream = new Smdn.IO.PartialStream(infileStream,
                                                                    test.StartFrameOffset,
                                                                    test.EndFrameOffset - test.StartFrameOffset,
                                                                    true,
                                                                    true);
              var outfileFrameDataStream = new Smdn.IO.PartialStream(outfileStream,
                                                                     StreamFileHeaderData.Size,
                                                                     true,
                                                                     true);

              Assert.AreEqual(infileFrameDataStream.Length, outfileFrameDataStream.Length, "FrameData stream length Start = {0}, Count = {1}", test.Start, test.Count);

              var infileReader  = new StreamFileReader(infileFrameDataStream);
              var outfileReader = new StreamFileReader(outfileFrameDataStream);

              var testname = string.Format("Start = {0}, Count = {1}", test.Start, test.Count);

              for (var i = 0; i < test.Count; i++) {
                var infileFrame   = infileReader.ReadFrameData();
                var outfileFrame  = outfileReader.ReadFrameData();

                Assert.IsNotNull(outfileFrame, "frame {0}", testname);
                Assert.AreEqual( infileFrame.Audio.SamplingFrequency,
                                outfileFrame.Audio.SamplingFrequency, "frame Audio.SamplingFrequency {0} Frame{1}", testname, i);
                Assert.AreEqual( infileFrame.Video.DisplayAspectRatio,
                                outfileFrame.Video.DisplayAspectRatio, "frame Video.DisplayAspectRatio {0} Frame{1}", testname, i);
                Assert.AreEqual( infileFrame.Video.EncodingQuality,
                                outfileFrame.Video.EncodingQuality, "frame Video.EncodingQuality {0} Frame{1}", testname, i);
                Assert.AreEqual( infileFrame.Video.Block0.Count,
                                outfileFrame.Video.Block0.Count, "frame Video.Block0.Count {0} Frame{1}", testname, i);
                Assert.AreEqual( infileFrame.Video.Block1.Count,
                                outfileFrame.Video.Block1.Count, "frame Video.Block1.Count {0} Frame{1}", testname, i);
                Assert.AreEqual( infileFrame.Video.Block2.Count,
                                outfileFrame.Video.Block2.Count, "frame Video.Block2.Count {0} Frame{1}", testname, i);
                Assert.AreEqual( infileFrame.Video.Block3.Count,
                                outfileFrame.Video.Block3.Count, "frame Video.Block3.Count {0} Frame{1}", testname, i);

                if (i != test.Count - 1)
                  Assert.AreEqual( infileReader.BaseStream.Position,
                                  outfileReader.BaseStream.Position, "base stream position {0} Frame{1}", testname, i);
              }
            }
          }
        }
      }
      finally {
        if (File.Exists(outfile))
          File.Delete(outfile);
        if (File.Exists(outfile_index))
          File.Delete(outfile_index);
      }
    }
  }
}

