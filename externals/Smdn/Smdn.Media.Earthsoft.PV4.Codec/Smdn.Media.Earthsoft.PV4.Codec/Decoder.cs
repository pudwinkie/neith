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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

using Smdn.Formats.Earthsoft.PV4;
using Smdn.Imaging;
using Smdn.Interop;
using Smdn.Mathematics;
using Smdn.Mathematics.SignalProcessing.Transforms;

namespace Smdn.Media.Earthsoft.PV4.Codec {
  public abstract class Decoder : CodecBase {
    public static Decoder Create(DV dv)
    {
      return Create(dv, CodecBase.DefaultThreadCount, null);
    }

    public static Decoder Create(DV dv, int threadCount)
    {
      return Create(dv, threadCount, null);
    }

    public static Decoder Create(DV dv, CreateDctHandler createDct)
    {
      return Create(dv, CodecBase.DefaultThreadCount, createDct);
    }

    public static Decoder Create(DV dv, int threadCount, CreateDctHandler createDct)
    {
      if (Earthsoft.Decoder.IsAvailable)
        return new Earthsoft.Decoder(dv, threadCount);
      else
        return new Simple.Decoder(dv, threadCount, createDct ?? (CreateDctHandler)Smpte370MDct.CreateBest);
    }

    protected Decoder(DV dv, int threadCount)
      : base(dv, threadCount)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (packedYUV422Buffer != null) {
          packedYUV422Buffer.Free();
          packedYUV422Buffer = null;
        }
      }

