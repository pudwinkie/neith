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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.Shells {
  public class ShellFolder : IDisposable {
    /*
     * class members
     */
    private static IShellFolder shellFolderDesktop;

    static ShellFolder()
    {
      shell32.SHGetDesktopFolder(out shellFolderDesktop);
    }

    public static ShellFolder Create(string pathOrDisplayName)
    {
      return Create(IntPtr.Zero, pathOrDisplayName);
    }

    public static ShellFolder Create(IntPtr hWnd, string pathOrDisplayName)
    {
      if (pathOrDisplayName.EndsWith(Path.DirectorySeparatorChar.ToString()))
        pathOrDisplayName = pathOrDisplayName.Substring(0, pathOrDisplayName.Length - 1);

      IntPtr pidl;

      Console.WriteLine(pathOrDisplayName);

      var ret = shellFolderDesktop.ParseDisplayName(hWnd, IntPtr.Zero, pathOrDisplayName, IntPtr.Zero, out pidl, IntPtr.Zero);

      Console.WriteLine(ret);
      Console.WriteLine(pidl);

      return new ShellFolder(pidl);
    }

    /*
     * instance members
     */
    private ShellFolder(IntPtr pidl)
    {
      var guid = new Guid(IID.IShellFolder);

      shellFolderDesktop.BindToObject(pidl, IntPtr.Zero, ref guid, out shellFolder);
    }

    ~ShellFolder()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (shellFolder != null) {
        Marshal.ReleaseComObject(shellFolder);
        shellFolder = null;
      }
    }

    private void CheckDisposed()
    {
      if (shellFolder == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private IShellFolder shellFolder;
  }
}
