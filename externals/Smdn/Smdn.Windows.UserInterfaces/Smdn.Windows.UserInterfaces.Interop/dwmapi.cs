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

namespace Smdn.Windows.UserInterfaces.Interop {
  [CLSCompliant(false)]
  public static class dwmapi {
    private const string dllname = "dwmapi";

    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmEnableBlurBehindWindow(IntPtr hWnd, [In] ref DWM_BLURBEHIND pBlurBehind);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmEnableComposition(DWM_EC uCompositionAction);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmExtendFrameIntoClientArea(IntPtr hWnd, [In] ref MARGINS pMarInset);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmFlush();
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmGetColorizationColor(out uint pcrColorization, out bool pfOpaqueBlend);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmIsCompositionEnabled(out bool pfEnabled);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmQueryThumbnailSourceSize(IntPtr hThumbnail, out SIZE size);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmRegisterThumbnail(IntPtr hwndDestination, IntPtr hwndSource, out IntPtr phThumbnailId);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmUnregisterThumbnail(IntPtr hThumbnailId);
    [DllImport(dllname, SetLastError = true)] public static extern int /*HRESULT*/  DwmUpdateThumbnailProperties(IntPtr hThumbnailId, [In] ref DWM_THUMBNAIL_PROPERTIES props);
  }
}
