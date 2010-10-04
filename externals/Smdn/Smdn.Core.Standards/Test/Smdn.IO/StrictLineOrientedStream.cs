using System;
using System.IO;
using NUnit.Framework;

using Smdn.Formats;

namespace Smdn.IO {
  [TestFixture]
  public class StrictLineOrientedStreamTests {
    [Test]
    public void TestReadLineCRLF()
    {
      var data = new byte[] {0x00, Octets.CR, 0x02, Octets.LF, 0x04, Octets.LF, Octets.CR, 0x07, Octets.CR, Octets.LF, 0x10};
      var stream = new StrictLineOrientedStream(new MemoryStream(data), 8);

      Assert.AreEqual(ArrayExtensions.Slice(data, 0, 10), stream.ReadLine());
      Assert.AreEqual(ArrayExtensions.Slice(data, 10, 1), stream.ReadLine());
      Assert.IsNull(stream.ReadLine());
    }

    [Test]
    public void TestReadLineDiscardEOL()
    {
      var data = new byte[] {0x00, 0x01, 0x02, 0x03, Octets.CR, Octets.LF, 0x04, 0x05};
      var stream = new StrictLineOrientedStream(new MemoryStream(data), 8);

      Assert.AreEqual(ArrayExtensions.Slice(data, 0, 4), stream.ReadLine(false));
      Assert.AreEqual(ArrayExtensions.Slice(data, 6, 2), stream.ReadLine(false));
      Assert.IsNull(stream.ReadLine());
    }

    [Test]
    public void TestReadLineKeepEOL()
    {
      var data = new byte[] {0x00, 0x01, 0x02, 0x03, Octets.CR, Octets.LF, 0x04, 0x05};
      var stream = new StrictLineOrientedStream(new MemoryStream(data), 8);

      Assert.AreEqual(ArrayExtensions.Slice(data, 0, 6), stream.ReadLine());
      Assert.AreEqual(ArrayExtensions.Slice(data, 6, 2), stream.ReadLine());
      Assert.IsNull(stream.ReadLine());
    }

    [Test]
    public void TestReadLineLongerThanBufferDiscardEOL()
    {
      var data = new byte[] {
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, Octets.CR, Octets.LF,
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
      };
      var stream = new StrictLineOrientedStream(new MemoryStream(data), 8);

      Assert.AreEqual(ArrayExtensions.Slice(data, 0, 10), stream.ReadLine(false));
      Assert.AreEqual(ArrayExtensions.Slice(data, 12), stream.ReadLine(false));
      Assert.IsNull(stream.ReadLine());
    }

    [Test]
    public void TestReadLineLongerThanBufferKeepEOL()
    {
      var data = new byte[] {
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, Octets.CR, Octets.LF,
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
      };
      var stream = new StrictLineOrientedStream(new MemoryStream(data), 8);

      Assert.AreEqual(ArrayExtensions.Slice(data, 0, 12), stream.ReadLine());
      Assert.AreEqual(ArrayExtensions.Slice(data, 12), stream.ReadLine());
      Assert.IsNull(stream.ReadLine());
    }
  }
}
