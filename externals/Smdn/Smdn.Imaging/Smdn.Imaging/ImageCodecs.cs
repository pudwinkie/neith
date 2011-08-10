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
using System.Reflection;

namespace Smdn.Imaging {
  public static class ImageCodecs {
    public static class Guids {
      public static readonly Guid Fallback  = new Guid("e1b78d5a-c079-5292-b658-7b9e77b9d252"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#Fallback
      public static readonly Guid Ico       = new Guid("0b018592-e46f-55b5-bd71-983a919388c0"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#Ico
      public static readonly Guid Netpbm    = new Guid("20aa9adb-53cd-57e1-b9b6-c52e30d80c92"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#Netpbm
      public static readonly Guid PbmAscii  = new Guid("7c37c42b-fed9-5eee-a64f-317d9fe1458b"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#PbmAscii
      public static readonly Guid PgmAscii  = new Guid("54e6d0df-3a47-5bbe-b0bd-b508f45ed217"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#PgmAscii
      public static readonly Guid PpmAscii  = new Guid("14a3b254-8837-53ce-93d9-45d6bf5819e2"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#PpmAscii
      public static readonly Guid PbmBinary = new Guid("ea71ad71-2651-5426-97bd-7033b2bf0ff5"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#PbmBinary
      public static readonly Guid PgmBinary = new Guid("bdb72cca-1247-5f22-a5c3-7d0b76bccc20"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#PgmBinary
      public static readonly Guid PpmBinary = new Guid("40ff51bf-46b3-548e-b355-5f62c91f751c"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#PpmBinary
      public static readonly Guid Pam       = new Guid("6962ae21-a820-5437-87ff-e5f60f64f371"); // uuid -v5 ns:URL http://smdn.jp/works/libs/Smdn.Imaging/uuid/#Pam
    }

    public static class Decoders {
      // TODO: impl
    }

    public static class Encoders {
      public static IImageEncoder Bmp   { get { return ImageCodecs.GetEncoder(ImageFormat.Bmp.Guid); } }
      public static IImageEncoder Jpeg  { get { return ImageCodecs.GetEncoder(ImageFormat.Jpeg.Guid); } }
      public static IImageEncoder Png   { get { return ImageCodecs.GetEncoder(ImageFormat.Png.Guid); } }
      public static IImageEncoder Gif   { get { return ImageCodecs.GetEncoder(ImageFormat.Gif.Guid); } }
      public static IImageEncoder Tiff  { get { return ImageCodecs.GetEncoder(ImageFormat.Tiff.Guid); } }

      public static IImageEncoder Ico       { get { return ImageCodecs.GetEncoder(Guids.Ico); } }
      public static IImageEncoder PbmAscii  { get { return ImageCodecs.GetEncoder(Guids.PbmAscii); } }
      public static IImageEncoder PgmAscii  { get { return ImageCodecs.GetEncoder(Guids.PgmAscii); } }
      public static IImageEncoder PpmAscii  { get { return ImageCodecs.GetEncoder(Guids.PpmAscii); } }
      public static IImageEncoder PbmBinary { get { return ImageCodecs.GetEncoder(Guids.PbmBinary); } }
      public static IImageEncoder PgmBinary { get { return ImageCodecs.GetEncoder(Guids.PgmBinary); } }
      public static IImageEncoder PpmBinary { get { return ImageCodecs.GetEncoder(Guids.PpmBinary); } }
    }

#region "fallback image codec"
    private class FallbackImageCodec : IImageCodec {
      public Guid Guid {
        get { return Guids.Fallback; }
      }

      public string Name {
        get { return "Fallback Codec"; }
      }

      public string[] Extensions {
        get { return new[] {".bmp", ".png", ".jpg", ".jpeg", ".gif", ".tiff", ".ico"}; }
      }

      public MimeType MimeType {
        get { return null; }
      }
    }

    private class FallbackImageDecoder : FallbackImageCodec, IImageDecoder {
      public bool GetImageFormat(Stream stream, out int? imageCount, out int? width, out int? height)
      {
        throw new NotSupportedException();
      }

      public Bitmap Decode(Stream stream, bool useIcm)
      {
        return new Bitmap(stream, useIcm);
      }
    }

    private class FallbackImageEncoder : FallbackImageCodec, IImageEncoder {
      public void Encode(Bitmap bitmap, Stream stream, EncoderParameters encoderParams)
      {
        bitmap.Save(stream, ImageFormat.Png);
      }
    }
#endregion

    private static void Initialize()
    {
      decoders = new Dictionary<Guid, IImageDecoder>();
      encoders = new Dictionary<Guid, IImageEncoder>();

      foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Smdn.Imaging.*.dll", SearchOption.AllDirectories)) {
        Assembly.LoadFrom(file);
      }

      foreach (var assm in AppDomain.CurrentDomain.GetAssemblies()) {
        if (!assm.GetName().Name.StartsWith("Smdn.Imaging"))
          continue;

        foreach (var type in assm.GetExportedTypes()) {
          if (!typeof(IImageCodecs).IsAssignableFrom(type))
            continue;

          IImageCodecs codecs = null;

          try {
            codecs = (IImageCodecs)Activator.CreateInstance(type);
          }
          catch {
            // ignore exceptions
            continue;
          }

          try {
            foreach (var decoder in codecs.CreateDecoders()) {
              decoders.Add(decoder.Guid, decoder);
            }
          }
          catch {
            // ignore exceptions
          }

          try {
            foreach (var encoder in codecs.CreateEncoders()) {
              encoders.Add(encoder.Guid, encoder);
            }
          }
          catch {
            // ignore exceptions
          }
        }
      }

      initialized = true;
    }

    public static IImageDecoder GetFallbackDecoder()
    {
      return new FallbackImageDecoder();
    }

    public static IImageEncoder GetFallbackEncoder()
    {
      return new FallbackImageEncoder();
    }

    public static IImageDecoder GetDecoder(Guid guid)
    {
      if (!initialized)
        Initialize();

      if (decoders.ContainsKey(guid))
        return decoders[guid];
      else
        return null;
    }

    public static IImageEncoder GetEncoder(Guid guid)
    {
      if (!initialized)
        Initialize();

      if (encoders.ContainsKey(guid))
        return encoders[guid];
      else
        return null;
    }

    public static IEnumerable<IImageDecoder> GetDecoders()
    {
      if (!initialized)
        Initialize();

      lock ((decoders as System.Collections.ICollection).SyncRoot) {
        return new List<IImageDecoder>(decoders.Values);
      }
    }

    public static IEnumerable<IImageEncoder> GetEncoders()
    {
      if (!initialized)
        Initialize();

      lock ((encoders as System.Collections.ICollection).SyncRoot) {
        return new List<IImageEncoder>(encoders.Values);
      }
    }

    public static IImageDecoder GuessDecoderFromStream(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (!stream.CanRead)
        throw ExceptionUtils.CreateArgumentMustBeReadableStream("stream");
      if (!stream.CanSeek)
        throw ExceptionUtils.CreateArgumentMustBeSeekableStream("stream");

      var initialPosition = stream.Position;

      try {
        foreach (var decoder in ImageCodecs.GetDecoders()) {
          int? imageCount, width, height;

          try {
            if (decoder.GetImageFormat(stream, out imageCount, out width, out height))
              return decoder;
            else
              stream.Position = initialPosition;
          }
          catch (NotSupportedException) {
            continue;
          }
        }

        return null; // not found
      }
      finally {
        stream.Position = initialPosition;
      }
    }

    public static IImageDecoder GuessDecoderFromExtension(string extensionOrPath)
    {
      return GuessCodecFromExtension(GetDecoders(), extensionOrPath);
    }

    public static IImageEncoder GuessEncoderFromExtension(string extensionOrPath)
    {
      return GuessCodecFromExtension(GetEncoders(), extensionOrPath);
    }

    private static TImageCodec GuessCodecFromExtension<TImageCodec>(IEnumerable<TImageCodec> codecs, string extensionOrPath) where TImageCodec : class, IImageCodec
    {
      foreach (var c in codecs) {
        foreach (var ext in c.Extensions) {
          if (Smdn.IO.PathUtils.AreExtensionEqual(extensionOrPath, ext))
            return c;
        }
      }

      return null;
    }

    public static string[] GetDecoderExtensions()
    {
      var extensions = new List<string>(GetFallbackDecoder().Extensions);

      foreach (var d in GetDecoders()) {
        foreach (var ext in d.Extensions) {
          if (!extensions.Contains(ext))
            extensions.Add(ext);
        }
      }

      return extensions.ToArray();
    }

    public static string GetDecoderExtensionPattern()
    {
      return ConvertExtensionsToPattern(GetDecoderExtensions());
    }

    public static string[] GetEncoderExtensions()
    {
      var extensions = new List<string>(GetFallbackEncoder().Extensions);

      foreach (var e in GetEncoders()) {
        foreach (var ext in e.Extensions) {
          if (!extensions.Contains(ext))
            extensions.Add(ext);
        }
      }

      return extensions.ToArray();
    }

    public static string GetEncoderExtensionPattern()
    {
      return ConvertExtensionsToPattern(GetEncoderExtensions());
    }

    private static string ConvertExtensionsToPattern(string[] extensions)
    {
      return string.Join(";", Array.ConvertAll(extensions, delegate(string ext) {
        return "*" + ext;
      }));
    }

    private static bool initialized = false;
    private static Dictionary<Guid, IImageDecoder> decoders;
    private static Dictionary<Guid, IImageEncoder> encoders;
  }
}
