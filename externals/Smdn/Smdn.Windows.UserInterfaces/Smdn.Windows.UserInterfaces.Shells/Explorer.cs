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
   * http://pasofaq.jp/windows/mycomputer/folderclsid.htm
   */
  public static class Explorer {
    public enum SpecialFolder {
      MyComputer,
      MyNetwork,
      MyDocument,
      RecycleBin,
      ControlPanel,
      Fonts,
      NetworkConnections,
    }

    private static Dictionary<SpecialFolder, string> specialFolderGuids = new Dictionary<SpecialFolder, string> (){
      {SpecialFolder.MyComputer,            @"::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"},
      {SpecialFolder.MyNetwork,             @"::{208D2C60-3AEA-1069-A2D7-08002B30309D}"},
      {SpecialFolder.MyDocument,            @"::{450D8FBA-AD25-11D0-98A8-0800361B1103}"},
      {SpecialFolder.RecycleBin,            @"::{645FF040-5081-101B-9F08-00AA002F954E}"},
      {SpecialFolder.ControlPanel,          @"::{21EC2020-3AEA-1069-A2DD-08002B30309D}"},
      {SpecialFolder.Fonts,                 @"::{20D04FE0-3AEA-1069-A2D8-08002B30309D}\::{21EC2020-3AEA-1069-A2DD-08002B30309D}\::{D20EA4E1-3957-11d2-A40B-0C5020524152}"},
      {SpecialFolder.NetworkConnections,    @"::{20D04FE0-3AEA-1069-A2D8-08002B30309D}\::{21EC2020-3AEA-1069-A2DD-08002B30309D}\::{7007ACC7-3202-11D1-AAD2-00805FC1270E}"},
    };

    public static void Browse(SpecialFolder folder)
    {
      using (OpenBrowsingProcess(folder)) {}
    }

    public static Process OpenBrowsingProcess(SpecialFolder folder)
    {
      string guid;

      if (!specialFolderGuids.TryGetValue(folder, out guid))
        throw new NotSupportedException(string.Format("special folder '{0}' is not unsupported", folder));

      return OpenBrowsingProcess(guid);
    }

    public static void Browse(Environment.SpecialFolder folder)
    {
      using (OpenBrowsingProcess(folder)) {}
    }

    public static Process OpenBrowsingProcess(Environment.SpecialFolder folder)
    {
      return OpenBrowsingProcess(Environment.GetFolderPath(folder));
    }

    public static void Browse(string path)
    {
      using (OpenBrowsingProcess(path)) {}
    }

    public static Process OpenBrowsingProcess(string path)
    {
      if (path == null)
        throw new ArgumentNullException("path");

      return OpenBrowsingProcessCore(string.Format("/n,{0}", path));
    }

    internal static Process OpenBrowsingProcessCore(string args)
    {
      var psi = new ProcessStartInfo("explorer.exe", args);

      return Process.Start(psi);
    }

    public static string GetLocalizedString(SpecialFolder folder)
    {
      // TODO: impl
      return folder.ToString();
    }
  }
}
