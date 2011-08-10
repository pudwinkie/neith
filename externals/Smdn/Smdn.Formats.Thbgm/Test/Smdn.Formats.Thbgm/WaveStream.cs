using System;
using System.IO;
using NUnit.Framework;

using Smdn.Media;
using Smdn.Formats.Riff;

namespace Smdn.Formats.Thbgm {
  [TestFixture]
  public class WaveStreamTests {
    private const string testStreamFile = "test.thbgm.dat";

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
    public void TestCreatedRiffStructure()
    {
      var productInfo = new ProductInfo("タイトル", "Creator", string.Empty, new TrackInfo[] {});
      var trackInfo = new TrackInfo(productInfo, "トラック", StreamFormat.ThXX, 0x10, 0x20, 0x20);

      using (var stream = WaveStream.CreateFrom(trackInfo.GetStream(testStreamFile, 2))) {
        var riffStructures = RiffStructure.ReadFrom(stream);

        Assert.AreEqual(1, riffStructures.Length);

        var riff = riffStructures[0];

        Assert.AreEqual(RiffType.Wave, riff.RiffType);
        Assert.AreEqual(KnownFourCC.RiffType.Wave, riff.FourCC);

        Assert.AreEqual(3, riff.SubChunks.Count);

        Assert.AreEqual(KnownFourCC.ChunkType.Format, riff.SubChunks[0].FourCC);
        Assert.AreEqual(0x10, riff.SubChunks[0].Size);

        Assert.AreEqual(KnownFourCC.ChunkType.Info, riff.SubChunks[1].FourCC);

        Assert.AreEqual(KnownFourCC.ChunkType.Data, riff.SubChunks[2].FourCC);
        Assert.AreEqual(0x20 + 0x20 * 2, riff.SubChunks[2].Size);

        // fmt
        var fmt = WAVEFORMATEX.ReadFrom(RiffStructure.GetChunkStream(stream, riff.SubChunks[0]));

        Assert.AreEqual(WAVE_FORMAT_TAG.WAVE_FORMAT_PCM, fmt.wFormatTag);
        Assert.AreEqual(44100, fmt.nSamplesPerSec);
        Assert.AreEqual(2, fmt.nChannels);
        Assert.AreEqual(16, fmt.wBitsPerSample);
        Assert.AreEqual(4 * 44100, fmt.nAvgBytesPerSec);
        Assert.AreEqual(4, fmt.nBlockAlign);

        // INFO
        var info = riff.SubChunks[1] as List;

        Assert.AreEqual(4, info.SubChunks.Count);
        Assert.AreEqual(new FourCC("INAM"), info.SubChunks[0].FourCC);
        Assert.AreEqual(new FourCC("IPRD"), info.SubChunks[1].FourCC);
        Assert.AreEqual(new FourCC("IART"), info.SubChunks[2].FourCC);
        Assert.AreEqual(new FourCC("ISTR"), info.SubChunks[3].FourCC);
        Assert.IsTrue(info.SubChunks[0].Size % 2 == 0);
        Assert.IsTrue(info.SubChunks[1].Size % 2 == 0);
        Assert.IsTrue(info.SubChunks[2].Size % 2 == 0);
        Assert.IsTrue(info.SubChunks[3].Size % 2 == 0);

        Assert.AreEqual(trackInfo.Title, ReadStringFromInfoChunk(stream, info.SubChunks[0]));
        Assert.AreEqual(productInfo.Title, ReadStringFromInfoChunk(stream, info.SubChunks[1]));
        Assert.AreEqual(productInfo.Creator, ReadStringFromInfoChunk(stream, info.SubChunks[2]));
        Assert.AreEqual(productInfo.Creator, ReadStringFromInfoChunk(stream, info.SubChunks[3]));
      }
    }

    [Test]
    public void TestCreatedRiffStructureWithAlignment()
    {
      var productInfo = new ProductInfo("タイトル", "Creator", string.Empty, new TrackInfo[] {});
      var trackInfo = new TrackInfo(productInfo, "トラック", StreamFormat.ThXX, 0x10, 0x20, 0x20);
      var alignment = 1024;

      using (var stream = WaveStream.CreateFrom(trackInfo.GetStream(testStreamFile, 2), true, alignment)) {
        var riffStructures = RiffStructure.ReadFrom(stream);

        Assert.AreEqual(1, riffStructures.Length);

        var riff = riffStructures[0];

        Assert.AreEqual(4, riff.SubChunks.Count);

        Assert.AreEqual(KnownFourCC.ChunkType.Format, riff.SubChunks[0].FourCC);
        Assert.AreEqual(0x10, riff.SubChunks[0].Size);

        Assert.AreEqual(KnownFourCC.ChunkType.Info, riff.SubChunks[1].FourCC);

        Assert.AreEqual(KnownFourCC.ChunkType.Junk, riff.SubChunks[2].FourCC);

        Assert.AreEqual(KnownFourCC.ChunkType.Data, riff.SubChunks[3].FourCC);
        Assert.AreEqual(0, riff.SubChunks[3].Offset % alignment);
      }
    }

    private string ReadStringFromInfoChunk(Stream stream, Chunk chunk)
    {
      using (var chunkStream = RiffStructure.GetChunkStream(stream, chunk)) {
        var data = new byte[chunk.Size];

        chunkStream.Read(data, 0, data.Length);

        var str = System.Text.Encoding.Default.GetString(data);
        var eos = str.IndexOf("\0");

        if (0 <= eos)
          return str.Substring(0, eos);
        else
          return str;
      }
    }
  }
}