using System;
using System.Drawing;
using NUnit.Framework;

namespace Smdn.Imaging {
  [TestFixture]
  public class RectangleFExtensionsTests {
    [Test]
    public void TestCreateCircumscribed()
    {
      Assert.AreEqual(new RectangleF(0.0f, 0.0f, 30.0f, 30.0f), RectangleFExtensions.CreateCircumscribed(15.0f, 15.0f, 15.0f));
      Assert.AreEqual(new RectangleF(-15.0f, -15.0f, 30.0f, 30.0f), RectangleFExtensions.CreateCircumscribed(0.0f, 0.0f, 15.0f));
    }

    [Test]
    public void TestGetCorners()
    {
      var rect = new RectangleF(10.0f, 20.0f, 30.0f, 40.0f);

      Assert.AreEqual(new PointF(10.0f, 20.0f), RectangleFExtensions.GetTopLeft(rect));
      Assert.AreEqual(new PointF(40.0f, 20.0f), RectangleFExtensions.GetTopRight(rect));
      Assert.AreEqual(new PointF(10.0f, 60.0f), RectangleFExtensions.GetBottomLeft(rect));
      Assert.AreEqual(new PointF(40.0f, 60.0f), RectangleFExtensions.GetBottomRight(rect));
    }

    [Test]
    public void TestGetCenter()
    {
      var rect = new RectangleF(10.0f, 10.0f, 30.0f, 60.0f);

      Assert.AreEqual(new PointF(25.0f, 40.0f), RectangleFExtensions.GetCenter(rect));
    }
  }
}