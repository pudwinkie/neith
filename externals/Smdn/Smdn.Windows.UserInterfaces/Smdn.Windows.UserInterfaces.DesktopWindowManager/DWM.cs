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
using System.Drawing;
using System.Runtime.InteropServices;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.DesktopWindowManager {
  /*
   * http://msdn.microsoft.com/en-us/library/aa969540(VS.85).aspx
   * http://msdn.microsoft.com/en-us/magazine/cc163435.aspx
   * http://mag.autumn.org/Content.modf?id=20070604172101
   */
  public static class DWM {
    public static bool IsPlatformSupported {
      get
      {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && 6 <= Environment.OSVersion.Version.Major)
          // Vista or over
          return true;
        else
          return false;
      }
    }

    public static bool IsCompositionEnabled {
      get
      {
        if (!IsPlatformSupported)
          return false;

        bool fEnabled;

        Marshal.ThrowExceptionForHR(dwmapi.DwmIsCompositionEnabled(out fEnabled));

        return fEnabled;
      }
    }

    public static void EnableComposition()
    {
      Marshal.ThrowExceptionForHR(dwmapi.DwmEnableComposition(DWM_EC.ENABLECOMPOSITION));
    }

    public static void DisableComposition()
    {
      Marshal.ThrowExceptionForHR(dwmapi.DwmEnableComposition(DWM_EC.DISABLECOMPOSITION));
    }

    public static void ApplyBlurBehind(IntPtr hWnd)
    {
      var blurBehind = new DWM_BLURBEHIND();

      blurBehind.dwFlags = DWM_BB.ENABLE;
      blurBehind.fEnable = true;
      blurBehind.hRgnBlur = IntPtr.Zero; // entire client area

      Marshal.ThrowExceptionForHR(dwmapi.DwmEnableBlurBehindWindow(hWnd, ref blurBehind));
    }

    public static void ExtendFrameIntoClientArea(IntPtr hWnd)
    {
      ExtendFrameIntoClientArea(hWnd, MARGINS.ClientAll);
    }

    public static void ExtendFrameIntoClientArea(IntPtr hWnd, Size margin)
    {
      ExtendFrameIntoClientArea(hWnd, new MARGINS(margin.Width, margin.Width, margin.Height, margin.Height));
    }

    public static void ExtendFrameIntoClientArea(IntPtr hWnd, int marginLeft, int marginRight, int marginTop, int marginBottom)
    {
      ExtendFrameIntoClientArea(hWnd, new MARGINS(marginLeft, marginRight, marginTop, marginBottom));
    }

    public static void ExtendFrameIntoClientArea(IntPtr hWnd, MARGINS margin)
    {
      CheckPlatformSupported();

      Marshal.ThrowExceptionForHR(dwmapi.DwmExtendFrameIntoClientArea(hWnd, ref margin));
    }

    private static void CheckPlatformSupported()
    {
      if (!IsPlatformSupported)
        throw new PlatformNotSupportedException("Desktop Window Manager is not supported on this platform.");
    }

    /*
     * undocumented function:
     *   HRESULT DwmXXXXX(VOID);
     */
    private delegate int/*HRESULT*/ DwmInvokeFlip3D();

    public static void InvokeFlip3D()
    {
      // http://barca.daa.jp/archives/2007/02/flip3d-tips-of-vista.php
      using (var module = new Smdn.Interop.DynamicLinkLibrary("dwmapi.dll")) {
        var func = module.GetFunction<DwmInvokeFlip3D>(105);

        if (func != null)
          Marshal.ThrowExceptionForHR(func());
      }
    }
  }
}
