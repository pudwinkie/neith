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
using System.Text;

namespace Smdn.Windows.UserInterfaces.Interop {
  public static partial class Consts {
    public const int ICON_SMALL   = 0;
    public const int ICON_BIG     = 1;
    public const int ICON_SMALL2  = 2;
  }

  [CLSCompliant(false)]
  public static class user32 {
    private const string dllname = "user32.dll";

    [DllImport(dllname, SetLastError = true)] public static extern bool SystemParametersInfo(SPI uAction, int uParam, string lpvParam, SPIF fuWinIini);

    /*
     * Resources
     */
    [DllImport(dllname, SetLastError = true, CharSet = CharSet.Auto)] public static extern int LoadString(IntPtr hInstance, uint uID, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, int bufferMax);

    /*
     * Windowing
     */
    [DllImport(dllname, SetLastError = true, CharSet = CharSet.Auto)] public static extern IntPtr/*HWND*/ FindWindow(string lpClassName, string lpWindowName);
    [DllImport(dllname, SetLastError = true)] public static extern IntPtr/*LRESULT*/ SendMessage(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

    /*
     * Keyboard Input
     */
    [DllImport(dllname, SetLastError = true)] public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport(dllname, SetLastError = true)] public static extern uint SendInput(uint nInputs, [In] ref INPUT pInputs, int cbSize);

    [DllImport(dllname, SetLastError = true)] public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifier, int vk);
    [DllImport(dllname, SetLastError = true)] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
  }
}
