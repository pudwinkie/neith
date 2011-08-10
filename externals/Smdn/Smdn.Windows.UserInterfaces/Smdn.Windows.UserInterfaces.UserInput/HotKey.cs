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
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.UserInput {
  public class HotKey : IDisposable {
    public const Keys ModifierWin = (Keys)((int)MOD.WIN << 16);

    public Keys Key {
      get; private set;
    }

    protected short Atom {
      get; private set;
    }

    public IntPtr WindowHandle {
      get; private set;
    }

    public bool IsRegistered {
      get { CheckDisposed(); return WindowHandle != IntPtr.Zero; }
    }

    protected int Modifier {
      get { return (int)(Key & Keys.Modifiers) >> 16; }
    }

    protected int VirtKey {
      get { return (int)(Key & Keys.KeyCode); }
    }

    protected IntPtr LParam {
      // WM_HOTKEY
      //   fuModifiers = (UINT) LOWORD(lParam);
      //   uVirtKey = (UINT) HIWORD(lParam);
      get { return new IntPtr((VirtKey << 16) | Modifier); }
    }

    public HotKey(Keys key)
    {
      if (!IsValidKey(key))
        throw new ArgumentException("invalid key", "key");

      this.WindowHandle = IntPtr.Zero;
      this.Key = key;
      this.Atom = kernel32.GlobalAddAtom(CreateAtomStringFromKeys(key));

      if (this.Atom == 0)
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    ~HotKey()
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
      if (Atom == 0) // disposed
        return;

      if (IsRegistered)
        Unregister();

      kernel32.GlobalDeleteAtom(Atom);

      this.Atom = 0;
    }

    public static bool IsValidKey(Keys key)
    {
      //var modifier = key & Keys.Modifiers;
      key &= Keys.KeyCode;

      if (key == Keys.None)
        return false;

      if (Keys.A <= key && key <= Keys.Z)
        return true;
      else if (Keys.NumPad0 <= key && key <= Keys.NumPad9)
        return true;
      else if (Keys.D0 <= key && key <= Keys.D9)
        return true;
      else if (Keys.F1 <= key && key <= Keys.F24)
        return true;

      switch (key) {
        case Keys.Escape: return true;
        case Keys.Enter:  return true;

        case Keys.Home: return true;
        case Keys.End:  return true;

        case Keys.PageUp:   return true;
        case Keys.PageDown: return true;

        case Keys.Left:  return true;
        case Keys.Right: return true;
        case Keys.Up:    return true;
        case Keys.Down:  return true;

        case Keys.Add:      return true;
        case Keys.Subtract: return true;
        case Keys.Divide:   return true;
        case Keys.Multiply: return true;
        case Keys.Decimal:  return true;

        case Keys.Insert:  return true;
        case Keys.Delete:  return true;
        case Keys.Back:    return true;
        case Keys.Tab:     return true;
        case Keys.Space:   return true;
        case Keys.Capital: return true;

        case Keys.PrintScreen: return true;
        case Keys.Scroll:      return true;
        case Keys.Pause:       return true;
        case Keys.NumLock:     return true;
      }

      return false;
    }

    private static string CreateAtomStringFromKeys(Keys key)
    {
      var modifiers = key & Keys.Modifiers;
      var keyString = string.Empty;

      if ((int)(modifiers & Keys.Alt) != 0)
        keyString += "Alt";
      if ((int)(modifiers & Keys.Control) != 0)
        keyString += "Ctrl";
      if ((int)(modifiers & Keys.Shift) != 0)
        keyString += "Shift";
      if ((int)(modifiers & ModifierWin) != 0)
        keyString += "Win";

      keyString += (key & Keys.KeyCode).ToString();

      return keyString;
    }

    public bool IsPressed(System.Windows.Forms.Message m)
    {
      if (m.Msg != (int)WM.HOTKEY)
        return false;
      if (m.HWnd != WindowHandle)
        return false; // not registered or not target window
      if (m.LParam != LParam)
        return false;

      return true;
    }

    public void Register(IntPtr hWnd)
    {
      CheckDisposed();

      if (hWnd == IntPtr.Zero)
        throw new ArgumentException("hWnd == NULL", "hWnd");
      if (WindowHandle != IntPtr.Zero)
        Unregister();

      if (!user32.RegisterHotKey(hWnd, Atom, Modifier, VirtKey))
        throw new Win32Exception(Marshal.GetLastWin32Error());

      WindowHandle = hWnd;
    }

    public void Unregister()
    {
      CheckDisposed();

      if (!IsRegistered)
        return;

      if (!user32.UnregisterHotKey(WindowHandle, Atom))
        throw new Win32Exception(Marshal.GetLastWin32Error());

      WindowHandle = IntPtr.Zero;
    }

    private void CheckDisposed()
    {
      if (Atom == 0)
        throw new ObjectDisposedException(GetType().FullName);
    }
  }
}
