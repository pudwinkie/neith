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
using System.Text;
using System.Runtime.InteropServices;

using Smdn.Interop;
using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces {
  public static class Resource {
    public static bool IsIntResource(int i)
    {
      return (i >> 16) == 0;
    }

    public static bool IsIntResource(IntPtr i)
    {
      // IS_INTRESOURCE(i) (((ULONG_PTR)(i) >> 16) == 0)
      return (i.ToInt64() >> 16) == 0;
    }

    public static IntPtr MakeIntResource(short i)
    {
      // #define MAKEINTRESOURCEA(i) ((LPSTR)((ULONG_PTR)((WORD)(i))))
      // #define MAKEINTRESOURCEW(i) ((LPWSTR)((ULONG_PTR)((WORD)(i))))
      return new IntPtr(i);
    }

    public static string LoadString(string moduleName, int resourceId)
    {
      string ret = null;

      UsingModuleHandle(moduleName, delegate(IntPtr hInstance) {
        var lpBuffer = new StringBuilder(0x200, 0x200);
        var uID = unchecked((resourceId < 0) ? (uint)-resourceId : (uint)resourceId); // TODO: use IsIntResource

        if (0 == Smdn.Windows.UserInterfaces.Interop.user32.LoadString(hInstance, uID, lpBuffer, lpBuffer.Capacity))
          throw new Win32Exception(Marshal.GetLastWin32Error());

        ret = lpBuffer.ToString();
      });

      return ret;
    }

    private static void UsingModuleHandle(string moduleName, Action<IntPtr> action)
    {
      if (moduleName == null)
        throw new ArgumentNullException("moduleName");

      var hInstance = Smdn.Interop.kernel32.GetModuleHandle(moduleName);

      if (hInstance == IntPtr.Zero) {
        using (var module = new DynamicLinkLibrary(moduleName)) {
          action(module.Handle);
        }
      }
      else {
        action(hInstance);
      }
    }
  }
}
