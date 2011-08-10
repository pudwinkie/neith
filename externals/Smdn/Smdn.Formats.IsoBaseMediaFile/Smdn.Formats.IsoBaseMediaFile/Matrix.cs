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
  public struct Matrix : IEquatable<Matrix> {
    public static readonly Matrix Unity = new Matrix(new [] {
      1.0m, 0.0m, 0.0m,
      0.0m, 1.0m, 0.0m,
      0.0m, 0.0m, 1.0m,
    });

    public decimal[] Elements {
      get { return ToArray(); }
    }

    public decimal A {
      get { return a; }
      set { a = value; }
    }

    public decimal B {
      get { return b; }
      set { b = value; }
    }

    public decimal U {
      get { return u; }
      set { u = value; }
    }

    public decimal C {
      get { return c; }
      set { c = value; }
    }

    public decimal D {
      get { return d; }
      set { d = value; }
    }

    public decimal V {
      get { return v; }
      set { v = value; }
    }

    public decimal X {
      get { return x; }
      set { x = value; }
    }

    public decimal Y {
      get { return y; }
      set { y = value; }
    }

    public decimal W {
      get { return w; }
      set { w = value; }
    }

    public Matrix(decimal a, decimal b, decimal u, decimal c, decimal d, decimal v, decimal x, decimal y, decimal w)
    {
      this.a = a;
      this.b = b;
      this.u = u;
      this.c = c;
      this.d = d;
      this.v = v;
      this.x = x;
      this.y = y;
      this.w = w;
    }

    public Matrix(decimal[] matrix)
    {
      if (matrix.Length != 9)
        throw new ArgumentException("invalid length");

      this.a = matrix[0];
      this.b = matrix[1];
      this.u = matrix[2];
      this.c = matrix[3];
      this.d = matrix[4];
      this.v = matrix[5];
      this.x = matrix[6];
      this.y = matrix[7];
      this.w = matrix[8];
    }

    public decimal[] ToArray()
    {
      return new[] {
        a, b, u,
        c, d, v,
        x, y, w,
      };
    }

    public override bool Equals(object obj)
    {
      if (obj is Matrix)
        return Equals((Matrix)obj);
      else
        return false;
    }

    public bool Equals(Matrix other)
    {
      return
        a == other.a &&
        b == other.b &&
        u == other.u &&
        c == other.c &&
        d == other.d &&
        v == other.v &&
        x == other.x &&
        y == other.y &&
        w == other.w;
    }

    public override int GetHashCode()
    {
      return
        a.GetHashCode() ^
        b.GetHashCode() ^
        u.GetHashCode() ^
        c.GetHashCode() ^
        d.GetHashCode() ^
        v.GetHashCode() ^
        x.GetHashCode() ^
        y.GetHashCode() ^
        w.GetHashCode();
    }

    public override string ToString()
    {
      return string.Format("Matrix{{A={0}, B={1}, U={2}, C={3}, D={4}, V={5}, X={6}, Y={7}, W={8}}}",
                           a, b, u,
                           c, d, v,
                           x, y, w);
    }

    private decimal a;
    private decimal b;
    private decimal u;
    private decimal c;
    private decimal d;
    private decimal v;
    private decimal x;
    private decimal y;
    private decimal w;
  }
}
