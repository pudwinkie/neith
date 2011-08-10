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
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Smdn.Imaging.Interop;
using Smdn.IO;

namespace Smdn.Imaging.Formats.Ico {
  public static class Icon {
    private static ColorDepth GetColorDepthOrThrowException(int colorDepth)
    {
      switch (colorDepth) {
        case 1:  return ColorDepth.Depth1Bit;
        case 4:  return ColorDepth.Depth4Bit;
        case 8:  return ColorDepth.Depth8Bit;
        case 16: return ColorDepth.Depth16Bit;
        case 24: return ColorDepth.Depth24Bit;
        case 32: return ColorDepth.Depth32Bit;
        default:
          throw new NotSupportedException(string.Format("unsupported color depth: {0}", colorDepth));
      }
    }

    public static Bitmap FromHIcon(IntPtr hIcon)
    {
      if (!Runtime.IsRunningOnWindows)
        throw new PlatformNotSupportedException();
      if (hIcon == IntPtr.Zero)
        throw new ArgumentException("hIcon == NULL", "hIcon");

      ICONINFO iconinfo;

      if (!user32.GetIconInfo(hIcon, out iconinfo))
        throw new InvalidDataException("GetIconInfo failed");

      var hdc = IntPtr.Zero;

      try {
        var bmi = new BITMAPINFO_32BPP();

        bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(bmi.bmiHeader);

        hdc = gdi32.CreateCompatibleDC(IntPtr.Zero);

        if (0 == gdi32.GetDIBits(hdc, iconinfo.hbmColor, 0, 0, IntPtr.Zero, ref bmi, Consts.DIB_RGB_COLORS))
          throw new InvalidDataException("getting image info failed");

        GetColorDepthOrThrowException(bmi.bmiHeader.biBitCount);

        var w = bmi.bmiHeader.biWidth;
        var h = bmi.bmiHeader.biHeight;

        /*
         * image
         */
        var image = new RGBQUAD[w, h];

        unsafe {
          byte* bits = stackalloc byte[(int)bmi.bmiHeader.biSizeImage];

          if (h != gdi32.GetDIBits(hdc, iconinfo.hbmColor, 0, (uint)h, (void*)bits, ref bmi, Consts.DIB_RGB_COLORS))
            throw new InvalidDataException("getting image DIB failed");

          var stride = (((w * bmi.bmiHeader.biPlanes * bmi.bmiHeader.biBitCount) + 31) & ~31) / 8;

          for (var y = 0; y < h; y++) {
            var pixel = (RGBQUAD*)(bits + (h - 1 - y) * stride);

            for (var x = 0; x < w; x++) {
              image[x, y] = *(pixel++);
            }
          }
        }

        /*
         * mask
         */
        var mask = new byte[w, h];

        if (bmi.bmiHeader.biClrUsed == 0) {
          for (var y = 0; y < h; y++) {
            for (var x = 0; x < w; x++) {
              mask[x, y] = (byte)0xff;
            }
          }
        }
        else {
          var bmi1bit = new BITMAPINFO();

          bmi1bit.bmiHeader.biSize = (uint)Marshal.SizeOf(bmi1bit.bmiHeader);

          if (0 == gdi32.GetDIBits(hdc, iconinfo.hbmMask, 0, 0, IntPtr.Zero, ref bmi1bit, Consts.DIB_PAL_COLORS))
            throw new InvalidDataException("getting mask info failed");

          unsafe {
            byte* bits = stackalloc byte[(int)bmi1bit.bmiHeader.biSizeImage];

            if (h != gdi32.GetDIBits(hdc, iconinfo.hbmMask, 0, (uint)h, bits, ref bmi1bit, Consts.DIB_RGB_COLORS))
              throw new InvalidDataException("getting mask DIB failed");

            var stride = (((w * bmi1bit.bmiHeader.biPlanes * bmi1bit.bmiHeader.biBitCount) + 31) & ~31) / 8;

            for (var y = 0; y < h; y++) {
              var pixel = (bits + (h - 1 - y) * stride);

              for (var x = 0; x < w; x++) {
                //mask[x, y] = (((*(pixel + x / 8) >> (7 - x % 8)) & 0x1) == 0) ? (byte)0xff : (byte)0x00;
                mask[x, y] = (byte)(((*(pixel + x / 8) >> (7 - x % 8)) & 0x1) - 1);
              }
            }
          }
        } // if bmi.bmiHeader.biClrUsed != 0

        return ToBitmap(image, mask);
      }
      finally {
        if (hdc != IntPtr.Zero)
          gdi32.DeleteDC(hdc);
      }
    }

