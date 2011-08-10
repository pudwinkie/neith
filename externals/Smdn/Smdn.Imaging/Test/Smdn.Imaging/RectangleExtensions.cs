using System;
using System.Drawing;
using NUnit.Framework;

namespace Smdn.Imaging {
  [TestFixture]
  public class RectangleExtensionsTests {
    [Test]
    public void TestCreateCircumscribed()
    {
      Assert.AreEqual(new Rectangle(0, 0, 30, 30), RectangleExtensions.CreateCircumscribed(15, 15, 15));
      Assert.AreEqual(new Rectangle(-15, -15, 30, 30), RectangleExtensions.CreateCircumscribed(0, 0, 15));
    }

    [Test]
    public void TestGetCorners()
    {
      var rect = new Rectangle(10, 20, 30, 40);

      Assert.AreEqual(new Point(10, 20), RectangleExtensions.GetTopLeft(rect));
      Assert.AreEqual(new Point(40, 20), RectangleExtensions.GetTopRight(rect));
      Assert.AreEqual(new Point(10, 60), RectangleExtensions.GetBottomLeft(rect));
      Assert.AreEqual(new Point(40, 60), RectangleExtensions.GetBottomRight(rect));
    }

    [Test]
    public void TestGetCenter()
    {
      var rect = new Rectangle(10, 10, 30, 60);

      Assert.AreEqual(new Point(25, 40), RectangleExtensions.GetCenter(rect));
      Assert.AreEqual(new PointF(25.0f, 40.0f), RectangleExtensions.GetCenterF(rect));
    }
  }
}