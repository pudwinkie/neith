using System;
using NUnit.Framework;

namespace Smdn.Formats.Loliconvert {
  [TestFixture]
  public class FromLoliTransformTests {
    const byte ロ = 0xdb;
    const byte リ = 0xd8;
    const byte CR = 0x0d;
    const byte LF = 0x0a;

    [Test]
    public void TestTransformBlock()
    {
      using (var transform = new FromLoliTransform()) {
        var outputBuffer = new byte[4];
        var inputBuffer = new byte[] {
          ロ, ロ, ロ, ロ, ロ, ロ, ロ, リ, // 0
          ロ, ロ, リ, ロ, ロ, ロ, リ, リ, // 8
          ロ, リ, ロ, ロ, ロ, リ, ロ, リ, // 16
          ロ, リ, リ, ロ, ロ, リ, リ, リ, // 24
          CR, LF, // 32
          リ, ロ, ロ, ロ, リ, ロ, ロ, リ, // 34
          リ, ロ, リ, ロ, リ, ロ, リ, リ, // 42
          リ, リ, ロ, ロ, リ, リ, ロ, リ, // 50
          リ, リ, リ, ロ, リ, リ, リ, リ, // 58
          // 66
        };

        var ret = transform.TransformBlock(inputBuffer, 0, 6, outputBuffer, 0);

        Assert.AreEqual(0, ret, "0-6 bit");

        ret = transform.TransformBlock(inputBuffer, 6, 4, outputBuffer, 1);

        Assert.AreEqual(1, ret, "6-10 bit");
        Assert.AreEqual(0x01, outputBuffer[1], "6-10 bit");

        ret = transform.TransformBlock(inputBuffer, 10, 14, outputBuffer, 2);

        Assert.AreEqual(2, ret, "10-24 bit");
        Assert.AreEqual(0x23, outputBuffer[2], "10-24 bit");
        Assert.AreEqual(0x45, outputBuffer[3], "10-24 bit");

        ret = transform.TransformBlock(inputBuffer, 24, 7, outputBuffer, 0);

        Assert.AreEqual(0, ret, "24-31 bit");

        ret = transform.TransformBlock(inputBuffer, 31, 4, outputBuffer, 0);

        Assert.AreEqual(1, ret, "31-35 bit");
        Assert.AreEqual(0x67, outputBuffer[0], "31-35 bit");

        ret = transform.TransformBlock(inputBuffer, 35, 16, outputBuffer, 0);

        Assert.AreEqual(2, ret, "35-51 bit");
        Assert.AreEqual(0x89, outputBuffer[0], "35-51 bit");
        Assert.AreEqual(0xab, outputBuffer[1], "35-51 bit");

        ret = transform.TransformBlock(inputBuffer, 51, 15, outputBuffer, 0);

        Assert.AreEqual(2, ret, "51-66 bit");
        Assert.AreEqual(0xcd, outputBuffer[0], "51-66 bit");
        Assert.AreEqual(0xef, outputBuffer[1], "51-66 bit");
      }
    }

    [Test, ExpectedException(typeof(LolizationException))]
    public void TestTransformBlockContainsInvalidBit()
    {
      using (var transform = new FromLoliTransform()) {
        var inputBuffer = new byte[] {ロ, リ, CR, リ + 1};
        var outputBuffer = new byte[inputBuffer.Length * transform.OutputBlockSize];

        transform.TransformBlock(inputBuffer, 0, 4, outputBuffer, 0);
      }
    }

    [Test]
    public void TestTransformFinalBlock()
    {
      using (var transform = new FromLoliTransform()) {
        var inputBuffer = new byte[] {
          ロ, ロ, ロ, ロ, ロ, ロ, ロ, リ, // 0
          ロ, ロ, リ, ロ, ロ, ロ, リ, リ, // 8
          ロ, リ, ロ, ロ, ロ, リ, ロ, リ, // 16
          ロ, リ, リ, ロ, ロ, リ, リ, リ, // 24
          CR, LF, // 32
          リ, ロ, ロ, ロ, リ, ロ, ロ, リ, // 34
          リ, ロ, リ, ロ, リ, ロ, リ, リ, // 42
          リ, リ, ロ, ロ, リ, リ, ロ, リ, // 50
          リ, リ, リ, ロ, リ, リ, リ, リ, // 58
          // 66
        };

        var finalBlock = transform.TransformFinalBlock(inputBuffer, 0, 7);

        Assert.AreEqual(new byte[] {}, finalBlock, "0-7 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 0, 8);

        Assert.AreEqual(new byte[] {0x01}, finalBlock, "0-8 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 0, 9);

        Assert.AreEqual(new byte[] {0x01}, finalBlock, "0-9 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 0, 15);

        Assert.AreEqual(new byte[] {0x01}, finalBlock, "0-15 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 0, 16);

        Assert.AreEqual(new byte[] {0x01, 0x23}, finalBlock, "0-16 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 0, 17);

        Assert.AreEqual(new byte[] {0x01, 0x23}, finalBlock, "0-17 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 28, 4);

        Assert.AreEqual(new byte[] {}, finalBlock, "28-32 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 28, 5);

        Assert.AreEqual(new byte[] {}, finalBlock, "28-33 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 28, 6);

        Assert.AreEqual(new byte[] {}, finalBlock, "28-34 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 28, 9);

        Assert.AreEqual(new byte[] {}, finalBlock, "28-37 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 28, 10);

        Assert.AreEqual(new byte[] {0x78}, finalBlock, "28-38 bit");

        finalBlock = transform.TransformFinalBlock(inputBuffer, 28, 11);

        Assert.AreEqual(new byte[] {0x78}, finalBlock, "28-39 bit");
      }
    }
  }
}
