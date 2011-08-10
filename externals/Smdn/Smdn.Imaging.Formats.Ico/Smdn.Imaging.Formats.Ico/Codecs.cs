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

namespace Smdn.Imaging.Formats.Ico {
  public class Codecs : IImageCodecs {
    private class IconImageDecoder : IImageDecoder {
      public Guid Guid {
        get { return ImageCodecs.Guids.Ico; }
      }

      public string Name {
        get { return "Icon"; }
      }

      public string[] Extensions {
        get { return new[] {".ico", ".cur"}; }
      }

      public MimeType MimeType {
        get { return MimeType.CreateImageType("image/x-icon" /* "vnd.microsoft.icon" */); }
      }

      public bool GetImageFormat(Stream stream, out int? imageCount, out int? width, out int? height)
      {
        imageCount = null;
        width = null;
        height = null;

        try {
          var icondir = Icon.ReadIconDir(stream);

          imageCount = icondir.idCount;

          return true;
        }
        catch (InvalidDataException) {
          return false;
        }
      }

      public Bitmap Decode(Stream stream, bool useIcm)
      {
        foreach (var icon in Icon.FromStream(stream)) {
          return icon;
        }

        throw new InvalidDataException();
      }
    }

    public IEnumerable<IImageDecoder> CreateDecoders()
    {
      return new IImageDecoder[] {
        new IconImageDecoder(),
      };
    }

    public IEnumerable<IImageEncoder> CreateEncoders()
    {
      // TODO
      return new IImageEncoder[] {};
    }
  }
}
