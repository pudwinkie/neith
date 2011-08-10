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
  public static class BitmapExtensions {
    public static Bitmap LoadFrom(string file)
    {
      return LoadFrom(file, null, false);
    }

    public static Bitmap LoadFrom(string file, IImageDecoder decoder)
    {
      return LoadFrom(file, decoder, false);
    }

    public static Bitmap LoadFrom(string file, bool useIcm)
    {
      return LoadFrom(file, null, useIcm);
    }

    public static Bitmap LoadFrom(string file, IImageDecoder decoder, bool useIcm)
    {
      using (var stream = File.OpenRead(file)) {
        return LoadFrom(stream, decoder, useIcm);
      }
    }

    public static Bitmap LoadFrom(Stream stream)
    {
      return LoadFrom(stream, null, false);
    }

    public static Bitmap LoadFrom(Stream stream, bool useIcm)
    {
      return LoadFrom(stream, null, useIcm);
    }

    public static Bitmap LoadFrom(Stream stream, IImageDecoder decoder)
    {
      return LoadFrom(stream, decoder, false);
    }

    public static Bitmap LoadFrom(Stream stream, IImageDecoder decoder, bool useIcm)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      if (decoder == null && stream.CanSeek)
        decoder = ImageCodecs.GuessDecoderFromStream(stream);

      return (decoder ?? ImageCodecs.GetFallbackDecoder()).Decode(stream, useIcm);
    }

    public static void SaveTo(this Bitmap bitmap, string filename)
    {
      Save(bitmap, filename, null, (EncoderParameters)null);
    }

    public static void SaveTo(this Bitmap bitmap, string filename, IEnumerable<EncoderParameter> encoderParams)
    {
      Save(bitmap, filename, null, encoderParams);
    }

    public static void SaveTo(this Bitmap bitmap, string filename, EncoderParameters encoderParams)
    {
      Save(bitmap, filename, null, encoderParams);
    }

    public static void Save(this Bitmap bitmap, string filename, IImageEncoder encoder)
    {
      Save(bitmap, filename, encoder, (EncoderParameters)null);
    }

    public static void Save(this Bitmap bitmap, string filename, IImageEncoder encoder, IEnumerable<EncoderParameter> encoderParams)
    {
      using (var encParams = CreateEncoderParameters(encoderParams)) {
        Save(bitmap, filename, encoder, encParams);
      }
    }

    public static void Save(this Bitmap bitmap, string filename, IImageEncoder encoder, EncoderParameters encoderParams)
    {
      if (filename == null)
        throw new ArgumentNullException("filename");

      using (var stream = File.OpenWrite(filename)) {
        stream.SetLength(0L);

        Save(bitmap, stream, encoder ?? ImageCodecs.GuessEncoderFromExtension(filename), encoderParams);

        stream.Flush();
      }
    }

    public static void SaveTo(this Bitmap bitmap, Stream stream)
    {
      Save(bitmap, stream, null, (EncoderParameters)null);
    }

    public static void SaveTo(this Bitmap bitmap, Stream stream, IEnumerable<EncoderParameter> encoderParams)
    {
      Save(bitmap, stream, null, encoderParams);
    }

    public static void SaveTo(this Bitmap bitmap, Stream stream, EncoderParameters encoderParams)
    {
      Save(bitmap, stream, null, encoderParams);
    }

    public static void Save(this Bitmap bitmap, Stream stream, IImageEncoder encoder)
    {
      Save(bitmap, stream, encoder, (EncoderParameters)null);
    }

    public static void Save(this Bitmap bitmap, Stream stream, IImageEncoder encoder, IEnumerable<EncoderParameter> encoderParams)
    {
      using (var encParams = CreateEncoderParameters(encoderParams)) {
        Save(bitmap, stream, encoder, encParams);
      }
    }

    public static void Save(this Bitmap bitmap, Stream stream, IImageEncoder encoder, EncoderParameters encoderParams)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      (encoder ?? ImageCodecs.GetFallbackEncoder()).Encode(bitmap, stream, encoderParams);
    }

    private static EncoderParameters CreateEncoderParameters(IEnumerable<EncoderParameter> encoderParams)
    {
      if (encoderParams == null)
        return null;

      var encParams = new EncoderParameters();

      encParams.Param = (encoderParams is EncoderParameter[])
        ? encoderParams as EncoderParameter[]
        : (new List<EncoderParameter>(encoderParams)).ToArray();

      return encParams;
    }
  }
}
