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

using Mono.Simd;

namespace Smdn.Mathematics.SignalProcessing.Transforms {
  public sealed  class Smpte370MIntegerSimdDct : Smpte370MDct {
#region "class members"
    public static AccelMode RequiredAcceleration {
      get
      {
        return
          AccelMode.SSE1 | // StoreAligned
          AccelMode.SSE2 | // operator >>
          AccelMode.SSE41; // operator *
      }
    }

    static Smpte370MIntegerSimdDct()
    {
      var sqrt2 = 0.5 / Math.Sqrt(2.0);
      var cosTable = new double[64];

      for (var y = 0; y < 8; y++) {
        for (var v = 0; v < 8; v++) {
          cosTable[y * 8 + v] = (v == 0) ? sqrt2 : (0.5 * Math.Cos((v * (2 * y + 1) * Math.PI) / 16.0));
        }
      }

      var cos = 0;

      for (var y = 0; y < 8; y++) {
        for (var x = 0; x < 8; x++) {
          for (var v = 0; v < 8; v++) {
            var cosCv = cosTable[y * 8 + v];
            var cosCu = new double[8];

            for (var u = 0; u < 8; u++) {
              cosCu[u] = (u == 0) ? sqrt2 : (0.5 * Math.Cos((u * (2 * x + 1) * Math.PI) / 16.0));
            }

            cvcuCosineTable[cos++] = Mono.Simd.ArrayExtensions.GetVectorAligned(new[] {
              (int)(cosCv * cosCu[0] * (1 << cvcuCosineTableScale) + 0.5),
              (int)(cosCv * cosCu[1] * (1 << cvcuCosineTableScale) + 0.5),
              (int)(cosCv * cosCu[2] * (1 << cvcuCosineTableScale) + 0.5),
              (int)(cosCv * cosCu[3] * (1 << cvcuCosineTableScale) + 0.5),
            }, 0);

            cvcuCosineTable[cos++] = Mono.Simd.ArrayExtensions.GetVectorAligned(new[] {
              (int)(cosCv * cosCu[4] * (1 << cvcuCosineTableScale) + 0.5),
              (int)(cosCv * cosCu[5] * (1 << cvcuCosineTableScale) + 0.5),
              (int)(cosCv * cosCu[6] * (1 << cvcuCosineTableScale) + 0.5),
              (int)(cosCv * cosCu[7] * (1 << cvcuCosineTableScale) + 0.5),
            }, 0);
          }
        }
      }
    }

    // pre-calculated values
    private const int cvcuCosineTableScale = 16; // 2^16
    private static readonly Vector4i[] cvcuCosineTable = new Vector4i[(8 * 8) * (8 * 8) / 4];
#endregion

#region "instance members"
    public Smpte370MIntegerSimdDct()
      : base(ZigZag.ForwardZigZagIndices, ZigZag.InverseZigZagIndices)
    {
    }

    public override unsafe void InverseDct(DctBlockInfo[] blocks)
    {
      var coefs = stackalloc Vector4i[16];
      var pp = stackalloc int[4];

      foreach (var block in blocks) {
        var p = (Vector4i*)pp;

        // short[64] => Vector4i[16]
        var coefficients = block.Coefficients;

        for (var i = 0; i < 16; i++) {
          Vector4i.StoreAligned(coefs + i, new Vector4i(*(coefficients++), *(coefficients++), *(coefficients++), *(coefficients++)));
        }

        fixed (Vector4i* cosineTable = cvcuCosineTable) {
          var cos = cosineTable;

          for (var y = 0; y < 8; y++) {
            var putAt = block.Buffer + y * block.Stride;

            for (var x = 0; x < 8; x++, putAt += block.BytesPerPixel) {
              var coef = coefs;

              *p = Vector4i.Zero;

              for (var vu = 0; vu < 16; vu++) {
                *p += (*(cos++) * (*coef++));
              }

              *p >>= cvcuCosineTableScale;

              *putAt = cropTable[cropRangeMax + ((1024 + pp[0] + pp[1] + pp[2] + pp[3]) >> 3)];
            }
          }
        }
      }
    }
#endregion
  }
}
