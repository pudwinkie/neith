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

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  public /*static*/ class BinaryConvert : Smdn.BinaryConvert {
    protected BinaryConvert() {}

    public static decimal ToFixedPointSigned88(byte[] @value, int startIndex)
    {
      return (decimal)ToInt16BE(@value, startIndex) / 0x100;
    }

    public static decimal ToFixedPointSigned1616(byte[] @value, int startIndex)
    {
      return (decimal)ToInt32BE(@value, startIndex) / 0x10000;
    }

    public static decimal ToFixedPointUnsigned88(byte[] @value, int startIndex)
    {
      return (decimal)ToUInt16BE(@value, startIndex) / 0x100;
    }

    public static decimal ToFixedPointUnsigned1616(byte[] @value, int startIndex)
    {
      return (decimal)ToUInt32BE(@value, startIndex) / 0x10000;
    }

    public static void GetBytesFixedPointSigned88(decimal @value, byte[] @bytes, int startIndex)
    {
      GetBytesBE((short)(@value * 0x100), @bytes, startIndex);
    }

    public static void GetBytesFixedPointSigned1616(decimal @value, byte[] @bytes, int startIndex)
    {
      GetBytesBE((int)(@value * 0x10000), @bytes, startIndex);
    }

    public static void GetBytesFixedPointUnsigned88(decimal @value, byte[] @bytes, int startIndex)
    {
      GetBytesBE((ushort)(@value * 0x100), @bytes, startIndex);
    }

    public static void GetBytesFixedPointUnsigned1616(decimal @value, byte[] @bytes, int startIndex)
    {
      GetBytesBE((uint)(@value * 0x10000), @bytes, startIndex);
    }

    private static decimal ToFixedPointSigned30_2(byte[] @value, int startIndex)
    {
      // 30.2 fixed-point
      return (decimal)ToInt32BE(@value, startIndex) / 0x40000000;
    }

    public static Matrix ToMatrix(byte[] @value, int startIndex)
    {
      return new Matrix(
        ToFixedPointSigned1616(@value, startIndex + 0),
        ToFixedPointSigned1616(@value, startIndex + 4),
        ToFixedPointSigned30_2(@value, startIndex + 8),

        ToFixedPointSigned1616(@value, startIndex + 12),
        ToFixedPointSigned1616(@value, startIndex + 16),
        ToFixedPointSigned30_2(@value, startIndex + 20),

        ToFixedPointSigned1616(@value, startIndex + 24),
        ToFixedPointSigned1616(@value, startIndex + 28),
        ToFixedPointSigned30_2(@value, startIndex + 32)
      );
    }

    private static void GetBytesFixedPointSigned30_2(decimal @value, byte[] @bytes, int startIndex)
    {
      GetBytesBE((int)(@value * 0x40000000), @bytes, startIndex);
    }

    public static void GetBytes(Matrix @value, byte[] @bytes, int startIndex)
    {
      GetBytesFixedPointSigned1616(@value.A, @bytes, startIndex + 0);
      GetBytesFixedPointSigned1616(@value.B, @bytes, startIndex + 4);
      GetBytesFixedPointSigned30_2(@value.U, @bytes, startIndex + 8);

      GetBytesFixedPointSigned1616(@value.C, @bytes, startIndex + 12);
      GetBytesFixedPointSigned1616(@value.D, @bytes, startIndex + 16);
      GetBytesFixedPointSigned30_2(@value.V, @bytes, startIndex + 20);

      GetBytesFixedPointSigned1616(@value.X, @bytes, startIndex + 24);
      GetBytesFixedPointSigned1616(@value.Y, @bytes, startIndex + 28);
      GetBytesFixedPointSigned30_2(@value.W, @bytes, startIndex + 32);
    }
  }
}

