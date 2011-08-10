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
  /*
   * http://pasofaq.jp/windows/admintools/cpllistvista.htm
   * http://www.ontechnews.com/computer-tips/shortcuts/useful-run-commands-for-windows-vista-and-xp/
   */
  public static class ControlPanels {
    public enum Panel {
      ControlPanel,
      FolderOptions,
      Keyboard,
      Mouse,
      MousePointer,
      MousePointerOptions,
      MouseWheel,
      MouseHardware,
      DisplaySettings,
      DisplaySettingsScreenSaver,
      DisplaySettingsOptions,
      ColorManagement,
      AppearanceSettings,
      DateAndTime,
      RegionalAndLanguageOptions,
      AdministrativeTools,
      Fonts,
      PrintersAndFaxes,
      NetworkConnections,
      UserAccounts,
      TaskScheduler,
      ScannersAndCameras,
      System,
      SystemComputerName,
      SystemHardware,
      SystemAdvanced,
      SystemSystemRestore,
      SystemAutomaticUpdates,
      SystemRemote,
      AudioDevicesAndSoundThemes,
      Firewall,
      GameControllers,
      ISCSIInitiator,
      PenAndInputDevices,
      PowerOptions,
      InternetOptions,
      WindowsSecurityCenter,
      TextToSpeech,
      AddHardware,
      AddOrRemovePrograms,
      AddOrRemoveProgramsPrograms,
      AddOrRemoveProgramsWindowsComponents,
      AddOrRemoveProgramsSetProgramAccessAndDefaults,
      WindowsUpdate,
      NetworkSetupWizard,
      Accessibility,
      DirectX,
    }

    private static Dictionary<Panel, string> controlPanelArgs = new Dictionary<Panel, string>() {
      {Panel.ControlPanel,                            string.Empty},

      {Panel.FolderOptions,                           "folders"},
      {Panel.Keyboard,                                "keyboard"},
      {Panel.Mouse,                                   "mouse"},
      {Panel.DisplaySettings,                         "desktop"},
      {Panel.ColorManagement,                         "colorcpl"},
      {Panel.AppearanceSettings,                      "color"},
      {Panel.DateAndTime,                             "date/time"},
      {Panel.RegionalAndLanguageOptions,              "international"},
      {Panel.AdministrativeTools,                     "admintools"},
      {Panel.Fonts,                                   "fonts"},
      {Panel.PrintersAndFaxes,                        "printers"},
      {Panel.NetworkConnections,                      "netconnections"},
      {Panel.UserAccounts,                            "userpasswords"},
      {Panel.TaskScheduler,                           "schedtasks"},
      {Panel.ScannersAndCameras,                      "scannercamera"},
      {Panel.TextToSpeech,                            "speech"},
    };

    private static Dictionary<Panel, string> controlPanels = new Dictionary<Panel, string>() {
      {Panel.Mouse,                                   "main.cpl"},
      {Panel.MousePointer,                            "main.cpl,,1"},
      {Panel.MousePointerOptions,                     "main.cpl,,2"},
      {Panel.MouseWheel,                              "main.cpl,,3"},
      {Panel.MouseHardware,                           "main.cpl,,4"},

      {Panel.DisplaySettings,                         "desk.cpl"},
      {Panel.DisplaySettingsScreenSaver,              "desk.cpl,,1"},
      {Panel.AppearanceSettings,                      "desk.cpl,,2"},
      {Panel.DisplaySettingsOptions,                  "desk.cpl,,3"},

      {Panel.System,                                  "sysdm.cpl"},
      {Panel.SystemComputerName,                      "sysdm.cpl,System,1"},
      {Panel.SystemHardware,                          "sysdm.cpl,System,2"},
      {Panel.SystemAdvanced,                          "sysdm.cpl,System,3"},
      {Panel.SystemSystemRestore,                     "sysdm.cpl,System,4"},
      {Panel.SystemAutomaticUpdates,                  "sysdm.cpl,System,5"},
      {Panel.SystemRemote,                            "sysdm.cpl,System,6"},

      {Panel.Accessibility,                           "access.cpl"},

      {Panel.AddOrRemovePrograms,                     "appwiz.cpl"},
      {Panel.AddOrRemoveProgramsPrograms,             "appwiz.cpl,,1"},
      {Panel.AddOrRemoveProgramsWindowsComponents,    "appwiz.cpl,,2"},
      {Panel.AddOrRemoveProgramsSetProgramAccessAndDefaults, "appwiz.cpl,,3"},

      {Panel.AddHardware,                             "hdwwiz.cpl"},

      {Panel.NetworkSetupWizard,                      "netsetup.cpl"},

      {Panel.Firewall,                                "firewall.cpl"},

      {Panel.InternetOptions,                         "inetcpl.cpl"},

      {Panel.GameControllers,                         "joy.cpl"},

      {Panel.ISCSIInitiator,                          "iscsicpl.cpl"},

      {Panel.PenAndInputDevices,                      "tabletpc.cpl"},

      {Panel.AudioDevicesAndSoundThemes,              "mmsys.cpl"},

      {Panel.WindowsSecurityCenter,                   "wscui.cpl"},

      {Panel.WindowsUpdate,                           "wuaucpl.cpl"},

      {Panel.PowerOptions,                            "powercfg.cpl"},

      {Panel.UserAccounts,                            "nusrmgr.cpl"},

      {Panel.DirectX,                                 "directx.cpl"},
    };

    public static void Open()
    {
      Open(Panel.ControlPanel);
    }

    public static void Open(Panel panel)
    {
      using (OpenProcess(panel)) {}
    }

    public static Process OpenProcess()
    {
      return OpenProcess(Panel.ControlPanel);
    }

    public static Process OpenProcess(Panel panel)
    {
      string arg;

      if (!controlPanelArgs.TryGetValue(panel, out arg))
        if (!controlPanels.TryGetValue(panel, out arg))
          throw ExceptionUtils.CreateNotSupportedEnumValue(panel);

      return OpenProcess(arg);
    }

    internal static Process OpenProcess(string args)
    {
      var psi = new ProcessStartInfo("control", args);

      return Process.Start(psi);
    }

    public static string GetLocalizedString(Panel panel)
    {
      // TODO: impl
      return panel.ToString();
    }
  }
}
