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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.Shells {
  /*
   * http://msdn.microsoft.com/en-us/library/bb776784%28VS.85%29.aspx
   * http://msdn.microsoft.com/en-us/library/dd565671%28VS.85%29.aspx
   */
  public class InternetShortcut : IDisposable {
    /*
     * class members
     */
    public static string GetUrl(string shortcutFile)
    {
      using (var internetShortcut = new InternetShortcut(shortcutFile)) {
        return internetShortcut.URL;
      }
    }

    /*
     * instance members
     */
    public string CurrentFile {
      get
      {
        string file;

        PersistFile.GetCurFile(out file);

        return file;
      }
    }

    public string URL {
      get
      {
        CheckDisposed();

        var ppszURL = IntPtr.Zero;

        try {
          uniformResourceLocatorW.GetURL(out ppszURL);

          return Marshal.PtrToStringUni(ppszURL);
        }
        finally {
          // http://www.atmarkit.co.jp/bbs/phpBB/viewtopic.php?forum=7&topic=27084
          if (ppszURL != IntPtr.Zero)
            Marshal.FreeCoTaskMem(ppszURL);
        }
      }
      set
      {
        CheckDisposed();

        uniformResourceLocatorW.SetURL(value, (IURL_SETURL_FLAGS)0);
      }
    }

    private IPersistFile PersistFile {
      get
      {
        CheckDisposed();

        var ret = uniformResourceLocatorW as IPersistFile;

        if (ret == null)
          throw new COMException("cannot create IPersistFile");
        else
          return ret;
      }
    }

    public InternetShortcut()
      : this(null)
    {
    }

    public InternetShortcut(string shortcutFile)
    {
      try {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
          uniformResourceLocatorW = (IUniformResourceLocatorW)new InternetShortcutObject();
        }
        else {
          throw new NotImplementedException("IUniformResourceLocatorA is not implemented");
        }
      }
      catch {
        throw new COMException("cannot create ShellLinkObject");
      }

      if (shortcutFile != null)
        Load(shortcutFile);
    }

    ~InternetShortcut()
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
      if (uniformResourceLocatorW != null) {
        Marshal.ReleaseComObject(uniformResourceLocatorW);
        uniformResourceLocatorW = null;
      }
    }

    public void InvokeCommand()
    {
      InvokeCommand(IntPtr.Zero, null);
    }

    public void InvokeCommand(IntPtr hwndParent)
    {
      InvokeCommand(hwndParent, null);
    }

    public void InvokeCommand(IntPtr hwndParent, string verb)
    {
      CheckDisposed();

      var urlCommandInfo = new URLINVOKECOMMANDINFO();

      urlCommandInfo.dwcbSize = (uint)URLINVOKECOMMANDINFO.Size;
      urlCommandInfo.dwFlags = IURL_INVOKECOMMAND_FLAGS.ALLOW_UI;
      urlCommandInfo.hwndParent = hwndParent;

      if (verb == null)
        urlCommandInfo.dwFlags |= IURL_INVOKECOMMAND_FLAGS.USE_DEFAULT_VERB;
      else
        urlCommandInfo.pcszVerb = verb;

      uniformResourceLocatorW.InvokeCommand(ref urlCommandInfo);
    }

    public void Save()
    {
      var file = CurrentFile;

      if (file == null)
        throw new InvalidOperationException("file name must be specified");

      Save(file);
    }

    public void Save(string file)
    {
      CheckDisposed();

      if (file == null)
        throw new ArgumentNullException("file");

      PersistFile.Save(file, true);
    }

    public void Load(string file)
    {
      CheckDisposed();

      if (!File.Exists(file))
        throw new FileNotFoundException("file not found", file);

      PersistFile.Load(file, 0x00000000);
    }

    private void CheckDisposed()
    {
      if (uniformResourceLocatorW == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private IUniformResourceLocatorW uniformResourceLocatorW = null;
  }
}