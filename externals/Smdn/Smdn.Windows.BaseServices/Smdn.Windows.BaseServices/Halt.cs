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
using System.Diagnostics;
using System.Runtime.InteropServices;

using Smdn.Windows.BaseServices.Interop;

namespace Smdn.Windows.BaseServices {
  public static class Halt {
    public enum HaltMethod {
      LogOff,
      Shutdown,
      Reboot,
      PowerOff,
      Lock,
      Suspend,
      Hibernate,
    }

    public enum SuspendMethod {
      Suspend,
      Hibernate,
    }

    public enum ExitMethod {
      LogOff,
      Shutdown,
      Reboot,
      PowerOff,
    }

    private static bool IsRunningOnNT {
      get { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
    }

    public static ExitMethod ToExitMethod(HaltMethod method)
    {
      switch (method) {
        case HaltMethod.LogOff:   return ExitMethod.LogOff;
        case HaltMethod.Shutdown: return ExitMethod.Shutdown;
        case HaltMethod.Reboot:   return ExitMethod.Reboot;
        case HaltMethod.PowerOff: return ExitMethod.PowerOff;
        default: throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("method", method);
      }
    }

    public static SuspendMethod ToSuspendMethod(HaltMethod method)
    {
      switch (method) {
        case HaltMethod.Suspend:    return SuspendMethod.Suspend;
        case HaltMethod.Hibernate:  return SuspendMethod.Hibernate;
        default: throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("method", method);
      }
    }

    public static void HaltSession(HaltMethod method)
    {
      HaltSession(method, false);
    }

    public static void HaltSession(HaltMethod method, bool force)
    {
      switch (method) {
        case HaltMethod.LogOff:   Exit(ExitMethod.LogOff,   force); break;
        case HaltMethod.Shutdown: Exit(ExitMethod.Shutdown, force); break;
        case HaltMethod.Reboot:   Exit(ExitMethod.Reboot,   force); break;
        case HaltMethod.PowerOff: Exit(ExitMethod.PowerOff, force); break;

        case HaltMethod.Lock: Lock(); break;

        case HaltMethod.Suspend:    Suspend(SuspendMethod.Suspend,   force); break;
        case HaltMethod.Hibernate:  Suspend(SuspendMethod.Hibernate, force); break;

        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(method);
      }
    }

    public static void Shutdown()
    {
      Exit(ExitMethod.Shutdown);
    }

    public static void Shutdown(bool force)
    {
      Exit(ExitMethod.Shutdown, force);
    }

    public static void ShutdownImmediately()
    {
      ExitImmediately(ExitMethod.Shutdown);
    }

    public static void Reboot()
    {
      Exit(ExitMethod.Reboot);
    }

    public static void Reboot(bool force)
    {
      Exit(ExitMethod.Reboot, force);
    }

    public static void RebootImmediately()
    {
      ExitImmediately(ExitMethod.Reboot);
    }

    public static void PowerOff()
    {
      Exit(ExitMethod.PowerOff);
    }

    public static void PowerOff(bool force)
    {
      Exit(ExitMethod.PowerOff, force);
    }

    public static void PowerOffImmediately()
    {
      ExitImmediately(ExitMethod.PowerOff);
    }

    public static void LogOff()
    {
      Exit(ExitMethod.LogOff);
    }

    public static void LogOff(bool force)
    {
      Exit(ExitMethod.LogOff, force);
    }

    public static void LogOffImmediately()
    {
      ExitImmediately(ExitMethod.LogOff);
    }

    public static void Exit(ExitMethod method)
    {
      Exit(method, false);
    }

    public static void Exit(ExitMethod method, bool forceIfHung)
    {
      ExitWindows(method, forceIfHung ? EWX.FORCEIFHUNG : (EWX)0);
    }

    public static void ExitImmediately(ExitMethod method)
    {
      ExitWindows(method, EWX.FORCE);
    }

    public static void Lock()
    {
      if (!user32.LockWorkStation())
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void Suspend()
    {
      Suspend(false);
    }

    public static void Suspend(bool force)
    {
      Suspend(SuspendMethod.Suspend, force);
    }

    public static void Hibernate()
    {
      Hibernate(false);
    }

    public static void Hibernate(bool force)
    {
      Suspend(SuspendMethod.Hibernate, force);
    }

    public static void Suspend(SuspendMethod method)
    {
      Suspend(method, false);
    }

    public static void Suspend(SuspendMethod method, bool force)
    {
      SetSystemPowerState(method, force);
    }

    private static void ExitWindows(ExitMethod method, EWX flags)
    {
      switch (method) {
        case ExitMethod.LogOff:   flags |= EWX.LOGOFF; break;
        case ExitMethod.Shutdown: flags |= EWX.SHUTDOWN; break;
        case ExitMethod.Reboot:   flags |= EWX.REBOOT; break;
        case ExitMethod.PowerOff: flags |= EWX.POWEROFF; break;
        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(method);
      }

      if (IsRunningOnNT && method != ExitMethod.LogOff)
        SetShutdownPrivilege();
      else if (Environment.OSVersion.Platform == PlatformID.Win32Windows && (int)(flags & EWX.FORCE) != 0)
        // win9x
        KillExplorer();

      if (!user32.ExitWindowsEx(flags, 0))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private static void SetSystemPowerState(SuspendMethod method, bool force)
    {
      bool suspend;

      switch (method) {
        case SuspendMethod.Suspend:   suspend = true;  break;
        case SuspendMethod.Hibernate: suspend = false; break;
        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(method);
      }

      if (IsRunningOnNT)
        SetShutdownPrivilege();

      if (!kernel32.SetSystemPowerState(suspend, force))
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private static void SetShutdownPrivilege()
    {
      var hToken = IntPtr.Zero;

      try {
        if (!advapi32.OpenProcessToken(Process.GetCurrentProcess().Handle, Consts.TOKEN_QUERY | Consts.TOKEN_ADJUST_PRIVILEGES, ref hToken))
          throw new Win32Exception(Marshal.GetLastWin32Error(), "OpenProcessToken failed");

        var luid = new LUID();

        if (!advapi32.LookupPrivilegeValue(null, Consts.SE_SHUTDOWN_NAME, ref luid))
          throw new Win32Exception(Marshal.GetLastWin32Error(), "LookupPrivilegeValue failed");

        var privs = new TOKEN_PRIVILEGES();

        privs.PrivilegeCount = 1;
        privs.Privilege = new LUID_AND_ATTRIBUTES();
        privs.Privilege.Luid = luid;
        privs.Privilege.Attributes = Consts.SE_PRIVILEGE_ENABLED;

        if (!advapi32.AdjustTokenPrivileges(hToken, false, ref privs, 0, IntPtr.Zero, IntPtr.Zero))
          throw new Win32Exception(Marshal.GetLastWin32Error(), "AdjustTokenPrivileges failed");
      }
      finally {
        if (hToken != IntPtr.Zero) {
          Smdn.Interop.kernel32.CloseHandle(hToken);
          hToken = IntPtr.Zero;
        }
      }
    }

    private static void KillExplorer()
    {
      foreach (var explorerProcess in Process.GetProcessesByName("Explorer")) {
        explorerProcess.Kill();
      }
    }
  }
}
