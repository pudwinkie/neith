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

using Smdn.Windows.BaseServices.Interop;

namespace Smdn.Windows.BaseServices.PlugAndPlay {
  public static class ThreadExecutionState {
    [Flags]
    public enum Target : int {
      None      = 0x00000000,
      System    = (int)EXECUTION_STATE.SYSTEM_REQUIRED,
      Display   = (int)EXECUTION_STATE.DISPLAY_REQUIRED,
      All       = System | Display,
    }

    private static EXECUTION_STATE GetCurrentState()
    {
      var currentState = kernel32.SetThreadExecutionState(EXECUTION_STATE.CONTINUOUS);

      if (0 == currentState)
        throw new Win32Exception(Marshal.GetLastWin32Error());
      else
        return currentState;
    }

    public static void RevertActivated(Target target)
    {
      var newState = GetCurrentState();

      newState &= ~(EXECUTION_STATE)target;
      newState |= EXECUTION_STATE.CONTINUOUS;

      if (0 == kernel32.SetThreadExecutionState(newState))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void KeepActive(Target target)
    {
      var newState = GetCurrentState();

      newState |= (EXECUTION_STATE)target;
      newState |= EXECUTION_STATE.CONTINUOUS;

      if (0 == kernel32.SetThreadExecutionState(newState))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }
  }
}
