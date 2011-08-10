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
using System.Reflection;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.IO;

namespace Smdn.Windows.UserInterfaces.Shells {
  public class Shortcut {
    public bool Exists {
      get { return Find() != null; }
    }

    public Shortcut(Environment.SpecialFolder location)
      : this(location, GetDefaultDisplayName(), GetDefaultTargetPath())
    {
    }

    public Shortcut(Environment.SpecialFolder location, string displayName)
      : this(location, displayName, GetDefaultTargetPath())
    {
    }

    public Shortcut(Environment.SpecialFolder location, string displayName, string targetPath)
    {
      if (displayName == null)
        throw new ArgumentNullException("displayName");
      if (targetPath == null)
        throw new ArgumentNullException("targetPath");

      this.location = location;
      this.displayName = displayName;
      this.targetPath = targetPath;
    }

    private static string GetDefaultTargetPath()
    {
      return Assembly.GetEntryAssembly().Location;
    }

    private static string GetDefaultDisplayName()
    {
      var entryAssembly = Assembly.GetEntryAssembly();
      var title = entryAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).First() as AssemblyTitleAttribute;

      if (title == null)
        return entryAssembly.GetName().Name;
      else
        return title.Title;
    }

    private string Find()
    {
      foreach (var lnkFile in Directory.GetFiles(Environment.GetFolderPath(location), "*" + ShellLink.Extension, SearchOption.TopDirectoryOnly)) {
        if (PathUtils.ArePathEqual(Environment.ExpandEnvironmentVariables(ShellLink.GetLinkTargetPath(lnkFile)), targetPath))
          return lnkFile;
      }

      return null;
    }

    public void ToggleExistence()
    {
      var lnkFile = Find();

      if (lnkFile == null)
        Create();
      else
        Delete(lnkFile);
    }

    public void Create()
    {
      Create(null);
    }

    public void Create(Action<ShellLink> action)
    {
      using (var lnk = new ShellLink()) {
        lnk.TargetPath = targetPath;

        if (action != null)
          action(lnk);

        lnk.Save(Path.Combine(Environment.GetFolderPath(location), PathUtils.ReplaceInvalidFileNameChars(displayName + ".lnk", " ")));
      }
    }

    public void Delete()
    {
      var lnkFile = Find();

      if (lnkFile == null)
        throw new FileNotFoundException();
      else
        Delete(lnkFile);
    }

    private void Delete(string lnkFile)
    {
      File.Delete(lnkFile);
    }

    private Environment.SpecialFolder location;
    private string displayName;
    private string targetPath;
  }
}
