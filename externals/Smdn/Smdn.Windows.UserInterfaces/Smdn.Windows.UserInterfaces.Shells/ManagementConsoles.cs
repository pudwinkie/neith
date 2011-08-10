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
using System.Diagnostics;

namespace Smdn.Windows.UserInterfaces.Shells {
  public static class ManagementConsoles {
    public enum Console {
      CertificateManager,
      ComputerManagement,
      DeviceManager,
      DiskManagement,
      EventViewer,
      Services,
      SharedFolders,
      WindowsManagementInfrastructure,
      /*
       * only in Windows Vista
       */
      AuthorizationManager,
      WindowsFirewallWithAdvancedSecurity,
      /*
       * only in Windows XP
       */
      GroupPolicyEditor,
      LocalSecuritySettings,
      LocalUsersAndGroups,
      ResultantSetOfPolicy,
    }

    private static Dictionary<Console, string> consoles = new Dictionary<Console, string>() {
      {Console.CertificateManager,                         "certmgr.msc"},
      {Console.ComputerManagement,                         "compmgmt.msc"},
      {Console.DeviceManager,                              "devmgmt.msc"},
      {Console.DiskManagement,                             "diskmgmt.msc"},
      {Console.EventViewer,                                "eventvwr.msc"},
      {Console.Services,                                   "services.msc"},
      {Console.SharedFolders,                              "fsmgmt.msc"},
      {Console.WindowsManagementInfrastructure,            "wmimgmt.msc"},
      /*
       * only in Windows Vista
       */
      {Console.AuthorizationManager,                       "azman.msc"},
      {Console.WindowsFirewallWithAdvancedSecurity,        "wf.msc"},
      /*
       * only in Windows XP
       */
      {Console.GroupPolicyEditor,                          "gpedit.msc"},
      {Console.LocalSecuritySettings,                      "secpol.msc"},
      {Console.LocalUsersAndGroups,                        "lusrmgr.msc"},
      {Console.ResultantSetOfPolicy,                       "rsop.msc"},
    };

    public static void Open(Console console)
    {
      using (OpenProcess(console)) {}
    }

    public static Process OpenProcess(Console console)
    {
      string executable;

      if (!consoles.TryGetValue(console, out executable))
        throw ExceptionUtils.CreateNotSupportedEnumValue(console);

      return OpenProcess(executable);
    }

    internal static Process OpenProcess(string executable)
    {
      var psi = new ProcessStartInfo(executable);

      return Process.Start(psi);
    }

    public static string GetLocalizedString(Console console)
    {
      // TODO: impl
      return console.ToString();
    }
  }
}