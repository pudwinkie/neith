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

namespace Smdn.Imaging.Processing {
  public static class Filters {
    public static void NoOp(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      Array.Copy(original, filtered, pixels);
    }

    public static void Negate(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        filtered[index] = ~original[index];
      }
    }

    public static void Posterization2Bit(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        var col = original[index] & 0x00808080;
        filtered[index] = col | (col - (col >> 7));
      }
    }

    public static void Posterization4Bit(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      byte[] r, g, b;

      SeparateChannels(original, out r, out g, out b);

      for (var index = 0; index < pixels; index++) {
        var rr = (byte)((r[index] >> 6) << 6);
        var gg = (byte)((g[index] >> 6) << 6);
        var bb = (byte)((b[index] >> 6) << 6);

        filtered[index] = (uint)(rr << 16 | gg << 8 | bb);
      }
    }

    private static void SeparateChannels(uint[] composite, out byte[] r, out byte[] g, out byte[] b)
    {
      var pixels = composite.Length;

      r = new byte[pixels];
      g = new byte[pixels];
      b = new byte[pixels];

      for (var index = 0; index < pixels; index++) {
        r[index] = (byte)((composite[index] & 0x00ff0000) >> 16);
        g[index] = (byte)((composite[index] & 0x0000ff00) >> 8);
        b[index] = (byte) (composite[index] & 0x000000ff);
      }
    }

    public static void FeatherFast(uint[] filtered, uint[] original, int width, int height, FilterArgs args)
    {
      byte[] r, g, b;

      SeparateChannels(original, out r, out g, out b);

      var index = 0;

      for (var x = 0; x < width; x++) {
        filtered[index] = original[index++];
      }

      for (var y = 1; y < height - 1; y++) {
        filtered[index] = original[index++];

        var above = index - width;
        var beneath = index + width;
        var left = index - 1;
        var right = index + 1;

        for (var x = 1; x < width - 1; x++) {
          var rr = (r[above - 1] + r[above] + r[above + 1] + r[left] + r[right] + r[beneath - 1] + r[beneath] + r[beneath + 1]) >> 3;
          var gg = (g[above - 1] + g[above] + g[above + 1] + g[left] + g[right] + g[beneath - 1] + g[beneath] + g[beneath + 1]) >> 3;
          var bb = (b[above - 1] + b[above] + b[above + 1] + b[left] + b[right] + b[beneath - 1] + b[beneath] + b[beneath + 1]) >> 3;

          filtered[index++] = (uint)(rr << 16 | gg << 8 | bb);

          above++;
          beneath++;
          left++;
          right++;
        }

        filtered[index] = original[index++];
      }

      for (var x = 0; x < width; x++) {
        filtered[index] = original[index++];
      }
    }

    public static void DifferentiationX(uint[] filtered, uint[] original, int width, int height, FilterArgs args)
    {
      var index = 0;

      for (var y = 0; y < height; y++) {
        filtered[index++] = 0x00000000;
        for (var x = 1; x < width; x++) {
          var r = (int)(original[index] & 0x00ff0000) - (int)(original[index - 1] & 0x00ff0000);
          var g = (int)(original[index] & 0x0000ff00) - (int)(original[index - 1] & 0x0000ff00);
          var b = (int)(original[index] & 0x000000ff) - (int)(original[index - 1] & 0x000000ff);

          filtered[index++] = (uint)(((r < 0 ? -r : r) & 0x00ff0000) |
                                     ((g < 0 ? -g : g) & 0x0000ff00) |
                                     ((b < 0 ? -b : b) & 0x000000ff));
        }
      }
    }

    public static void DifferentiationY(uint[] filtered, uint[] original, int width, int height, FilterArgs args)
    {
      var index = 0;

      for (var x = 0; x < width; x++) {
        filtered[index++] = 0x00000000;
      }

      for (var y = 1; y < height; y++) {
        for (var x = 0; x < width; x++) {
          var r = (int)(original[index] & 0x00ff0000) - (int)(original[index - width] & 0x00ff0000);
          var g = (int)(original[index] & 0x0000ff00) - (int)(original[index - width] & 0x0000ff00);
          var b = (int)(original[index] & 0x000000ff) - (int)(original[index - width] & 0x000000ff);

          filtered[index++] = (uint)(((r < 0 ? -r : r) & 0x00ff0000) |
                                     ((g < 0 ? -g : g) & 0x0000ff00) |
                                     ((b < 0 ? -b : b) & 0x000000ff));
        }
      }
    }

    public static void DifferentiationXY(uint[] filtered, uint[] original, int width, int height, FilterArgs args)
    {
      var index = 0;

      for (var x = 0; x < width; x++) {
        filtered[index++] = 0x00000000;
      }

      for (var y = 1; y < height; y++) {
        filtered[index++] = 0x00000000;
        for (var x = 1; x < width; x++) {
          var r = (int)(original[index] & 0x00ff0000);
          var g = (int)(original[index] & 0x0000ff00);
          var b = (int)(original[index] & 0x000000ff);

          var dxr = r - (int)(original[index - 1] & 0x00ff0000);
          var dxg = g - (int)(original[index - 1] & 0x0000ff00);
          var dxb = b - (int)(original[index - 1] & 0x000000ff);

          var dyr = r - (int)(original[index - width] & 0x00ff0000);
          var dyg = g - (int)(original[index - width] & 0x0000ff00);
          var dyb = b - (int)(original[index - width] & 0x000000ff);

          r = (dxr < 0 ? -dxr : dxr) + (dyr < 0 ? -dyr : dyr);
          g = (dxg < 0 ? -dxg : dxg) + (dyg < 0 ? -dyg : dyg);
          b = (dxb < 0 ? -dxb : dxb) + (dyb < 0 ? -dyb : dyb);

          if (0x00ff0000 < r)
            r = 0x00ff0000;
          if (0x0000ff00 < g)
            g = 0x0000ff00;
          if (0x000000ff < b)
            b = 0x000000ff;

          filtered[index++] = (uint)(r | g | b);
        }
      }
    }

    public static void HSV(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        filtered[index] = ColorModel.ToAlignedHsv((AlignedRgbColor)original[index]).Value;
      }
    }

    public static void Hue(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        var hsv = ColorModel.ToAlignedHsv((AlignedRgbColor)original[index]);
        filtered[index] = (uint)(hsv.H << 16 | hsv.H << 8 | hsv.H);
      }
    }

    public static void Saturation(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        var hsv = ColorModel.ToAlignedHsv((AlignedRgbColor)original[index]);
        filtered[index] = (uint)(hsv.S << 16 | hsv.S << 8 | hsv.S);
      }
    }

    public static void Value(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        var hsv = ColorModel.ToAlignedHsv((AlignedRgbColor)original[index]);
        filtered[index] = (uint)(hsv.V << 16 | hsv.V << 8 | hsv.V);
      }
    }

    public static void R(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        filtered[index] = original[index] & 0x00ff0000;
      }
    }

    public static void G(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        filtered[index] = original[index] & 0x0000ff00;
      }
    }

    public static void B(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        filtered[index] = original[index] & 0x000000ff;
      }
    }

    public static void ZeroOrElse(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        if (original[index] == 0)
          filtered[index] = 0x00000000;
        else
          filtered[index] = 0x00ffffff;
      }
    }

    public static void MaxOrElse(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      for (var index = 0; index < pixels; index++) {
        if (original[index] == 0x00ffffff)
          filtered[index] = 0x00ffffff;
        else
          filtered[index] = 0x00000000;
      }
    }

    public static void BandPass(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      var min = (args as BandPassFilterArgs).Min;
      var max = (args as BandPassFilterArgs).Max;
      var andAlso = (args as BandPassFilterArgs).AndAlso;

      byte[] r, g, b;

      SeparateChannels(original, out r, out g, out b);

      for (var index = 0; index < pixels; index++) {
        byte rr, gg, bb;

        if (min.R <= r[index] && r[index] <= max.R)
          rr = r[index];
        else
          rr = 0;

        if (min.G <= g[index] && g[index] <= max.G)
          gg = g[index];
        else
          gg = 0;

        if (min.B <= b[index] && b[index] <= max.B)
          bb = b[index];
        else
          bb = 0;

        if (andAlso && (rr == 0 || gg == 0 || bb == 0))
          filtered[index] = 0x00000000;
        else
          filtered[index] = (uint)(rr << 16 | gg << 8 | bb);
      }
    }

    public static void Expand(uint[] filtered, uint[] original, int width, int height, FilterArgs args)
    {
      var pixels = width * height;

      for (var pixel = 0; pixel < pixels; pixel++) {
        filtered[pixel] = 0x00000000;
      }

      for (var y = 1; y < height - 1; y++) {
        var index = 1 + y * width;
        var above = index - width;
        var beneath = index + width;

        for (var x = 1; x < width - 1; x++) {
          if (original[index] != 0x00000000) {
            //filtered[above - 1] = filtered[above] = filtered[above + 1] = original[index];
            filtered[above] = original[index];
            filtered[index - 1] = filtered[index] = filtered[index + 1] = original[index];
            filtered[beneath] = original[index];
            //filtered[beneath - 1] = filtered[beneath] = filtered[beneath + 1] = original[index];
          }

          above++;
          index++;
          beneath++;
        }
      }
    }

    public static void Contrast(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      var offsetR = (args as ContrastFilterArgs).R;
      var offsetG = (args as ContrastFilterArgs).G;
      var offsetB = (args as ContrastFilterArgs).B;

      byte[] r, g, b;

      SeparateChannels(original, out r, out g, out b);

      // TODO: process G, B same as R
      if (0 < offsetR || 0 < offsetG || 0 < offsetB) {
        var offsetRMax = 255 - offsetR;
        var offsetGMax = 255 - offsetG;
        var offsetBMax = 255 - offsetB;
        var offsetRMultiplyer = 256.0f / (256.0f - offsetR * 2.0f);
        var offsetGMultiplyer = 256.0f / (256.0f - offsetG * 2.0f);
        var offsetBMultiplyer = 256.0f / (256.0f - offsetB * 2.0f);

        for (var index = 0; index < pixels; index++)  {
          var fr = (r[index] < offsetR) ? (byte)0x00 : ((offsetRMax < r[index]) ? (byte)0xff : (byte)(((float)r[index] - offsetR) * offsetRMultiplyer));
          var fg = (g[index] < offsetG) ? (byte)0x00 : ((offsetGMax < g[index]) ? (byte)0xff : (byte)(((float)g[index] - offsetG) * offsetGMultiplyer));
          var fb = (b[index] < offsetB) ? (byte)0x00 : ((offsetBMax < b[index]) ? (byte)0xff : (byte)(((float)b[index] - offsetB) * offsetBMultiplyer));

          filtered[index] = (uint)(fr << 16 | fg << 8 | fb);
        }
      }
      else if (offsetR < 0) {
        var offsetRFloated = (float)-offsetR;
        var offsetGFloated = (float)-offsetG;
        var offsetBFloated = (float)-offsetB;
        var offsetRMultiplier = (256.0f - offsetR * 2.0f);
        var offsetGMultiplier = (256.0f - offsetG * 2.0f);
        var offsetBMultiplier = (256.0f - offsetB * 2.0f);
        var invertedMax = 1.0f / 256.0f;

        for (var index = 0; index < pixels; index++) {
          var fr = (byte)(offsetRFloated + ((double)r[index] * offsetRMultiplier) * invertedMax);
          var fg = (byte)(offsetGFloated + ((double)g[index] * offsetGMultiplier) * invertedMax);
          var fb = (byte)(offsetBFloated + ((double)b[index] * offsetBMultiplier) * invertedMax);

          filtered[index] = (uint)(fr << 16 | fg << 8 | fb);
        }
      }
      else {
        Array.Copy(original, filtered, pixels);
      }
    }

    public static void Gamma(uint[] filtered, uint[] original, int pixels, FilterArgs args)
    {
      var gr = 1.0 / (double)(args as GammaFilterArgs).R;
      var gg = 1.0 / (double)(args as GammaFilterArgs).G;
      var gb = 1.0 / (double)(args as GammaFilterArgs).B;

      byte[] r, g, b;

      SeparateChannels(original, out r, out g, out b);

      if (gr == gg && gg == gb) {
        var pow = new byte[0x100];
        var scaling = 1.0 / 255.0;

        for (var i = 0; i < 0x100; i++) {
          pow[i] = (byte)(255.0 * Math.Pow(i * scaling, gr));
        }

        for (var index = 0; index < pixels; index++)  {
          filtered[index] = (uint)(pow[r[index]] << 16 | pow[g[index]] << 8 | pow[b[index]]);
        }
      }
      else {
        var powR = new byte[0x100];
        var powG = new byte[0x100];
        var powB = new byte[0x100];
        var scaling = 1.0 / 255.0;

        for (var i = 0; i < 0x100; i++) {
          powR[i] = (byte)(255.0 * Math.Pow(i * scaling, gr));
        }

        for (var i = 0; i < 0x100; i++) {
          powG[i] = (byte)(255.0 * Math.Pow(i * scaling, gg));
        }

        for (var i = 0; i < 0x100; i++) {
          powB[i] = (byte)(255.0 * Math.Pow(i * scaling, gb));
        }

        for (var index = 0; index < pixels; index++)  {
          filtered[index] = (uint)(powR[r[index]] << 16 | powG[g[index]] << 8 | powB[b[index]]);
        }
      }
    }

    public static void LinearFilter(uint[] filtered, uint[] original, int width, int height, FilterArgs args)
    {
      var matrix = (args as LinearFilterArgs).Matrix;
      var scale  = (args as LinearFilterArgs).Scale;
      var offset = (args as LinearFilterArgs).Offset;
      var fl = (args as LinearFilterArgs).X;
      var fr = fl + (args as LinearFilterArgs).Width;
      var ft = (args as LinearFilterArgs).Y;
      var fb = ft + (args as LinearFilterArgs).Height;

      if (fl == fr)
        fr = width;
      if (ft == fb)
        fb = height;

      byte[] r, g, b;

      SeparateChannels(original, out r, out g, out b);

      var index = 0;

      for (var x = 0; x < width; x++) {
        filtered[index++] = 0x00000000;
      }

      for (var y = 1; y < height - 1; y++) {
        filtered[index++] = 0x00000000;

        if (fb <= y || y < ft) {
          for (var x = 1; x < width - 1; x++) {
            filtered[index] = original[index];
            index++;
          }
        }
        else {
          for (var x = 1; x < width - 1; x++) {
            if (fr <= x || x < fl) {
              filtered[index] = original[index];
            }
            else {
              float ab, ag, ar;

              var targetIndex = index - width - 1;

              ab  = (float)b[targetIndex] * matrix.E11;
              ag  = (float)g[targetIndex] * matrix.E11;
              ar  = (float)r[targetIndex] * matrix.E11;

              targetIndex++;

              ab += (float)b[targetIndex] * matrix.E12;
              ag += (float)g[targetIndex] * matrix.E12;
              ar += (float)r[targetIndex] * matrix.E12;

              targetIndex++;

              ab += (float)b[targetIndex] * matrix.E13;
              ag += (float)g[targetIndex] * matrix.E13;
              ar += (float)r[targetIndex] * matrix.E13;

              targetIndex = index - 1;

              ab += (float)b[targetIndex] * matrix.E21;
              ag += (float)g[targetIndex] * matrix.E21;
              ar += (float)r[targetIndex] * matrix.E21;

              targetIndex++;

              ab += (float)b[targetIndex] * matrix.E22;
              ag += (float)g[targetIndex] * matrix.E22;
              ar += (float)r[targetIndex] * matrix.E22;

              targetIndex++;

              ab += (float)b[targetIndex] * matrix.E23;
              ag += (float)g[targetIndex] * matrix.E23;
              ar += (float)r[targetIndex] * matrix.E23;

              targetIndex = index + width - 1;

              ab += (float)b[targetIndex] * matrix.E31;
              ag += (float)g[targetIndex] * matrix.E31;
              ar += (float)r[targetIndex] * matrix.E31;

              targetIndex++;

              ab += (float)b[targetIndex] * matrix.E32;
              ag += (float)g[targetIndex] * matrix.E32;
              ar += (float)r[targetIndex] * matrix.E32;

              targetIndex++;

              ab += (float)b[targetIndex] * matrix.E33;
              ag += (float)g[targetIndex] * matrix.E33;
              ar += (float)r[targetIndex] * matrix.E33;

              ab = (ab * scale) + offset;
              ag = (ag * scale) + offset;
              ar = (ar * scale) + offset;

              byte rr = 0;
              byte gg = 0;
              byte bb = 0;

              if (255.0f < ab)
                bb = 0xff;
              else if (ab <= 0.0)
                bb = 0x00;
              else
                bb = (byte)ab;

              if (255.0f < ag)
                gg = 0xff;
              else if (ag <= 0.0)
                gg = 0x00;
              else
                gg = (byte)ag;

              if (255.0f < ar)
                rr = 0xff;
              else if (ar <= 0.0)
                rr = 0x00;
              else
                rr = (byte)ar;

              filtered[index] = (uint)(rr << 16 | gg << 8 | bb);
            } // if

            index++;
          } // for x
        } // if

        filtered[index++] = 0x00000000;
      }

      for (var x = 0; x < width; x++) {
        filtered[index++] = 0x00000000;
      }
    }
  }
}
