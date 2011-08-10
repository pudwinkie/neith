// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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

//#define GETPICTUREINFO

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

using Smdn.Interop;
using Smdn.Imaging.Interop;

namespace Smdn.Imaging.Formats.SusiePlugins {
  [CLSCompliant(false)]
  public class SusieImportFilterPlugin : SusiePlugin, IImageDecoder {
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] public unsafe delegate bool SpiIsSupported(/*LPSTR*/ string filename, /*DWORD*/ void* dw);
#if GETPICTUREINFO
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] public delegate SusieErrorCode SpiGetPictureInfoString(/*LPSTR*/ string buf, int len, uint flag, ref PictureInfo lpInfo);
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] public delegate SusieErrorCode SpiGetPictureInfoPtr(/*LPSTR*/ IntPtr buf, int len, uint flag, ref PictureInfo lpInfo);
#endif
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)] public delegate SusieErrorCode SpiGetPictureString(/*LPSTR*/ string buf, int len, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, SpiProgressCallback lpProgressCallback, IntPtr lData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] public unsafe delegate SusieErrorCode SpiGetPicturePtr(/*LPSTR*/ void* buf, int len, uint flag, out IntPtr pHBInfo, out IntPtr pHBm, SpiProgressCallback lpProgressCallback, IntPtr lData);

    internal SusieImportFilterPlugin(DynamicLinkLibrary library, SpiGetPluginInfo getPluginInfo, SusiePluginApiVersion apiVersion)
      : base(library, getPluginInfo, apiVersion)
    {
      isSupported = Library.GetFunction<SpiIsSupported>("IsSupported");

#if GETPICTUREINFO
      getPictureInfoString = Library.GetFunction<SpiGetPictureInfoString>("GetPictureInfo");
      getPictureInfoPtr = Library.GetFunction<SpiGetPictureInfoPtr>("GetPictureInfo");
#endif

      getPictureString = Library.GetFunction<SpiGetPictureString>("GetPicture");
      getPicturePtr = Library.GetFunction<SpiGetPicturePtr>("GetPicture");
    }

#region "IImageDecoder implementation"
    bool IImageDecoder.GetImageFormat(Stream stream, out int? imageCount, out int? width, out int? height)
    {
      imageCount = null;
      width = null;
      height = null;

      // TODO: use GetPictureinfo
      imageCount = 1;

      // TODO: exception
      return IsSupported(stream);
    }

    Bitmap IImageDecoder.Decode(Stream stream, bool useIcm)
    {
      // TODO: exception
      return GetPicture(stream);
    }
#endregion

#region "IsSupported"
    private const int IsSupportedProcRequedLength = 2 * 1024; // SPI-SPEC: IsSupported requires at least 2kB

    public bool IsSupported(string filename)
    {
      CheckDisposed();

      using (var stream = File.OpenRead(filename)) {
        // TODO: use HandleRef instead of IntPtr (http://msdn.microsoft.com/ja-jp/library/hc662t8k(VS.80).aspx)
        unsafe {
          return isSupported(Path.GetFullPath(filename), (void*)stream.SafeFileHandle.DangerousGetHandle());
        }
      }
    }

    public bool IsSupported(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      return IsSupported(stream, (stream is FileStream) ? (stream as FileStream).Name : null);
    }

    public bool IsSupported(Stream stream, string referenceFilename)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      var data = new byte[IsSupportedProcRequedLength];

      stream.Read(data, 0, data.Length);

      return IsSupported(data, referenceFilename);
    }

    public bool IsSupported(byte[] data)
    {
      return IsSupported(data, null);
    }

    public bool IsSupported(byte[] data, string referenceFilename)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      if (data.Length < IsSupportedProcRequedLength) {
        var buffer = new byte[IsSupportedProcRequedLength];

        Array.Copy(data, buffer, data.Length);

        data = buffer;
      }

      unsafe {
        fixed (byte* ptr = data) {
          return IsSupported((void*)ptr, referenceFilename);
        }
      }
    }

    public bool IsSupported(IntPtr data)
    {
      return IsSupported(data, null);
    }

    public bool IsSupported(IntPtr data, string referenceFilename)
    {
      if (data == IntPtr.Zero)
        throw new ArgumentException("data == NULL");

      unsafe {
        return IsSupported((void*)data, referenceFilename);
      }
    }

    public unsafe bool IsSupported(void* data)
    {
      return IsSupported(data, null);
    }

    public unsafe bool IsSupported(void* data, string referenceFilename)
    {
      CheckDisposed();

      return isSupported(Path.GetFullPath(referenceFilename) ?? string.Empty, // XXX
                         data);
    }
#endregion

#if GETPICTUREINFO
    public PictureInfo GetPictureInfo(string filename)
    {
      CheckDisposed();

      var offset = 0; // TODO: MacBin
      var info = new PictureInfo(); // TODO: disposable

      SusiePluginException.ThrowIfError(getPictureInfoString(filename, offset, 0 /* SPI-SPEC: read from file */, ref info));

      return info;
    }

    public PictureInfo GetPictureInfo(IntPtr data, int length)
    {
      CheckDisposed();

      var info = new PictureInfo(); // TODO: disposable

      SusiePluginException.ThrowIfError(getPictureInfoPtr(data, length, 1 /* SPI-SPEC: read from memory */, ref info));

      return info;
    }

    public PictureInfo GetPictureInfo(byte[] data)
    {
      unsafe {
        fixed (byte* ptr = data) {
          return GetPictureInfo(new IntPtr(ptr), data.Length);
        }
      }
    }
#endif

