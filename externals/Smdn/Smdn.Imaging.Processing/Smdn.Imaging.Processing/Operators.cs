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

using Smdn.Mathematics;

namespace Smdn.Imaging.Processing {
  public static class Operators {
    public static void Clear(Image target)
    {
      var rgb = target.Rgb;

      for (var index = 0; index < target.PixelCount; index++) {
        rgb[index] = 0x00000000;
      }
    }

    public static void BitBlt(Image target, int targetX, int targetY, Image source)
    {
      BitBlt(target, targetX, targetY, source, 0, 0, source.Width, source.Height);
    }

    public static void BitBlt(Image target, int targetX, int targetY, Image source, int sourceX, int sourceY, int width, int height)
    {
      var targetRgb = target.Rgb;
      var sourceRgb = source.Rgb;
      var tw = target.Width;
      var th = target.Height;
      var sw = source.Width;

      for (var y = 0; y < height; y++) {
        if (th <= targetY + y)
          break;

        var targetIndex = targetX + (targetY + y) * tw;
        var sourceIndex = sourceX + (sourceY + y) * sw;

        for (var x = 0; x < width; x++) {
          if (tw <= targetX + x)
            break;

          targetRgb[targetIndex++] = sourceRgb[sourceIndex++];
        }
      }
    }

    public static Image Trim(Image original, int left, int top, int right, int bottom)
    {
      var trimmed = new Image(original.Width - left - right, original.Height - top - bottom);

      BitBlt(trimmed, 0, 0, original, left, top, trimmed.Width, trimmed.Height);

      return trimmed;
    }

    public static void RenderTransformed(Image target, Image original, Vector4D viewPoint, Vector4D targetPoint, float focusLength)
    {
      var viewToTarget = targetPoint - viewPoint;
      var transformMatrix = Matrix4D.Shift(-viewPoint) * (Matrix4D.RotateX(viewToTarget.Y, viewToTarget.Z) * Matrix4D.RotateY(viewToTarget.X, viewToTarget.Z));

      var ow = original.Width;
      var oh = original.Height;
      var ohwf = ow * 0.5f;
      var ohhf = oh * 0.5f;

      var tw = target.Width;
      var th = target.Height;
      var thwf = tw * 0.5f;
      var thhf = th * 0.5f;

      var orgb = original.Rgb;
      var trgb = target.Rgb;

      for (var y = 0; y < oh; y++) {
        var index = y * ow;
        for (var x = 0; x < ow; x++) {
          var originalPoint = new Vector4D(x - ohwf, y - ohhf, 0.0f);
          var transformedPoint = originalPoint * transformMatrix;
          var perspectiveScale = focusLength / transformedPoint.Z;

          var transformedX = (int)(transformedPoint.X * perspectiveScale + thwf);
          var transformedY = (int)(transformedPoint.Y * perspectiveScale + thhf);

          if (0 <= transformedY && transformedY < th && 0 <= transformedX && transformedX < tw) {
            trgb[transformedX + transformedY * tw] = orgb[x + y * ow];
          }

          index++;
        }
      }
    }


