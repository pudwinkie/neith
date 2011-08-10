using System;
using System.Collections.Generic;
using System.Text;
using CSUtil;

namespace CSUtil.Test
{
  using NUnit.Framework;

  [TestFixture]
  public class BoolCollectionTest
  {
    [Test]
    public void Test1()
    {
      List<byte> input = new List<byte>();
      List<bool> check = new List<bool>();
      for (int i = 0; i < 256; i++) {
        input.Add((byte)i);
        check.Add((i & (1 << 0)) != 0);
        check.Add((i & (1 << 1)) != 0);
        check.Add((i & (1 << 2)) != 0);
        check.Add((i & (1 << 3)) != 0);
        check.Add((i & (1 << 4)) != 0);
        check.Add((i & (1 << 5)) != 0);
        check.Add((i & (1 << 6)) != 0);
        check.Add((i & (1 << 7)) != 0);
      }
      BoolCollection bc = new BoolCollection(input.ToArray(), 0, 256);
      for (int i = 0; i < check.Count; i++) {
        Assert.AreEqual(check[i], bc[i], string.Format("COUNT[{0}] UNMATCH", i));
      }
    }

    [Test]
    public void Test2()
    {
      byte[] old = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
      byte[] now = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
      IEnumerator<BoolCollection.ChangeBit> changeBits =
        BoolCollection.EnumBitChange(old, now).GetEnumerator();

      for (int i = 0; i < 8; i++) {
        Assert.IsTrue(changeBits.MoveNext());
        Assert.AreEqual(8 * i + i, changeBits.Current.Index);
        Assert.AreEqual(true, changeBits.Current.Bit);
      }
      Assert.IsFalse(changeBits.MoveNext());
    }

    [Test]
    public void Test3()
    {
      byte[] o2 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
      byte[] n2 = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
      byte[] old = new byte[8];
      byte[] now = new byte[8];
      for (int i = 0; i < 8; i++) {
        old[i] = (byte)(o2[i] ^ 0xff);
        now[i] = (byte)(n2[i] ^ 0xff);
      }

      IEnumerator<BoolCollection.ChangeBit> changeBits =
        BoolCollection.EnumBitChange(old, now).GetEnumerator();

      for (int i = 0; i < 8; i++) {
        Assert.IsTrue(changeBits.MoveNext());
        Assert.AreEqual(8 * i + i, changeBits.Current.Index);
        Assert.AreEqual(false, changeBits.Current.Bit);
      }
      Assert.IsFalse(changeBits.MoveNext());
    }

    [Test]
    public void Test4()
    {
      byte[] d1 = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
      byte[] d2 = (byte[])d1.Clone();
      Assert.AreEqual(d1, d2);
      d1[0] = 0;
      Assert.AreNotEqual(d1, d2);
      d1[0] = 0x01;
      BoolCollection bits = new BoolCollection(d1);

      Assert.IsTrue(bits[0]);
      bits[0] = false;
      Assert.IsFalse(bits[0]);
      d2[0] = 0;
      Assert.AreEqual(d1, d2);

      bits[7] = true;
      Assert.IsFalse(bits[0]);
      Assert.IsTrue(bits[7]);
      d2[0] = 0x80;
      Assert.AreEqual(d1, d2);

      bits[63] = false;
      Assert.IsFalse(bits[63]);
      d2[7] =0;
      Assert.AreEqual(d1, d2);
    }

  }
}
