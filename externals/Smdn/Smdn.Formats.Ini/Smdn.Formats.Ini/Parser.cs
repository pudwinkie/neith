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
using System.IO;
using System.Text.RegularExpressions;

namespace Smdn.Formats.Ini {
  internal static class Parser {
    public static IniDocument Parse(TextReader reader, IEqualityComparer<string> comparer)
    {
      var document = new IniDocument(comparer);
      var section = string.Empty; // default section name

      for (;;) {
        var line = reader.ReadLine();

        if (line == null)
          break;

        // 空行は読み飛ばす
        if (string.Empty.Equals(line))
          continue;

        // コメント行は読み飛ばす
        if (line.StartsWith(";", StringComparison.Ordinal))
          continue;

        if (line.StartsWith("#", StringComparison.Ordinal))
          continue;

        var matchEntry = regexEntry.Match(line);

        if (matchEntry.Success) {
          // name=valueの行
          document[section][matchEntry.Groups["name"].Value.Trim()] = matchEntry.Groups["value"].Value.Trim();
          continue;
        }

        var matchSection = regexSection.Match(line);

        if (matchSection.Success) {
          // [section]の行
          section = matchSection.Groups["section"].Value;
          continue;
        }
      }
      return document;
    }

    private static readonly Regex regexSection = new Regex(@"^[\s\t]*\[(?<section>[^\]]+)\].*$", RegexOptions.Singleline);
    private static readonly Regex regexEntry = new Regex(@"^[\s\t]*(?<name>[^=]+)=(?<value>.*?)([\s\t]+;(?<comment>.*))?$", RegexOptions.Singleline);
  }
}
