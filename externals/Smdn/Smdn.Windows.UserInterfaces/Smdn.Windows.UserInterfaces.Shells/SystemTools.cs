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
   * http://www.ontechnews.com/computer-tips/shortcuts/useful-run-commands-for-windows-vista-and-xp/
   */
  public static class SystemTools {
    public enum Tool {
      Calculator,
      CharacterMap,
      CommandPrompt,
      ComponentServices,
      ControlPanel,
      DirectXTroubleshooter,
      DiskCleanupUtility,
      DiskPartitionManager,
      DriverVerifierUtility,
      FileSignatureVerificationTool,
      FilesAndSettingsTransferTool,
      InternetExplorer,
      LogOff,
      MicrosoftPaint,
      Notepad,
      OnScreenKeyboard,
      OutlookExpress,
      Paint,
      PhoneDialer,
      PrivateCharacterEditor,
      RegistryEditor,
      RemoteDesktop,
      SharedCreationWizard,
      ShutsDownWindows,
      SQLClientConfiguration,
      SyncCenter,
      SyncronizationTool,
      SystemConfigurationEditor,
      SystemConfigurationUtility,
      SystemInformation,
      TaskManager,
      UtilityManager,
      WindowsMagnifier,
      WindowsMediaPlayer,
      WindowsPictureImportWizard,
      WindowsSystemSecurityTool,
      WindowsVersion,
      Wordpad,
      /*
       * only in Windows Vista
       */
      AdapterTroubleshooter,
      AdvancedUserAccountsControlPanel,
      BackupStatusAndUtility,
      ColorManagement,
      ComputerManagementSnapinLauncher,
      CredentialBackupAndRestoreWizard,
      DiskDefragmenter,
      DitilizerCalibrationTool,
      DPIScaling,
      DriverPackageInstaller,
      FirewallControlPanel,
      FirewallSettings,
      ISCSIInitiator,
      LanguagePackInstaller,
      MicrosoftSupportDiagnosticTool,
      ProblemReportsAndSolutions,
      RemoteAssistance,
      SoftwareLicensing,
      SoundRecorder,
      SoundVolume,
      SystemPropertiesAdvanced,
      SystemPropertiesComputerName,
      SystemPropertiesDataExecutionPrevention,
      SystemPropertiesHardware,
      SystemPropertiesPerformance,
      SystemPropertiesProtection,
      SystemPropertiesRemote,
      TrustedPlatformModuleInitializationWizard,
      WindowsFeatures,
      WindowsImageAcquisition,
      WindowsMobilityCenter,
      WindowsUpdateApplication,
      WindowsUpdateStandaloneInstaller,
      /*
       * only in Windows XP
       */
      CheckDiskUtility,
      WindowsXPTourWizard,
    }

    private static Dictionary<Tool, string> executables = new Dictionary<Tool, string>() {
      {Tool.Calculator,                                 "calc"},
      {Tool.CharacterMap,                               "charmap"},
      {Tool.CommandPrompt,                              "cmd"},
      {Tool.ComponentServices,                          "dcomcnfg"},
      {Tool.ControlPanel,                               "control"},
      {Tool.DirectXTroubleshooter,                      "dxdiag"},
      {Tool.DiskCleanupUtility,                         "cleanmgr"},
      {Tool.DiskPartitionManager,                       "diskpart"},
      {Tool.DriverVerifierUtility,                      "verifier"},
      {Tool.FileSignatureVerificationTool,              "sigverif"},
      {Tool.FilesAndSettingsTransferTool,               "migwiz"},
      {Tool.InternetExplorer,                           "iexplore"},
      {Tool.LogOff,                                     "logoff"},
      {Tool.MicrosoftPaint,                             "mspaint"},
      {Tool.Notepad,                                    "notepad"},
      {Tool.OnScreenKeyboard,                           "osk"},
      {Tool.OutlookExpress,                             "msimn"},
      {Tool.Paint,                                      "pbrush"},
      {Tool.PhoneDialer,                                "dialer"},
      {Tool.PrivateCharacterEditor,                     "eudcedit"},
      {Tool.RegistryEditor,                             "regedit"},
      {Tool.RemoteDesktop,                              "mstsc"},
      {Tool.SharedCreationWizard,                       "shrpubw"},
      {Tool.ShutsDownWindows,                           "shutdown"},
      {Tool.SQLClientConfiguration,                     "cliconfg"},
      {Tool.SyncronizationTool,                         "mobsync"},
      {Tool.SystemConfigurationEditor,                  "sysedit"},
      {Tool.SystemConfigurationUtility,                 "msconfig"},
      {Tool.SystemInformation,                          "msinfo32"},
      {Tool.TaskManager,                                "taskmgr"},
      {Tool.UtilityManager,                             "utilman"},
      {Tool.WindowsMagnifier,                           "magnify"},
      {Tool.WindowsMediaPlayer,                         "wmplayer"},
      {Tool.WindowsPictureImportWizard,                 "wiaacmgr"},
      {Tool.WindowsSystemSecurityTool,                  "syskey"},
      {Tool.WindowsVersion,                             "winver"},
      {Tool.Wordpad,                                    "write"},
      /*
       * only in Windows Vista
       */
      {Tool.AdapterTroubleshooter,                      "AdapterTroubleshooter"},
      {Tool.AdvancedUserAccountsControlPanel,           "Netplwiz"},
      {Tool.BackupStatusAndUtility,                     "sdclt"},
      {Tool.ColorManagement,                            "colorcpl"},
      {Tool.ComputerManagementSnapinLauncher,           "CompMgmtLauncher"},
      {Tool.CredentialBackupAndRestoreWizard,           "credwiz"},
      {Tool.DiskDefragmenter,                           "dfrgui"},
      {Tool.DitilizerCalibrationTool,                   "tabcal"},
      {Tool.DPIScaling,                                 "dpiscaling"},
      {Tool.DriverPackageInstaller,                     "dpinst"},
      {Tool.FirewallControlPanel,                       "FirewallControlPanel"},
      {Tool.FirewallSettings,                           "FirewallSettings"},
      {Tool.ISCSIInitiator,                             "iscsicpl"},
      {Tool.LanguagePackInstaller,                      "lpksetup"},
      {Tool.MicrosoftSupportDiagnosticTool,             "msdt"},
      {Tool.ProblemReportsAndSolutions,                 "wercon"},
      {Tool.RemoteAssistance,                           "msra"},
      {Tool.SoftwareLicensing,                          "slui"},
      {Tool.SoundRecorder,                              "soundrecorder"},
      {Tool.SoundVolume,                                "sndvol"},
      {Tool.SystemPropertiesAdvanced,                   "SystemPropertiesAdvanced"},
      {Tool.SystemPropertiesComputerName,               "SystemPropertiesComputerName"},
      {Tool.SystemPropertiesDataExecutionPrevention,    "SystemPropertiesDataExecutionPrevention"},
      {Tool.SystemPropertiesHardware,                   "SystemPropertiesHardware"},
      {Tool.SystemPropertiesPerformance,                "SystemPropertiesPerformance"},
      {Tool.SystemPropertiesProtection,                 "SystemPropertiesProtection"},
      {Tool.SystemPropertiesRemote,                     "SystemPropertiesRemote"},
      {Tool.TrustedPlatformModuleInitializationWizard,  "TpmInit"},
      {Tool.WindowsFeatures,                            "optionalfeatures"},
      {Tool.WindowsImageAcquisition,                    "wiaacmgr"},
      {Tool.WindowsMobilityCenter,                      "mblctr"},
      {Tool.WindowsUpdateApplication,                   "wuapp"},
      {Tool.WindowsUpdateStandaloneInstaller,           "wusa"},
      /*
       * only in Windows XP
       */
      {Tool.CheckDiskUtility,                           "chkdsk"},
      {Tool.WindowsXPTourWizard,                        "tourstart"},
    };

    public static void Run(Tool tool)
    {
      using (RunProcess(tool)) {}
    }

    public static Process RunProcess(Tool tool)
    {
      string executable;

      if (!executables.TryGetValue(tool, out executable))
        throw ExceptionUtils.CreateNotSupportedEnumValue(tool);

      return RunProcess(executable);
    }

    internal static Process RunProcess(string executable)
    {
      var psi = new ProcessStartInfo(executable);

      return Process.Start(psi);
    }

    public static string GetLocalizedString(Tool tool)
    {
      // TODO: impl
      return tool.ToString();
    }
  }
}