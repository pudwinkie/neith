using System;
using System.IO;
using NUnit.Framework;

using Smdn.IO;

namespace Smdn.Formats.Riff {
  [TestFixture]
  public class RiffStructureTests {
    [Test]
    public void TestReadFrom()
    {
      var data = new byte[] {
        0x52, 0x49, 0x46, 0x46, 0x28, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20,
        0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x44, 0xac, 0x00, 0x00, 0x88, 0x58, 0x01, 0x00,
        0x02, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61, 0x04, 0x00, 0x00, 0x00, 0xde, 0xad, 0xbe, 0xaf,
      };

      using (var stream = new MemoryStream(data)) {
        var structures = RiffStructure.ReadFrom(stream);

        Assert.IsNotNull(structures);
        Assert.AreEqual(1, structures.Length);

        var riffWaveStructure = structures[0];

        Assert.IsNotNull(riffWaveStructure);
        Assert.AreEqual(KnownFourCC.RiffType.Wave, riffWaveStructure.FourCC, "RiffStructure.FourCC");
        Assert.AreEqual(RiffType.Wave, riffWaveStructure.RiffType, "RiffStructure.RiffType");
        Assert.AreEqual(0L, riffWaveStructure.Offset, "RiffStructure.Offset");
        Assert.AreEqual(0x00000028, riffWaveStructure.Size, "RiffStructure.Size");
        Assert.AreEqual(12L, riffWaveStructure.DataOffset, "RiffStructure.DataOffset");
        Assert.AreEqual(0x00000024, riffWaveStructure.DataSize, "RiffStructure.DataSize");
        Assert.AreEqual(2, riffWaveStructure.SubChunks.Count, "RiffStructure.SubChunks");

        var fmtChunk = riffWaveStructure.SubChunks[0];

        Assert.AreEqual(KnownFourCC.ChunkType.Format, fmtChunk.FourCC, "SubChunks[0].FourCC");
        Assert.AreEqual(12L, fmtChunk.Offset, "SubChunks[0].Offset");
        Assert.AreEqual(0x00000010, fmtChunk.Size, "SubChunks[0].Size");

        using (var fmtChunkStream = RiffStructure.GetChunkStream(stream, fmtChunk)) {
          Assert.IsNotNull(fmtChunkStream, "SubChunks[0] Stream");
          Assert.AreEqual(0L, fmtChunkStream.Position, "SubChunks[0] Stream.Position");
          Assert.AreEqual(0x10, fmtChunkStream.Length, "SubChunks[0] Stream.Length");

          CollectionAssert.AreEqual(new byte[] {
            0x01, 0x00, 0x01, 0x00, 0x44, 0xac, 0x00, 0x00, 0x88, 0x58, 0x01, 0x00, 0x02, 0x00, 0x10, 0x00,
          }, fmtChunkStream.ReadToEnd(), "SubChunks[0] Stream content");
        }

        var dataChunk = riffWaveStructure.SubChunks[1];

        Assert.AreEqual(KnownFourCC.ChunkType.Data, dataChunk.FourCC, "SubChunks[1].FourCC");
        Assert.AreEqual(36L, dataChunk.Offset, "SubChunks[1].Offset");
        Assert.AreEqual(0x00000004, dataChunk.Size, "SubChunks[1].Size");

        using (var dataChunkStream = RiffStructure.GetChunkStream(stream, dataChunk)) {
          Assert.IsNotNull(dataChunkStream, "SubChunks[1] Stream");
          Assert.AreEqual(0L, dataChunkStream.Position, "SubChunks[1] Stream.Position");
          Assert.AreEqual(0x4, dataChunkStream.Length, "SubChunks[1] Stream.Length");

          CollectionAssert.AreEqual(new byte[] {
            0xde, 0xad, 0xbe, 0xaf,
          }, dataChunkStream.ReadToEnd(), "SubChunks[1] Stream content");
        }
      }
    }
  }
}

