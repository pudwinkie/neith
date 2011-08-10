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
  public static partial class IID {
    public const string IExtractIconW = "000214FA-0000-0000-C000-000000000046";
    public const string IExtractIconA = "000214EB-0000-0000-C000-000000000046";
  }

  [CLSCompliant(false), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID.IExtractIconW)]
  public interface IExtractIconW { // cannot list any base interfaces here
    void Extract([MarshalAs(UnmanagedType.LPWStr)] string pszFile,
                 uint nIconIndex,
                 out /* HICON */ IntPtr phiconLarge,
                 out /* HICON */ IntPtr phiconSmall,
                 uint nIconSize);

    void GetIconLocation(GIL uFlags,
                         [MarshalAs(UnmanagedType.LPWStr)] StringBuilder dzIconFile,
                         uint cchMax,
                         out int piIndex,
                         out GIL pwFlags);
  }

  [CLSCompliant(false), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID.IExtractIconA)]
  public interface IExtractIconA {
    void Extract([MarshalAs(UnmanagedType.LPStr)] string pszFile,
                 uint nIconIndex,
                 out /* HICON */ IntPtr phiconLarge,
                 out /* HICON */ IntPtr phiconSmall,
                 uint nIconSize);

    void GetIconLocation(GIL uFlags,
                         [MarshalAs(UnmanagedType.LPStr)] StringBuilder dzIconFile,
                         uint cchMax,
                         out int piIndex,
                         out GIL pwFlags);
  }
}
