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

//#define MONO_SIMD
//#define YUV_FLOAT

#if MONO_SIMD
#define YUV_SIMD
using Mono.Simd;
#endif

using System;

namespace Smdn.Media.Earthsoft.PV4.Codec {
  internal static class CodecUtils {
    static CodecUtils()
    {
      InitializeYuv422CropTable();
    }

    internal static unsafe void ConvertPackedYUV422ToRgb(byte* yuv422, int yuv422Stride, int width, int height, byte* rgb, int rgbStride)
    {
#if DEBUG
      if ((width & 0x1) != 0x0)
        throw ExceptionUtils.CreateArgumentMustBeMultipleOf(2, "width");
#endif

      width >>= 1;

      var srcScanLine  = yuv422;
      var destScanLine = rgb;

      for (var y = 0; y < height; y++, srcScanLine += yuv422Stride, destScanLine += rgbStride) {
        var yuv = srcScanLine;
        var bgr = destScanLine;

        for (var x = 0; x < width; x++) {
#if YUV_SIMD
  #if YUV_FLOAT
          var y0 = (float)(*(yuv++) -  16);
          var cb = (float)(*(yuv++) - 128);
          var y1 = (float)(*(yuv++) -  16);
          var cr = (float)(*(yuv++) - 128);

          var  lum0Vector = new Vector4f(y0, y0, y0, 0.0f);
          var  lum1Vector = new Vector4f(y1, y1, y1, 0.0f);
          var chromVector = new Vector4f(cb, cb, cr, cr);

          chromVector *= chromScaleVector;

          lum0Vector = (lum0Vector * lumScaleVector) + chromVector;

          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int) lum0Vector.X];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(lum0Vector.Y + lum0Vector.W)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int) lum0Vector.Z];

          lum1Vector = (lum1Vector * lumScaleVector) + chromVector;

          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int) lum1Vector.X];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(lum1Vector.Y + lum1Vector.W)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int) lum1Vector.Z];
  #else
          var y0 = (short)(*(yuv++) -  16);
          var cb = (short)(*(yuv++) - 128);
          var y1 = (short)(*(yuv++) -  16);
          var cr = (short)(*(yuv++) - 128);

          var   lumVector = new Vector8s(y0, y0, y0,  0, y1, y1, y1,  0);
          var chromVector = new Vector8s(cb, cb, cr, cr, cb, cb, cr, cr);

          lumVector = ((lumVector * lumScaleVector) + (chromVector * chromScaleVector)) >> 6;

          *(bgr++) = yuv422CropTable[yuv422CropRangeMax +  lumVector.V0];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (lumVector.V1 + lumVector.V3)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax +  lumVector.V2];

          *(bgr++) = yuv422CropTable[yuv422CropRangeMax +  lumVector.V4];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (lumVector.V5 + lumVector.V7)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax +  lumVector.V6];
  #endif
#else // if SIMD
  #if YUV_FLOAT
          var y0 = +1.164f * (*(yuv++) -  16);
          var cb =           (*(yuv++) - 128);
          var y1 = +1.164f * (*(yuv++) -  16);
          var cr =           (*(yuv++) - 128);

          var db = (+2.018f * cb               );
          var dg = (-0.391f * cb + -0.813f * cr);
          var dr = (               +1.596f * cr);

          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(y0 + db)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(y0 + dg)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(y0 + dr)];

          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(y1 + db)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(y1 + dg)];
          *(bgr++) = yuv422CropTable[yuv422CropRangeMax + (int)(y1 + dr)];
  #else
          // scaled by 2^10
          var y0 = +1192 * (*(yuv++) -  16);
          var cb =         (*(yuv++) - 128);
          var y1 = +1192 * (*(yuv++) -  16);
          var cr =         (*(yuv++) - 128);

          var db = (+2066 * cb             );
          var dg = (-0400 * cb + -0833 * cr);
          var dr = (             +1634 * cr);

          *(bgr++) = Yuv422CropTable[Yuv422CropRangeMax + ((y0 + db) >> 10)];
          *(bgr++) = Yuv422CropTable[Yuv422CropRangeMax + ((y0 + dg) >> 10)];
          *(bgr++) = Yuv422CropTable[Yuv422CropRangeMax + ((y0 + dr) >> 10)];

          *(bgr++) = Yuv422CropTable[Yuv422CropRangeMax + ((y1 + db) >> 10)];
          *(bgr++) = Yuv422CropTable[Yuv422CropRangeMax + ((y1 + dg) >> 10)];
          *(bgr++) = Yuv422CropTable[Yuv422CropRangeMax + ((y1 + dr) >> 10)];
  #endif
