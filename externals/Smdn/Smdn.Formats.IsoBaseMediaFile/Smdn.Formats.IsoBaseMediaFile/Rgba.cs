// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

namespace Smdn.Formats.IsoBaseMediaFile {
  public struct Rgba : IEquatable<Rgba>, IEquatable<uint>, IEquatable<int> {
    public static readonly Rgba Transparent = new Rgba(0xff, 0xff, 0xff, 0x00);
    public static readonly Rgba Empty = new Rgba(0x00, 0x00, 0x00, 0x00);

    public byte R {
      get { return r; }
      set { r = value; }
    }

    public byte G {
      get { return g; }
      set { g = value; }
    }

    public byte B {
      get { return b; }
      set { b = value; }
    }

    public byte A {
      get { return a; }
      set { a = value; }
    }

    public Rgba(byte r, byte g, byte b, byte a)
    {
      this.r = r;
      this.g = g;
      this.b = b;
      this.a = a;
    }

    public Rgba(uint rgba)
    {
      this.r = (byte)((rgba & 0xff000000) >> 24);
      this.g = (byte)((rgba & 0x00ff0000) >> 16);
      this.b = (byte)((rgba & 0x0000ff00) >> 8);
      this.a = (byte) (rgba & 0x000000ff);
    }

    public override bool Equals(object obj)
    {
      if (obj is Rgba)
        return Equals((Rgba)obj);
      else if (obj is uint)
        return Equals((uint)obj);
      else if (obj is int)
        return Equals((int)obj);
      else
        return false;
    }

    public bool Equals(Rgba rgba)
    {
      return this.r == rgba.r &&
              this.g == rgba.g &&
              this.b == rgba.b &&
              this.a == rgba.a;
    }

    public bool Equals(uint rgba)
    {
      return (this.ToUInt32() == rgba);
    }

    public bool Equals(int rgba)
    {
      return (this.ToInt32() == rgba);
    }

    public static explicit operator uint(Rgba rgba)
    {
      return rgba.ToUInt32();
    }

    public static explicit operator int(Rgba rgba)
    {
      return rgba.ToInt32();
    }

    public uint ToUInt32()
    {
      return (uint)(r << 24 | g << 16 | b << 8 | a);
    }

    public int ToInt32()
    {
      return (int)(r << 24 | g << 16 | b << 8 | a);
    }

    public override int GetHashCode()
    {
      return (r << 24 | g << 16 | b << 8 | a);
    }

    public override string ToString()
    {
      return string.Format("{{R={0:x2}, G={1:x2}, B={2:x2}, A={3:x2}}}", r, g, b, a);
    }

    private byte r;
    private byte g;
    private byte b;
    private byte a;
  }
}