    public static void RenderTransformedWithInterpolation(Image target, Image original, Vector4D viewPoint, Vector4D targetPoint, float focusLength)
    {
      var viewToTarget = targetPoint - viewPoint;
      var transformMatrix = Matrix4D.Shift(-viewPoint) * (Matrix4D.RotateX(viewToTarget.Y, viewToTarget.Z) * Matrix4D.RotateY(viewToTarget.X, viewToTarget.Z));

      var inverseTransform = Matrix4D.Shift(viewPoint) * (Matrix4D.RotateX(-viewToTarget.Y, -viewToTarget.Z) * Matrix4D.RotateY(-viewToTarget.X, -viewToTarget.Z));

      var ow = original.Width;
      var oh = original.Height;
      var ohwf = ow * 0.5f;
      var ohhf = oh * 0.5f;

      var tw = target.Width;
      var th = target.Height;
      var thw = target.Width / 2 - 1;
      var thh = target.Height / 2 - 1;
      var thwf = tw * 0.5f;
      var thhf = th * 0.5f;

      var orgb = original.Rgb;
      var trgb = target.Rgb;

      // 変換後の頂点座標からスキャンすべき範囲を定める
      var scanLeft   = +thw;
      var scanRight  = -thw;
      var scanTop    = +thh;
      var scanBottom = -thh;

      foreach (var vertex in new[] {
        new Vector4D(-ohwf, -ohhf, 0.0f),
        new Vector4D(+ohwf, -ohhf, 0.0f),
        new Vector4D(+ohwf, +ohhf, 0.0f),
        new Vector4D(-ohwf, +ohhf, 0.0f),
      }) {
        var transformedPoint = vertex * transformMatrix;
        var perspectiveScale = focusLength / transformedPoint.Z;

        var tx = (int)Math.Ceiling(transformedPoint.X * perspectiveScale);
        var ty = (int)Math.Ceiling(transformedPoint.Y * perspectiveScale);

        if (tx < scanLeft)
          scanLeft = (tx <= -thw) ? -thw : tx;
        else if (scanRight < tx)
          scanRight = (+thw <= tx) ? +thw : tx;

        if (ty < scanTop)
          scanTop = (ty <= -thh) ? -thh : ty;
        else if (scanBottom < ty)
          scanBottom = (+thh <= ty) ? +thh: ty;
      }

      Console.WriteLine(scanTop);
      Console.WriteLine(scanBottom);
      Console.WriteLine(scanLeft);
      Console.WriteLine(scanRight);

      var pixels = new Vector4D[4];
      var px = new int[4];
      var py = new int[4];

      for (var sy = scanTop; sy <= scanBottom; sy++) {
        var index = (sy + thh) * tw;
        for (var sx = scanLeft; sx <= scanRight; sx++) {
          pixels[0] = new Vector4D((float)sx,        (float)sy,        0.0f);
          pixels[1] = new Vector4D((float)sx + 1.0f, (float)sy,        0.0f);
          pixels[2] = new Vector4D((float)sx,        (float)sy + 1.0f, 0.0f);
          pixels[3] = new Vector4D((float)sx + 1.0f, (float)sy + 1.0f, 0.0f);

          for (var p = 0; p < 4; p++) {
            var transformedPoint = pixels[p] * transformMatrix;
            var perspectiveScale = focusLength / transformedPoint.Z;

            px[p] = (int)(transformedPoint.X * perspectiveScale + ohwf);
            py[p] = (int)(transformedPoint.Y * perspectiveScale + ohhf);
          }

          if (0 <= px[0] && px[0] < ow && 0 <= py[0] && py[0] < oh) {
            trgb[index++] = orgb[px[0] + py[0] * ow];
          }
        }
      }
    }

