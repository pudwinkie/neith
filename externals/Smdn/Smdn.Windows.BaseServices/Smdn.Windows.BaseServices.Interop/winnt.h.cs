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
  public partial class Consts {
    [CLSCompliant(false)] public const uint TOKEN_ASSIGN_PRIMARY    = 0x0001;
    [CLSCompliant(false)] public const uint TOKEN_DUPLICATE         = 0x0002;
    [CLSCompliant(false)] public const uint TOKEN_IMPERSONATE       = 0x0004;
    [CLSCompliant(false)] public const uint TOKEN_QUERY             = 0x0008;
    [CLSCompliant(false)] public const uint TOKEN_QUERY_SOURCE      = 0x0010;
    [CLSCompliant(false)] public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    [CLSCompliant(false)] public const uint TOKEN_ADJUST_GROUPS     = 0x0040;
    [CLSCompliant(false)] public const uint TOKEN_ADJUST_DEFAULT    = 0x0080;
    [CLSCompliant(false)] public const uint TOKEN_ADJUST_SESSIONID  = 0x0100;

    public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

    [CLSCompliant(false)] public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
    [CLSCompliant(false)] public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    [CLSCompliant(false)] public const uint SE_PRIVILEGE_REMOVED = 0X00000004;
    [CLSCompliant(false)] public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct LUID {
    public uint LowPart;
    public int HighPart;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct LUID_AND_ATTRIBUTES {
    public LUID Luid;
    public uint Attributes;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct TOKEN_PRIVILEGES {
    public uint PrivilegeCount;
    public LUID_AND_ATTRIBUTES Privilege; // Privileges[ANYSIZE_ARRAY], ANYSIZE_ARRAY = 1
  }
}