#endif
        }
      }
    }

    internal static unsafe void ConvertPackedYUV422ToArgb(byte* yuv422, int yuv422Stride, int width, int height, byte* argb, int rgbStride)
    {
#if DEBUG
      if ((width & 0x1) != 0x0)
        throw ExceptionUtils.CreateArgumentMustBeMultipleOf(2, "width");
#endif

      width >>= 1;

      var srcScanLine  = yuv422;
      var destScanLine = argb;

      for (var y = 0; y < height; y++) {
        var yuv = srcScanLine;
        var bgra = destScanLine;

        for (var x = 0; x < width; x++) {
          // scaled by 2^10
          var y0 = +1192 * (yuv[0] -  16);
          var cb =         (yuv[1] - 128);
          var y1 = +1192 * (yuv[2] -  16);
          var cr =         (yuv[3] - 128);

          var db = (+2066 * cb             );
          var dg = (-0400 * cb + -0833 * cr);
          var dr = (             +1634 * cr);

          bgra[0] = Yuv422CropTable[Yuv422CropRangeMax + ((y0 + db) >> 10)];
          bgra[1] = Yuv422CropTable[Yuv422CropRangeMax + ((y0 + dg) >> 10)];
          bgra[2] = Yuv422CropTable[Yuv422CropRangeMax + ((y0 + dr) >> 10)];
          bgra[3] = 0xff;

          bgra[4] = Yuv422CropTable[Yuv422CropRangeMax + ((y1 + db) >> 10)];
          bgra[5] = Yuv422CropTable[Yuv422CropRangeMax + ((y1 + dg) >> 10)];
          bgra[6] = Yuv422CropTable[Yuv422CropRangeMax + ((y1 + dr) >> 10)];
          bgra[7] = 0xff;

          yuv   += 4;
          bgra  += 8;
        }

        srcScanLine   += yuv422Stride;
        destScanLine  += rgbStride;
      }
    }

    internal static unsafe void ConvertPackedYUV422ToDeinterlacedXrgb(byte* yuv422,
                                                                      int yuv422Stride,
                                                                      int width,
                                                                      int height,
                                                                      byte* xrgb,
                                                                      int xrgbStride,
                                                                      bool topField)
    {
#if DEBUG
      if ((width & 0x1) != 0x0)
        throw ExceptionUtils.CreateArgumentMustBeMultipleOf(2, "width");
#endif

      width >>= 1;

      var yuvLine0 = yuv422;
      var xrgbLine0 = xrgb;
      var y = 0;

      if (!topField) {
        yuvLine0 += yuv422Stride;
        xrgbLine0 += xrgbStride;
        y = 1;
      }

      var xrgbStride2 = xrgbStride << 1;

      yuv422Stride <<= 1;

      for (; y < height - 2; y += 2) {
        var yuvLine1 = yuvLine0 + yuv422Stride;
        var xrgbLine1 = xrgbLine0 + xrgbStride;
        var yuv0 = yuvLine0;
        var yuv1 = yuvLine1;
        var bgrx0 = xrgbLine0;
        var bgrx1 = xrgbLine1;

        for (var x = 0; x < width; x++) {
          var l0y0 =  +1192 * (yuv0[0] -  16);
          var l0cb =          (yuv0[1] - 128);
          var l0y1 =  +1192 * (yuv0[2] -  16);
          var l0cr =          (yuv0[3] - 128);
          var l1y0 = (+1192 * (yuv1[0] -  16) + l0y0 + 1) >> 1;
          var l1cb = (        (yuv1[1] - 128) + l0cb + 1) >> 1;
          var l1y1 = (+1192 * (yuv1[2] -  16) + l0y1 + 1) >> 1;
          var l1cr = (        (yuv1[3] - 128) + l0cr + 1) >> 1;

          var db = (+2066 * l0cb               );
          var dg = (-0400 * l0cb + -0833 * l0cr);
          var dr = (               +1634 * l0cr);

          bgrx0[0] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l0y0 + db) >> 10)];
          bgrx0[1] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l0y0 + dg) >> 10)];
          bgrx0[2] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l0y0 + dr) >> 10)];
          bgrx0[3] = 0xff;

          bgrx0[4] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l0y1 + db) >> 10)];
          bgrx0[5] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l0y1 + dg) >> 10)];
          bgrx0[6] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l0y1 + dr) >> 10)];
          bgrx0[7] = 0xff;

          db = (+2066 * l1cb               );
          dg = (-0400 * l1cb + -0833 * l1cr);
          dr = (               +1634 * l1cr);

          bgrx1[0] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l1y0 + db) >> 10)];
          bgrx1[1] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l1y0 + dg) >> 10)];
          bgrx1[2] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l1y0 + dr) >> 10)];
          bgrx1[3] = 0xff;

          bgrx1[4] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l1y1 + db) >> 10)];
          bgrx1[5] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l1y1 + dg) >> 10)];
          bgrx1[6] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((l1y1 + dr) >> 10)];
          bgrx1[7] = 0xff;

          yuv0  += 4;
          yuv1  += 4;
          bgrx0 += 8;
          bgrx1 += 8;
        }

        yuvLine0 = yuvLine1;
        xrgbLine0 += xrgbStride2;
      }

      // last line
      for (var x = 0; x < width; x++) {
        var y0 =  +1192 * (yuvLine0[0] -  16);
        var cb =          (yuvLine0[1] - 128);
        var y1 =  +1192 * (yuvLine0[2] -  16);
        var cr =          (yuvLine0[3] - 128);

        var db = (+2066 * cb             );
        var dg = (-0400 * cb + -0833 * cr);
        var dr = (             +1634 * cr);

        xrgbLine0[0] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y0 + db) >> 10)];
        xrgbLine0[1] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y0 + dg) >> 10)];
        xrgbLine0[2] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y0 + dr) >> 10)];
        xrgbLine0[3] = 0xff;

        xrgbLine0[4] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y1 + db) >> 10)];
        xrgbLine0[5] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y1 + dg) >> 10)];
        xrgbLine0[6] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y1 + dr) >> 10)];
        xrgbLine0[7] = 0xff;

        yuvLine0 += 4;
        xrgbLine0 += 8;
      }
    }

    private static void InitializeYuv422CropTable()
    {
      Yuv422CropTable = new byte[0x100 + Yuv422CropRangeMax * 2];

      for (var i = 0x00; i < 0x100; i++) {
        Yuv422CropTable[i + Yuv422CropRangeMax] = (byte)i;
      }

      for (var i = 0; i < Yuv422CropRangeMax; i++) {
        Yuv422CropTable[i] = 0x00;
        Yuv422CropTable[i + 0x100 + Yuv422CropRangeMax] = 0xff;
      }
    }

    internal const int Yuv422CropRangeMax = 192;
    internal static byte[] Yuv422CropTable;

#if YUV_SIMD
  #if YUV_FLOAT
    private static readonly Vector4f   lumScaleVector = new Vector4f(+1.164f, +1.164f, +1.164f,    0.0f);
    private static readonly Vector4f chromScaleVector = new Vector4f(+2.018f, -0.391f, +1.596f, -0.813f);
  #else
    // scaled by 2^6
    private static readonly Vector8s   lumScaleVector = new Vector8s(+ 74, + 74, + 74,    0, + 74, + 74, + 74,    0);
    private static readonly Vector8s chromScaleVector = new Vector8s(+129, - 25, +102, - 52, +129, - 25, +102, - 52);
  #endif
#endif
  }
}
