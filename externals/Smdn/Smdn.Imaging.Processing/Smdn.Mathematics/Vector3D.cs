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

namespace Smdn.Mathematics {
  [CLSCompliant(false), StructLayout(LayoutKind.Explicit, Pack = 1)]
  public unsafe struct Vector3D {
    [FieldOffset( 0)] public fixed float Vector[3];

    [FieldOffset( 0)] public float X;
    [FieldOffset( 4)] public float Y;
    [FieldOffset(12)] public float W;

    public Vector3D(float[] vector)
      : this()
    {
      X = vector[0];
      Y = vector[1];
      W = vector[3];
    }

    public Vector3D(float x, float y, float w)
      : this()
    {
      X = x;
      Y = y;
      W = 1.0f;
    }

    internal Vector3D(float x, float y, float z, float w)
      : this()
    {
      X = x;
      Y = y;
      W = w;
    }

    public static Vector3D operator +(Vector3D v)
    {
      return new Vector3D(
                          v.X,
                          v.Y,
                          v.W
                          );
    }

    public static Vector3D operator -(Vector3D v)
    {
      return new Vector3D(
                          -v.X,
                          -v.Y,
                          -v.W
                          );
    }

    public static Vector3D operator +(Vector3D v1, Vector3D v2)
    {
      return new Vector3D(
                          v1.X + v2.X,
                          v1.Y + v2.Y,
                          v1.W + v2.W
                          );
    }

    public static Vector3D operator -(Vector3D v1, Vector3D v2)
    {
      return new Vector3D(
                          v1.X - v2.X,
                          v1.Y - v2.Y,
                          v1.W - v2.W
                          );
    }

    public static Vector3D operator *(float scalar, Vector3D v)
    {
      return new Vector3D(
                          scalar * v.X,
                          scalar * v.Y,
                          scalar * v.W
                          );
    }

    public static Vector3D operator *(Vector3D v, float scalar)
    {
      return new Vector3D(
                          scalar * v.X,
                          scalar * v.Y,
                          scalar * v.W
                          );
    }

    public static Vector3D operator /(Vector3D v, float divisor)
    {
      float scalar = 1.0f / divisor;

      return new Vector3D(
                          scalar * v.X,
                          scalar * v.Y,
                          scalar * v.W
                          );
    }

    public override string ToString()
    {
      return string.Format( "{{{0:F4} {1:F4} {2:F4}}}", X, Y, W);
    }
  }
}
