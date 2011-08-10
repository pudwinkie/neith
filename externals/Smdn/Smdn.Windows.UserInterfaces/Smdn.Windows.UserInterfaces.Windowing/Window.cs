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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.Windowing {
  public static class Window {
#region "WM_COMMAND"
    public static IntPtr SendCommand(string className, IntPtr wParam, IntPtr lParam)
    {
      return SendCommand(className, null, wParam, lParam);
    }

    public static IntPtr SendCommand(string className, string windowName, IntPtr wParam, IntPtr lParam)
    {
      if (className == null)
        throw new ArgumentNullException("className");

      var hWnd = user32.FindWindow(className, windowName);

      if (hWnd == IntPtr.Zero)
        throw new Win32Exception(Marshal.GetLastWin32Error());

      return SendCommand(hWnd, wParam, lParam);
    }

    public static IntPtr SendCommand(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
    {
      if (hWnd == IntPtr.Zero)
        throw new ArgumentException("hWnd == null", "hWnd");

      return user32.SendMessage(hWnd, WM.COMMAND, wParam, lParam);
    }
#endregion

#region "WM_GETICON, WM_SETICON"
    public static void SetAltTabDialogIcon(IntPtr hWnd, Icon icon)
    {
      Bitmap discard;

      SetAltTabDialogIcon(hWnd, icon, out discard);
    }

    public static void SetAltTabDialogIcon(IntPtr hWnd, Icon icon, out Bitmap prevIcon)
    {
      IntPtr hPrevIcon;

      SetAltTabDialogIcon(hWnd, (icon == null) ? IntPtr.Zero : icon.Handle, out hPrevIcon);

      prevIcon = (hPrevIcon == IntPtr.Zero) ? null : Smdn.Imaging.Formats.Ico.Icon.FromHIcon(hPrevIcon);
    }

    public static void SetAltTabDialogIcon(IntPtr hWnd, IntPtr hIcon)
    {
      IntPtr discard;

      SetAltTabDialogIcon(hWnd, hIcon, out discard);
    }

    public static void SetAltTabDialogIcon(IntPtr hWnd, IntPtr hIcon, out IntPtr hPrevIcon)
    {
      hPrevIcon = SetIcon(hWnd, Consts.ICON_BIG, hIcon);
    }

    public static void SetWindowCaptionIcon(IntPtr hWnd, Icon icon)
    {
      Bitmap discard;

      SetWindowCaptionIcon(hWnd, icon, out discard);
    }

    public static void SetWindowCaptionIcon(IntPtr hWnd, Icon icon, out Bitmap prevIcon)
    {
      IntPtr hPrevIcon;

      SetWindowCaptionIcon(hWnd, (icon == null) ? IntPtr.Zero : icon.Handle, out hPrevIcon);

      prevIcon = (hPrevIcon == IntPtr.Zero) ? null : Smdn.Imaging.Formats.Ico.Icon.FromHIcon(hPrevIcon);
    }

    public static void SetWindowCaptionIcon(IntPtr hWnd, IntPtr hIcon)
    {
      IntPtr discard;

      SetWindowCaptionIcon(hWnd, hIcon, out discard);
    }

    public static void SetWindowCaptionIcon(IntPtr hWnd, IntPtr hIcon, out IntPtr hPrevIcon)
    {
      hPrevIcon = SetIcon(hWnd, Consts.ICON_SMALL, hIcon);
    }

    /*
     * http://msdn.microsoft.com/en-us/library/ms632643(VS.85).aspx
     */
    private static IntPtr SetIcon(IntPtr hWnd, int icon, IntPtr hIcon)
    {
      if (hWnd == IntPtr.Zero)
        throw new ArgumentException("hWnd == null", "hWnd");

      return user32.SendMessage(hWnd, WM.SETICON, (IntPtr)icon, hIcon);
    }

    public static Bitmap GetAltTabDialogIcon(IntPtr hWnd)
    {
      var hIcon = GetAltTabDialogIconHandle(hWnd);

      return (hIcon == IntPtr.Zero) ? null : Smdn.Imaging.Formats.Ico.Icon.FromHIcon(hIcon);
    }

    public static IntPtr GetAltTabDialogIconHandle(IntPtr hWnd)
    {
      return GetIcon(hWnd, Consts.ICON_BIG);
    }

    public static Bitmap GetWindowCaptionIcon(IntPtr hWnd)
    {
      var hIcon = GetWindowCaptionIconHandle(hWnd);

      return (hIcon == IntPtr.Zero) ? null : Smdn.Imaging.Formats.Ico.Icon.FromHIcon(hIcon);
    }

    public static IntPtr GetWindowCaptionIconHandle(IntPtr hWnd)
    {
      return GetIcon(hWnd, Consts.ICON_SMALL);
    }

    /*
     * http://msdn.microsoft.com/en-us/library/ms632625(VS.85).aspx
     */
    private static IntPtr GetIcon(IntPtr hWnd, int icon)
    {
      if (hWnd == IntPtr.Zero)
        throw new ArgumentException("hWnd == null", "hWnd");

      return user32.SendMessage(hWnd, WM.GETICON, (IntPtr)icon, IntPtr.Zero);
    }
#endregion
  }
}
