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
using System.Threading;

using Smdn.Formats.Earthsoft.PV4;
using Smdn.Formats.Earthsoft.PV4.IO;
using Smdn.Mathematics;
using Smdn.Mathematics.SignalProcessing.Encoding;
using Smdn.Mathematics.SignalProcessing.Transforms;

namespace Smdn.Media.Earthsoft.PV4.Codec.Simple {
  internal sealed class Encoder : Codec.Encoder {
    private class EncodingContext {
      public AutoResetEvent ThreadWaitHandle {
        get; private set;
      }

      public BitStreamWriter Writer {
        get; private set;
      }

      public IntPtr Buffer {
        get; private set;
      }

      public Smpte370MDct Dct;
      public MacroBlockArrangement MacroBlockArrangement;
      public readonly int BufferStride;

      public readonly ushort[] LuminanceQuants   = new ushort[64];
      public readonly ushort[] ChrominanceQuants = new ushort[64];

      public EncodingContext(DV dv, MacroBlockArrangement arrangement, CreateDctHandler createDct)
        : base()
      {
        Dct = createDct();
        MacroBlockArrangement = arrangement;
        BufferStride = dv.PixelsHorizontal * 2;

        for (var i = 0; i < 64; i++) {
          LuminanceQuants  [i] = dv.Header.LuminanceQuantizerTable  [ZigZag.InverseZigZagIndices[i]];
          ChrominanceQuants[i] = dv.Header.ChrominanceQuantizerTable[ZigZag.InverseZigZagIndices[i]];
        }
      }

      public void SetFrameData(/*byte[] data, IntPtr buffer, */AutoResetEvent threadWaitHandle)
      {
        ThreadWaitHandle = threadWaitHandle;

        throw new NotImplementedException();
      }
    }

    private unsafe struct MacroBlock {
#pragma warning disable 169 // field never used
      private fixed short Elements[64 * 8];
#pragma warning restore 169
    }

    internal Encoder(DV dv, int threadCount, CreateDctHandler createDct)
      : base(dv, threadCount)
    {
      encodingContexts = Array.ConvertAll(MacroBlockArrangement.Create(dv), delegate(MacroBlockArrangement arrangement) {
        return new EncodingContext(dv, arrangement, createDct);
      });

#if DEBUG
      Console.Error.WriteLine("using DCT implementation: {0}", encodingContexts[0].Dct.GetType().FullName);
#endif
    }

    protected override StreamFileFrameData EncodePackedYUV422Frame(IntPtr packedYuv422, Fraction displayAspectRatio)
    {
      throw new NotImplementedException();
      /*
      var frameData = new StreamFileFrameData();

      frameData.DisplayAspectRatio = displayAspectRatio;

      return frameData;
      */
    }

#pragma warning disable 414 // assigned but never used
    private readonly EncodingContext[] encodingContexts;
#pragma warning restore 414
  }
}
