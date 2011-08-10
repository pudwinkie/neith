// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Drawing;

namespace Smdn.Imaging {
  public static class RectangleExtensions {
    public static Rectangle CreateCircumscribed(Point center, int radius)
    {
      return CreateCircumscribed(center.X, center.Y, radius);
    }

    public static Rectangle CreateCircumscribed(int x, int y, int radius)
    {
      return new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
    }

#region "corners"
    /*
     * XXX: System.Windows.Rect.TopLeft
     */
    public static Point GetTopLeft(this Rectangle rect)
    {
      return new Point(rect.Left, rect.Top);
    }

    public static Point GetTopRight(this Rectangle rect)
    {
      return new Point(rect.Right, rect.Top);
    }

    public static Point GetBottomLeft(this Rectangle rect)
    {
      return new Point(rect.Left, rect.Bottom);
    }

    public static Point GetBottomRight(this Rectangle rect)
    {
      return new Point(rect.Right, rect.Bottom);
    }
#endregion

    public static Point GetCenter(this Rectangle rect)
    {
      return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
    }

    public static PointF GetCenterF(this Rectangle rect)
    {
      return new PointF(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f);
    }
  }
}
