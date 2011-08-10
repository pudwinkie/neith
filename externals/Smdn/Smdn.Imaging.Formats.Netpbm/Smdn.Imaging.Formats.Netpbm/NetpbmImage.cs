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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using Smdn.Formats;
using Smdn.Imaging;

namespace Smdn.Imaging.Formats.Netpbm {
  public static class NetpbmImage {
#region "writing"
    public static void WriteTo(Bitmap bitmap, Stream stream, NetpbmFormat format)
    {
      throw new NotImplementedException();
    }
#endregion

#region "reading"
    private class AsciiPixmapBinaryReader : BinaryReader {
      public AsciiPixmapBinaryReader(BinaryReader reader)
        : base(reader.BaseStream)
      {
      }

      public override byte ReadByte()
      {
        for (;;) {
          var ch = base.PeekChar();

          if (ch == -1)
            throw new EndOfStreamException();

          if (ch == Octets.CR ||
              ch == Octets.LF ||
              ch == Octets.SP ||
              ch == Octets.HT ||
              ch == 0x0b /* VT */ ||
              ch == 0x0c /* FF */) {
            base.ReadByte();
            continue; // whitespace
          }
          else {
            break;
          }
        }

        var val = 0;

        for (;;) {
          var ch = base.PeekChar();

          if (ch == -1)
            throw new EndOfStreamException();
          else if (0x30 <= ch && ch <= 0x39)
            val = (val * 10) + (base.ReadByte() - 0x30);
          else
            break;
        }

        if (byte.MaxValue < val)
          throw new InvalidDataException();

        return (byte)val;
      }
    }

    private class AsciiBitmapBinaryReader : BinaryReader {
      public AsciiBitmapBinaryReader(BinaryReader reader)
        : base(reader.BaseStream)
      {
      }

      public byte ReadByte(int bits)
      {
        var val = 0;
        var shift = 7;

        for (;;) {
          var b = base.ReadByte();

          if (b == Octets.CR ||
              b == Octets.LF ||
              b == Octets.SP ||
              b == Octets.HT ||
              b == 0x0b /* VT */ ||
              b == 0x0c /* FF */) {
            continue; // whitespace
          }
          else if (b == 0x30 || b == 0x31) {
            val |= (b - 0x30) << shift;

            if (--bits == 0)
              return (byte)val;
            else if (shift-- == 0)
              shift = 7;
          }
          else {
            throw new InvalidDataException();
          }
        }
      }
    }

    public static Bitmap LoadFrom(string file)
    {
      NetpbmFormat discard;

      return LoadFrom(file, out discard);
    }

    public static Bitmap LoadFrom(string file, out NetpbmFormat format)
    {
      using (var stream = File.OpenRead(file)) {
        return LoadFrom(stream, out format);
      }
    }

    public static Bitmap LoadFrom(Stream stream)
    {
      NetpbmFormat discard;

      return LoadFrom(stream, out discard);
    }

    public static Bitmap LoadFrom(Stream stream, out NetpbmFormat format)
    {
      int width, height, depth;

      ReadHeader(stream, out format, out width, out height, out depth);

      return ReadImage(stream, format, width, height, depth);
    }

    internal static void ReadHeader(Stream stream, out NetpbmFormat format, out int width, out int height, out int depth)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      format = NetpbmFormat.Unknown;

      var reader = new BinaryReader(stream);

      if (reader.ReadByte() != 0x50) // 'P'
        throw new InvalidDataException("stream is not PNM");

      var pn = reader.ReadByte();

      if (0x31 <= pn && pn <= 0x37) // '1' to '7'
        format = NetpbmFormat.P1 + (pn - 0x31);
      else
        throw new InvalidDataException("stream is not PNM");

      var bitmap = (format == NetpbmFormat.PbmAscii || format == NetpbmFormat.PbmBinary);
      var headerEntries = new List<string>();
      var currentHeaderEntry = new StringBuilder();
      var commentLine = false;

      try {
        for (;;) {
          var b = reader.ReadByte();

          if (commentLine) {
            if (b == Octets.CR || b == Octets.LF)
              commentLine = false;
            continue;
          }

          if (b == 0x23) { // '#'
            commentLine = true;
            continue;
          }

          if (b == Octets.CR || b == Octets.LF || b == Octets.HT || b == Octets.SP) { // whitespace(CR, LF, TAB, SP)
            headerEntries.Add(currentHeaderEntry.ToString().Trim());
            currentHeaderEntry.Length = 0;

            if ((bitmap && headerEntries.Count == 3) || // bitmap has no depth
                (!bitmap && headerEntries.Count == 4)) // graymap and pixmap has depth
              break;
            else
              continue;
          }
          else {
            currentHeaderEntry.Append((char)b);
          }
        }
      }
      catch (EndOfStreamException) {
        // unexpected EOF
        throw new InvalidDataException("invalid PNM header");
      }

