using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Smdn.Formats.Loliconvert {
  [TestFixture]
  public class ToLoliTransformTests {
    const byte ロ = 0xdb;
    const byte リ = 0xd8;
    const byte CR = 0x0d;
    const byte LF = 0x0a;

    [Test]
    public void TestTransformBlock()
    {
      using (var transform = new ToLoliTransform()) {
        var outputBuffer = new byte[48];
        var inputBuffer = new byte[] {
          0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef
        };
        int ret;

        ret = transform.TransformBlock(inputBuffer, 0, 1, outputBuffer, 0);

        Assert.AreEqual(ret, 8, "0-1");
        Assert.AreEqual(new byte[] {ロ, ロ, ロ, ロ,   ロ, ロ, ロ, リ},
                        ArrayExtensions.Slice(outputBuffer, 0, 8), "0-1");

        ret = transform.TransformBlock(inputBuffer, 1, 2, outputBuffer, 8);

        Assert.AreEqual(ret, 16, "1-3");
        Assert.AreEqual(new byte[] {ロ, ロ, リ, ロ,   ロ, ロ, リ, リ,   ロ, リ, ロ, ロ,   ロ, リ, ロ, リ},
                        ArrayExtensions.Slice(outputBuffer, 8, 16), "1-3");

        ret = transform.TransformBlock(inputBuffer, 3, 2, outputBuffer, 24);

        Assert.AreEqual(ret, 16, "3-5");
        Assert.AreEqual(new byte[] {ロ, リ, リ, ロ,   ロ, リ, リ, リ,   リ, ロ, ロ, ロ,   リ, ロ, ロ, リ},
                        ArrayExtensions.Slice(outputBuffer, 24, 16), "3-5");

        ret = transform.TransformBlock(inputBuffer, 5, 1, outputBuffer, 0);

        Assert.AreEqual(ret, 10, "CRLF 5-6");
        Assert.AreEqual(new byte[] {CR, LF,   リ, ロ, リ, ロ,   リ, ロ, リ, リ},
                        ArrayExtensions.Slice(outputBuffer, 0, 10), "CRLF 5-6");

        ret = transform.TransformBlock(inputBuffer, 6, 2, outputBuffer, 0);

        Assert.AreEqual(ret, 16, "6-8");
        Assert.AreEqual(new byte[] {リ, リ, ロ, ロ,   リ, リ, ロ, リ,   リ, リ, リ, ロ,   リ, リ, リ, リ},
                        ArrayExtensions.Slice(outputBuffer, 0, 16), "6-8");
      }
    }

    [Test]
    public void TestTransformFinalBlock()
    {
      using (var transform = new ToLoliTransform()) {
        var outputBuffer = new byte[42];
        var inputBuffer = new byte[] {
          0x01, 0x23, 0x45, 0x67, 0x89, 0xab,
        };
        int ret;

        ret = transform.TransformBlock(inputBuffer, 0, 4, outputBuffer, 0);

        Assert.AreEqual(ret, 32, "first");

        var finalBlock = transform.TransformFinalBlock(inputBuffer, 4, 1);

        Assert.AreEqual(8, finalBlock.Length);
        Assert.AreEqual(new byte[] {リ, ロ, ロ, ロ,   リ, ロ, ロ, リ},
                        finalBlock, "first");

        ret = transform.TransformBlock(inputBuffer, 0, 4, outputBuffer, 0);

        Assert.AreEqual(ret, 32, "second");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 4, 2);

        Assert.AreEqual(8 + 2 + 8, finalBlock.Length);
        Assert.AreEqual(new byte[] {リ, ロ, ロ, ロ,   リ, ロ, ロ, リ,   CR, LF,   リ, ロ, リ, ロ,   リ, ロ, リ, リ},
                        finalBlock, "second");
      }
    }
  }
}
