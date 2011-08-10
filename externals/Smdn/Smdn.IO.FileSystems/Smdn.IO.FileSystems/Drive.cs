// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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

namespace Smdn.IO.FileSystems {
  public abstract class Drive : IDirectory {
    public abstract string Name { get; }
    public abstract System.IO.DriveType Type { get; }
    public abstract bool IsReady { get; }
    public abstract Directory RootDirectory { get; }
    public abstract string Format { get; }
    public abstract object RawInfo { get; }

    protected Drive()
    {
    }

    public virtual void Refresh()
    {
    }

    public IEnumerable<DirectoryEntry> GetEntries()
    {
      return GetEntries(null, null);
    }

    public IEnumerable<DirectoryEntry> GetEntries(Predicate<DirectoryEntry> match)
    {
      return GetEntries(null, match);
    }

    public IEnumerable<DirectoryEntry> GetEntries(ScanDirectoryExceptionHandler exceptionHandler)
    {
      return GetEntries(exceptionHandler, null);
    }

    public virtual IEnumerable<DirectoryEntry> GetEntries(ScanDirectoryExceptionHandler exceptionHandler, Predicate<DirectoryEntry> match)
    {
      if (RootDirectory == null)
        return new DirectoryEntry[] {};
      else
        return RootDirectory.GetEntries(exceptionHandler, match);
    }

    public IEnumerable<File> GetFiles()
    {
      return GetFiles(null, null);
    }

    public IEnumerable<File> GetFiles(Predicate<File> match)
    {
      return GetFiles(null, match);
    }

    public IEnumerable<File> GetFiles(ScanDirectoryExceptionHandler exceptionHandler)
    {
      return GetFiles(exceptionHandler, null);
    }
    
    public virtual IEnumerable<File> GetFiles(ScanDirectoryExceptionHandler exceptionHandler, Predicate<File> match)
    {
      if (RootDirectory == null)
        return new File[] {};
      else
        return RootDirectory.GetFiles(exceptionHandler, match);
    }

    public IEnumerable<Directory> GetDirectories()
    {
      return GetDirectories(null, null);
    }

    public IEnumerable<Directory> GetFiles(Predicate<Directory> match)
    {
      return GetDirectories(null, match);
    }

    public IEnumerable<Directory> GetDirectories(ScanDirectoryExceptionHandler exceptionHandler)
    {
      return GetDirectories(exceptionHandler, null);
    }

    public virtual IEnumerable<Directory> GetDirectories(ScanDirectoryExceptionHandler exceptionHandler, Predicate<Directory> match)
    {
      if (RootDirectory == null)
        return new Directory[] {};
      else
        return RootDirectory.GetDirectories(exceptionHandler, match);
    }
  }
}
