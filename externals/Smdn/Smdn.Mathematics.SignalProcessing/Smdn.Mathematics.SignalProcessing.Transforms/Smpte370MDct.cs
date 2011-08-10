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
  /*
   * SMPTE370M inverse DCT:
   *           7   7                  {v(2y+1)   }    {u(2x+1)   }
   * P(x,y) =  S   S  Cv Cu C(u,v) cos{------- pi} cos{------- pi}
   *          v=0 u=0                 {  16      }    {  16      }
   *
   *              where 
   *                   Cu = 0.5 / sqrt(2)  for u = 0
   *                   Cu = 0.5            for u = 1 to 7
   *                   Cv = 0.5 / sqrt(2)  for v = 0
   *                   Cv = 0.5            for v = 1 to 7
   */

  public abstract class Smpte370MDct {
#region "class members"
    static Smpte370MDct()
    {
      for (var i = 0x00; i < 0x100; i++) {
        cropTable[i + cropRangeMax] = (byte)i;
      }

      for (var i = 0; i < cropRangeMax; i++) {
        cropTable[i] = 0x00;
        cropTable[i + 0x100 + cropRangeMax] = 0xff;
      }
    }

    protected const int cropRangeMax = 0x400;
    protected static readonly byte[] cropTable = new byte[0x100 + cropRangeMax * 2];

    public static Smpte370MDct CreateBest()
    {
      if (Runtime.IsSimdRuntimeAvailable)
        return CreateSimdBest();
      else
        return CreateSisdBest();
    }

    public static Smpte370MDct CreateSimdBest()
    {
      if ((Mono.Simd.SimdRuntime.AccelMode & Smpte370MFloatSimdDct.RequiredAcceleration) != 0)
        return new Smpte370MFloatSimdDct();
      else if ((Mono.Simd.SimdRuntime.AccelMode & Smpte370MIntegerSimdDct.RequiredAcceleration) != 0)
        return new Smpte370MIntegerSimdDct();
      else
        return CreateSisdBest();
    }

    public static Smpte370MDct CreateSisdBest()
    {
      return new Smpte370MIntegerSisdDct();
    }
#endregion

#region "instance members"
    public int[] ForwardZigZag {
      get { return forwardZigZag; }
    }

    public int[] InverseZigZag {
      get { return inverseZigZag; }
    }

    protected Smpte370MDct(int[] forwardZigZag, int[] inverseZigZag)
    {
      this.forwardZigZag = (int[])forwardZigZag.Clone();
      this.inverseZigZag = (int[])inverseZigZag.Clone();
    }

    public virtual unsafe void InverseDct(byte* buffer, int bytesPerPixel, int stride, short* coefs)
    {
      InverseDct(new[] {new DctBlockInfo(buffer, bytesPerPixel, stride, coefs)});
    }

    public abstract void InverseDct(DctBlockInfo[] blocks);

    public unsafe void ForwardDct(byte* buffer, int bytesPerPixel, int stride, short* coefs)
    {
      throw new NotImplementedException();
    }

    private int[] forwardZigZag;
    private int[] inverseZigZag;
#endregion
  }
}
