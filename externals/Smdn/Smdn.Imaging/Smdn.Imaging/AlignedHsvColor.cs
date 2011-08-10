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

namespace Smdn.Imaging {
  [CLSCompliant(false), StructLayout(LayoutKind.Explicit)]
  public struct AlignedHsvColor {
    [FieldOffset(0)] public byte H;
    [FieldOffset(1)] public byte S;
    [FieldOffset(2)] public byte V;
    [FieldOffset(3)] public byte A;

    [FieldOffset(0)] public uint Value;

    public AlignedHsvColor(byte hue, byte saturation, byte @value)
      : this(hue, saturation, @value, 0xff)
    {
    }

    public AlignedHsvColor(byte hue, byte saturation, byte @value, byte alpha)
    {
      this.Value = 0;
      this.H = hue;
      this.S = saturation;
      this.V = @value;
      this.A = alpha;
    }

    public AlignedHsvColor(uint hsvValue)
    {
      this.H = 0;
      this.S = 0;
      this.V = 0;
      this.A = 0;
      this.Value = hsvValue;
    }
  }
}
