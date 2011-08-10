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
  public static class advapi32 {
    private const string dllname = "advapi32";

    [DllImport(dllname, SetLastError = true)] public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, ref IntPtr TokenHandle);
    [DllImport(dllname, SetLastError = true)] public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);
    [DllImport(dllname, SetLastError = true)] public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
                                                                                              bool DisableAllPrivileges,
                                                                                              ref TOKEN_PRIVILEGES NewState,
                                                                                              uint BufferLength,
                                                                                              ref TOKEN_PRIVILEGES PreviousState,
                                                                                              ref uint ReturnLength);
    [DllImport(dllname, SetLastError = true)] public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
                                                                                              bool DisableAllPrivileges,
                                                                                              ref TOKEN_PRIVILEGES NewState,
                                                                                              uint BufferLength,
                                                                                              IntPtr PreviousState,
                                                                                              IntPtr ReturnLength);
  }
}
