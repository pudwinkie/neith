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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Smdn.Formats.Diff {
  internal static class Parser {
    public static DiffDocument Parse(TextReader reader, DiffFormat format)
    {
      switch (format) {
        case DiffFormat.Unified: return ParseUnified(reader);
        default:
          throw new NotImplementedException(string.Format("unimplemented feature: {0}", format));
      }
    }

    private static DiffDocument ParseUnified(TextReader reader)
    {
      var document = new DiffDocument(DiffFormat.Unified);
      var differenceBuilder = new StringBuilder();
      var entryHeaderLines = new List<string>();

      for (;;) {
        var line = reader.ReadLine();

        if (line == null)
          break;

        if (line.StartsWith("--- ", StringComparison.Ordinal)) {
          var entry = ParseUnifiedEntry(reader, line);

          entry.HeaderLines = entryHeaderLines.ToArray();

          document.Entries.Add(entry);

          differenceBuilder.Append(entry.Difference);

          entryHeaderLines.Clear();
        }
        else {
          differenceBuilder.AppendLine(line);

          entryHeaderLines.Add(line);
        }
      }

      document.Difference = differenceBuilder.ToString();

      return document;
    }

    private static DiffEntry ParseUnifiedEntry(TextReader reader, string firstLine)
    {
      var entry = new DiffEntry();
      var differenceBuilder = new StringBuilder();
      var line = reader.ReadLine();

      if (line == null || !line.StartsWith("+++ ", StringComparison.Ordinal))
        throw new FormatException("missing diff-from line");

      entry.DifferenceFrom = firstLine.Substring(4).TrimStart();
      entry.DifferenceTo   = line.Substring(4).TrimStart();

      differenceBuilder.AppendLine(firstLine);
      differenceBuilder.AppendLine(line);

      line = reader.ReadLine();

      if (line == null || !line.StartsWith("@@ ", StringComparison.Ordinal))
        throw new FormatException("missing hunk line");

      foreach (var hunk in ParseUnifiedHunk(reader, line)) {
        entry.Hunks.Add(hunk);

        differenceBuilder.Append(hunk.Difference);
      }

      entry.Difference = differenceBuilder.ToString();

      return entry;
    }

    private static IEnumerable<DiffHunk> ParseUnifiedHunk(TextReader reader, string firstLine)
    {
      var hunk = new DiffHunk();
      var differenceBuilder = new StringBuilder();

      ParseUnifiedHunkRange(hunk, firstLine);

      differenceBuilder.AppendLine(firstLine);

      for (;;) {
        var ch = reader.Peek();

        if (!(ch == '+' || ch == '-' || ch == '@' || ch == ' '))
          break;

        var line = reader.ReadLine();

        if (line.StartsWith("@@ ", StringComparison.Ordinal)) {
          hunk.Difference = differenceBuilder.ToString();

          yield return hunk;

          differenceBuilder.Length = 0;
          differenceBuilder.AppendLine(line);

          hunk = new DiffHunk();

          ParseUnifiedHunkRange(hunk, line);
        }
        else {
          differenceBuilder.AppendLine(line);
        }
      }

      hunk.Difference = differenceBuilder.ToString();

      yield return hunk;
    }

    // "@@ FROMFILE-RANGE TOFILE-RANGE @@"
    // "@@ -1,7 +1,6 @@"
    private static Regex unifiedHunkRangeRegex = new Regex(@"^\@\@ \-(\d+),(\d+) \+(\d+),(\d+) \@\@$",
                                                           RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static void ParseUnifiedHunkRange(DiffHunk hunk, string line)
    {
      lock (unifiedHunkRangeRegex) {
        var match = unifiedHunkRangeRegex.Match(line);

        if (match.Success) {
          hunk.FromRange = new DiffHunkRange(int.Parse(match.Groups[1].Value),
                                             int.Parse(match.Groups[2].Value));
          hunk.ToRange   = new DiffHunkRange(int.Parse(match.Groups[3].Value),
                                             int.Parse(match.Groups[4].Value));
        }
        else {
          throw new FormatException(string.Format("invalid hunk range: {0}", line));
        }
      }
    }
  }
}
