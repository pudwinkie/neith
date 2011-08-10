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

namespace Smdn.Mathematics.SignalProcessing.Transforms {
  public sealed class Smpte370MIntegerSisdDct : Smpte370MDct {
#region "class members"
    static Smpte370MIntegerSisdDct()
    {
      var sqrt2 = 0.5 / Math.Sqrt(2.0);
      var index = 0;

      for (var y = 0; y < 8; y++) {
        for (var v = 0; v < 8; v++) {
          var cos = (v == 0) ? sqrt2 : (0.5 * Math.Cos((v * (2 * y + 1) * Math.PI) / 16.0));

          cosineTable[index++] = (int)(cos * (1 << cosineScale) + 0.5);
        }
      }
    }

    // pre-calculated values
    private const int cosineScale = 16; // 2^16
    private static readonly int[] cosineTable = new int[8 * 8];
#endregion

#region "instance members"
    public Smpte370MIntegerSisdDct()
      : base(ZigZag.ForwardZigZagIndices, ZigZag.TransposedInverseZigZagIndices)
    {
    }

    public override unsafe void InverseDct(DctBlockInfo[] blocks)
    {
      var sumY = stackalloc short[64];

      foreach (var block in blocks) {
        fixed (int* cosTable = cosineTable) {
          { // IDCT-Y
            var sum = sumY;
            var cos = cosTable;

            for (var y = 0; y < 8; y++, cos += 8) {
              var coefs = block.Coefficients;

              for (var u = 0; u < 8; u++) {
                int s = 0;

                for (var v = 0; v < 8; v++) {
                  s += (*(coefs++) * cos[v]);
                }

                *(sum++) = (short)(s >> cosineScale);
              }
            }
          }

          { // IDCT-X
            var line = block.Buffer;
            var coefs = sumY;

            for (var y = 0; y < 8; y++, line += block.Stride, coefs += 8) {
              var putAt = line;
              var cos = cosTable;

              for (var x = 0; x < 8; x++, putAt += block.BytesPerPixel) {
                int p = 0;

                for (var u = 0; u < 8; u++) {
                  p += (coefs[u] * *(cos++));
                }

                *putAt = cropTable[cropRangeMax + ((1024 + (p >> cosineScale)) >> 3)];
              }
            }
          }
        }
      }
    }
#endregion
  }
}
