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
  public static class Compositions {
    public static void Blend(uint[] composited, int pixels, uint[] image1, byte[] alpha1, uint[] image2, byte[] alpha2)
    {
      for (var index = 0; index < pixels; index++) {
        var a2 = (uint)alpha2[index];
        var a1 = (uint)(0xff - a2);

        var r = (((image1[index] & 0x00ff0000) * a1 + (image2[index] & 0x00ff0000) * a2) / 255) & 0x00ff0000;
        var g = (((image1[index] & 0x0000ff00) * a1 + (image2[index] & 0x0000ff00) * a2) / 255) & 0x0000ff00;
        var b = (((image1[index] & 0x000000ff) * a1 + (image2[index] & 0x000000ff) * a2) / 255) & 0x000000ff;

        /*
        if (0x00ff0000 < r)
          r = 0x00ff0000;
        if (0x0000ff00 < g)
          g = 0x0000ff00;
        if (0x000000ff < b)
          b = 0x000000ff;
          */

        composited[index] = (uint)(r | g | b);
      }
    }

    public static void Add(uint[] composited, int pixels, uint[] image1, byte[] alpha1, uint[] image2, byte[] alpha2)
    {
      for (var index = 0; index < pixels; index++) {
        var r = (image1[index] & 0x00ff0000) + (image2[index] & 0x00ff0000);
        var g = (image1[index] & 0x0000ff00) + (image2[index] & 0x0000ff00);
        var b = (image1[index] & 0x000000ff) + (image2[index] & 0x000000ff);

        if (0x00ff0000 < r)
          r = 0x00ff0000;
        if (0x0000ff00 < g)
          g = 0x0000ff00;
        if (0x000000ff < b)
          b = 0x000000ff;

        composited[index] = (r | g | b);
        /*
        var rmask = r & 0x01000000;
        var gmask = g & 0x00010000;
        var bmask = b & 0x00000100;

        composited[index] = (uint)( ((r | (rmask - (rmask >> 8))) & 0x00ff0000) |
                                       ((g | (gmask - (gmask >> 8))) & 0x0000ff00) |
                                       ((b | (bmask - (bmask >> 8))) & 0x000000ff));
                                       */
      }
    }

    public static void Differentiate(uint[] composited, int pixels, uint[] image1, byte[] alpha1, uint[] image2, byte[] alpha2)
    {
      for (var index = 0; index < pixels; index++) {
        var r = (int)(image1[index] & 0x00ff0000) - (int)(image2[index] & 0x00ff0000);
        var g = (int)(image1[index] & 0x0000ff00) - (int)(image2[index] & 0x0000ff00);
        var b = (int)(image1[index] & 0x000000ff) - (int)(image2[index] & 0x000000ff);

        composited[index] = (uint)(((r < 0 ? -r : r) & 0x00ff0000) |
                                    ((g < 0 ? -g : g) & 0x0000ff00) |
                                    ((b < 0 ? -b : b) & 0x000000ff));
      }
    }

    public static void BitwiseAnd(uint[] composited, int pixels, uint[] image1, byte[] alpha1, uint[] image2, byte[] alpha2)
    {
      for (var index = 0; index < pixels; index++) {
        composited[index] = image1[index] & image2[index];
      }
    }

    public static void BitwiseOr(uint[] composited, int pixels, uint[] image1, byte[] alpha1, uint[] image2, byte[] alpha22)
    {
      for (var index = 0; index < pixels; index++) {
        composited[index] = image1[index] | image2[index];
      }
    }
  }
}