    [CLSCompliant(false)]
    public static Bitmap FromIconImage(ICONIMAGE iconImage)
    {
      var colorDepth = GetColorDepthOrThrowException(iconImage.icHeader.biBitCount);
      var width  = iconImage.icHeader.biWidth;
      var height = iconImage.icHeader.biHeight / 2;

      var stride = (((iconImage.icHeader.biWidth * iconImage.icHeader.biPlanes * iconImage.icHeader.biBitCount) + 31) & ~31) / 8;

      /*
       * image
       */
      var image = new RGBQUAD[width, height];

      for (var y = 0; y < height; y++) {
        var offset = stride * (height - 1 - y);

        for (var x = 0; x < width; x++) {
          switch (colorDepth) {
            case ColorDepth.Depth1Bit:
              image[x, y] = iconImage.icColors[((iconImage.icXor[offset + x / 8] >> (7 - x % 8)) & 0x1)];
              image[x, y].rgbReserved = 0xff;
              break;

            case ColorDepth.Depth4Bit:
              image[x, y] = iconImage.icColors[((iconImage.icXor[offset + x / 2] >> ((1 - x % 2) << 2)) & 0xf)];
              image[x, y].rgbReserved = 0xff;
              break;

            case ColorDepth.Depth8Bit:
              image[x, y] = iconImage.icColors[iconImage.icXor[offset + x]];
              image[x, y].rgbReserved = 0xff;
              break;

            case ColorDepth.Depth16Bit:
              throw new NotImplementedException("16BPP is not implemented");

            case ColorDepth.Depth24Bit: {
              var index = offset + x * 3;
              image[x, y] = new RGBQUAD(iconImage.icXor[index++], iconImage.icXor[index++], iconImage.icXor[index++], 0xff);
              break;
            }

            case ColorDepth.Depth32Bit: {
              var index = offset + x * 4;
              image[x, y] = new RGBQUAD(iconImage.icXor[index++], iconImage.icXor[index++], iconImage.icXor[index++], iconImage.icXor[index++]);
              break;
            }

            default:
              throw new NotSupportedException();
          }
        } // for x
      } // for y

      /*
       * mask
       */
      var mask = new byte[width, height];

      stride = ((iconImage.icHeader.biWidth + 31) & ~31) / 8;

      for (var y = 0; y < height; y++) {
        var offset = stride * (height - 1 - y);

        for (var x = 0; x < width; x++) {
          //mask[x, y] = ((byte)(((iconImage.icAnd[offset + x / 8]) >> (7 - x % 8)) & 0x1) == 0) ? (byte)0xff : (byte)0x00;
          mask[x, y] = (byte)((((iconImage.icAnd[offset + x / 8]) >> (7 - x % 8)) & 0x1) - 1);
        }
      }

      return ToBitmap(image, mask);
    }

    public static IEnumerable<Bitmap> FromStream(Stream stream)
    {
      return FromStream(stream, true, false);
    }

    /*
     * http://www14.ocn.ne.jp/~setsuki/ext/ico.htm
     * http://www.syuhitu.org/other/ico.html
     */
    public static IEnumerable<Bitmap> FromStream(Stream stream, bool ignoreUnsupportedEntry, bool ignoreInvalidEntry)
    {
      var icondir = ReadIconDir(stream);

      return ReadIconDirEntries(stream, icondir, ignoreUnsupportedEntry, ignoreInvalidEntry);
    }

    internal static ICONDIR ReadIconDir(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      var reader = new System.IO.BinaryReader(stream);
      var icondir = new ICONDIR();

      icondir.idReserved  = reader.ReadUInt16();
      icondir.idType      = reader.ReadUInt16();
      icondir.idCount     = reader.ReadUInt16();

      if (icondir.idType != 1/*icon*/ && icondir.idType != 2 /*cursor*/)
        throw new InvalidDataException("stream is not icon");

      if (icondir.idCount <= 0 || 0x400 < icondir.idCount)
        throw new InvalidDataException("invalid icon count");

      return icondir;
    }

