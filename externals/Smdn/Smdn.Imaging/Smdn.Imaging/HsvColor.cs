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

namespace Smdn.Imaging {
  public struct HsvColor : IEquatable<HsvColor> {
    public static readonly HsvColor Black = new HsvColor(0, 0x00, 0x00, 0xff);
    public static readonly HsvColor White = new HsvColor(0, 0x00, 0xff, 0xff);
    public static readonly HsvColor Transparent = new HsvColor(0, 0x00, 0xff, 0x00);
    public static readonly HsvColor Empty = new HsvColor(0, 0x00, 0x00, 0x00);

    private int h;

    public int H {
      get { return h; }
      set
      {
        h = value;

        if (h < 0) {
          for (;;) {
            if (0 <= h)
              break;
            else
              h += 360;
          }
        }
        else {
          for (;;) {
            if (h < 360)
              break;
            else
              h -= 360;
          }
        }
      }
    }

    public byte S;
    public byte V;
    public byte A;

    public HsvColor(int hue, byte saturation, byte @value)
      : this(hue, saturation, @value, 0xff)
    {
    }

    public HsvColor(int hue, byte saturation, byte @value, byte alpha)
    {
      this.h = 0;

      this.S = saturation;
      this.V = @value;
      this.A = alpha;
      this.H = hue;
    }

#region "equality"
    public static bool operator == (HsvColor x, HsvColor y)
    {
      return
        x.h == y.h &&
        x.S == y.S &&
        x.V == y.V &&
        x.A == y.A;
    }

    public static bool operator != (HsvColor x, HsvColor y)
    {
      return !(x == y);
    }

    public override bool Equals(object obj)
    {
      if (obj is HsvColor)
        return Equals((HsvColor)obj);
      else
        return false;
    }

    public bool Equals(HsvColor other)
    {
      return this == other;
    }
#endregion

    public override int GetHashCode()
    {
      return A.GetHashCode() ^ ((h << 16) | (S << 8) | V).GetHashCode();
    }
  }
}