      base.Dispose(disposing);
    }

    protected abstract void DecodeFrameAsPackedYUV422(int frame, IntPtr buffer, int bufferStride, out Fraction displayAspectRatio);
    protected abstract void DecodeFrameAsXrgb        (int frame, IntPtr buffer, int bufferStride, out Fraction displayAspectRatio);

    public Bitmap DecodeFrame(int frame)
    {
      Fraction discard;

      return DecodeFrame(frame, out discard);
    }

    public Bitmap DecodeFrame(int frame, out Fraction displayAspectRatio)
    {
      Bitmap ret = null;

      DecodeFrame(frame, ref ret, out displayAspectRatio);

      return ret;
    }

    public void DecodeFrame(int frame, ref Bitmap decoded)
    {
      Fraction discard;

      DecodeFrame(frame, ref decoded, out discard);
    }

    public void DecodeFrame(int frame, ref Bitmap decoded, out Fraction displayAspectRatio)
    {
      CheckDisposed();

      EnsureBitmapCreated(ref decoded);

      try {
        using (var locked = new LockedBitmap(decoded, ImageLockMode.WriteOnly, decoded.PixelFormat)) {
          DecodeFrameCore(frame, locked.LockedData, out displayAspectRatio);
        }
      }
      catch {
        if (decoded != null) {
          decoded.Dispose();
          decoded = null;
        }

        throw;
      }
    }

    public void DecodeFrame(int frame, BitmapData dest)
    {
      Fraction discard;

      DecodeFrame(frame, dest, out discard);
    }

    public void DecodeFrame(int frame, BitmapData dest, out Fraction displayAspectRatio)
    {
      CheckDisposed();

      CheckBitmapDataArg(dest, "dest");

      DecodeFrameCore(frame, dest, out displayAspectRatio);
    }

    private void DecodeFrameCore(int frame, BitmapData dest, out Fraction displayAspectRatio)
    {
      CheckFrameNumberArg(frame);

      if (!IsSupportedPixelFormat(dest.PixelFormat))
        throw ExceptionUtils.CreateNotSupportedEnumValue(dest.PixelFormat);

      DecodeFrameAsXrgb(frame, dest.Scan0, dest.Stride, out displayAspectRatio);
    }

    public void DecodeFrameDeinterlaced(int frame, ref Bitmap firstField, ref Bitmap secondField)
    {
      Fraction discard;

      DecodeFrameDeinterlaced(frame, ref firstField, ref secondField, out discard);
    }

    public void DecodeFrameDeinterlaced(int frame, ref Bitmap firstField, ref Bitmap secondField, out Fraction displayAspectRatio)
    {
      CheckDisposed();

      EnsureBitmapCreated(ref firstField);
      EnsureBitmapCreated(ref secondField);

      try {
        using (LockedBitmap
               first = new LockedBitmap(firstField, ImageLockMode.WriteOnly, firstField.PixelFormat),
               second = new LockedBitmap(secondField, ImageLockMode.WriteOnly, secondField.PixelFormat)) {
          DecodeFrameDeinterlacedCore(frame,
                                      first.LockedData,
                                      second.LockedData,
                                      out displayAspectRatio);
        }
      }
      catch {
        if (firstField != null) {
          firstField.Dispose();
          firstField = null;
        }

        if (secondField != null) {
          secondField.Dispose();
          secondField = null;
        }

        throw;
      }
    }

    public void DecodeFrameDeinterlaced(int frame, BitmapData firstField, BitmapData secondField)
    {
      Fraction discard;

      DecodeFrameDeinterlaced(frame, firstField, secondField, out discard);
    }

    public void DecodeFrameDeinterlaced(int frame, BitmapData firstField, BitmapData secondField, out Fraction displayAspectRatio)
    {
      CheckDisposed();

      CheckBitmapDataArg(firstField, "firstField");
      CheckBitmapDataArg(secondField, "secondField");

      DecodeFrameDeinterlacedCore(frame, firstField, secondField, out displayAspectRatio);
    }

    private void DecodeFrameDeinterlacedCore(int frame, BitmapData fieldFirst, BitmapData fieldSecond, out Fraction displayAspectRatio)
    {
      CheckFrameNumberArg(frame);

      if (DV.FrameScanning != FrameScanning.Interlaced)
        throw new NotSupportedException("video stream is not interlaced");
      if (!IsSupportedPixelFormat(fieldFirst.PixelFormat))
        throw ExceptionUtils.CreateNotSupportedEnumValue(fieldFirst.PixelFormat);
      if (!IsSupportedPixelFormat(fieldSecond.PixelFormat))
        throw ExceptionUtils.CreateNotSupportedEnumValue(fieldSecond.PixelFormat);

      int yuvBufferStride;
      var yuvBuffer = EnsureAllocatedPackedYUV422Buffer(out yuvBufferStride);

      DecodeFrameAsPackedYUV422(frame, yuvBuffer, yuvBufferStride, out displayAspectRatio);

      unsafe {
        CodecUtils.ConvertPackedYUV422ToDeinterlacedXrgb((byte*)yuvBuffer.ToPointer(),
                                                         yuvBufferStride,
                                                         DV.PixelsHorizontal,
                                                         DV.PixelsVertical,
                                                         (byte*)fieldFirst.Scan0.ToPointer(),
                                                         fieldFirst.Stride,
                                                         true);

        CodecUtils.ConvertPackedYUV422ToDeinterlacedXrgb((byte*)yuvBuffer.ToPointer(),
                                                         yuvBufferStride,
                                                         DV.PixelsHorizontal,
                                                         DV.PixelsVertical,
                                                         (byte*)fieldSecond.Scan0.ToPointer(),
                                                         fieldSecond.Stride,
                                                         false);
      }
    }

    public void DecodeFramePackedYUV422(int frame, IntPtr buffer, int stride)
    {
      Fraction discard;

      DecodeFramePackedYUV422(frame, buffer, stride, out discard);
    }

    public void DecodeFramePackedYUV422(int frame, IntPtr buffer, int stride, out Fraction displayAspectRatio)
    {
      CheckDisposed();

      CheckFrameNumberArg(frame);

      if (buffer == IntPtr.Zero)
        throw new ArgumentException("buffer == IntPtr.Zero", "buffer");
      if (stride <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("stride", stride);

      DecodeFrameAsPackedYUV422(frame, buffer, stride, out displayAspectRatio);
    }

    private static bool IsSupportedPixelFormat(PixelFormat format)
    {
      switch (format) {
        case PixelFormat.Format32bppArgb:
        case PixelFormat.Format32bppPArgb:
        case PixelFormat.Format32bppRgb:
          return true;

        default:
          return false;
      }
    }

    private void EnsureBitmapCreated(ref Bitmap bitmap)
    {
      if (bitmap != null &&
           (bitmap.Width != DV.PixelsHorizontal ||
            bitmap.Height != DV.PixelsVertical ||
            !IsSupportedPixelFormat(bitmap.PixelFormat))
         ) {
        bitmap.Dispose();
        bitmap = null;
      }

      if (bitmap == null)
        // (re)create instance
        bitmap = new Bitmap(DV.PixelsHorizontal, DV.PixelsVertical, PixelFormat.Format32bppRgb);
    }

    protected IntPtr EnsureAllocatedPackedYUV422Buffer(out int stride)
    {
      stride = DV.PixelsHorizontal << 1;

      if (packedYUV422Buffer == null) {
        if (Runtime.IsRunningOnWindows)
          packedYUV422Buffer = new HeapMemoryBuffer(stride * DV.PixelsVertical);
        else
          packedYUV422Buffer = new CoTaskMemoryBuffer(stride * DV.PixelsVertical);
      }

      return packedYUV422Buffer.Ptr;
    }

    private void CheckBitmapDataArg(BitmapData data, string paramName)
    {
      if (data == null)
        throw new ArgumentNullException(paramName);
      if (data.Width < DV.PixelsHorizontal)
        throw new ArgumentException("invalid image (Width is less than frame width)", paramName);
      if (data.Height < DV.PixelsVertical)
        throw new ArgumentException("invalid image (Height is less than frame height)", paramName);
      if (data.Stride <= 0)
        throw new ArgumentException("invalid image (Stride must be non-zero positive number", paramName);
      if (data.Scan0 == IntPtr.Zero)
        throw new ArgumentException("invalid image (Scan0 is zero)", paramName);
    }

    private void CheckFrameNumberArg(int frame)
    {
      if (frame < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("frame", frame);
      if (DV.FrameCount <= frame)
        throw ExceptionUtils.CreateArgumentMustBeLessThan("'DV.FrameCount'", "frame", frame);
    }

    private UnmanagedMemoryBuffer packedYUV422Buffer;
  }
}
