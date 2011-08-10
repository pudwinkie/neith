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
  public static class ZigZag {
    public static readonly int[] ForwardZigZagIndices = new[] {
      00, 01, 05, 06, 14, 15, 27, 28,
      02, 04, 07, 13, 16, 26, 29, 42,
      03, 08, 12, 17, 25, 30, 41, 43,
      09, 11, 18, 24, 31, 40, 44, 53,
      10, 19, 23, 32, 39, 45, 52, 54,
      20, 22, 33, 38, 46, 51, 55, 60,
      21, 34, 37, 47, 50, 56, 59, 61,
      35, 36, 48, 49, 57, 58, 62, 63,
    };

    public static readonly int[] InverseZigZagIndices = new[] {
      00, 01, 08, 16, 09, 02, 03, 10,
      17, 24, 32, 25, 18, 11, 04, 05,
      12, 19, 26, 33, 40, 48, 41, 34,
      27, 20, 13, 06, 07, 14, 21, 28,
      35, 42, 49, 56, 57, 50, 43, 36,
      29, 22, 15, 23, 30, 37, 44, 51,
      58, 59, 52, 45, 38, 31, 39, 46,
      53, 60, 61, 54, 47, 55, 62, 63,
    };

    public static readonly int[] TransposedInverseZigZagIndices = new[] {
      00, 08, 01, 02, 09, 16, 24, 17,
      10, 03, 04, 11, 18, 25, 32, 40,
      33, 26, 19, 12, 05, 06, 13, 20,
      27, 34, 41, 48, 56, 49, 42, 35,
      28, 21, 14, 07, 15, 22, 29, 36,
      43, 50, 57, 58, 51, 44, 37, 30,
      23, 31, 38, 45, 52, 59, 60, 53,
      46, 39, 47, 54, 61, 62, 55, 63,
    };
  }
}
