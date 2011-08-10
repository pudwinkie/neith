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
  [CLSCompliant(false), Flags]
  public enum DWM_BB : uint {
    ENABLE                = 0x00000001,
    BLURREGION            = 0x00000002,
    TRANSITIONONMAXIMIZED = 0x00000004,
  }

  [CLSCompliant(false), Flags]
  public enum DWM_EC : uint {
    DISABLECOMPOSITION  = 0,
    ENABLECOMPOSITION   = 1,
  }

  public enum DWMWINDOWATTRIBUTE {
    DWMWA_NCRENDERING_ENABLED = 1,
    DWMWA_NCRENDERING_POLICY,
    DWMWA_TRANSITIONS_FORCEDISABLED,
    DWMWA_ALLOW_NCPAINT,
    DWMWA_CAPTION_BUTTON_BOUNDS,
    DWMWA_NONCLIENT_RTL_LAYOUT,
    DWMWA_FORCE_ICONIC_REPRESENTATION,
    DWMWA_FLIP3D_POLICY,
    DWMWA_EXTENDED_FRAME_BOUNDS,
    DWMWA_HAS_ICONIC_BITMAP,
    DWMWA_DISALLOW_PEEK,
    DWMWA_EXCLUDED_FROM_PEEK,
    DWMWA_LAST,
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct DWM_BLURBEHIND {
    public DWM_BB dwFlags;
    public bool fEnable;
    public IntPtr hRgnBlur;
    public bool fTransitionOnMaximized;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct DWM_THUMBNAIL_PROPERTIES {
    public uint dwFlags;
    public RECT rcDestination;
    public RECT rcSource;
    public byte opacity;
    public bool fVisible;
    public bool fSourceClientAreaOnly;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct DWM_PRESENT_PARAMETERS {
    public int cbSize;
    public bool fQueue;
    public ulong cRefreshStart;
    public uint cBuffer;
    public bool fUseSourceRate;
  }
}