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
   * http://msdn.microsoft.com/en-us/library/bb774950%28VS.85%29.aspx
   */
  public class ShellLink : IDisposable {
    /*
     * class members
     */
    public const string Extension = ".lnk";

    public static string GetLinkTargetPath(string linkFile)
    {
      using (var shellLink = new ShellLink(linkFile)) {
        return shellLink.TargetPath;
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

    public string TargetPath {
      get
      {
        CheckDisposed();

        var targetPath = CreatePathStringBuffer();

        if (shellLinkW != null) {
          var data = new WIN32_FIND_DATAW();

          shellLinkW.GetPath(targetPath, targetPath.Capacity, ref data, SLGP_FLAGS.UNCPRIORITY);
        }
        else {
          var data = new WIN32_FIND_DATAA();

          shellLinkA.GetPath(targetPath, targetPath.Capacity, ref data, SLGP_FLAGS.UNCPRIORITY);
        }
        
        return targetPath.ToString();
      }
      set
      {
        CheckDisposed();

        if (shellLinkW != null)
          shellLinkW.SetPath(value);
        else
          shellLinkA.SetPath(value);
      }
    }

    public string WorkingDirectory {
      get
      {
        CheckDisposed();

        var workingDirectory = CreatePathStringBuffer();

        if (shellLinkW != null)
          shellLinkW.GetWorkingDirectory(workingDirectory, workingDirectory.Capacity);
        else
          shellLinkA.GetWorkingDirectory(workingDirectory, workingDirectory.Capacity);

        return workingDirectory.ToString();
      }
      set
      {
        CheckDisposed();

        if (shellLinkW != null)
          shellLinkW.SetWorkingDirectory(value);
        else
          shellLinkA.SetWorkingDirectory(value);
      }
    }

    public string Arguments {
      get
      {
        CheckDisposed();

        var arguments = CreatePathStringBuffer();

        if (shellLinkW != null)
          shellLinkW.GetArguments(arguments, arguments.Capacity);
        else
          shellLinkA.GetArguments(arguments, arguments.Capacity);

        return arguments.ToString();
      }
      set
      {
        CheckDisposed();

        if (shellLinkW != null)
          shellLinkW.SetArguments(value);
        else
          shellLinkA.SetArguments(value);
      }
    }

    public string Description {
      get
      {
        CheckDisposed();

        var description = CreatePathStringBuffer();

        if (shellLinkW != null)
          shellLinkW.GetDescription(description, description.Capacity);
        else
          shellLinkA.GetDescription(description, description.Capacity);

        return description.ToString();
      }
      set
      {
        CheckDisposed();

        if (shellLinkW != null)
          shellLinkW.SetDescription(value);
        else
          shellLinkA.SetDescription(value);
      }
    }

    public IconLocation IconLocation {
      get
      {
        CheckDisposed();

        var iconFileBuffer = CreatePathStringBuffer();
        int iconIndex;

        if (shellLinkW != null)
          shellLinkW.GetIconLocation(iconFileBuffer, iconFileBuffer.Capacity, out iconIndex);
        else
          shellLinkA.GetIconLocation(iconFileBuffer, iconFileBuffer.Capacity, out iconIndex);

        return new IconLocation(iconFileBuffer.ToString(), iconIndex);
      }
      set
      {
        CheckDisposed();

        if (shellLinkW != null)
          shellLinkW.SetIconLocation(value.File, value.Index);
        else
          shellLinkA.SetIconLocation(value.File, value.Index);
      }
    }

    public SW ShowCommand {
      get
      {
        CheckDisposed();

        SW showCmd;

        if (shellLinkW != null)
          shellLinkW.GetShowCmd(out showCmd);
        else
          shellLinkA.GetShowCmd(out showCmd);

        return showCmd;
      }
      set
      {
        CheckDisposed();

        if (shellLinkW != null)
          shellLinkW.SetShowCmd(value);
        else
          shellLinkA.SetShowCmd(value);
      }
    }

    public Keys HotKey {
      get
      {
        CheckDisposed();

        ushort hotKey;

        if (shellLinkW != null)
          shellLinkW.GetHotkey(out hotKey);
        else
          shellLinkA.GetHotkey(out hotKey);

        return TranslateKeyCode(hotKey);
      }
      set
      {
        CheckDisposed();

        var newHotKey = TranslateKeyCode(value);

        if (shellLinkW != null)
          shellLinkW.SetHotkey(newHotKey);
        else
          shellLinkA.SetHotkey(newHotKey);
      }
    }

    private IPersistFile PersistFile {
      get
      {
        CheckDisposed();

        var ret = (shellLinkW != null)
          ? shellLinkW as IPersistFile
          : shellLinkA as IPersistFile;

        if (ret == null)
          throw new COMException("cannot create IPersistFile");
        else
          return ret;
      }
    }

    public ShellLink()
      : this(null)
    {
    }

    public ShellLink(string linkFile)
    {
      try {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
          shellLinkW = (IShellLinkW)new ShellLinkObject();
          shellLinkA = null;
        }
        else {
          shellLinkA = (IShellLinkA)new ShellLinkObject();
          shellLinkW = null;
        }
      }
      catch {
        throw new COMException("cannot create ShellLinkObject");
      }

      if (linkFile != null)
        Load(linkFile);
    }

    ~ShellLink()
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
      if (shellLinkW != null) {
        Marshal.ReleaseComObject(shellLinkW);
        shellLinkW = null;
      }

      if (shellLinkA != null) {
        Marshal.ReleaseComObject(shellLinkA);
        shellLinkA = null;
      }
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
      Load(file, IntPtr.Zero, SLR_FLAGS.ANY_MATCH | SLR_FLAGS.NO_UI, 1);
    }

    [CLSCompliant(false)]
    public void Load(string file, IntPtr hWnd, SLR_FLAGS flags)
    {
      Load(file, hWnd, flags, 1);
    }

    [CLSCompliant(false)]
    public void Load(string file, IntPtr hWnd, SLR_FLAGS flags, TimeSpan timeOut)
    {
      Load(file, hWnd, flags, (int)timeOut.TotalMilliseconds);
    }

    [CLSCompliant(false)]
    public void Load(string file, IntPtr hWnd, SLR_FLAGS flags, int timeoutMilliseconds)
    {
      CheckDisposed();

      if (!File.Exists(file))
        throw new FileNotFoundException("file not found", file);

      PersistFile.Load(file, 0x00000000);

      if ((int)(flags & SLR_FLAGS.NO_UI) != 0)
        flags |= (SLR_FLAGS)(timeoutMilliseconds << 16);

      if (shellLinkW != null)
        shellLinkW.Resolve(hWnd, flags);
      else
        shellLinkA.Resolve(hWnd, flags);
    }

    private static StringBuilder CreatePathStringBuffer()
    {
      return new StringBuilder(Consts.MAX_PATH, Consts.MAX_PATH);
    }

    private static ushort TranslateKeyCode(Keys key)
    {
      // IShellLink::SetHotkey Method
      //   wHotkey
      //     The new keyboard shortcut. The virtual key code is in the low-order byte, and the modifier flags are in the high-order byte.
      //     The modifier flags can be a combination of the values specified in the description of the IShellLink::GetHotkey method.
      var virtKey  = ((int)(key & Keys.KeyCode) & 0x00ff);
      var modifier = (((int)(key & Keys.Modifiers) >> 8) & 0xff00);

      return (ushort)(virtKey | modifier);
    }

    private static Keys TranslateKeyCode(ushort key)
    {
      // IShellLink::GetHotkey Method
      //   pwHotkey
      //     The address of the keyboard shortcut. The virtual key code is in the low-order byte, 
      //     and the modifier flags are in the high-order byte. The modifier flags can be a combination of the following values.
      var virtKey = (Keys)(key & 0x00ff);
      var modifier = (Keys)((key & 0xff00) << 8);

      return virtKey | modifier;
    }

    private void CheckDisposed()
    {
      if (shellLinkW == null && shellLinkA == null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private IShellLinkW shellLinkW = null;
    private IShellLinkA shellLinkA = null;
  }
}