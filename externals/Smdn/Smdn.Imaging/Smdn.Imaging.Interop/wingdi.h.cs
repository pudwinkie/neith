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

namespace Smdn.Imaging.Interop {
  [CLSCompliant(false)]
  public static class Consts {
    public const uint DIB_RGB_COLORS  = 0;
    public const uint DIB_PAL_COLORS  = 1;
    public const uint DIB_PAL_INDICES = 2;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct BITMAP {
    public int bmType;
    public int bmWidth;
    public int bmHeight;
    public int bmWidthBytes;
    public ushort bmPlanes;
    public ushort bmBitsPixel;
    public IntPtr bmBits;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct BITMAPFILEHEADER {
    public ushort bfType;
    public uint bfSize;
    public ushort bfReserved1;
    public ushort bfReserved2;
    public uint bfOffBits;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct BITMAPINFOHEADER {
    public uint biSize;
    public int biWidth;
    public int biHeight;
    public ushort biPlanes;
    public ushort biBitCount;
    public uint /*FourCC*/ biCompression;
    public uint biSizeImage;
    public int biXPelsPerMeter;
    public int biYPelsPerMeter;
    public uint biClrUsed;
    public uint biClrImportant;

    public static readonly int Size = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Explicit)]
  public struct RGBQUAD {
    [FieldOffset(0)] public uint rgbValue;

    [FieldOffset(0)] public byte rgbBlue;
    [FieldOffset(1)] public byte rgbGreen;
    [FieldOffset(2)] public byte rgbRed;
    [FieldOffset(3)] public byte rgbReserved;

    public RGBQUAD(uint rgbValue)
    {
      this.rgbBlue      = 0;
      this.rgbGreen     = 0;
      this.rgbRed       = 0;
      this.rgbReserved  = 0;
      this.rgbValue     = rgbValue;
    }

    public RGBQUAD(byte rgbRed, byte rgbGreen, byte rgbBlue, byte rgbReserved)
    {
      this.rgbValue     = 0;
      this.rgbBlue      = rgbBlue;
      this.rgbGreen     = rgbGreen;
      this.rgbRed       = rgbRed;
      this.rgbReserved  = rgbReserved;
    }

    public static implicit operator uint(RGBQUAD rgb)
    {
      return rgb.rgbValue;
    }

    public static implicit operator RGBQUAD(uint rgb)
    {
      return new RGBQUAD(rgb);
    }

    public static implicit operator Color(RGBQUAD rgb)
    {
      return Color.FromArgb(rgb.rgbReserved, rgb.rgbRed, rgb.rgbGreen, rgb.rgbBlue);
    }

    public static implicit operator RGBQUAD(Color color)
    {
      return new RGBQUAD(color.R, color.G, color.B, color.A);
    }
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct DIBSECTION {
    public BITMAP dsBm;
    public BITMAPINFOHEADER dsBmih;
    public uint dsBitfields0;
    public uint dsBitfields1;
    public uint dsBitfields2;
    public uint dsBitfields3;
    public IntPtr dshSection;
    public uint dsOffset;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct BITMAPINFO {
    public BITMAPINFOHEADER bmiHeader;
    public RGBQUAD bmiColor1;
    public RGBQUAD bmiColor2;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct BITMAPINFO_32BPP {
    public BITMAPINFOHEADER bmiHeader;
  }
}