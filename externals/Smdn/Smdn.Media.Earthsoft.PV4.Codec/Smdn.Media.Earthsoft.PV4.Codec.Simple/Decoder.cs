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
  internal sealed class Decoder : Codec.Decoder {
    private class DecodingContext {
      public AutoResetEvent ThreadWaitHandle {
        get; private set;
      }

      public BitStream Stream;

      public IntPtr Buffer {
        get; private set;
      }

      public int BufferStride {
        get; private set;
      }

      public InverseMacroBlockFunction InverseMacroBlock {
        get; private set;
      }

      public Smpte370MDct Dct;
      public MacroBlockArrangement MacroBlockArrangement;
      public DctBlockInfo[] DctBlocks;

      public readonly ushort[] LuminanceQuants   = new ushort[64];
      public readonly ushort[] ChrominanceQuants = new ushort[64];

      public DecodingContext(DV dv, MacroBlockArrangement arrangement, CreateDctHandler createDct)
        : base()
      {
        Dct = createDct();
        MacroBlockArrangement = arrangement;

        for (var i = 0; i < 64; i++) {
          LuminanceQuants  [i] = dv.Header.LuminanceQuantizerTable  [ZigZag.InverseZigZagIndices[i]];
          ChrominanceQuants[i] = dv.Header.ChrominanceQuantizerTable[ZigZag.InverseZigZagIndices[i]];
        }
      }

      public void SetFrameData(ArraySegment<byte> data, IntPtr buffer, int bufferStride,
                               InverseMacroBlockFunction inverseMacroBlock, AutoResetEvent threadWaitHandle)
      {
        Stream = new BitStream(data);
        Buffer = buffer;
        BufferStride = bufferStride;
        InverseMacroBlock = inverseMacroBlock;
        ThreadWaitHandle = threadWaitHandle;
      }
    }

    internal Decoder(DV dv, int threadCount, CreateDctHandler createDct)
      : base(dv, threadCount)
    {
      decodingContexts = Array.ConvertAll(MacroBlockArrangement.Create(dv), delegate(MacroBlockArrangement arrangement) {
        return new DecodingContext(dv, arrangement, createDct);
      });

#if DEBUG
      Console.Error.WriteLine("using DCT implementation: {0}", decodingContexts[0].Dct.GetType().FullName);
#endif
    }

    protected unsafe override void DecodeFrameAsPackedYUV422(int frame, IntPtr buffer, int bufferStride, out Fraction displayAspectRatio)
    {
      DecodeFrameCore(frame,
                      buffer,
                      bufferStride,
                      InitializeDecodingContextPackedYUV422,
                      InverseAndArrangeMacroBlockPackedYUV422,
                      out displayAspectRatio);
    }

    protected unsafe override void DecodeFrameAsXrgb(int frame, IntPtr buffer, int bufferStride, out Fraction displayAspectRatio)
    {
      DecodeFrameCore(frame,
                      buffer,
                      bufferStride,
                      InitializeDecodingContextXrgb,
                      InverseAndArrangeMacroBlockXrgb,
                      out displayAspectRatio);
    }

    private void DecodeFrameCore(int frame,
                                 IntPtr buffer,
                                 int bufferStride,
                                 Action<DecodingContext> initializeContext,
                                 InverseMacroBlockFunction inverseMacroBlock,
                                 out Fraction displayAspectRatio)
    {
      DV.GetVideo(frame, ref frameBuffer);

      displayAspectRatio = frameBuffer.Video.DisplayAspectRatio;

      if (0 < ThreadCount) {
        for (var i = 0; i < ThreadCount; i++) {
          ThreadWaitHandles[i].Set();
        }

        for (var index = 0; index < decodingContexts.Length; index++) {
          var threadWaitHandle = ThreadWaitHandles[WaitHandle.WaitAny(ThreadWaitHandles)];

          decodingContexts[index].SetFrameData(frameBuffer.Video.GetBlock(index), buffer, bufferStride, inverseMacroBlock, threadWaitHandle);

          if (initializeContext != null)
            initializeContext(decodingContexts[index]);

          ThreadPool.QueueUserWorkItem(DecodeBlock, decodingContexts[index]);
        }

        WaitHandle.WaitAll(ThreadWaitHandles);
      }
      else {
        for (var index = 0; index < decodingContexts.Length; index++) {
          decodingContexts[index].SetFrameData(frameBuffer.Video.GetBlock(index), buffer, bufferStride, inverseMacroBlock, null);

          if (initializeContext != null)
            initializeContext(decodingContexts[index]);

          DecodeBlock(decodingContexts[index]);
        }
      }
    }

    private unsafe static void DecodeBlock(object state)
    {
      var context = state as DecodingContext;

      var dequantizedCoefs      = stackalloc short[64 * 8];

      var macroBlockArrangement = context.MacroBlockArrangement;
      var inverseMacroBlock     = context.InverseMacroBlock;
      var st                    = context.Stream;
      var buffer                = (byte*)context.Buffer.ToPointer();
      var macroBlockX           = 0;
      var macroBlockY           = macroBlockArrangement.VideoBlockIndex;
      var macroBlockYStep       = macroBlockArrangement.Interlaced ? 4 : 2;
      var padding = false;
      var isFieldMode = false;

      try {
        for (var macroBlock = 0; macroBlock < macroBlockArrangement.MacroBlockCount; macroBlock++) {
          for (var i = 0; i < 64 * 8; i++) {
            dequantizedCoefs[i] = 0;
          }

          try {
            if (context.MacroBlockArrangement.Interlaced) {
              isFieldMode = ((st.Array[st.ByteOffset] & st.BitMask) != 0); // M

              if ((st.BitMask >>= 1) == 0) {
                st.BitMask = 0x80;
                st.ByteOffset++;
              }
            }

            Smpte370MVlc.DecodeEarthsoftPV4MacroBlock(dequantizedCoefs,
                                                      ref st,
                                                      context.Dct.InverseZigZag,
                                                      context.LuminanceQuants,
                                                      context.ChrominanceQuants);

            inverseMacroBlock(context,
                              buffer,
                              dequantizedCoefs,
                              macroBlockX,
                              macroBlockY,
                              isFieldMode,
                              (macroBlockArrangement.MacroBlock32x8LineY == macroBlockY));
          }
          catch (Exception ex) {
            throw new System.IO.InvalidDataException(string.Format("unexpected exception occured while decoding: video block = {0}, macro block = {1}/{2}",
                                                                   macroBlockArrangement.VideoBlockIndex,
                                                                   macroBlock,
                                                                   macroBlockArrangement.MacroBlockCount),
                                                     ex);
          }

          if (++macroBlockX == macroBlockArrangement.MacroBlockPerLineCount) {
            macroBlockX  = 0;
            macroBlockY += macroBlockYStep;

            if (0 < macroBlockArrangement.VideoBlockIndex && !padding && macroBlockArrangement.MacroBlockPaddingY <= macroBlockY) {
              padding = true;
              macroBlockYStep = 1;
              macroBlockX = macroBlockArrangement.MacroBlockPaddingX;
              macroBlockY = macroBlockArrangement.MacroBlockPaddingY;
            }
          }
        } // for each macro block
      }
      finally {
        if (context.ThreadWaitHandle != null)
          context.ThreadWaitHandle.Set();
      }
    }

    // arrangement of standard DCT block
    //   [Y0] [Y2]
    //   [Y1] [Y3]
    private static readonly int[] standardLuminanceOffsetsX   = new[] {0, 0, 8, 8};
    private static readonly int[] standardLuminanceOffsetsY   = new[] {0, 8, 0, 8};
    //   [Cr0], [Cb0]
    //   [Cr1], [Cb1]
    private static readonly int[] standardChrominanceOffsetsX = new[] {0, 0, 0, 0};
    private static readonly int[] standardChrominanceOffsetsY = new[] {0, 8, 0, 8};

    // arrangement of field-mode DCT block
    private static readonly int[] fieldModeLuminanceOffsetsX   = new[] {0, 0, 8, 8};
    private static readonly int[] fieldModeLuminanceOffsetsY   = new[] {0, 1, 0, 1};
    private static readonly int[] fieldModeChrominanceOffsetsX = new[] {0, 0, 0, 0};
    private static readonly int[] fieldModeChrominanceOffsetsY = new[] {0, 1, 0, 1};

    // arrangement of 32x8 DCT block
    //   [Y0] [Y2] [Y1] [Y3]
    private static readonly int[] bottomMostLuminanceOffsetsX   = new[] { 0, 16,  8, 24};
    private static readonly int[] bottomMostLuminanceOffsetsY   = new[] { 0,  0,  0,  0};
    //   [Cr0] [Cr1], [Cb0] [Cb1]
    private static readonly int[] bottomMostChrominanceOffsetsX = new[] { 0, 16,  0, 16};
    private static readonly int[] bottomMostChrominanceOffsetsY = new[] { 0,  0,  0,  0};

    private unsafe delegate void InverseMacroBlockFunction(DecodingContext context,
                                                           byte* destBuffer,
                                                           short* dequantizedCoefs,
                                                           int macroBlockX,
                                                           int macroBlockY,
                                                           bool isFieldMode,
                                                           bool is32x8MacroBlock);

    private static void InitializeDecodingContextPackedYUV422(DecodingContext context)
    {
      context.DctBlocks = new DctBlockInfo[8];

      context.DctBlocks[0].BytesPerPixel = 2; // Y0
      context.DctBlocks[1].BytesPerPixel = 2; // Y1
      context.DctBlocks[2].BytesPerPixel = 2; // Y2
      context.DctBlocks[3].BytesPerPixel = 2; // Y3
      context.DctBlocks[4].BytesPerPixel = 4; // Cr0
      context.DctBlocks[5].BytesPerPixel = 4; // Cr1
      context.DctBlocks[6].BytesPerPixel = 4; // Cb0
      context.DctBlocks[7].BytesPerPixel = 4; // Cb1
    }

    private unsafe static void InverseAndArrangeMacroBlockPackedYUV422(DecodingContext context,
                                                                       byte* buffer,
                                                                       short* dequantizedCoefs,
                                                                       int macroBlockX,
                                                                       int macroBlockY,
                                                                       bool isFieldMode,
                                                                       bool is32x8MacroBlock)
    {
      // arrangement of Y0-Y3 DCT block:
      var luminanceOffsetsX = standardLuminanceOffsetsX;
      var luminanceOffsetsY = standardLuminanceOffsetsY;

      // arrangement of Cr0-Cr1, Cb0-Cb1 DCT block:
      var chrominanceOffsetsX = standardChrominanceOffsetsX;
      var chrominanceOffsetsY = standardChrominanceOffsetsY;

      var arrangeX = macroBlockX << 4; // * 16
      var arrangeY = macroBlockY << 4; // * 16
      var arrangeStride = context.BufferStride;

      if (isFieldMode) {
        /* rearrangement of pixels in the field mode:
         *   DCT block 1 (Y0, Y2, Cb0, Cr0)         Rearranged DCT block 1
         *     A00 A01 A02 A03 A04 A05 A06 A07        A00 A01 A02 A03 A04 A05 A06 A07
         *     A10 A11 A12 A13 A14 A15 A16 A17        A20 A21 A22 A23 A24 A25 A26 A27
         *     A20 A21 A22 A23 A24 A25 A26 A27        A40 A41 A42 A43 A44 A45 A46 A47
         *     A30 A31 A32 A33 A34 A35 A36 A37        A60 A61 A62 A63 A64 A65 A66 A67
         *     A40 A41 A42 A43 A44 A45 A46 A47   =>   B00 B01 B02 B03 B04 B05 B06 B07
         *     A50 A51 A52 A53 A54 A55 A56 A57        B20 B21 B22 B23 B24 B25 B26 B27
         *     A60 A61 A62 A63 A64 A65 A66 A67        B40 B41 B42 B43 B44 B45 B46 B47
         *     A70 A71 A72 A73 A74 A75 A76 A77        B60 B61 B62 B63 B64 B65 B66 B67
         *
         *   DCT block 2 (Y1, Y3, Cb1, Cr1)         Rearranged DCT block 1
         *     B00 B01 B02 B03 B04 B05 B06 B07        A10 A11 A12 A13 A14 A15 A16 A17
         *     B10 B11 B12 B13 B14 B15 B16 B17        A30 A31 A32 A33 A34 A35 A36 A37
         *     B20 B21 B22 B23 B24 B25 B26 B27        A50 A51 A52 A53 A54 A55 A56 A57
         *     B30 B31 B32 B33 B34 B35 B36 B37        A70 A71 A72 A73 A74 A75 A76 A77
         *     B40 B41 B42 B43 B44 B45 B46 B47   =>   B10 B11 B12 B13 B14 B15 B16 B17
         *     B50 B51 B52 B53 B54 B55 B56 B57        B30 B31 B32 B33 B34 B35 B36 B37
         *     B60 B61 B62 B63 B64 B65 B66 B67        B50 B51 B52 B53 B54 B55 B56 B57
         *     B70 B71 B72 B73 B74 B75 B76 B77        B70 B71 B72 B73 B74 B75 B76 B77
         */
        arrangeStride <<= 1;

        luminanceOffsetsX   = fieldModeLuminanceOffsetsX;
        luminanceOffsetsY   = fieldModeLuminanceOffsetsY;
        chrominanceOffsetsX = fieldModeChrominanceOffsetsX;
        chrominanceOffsetsY = fieldModeChrominanceOffsetsY;
      }
      else if (is32x8MacroBlock) {
        /*
         * DCT blocks at bottom-most
         * each macro blocks has 32x8 pixels.
         */
        arrangeX <<= 1; // * 2

        luminanceOffsetsX   = bottomMostLuminanceOffsetsX;
        luminanceOffsetsY   = bottomMostLuminanceOffsetsY;
        chrominanceOffsetsX = bottomMostChrominanceOffsetsX;
        chrominanceOffsetsY = bottomMostChrominanceOffsetsY;
      }

      // luminance (Y0, Y1, Y2, Y3) DCT block
      for (var y = 0; y < 4; y++) {
        context.DctBlocks[y].Buffer = buffer + (arrangeX + luminanceOffsetsX[y]) * 2 + (arrangeY + luminanceOffsetsY[y]) * context.BufferStride;
        context.DctBlocks[y].Stride = arrangeStride;
        context.DctBlocks[y].Coefficients = dequantizedCoefs;

        dequantizedCoefs += 64;
      }

      // chrominance (Cr0, Cr1) DCT block
      for (var c = 0; c < 2; c++) {
        context.DctBlocks[4 + c].Buffer = buffer + ((arrangeX + chrominanceOffsetsX[c]) * 2 + 3) + (arrangeY + chrominanceOffsetsY[c]) * context.BufferStride;
        context.DctBlocks[4 + c].Stride = arrangeStride;
        context.DctBlocks[4 + c].Coefficients = dequantizedCoefs;

        dequantizedCoefs += 64;
      }

      // chrominance (Cb0, Cb1) DCT block
      for (var c = 2; c < 4; c++) {
        context.DctBlocks[4 + c].Buffer = buffer + ((arrangeX + chrominanceOffsetsX[c]) * 2 + 1) + (arrangeY + chrominanceOffsetsY[c]) * context.BufferStride;
        context.DctBlocks[4 + c].Stride = arrangeStride;
        context.DctBlocks[4 + c].Coefficients = dequantizedCoefs;

        dequantizedCoefs += 64;
      }

      // IDCT and arrange DCT block
      context.Dct.InverseDct(context.DctBlocks);
    }

    private static readonly int[] inversedDctBlockOffsets = new[] { 0, 32, 16, 48};

    private static void InitializeDecodingContextXrgb(DecodingContext context)
    {
      const int yuvBufferStride = 8 * 8; // 8 blocks * each blocks are 8 pixels width

      context.DctBlocks = new DctBlockInfo[8];

      context.DctBlocks[0].BytesPerPixel = 2; context.DctBlocks[0].Stride = yuvBufferStride; // Y0
      context.DctBlocks[1].BytesPerPixel = 2; context.DctBlocks[1].Stride = yuvBufferStride; // Y1
      context.DctBlocks[2].BytesPerPixel = 2; context.DctBlocks[2].Stride = yuvBufferStride; // Y2
      context.DctBlocks[3].BytesPerPixel = 2; context.DctBlocks[3].Stride = yuvBufferStride; // Y3
      context.DctBlocks[4].BytesPerPixel = 4; context.DctBlocks[4].Stride = yuvBufferStride; // Cr0
      context.DctBlocks[5].BytesPerPixel = 4; context.DctBlocks[5].Stride = yuvBufferStride; // Cr1
      context.DctBlocks[6].BytesPerPixel = 4; context.DctBlocks[6].Stride = yuvBufferStride; // Cb0
      context.DctBlocks[7].BytesPerPixel = 4; context.DctBlocks[7].Stride = yuvBufferStride; // Cb1
    }

    private unsafe static void InverseAndArrangeMacroBlockXrgb(DecodingContext context,
                                                               byte* buffer,
                                                               short* dequantizedCoefs,
                                                               int macroBlockX,
                                                               int macroBlockY,
                                                               bool isFieldMode,
                                                               bool is32x8MacroBlock)
    {
      //    offset 0    1     2    3       16                      32   33    34   35      48   49    50   51
      // line0     [Y0] [Cb0] (Y0) [Cr0]   [Y2] (Cb0) (Y2) (Cr0)   [Y1] [Cb1] (Y1) [Cr1]   [Y3] (Cb1) (Y1) (Cr1)
      //   |  
      // line7
      const int yuvBufferStride = 8 * 8; // 8 blocks * each blocks are 8 pixels width
      var yuvBuffer = stackalloc byte[yuvBufferStride * 8];

      // arrangement of converted block:
      var arrangeOffsetsX = standardLuminanceOffsetsX;
      var arrangeOffsetsY = standardLuminanceOffsetsY;

      var arrangeX = macroBlockX << 4; // * 16
      var arrangeY = macroBlockY << 4; // * 16
      var arrangeStride = context.BufferStride;

      if (isFieldMode) {
        arrangeStride <<= 1;

        arrangeOffsetsX = fieldModeLuminanceOffsetsX;
        arrangeOffsetsY = fieldModeLuminanceOffsetsY;
      }
      else if (is32x8MacroBlock) {
        arrangeX <<= 1;

        arrangeOffsetsX = bottomMostLuminanceOffsetsX;
        arrangeOffsetsY = bottomMostLuminanceOffsetsY;
      }

      // IDCT and arrange DCT block
      context.DctBlocks[0].Buffer = yuvBuffer +  0; // Y0
      context.DctBlocks[1].Buffer = yuvBuffer + 32; // Y1
      context.DctBlocks[2].Buffer = yuvBuffer + 16; // Y2
      context.DctBlocks[3].Buffer = yuvBuffer + 48; // Y3
      context.DctBlocks[4].Buffer = yuvBuffer +  3 /*  0 + 3 */; // Cr0
      context.DctBlocks[5].Buffer = yuvBuffer + 35 /* 32 + 3 */; // Cr1
      context.DctBlocks[6].Buffer = yuvBuffer +  1 /*  0 + 1 */; // Cb0
      context.DctBlocks[7].Buffer = yuvBuffer + 33 /* 32 + 1 */; // Cb1

      context.DctBlocks[0].Coefficients = dequantizedCoefs; // Y0
      context.DctBlocks[1].Coefficients = dequantizedCoefs + 0x040 /* 1 * 64 */; // Y1
      context.DctBlocks[2].Coefficients = dequantizedCoefs + 0x080 /* 2 * 64 */; // Y2
      context.DctBlocks[3].Coefficients = dequantizedCoefs + 0x0c0 /* 3 * 64 */; // Y3
      context.DctBlocks[4].Coefficients = dequantizedCoefs + 0x100; // Cr0
      context.DctBlocks[5].Coefficients = dequantizedCoefs + 0x140; // Cr1
      context.DctBlocks[6].Coefficients = dequantizedCoefs + 0x180; // Cb0
      context.DctBlocks[7].Coefficients = dequantizedCoefs + 0x1c0; // Cb1

      context.Dct.InverseDct(context.DctBlocks);

      // convert to rgb and arrange macro block
      for (var block = 0; block < 4; block++) {
        var yuvScanLine = yuvBuffer + inversedDctBlockOffsets[block];
        var bgrxScanLine = buffer
                           + ((arrangeX + arrangeOffsetsX[block]) << 2) // * 4 (bytes per argb pixel)
                           + ((arrangeY + arrangeOffsetsY[block]) * context.BufferStride);

        for (var y = 0; y < 8; y++) {
          var bgrx = bgrxScanLine;
          var yuv = yuvScanLine;

          for (var x = 0; x < 4; x++) {
            var y0 = +1192 * (yuv[0] -  16);
            var cb =         (yuv[1] - 128);
            var y1 = +1192 * (yuv[2] -  16);
            var cr =         (yuv[3] - 128);

            var db = (+2066 * cb             );
            var dg = (-0400 * cb + -0833 * cr);
            var dr = (             +1634 * cr);

            bgrx[0] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y0 + db) >> 10)];
            bgrx[1] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y0 + dg) >> 10)];
            bgrx[2] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y0 + dr) >> 10)];
            bgrx[3] = 0xff;

            bgrx[4] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y1 + db) >> 10)];
            bgrx[5] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y1 + dg) >> 10)];
            bgrx[6] = CodecUtils.Yuv422CropTable[CodecUtils.Yuv422CropRangeMax + ((y1 + dr) >> 10)];
            bgrx[7] = 0xff;

            yuv   += 4;
            bgrx  += 8;
          }

          bgrxScanLine += arrangeStride;
          yuvScanLine += yuvBufferStride;
        }
      }
    }

    private readonly DecodingContext[] decodingContexts;
    private StreamFileFrameData frameBuffer;
  }
}
