using System;
using System.Drawing;
using NUnit.Framework;

namespace Smdn.Imaging {
  [TestFixture]
  public class ColorModelTests {
    [Test]
    public void TestLuminanceOf()
    {
      Assert.AreEqual(1, (int)ColorModel.LuminanceOf(Color.White));
      Assert.AreEqual(0, (int)ColorModel.LuminanceOf(Color.Black));

      Assert.AreEqual(1, (int)ColorModel.LuminanceOf(HsvColor.White));
      Assert.AreEqual(0, (int)ColorModel.LuminanceOf(HsvColor.Black));
    }

    [Test]
    public void TestAverageOf()
    {
      Assert.AreEqual(Color.FromArgb(0xff, 0x40, 0x40, 0x40),
                      ColorModel.AverageOf(Color.FromArgb(0xff, 0x80, 0x00, 0x80), 
                                           Color.FromArgb(0xff, 0x00, 0x80, 0x00)));
      Assert.AreEqual(Color.FromArgb(0xff, 0x20, 0x20, 0x20),
                      ColorModel.AverageOf(Color.FromArgb(0xff, 0x60, 0x00, 0x00), 
                                           Color.FromArgb(0xff, 0x00, 0x60, 0x00),
                                           Color.FromArgb(0xff, 0x00, 0x00, 0x60)));
    }
  }
}