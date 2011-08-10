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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Smdn.Formats.IsoBaseMediaFile;

namespace Smdn.Formats.MP4.MetaData.iTunes {
  public abstract class ArtworkBase : IDisposable {
    public static ArtworkBase CreateFrom(string file)
    {
      using (var image = Image.FromFile(file)) {
        return CreateFrom(image);
      }
    }

    public static ArtworkBase CreateFrom(Image image)
    {
      if (image.RawFormat.Equals(ImageFormat.Jpeg))
        return new JpegArtwork(image);
      else if (image.RawFormat.Equals(ImageFormat.Png))
        return new PngArtwork(image);
      else
        throw new NotSupportedException(string.Format("{0} is not supported. (image must be JPEG or PNG)", image.RawFormat));
    }

    protected static MemoryStream ToStream(Image image, ImageFormat requiredFormat)
    {
      if (!image.RawFormat.Equals(requiredFormat))
        throw new ArgumentException(string.Format("image is not {0}", requiredFormat), "image");

      var stream = new MemoryStream();

      image.Save(stream, requiredFormat);

      return stream;
    }

    internal protected MemoryStream ImageStream {
      get { CheckDisposed(); return imageStream; }
    }

    protected ArtworkBase(DataBlock data)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      this.imageStream = data.ToStream();
    }

    protected ArtworkBase(MemoryStream imageStream)
    {
      if (imageStream == null)
        throw new ArgumentNullException("imageStream");

      this.imageStream = imageStream;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (imageStream != null) {
          imageStream.Close();
          imageStream = null;
        }
      }
    }

    public Image ToImage()
    {
      CheckDisposed();

      return Image.FromStream(imageStream);
    }

    private void CheckDisposed()
    {
      if (imageStream == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private MemoryStream imageStream;
  }
}
