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
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Smdn.Windows.UserInterfaces.Shells {
  public class SpecialFolderInfo : SpecialFolderEntryInfo {
    public DirectoryInfo DirectoryInfo {
      get { return base.FileSystemInfo as DirectoryInfo; }
    }

    public DesktopIni DesktopIni {
      get; private set;
    }

    internal protected SpecialFolderInfo(SpecialFolderInfo parent, string directory)
      : this(parent, new DirectoryInfo(directory))
    {
    }

    internal protected SpecialFolderInfo(SpecialFolderInfo parent, DirectoryInfo directory)
      : base(parent, directory)
    {
      if (directory == null)
        throw new ArgumentNullException("directory");

      Refresh();
    }

    public IEnumerable<SpecialFolderEntryInfo> GetFolderEntries()
    {
      foreach (var subdir in GetDirectories()) {
        yield return subdir;
      }

      foreach (var file in GetFiles()) {
        yield return file;
      }
    }

    public IEnumerable<SpecialFolderInfo> GetDirectories()
    {
      var subDirs = new List<SpecialFolderInfo>();
      var root = Root;

      try {
        foreach (var subdirectory in DirectoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)) {
          var dir = root.CreateFolderInfo(this, subdirectory, desktopIni);

          if (dir != null)
            subDirs.Add(dir);
        }
      }
      catch (UnauthorizedAccessException) {
        // ignore
      }
      catch (DirectoryNotFoundException) {
        // continue
      }

      return subDirs;
    }

    protected virtual SpecialFolderInfo CreateFolderInfo(SpecialFolderInfo parent, DirectoryInfo directory, DesktopIni desktopIni)
    {
      return new SpecialFolderInfo(parent, directory);
    }

    public IEnumerable<SpecialFolderFileInfo> GetFiles()
    {
      var files = new List<SpecialFolderFileInfo>();
      var root = Root;

      // search files
      try {
        foreach (var file in DirectoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly)) {
          if (Smdn.IO.PathUtils.ArePathEqual(file.Name, DesktopIni.FileName))
            continue;

          var f = root.CreateFileInfo(this, file, desktopIni);

          if (f != null)
            files.Add(f);
        }
      }
      catch (UnauthorizedAccessException) {
        // ignore
      }
      catch (FileNotFoundException) {
        // continue
      }

      return files;
    }

    protected virtual SpecialFolderFileInfo CreateFileInfo(SpecialFolderInfo parent, FileInfo file, DesktopIni desktopIni)
    {
      return new SpecialFolderFileInfo(parent, file, (desktopIni == null) ? file.Name : desktopIni.GetLocalizedFileName(file.Name));
    }

    public virtual void Refresh()
    {
      DirectoryInfo.Refresh();

      var desktopIniFile = Path.Combine(DirectoryInfo.FullName, DesktopIni.FileName);

      desktopIni = File.Exists(desktopIniFile) ? new DesktopIni(desktopIniFile) : null;

      if (desktopIni == null)
        base.Name = DirectoryInfo.Name;
      else
        base.Name = desktopIni.LocalizedString ?? DirectoryInfo.Name;
    }

    public Bitmap GetIcon()
    {
      if (desktopIni == null)
        return null;
      else
        return desktopIni.IconLocation.Extract();
    }

    private DesktopIni desktopIni = null;
  }
}
