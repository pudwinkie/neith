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
using System.Drawing;
using System.IO;
using System.Text;

using Smdn.Formats.Ini;
using Smdn.Interop;
using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.UserInterfaces.Shells {
  /*
   * http://msdn.microsoft.com/en-us/library/cc144102%28VS.85%29.aspx
   * http://msdn.microsoft.com/en-us/library/ms920369.aspx
   */
  public sealed class DesktopIni {
    /*
     * class members
     */
    public const string FileName = @"Desktop.ini";

    public static DesktopIni Load(string fileOrDirectory)
    {
      try {
        return new DesktopIni(fileOrDirectory);
      }
      catch (FileNotFoundException) {
        return null;
      }
    }

    /*
     * instance members
     */
    [System.Runtime.CompilerServices.IndexerName("Sections")]
    public IniSection this[string name] {
      get { return ini[name]; }
    }

    /// <summary>[.ShellClassInfoSection] section</summary>
    public IniSection ShellClassInfoSection {
      get { return shellClassInfoSection; }
    }

    /// <summary>If you want to specify a custom icon for the folder, set this entry to the icon's file name. The .ico file extension is preferred, but it is also possible to specify .bmp files, or .exe and .dll files that contain icons. If you use a relative path, the icon is available to people who view the folder over the network. You must also set the IconIndex entry.</summary>
    /// <value>A value of IconFile entry if specified, otherwise null.</value>
    public string IconFile {
      get
      {
        if (shellClassInfoSection.Entries.ContainsKey("IconFile"))
          return Environment.ExpandEnvironmentVariables(shellClassInfoSection["IconFile"]);
        else
          return null;
      }
    }

    /// <summary>Set this entry to specify the index for a custom icon. If the file assigned to IconFile only contains a single icon, set IconIndex to 0.</summary>
    /// <value>A value of IconIndex entry if specified, otherwise 0.</value>
    public int IconIndex {
      get { return shellClassInfoSection.Get<int>("IconIndex", 0, Convert.ToInt32) & 0x0000ffff; } // XXX
    }

    public IconLocation IconLocation {
      get { return new IconLocation(IconFile, IconIndex); }
    }

    /* Vista or over? */
    public IconLocation IconResource {
      get
      {
        var iconResource = shellClassInfoSection.Get("IconResource");

        if (iconResource == null)
          return IconLocation.Empty;

        var delim = iconResource.LastIndexOf(',');
        var file = (0 <= delim) ? iconResource.Substring(0, delim) : iconResource;
        var index = (0 <= delim) ? int.Parse(iconResource.Substring(delim + 1)) : 0;

        return new IconLocation(Environment.ExpandEnvironmentVariables(file), index);
      }
    }
    /// <summary>Set this entry to an informational text string. It is displayed as an infotip when the cursor hovers over the folder. If the user clicks the folder, the information text is displayed in the folder's information block, below the standard information.</summary>
    /// <value>value of InfoTip entry if specified, otherwise null.</value>
    public string InfoTip {
      get { return shellClassInfoSection["InfoTip"]; }
    }

    /// <value>value of LocalizedResourceName</value>
    public string LocalizedString {
      get
      {
        var localizedResourceString = shellClassInfoSection["LocalizedResourceName"];

        if (localizedResourceString == null)
          return null;
        else
          return LoadResourceString(localizedResourceString);
      }
    }

    public DesktopIni(string fileOrDirectoryPath)
    {
      if (fileOrDirectoryPath == null)
        throw new ArgumentNullException("fileOrDirectoryPath");

      if (Directory.Exists(fileOrDirectoryPath))
        fileOrDirectoryPath = Path.Combine(fileOrDirectoryPath, FileName);

      if (File.Exists(fileOrDirectoryPath))
        ini = IniDocument.Load(fileOrDirectoryPath);
      else
        throw new FileNotFoundException("not found", fileOrDirectoryPath);

      shellClassInfoSection     = ini[".ShellClassInfo"];
      localizedFileNamesSection = ini["LocalizedFileNames"];
    }

    public string GetLocalizedFileName(string file)
    {
      if (localizedFileNamesSection.Entries.ContainsKey(file))
        return LoadResourceString(localizedFileNamesSection[file]);
      else
        return file;
    }

    private static string LoadResourceString(string entryValue)
    {
      if (!entryValue.StartsWith("@"))
        return entryValue;

      var moduleNameAndResourceId = entryValue.Substring(1);

      var delim = moduleNameAndResourceId.IndexOf(',');

      if (delim < 0)
        return entryValue;

      var moduleName = moduleNameAndResourceId.Substring(0, delim);
      int resourceId;

      if (!int.TryParse(moduleNameAndResourceId.Substring(delim + 1), out resourceId))
        // invalid format
        return entryValue;

      return Resource.LoadString(Environment.ExpandEnvironmentVariables(moduleName), resourceId);
    }

    private IniDocument ini;
    private IniSection shellClassInfoSection;
    private IniSection localizedFileNamesSection;
  }
}