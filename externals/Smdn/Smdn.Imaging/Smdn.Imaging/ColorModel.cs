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
using System.Drawing;

namespace Smdn.Imaging {
  public static class ColorModel {
#region "RGB <-> HSV"
    [CLSCompliant(false)]
    public static AlignedHsvColor ToAlignedHsv(Color rgb)
    {
      var hsv = new AlignedHsvColor();

      ToHsv(rgb, out hsv.H, out hsv.S, out hsv.V);

      hsv.A = rgb.A;

      return hsv;
    }

    public static void ToHsv(Color rgb, out byte h, out byte s, out byte v)
    {
      if (rgb.R == rgb.G && rgb.G == rgb.B) {
        h = rgb.R;
        s = rgb.G;
        v = rgb.B;
        return;
      }

      byte min, max;
      int hd, ht;

      if (rgb.R < rgb.G) {
        if (rgb.B < rgb.R) {
          min = rgb.B;
          max = rgb.G;
          hd = rgb.B - rgb.R;
          ht = 85; // 255 / 3
        }
        else if (rgb.G < rgb.B) {
          min = rgb.R;
          max = rgb.B;
          hd = rgb.R - rgb.G;
          ht = 170; // 2 * 255 / 3
        }
        else {
          min = rgb.R;
          max = rgb.G;
          hd = rgb.B - rgb.R;
          ht = 85; // 255 / 3
        }
      }
      else {
        if (rgb.B < rgb.G) {
          min = rgb.B;
          max = rgb.R;
          hd = rgb.G - rgb.B;
          ht = 0;
        }
        else if (rgb.R < rgb.B) {
          min = rgb.G;
          max = rgb.B;
          hd = rgb.R - rgb.G;
          ht = 170; // 2 * 255 / 3
        }
        else {
          min = rgb.G;
          max = rgb.R;
          hd = rgb.G - rgb.B;
          ht = 0;
        }
      }

      var d = max - min;

      h = (byte)((ht + hd * 43 / d) & 0xff);
      s = (byte)(255 * d / max);
      v = max;
    }

    public static HsvColor ToHsv(Color rgb)
    {
      var hsv = new HsvColor();
      int hue;

      ToHsv(rgb, out hue, out hsv.S, out hsv.V);

      hsv.A = rgb.A;
      hsv.H = hue;

      return hsv;
    }

    public static void ToHsv(Color rgb, out int hue, out byte saturation, out byte @value)
    {
      var max =
        (rgb.R < rgb.G)
        ? (rgb.G < rgb.B) ? rgb.B : rgb.G
        : (rgb.R < rgb.B)
          ? (rgb.B < rgb.G) ? rgb.G : rgb.B
          : rgb.R;
      var min =
        (rgb.G < rgb.R)
        ? (rgb.B < rgb.G) ? rgb.B : rgb.G
        : (rgb.B < rgb.R)
          ? (rgb.G < rgb.B) ? rgb.G : rgb.B
          : rgb.R;

      var d = max - min;

      @value = max;
      saturation = (byte)((d == 0) ? 0x00 : (d * 0xff / max));

      if (saturation == 0) {
        hue = 0;
      }
      else {
        var rt = max - (rgb.R * 60 / d);
        var gt = max - (rgb.G * 60 / d);
        var bt = max - (rgb.B * 60 / d);

        if (max == rgb.R)
          hue = bt - gt;
        else if (max == rgb.G)
          hue = rt - bt + 120;
        else /* if (max == rgb.B) */
          hue = gt - rt + 240;

        if (hue < 0)
          hue += 360;
      }
    }

    public static Color ToRgb(HsvColor hsv)
    {
      if (hsv.S == 0)
        return Color.FromArgb(hsv.A, hsv.V, hsv.V, hsv.V);

      //var p = (byte)((hsv.V * (255 - hsv.S)) / 255);
      var p = (byte)((hsv.V * (~hsv.S & 0xff)) >> 8);

      var ht = hsv.H * 6;
      var d  = ht % 360;

      ht /= 360;

      if ((ht & 0x1) == 0) {
        // ht = 0, 2, 4
        var t = (byte)((hsv.V * (255 - hsv.S * (360 - d) / 360)) / 255);

        switch (ht) {
          case 0:  return Color.FromArgb(hsv.A, hsv.V, t, p);
          case 2:  return Color.FromArgb(hsv.A, p, hsv.V, t);
          default: return Color.FromArgb(hsv.A, t, p, hsv.V);
        }
      }
      else {
        // ht = 1, 3 ,5
        var q = (byte)((hsv.V * (255 - hsv.S * d / 360)) / 255);

        switch (ht) {
          case 1:  return Color.FromArgb(hsv.A, q, hsv.V, p);
          case 3:  return Color.FromArgb(hsv.A, p, q, hsv.V);
          default: return Color.FromArgb(hsv.A, hsv.V, p, q);
        }
      }
    }

