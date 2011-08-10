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
using System.IO;

namespace Smdn.Windows.UserInterfaces.Shells {
  public abstract class SpecialFolderEntryInfo {
    public SpecialFolderInfo Root {
      get
      {
        if (this is SpecialFolderInfo && Parent == null)
          return this as SpecialFolderInfo;
        else
          return Parent.Root;
      }
    }

    public SpecialFolderInfo Parent {
      get; private set;
    }

    protected FileSystemInfo FileSystemInfo {
      get; private set;
    }

    public string FullName {
      get { return FileSystemInfo.FullName; }
    }

    public virtual string HierarchyPath {
      get
      {
        if (Parent == null)
          return Name;
        else
          return Path.Combine(Parent.HierarchyPath, Name);
      }
    }

    public virtual string Name {
      get { return name; }
      internal protected set
      {
        if (value == null)
          throw new ArgumentNullException("value");
        else if (value.Length == 0)
          throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("Name");

        name = value;
      }
    }

    internal protected SpecialFolderEntryInfo(SpecialFolderInfo parent, FileSystemInfo fileSystemInfo)
      : this(parent, fileSystemInfo, fileSystemInfo.Name)
    {
    }

    internal protected SpecialFolderEntryInfo(SpecialFolderInfo parent, FileSystemInfo fileSystemInfo, string name)
    {
      if (fileSystemInfo == null)
        throw new ArgumentNullException("fileSystemInfo");

      this.Parent = parent;
      this.FileSystemInfo = fileSystemInfo;
      this.Name = name;
    }

    public override string ToString()
    {
      return string.Format("{{Name={0}, FullName={1}}}", name, this.FileSystemInfo.FullName);
    }

    private string name;
  }
}
