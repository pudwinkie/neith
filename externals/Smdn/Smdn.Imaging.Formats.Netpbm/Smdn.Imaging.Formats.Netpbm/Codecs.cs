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

namespace Smdn.Imaging.Formats.Netpbm {
  public class Codecs : IImageCodecs {
    private class NetpbmImageDecoder : IImageDecoder {
      public Guid Guid {
        get { return ImageCodecs.Guids.Netpbm; }
      }

      public string Name {
        get { return "Portable Anymap"; }
      }

      public string[] Extensions {
        get { return new[] {".pnm", ".pbm", ".pgm", ".ppm", ".pam"}; }
      }

      public MimeType MimeType {
        get { return MimeType.CreateImageType("x-portable-anymap"); }
      }

      public bool GetImageFormat(Stream stream, out int? imageCount, out int? width, out int? height)
      {
        imageCount = null;
        width = null;
        height = null;

        try {
          NetpbmFormat format;
          int w, h, depth;

          NetpbmImage.ReadHeader(stream, out format, out w, out h, out depth);

          imageCount = 1;
          width = w;
          height = h;

          return true;
        }
        catch (InvalidDataException) {
          return false;
        }
      }

      public Bitmap Decode(Stream stream, bool useIcm)
      {
        return NetpbmImage.LoadFrom(stream);
      }
    }

    private class NetpbmImageEncoder : IImageEncoder {
      public Guid Guid {
        get; private set;
      }

      public string Name {
        get; private set;
      }

      public string[] Extensions {
        get; private set;
      }

      public MimeType MimeType {
        get; private set;
      }

      public NetpbmImageEncoder(NetpbmFormat format)
      {
        switch (format) {
          case NetpbmFormat.PbmAscii:
            this.Guid = ImageCodecs.Guids.PbmAscii;
            this.Name = "Portable Bitmap (Ascii)";
            this.Extensions = new[] {".pbm"};
            this.MimeType = MimeType.CreateImageType("x-portable-bitmap");
            break;
          case NetpbmFormat.PgmAscii:
            this.Guid = ImageCodecs.Guids.PgmAscii;
            this.Name = "Portable Graymap (Ascii)";
            this.Extensions = new[] {".pgm"};
            this.MimeType = MimeType.CreateImageType("x-portable-graymap");
            break;
          case NetpbmFormat.PpmAscii:
            this.Guid = ImageCodecs.Guids.PpmAscii;
            this.Name = "Portable Pixmap (Ascii)";
            this.Extensions = new[] {".ppm"};
            this.MimeType = MimeType.CreateImageType("x-portable-pixmap");
            break;
          case NetpbmFormat.PbmBinary:
            this.Guid = ImageCodecs.Guids.PbmBinary;
            this.Name = "Portable Bitmap (Binary)";
            this.Extensions = new[] {".pbm"};
            this.MimeType = MimeType.CreateImageType("x-portable-bitmap");
            break;
          case NetpbmFormat.PgmBinary:
            this.Guid = ImageCodecs.Guids.PgmBinary;
            this.Name = "Portable Graymap (Binary)";
            this.Extensions = new[] {".pgm"};
            this.MimeType = MimeType.CreateImageType("x-portable-graymap");
            break;
          case NetpbmFormat.PpmBinary:
            this.Guid = ImageCodecs.Guids.PpmBinary;
            this.Name = "Portable Pixmap (Binary)";
            this.Extensions = new[] {".ppm"};
            this.MimeType = MimeType.CreateImageType("x-portable-pixmap");
            break;
          case NetpbmFormat.Pam:
            this.Guid = ImageCodecs.Guids.Pam;
            this.Name = "Portable Arbitrary Map";
            this.Extensions = new[] {".pam"};
            this.MimeType = null; // XXX
            break;
          default:
            throw new ArgumentOutOfRangeException("format");
        }
      }

      public void Encode(Bitmap bitmap, Stream stream, EncoderParameters encoderParams)
      {
        NetpbmImage.WriteTo(bitmap, stream, format);
      }

      private NetpbmFormat format;
    }

    public IEnumerable<IImageDecoder> CreateDecoders()
    {
      return new IImageDecoder[] {
        new NetpbmImageDecoder(),
      };
    }

    public IEnumerable<IImageEncoder> CreateEncoders()
    {
      return new IImageEncoder[] {
        new NetpbmImageEncoder(NetpbmFormat.PbmAscii),
        new NetpbmImageEncoder(NetpbmFormat.PgmAscii),
        new NetpbmImageEncoder(NetpbmFormat.PpmAscii),
        new NetpbmImageEncoder(NetpbmFormat.PbmBinary),
        new NetpbmImageEncoder(NetpbmFormat.PgmBinary),
        new NetpbmImageEncoder(NetpbmFormat.PpmBinary),
        new NetpbmImageEncoder(NetpbmFormat.Pam),
      };
    }
  }
}
