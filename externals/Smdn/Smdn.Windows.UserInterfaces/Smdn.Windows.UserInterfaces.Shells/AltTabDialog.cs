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

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.Shells {
  public static class AltTabDialog {
    public static void Show()
    {
      Show(3000);
    }

    public static void Show(int timeoutMilliseconds)
    {
      Show(TimeSpan.FromMilliseconds((double)timeoutMilliseconds));
    }

    public static void Show(TimeSpan timeout)
    {
      Show(timeout, delegate {
        System.Threading.Thread.Sleep(timeout);
      });
    }

    public static void SelectLeft()
    {
      Select(-1);
    }

    public static void SelectLeft(int count)
    {
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);

      Select(-count);
    }

    public static void SelectRight()
    {
      Select(+1);
    }

    public static void SelectRight(int count)
    {
      if (count < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("count", count);

      Select(+count);
    }

    public static void Select(int count)
    {
      var input = INPUT.CreateKeyboardInput();

      input.ki.wScan = 0;
      input.ki.time = 0;

      if (count < 0) {
        input.ki.wVk = VK.LEFT;
        count = -count;
      }
      else {
        input.ki.wVk = VK.RIGHT;
        count = +count;
      }

      Show(TimeSpan.Zero, delegate {
        for (var i = 0; i < count; i++) {
          // send LEFT/RIGHT down
          input.ki.dwFlags = KEYEVENTF.None;

          SendKeyInputOrThrow(ref input);

          System.Threading.Thread.Sleep(50);

          // send LEFT/RIGHT up
          input.ki.dwFlags = KEYEVENTF.KEYUP;

          SendKeyInputOrThrow(ref input);
        }
      });
    }

    private static void Show(TimeSpan timeout, Action action)
    {
      var input = INPUT.CreateKeyboardInput();

      input.ki.wScan = 0;
      input.ki.time = 0;

      // send ALT down
      input.ki.dwFlags = KEYEVENTF.None;
      input.ki.wVk = VK.MENU;

      SendKeyInputOrThrow(ref input);

      // send TAB down
      input.ki.dwFlags = KEYEVENTF.None;
      input.ki.wVk = VK.TAB;

      SendKeyInputOrThrow(ref input);

      // send TAB up
      input.ki.dwFlags = KEYEVENTF.KEYUP;
      input.ki.wVk = VK.TAB;

      SendKeyInputOrThrow(ref input);

      if (action != null)
        action();

      // send ALT up
      input.ki.dwFlags = KEYEVENTF.KEYUP;
      input.ki.wVk = VK.MENU;

      SendKeyInputOrThrow(ref input);
    }

    private static void SendKeyInputOrThrow(ref INPUT input)
    {
      if (0 == user32.SendInput(1, ref input, INPUT.Size))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }
  }
}
