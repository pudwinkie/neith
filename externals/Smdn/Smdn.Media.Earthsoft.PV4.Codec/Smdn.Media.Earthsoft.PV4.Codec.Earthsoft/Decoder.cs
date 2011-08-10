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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Smdn.Formats.Earthsoft.PV4;
using Smdn.Formats.Earthsoft.PV4.IO;
using Smdn.Interop;
using Smdn.Mathematics;
#if NET_4_0
using System.Threading.Tasks;
#else
using Smdn.Threading;
#endif

namespace Smdn.Media.Earthsoft.PV4.Codec.Earthsoft {
  public class Decoder : Codec.Decoder {
    /*
     * class members
     */
    static Decoder()
    {
      const string auiRelativePath = @"EARTH SOFT\PV3 3.x\AviUtl\EARTH SOFT DV.aui";

#if NET_4_0
      if (Environment.Is64BitProcess)
        AuiPath = null;
      else
        AuiPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                               auiRelativePath);
#else
      AuiPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                             auiRelativePath);
#endif
    }

    public static bool IsAvailable {
      get { return !Environment.Is64BitProcess && Runtime.IsRunningOnWindows && File.Exists(AuiPath); }
    }

    private static void Attach(Decoder decoder)
    {
      if (AuiPath == null)
        throw new NotSupportedException("unsupported platform or environment");

      lock (decoder) {
        if (wrapper == null) {
          wrapper = new AviUtlPluginWrapper(AuiPath);

          if (!wrapper.Init())
            throw new ApplicationException("init aui failed");
        }
      }
    }

    private static void Detach(Decoder decoder) {
      lock (decoder) {
        if (wrapper != null) {
          wrapper.Exit();
          wrapper.Dispose();
          wrapper = null;
        }
      }
    }

    public static readonly string AuiPath;
    private static int instanceCount = 0;
    private static AviUtlPluginWrapper wrapper = null;

    /*
     * instance members
     */
    public Decoder(DV dv)
      : this(dv, Decoder.DefaultThreadCount)
    {
    }

    public Decoder(DV dv, int threadCount)
      : base(dv, threadCount)
    {
      if (0 <= Interlocked.Increment(ref instanceCount))
        Attach(this);

      inputHandle = wrapper.Open();

      if (inputHandle == IntPtr.Zero)
        throw new ApplicationException("open aui failed");

      try {
        unsafe {
          fixed (byte* header = dv.Reader.ReadHeaderAsByteArray()) {
            wrapper.Header(inputHandle, (IntPtr)header);
          }
        }
      }
      catch {
        Close();
        throw;
      }

      conversionContexts = new ConversionContext[(0 < threadCount) ? threadCount : 1];
    }

    ~Decoder() {
      if (Interlocked.Decrement(ref instanceCount) <= 0)
        Detach(this);
    }

    protected override void Dispose(bool disposing)
    {
      if (inputHandle != IntPtr.Zero) {
        wrapper.Close(inputHandle);
        inputHandle = IntPtr.Zero;
      }

      base.Dispose(disposing);
    }

    protected override void DecodeFrameAsPackedYUV422(int frame, IntPtr buffer, int bufferStride, out Fraction displayAspectRatio)
    {
      unsafe {
        fixed (byte* frameData = DV.Reader.ReadFrameDataAsByteArray(DV.GetIndex(frame))) {
          displayAspectRatio = new Fraction(BinaryConvert.ByteSwap((ushort)Marshal.ReadInt16((IntPtr)frameData, 256)),
                                            BinaryConvert.ByteSwap((ushort)Marshal.ReadInt16((IntPtr)frameData, 258)));

          wrapper.Decode(inputHandle,
                         (IntPtr)frameData,
                         buffer,
                         bufferStride);
        }
      }
    }

    protected override void DecodeFrameAsXrgb(int frame, IntPtr buffer, int bufferStride, out Fraction displayAspectRatio)
    {
      unsafe {
        int yuvBufferStride;
        var yuvBuffer = EnsureAllocatedPackedYUV422Buffer(out yuvBufferStride);

        DecodeFrameAsPackedYUV422(frame, yuvBuffer, yuvBufferStride, out displayAspectRatio);

        // convert packed YUV422 to XRGB
        var startY = 0;
        var countY = DV.PixelsVertical / conversionContexts.Length;

        for (var i = 0; i < conversionContexts.Length; i++) {
          if (i == 0) {
            startY = countY + DV.PixelsVertical % conversionContexts.Length;
            conversionContexts[i] = new ConversionContext(yuvBuffer,
                                                          yuvBufferStride,
                                                          DV.PixelsHorizontal,
                                                          0,
                                                          startY,
                                                          buffer,
                                                          bufferStride);
          }
          else {
            conversionContexts[i] = new ConversionContext(yuvBuffer,
                                                          yuvBufferStride,
                                                          DV.PixelsHorizontal,
                                                          startY,
                                                          countY,
                                                          buffer,
                                                          bufferStride);
            startY += countY;
          }
        }

        if (1 < conversionContexts.Length)
          Parallel.ForEach(conversionContexts, ConvertPackedYUV422ToArgb);
        else
          ConvertPackedYUV422ToArgb(conversionContexts[0]);
      }
    }

    private unsafe class ConversionContext {
      public int Width;
      public int Height;
      public byte* BufferYuv;
      public int   StrideYuv;
      public byte* BufferXrgb;
      public int   StrideXrgb;

      public ConversionContext(IntPtr bufferYuv, int strideYuv, int width, int startY, int countY, IntPtr bufferXrgb, int strideXrgb)
      {
        Width  = width;
        Height = countY;
        StrideYuv = strideYuv;
        BufferYuv = (byte*)bufferYuv.ToPointer() + startY * strideYuv;
        StrideXrgb = strideXrgb;
        BufferXrgb = (byte*)(bufferXrgb + startY * strideXrgb);
      }
    }

    private static void ConvertPackedYUV422ToArgb(ConversionContext context)
    {
      unsafe {
        CodecUtils.ConvertPackedYUV422ToArgb(context.BufferYuv,
                                             context.StrideYuv,
                                             context.Width,
                                             context.Height,
                                             context.BufferXrgb,
                                             context.StrideXrgb);
      }
    }

    private IntPtr inputHandle = IntPtr.Zero;
    private ConversionContext[] conversionContexts;
  }
}
