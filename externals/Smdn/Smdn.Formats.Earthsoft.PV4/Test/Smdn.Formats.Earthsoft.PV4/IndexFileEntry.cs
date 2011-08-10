using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Smdn.Formats.Earthsoft.PV4 {
  [TestFixture]
  public class IndexFileEntryTests {
    [Test]
    public void TestSizeOfStructure()
    {
      Assert.AreEqual(IndexFileEntry.Size, Marshal.SizeOf(typeof(IndexFileEntry)));
    }

    [Test]
    public void TestEquals()
    {
      var entry = new IndexFileEntry(0x00000004 * 4096, 0x000f * 4096);

      Assert.IsTrue(IndexFileEntry.Empty.Equals(new IndexFileEntry()));
      Assert.IsFalse(IndexFileEntry.Empty.Equals(entry));
      Assert.IsFalse(IndexFileEntry.Empty.Equals(null));
      Assert.IsFalse(IndexFileEntry.Empty.Equals(123));
      Assert.IsFalse(entry.Equals(new IndexFileEntry()));
      Assert.IsTrue(entry.Equals(entry));
      Assert.IsFalse(entry.Equals(IndexFileEntry.Empty));
    }

    [Test]
    public void TestOpEquality()
    {
      var entry = new IndexFileEntry(0x00000004 * 4096, 0x000f * 4096);

      Assert.IsTrue(IndexFileEntry.Empty == new IndexFileEntry());
      Assert.IsFalse(IndexFileEntry.Empty == entry);
      Assert.IsFalse(entry == IndexFileEntry.Empty);
    }

    [Test]
    public void TestOpInequality()
    {
      var entry = new IndexFileEntry(0x00000004 * 4096, 0x000f * 4096);

      Assert.IsFalse(IndexFileEntry.Empty != new IndexFileEntry());
      Assert.IsTrue(IndexFileEntry.Empty != entry);
      Assert.IsTrue(entry != IndexFileEntry.Empty);
    }
  }
}

