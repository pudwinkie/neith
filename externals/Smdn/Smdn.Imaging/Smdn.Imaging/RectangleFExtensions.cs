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
  public static class RectangleFExtensions {
    public static RectangleF CreateCircumscribed(PointF center, float radius)
    {
      return CreateCircumscribed(center.X, center.Y, radius);
    }

    public static RectangleF CreateCircumscribed(float x, float y, float radius)
    {
      return new RectangleF(x - radius, y - radius, radius * 2.0f, radius * 2.0f);
    }

#region "corners"
    /*
     * XXX: System.Windows.Rect.TopLeft
     */
    public static PointF GetTopLeft(this RectangleF rect)
    {
      return new PointF(rect.Left, rect.Top);
    }

    public static PointF GetTopRight(this RectangleF rect)
    {
      return new PointF(rect.Right, rect.Top);
    }

    public static PointF GetBottomLeft(this RectangleF rect)
    {
      return new PointF(rect.Left, rect.Bottom);
    }

    public static PointF GetBottomRight(this RectangleF rect)
    {
      return new PointF(rect.Right, rect.Bottom);
    }
#endregion

    public static PointF GetCenter(this RectangleF rect)
    {
      return new PointF(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f);
    }
  }
}