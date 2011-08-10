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
  public sealed class Smpte370MFloatSimdDct : Smpte370MDct {
#region "class members"
    public static AccelMode RequiredAcceleration {
      get
      {
        return AccelMode.SSE1; // operator +, operator *, GetVectorAligned
      }
    }

    static Smpte370MFloatSimdDct()
    {
      var sqrt2 = 0.5 / Math.Sqrt(2.0);
      var index = 0;
      var cosTable = new float[8];

      for (var y = 0; y < 8; y++) {
        for (var v = 0; v < 8; v++) {
          cosTable[v] = (float)((v == 0) ? sqrt2 : (0.5 * Math.Cos((v * (2 * y + 1) * Math.PI) / 16.0)));
        }

        cosineTable[index++] = Mono.Simd.ArrayExtensions.GetVectorAligned(cosTable, 0);
        cosineTable[index++] = Mono.Simd.ArrayExtensions.GetVectorAligned(cosTable, 4);
      }
    }

    // pre-calculated values
    private static readonly Vector4f[] cosineTable = new Vector4f[(8 * 8) / 4];
#endregion

#region "instance members"
    public Smpte370MFloatSimdDct()
      : base(ZigZag.ForwardZigZagIndices, ZigZag.TransposedInverseZigZagIndices)
    {
    }

    public unsafe override void InverseDct(DctBlockInfo[] blocks)
    {
      var sumY = stackalloc float[64];
      var coefsVector = stackalloc Vector4f[16];
      var pp = stackalloc float[4 * 8];

      foreach (var block in blocks) {
        var p = (Vector4f*)pp;

        // short[64] => Vector4f[16]
        var coefficients = block.Coefficients;

        for (var i = 0; i < 16; i++) {
          coefsVector[i] = new Vector4f(*(coefficients++), *(coefficients++), *(coefficients++), *(coefficients++));
        }

        fixed (Vector4f* cosTable = cosineTable) {
          { // IDCT-Y
            var sum = sumY;
            var cos = cosTable;

            for (var y = 0; y < 8; y++) {
              p[0] = (coefsVector[ 0] * cos[0]) + (coefsVector[ 1] * cos[1]);
              p[1] = (coefsVector[ 2] * cos[0]) + (coefsVector[ 3] * cos[1]);
              p[2] = (coefsVector[ 4] * cos[0]) + (coefsVector[ 5] * cos[1]);
              p[3] = (coefsVector[ 6] * cos[0]) + (coefsVector[ 7] * cos[1]);
              p[4] = (coefsVector[ 8] * cos[0]) + (coefsVector[ 9] * cos[1]);
              p[5] = (coefsVector[10] * cos[0]) + (coefsVector[11] * cos[1]);
              p[6] = (coefsVector[12] * cos[0]) + (coefsVector[13] * cos[1]);
              p[7] = (coefsVector[14] * cos[0]) + (coefsVector[15] * cos[1]);

              sum[0] = pp[ 0] + pp[ 1] + pp[ 2] + pp[ 3];
              sum[1] = pp[ 4] + pp[ 5] + pp[ 6] + pp[ 7];
              sum[2] = pp[ 8] + pp[ 9] + pp[10] + pp[11];
              sum[3] = pp[12] + pp[13] + pp[14] + pp[15];
              sum[4] = pp[16] + pp[17] + pp[18] + pp[19];
              sum[5] = pp[20] + pp[21] + pp[22] + pp[23];
              sum[6] = pp[24] + pp[25] + pp[26] + pp[27];
              sum[7] = pp[28] + pp[29] + pp[30] + pp[31];

              cos += 2;
              sum += 8;
            }
          }

          { // IDCT-X
            var line = block.Buffer;
            var coefs = (Vector4f*)sumY;

            for (var y = 0; y < 8; y++) {
              p[0] = (coefs[0] * cosTable[ 0]) + (coefs[1] * cosTable[ 1]);
              p[1] = (coefs[0] * cosTable[ 2]) + (coefs[1] * cosTable[ 3]);
              p[2] = (coefs[0] * cosTable[ 4]) + (coefs[1] * cosTable[ 5]);
              p[3] = (coefs[0] * cosTable[ 6]) + (coefs[1] * cosTable[ 7]);
              p[4] = (coefs[0] * cosTable[ 8]) + (coefs[1] * cosTable[ 9]);
              p[5] = (coefs[0] * cosTable[10]) + (coefs[1] * cosTable[11]);
              p[6] = (coefs[0] * cosTable[12]) + (coefs[1] * cosTable[13]);
              p[7] = (coefs[0] * cosTable[14]) + (coefs[1] * cosTable[15]);

              var putAt = line;

              //                                                        0.5f + (1024.0f + pp[ 0] + pp[ 1] + pp[ 2] + pp[ 3]) / 8.0f
              *(putAt)                        = cropTable[cropRangeMax + (int)((1028.0f + pp[ 0] + pp[ 1] + pp[ 2] + pp[ 3]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[ 4] + pp[ 5] + pp[ 6] + pp[ 7]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[ 8] + pp[ 9] + pp[10] + pp[11]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[12] + pp[13] + pp[14] + pp[15]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[16] + pp[17] + pp[18] + pp[19]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[20] + pp[21] + pp[22] + pp[23]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[24] + pp[25] + pp[26] + pp[27]) / 8.0f)];
              *(putAt += block.BytesPerPixel) = cropTable[cropRangeMax + (int)((1028.0f + pp[28] + pp[29] + pp[30] + pp[31]) / 8.0f)];

              coefs += 2;
              line += block.Stride;
            }
          }
        }
      }
    }
#endregion
  }
}
