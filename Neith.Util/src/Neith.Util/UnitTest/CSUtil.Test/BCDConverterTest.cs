using System;
using System.Collections.Generic;
using System.Text;
using CSUtil;

namespace CSUtil.Test
{
  using NUnit.Framework;

  [TestFixture]
  public class BCDConverterTest
  {
    [Test]
    public void ToInt32()
    {
      byte[] testData = new byte[] { 0x12, 0x34, 0x56, 0x78, 0, 0 };
      Array.Reverse(testData);
      Assert.AreEqual(12345678, BCDConverter.ToInt32(testData, 2, 4));
    }

    [Test]
    public void ToDateTime1()
    {
      int year = DateTime.Now.Year - 20;
      int YY = year % 100;
      byte bcdYY = (byte)(((YY / 10) << 4) + (YY % 10));

      List<byte> testDataList = new List<byte>();
      testDataList.Add(bcdYY);
      testDataList.Add(0x12);
      testDataList.Add(0x31);
      testDataList.Add(0x00);
      testDataList.AddRange(BitConverter.GetBytes((int)12345678));

      DateTime check = new DateTime(year, 12, 31);
      check += TimeSpan.FromMilliseconds(12345678);

      Assert.AreEqual(check, BCDConverter.ToDateTime(testDataList.ToArray(), 0));
    }

    [Test]
    public void ToDateTime2()
    {
      int year = DateTime.Now.Year + 60;
      int YY = year % 100;
      byte bcdYY = (byte)(((YY / 10) << 4) + (YY % 10));

      List<byte> testDataList = new List<byte>();
      testDataList.Add(bcdYY);
      testDataList.Add(0x12);
      testDataList.Add(0x31);
      testDataList.Add(0x00);
      testDataList.AddRange(BitConverter.GetBytes((int)12345678));

      DateTime check = new DateTime(year, 12, 31);
      check += TimeSpan.FromMilliseconds(12345678);

      Assert.AreEqual(check, BCDConverter.ToDateTime(testDataList.ToArray(), 0));
    }

    [Test]
    public void GetBytesDateTime()
    {
      DateTime dt = new DateTime(2007, 6, 7);
      dt += TimeSpan.FromMilliseconds(24 * 60 * 60 * 1000 - 1);
      List<byte> check = new List<byte>();
      check.AddRange(new byte[] { 0x07, 0x06, 0x07, 0x00 });
      check.AddRange(BitConverter.GetBytes((int) (24*60*60*1000-1)));

      Assert.AreEqual(check.ToArray(), BCDConverter.GetBytes(dt));
    }

  }
}
