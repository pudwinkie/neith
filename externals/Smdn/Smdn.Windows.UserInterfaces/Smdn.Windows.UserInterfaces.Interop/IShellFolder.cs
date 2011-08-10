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
  public static partial class IID {
    public const string IShellFolder = "000214E6-0000-0000-C000-000000000046";
  }

  [CLSCompliant(false), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID.IShellFolder)]
  public interface IShellFolder { // cannot list any base interfaces here
    void BindToObject(/* PCUIDLIST_RELATIVE */ IntPtr pidl,
                      /* IBindCtx* */ IntPtr pbc,
                      [In] ref /* RFIID */ Guid riid,
                      [MarshalAs(UnmanagedType.Interface)] out object ppv);

    void BindToObject(/* PCUIDLIST_RELATIVE */ IntPtr pidl,
                      /* IBindCtx* */ IntPtr pbc,
                      [In] ref /* RFIID */ Guid riid,
                      [MarshalAs(UnmanagedType.Interface)] out IShellFolder ppv);

    void BindToStorage(/* PCUIDLIST_RELATIVE */ IntPtr pidl,
                       /* IBindCtx* */ IntPtr pbc,
                       [In] ref /* REFIID*/ Guid riid,
                       [MarshalAs(UnmanagedType.Interface)] out object ppv);

    /*HRESULT*/ int CompareIDs(/* LPARAM */ IntPtr lParam,
                               /* PCUIDLIST_RELATIVE */ IntPtr pidl1,
                               /* PCUIDLIST_RELATIVE */ IntPtr pidl2);


    void CreateViewObject(IntPtr hwndOwner,
                          [In] ref /* REFIID*/ Guid riid,
                          [MarshalAs(UnmanagedType.Interface)] out object ppv);

    void EnumObjects(IntPtr hwnd,
                     SHCONTF grfFlags,
                     out IEnumIDList ppenumIDList);

    void GetAttributesOf(uint cidl,
                         out /* PCUITEMID_CHILD_ARRAY */ IntPtr apidl,
                         out SFGAOF rgfInOut);

    void GetDisplayNameOf(/* PCUITEMID_CHILD */ IntPtr pidl,
                          SHGDNF uFlags,
                          out STRRETW pName);

    void GetUIObjectOf(IntPtr hwndOwner,
                       uint cidl,
                       [In, MarshalAs(UnmanagedType.LPArray)] /* PCUITEMID_CHILD_ARRAY */ IntPtr[] apidl,
                       ref /* REFIID */ Guid riid,
                       ref uint rgfReserved,
                       [MarshalAs(UnmanagedType.Interface)] out object ppv);

    void ParseDisplayName(IntPtr hwnd,
                          /* IBindCtx */ IntPtr pdc,
                          [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
                          out uint pchEaten,
                          /* PIDLIST_RELATIVE */ out IntPtr ppidl,
                          ref SFGAOF pdwAttributes);

    [PreserveSig]
    int ParseDisplayName(IntPtr hwnd,
                          /* IBindCtx */ IntPtr pdc,
                          [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
                          IntPtr pchEaten,
                          /* PIDLIST_RELATIVE */ out IntPtr ppidl,
                          IntPtr pdwAttributes);

    void SetNameOf(IntPtr hwnd,
                   /* PCUITEMID_CHILD */ IntPtr pidl,
                   [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                   SHGDNF uFlags,
                   out /* PITEMID_CHILD */ IntPtr ppidOut);
  }
}
