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

using Smdn.Imaging.Interop;

namespace Smdn.Windows.Forms.Interop {
  [CLSCompliant(false)]
  public static class user32 {
    private const string dllname = "user32.dll";

    [DllImport(dllname, SetLastError = true)] public static extern bool ShowWindow(IntPtr hWnd, Smdn.Windows.UserInterfaces.Interop.SW nCmdShow);

    [DllImport(dllname, SetLastError = true)] public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);
    [DllImport(dllname, SetLastError = true)] public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

    [DllImport(dllname, SetLastError = true)] public static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hDCDest, ref POINT pptDest, ref SIZE pSize, IntPtr hDCSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pBlend, uint dwFlags);
    [DllImport(dllname, SetLastError = true)] public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
  }
}
