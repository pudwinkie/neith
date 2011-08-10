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

namespace Smdn.Windows.UserInterfaces.Interop {
  public static partial class Consts {
    public const uint STRRET_WSTR = 0;
    public const uint STRRET_OFFSET = 1;
    public const uint STRRET_CSTR = 2;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Explicit)]
  public struct STRRETW {
    [FieldOffset(0)] public uint uType;

    [FieldOffset(4), MarshalAs(UnmanagedType.LPWStr)] public string pOleStr;
    [FieldOffset(4)] public uint uOffset;
    [FieldOffset(4), MarshalAs(UnmanagedType.ByValTStr, SizeConst = Consts.MAX_PATH)] public string cFileName;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Explicit)]
  public struct STRRETA {
    [FieldOffset(0)] public uint uType;

    [FieldOffset(4), MarshalAs(UnmanagedType.LPStr)] public string pOleStr;
    [FieldOffset(4)] public uint uOffset;
    [FieldOffset(4), MarshalAs(UnmanagedType.ByValTStr, SizeConst = Consts.MAX_PATH)] public string cFileName;
  }
}
