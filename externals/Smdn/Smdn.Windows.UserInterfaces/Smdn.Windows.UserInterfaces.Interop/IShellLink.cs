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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Smdn.Windows.UserInterfaces.Interop {
  public static partial class IID {
    public const string IShellLinkW = "000214F9-0000-0000-C000-000000000046";
    public const string IShellLinkA = "000214EE-0000-0000-C000-000000000046";
  }

  [CLSCompliant(false), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID.IShellLinkW)]
  public interface IShellLinkW { // cannot list any base interfaces here
    //HRESULT GetPath([out, size_is(cch)] LPWSTR pszFile, [in] int cch, [in, out, ptr] WIN32_FIND_DATAW *pfd, [in] DWORD fFlags);
    void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, ref WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);

    //HRESULT GetIDList([out] LPITEMIDLIST * ppidl);
    void GetIDList(out IntPtr ppidl);

    //HRESULT SetIDList([in] LPCITEMIDLIST pidl);
    void SetIDList(IntPtr pidl);

    //HRESULT GetDescription([out, size_is(cch)] LPWSTR pszName, int cch);
    void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);

    //HRESULT SetDescription([in] LPCWSTR pszName);
    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

    //HRESULT GetWorkingDirectory([out, size_is(cch)] LPWSTR pszDir, int cch);
    void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);

    //HRESULT SetWorkingDirectory([in] LPCWSTR pszDir);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

    //HRESULT GetArguments([out, size_is(cch)] LPWSTR pszArgs, int cch);
    void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);

    //HRESULT SetArguments([in] LPCWSTR pszArgs);
    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

    //HRESULT GetHotkey([out] WORD *pwHotkey);
    void GetHotkey(out ushort pwHotkey);

    //HRESULT SetHotkey([in] WORD wHotkey);
    void SetHotkey(ushort wHotkey);

    //HRESULT GetShowCmd([out] int *piShowCmd);
    void GetShowCmd(out SW piShowCmd);

    //HRESULT SetShowCmd([in] int iShowCmd);
    void SetShowCmd(SW iShowCmd);

    //HRESULT GetIconLocation([out, size_is(cch)] LPWSTR pszIconPath, [in] int cch, [out] int *piIcon);
    void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);

    //HRESULT SetIconLocation([in] LPCWSTR pszIconPath, [in] int iIcon);
    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

    //HRESULT SetRelativePath([in] LPCWSTR pszPathRel, [in] DWORD dwReserved);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

    //HRESULT Resolve([in] HWND hwnd, [in] DWORD fFlags);
    void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

    //HRESULT SetPath([in] LPCWSTR pszFile);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
  }

  [CLSCompliant(false), ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(IID.IShellLinkA)]
  public interface IShellLinkA { // cannot list any base interfaces here
    //HRESULT GetPath([out, size_is(cch)] LPSTR pszFile, [in] int cch, [in, out, ptr] WIN32_FIND_DATAW *pfd, [in] DWORD fFlags);
    void GetPath([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cch, ref WIN32_FIND_DATAA pfd, SLGP_FLAGS fFlags);

    //HRESULT GetIDList([out] LPITEMIDLIST * ppidl);
    void GetIDList(out IntPtr ppidl);

    //HRESULT SetIDList([in] LPCITEMIDLIST pidl);
    void SetIDList(IntPtr pidl);

    //HRESULT GetDescription([out, size_is(cch)] LPSTR pszName, int cch);
    void GetDescription([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, int cch);

    //HRESULT SetDescription([in] LPCSTR pszName);
    void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

    //HRESULT GetWorkingDirectory([out, size_is(cch)] LPSTR pszDir, int cch);
    void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cch);

    //HRESULT SetWorkingDirectory([in] LPCSTR pszDir);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

    //HRESULT GetArguments([out, size_is(cch)] LPSTR pszArgs, int cch);
    void GetArguments([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cch);

    //HRESULT SetArguments([in] LPCSTR pszArgs);
    void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

    //HRESULT GetHotkey([out] WORD *pwHotkey);
    void GetHotkey(out ushort pwHotkey);

    //HRESULT SetHotkey([in] WORD wHotkey);
    void SetHotkey(ushort wHotkey);

    //HRESULT GetShowCmd([out] int *piShowCmd);
    void GetShowCmd(out SW piShowCmd);

    //HRESULT SetShowCmd([in] int iShowCmd);
    void SetShowCmd(SW iShowCmd);

    //HRESULT GetIconLocation([out, size_is(cch)] LPSTR pszIconPath, [in] int cch, [out] int *piIcon);
    void GetIconLocation([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cch, out int piIcon);

    //HRESULT SetIconLocation([in] LPCSTR pszIconPath, [in] int iIcon);
    void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

    //HRESULT SetRelativePath([in] LPCSTR pszPathRel, [in] DWORD dwReserved);
    void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);

    //HRESULT Resolve([in] HWND hwnd, [in] DWORD fFlags);
    void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

    //HRESULT SetPath([in] LPCSTR pszFile);
    void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
  }
}
