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
  internal static class Smpte370MVlcCodewords {
    internal class Node {
      public Node Zero = null;
      public Node One = null;
      public readonly bool HasChild;
      public readonly int Run;
      public readonly int Amplitude;

      public Node()
      {
        HasChild  = true;
        Run       = 0;
        Amplitude = 0;
      }

      public Node(int run, int amplitude)
      {
        HasChild  = false;
        Run       = run;
        Amplitude = amplitude;
      }
    }

    internal static readonly Node CodewordTree;
    internal static readonly Node EobNode;

    static Smpte370MVlcCodewords()
    {
      var codewordTable = new[] {
        // 2+1
        new {Run =  0, Amplitude =  1, Codeword = "00"},
        // 3+1
        new {Run =  0, Amplitude =  2, Codeword = "010"},
        // 4
        new {Run = -1, Amplitude = -1, Codeword = "0110"}, // EOB
        // 4+1
        new {Run =  1, Amplitude =  1, Codeword = "0111"},
        new {Run =  0, Amplitude =  3, Codeword = "1000"},
        new {Run =  0, Amplitude =  4, Codeword = "1001"},
        // 5+1
        new {Run =  2, Amplitude =  1, Codeword = "10100"},
        new {Run =  1, Amplitude =  2, Codeword = "10101"},
        new {Run =  0, Amplitude =  5, Codeword = "10110"},
        new {Run =  0, Amplitude =  6, Codeword = "10111"},
        // 6+1
        new {Run =  3, Amplitude =  1, Codeword = "110000"},
        new {Run =  4, Amplitude =  1, Codeword = "110001"},
        new {Run =  0, Amplitude =  7, Codeword = "110010"},
        new {Run =  0, Amplitude =  8, Codeword = "110011"},
        // 7+1
        new {Run =  5, Amplitude =  1, Codeword = "1101000"},
        new {Run =  6, Amplitude =  1, Codeword = "1101001"},
        new {Run =  2, Amplitude =  2, Codeword = "1101010"},
        new {Run =  1, Amplitude =  3, Codeword = "1101011"},
        new {Run =  1, Amplitude =  4, Codeword = "1101100"},
        new {Run =  0, Amplitude =  9, Codeword = "1101101"},
        new {Run =  0, Amplitude = 10, Codeword = "1101110"},
        new {Run =  0, Amplitude = 11, Codeword = "1101111"},
        // 8+1
        new {Run =  7, Amplitude =  1, Codeword = "11100000"},
        new {Run =  8, Amplitude =  1, Codeword = "11100001"},
        new {Run =  9, Amplitude =  1, Codeword = "11100010"},
        new {Run = 10, Amplitude =  1, Codeword = "11100011"},
        new {Run =  3, Amplitude =  2, Codeword = "11100100"},
        new {Run =  4, Amplitude =  2, Codeword = "11100101"},
        new {Run =  2, Amplitude =  3, Codeword = "11100110"},
        new {Run =  1, Amplitude =  5, Codeword = "11100111"},
        new {Run =  1, Amplitude =  6, Codeword = "11101000"},
        new {Run =  1, Amplitude =  7, Codeword = "11101001"},
        new {Run =  0, Amplitude = 12, Codeword = "11101010"},
        new {Run =  0, Amplitude = 13, Codeword = "11101011"},
        new {Run =  0, Amplitude = 14, Codeword = "11101100"},
        new {Run =  0, Amplitude = 15, Codeword = "11101101"},
        new {Run =  0, Amplitude = 16, Codeword = "11101110"},
        new {Run =  0, Amplitude = 17, Codeword = "11101111"},
        // 9+1
        new {Run = 11, Amplitude =  1, Codeword = "111100000"},
        new {Run = 12, Amplitude =  1, Codeword = "111100001"},
        new {Run = 13, Amplitude =  1, Codeword = "111100010"},
        new {Run = 14, Amplitude =  1, Codeword = "111100011"},
        new {Run =  5, Amplitude =  2, Codeword = "111100100"},
        new {Run =  6, Amplitude =  2, Codeword = "111100101"},
        new {Run =  3, Amplitude =  3, Codeword = "111100110"},
        new {Run =  4, Amplitude =  3, Codeword = "111100111"},
        new {Run =  2, Amplitude =  4, Codeword = "111101000"},
        new {Run =  2, Amplitude =  5, Codeword = "111101001"},
        new {Run =  1, Amplitude =  8, Codeword = "111101010"},
        new {Run =  0, Amplitude = 18, Codeword = "111101011"},
        new {Run =  0, Amplitude = 19, Codeword = "111101100"},
        new {Run =  0, Amplitude = 20, Codeword = "111101101"},
        new {Run =  0, Amplitude = 21, Codeword = "111101110"},
        new {Run =  0, Amplitude = 22, Codeword = "111101111"},
        // 10+1
        new {Run =  5, Amplitude =  3, Codeword = "1111100000"},
        new {Run =  3, Amplitude =  4, Codeword = "1111100001"},
        new {Run =  3, Amplitude =  5, Codeword = "1111100010"},
        new {Run =  2, Amplitude =  6, Codeword = "1111100011"},
        new {Run =  1, Amplitude =  9, Codeword = "1111100100"},
        new {Run =  1, Amplitude = 10, Codeword = "1111100101"},
        new {Run =  1, Amplitude = 11, Codeword = "1111100110"},
        // 11
        new {Run =  0, Amplitude =  0, Codeword = "11111001110"},
        new {Run =  1, Amplitude =  0, Codeword = "11111001111"},
        // 11+1
        new {Run =  6, Amplitude =  3, Codeword = "11111010000"},
        new {Run =  4, Amplitude =  4, Codeword = "11111010001"},
        new {Run =  3, Amplitude =  6, Codeword = "11111010010"},
        new {Run =  1, Amplitude = 12, Codeword = "11111010011"},
        new {Run =  1, Amplitude = 13, Codeword = "11111010100"},
        new {Run =  1, Amplitude = 14, Codeword = "11111010101"},
        // 12
        new {Run =  2, Amplitude =  0, Codeword = "111110101100"},
        new {Run =  3, Amplitude =  0, Codeword = "111110101101"},
        new {Run =  4, Amplitude =  0, Codeword = "111110101110"},
        new {Run =  5, Amplitude =  0, Codeword = "111110101111"},
        // 12+1
        new {Run =  7, Amplitude =  2, Codeword = "111110110000"},
        new {Run =  8, Amplitude =  2, Codeword = "111110110001"},
        new {Run =  9, Amplitude =  2, Codeword = "111110110010"},
        new {Run = 10, Amplitude =  2, Codeword = "111110110011"},
        new {Run =  7, Amplitude =  3, Codeword = "111110110100"},
        new {Run =  8, Amplitude =  3, Codeword = "111110110101"},
        new {Run =  4, Amplitude =  5, Codeword = "111110110110"},
        new {Run =  3, Amplitude =  7, Codeword = "111110110111"},
        new {Run =  2, Amplitude =  7, Codeword = "111110111000"},
        new {Run =  2, Amplitude =  8, Codeword = "111110111001"},
        new {Run =  2, Amplitude =  9, Codeword = "111110111010"},
        new {Run =  2, Amplitude = 10, Codeword = "111110111011"},
        new {Run =  2, Amplitude = 11, Codeword = "111110111100"},
        new {Run =  1, Amplitude = 15, Codeword = "111110111101"},
        new {Run =  1, Amplitude = 16, Codeword = "111110111110"},
        new {Run =  1, Amplitude = 17, Codeword = "111110111111"},
      };

      var codewordLength = new[] {
                     /*                                             Amplitude                                               */
                     /*Run    0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18   9  20  21  22  23 */
        new[] /* 0*/ {11,  2,  3,  4,  4,  5,  5,  6,  6,  7,  7,  7,  8,  8,  8,  8,  8,  8,  9,  9,  9,  9,  9, 15},
        new[] /* 1*/ {11,  4,  5,  7,  7,  8,  8,  8,  9, 10, 10, 10, 11, 11, 11, 12, 12, 12, -1, -1, -1, -1, -1, -1},
        new[] /* 2*/ {12,  5,  7,  8,  9,  9, 10, 12, 12, 12, 12, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 3*/ {12,  6,  8,  9, 10, 10, 11, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 4*/ {12,  6,  8,  9, 11, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 5*/ {12,  7,  9, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 6*/ {13,  7,  9, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 7*/ {13,  8, 12, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 8*/ {13,  8, 12, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /* 9*/ {13,  8, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /*10*/ {13,  8, 12, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /*11*/ {13,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /*12*/ {13,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /*13*/ {13,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /*14*/ {13,  9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new[] /*15*/ {13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
      };

      CodewordTree = new Node();

      foreach (var tableEntry in codewordTable) {
        var code = 0;

        for (var index = 0; index < tableEntry.Codeword.Length; index++) {
          if (index != 0)
            code <<= 1;
          code |= (tableEntry.Codeword[index] == '0') ? 0x0 : 0x1;
        }

        if (tableEntry.Codeword == "0110")
          // EOB
          EobNode = InsertIntoCodewordTree(CodewordTree, code, 4, -1, -1);
        else
          InsertIntoCodewordTree(CodewordTree, code, codewordLength[tableEntry.Run][tableEntry.Amplitude], tableEntry.Run, tableEntry.Amplitude);
      }

      for (var run = 6; run <= 61; run++) {
        InsertIntoCodewordTree(CodewordTree, 0x1f80 /* 1111110000000b*/ | run, 13, run, 0);
      }

      for (var amp = 23; amp <= 255; amp++) {
        InsertIntoCodewordTree(CodewordTree, 0x7f00 /* 111111100000000b*/ | amp, 15, 0, amp);
      }
    }

    private static Node InsertIntoCodewordTree(Node root, int code, int length, int run, int amp)
    {
      var current = root;
      var ret = root;

      for (var index = length - 1; 0 <= index; index--) {
        var bit = (code >> index) & 0x1;

        if (index == 0) {
          if (bit == 1) {
#if DEBUG
            if (current.One != null)
              throw new FormatException(string.Concat("invalid codeword: ", CodewordToString(code, length)));
#endif
            ret = current.One = new Node(run, amp);
          }
          else {
#if DEBUG
            if (current.Zero != null)
              throw new FormatException(string.Concat("invalid codeword: ", CodewordToString(code, length)));
#endif
            ret = current.Zero = new Node(run, amp);
          }
        }
        else {
          if (bit == 1) {
            if (current.One == null)
              current.One = new Node();
            ret = current = current.One;
          }
          else {
            if (current.Zero == null)
              current.Zero = new Node();
            ret = current = current.Zero;
          }
        }
      }

      return ret;
    }

#if DEBUG
    internal static string CodewordToString(int codeword, int length)
    {
      var sb = new System.Text.StringBuilder();

      sb.Append("codeword ");
      for (var bit = length - 1; 0 <= bit; bit--) {
        sb.Append((codeword >> bit) & 0x1);
      }
      sb.Append("b, length ");
      sb.Append(length);

      return sb.ToString();
    }
#endif
  }
}