#region "GetPicture"
    public Bitmap GetPicture(string filename)
    {
      if (filename == null)
        throw new ArgumentNullException("filename");

      IntPtr hBInfo, hBm;
      var offset = 0; // TODO: MacBin

      SusiePluginException.ThrowIfError(getPictureString(filename, offset, 0 /* SPI-SPEC: read from file */, out hBInfo, out hBm, null, IntPtr.Zero));

      using (var ptrBitmapInfoHeader = LocalMemoryBuffer.FromHLOCAL(hBInfo, true))
      using (var ptrBitmap = LocalMemoryBuffer.FromHLOCAL(hBm, true)) {
        return CreateBitmap(ptrBitmapInfoHeader.PtrLocked, ptrBitmap.PtrLocked);
      }
    }

    public Bitmap GetPicture(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      return GetPicture(Smdn.IO.StreamExtensions.ReadToEnd(stream));
    }

    public Bitmap GetPicture(byte[] data)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      unsafe {
        fixed (byte* ptr = data) {
          return GetPicture((void*)ptr, data.Length);
        }
      }
    }

    public Bitmap GetPicture(IntPtr data, int length)
    {
      if (data == IntPtr.Zero)
        throw new ArgumentException("data == NULL");

      unsafe {
        return GetPicture((void*)data, length);
      }
    }

    public unsafe Bitmap GetPicture(void* data, int length)
    {
      CheckDisposed();

      if (length <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("length", length);

      IntPtr hBInfo, hBm;

      SusiePluginException.ThrowIfError(getPicturePtr(data, length, 1 /* SPI-SPEC: read from memory */, out hBInfo, out hBm, null, IntPtr.Zero));

      using (var ptrBitmapInfoHeader = LocalMemoryBuffer.FromHLOCAL(hBInfo, true))
      using (var ptrBitmap = LocalMemoryBuffer.FromHLOCAL(hBm, true)) {
        return CreateBitmap(ptrBitmapInfoHeader.PtrLocked, ptrBitmap.PtrLocked);
      }
    }
#endregion

#region "utility methods"
    private static Bitmap CreateBitmap(IntPtr ptrBitmapInfoHeader, IntPtr ptrBitmap)
    {
      BITMAPINFOHEADER bmih;
      PixelFormat format;
      Color[] palette;

      // TODO: biCompression
      ReadBitmapInfoHeaderFromPointer(ptrBitmapInfoHeader, out bmih, out format, out palette);

      var height = bmih.biHeight;
      var scanlineBottomUp = true;

      if (height < 0) {
        scanlineBottomUp = false;
        height = -height;
      }

      var bitmap = new Bitmap(bmih.biWidth, height, format);
      var stride = (bmih.biWidth * (bmih.biBitCount / 8) + 3) & ~3;

      using (var locked = new LockedBitmap(bitmap, ImageLockMode.WriteOnly, format)) {
        unsafe {
          locked.ForEachScanLine(delegate(void* scanline, int y, int width) {
            byte* src = (byte*)ptrBitmap + stride * (scanlineBottomUp ? height - y - 1: y);

            UnmanagedMemoryBuffer.UncheckedMemCpy(scanline, src, stride);
          });
        }
      }

      if (palette != null && 0 < palette.Length) {
        var p = bitmap.Palette;

        for (var i = 0; i < Math.Min(p.Entries.Length, palette.Length); i++) {
          p.Entries[i] = palette[i];
        }

        bitmap.Palette = p;
      }

      // TODO: PelsPerMeter -> DPI
      //bitmap.SetResolution(bmih.biXPelsPerMeter, bmih.biYPelsPerMeter);

      return bitmap;
    }

    private static void ReadBitmapInfoHeaderFromPointer(IntPtr ptrBitmapInfoHeader, out BITMAPINFOHEADER bmih, out PixelFormat format, out Color[] palette)
    {
      bmih = (BITMAPINFOHEADER)Marshal.PtrToStructure(ptrBitmapInfoHeader, typeof(BITMAPINFOHEADER));

      switch (bmih.biBitCount) {
        case 1:  format = PixelFormat.Format1bppIndexed; break;
        // case 2: ?
        case 4:  format = PixelFormat.Format4bppIndexed; break;
        case 8:  format = PixelFormat.Format8bppIndexed; break;
        case 16: format = PixelFormat.Format16bppRgb555; break; // XXX: 565?
        case 24: format = PixelFormat.Format24bppRgb; break;
        case 32: format = PixelFormat.Format32bppArgb; break;
        // case 64: ?
        default: throw new NotSupportedException(string.Format("unsupported pixel format: biBitCount = {0}", bmih.biBitCount));
      }

      if (bmih.biBitCount <= 8) {
        var paletteCount = (bmih.biClrUsed == 0) ? 1 << bmih.biBitCount : (int)bmih.biClrUsed;
        var rgbquad = new int[paletteCount];

#if NET_4_0
        Marshal.Copy(ptrBitmapInfoHeader + BITMAPINFOHEADER.Size, rgbquad, 0, paletteCount);
#else
        Marshal.Copy(ptrBitmapInfoHeader.Add(BITMAPINFOHEADER.Size), rgbquad, 0, paletteCount);
#endif

        palette = Array.ConvertAll(rgbquad, delegate(int rgb) {
          return (Color)(new RGBQUAD((uint)rgb));
        });
      }
      else {
        palette = null;
      }
    }
#endregion

    private SpiIsSupported isSupported;
#if GETPICTUREINFO
    private SpiGetPictureInfoString getPictureInfoString;
    private SpiGetPictureInfoPtr getPictureInfoPtr;
#endif
    private SpiGetPictureString getPictureString;
    private SpiGetPicturePtr getPicturePtr;
  }
}
