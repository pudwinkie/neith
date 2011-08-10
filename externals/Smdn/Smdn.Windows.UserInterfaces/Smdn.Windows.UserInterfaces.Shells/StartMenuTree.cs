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
using System.Drawing;

namespace Smdn.Windows.UserInterfaces.Shells {
  public sealed class StartMenuTree {
    /*
     * class members
     */
    public static StartMenuTree Create()
    {
      if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        return new StartMenuTree(null, new CurrentUserStartMenuFolderInfo(), new AllUsersStartMenuFolderInfo());
      else
        return new StartMenuTree(null, new CurrentUserStartMenuFolderInfo(), null);
    }

    /*
     * instance members
     */
    public StartMenuTree Parent {
      get; private set;
    }

    public string Name {
      get { return FolderInfo.Name; }
    }

    public string HierarcyPath {
      get { return FolderInfo.HierarchyPath; }
    }

    public StartMenuFolderInfo CurrentUserFolderInfo {
      get { return (0 < currentUserFolderInfoes.Count) ? currentUserFolderInfoes[0] : null; }
    }

    public StartMenuFolderInfo AllUsersFolderInfo {
      get { return (0 < allUsersFolderInfoes.Count) ? allUsersFolderInfoes[0] : null; }
    }

    public StartMenuFolderInfo FolderInfo {
      get { return CurrentUserFolderInfo ?? AllUsersFolderInfo; }
    }

    private StartMenuTree(StartMenuTree parent, StartMenuFolderInfo currentUserFolderInfo, StartMenuFolderInfo allUsersFolderInfo)
    {
      if (currentUserFolderInfo == null && allUsersFolderInfo == null)
        throw new ArgumentNullException("both of currentUserFolderInfo and allUsersFolderInfo can not be null");

      this.Parent = parent;

      if (currentUserFolderInfo != null)
        MergeCurrentUserFolder(currentUserFolderInfo);
      if (allUsersFolderInfo != null)
        MergeAllUsersFolder(allUsersFolderInfo);
    }

    private void MergeCurrentUserFolder(StartMenuFolderInfo folder)
    {
      currentUserFolderInfoes.Add(folder);
    }

    private void MergeAllUsersFolder(StartMenuFolderInfo folder)
    {
      allUsersFolderInfoes.Add(folder);
    }

    public IEnumerable<SpecialFolderFileInfo> GetEntries()
    {
      foreach (var folder in currentUserFolderInfoes) {
        foreach (var file in folder.GetFiles()) {
          yield return file;
        }
      }

      foreach (var folder in allUsersFolderInfoes) {
        foreach (var file in folder.GetFiles()) {
          yield return file;
        }
      }
    }

    public IEnumerable<StartMenuTree> GetSubTrees()
    {
      var subTrees = new Dictionary<string, StartMenuTree>();

      foreach (var currentUserFolder in currentUserFolderInfoes) {
        foreach (StartMenuFolderInfo subDir in currentUserFolder.GetDirectories()) {
          if (subTrees.ContainsKey(subDir.Name)) {
            subTrees[subDir.Name].MergeCurrentUserFolder(subDir);
          }
          else {
            var subTree = new StartMenuTree(this, subDir, null);

            subTrees.Add(subTree.Name, subTree);
          }
        }
      }

      foreach (var allUsersFolder in allUsersFolderInfoes) {
        foreach (StartMenuFolderInfo subDir in allUsersFolder.GetDirectories()) {
          if (subTrees.ContainsKey(subDir.Name)) {
            subTrees[subDir.Name].MergeCurrentUserFolder(subDir);
          }
          else {
            var subTree = new StartMenuTree(this, null, subDir);

            subTrees.Add(subTree.Name, subTree);
          }
        }
      }

      return subTrees.Values;
    }

    public Bitmap GetIcon()
    {
      return FolderInfo.GetIcon();
    }

    private List<StartMenuFolderInfo> currentUserFolderInfoes = new List<StartMenuFolderInfo>();
    private List<StartMenuFolderInfo> allUsersFolderInfoes = new List<StartMenuFolderInfo>();
  }
}
