using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace Smdn.Imaging {
  [TestFixture]
  public class ImageCodecsTests {
    [Test]
    public void TestGetDecoder()
    {
      Assert.IsNull(ImageCodecs.GetDecoder(ImageCodecs.Guids.Netpbm));
    }

    [Test]
    public void TestGetEncoder()
    {
      Assert.IsNotNull(ImageCodecs.GetEncoder(ImageFormat.Bmp.Guid));
      Assert.IsNull(ImageCodecs.GetEncoder(ImageCodecs.Guids.Netpbm));
    }

    [Test]
    public void TestGuessDecoderFromExtension()
    {
      Assert.AreEqual(null, ImageCodecs.GuessDecoderFromExtension("image.bmp"));
      Assert.AreEqual(null, ImageCodecs.GuessDecoderFromExtension("image.jpg"));
      Assert.AreEqual(null, ImageCodecs.GuessDecoderFromExtension("image.png"));
    }

    [Test]
    public void TestGuessEncoderFromExtension()
    {
      Assert.AreEqual(ImageCodecs.Encoders.Bmp, ImageCodecs.GuessEncoderFromExtension("image.bmp"));
      Assert.AreEqual(ImageCodecs.Encoders.Jpeg, ImageCodecs.GuessEncoderFromExtension("image.jpg"));
      Assert.AreEqual(ImageCodecs.Encoders.Png, ImageCodecs.GuessEncoderFromExtension("image.png"));
    }
  }
}