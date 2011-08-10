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

namespace Smdn.Mathematics.SignalProcessing.Encoding {
  public static class Smpte370MVlc {
    public static unsafe void DecodeACCoefficients(short* coefficients, ref BitStream s,
                                                   int[] coefficientsScantable, ushort[] dequantizationMatrix, int dequantizationScale)
    {
      for (var index = 1;; index++) {
        int run;
        int amplitude;
        var currentNode = Smpte370MVlcCodewords.CodewordTree;

        { // decode codeword
#if DEBUG
          var codeword = 0;
          var codeword_length = 0;
#endif

          for (;;) {
            if ((s.Array[s.ByteOffset] & s.BitMask) == 0x0) {
              currentNode = currentNode.Zero;
#if DEBUG
              codeword = (codeword << 1) | 0;
              codeword_length++;
#endif
            }
            else {
              currentNode = currentNode.One;
#if DEBUG
              codeword = (codeword << 1) | 1;
              codeword_length++;
#endif
            }

            if ((s.BitMask >>= 1) == 0) {
              s.BitMask = 0x80;
              s.ByteOffset++;
            }

#if DEBUG
            if (currentNode == null)
              throw new System.IO.InvalidDataException(string.Format("invalid codeword detected: {0}",
                                                                     Smpte370MVlcCodewords.CodewordToString(codeword, codeword_length)));
            else
#endif
            if (currentNode.HasChild)
              continue;

            run       = currentNode.Run;
            amplitude = currentNode.Amplitude;

#if DEBUG
            if (64 <= index + run)
              throw new System.IO.InvalidDataException(string.Format("run length error detected: index={0} run={1} codeword={2}",
                                                                     index,
                                                                     run,
                                                                     Smpte370MVlcCodewords.CodewordToString(codeword, codeword_length)));
#endif

            if (0 < amplitude) {
              // sign bit
              if ((s.Array[s.ByteOffset] & s.BitMask) != 0)
                amplitude = -amplitude;

              if ((s.BitMask >>= 1) == 0) {
                s.BitMask = 0x80;
                s.ByteOffset++;
              }
            }

            break;
          }
        }

        if (currentNode == Smpte370MVlcCodewords.EobNode)
          return;

        index += run;

        coefficients[coefficientsScantable[index]] = (short)(amplitude * dequantizationMatrix[index] * dequantizationScale);
      }
    }

    public static unsafe void DecodeEarthsoftPV4MacroBlock(short* coefficients,
                                                           ref BitStream s,
                                                           int[] coefficientsScantable,
                                                           ushort[] luminanceDequantizationMatrix,
                                                           ushort[] chrominanceDequantizationMatrix)
    {
      var coefs = coefficients;
      var dequantizationMatrix = luminanceDequantizationMatrix;

      for (var dctBlock = 0; dctBlock < 8; dctBlock++, coefs += 64) {
        if (dctBlock == 4)
          dequantizationMatrix = chrominanceDequantizationMatrix;

        // read DC and Q
        int dc, qscale;
        int bit_offset = 0;

        for (int mask = 0x80; mask != s.BitMask; mask >>= 1, bit_offset++);

        dc  = ((s.Array[s.ByteOffset++] << 8 | s.Array[s.ByteOffset]) >> (7 - bit_offset)) & 0x1ff;
        dc |= -(dc & 0x100); // amplify sign bit

        // DC 成分 は係数 32 で量子化をして 9 ビットにします。
        coefs[0] = (short)(dc << 5);

        if (bit_offset == 7) {
          s.BitMask = 0x80;
          s.ByteOffset++;
        }
        else {
          s.BitMask = (0x80 >> (bit_offset + 1));
        }

        // Q : 0:通常通り ／ 1:全 AC 係数 2 倍 です。
        qscale = (s.Array[s.ByteOffset] & s.BitMask) == 0 ? 1 : 2;

        if ((s.BitMask >>= 1) == 0) {
          s.BitMask = 0x80;
          s.ByteOffset++;
        }

        for (var index = 1;; index++) {
          int run;
          int amplitude;
          var currentNode = Smpte370MVlcCodewords.CodewordTree;

          { // decode codeword
#if DEBUG
            var codeword = 0;
            var codeword_length = 0;
#endif

            for (;;) {
              if ((s.Array[s.ByteOffset] & s.BitMask) == 0x0) {
                currentNode = currentNode.Zero;
#if DEBUG
                codeword = (codeword << 1) | 0;
                codeword_length++;
#endif
              }
              else {
                currentNode = currentNode.One;
#if DEBUG
                codeword = (codeword << 1) | 1;
                codeword_length++;
#endif
              }

              if ((s.BitMask >>= 1) == 0) {
                s.BitMask = 0x80;
                s.ByteOffset++;
              }

#if DEBUG
              if (currentNode == null)
                throw new System.IO.InvalidDataException(string.Format("invalid codeword detected: dctBlock={0}, {1}",
                                                                       dctBlock,
                                                                       Smpte370MVlcCodewords.CodewordToString(codeword, codeword_length)));
              else
#endif
              if (currentNode.HasChild)
                continue;

              run       = currentNode.Run;
              amplitude = currentNode.Amplitude;

#if DEBUG
              if (64 <= index + run)
                throw new System.IO.InvalidDataException(string.Format("run length error detected: dctBlock={0} index={1} run={2} codeword={3}",
                                                                       dctBlock,
                                                                       index,
                                                                       run,
                                                                       Smpte370MVlcCodewords.CodewordToString(codeword, codeword_length)));
#endif

              if (0 < amplitude) {
                // sign bit
                if ((s.Array[s.ByteOffset] & s.BitMask) != 0)
                  amplitude = -amplitude;

                if ((s.BitMask >>= 1) == 0) {
                  s.BitMask = 0x80;
                  s.ByteOffset++;
                }
              }

              break;
            }
          }

          if (currentNode == Smpte370MVlcCodewords.EobNode)
            break;

          index += run;

          coefs[coefficientsScantable[index]] = (short)(amplitude * dequantizationMatrix[index] * qscale);
        } // for index
      } // for dctblock
    }
  }
}
