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

namespace Smdn.Windows.Forms.Interop {
  public static partial class Consts {
    [CLSCompliant(false)] public const uint LWA_COLORKEY  = 0x01;
    [CLSCompliant(false)] public const uint LWA_ALPHA     = 0x02;
    [CLSCompliant(false)] public const uint ULW_COLORKEY  = 0x01;
    [CLSCompliant(false)] public const uint ULW_ALPHA     = 0x02;
    [CLSCompliant(false)] public const uint ULW_OPAQUE    = 0x04;
  }

  [CLSCompliant(false)]
  public enum WS : uint {
    BORDER            = 0x800000,
    CAPTION           = 0xc00000,
    CHILD             = 0x40000000,
    CHILDWINDOW       = 0x40000000,
    CLIPCHILDREN      = 0x2000000,
    CLIPSIBLINGS      = 0x4000000,
    DISABLED          = 0x8000000,
    DLGFRAME          = 0x400000,
    GROUP             = 0x20000,
    HSCROLL           = 0x100000,
    ICONIC            = 0x20000000,
    MAXIMIZE          = 0x1000000,
    MAXIMIZEBOX       = 0x10000,
    MINIMIZE          = 0x20000000,
    MINIMIZEBOX       = 0x20000,
    OVERLAPPED        = 0,
    OVERLAPPEDWINDOW  = 0xcf0000,
    POPUP             = 0x80000000,
    POPUPWINDOW       = 0x80880000,
    SIZEBOX           = 0x40000,
    SYSMENU           = 0x80000,
    TABSTOP           = 0x10000,
    THICKFRAME        = 0x40000,
    TILED             = 0,
    TILEDWINDOW       = 0xcf0000,
    VISIBLE           = 0x10000000,
    VSCROLL           = 0x200000,
    ACTIVECAPTION     = 0x00000001,
  }

  [CLSCompliant(false)]
  public enum WS_EX : uint {
    LEFT                = 0,
    LTRREADING          = 0,
    RIGHTSCROLLBAR      = 0,
    DLGMODALFRAME       = 1,
    NOPARENTNOTIFY      = 4,
    TOPMOST             = 8,
    ACCEPTFILES         = 16,
    TRANSPARENT         = 32,
    MDICHILD            = 64,
    TOOLWINDOW          = 128,
    WINDOWEDGE          = 256,
    CLIENTEDGE          = 512,

    PALETTEWINDOW       = 0x188,
    OVERLAPPEDWINDOW    = 0x300,
    CONTEXTHELP         = 0x400,
    RIGHT               = 0x1000,
    RTLREADING          = 0x2000,
    LEFTSCROLLBAR       = 0x4000,
    CONTROLPARENT       = 0x10000,
    STATICEDGE          = 0x20000,
    APPWINDOW           = 0x40000,
    LAYERED             = 0x80000,
    NOINHERITLAYOUT     = 0x100000,
    LAYOUTRTL           = 0x400000,
    COMPOSITED          = 0x2000000,
    NOACTIVATE          = 0x8000000,
  }

  public enum GWL : int {
    EXSTYLE     = -20,
    STYLE       = -16,
    WNDPROC     = -4,
    HINSTANCE   = -6,
    HWNDPARENT  = -8,
    ID          = -12,
    USERDATA    = -21,
  }
}
