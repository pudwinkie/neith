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

namespace Smdn.Imaging {
  public class Codecs : IImageCodecs {
    private class ImageEncoder : IImageEncoder {
      public Guid Guid {
        get { return info.FormatID; }
      }

      public string Name {
        get { return info.FormatDescription; }
      }

      public string[] Extensions {
        get; private set;
      }

      public MimeType MimeType {
        get { return new MimeType(info.MimeType); }
      }

      public ImageEncoder(ImageFormat format)
      {
        this.info = null;

        foreach (var i in ImageCodecInfo.GetImageEncoders()) {
          if (i.FormatID == format.Guid) {
            this.info = i;
            break;
          }
        }

        if (this.info == null)
          throw new NotSupportedException("unsupported image format");

        this.Extensions = Array.ConvertAll(this.info.FilenameExtension.Split(';'), delegate(string ext) {
          if (ext.StartsWith("*."))
            ext = ext.Substring(1);
          return ext.ToLower();
        });
      }

      public void Encode(Bitmap bitmap, Stream stream, EncoderParameters encoderParams)
      {
        bitmap.Save(stream, info, encoderParams);
      }

      private ImageCodecInfo info;
    }

    public IEnumerable<IImageDecoder> CreateDecoders()
    {
      return new IImageDecoder[] {};
    }

    public IEnumerable<IImageEncoder> CreateEncoders()
    {
      return new IImageEncoder[] {
        new ImageEncoder(ImageFormat.Bmp),
        new ImageEncoder(ImageFormat.Jpeg),
        new ImageEncoder(ImageFormat.Png),
        new ImageEncoder(ImageFormat.Gif),
        new ImageEncoder(ImageFormat.Tiff),
      };
    }
  }
}
