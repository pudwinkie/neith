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

using System;
using System.Runtime.InteropServices;

namespace Smdn.Imaging.Interop {
  [CLSCompliant(false)]
  public static class gdi32 {
    private const string dllname = "gdi32.dll";

    [DllImport(dllname, SetLastError = true)] public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport(dllname, SetLastError = true)] public static extern bool DeleteDC(IntPtr hdc);
    [DllImport(dllname, SetLastError = true)] public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport(dllname, SetLastError = true)] public static extern bool DeleteObject(IntPtr hObject);
    [DllImport(dllname, SetLastError = true)] public static extern int GetObject(IntPtr handle, int cbBuffer, IntPtr lpvObject);
    [DllImport(dllname, SetLastError = true)] public static extern int GetObject(IntPtr handle, int cbBuffer, ref BITMAP lpvObject);
    [DllImport(dllname, SetLastError = true)] public static extern int GetObject(IntPtr handle, int cbBuffer, ref DIBSECTION lpvObject);
    [DllImport(dllname, SetLastError = true)] public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, IntPtr lpvBits, IntPtr lpbmi, uint uUsage);
    [DllImport(dllname, SetLastError = true)] public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, IntPtr lpvBits, ref BITMAPINFO lpbmi, uint uUsage);
    [DllImport(dllname, SetLastError = true)] public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, IntPtr lpvBits, ref BITMAPINFO_32BPP lpbmi, uint uUsage);
    [DllImport(dllname, SetLastError = true)] public static extern unsafe int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, void* lpvBits, ref BITMAPINFO lpbmi, uint uUsage);
    [DllImport(dllname, SetLastError = true)] public static extern unsafe int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, void* lpvBits, ref BITMAPINFO_32BPP lpbmi, uint uUsage);
  }
}
