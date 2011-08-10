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

namespace Smdn.Formats.Diff {
  public class DiffLine {
    public string Text {
      get; private set;
    }

    public int LineNumber {
      get; private set;
    }

    public DiffLineStatus Status {
      get; private set;
    }

    public static DiffLine CreateNonExistentLine()
    {
      return new DiffLine(null, 0, DiffLineStatus.NonExistent);
    }

    public DiffLine(string text, int lineNumber, DiffLineStatus status)
    {
      var nonExistent = (status == DiffLineStatus.NonExistent);

      if (!nonExistent && text == null)
        throw new ArgumentNullException("text");
      if (!nonExistent && lineNumber <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("lineNumber", lineNumber);

      Text = nonExistent ? null : text;
      LineNumber = lineNumber;
      Status = status;
    }

    internal void Update(DiffLineStatus status)
    {
      Status = status;
    }

    internal void Update(string text, int lineNumber, DiffLineStatus status)
    {
      Text = text;
      LineNumber = lineNumber;
      Status = status;
    }
  }
}