    public static Image AffineTransformation(Image original, double scaleX, double scaleY, double rotation)
    {
      var ow = original.Width;
      var oh = original.Height;
      var ohwf = ow * 0.5;
      var ohhf = oh * 0.5;

      var sin = Math.Sin(rotation);
      var cos = Math.Cos(rotation);

      var tw = (int)Math.Floor(ow * scaleX * Math.Abs(cos) + oh * scaleY * Math.Abs(sin));
      var th = (int)Math.Floor(ow * scaleX * Math.Abs(sin) + oh * scaleY * Math.Abs(cos));
      var tl = -tw / 2 + (tw + 1) % 2;
      var tr =  tw / 2;
      var tt = -th / 2 + (th + 1) % 2;
      var td =  th / 2;

      var transformed = new Image(tw, th);

      var isx = 1.0 / scaleX;
      var isy = 1.0 / scaleY;

      var orgb = original.Rgb;
      var trgb = transformed.Rgb;

      for (var ty = tt; ty <= td; ty++) {
        var index = (ty - tt) * tw;
        var tysin = ty * sin;
        var tycos = ty * cos;

        for (var tx = tl; tx <= tr; tx++) {
          var ox = (isx * ( tx * cos + tysin)) + ohwf;
          var oy = (isy * (-tx * sin + tycos)) + ohhf;

          // bilinear interpolation
          var ix = (int)Math.Truncate(ox);
          var iy = (int)Math.Truncate(oy);
          var fx = ox - ix;
          var fy = oy - iy;

          if (fx < 0.0) {
            trgb[index++] = 0x00000000;
            continue;
          }

          if (fy < 0.0) {
            trgb[index++] = 0x00000000;
            continue;
          }

          var dx = (uint)(0x100 * fx);
          var dy = (uint)(0x100 * fy);

          uint r = 0;
          uint g = 0;
          uint b = 0;
          uint area = 0;

          for (var y = iy; y <= iy + 1; y++) {
            if (y < 0 || oh <= y)
              continue;

            dy = 0x100 - dy;

            // (x, y), (x, y + 1)
            if (ix < 0 || ow <= ix)
              continue;

            var offset = ix + y * ow;

            var rgb = orgb[offset];
            var d = ((0x100 - dx) * dy) >> 8;

            r += ((rgb & 0x00ff0000) >> 16) * d;
            g += ((rgb & 0x0000ff00) >> 8 ) * d;
            b += ((rgb & 0x000000ff)      ) * d;

            area++;

            // (x + 1, y), (x + 1, y + 1)
            if (ow <= ix + 1)
              continue;

            rgb = orgb[offset + 1];
            d = (dx * dy) >> 8;

            r += ((rgb & 0x00ff0000) >> 16) * d;
            g += ((rgb & 0x0000ff00) >> 8 ) * d;
            b += ((rgb & 0x000000ff)      ) * d;

            area++;
          }

          if (area == 0) {
            trgb[index++] = 0x00000000;
          }
          else {
            //trgb[index++] = (uint)(r >> 8) << 16 | (uint)(g >> 8) << 8 | (uint)(b >> 8);
            trgb[index++] = (r & 0x0000ff00) << 8 | (g & 0x0000ff00) | (b & 0x0000ff00) >> 8;
          }
        }
      }

      return transformed;
    }

    public static Image ScaleByNearestNeighbor(Image original, int width, int height)
    {
      var scaledImage = new Image(width, height);

      var org = original.Rgb;
      var scaled = scaledImage.Rgb;
      var ow = original.Width;
      var scaleX = (float)ow / (float)width;
      var scaleY = (float)original.Height / (float)height;
      var scaledIndex = 0;

      for (var dy = 0; dy < height; dy++) {
        var originalIndex = (int)Math.Floor((float)dy * scaleY) * ow;
        for (var dx = 0; dx < width; dx++) {
          scaled[scaledIndex++] = org[originalIndex + (int)Math.Floor((float)dx * scaleX)];
        }
      }

      return scaledImage;
    }

    public static Image ScaleByIntegral(Image original, int width, int height)
    {
      return ScaleByIntegral(original, width, height, true);
    }

