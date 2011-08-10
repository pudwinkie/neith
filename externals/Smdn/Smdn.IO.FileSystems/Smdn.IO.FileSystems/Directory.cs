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
  public abstract class Directory : DirectoryEntry, IDirectory {
    public static readonly ScanDirectoryExceptionHandler IgnoreAnyExceptionHandler = IgnoreAnyException;

    private static void IgnoreAnyException(Directory directory, Exception exception)
    {
      // do nothing
    }

    public static readonly ScanDirectoryExceptionHandler IgnoreUnauthorizedAccessExceptionHandler = IgnoreUnauthorizedAccessException;

    private static void IgnoreUnauthorizedAccessException(Directory directory, Exception exception)
    {
      if (!(exception is UnauthorizedAccessException))
        throw exception;
    }

    public virtual bool IsRoot {
      get { return Parent == null; }
    }

    protected Directory()
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

    public abstract IEnumerable<DirectoryEntry> GetEntries(ScanDirectoryExceptionHandler exceptionHandler, Predicate<DirectoryEntry> match);

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

    public abstract IEnumerable<File> GetFiles(ScanDirectoryExceptionHandler exceptionHandler, Predicate<File> match);

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

    public abstract IEnumerable<Directory> GetDirectories(ScanDirectoryExceptionHandler exceptionHandler, Predicate<Directory> match);

    public void Traverse(Predicate<DirectoryEntry> continuation)
    {
      Traverse(null, continuation, null);
    }

    public void Traverse(Predicate<DirectoryEntry> continuation, Predicate<DirectoryEntry> match)
    {
      Traverse(null, continuation, match);
    }

    public void Traverse(ScanDirectoryExceptionHandler exceptionHandler, Predicate<DirectoryEntry> continuation)
    {
      Traverse(exceptionHandler, continuation, null);
    }

    public void Traverse(ScanDirectoryExceptionHandler exceptionHandler, Predicate<DirectoryEntry> continuation, Predicate<DirectoryEntry> match)
    {
      if (continuation == null)
        throw new ArgumentNullException("continuation");

      foreach (var entry in GetEntries(exceptionHandler, match)) {
        if (!continuation(entry))
          continue;

        if (entry is Directory)
          (entry as Directory).Traverse(exceptionHandler, continuation, match);
      }
    }

    public override string ToString()
    {
      throw new NotImplementedException();
    }
  }
}