    private static IEnumerable<Bitmap> ReadIconDirEntries(Stream stream, ICONDIR icondir, bool ignoreUnsupportedEntry, bool ignoreInvalidEntry)
    {
      var reader = new System.IO.BinaryReader(stream);
      var entries = new List<ICONDIRENTRY>();

      for (var index = 0; index < icondir.idCount; index++) {
        var entry = new ICONDIRENTRY();

        entry.bWidth        = reader.ReadByte();
        entry.bHeight       = reader.ReadByte();
        entry.bColorCount   = reader.ReadByte();
        entry.bReserved     = reader.ReadByte();
        entry.wPlanes       = reader.ReadUInt16();
        entry.wBitCount     = reader.ReadUInt16();
        entry.dwBytesInRes  = reader.ReadUInt32();
        entry.dwImageOffset = reader.ReadUInt32();

        entries.Add(entry);
      }

      var ret = new List<Bitmap>();

      foreach (var entry in entries) {
        try {
          var resourceReader = new System.IO.BinaryReader(new PartialStream(stream, entry.dwImageOffset, entry.dwBytesInRes, true, true));

          var bih = new BITMAPINFOHEADER();

          bih.biSize          = resourceReader.ReadUInt32();
          bih.biWidth         = resourceReader.ReadInt32();
          bih.biHeight        = resourceReader.ReadInt32();
          bih.biPlanes        = resourceReader.ReadUInt16();
          bih.biBitCount      = resourceReader.ReadUInt16();
          bih.biCompression   = resourceReader.ReadUInt32();
          bih.biSizeImage     = resourceReader.ReadUInt32();
          bih.biXPelsPerMeter = resourceReader.ReadInt32();
          bih.biYPelsPerMeter = resourceReader.ReadInt32();
          bih.biClrUsed       = resourceReader.ReadUInt32();
          bih.biClrImportant  = resourceReader.ReadUInt32();

          if (bih.biCompression != 0)
            throw new NotSupportedException("compressed bitmap is not supported");

          var image = new ICONIMAGE();

          image.icHeader = bih;

          if (bih.biClrUsed == 0) {
            if (bih.biBitCount < 16)
              image.icColors = new RGBQUAD[2 << (bih.biBitCount - 1)];
            else
              image.icColors = new RGBQUAD[0];
          }
          else {
            image.icColors = new RGBQUAD[bih.biClrUsed];
          }

          for (var paletteIndex = 0; paletteIndex < image.icColors.Length; paletteIndex++) {
            image.icColors[paletteIndex] = (RGBQUAD)resourceReader.ReadUInt32();
          }

          var height = bih.biHeight / 2;
          var stride = (((bih.biWidth * bih.biPlanes * bih.biBitCount) + 31) & ~31) / 8;

          image.icXor = resourceReader.ReadBytes(stride * height);
          image.icAnd = resourceReader.ReadBytes(stride * height);

          //icondir.idImages[index] = image;

          ret.Add(FromIconImage(image));
        }
        catch (IOException ex) {
          if (!ignoreInvalidEntry)
            throw new InvalidDataException("stream contains invalid icon", ex);
        }
        catch (InvalidDataException) {
          if (!ignoreInvalidEntry)
            throw;
        }
        catch (NotImplementedException) {
          if (!ignoreUnsupportedEntry)
            throw;
        }
        catch (NotSupportedException) {
          if (!ignoreUnsupportedEntry)
            throw;
        }
      }

      return ret;
    }

    private static Bitmap ToBitmap(RGBQUAD[,] image, byte[,] mask)
    {
      var width  = image.GetLength(0);
      var height = image.GetLength(1);
      var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

      using (var locked = new LockedBitmap(bitmap)) {
        unsafe {
          locked.ForEachScanLine(delegate(void* scanLine, int y, int w) {
            var pixel = (byte*)scanLine;

            for (var x = 0; x < w; x++) {
              *(pixel++) = image[x, y].rgbBlue;
              *(pixel++) = image[x, y].rgbGreen;
              *(pixel++) = image[x, y].rgbRed;
              *(pixel++) = (byte)(image[x, y].rgbReserved & mask[x, y]);
            }
          });
        }
      }

      return bitmap;
    }

#region "extraction"
    /*
     * these codes are based on SantaMarta.Win32APIWrapper.dll
     */

    private static IntPtr GetHInstance()
    {
      return Marshal.GetHINSTANCE(System.Reflection.Assembly.GetEntryAssembly().GetModules()[0]);
    }

    /// <summary>
    /// パスで指定されたファイルまたはフォルダに関連づけられたアイコンの総数を取得します。
    /// </summary>
    /// <param name="path">対象のファイルまたはフォルダのパス</param>
    /// <returns>含まれるアイコンの数。</returns>
    public static int GetCount(string path)
    {
      if (!Runtime.IsRunningOnWindows)
        throw new PlatformNotSupportedException();
      if (path == null)
        throw new ArgumentNullException("path");
      if (!File.Exists(path) && !Directory.Exists(path))
        throw new FileNotFoundException("directory or file not found", path);

      var ret = shell32.ExtractIcon(GetHInstance(), path, -1);

      if (ret == IntPtr.Zero)
        return 0;
      else
        return ret.ToInt32();
    }

    public static Bitmap Extract(string path)
    {
      return Extract(path, 0, IconSize.Large);
    }

    public static Bitmap Extract(string path, IconSize iconSize)
    {
      return Extract(path, 0, iconSize);
    }

    public static Bitmap Extract(string path, int index)
    {
      return Extract(path, index, IconSize.Large);
    }

