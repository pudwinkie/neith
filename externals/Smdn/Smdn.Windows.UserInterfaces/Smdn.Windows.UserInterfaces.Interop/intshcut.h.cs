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
  [CLSCompliant(false), Flags]
  public enum IURL_SETURL_FLAGS : uint {
    GUESS_PROTOCOL        = 0x0001,
    USE_DEFAULT_PROTOCOL  = 0x0002,
  }

  [CLSCompliant(false), Flags]
  public enum IURL_INVOKECOMMAND_FLAGS : uint {
    ALLOW_UI          = 0x0001,
    USE_DEFAULT_VERB  = 0x0002,
    DDEWAIT           = 0x0004,
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct URLINVOKECOMMANDINFO {
    public uint dwcbSize;
    public IURL_INVOKECOMMAND_FLAGS dwFlags;
    public IntPtr hwndParent;
    public string pcszVerb;

    public static readonly int Size = Marshal.SizeOf(typeof(URLINVOKECOMMANDINFO));
  }
}