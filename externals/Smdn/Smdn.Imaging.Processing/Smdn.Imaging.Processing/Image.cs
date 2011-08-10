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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Smdn.Imaging.Processing {
  public class Image {
    public readonly int Width;
    public readonly int Height;
    public readonly int PixelCount;
    public readonly uint[] Rgb;

    public byte[] Alpha {
      get { return alpha; }
    }

    public bool EnableAlpha {
      get { return alpha != null; }
      set
      {
        if (value) {
          if (alpha == null)
            alpha = new byte[PixelCount];
        }
        else {
          if (alpha != null)
            alpha = null;
        }
      }
    }

    public Image(int width, int height)
      : this(width, height, false)
    {
    }

    public Image(int width, int height, bool enableAlpha)
    {
      this.Width = width;
      this.Height = height;
      this.PixelCount = width * height;
      this.Rgb = new uint[PixelCount];

      if (enableAlpha)
        this.alpha = new byte[PixelCount];
      else
        this.alpha = null;
    }

    public Image(uint[] rgb, int width, int height)
    {
      if (rgb.Length != width * height)
        throw new ArgumentOutOfRangeException();

      this.Width = width;
      this.Height = height;
      this.PixelCount = width * height;
      this.Rgb = rgb;

      this.alpha = null;
    }

    public Image Filter(PixelFilterFunction filter)
    {
      return Filter(filter, null);
    }

    public Image Filter(PixelFilterFunction filter, FilterArgs args)
    {
      var filtered = new Image(Width, Height);

#if DEBUG
      stopwatch.Reset();
      stopwatch.Start();
#endif

      filter(filtered.Rgb, Rgb, PixelCount, args);

#if DEBUG
      stopwatch.Stop();
      Console.WriteLine("Spent: {0}, Filter: {3}.{2}.{1}",
                        stopwatch.Elapsed,
                        filter.Method.Name,
                        filter.Method.DeclaringType.Name,
                        filter.Method.DeclaringType.Namespace);
#endif

      return filtered;
    }

    public Image Filter(BlockFilterFunction filter)
    {
      return Filter(filter, null);
    }

    public Image Filter(BlockFilterFunction filter, FilterArgs args)
    {
      var filtered = new Image(Width, Height);

#if DEBUG
      stopwatch.Reset();
      stopwatch.Start();
#endif

      filter(filtered.Rgb, Rgb, Width, Height, args);

#if DEBUG
      stopwatch.Stop();
      Console.WriteLine("Spent: {0}, Filter: {3}.{2}.{1}",
                        stopwatch.Elapsed,
                        filter.Method.Name,
                        filter.Method.DeclaringType.Name,
                        filter.Method.DeclaringType.Namespace);
#endif

      return filtered;
    }

    public Image Composite(PixelCompositionFunction composit, Image image)
    {
      if (Width != image.Width)
        throw new InvalidOperationException("different width");
      if (Height != image.Height)
        throw new InvalidOperationException("different height");

      var composited = new Image(Width, Height);

#if DEBUG
      stopwatch.Reset();
      stopwatch.Start();
#endif

      composit(composited.Rgb, PixelCount, Rgb, Alpha, image.Rgb, image.Alpha);

#if DEBUG
      stopwatch.Stop();
      Console.WriteLine("Spent: {0}, Composit: {3}.{2}.{1}",
                        stopwatch.Elapsed,
                        composit.Method.Name,
                        composit.Method.DeclaringType.Name,
                        composit.Method.DeclaringType.Namespace);
#endif

      return composited;
    }

    public Image Composite(BlockCompositionFunction composit, Image image)
    {
      if (Width != image.Width)
        throw new InvalidOperationException("different width");
      if (Height != image.Height)
        throw new InvalidOperationException("different height");

      var composited = new Image(Width, Height);

#if DEBUG
      stopwatch.Reset();
      stopwatch.Start();
#endif

      composit(composited.Rgb, Width, Height, Rgb, Alpha, image.Rgb, image.Alpha);

#if DEBUG
      stopwatch.Stop();
      Console.WriteLine("Spent: {0}, Composit: {3}.{2}.{1}",
                        stopwatch.Elapsed,
                        composit.Method.Name,
                        composit.Method.DeclaringType.Name,
                        composit.Method.DeclaringType.Namespace);
#endif

      return composited;
    }

    /*
    public Image Operate(OperatorFunction @operator, params object[] args)
    {
#if DEBUG
      stopwatch.Reset();
      stopwatch.Start();
#endif

      var operated = @operator(this, args);

#if DEBUG
      stopwatch.Stop();
      Console.WriteLine("Spent: {0}, Operator: {3}.{2}.{1}",
                        stopwatch.Elapsed,
                        @operator.Method.Name,
                        @operator.Method.DeclaringType.Name,
                        @operator.Method.DeclaringType.Namespace);
#endif

      return operated;
    }
    */

    public static Image FromFile(string file)
    {
      using (var bitmap = BitmapExtensions.LoadFrom(file)) {
        return FromBitmap(bitmap);
      }
    }

    public static Image FromBitmap(Bitmap bitmap)
    {
      var rotation = RotateFlipType.RotateNoneFlipNone;

      foreach (var item in bitmap.PropertyItems) {
        if (item.Id != 0x0112)
          continue;

        switch (item.Value[0]) {
          case 3: rotation = RotateFlipType.Rotate180FlipNone; break;
          case 6: rotation = RotateFlipType.Rotate90FlipNone; break;
          case 8: rotation = RotateFlipType.Rotate270FlipNone; break;
        }
      }

      int width, height;

      if (rotation == RotateFlipType.Rotate90FlipNone || rotation == RotateFlipType.Rotate270FlipNone) {
        width = bitmap.Height;
        height = bitmap.Width;
      }
      else {
        width = bitmap.Width;
        height = bitmap.Height;
      }

      var image = new Image(width, height, true);

      BitmapData locked = null;

      try {
        locked = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        var rgb = image.Rgb;
        var alpha = image.alpha;
        var index = 0;

        unsafe {
          var scan = (byte*)locked.Scan0.ToPointer();

          if (rotation == RotateFlipType.Rotate90FlipNone) {
            for (var y = 0; y < width; y++, scan += locked.Stride) {
              var pixel = scan;

              index = width - 1 - y;

              for (var x = 0; x < height; x++, index += width) {
                rgb[index] = (uint)(*(pixel++) | *(pixel++) << 8 | *(pixel++) << 16);
                alpha[index] = *(pixel++);
              }
            }
          }
          else if (rotation == RotateFlipType.Rotate180FlipNone) {
            index = height * width - 1;

            for (var y = 0; y < height; y++, scan += locked.Stride) {
              var pixel = scan;

              for (var x = 0; x < width; x++, index--) {
                rgb[index] = (uint)(*(pixel++) | *(pixel++) << 8 | *(pixel++) << 16);
                alpha[index] = *(pixel++);
              }
            }
          }
          else if (rotation == RotateFlipType.Rotate270FlipNone) {
            for (var y = 0; y < width; y++, scan += locked.Stride) {
              var pixel = scan;

              index = y + (height - 1) * width;

              for (var x = 0; x < height; x++, index -= width) {
                rgb[index] = (uint)(*(pixel++) | *(pixel++) << 8 | *(pixel++) << 16);
                alpha[index] = *(pixel++);
              }
            }
          }
          else {
            for (var y = 0; y < height; y++, scan += locked.Stride) {
              var pixel = scan;

              for (var x = 0; x < width; x++, index++) {
                rgb[index] = (uint)(*(pixel++) | *(pixel++) << 8 | *(pixel++) << 16);
                alpha[index] = *(pixel++);
              }
            }
          }
        }
      }
      finally {
        if (locked != null)
          bitmap.UnlockBits(locked);
      }

      return image;
    }


    public void Save(string file)
    {
      Save(file, 75);
    }

    public void Save(string file, int quality)
    {
      using (var bitmap = ToBitmap()) {
        bitmap.SaveTo(file, new[] {
          new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality),
        });
      }
    }

    public Bitmap ToBitmap()
    {
      var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
      BitmapData locked = null;

      try {
        locked = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        var index = 0;

        unsafe {
          var scan = (byte*)locked.Scan0.ToPointer();

          if (alpha == null) {
            for (var y = 0; y < Height; y++, scan += locked.Stride) {
              var pixel = (uint*)scan;

              for (var x = 0; x < Width; x++, index++) {
                *(pixel++) = (uint)((Rgb[index] & 0x00ffffff) | 0xff000000);
              }
            }
          }
          else {
            for (var y = 0; y < Height; y++, scan += locked.Stride) {
              var pixel = (uint*)scan;

              for (var x = 0; x < Width; x++, index++) {
                *(pixel++) = (uint)(Rgb[index] & 0x00ffffff) | (uint)(alpha[index] << 24);
              }
            }
          }
        }
      }
      finally {
        if (locked != null)
          bitmap.UnlockBits(locked);
      }

      return bitmap;
    }

    private byte[] alpha = null;
#if DEBUG
    private readonly Stopwatch stopwatch = new Stopwatch();
#endif
  }
}
