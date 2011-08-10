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

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.Shells {
  public static class RecycleBin {
    public static int GetItemCount()
    {
      return GetItemCount(string.Empty);
    }

    public static int GetItemCount(DriveInfo drive)
    {
      return GetItemCount(drive.RootDirectory);
    }

    public static int GetItemCount(DirectoryInfo rootDirectory)
    {
      return GetItemCount(rootDirectory.FullName);
    }

    public static int GetItemCount(string rootPath)
    {
      return (int)GetInfo(rootPath).i64NumItems;
    }

    public static long GetSize()
    {
      return GetSize(string.Empty);
    }

    public static long GetSize(DriveInfo drive)
    {
      return GetSize(drive.RootDirectory);
    }

    public static long GetSize(DirectoryInfo rootDirectory)
    {
      return GetSize(rootDirectory.FullName);
    }

    public static long GetSize(string rootPath)
    {
      return (long)GetInfo(rootPath).i64Size;
    }

    private static SHQUERYRBINFO GetInfo(string rootPath)
    {
      var info = new SHQUERYRBINFO();

      info.cbSize = (uint)SHQUERYRBINFO.Size;

      Marshal.ThrowExceptionForHR(shell32.SHQueryRecycleBin(rootPath, ref info));

      return info;
    }

    public static void Empty()
    {
      Empty(string.Empty);
    }

    public static void Empty(DriveInfo drive)
    {
      Empty(drive.RootDirectory);
    }

    public static void Empty(DirectoryInfo rootDirectory)
    {
      Empty(rootDirectory.FullName);
    }

    public static void Empty(string rootPath)
    {
      Empty(IntPtr.Zero, rootPath);
    }

    public static void Empty(IntPtr hWnd, string rootPath)
    {
      Empty(hWnd, rootPath, true, true, false);
    }

    public static void Empty(IntPtr hWnd, string rootPath, bool confirm, bool showProgress, bool noSound)
    {
      var flags = (SHERB)0;

      if (!confirm)
        flags |= SHERB.NOCONFIRMATION;
      if (!showProgress)
        flags |= SHERB.NOPROGRESSUI;
      if (noSound)
        flags |= SHERB.NOSOUND;

      Marshal.ThrowExceptionForHR(shell32.SHEmptyRecycleBin(hWnd, rootPath ?? string.Empty, flags));
    }
  }
}