    public static Bitmap Extract(string path, int index, IconSize iconSize)
    {
      var icons = Extract(path, index, 1, iconSize, true);

      if (icons.Length == 0)
        return null;
      else
        return icons[0];
    }

    public static Bitmap[] ExtractAll(string path)
    {
      return ExtractAll(path, IconSize.Large, true);
    }

    public static Bitmap[] ExtractAll(string path, bool ignoreUnsupported)
    {
      return ExtractAll(path, IconSize.Large, ignoreUnsupported);
    }

    public static Bitmap[] ExtractAll(string path, IconSize iconSize)
    {
      return ExtractAll(path, iconSize, true);
    }

    public static Bitmap[] ExtractAll(string path, IconSize iconSize, bool ignoreUnsupported)
    {
      var count = GetCount(path);

      if (count == 0)
        return new Bitmap[] {};
      else
        return Extract(path, 0, GetCount(path), iconSize, ignoreUnsupported);
    }

    public static Bitmap[] Extract(string path, int index, int count)
    {
      return Extract(path, index, count, IconSize.Large, true);
    }

    public static Bitmap[] Extract(string path, int index, int count, bool ignoreUnsupported)
    {
      return Extract(path, index, count, IconSize.Large, ignoreUnsupported);
    }

    public static Bitmap[] Extract(string path, int index, int count, IconSize iconSize)
    {
      return Extract(path, index, count, iconSize, true);
    }

    /// <summary>パスで指定されたファイルまたはフォルダからアイコンを抜き出します。</summary>
    /// <param name="path">対象のファイルまたはフォルダのパス</param>
    /// <param name="index">抜き出すアイコンの最初のインデックス</param>
    /// <param name="count">抜き出すアイコンの数</param>
    /// <param name="iconSize">抜き出すアイコンのサイズ</param>
    /// <param name="ignoreUnsupported">サポートしていない形式を無視するかどうか</param>
    /// <returns>抜き出されたアイコンの<see cref="Bitmap"/>表現。</returns>
    public static Bitmap[] Extract(string path, int index, int count, IconSize iconSize, bool ignoreUnsupported)
    {
      if (!Runtime.IsRunningOnWindows)
        throw new PlatformNotSupportedException();
      if (path == null)
        throw new ArgumentNullException("path");
      if (!File.Exists(path) && !Directory.Exists(path))
        throw new FileNotFoundException("directory or file not found", path);
      if (index < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("index", index);
      if (count <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("count", count);

      var hIcons = new IntPtr[count];

      var extractedIconCount = (iconSize == IconSize.Large)
        ? shell32.ExtractIconEx(path, index, hIcons, null, count)
        : shell32.ExtractIconEx(path, index, null, hIcons, count);

      try {
        if (count != extractedIconCount)
          Array.Resize(ref hIcons, extractedIconCount);

        return Array.ConvertAll(hIcons, delegate(IntPtr hIcon) {
          try {
            return FromHIcon(hIcon);
          }
          catch (NotSupportedException) {
            if (ignoreUnsupported)
              return null;
            else
              throw;
          }
        });
      }
      finally {
        for (var i = 0; i < extractedIconCount; i++) {
          if (hIcons[i] == IntPtr.Zero)
            continue;

          user32.DestroyIcon(hIcons[i]);

          hIcons[i] = IntPtr.Zero;
        }
      }
    }

    public static Bitmap ExtractAssociated(string path)
    {
      return ExtractAssociated(path, 0);
    }

    /// <summary>
    /// パスで指定されたファイルまたはフォルダに関連付けられたアイコンを取得する
    /// </summary>
    /// <param name="path">対象のファイルまたはフォルダのパス</param>
    /// <param name="index">アイコンのインデックス</param>
    /// <param name="hInstance">HINSTANCE</param>
    /// <returns>抜き出された<see cref="Icon"/>。</returns>
    public static Bitmap ExtractAssociated(string path, int index)
    {
      if (!Runtime.IsRunningOnWindows)
        throw new PlatformNotSupportedException();
      if (path == null)
        throw new ArgumentNullException("path");
      if (!File.Exists(path) && !Directory.Exists(path))
        throw new FileNotFoundException("directory or file not found", path);
      if (index < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("index", index);

      var hIcon = IntPtr.Zero;

      try {
        hIcon = shell32.ExtractAssociatedIcon(GetHInstance(), path, ref index);

        if (hIcon == IntPtr.Zero)
          return null;
        else
          return FromHIcon(hIcon);
      }
      finally {
        if (hIcon != IntPtr.Zero) {
          user32.DestroyIcon(hIcon);
          hIcon = IntPtr.Zero;
        }
      }
    }
#endregion
  }
}
