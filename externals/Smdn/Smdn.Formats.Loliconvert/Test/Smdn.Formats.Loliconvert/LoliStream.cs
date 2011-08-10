using System;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace Smdn.Formats.Loliconvert {
  [TestFixture]
  public class LoliStreamTests {
    const byte ロ = 0xdb;
    const byte リ = 0xd8;
    const byte CR = 0x0d;
    const byte LF = 0x0a;

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestConstructInvalidArgument()
    {
      new LoliStream(Stream.Null, (LoliconvertMode)0);
    }

    [Test]
    public void TestCompress()
    {
      using (var memoryStream = new MemoryStream()) {
        using (var stream = new LoliStream(memoryStream, LoliconvertMode.Compress)) {
          stream.WriteByte(0x55);
          stream.Write(new byte[] {0x55, 0x55}, 0, 2);
          stream.Write(new byte[] {0x55, 0x55}, 0, 2);
          // CRLF
          stream.Write(new byte[] {0xaa}, 0, 1);
          stream.WriteByte(0xaa);
          stream.Write(new byte[] {0xaa, 0xaa, 0xaa}, 0, 3);
          // CRLF
          stream.WriteByte(0x33);
          stream.WriteByte(0x33);
        }

        var compressed = memoryStream.ToArray();

        Assert.AreEqual(5 * 8 + 2 + 5 * 8 + 2 + 2 * 8, compressed.Length);

        Assert.AreEqual(new byte[] {ロ, リ, ロ, リ,   ロ, リ, ロ, リ},
                        ArrayExtensions.Slice(compressed, 0, 8), "0-8 bytes");
        Assert.AreEqual(new byte[] {ロ, リ, ロ, リ,   ロ, リ, ロ, リ},
                        ArrayExtensions.Slice(compressed, 8, 8), "8-16 bytes");
        Assert.AreEqual(new byte[] {ロ, リ, ロ, リ,   ロ, リ, ロ, リ},
                        ArrayExtensions.Slice(compressed, 16, 8), "16-24 bytes");
        Assert.AreEqual(new byte[] {ロ, リ, ロ, リ,   ロ, リ, ロ, リ},
                        ArrayExtensions.Slice(compressed, 24, 8), "24-32 bytes");
        Assert.AreEqual(new byte[] {ロ, リ, ロ, リ,   ロ, リ, ロ, リ},
                        ArrayExtensions.Slice(compressed, 32, 8), "32-40 bytes");

        Assert.AreEqual(new byte[] {CR, LF},
                        ArrayExtensions.Slice(compressed, 40, 2), "40-42 bytes");

        Assert.AreEqual(new byte[] {リ, ロ, リ, ロ,   リ, ロ, リ, ロ},
                        ArrayExtensions.Slice(compressed, 42, 8), "42-50 bytes");
        Assert.AreEqual(new byte[] {リ, ロ, リ, ロ,   リ, ロ, リ, ロ},
                        ArrayExtensions.Slice(compressed, 50, 8), "50-58 bytes");
        Assert.AreEqual(new byte[] {リ, ロ, リ, ロ,   リ, ロ, リ, ロ},
                        ArrayExtensions.Slice(compressed, 58, 8), "58-66 bytes");
        Assert.AreEqual(new byte[] {リ, ロ, リ, ロ,   リ, ロ, リ, ロ},
                        ArrayExtensions.Slice(compressed, 66, 8), "66-74 bytes");
        Assert.AreEqual(new byte[] {リ, ロ, リ, ロ,   リ, ロ, リ, ロ},
                        ArrayExtensions.Slice(compressed, 74, 8), "74-82 bytes");

        Assert.AreEqual(new byte[] {CR, LF},
                        ArrayExtensions.Slice(compressed, 82, 2), "82-84 bytes");

        Assert.AreEqual(new byte[] {ロ, ロ, リ, リ,   ロ, ロ, リ, リ},
                        ArrayExtensions.Slice(compressed, 84, 8), "84-92 bytes");
        Assert.AreEqual(new byte[] {ロ, ロ, リ, リ,   ロ, ロ, リ, リ},
                        ArrayExtensions.Slice(compressed, 92, 8), "92-100 bytes");
      }
    }

    [Test]
    public void TestDecompress()
    {
      using (var memoryStream = new MemoryStream(new byte[] {
        ロ, リ, ロ, リ, ロ, リ, ロ, リ,
        リ, ロ, リ, ロ, リ, ロ, リ, ロ,
        ロ, リ, ロ, リ,
      })) {
        using (var stream = new LoliStream(memoryStream, LoliconvertMode.Decompress)) {
          var buffer = new byte[3];

          var ret = stream.Read(buffer, 0, 3);

          Assert.AreEqual(2, ret);
          Assert.AreEqual(0x55, buffer[0]);
          Assert.AreEqual(0xaa, buffer[1]);
        }
      }

      using (var memoryStream = new MemoryStream(new byte[] {
        ロ, リ, ロ, LF, リ, ロ, リ, ロ, リ, CR,
        リ, ロ, リ, ロ, CR, LF, リ, ロ, リ, ロ, LF, CR,
        ロ, リ, ロ, リ,
      })) {
        using (var stream = new LoliStream(memoryStream, LoliconvertMode.Decompress)) {
          var buffer = new byte[3];

          var ret = stream.Read(buffer, 0, 3);

          Assert.AreEqual(2, ret);
          Assert.AreEqual(0x55, buffer[0]);
          Assert.AreEqual(0xaa, buffer[1]);
        }
      }
    }

    [Test, ExpectedException(typeof(LolizationException))]
    public void TestDecompressContainsInvalidBit()
    {
      using (var memoryStream = new MemoryStream(new byte[] {
        ロ, リ, ロ, リ, ロ, リ, ロ, リ, 0x00,
      })) {
        using (var stream = new LoliStream(memoryStream, LoliconvertMode.Decompress)) {
          var buffer = new byte[2];
          stream.Read(buffer, 0, 2);
        }
      }
    }
  }
}