    public static Image ScaleByIntegral(Image original, int width, int height, bool skipping)
    {
      var ow = original.Width;
      var oh = original.Height;

      var wlcm = MathUtils.Lcm(ow, width);
      var hlcm = MathUtils.Lcm(oh, height);

      var scaledImage = new Image(width, height, original.EnableAlpha);

      var orgb    = original.Rgb;
      var oalha   = original.Alpha;
      var srgb    = scaledImage.Rgb;
      var salpha  = scaledImage.Alpha;

      var obw = wlcm / ow;
      var obh = hlcm / oh;
      var sbw = wlcm / width;
      var sbh = hlcm / height;

      // skipping
      var sbsx = 1;
      var sbsy = 1;

      if (skipping) {
        var ww = ow / width;
        var hh = oh / height;

        if (ww == 0)
          ww = 1;
        if (hh == 0)
          hh = 1;

        sbsx = (ww < sbw) ? sbw / ww : 1;
        sbsy = (hh < sbh) ? sbh / hh : 1;

        sbsx = (sbw / 4 < sbsx) ? sbw / 4 : sbsx;
        sbsy = (sbh / 4 < sbsy) ? sbh / 4 : sbsy;

        sbsx = (0 == sbsx) ? 1 : sbsx;
        sbsy = (0 == sbsy) ? 1 : sbsy;
      }
      else {
        //sbsx = (sbw / 4 < sbsx) ? sbw / 4 : sbsx;
        //sbsy = (sbh / 4 < sbsy) ? sbh / 4 : sbsy;
        sbsx = 1;
        sbsy = 1;
      }

      var sbix = 0;
      var sbiy = 0;

      for (var sy = 0; sy < height; sy++) {
        var syy = sy * width;

        for (var sx = 0; sx < width; sx++) {
          uint ir = 0;
          uint ig = 0;
          uint ib = 0;
          uint ia = 0;
          uint sba = 0;

          for (var sby = 0; sby < sbh; sby += sbsy) {
            var oy = ((sbiy + sby) / obh) * ow;

            for (var sbx = 0; sbx < sbw; sbx += sbsx) {
              var rgb = orgb[(sbix + sbx) / obw + oy];

              ir += (rgb & 0x00ff0000) >> 16;
              ig += (rgb & 0x0000ff00) >> 8;
              ib += (rgb & 0x000000ff);

              if (oalha != null)
                ia += oalha[(sbix + sbx) / obw + oy];

              sba++;
            }
          }

          srgb[sx + syy] = (uint)((ir / sba) << 16 | (ig / sba) << 8 | (ib / sba));

          if (salpha != null)
            salpha[sx + syy] = (byte)(ia / sba);

          sbix += sbw;
        }

        sbix = 0;
        sbiy += sbh;
      }

      return scaledImage;
    }

    public static Image Rotate(Image original, System.Drawing.RotateFlipType type)
    {
      switch (type) {
        case System.Drawing.RotateFlipType.Rotate90FlipNone:
          return Rotate90(original);
        case System.Drawing.RotateFlipType.Rotate180FlipNone:
          return Rotate180(original);
        case System.Drawing.RotateFlipType.Rotate270FlipNone:
          return Rotate270(original);
        case System.Drawing.RotateFlipType.RotateNoneFlipNone:
          return original;
        default:
          throw new NotImplementedException();
      }
    }

    private static Image Rotate90(Image original)
    {
      var rotated = new Image(original.Height, original.Width);
      var orgb = original.Rgb;
      var rrgb = rotated.Rgb;

      for (var y = 0; y < original.Height; y++) {
        var oy = y * original.Width;
        var rx = rotated.Width - 1 - y;

        for (int ox = 0, ry = 0; ox < original.Width; ox++, ry += rotated.Width) {
          rrgb[ry + rx] = orgb[oy + ox];
        }
      }

      return rotated;
    }

    private static Image Rotate270(Image original)
    {
      var rotated = new Image(original.Height, original.Width);
      var orgb = original.Rgb;
      var rrgb = rotated.Rgb;

      for (var y = 0; y < original.Height; y++) {
        var oy = y * original.Width;
        var rx = y;

        for (int ox = 0, ry = (rotated.Height - 1) * rotated.Width; ox < original.Width; ox++, ry -= rotated.Width) {
          rrgb[ry + rx] = orgb[oy + ox];
        }
      }

      return rotated;
    }

    private static Image Rotate180(Image original)
    {
      var rotated = new Image(original.Width, original.Height);
      var orgb = original.Rgb;
      var rrgb = rotated.Rgb;

      for (var y = 0; y < original.Height; y++) {
        var oy = y * original.Width;
        var ry = (rotated.Height - 1 - y) * original.Width;

        for (int ox = 0, rx = rotated.Width - 1; ox < original.Width; ox++, rx--) {
          rrgb[ry + rx] = orgb[oy + ox];
        }
      }

      return rotated;
    }
  }
}