    [CLSCompliant(false)]
    public static Color ToRgb(AlignedHsvColor hsv)
    {
      if (hsv.S == 0)
        return Color.FromArgb(hsv.A, hsv.V, hsv.V, hsv.V);

      //var ht = hsv.H * 6;
      var ht = hsv.H << 2 + hsv.H << 1;

      // var d = ht % 256;
      var d = ht & 0xff;

      //var p = (byte)((hsv.V * (255 - hsv.S)) / 255);
      var p = (byte)((hsv.V * (~hsv.S & 0xff)) >> 8);

      // ht /= 256
      ht >>= 8;

      if ((ht & 0x1) == 0) {
        // ht = 0, 2, 4
        //var t = (byte)((hsv.I * (255 - hsv.S * (255 - d) / 255)) / 255);
        var t = (byte)(hsv.V * (~(hsv.S * (~d & 0xff) >> 8) & 0xff) >> 8);

        switch (ht) {
          case 0:  return Color.FromArgb(hsv.A, hsv.V, t, p);
          case 2:  return Color.FromArgb(hsv.A, p, hsv.V, t);
          default: return Color.FromArgb(hsv.A, t, p, hsv.V);
        }
      }
      else {
        // ht = 1, 3 ,5
        //var q = (byte)((hsv.V * (255 - hsv.S * d / 255)) / 255);
        var q = (byte)((hsv.V * (~((hsv.S * d) >> 8) & 0xff)) >> 8);

        switch (ht) {
          case 1:  return Color.FromArgb(hsv.A, q, hsv.V, p);
          case 3:  return Color.FromArgb(hsv.A, p, q, hsv.V);
          default: return Color.FromArgb(hsv.A, hsv.V, p, q);
        }
      }
    }
#endregion

#region "RGB <-> CMY, CMKY"
    public static void ToCmy(Color rgb, out byte c, out byte m, out byte y)
    {
      c = (byte)(0xff - rgb.R);
      m = (byte)(0xff - rgb.G);
      y = (byte)(0xff - rgb.B);
    }

    public static Color ToRgbFromCmy(byte c, byte m, byte y)
    {
      return Color.FromArgb(0xff - c, 0xff - m, 0xff - y);
    }

    public static void ToCmyk(Color rgb, out int c, out int m, out int y, out int k)
    {
      var max =
        (rgb.R < rgb.G)
        ? (rgb.G < rgb.B) ? rgb.B : rgb.G
        : (rgb.R < rgb.B)
          ? (rgb.B < rgb.G) ? rgb.G : rgb.B
          : rgb.R;

      k = 0xff - max;
      c = (byte)(0xff - rgb.R - k);
      m = (byte)(0xff - rgb.G - k);
      y = (byte)(0xff - rgb.B - k);
    }

    public static Color ToRgbFromCmyk(byte c, byte m, byte y, byte k)
    {
      var r = (0xff - c - k);
      var g = (0xff - m - k);
      var b = (0xff - y - k);

      return Color.FromArgb(r < 0 ? 0 : r, g < 0 ? 0 : g, b < 0 ? 0 : b);
    }
#endregion

    public static double LuminanceOf(HsvColor hsv)
    {
      return LuminanceOf(ToRgb(hsv));
    }

    public static double LuminanceOf(Color rgb)
    {
      // ITU-R BT.601 (non-scaled)
      return ((0.299 * rgb.R + 0.587 * rgb.G + 0.114 * rgb.B) + 16.0) / 256.0;
    }

    public static Color AverageOf(Color color, params Color[] colors)
    {
      var r = (decimal)color.R;
      var g = (decimal)color.G;
      var b = (decimal)color.B;

      for (var i = 0; i < colors.Length; i++) {
        r += (decimal)colors[i].R;
        g += (decimal)colors[i].G;
        b += (decimal)colors[i].B;
      }

      return Color.FromArgb(0xff,
                            (int)(r / (1 + colors.Length)),
                            (int)(g / (1 + colors.Length)),
                            (int)(b / (1 + colors.Length)));
    }

    public static double DistanceBetween(Color x, Color y)
    {
      return Math.Sqrt((x.R - y.R) * (x.R - y.R) +
                       (x.G - y.G) * (x.G - y.G) +
                       (x.B - y.B) * (x.B - y.B));
    }

    public static double DistanceBetween(HsvColor x, HsvColor y)
    {
      return Math.Sqrt((x.H - y.H) * (x.H - y.H) +
                       (x.S - y.S) * (x.S - y.S) +
                       (x.V - y.V) * (x.V - y.V));
    }

    [CLSCompliant(false)]
    public static double DistanceBetween(AlignedRgbColor x, AlignedRgbColor y)
    {
      return Math.Sqrt((x.R - y.R) * (x.R - y.R) +
                       (x.G - y.G) * (x.G - y.G) +
                       (x.B - y.B) * (x.B - y.B));
    }
  }
}