      if (headerEntries.Count < (bitmap ? 3 : 4))
        throw new InvalidDataException("invalid PNM header");

      if (!int.TryParse(headerEntries[1], out width))
        throw new InvalidDataException("invalid PNM header (width)");
      if (!int.TryParse(headerEntries[2], out height))
        throw new InvalidDataException("invalid PNM header (heigth)");

      depth = 1;

      if (!bitmap && !int.TryParse(headerEntries[3], out depth))
        throw new InvalidDataException("invalid PNM header (depth)");
    }

    private static Bitmap ReadImage(Stream stream, NetpbmFormat format, int width, int height, int depth)
    {
      var reader = new BinaryReader(stream);

      Bitmap ret;

      switch (format) {
        case NetpbmFormat.PbmAscii: {
          var r = new AsciiBitmapBinaryReader(reader);

          ret = new Bitmap(width, height, PixelFormat.Format1bppIndexed);

          var palette = ret.Palette;

          palette.Entries[0] = Color.White;
          palette.Entries[1] = Color.Black;

          ret.Palette = palette;

          using (var locked = new LockedBitmap(ret, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed)) {
            unsafe {
              locked.ForEachScanLine(delegate(void* line, int y, int w) {
                var pixel = (byte*)line;
                var x = 0;

                for (;;) {
                  if (x + 8 < w) {
                    *(pixel++) = r.ReadByte(8);
                    x += 8;
                  }
                  else {
                    *(pixel++) = r.ReadByte(w - x);
                    break;
                  }
                }
              });
            }
          }

          break;
        }

        case NetpbmFormat.PbmBinary: {
          ret = new Bitmap(width, height, PixelFormat.Format1bppIndexed);

          var palette = ret.Palette;

          palette.Entries[0] = Color.White;
          palette.Entries[1] = Color.Black;

          ret.Palette = palette;

          using (var locked = new LockedBitmap(ret, ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed)) {
            unsafe {
              locked.ForEachScanLine(delegate(void* line, int y, int w) {
                var pixel = (byte*)line;

                for (var x = 0; x < w; x += 8) {
                  *(pixel++) = reader.ReadByte();
                }
              });
            }
          }

          break;
        }

        case NetpbmFormat.PgmAscii:
          reader = new AsciiPixmapBinaryReader(reader);
          goto case NetpbmFormat.PgmBinary;
        case NetpbmFormat.PgmBinary: {
          ret = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

          var palette = ret.Palette;

          for (var index = 0; index < depth; index++) {
            var i = (0xff * index) / depth;

            palette.Entries[index] = Color.FromArgb(i, i, i);
          }

          ret.Palette = palette;

          using (var locked = new LockedBitmap(ret, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed)) {
            unsafe {
              locked.ForEachScanLine(delegate(void* line, int y, int w) {
                var pixel = (byte*)line;

                for (var x = 0; x < w; x++) {
                  *(pixel++) = reader.ReadByte();
                }
              });
            }
          }

          break;
        }

        case NetpbmFormat.PpmAscii:
          reader = new AsciiPixmapBinaryReader(reader);
          goto case NetpbmFormat.PpmBinary;
        case NetpbmFormat.PpmBinary: {
          ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);

          using (var locked = new LockedBitmap(ret, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb)) {
            if (depth == 0xff) {
              unsafe {
                locked.ForEachScanLine(delegate(void* line, int y, int w) {
                  var bgr = (byte*)line;

                  for (var x = 0; x < w; x++) {
                    var r = reader.ReadByte();
                    var g = reader.ReadByte();

                    *(bgr++) = reader.ReadByte();
                    *(bgr++) = g;
                    *(bgr++) = r;
                  }
                });
              }
            }
            else {
              unsafe {
                locked.ForEachScanLine(delegate(void* line, int y, int w) {
                  var bgr = (byte*)line;

                  for (var x = 0; x < w; x++) {
                    var r = (byte)(0xff * reader.ReadByte() / depth);
                    var g = (byte)(0xff * reader.ReadByte() / depth);

                    *(bgr++) = (byte)(0xff * reader.ReadByte() / depth);
                    *(bgr++) = g;
                    *(bgr++) = r;
                  }
                });
              }
            } // else if depth != 0xff
          } // using locked

          break;
        }

        case NetpbmFormat.Pam:
          throw new NotImplementedException("PAM is not implemented");

        default:
          throw new InvalidDataException("unsupported or unknown format");
      }

      return ret;
    }
#endregion
  }
}
