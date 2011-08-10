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
using System.Runtime.InteropServices;

namespace Smdn.Imaging {
  [CLSCompliant(false), StructLayout(LayoutKind.Explicit)]
  public struct AlignedRgbColor {
    [FieldOffset(0)] public byte B;
    [FieldOffset(1)] public byte G;
    [FieldOffset(2)] public byte R;
    [FieldOffset(3)] public byte A;

    [FieldOffset(0)] public uint Value;

    public AlignedRgbColor(byte red, byte green, byte blue)
      : this(red, green, blue, 0xff)
    {
    }

    public AlignedRgbColor(byte red, byte green, byte blue, byte alpha)
    {
      this.Value = 0;
      this.B = blue;
      this.G = green;
      this.R = red;
      this.A = alpha;
    }

    public AlignedRgbColor(uint rgbValue)
    {
      this.B = 0;
      this.G = 0;
      this.R = 0;
      this.A = 0;
      this.Value = rgbValue;
    }

    public static explicit operator AlignedRgbColor (int val)
    {
      return new AlignedRgbColor((uint)val);
    }

    public static implicit operator AlignedRgbColor (uint val)
    {
      return new AlignedRgbColor(val);
    }

    public static explicit operator int (AlignedRgbColor rgb)
    {
      return (int)rgb.Value;
    }

    public static implicit operator uint (AlignedRgbColor rgb)
    {
      return rgb.Value;
    }

    public static implicit operator Color (AlignedRgbColor rgb)
    {
      return Color.FromArgb(rgb.A, rgb.R, rgb.G, rgb.B);
    }

    public static implicit operator AlignedRgbColor (Color rgb)
    {
      return new AlignedRgbColor(rgb.R, rgb.G, rgb.B, rgb.A);
    }
  }
}
