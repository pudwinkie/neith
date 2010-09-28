// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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

namespace Smdn.Net.Imap4 {
  /*
   * RFC 5464 - The IMAP METADATA Extension
   * http://tools.ietf.org/html/rfc5464
   */
  public class ImapMetadata {
    public const string Separator = "/";
    public const char SeparatorChar = '/';
    private readonly char[] invalidEntryNameChars = new[] {'*', '%', SeparatorChar};

    public string[] HierarchicalEntryName {
      get; private set;
    }

    public string EntryName {
      get { return JoinEntryName(HierarchicalEntryName); }
    }

    public ImapString Value {
      get; private set;
    }

    public bool IsShared {
      get { return string.Equals(HierarchicalEntryName[0], "shared", StringComparison.OrdinalIgnoreCase); }
    }

    public bool IsPrivate {
      get { return string.Equals(HierarchicalEntryName[0], "private", StringComparison.OrdinalIgnoreCase); }
    }

    /// <summary>The <see cref="Value"/> property will be set to null to indicate that the entry is to be removed.</summary>
    public static ImapMetadata CreateNil(string entryName)
    {
      return new ImapMetadata(SplitEntryName(entryName));
    }

    /// <summary>The <see cref="Value"/> property will be set to null to indicate that the entry is to be removed.</summary>
    public static ImapMetadata CreateNil(string hierarchicalEntryName, params string[] hierarchicalEntryNames)
    {
      return new ImapMetadata(hierarchicalEntryNames.Prepend(hierarchicalEntryName));
    }

    /// <param name="value">The value can be null to indicate that the entry is to be removed.</param>
    public static ImapMetadata CreateSharedVendorMetadata(ImapString @value, string hierarchicalEntryName, params string[] hierarchicalEntryNames)
    {
      var entryName = new List<string>(new[] {"shared", "vendor"});

      entryName.Add(hierarchicalEntryName);
      entryName.AddRange(hierarchicalEntryNames);

      if (@value == null)
        return new ImapMetadata(entryName.ToArray());
      else
        return new ImapMetadata(entryName.ToArray(), @value);
    }

    /// <param name="value">The value can be null to indicate that the entry is to be removed.</param>
    public static ImapMetadata CreatePrivateVendorMetadata(ImapString @value, string hierarchicalEntryName, params string[] hierarchicalEntryNames)
    {
      var entryName = new List<string>(new[] {"private", "vendor"});

      entryName.Add(hierarchicalEntryName);
      entryName.AddRange(hierarchicalEntryNames);

      if (@value == null)
        return new ImapMetadata(entryName.ToArray());
      else
        return new ImapMetadata(entryName.ToArray(), @value);
    }

    public ImapMetadata(string entryName, ImapString @value)
      : this(SplitEntryName(entryName), @value)
    {
    }

    public ImapMetadata(string[] hierarchicalEntryName, ImapString @value)
      : this(hierarchicalEntryName)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      this.Value = @value;
    }

    private ImapMetadata(string[] hierarchicalEntryName)
    {
      if (hierarchicalEntryName == null)
        throw new ArgumentNullException("hierarchicalEntryName");
      else if (hierarchicalEntryName.Length < 2)
        throw new ArgumentException("hierarchicalEntryName must have at least 2 components", "hierarchicalEntryName");

      foreach (var hierarchy in hierarchicalEntryName) {
        if (hierarchy == null)
          throw new ArgumentException("contains null", "hierarchicalEntryName");
        else if (0 <= hierarchy.IndexOfAny(invalidEntryNameChars))
          throw new ArgumentException("can't contain characters of '*', '%', '/'", "hierarchicalEntryName");
      }

      this.HierarchicalEntryName = hierarchicalEntryName;
      this.Value = null;
    }

    public static string[] SplitEntryName(string entryName)
    {
      if (entryName == null)
        throw new ArgumentNullException("entryName");
      else if (!entryName.StartsWith(Separator, StringComparison.Ordinal))
        throw new ArgumentException("must start with '/'", "entryName");

      return entryName.Substring(1).Split(SeparatorChar);
    }

    public static string JoinEntryName(string hierarchicalEntryName, params string[] hierarchicalEntryNames)
    {
      if (hierarchicalEntryNames == null)
        throw new ArgumentNullException("hierarchicalEntryNames");
      else if (hierarchicalEntryName.Length < 1)
        throw new ArgumentException("hierarchicalEntryName must have at least 2 components", "hierarchicalEntryName");

      return Separator + hierarchicalEntryName + Separator + string.Join(Separator, hierarchicalEntryNames);
    }

    private static string JoinEntryName(string[] hierarchicalEntryName)
    {
      return Separator + string.Join(Separator, hierarchicalEntryName);
    }
  }
}
