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

namespace Smdn.Windows.BaseServices.Interop {
  [CLSCompliant(false)]
  public static partial class Consts {
    public const uint DELETE                    = 0x00010000;
    public const uint READ_CONTROL              = 0x20000;
    public const uint WRITE_DAC                 = 0x40000;
    public const uint WRITE_OWNER               = 0x80000;
    public const uint SYNCHRONIZE               = 0x100000;
    public const uint STANDARD_RIGHTS_REQUIRED  = 0xf0000;
    public const uint STANDARD_RIGHTS_READ      = 0x20000;
    public const uint STANDARD_RIGHTS_WRITE     = 0x20000;
    public const uint STANDARD_RIGHTS_EXECUTE   = 0x20000;
    public const uint STANDARD_RIGHTS_ALL       = 0x1f0000;
    public const uint SPECIFIC_RIGHTS_ALL       = 0xffff;
    public const uint ACCESS_SYSTEM_SECURITY    = 0x1000000;
    public const uint MAXIMUM_ALLOWED           = 0x2000000;
    public const uint GENERIC_READ              = 0x80000000;
    public const uint GENERIC_WRITE             = 0x40000000;
    public const uint GENERIC_EXECUTE           = 0x20000000;
    public const uint GENERIC_ALL               = 0x10000000;

    public const uint CREATE_NEW        = 1;
    public const uint CREATE_ALWAYS     = 2;
    public const uint OPEN_EXISTING     = 3;
    public const uint OPEN_ALWAYS       = 4;
    public const uint TRUNCATE_EXISTING = 5;

    public static readonly IntPtr INVALID_FILE_ATTRIBUTES = new IntPtr(-1);
  }

  [CLSCompliant(false), Flags]
  public enum FILE_SHARE : uint {
    None        = 0,
    READ        = 0x00000001,
    WRITE       = 0x00000002,
    DELETE      = 0x00000004,
    VALID_FLAGS = 0x00000007,
  }

  [CLSCompliant(false), Flags]
  public enum FILE_ATTRIBUTE : uint {
    None        = 0,
    // TODO: defines
  }

  [CLSCompliant(false), Flags]
  public enum EXECUTION_STATE : uint {
    SYSTEM_REQUIRED   = 0x00000001,
    DISPLAY_REQUIRED  = 0x00000002,
    USER_PRESENT      = 0x00000004,
    CONTINUOUS        = 0x80000000,
  }
}

