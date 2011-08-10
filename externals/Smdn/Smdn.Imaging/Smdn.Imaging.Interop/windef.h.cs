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
using System.Runtime.InteropServices;

namespace Smdn.Imaging.Interop {
  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct RECT {
    public int left;
    public int top;
    public int right;
    public int bottom;

    public RECT(int left, int top, int right, int bottom)
    {
      this.left   = left;
      this.top    = top;
      this.right  = right;
      this.bottom = bottom;
    }

    public static implicit operator RECT (System.Drawing.Rectangle rect)
    {
      return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    public static implicit operator System.Drawing.Rectangle (RECT rect)
    {
      return System.Drawing.Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct POINT {
    public static readonly POINT Zero = new POINT(0, 0);

    public int x;
    public int y;

    public POINT(int x, int y)
    {
      this.x = x;
      this.y = y;
    }

    public static implicit operator POINT (System.Drawing.Point point)
    {
      return new POINT(point.X, point.Y);
    }

    public static implicit operator System.Drawing.Point (POINT point)
    {
      return new System.Drawing.Point(point.x, point.y);
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct POINTS {
    public static readonly POINTS Zero = new POINTS(0, 0);

    public short x;
    public short y;

    public POINTS(short x, short y)
    {
      this.x = x;
      this.y = y;
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct SIZE {
    public static readonly SIZE Zero = new SIZE(0, 0);

    public int cx;
    public int cy;

    public SIZE(int cx, int cy)
    {
      this.cx = cx;
      this.cy = cy;
    }

    public static implicit operator SIZE (System.Drawing.Size size)
    {
      return new SIZE(size.Width, size.Height);
    }

    public static implicit operator System.Drawing.Size (SIZE size)
    {
      return new System.Drawing.Size(size.cx, size.cy);
    }
  }
}