// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

namespace Smdn.Formats.Mime {
  public class MimeFormat {
    public static readonly MimeFormat Standard = new MimeFormat(76, NewLine.Standard, true);
    internal static readonly MimeFormat Unspecified = new MimeFormat(0, NewLine.Standard, true);

    /// <value>0 means no folding</value>
    public int Folding {
      get { return folding; }
      set
      {
        if (readOnly)
          throw new InvalidOperationException("readonly");
        if (value < 0)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Folding", value);
        folding = value;
      }
    }

    public NewLine NewLine {
      get { return newLine; }
      set
      {
        if (readOnly)
          throw new InvalidOperationException("readonly");
        newLine = value;
      }
    }

    public MimeFormat()
      : this(72, NewLine.Standard, false)
    {
    }

    public MimeFormat(int folding, NewLine newLine)
      : this(folding, newLine, false)
    {
    }

    private MimeFormat(int folding, NewLine newLine, bool readOnly)
    {
      this.Folding = folding;
      this.NewLine = newLine;

      this.readOnly = readOnly;
    }

    public byte[] GetEOLBytes()
    {
      switch (newLine) {
        case NewLine.CR:
          return new[] {Octets.CR};
        case NewLine.LF:
          return new[] {Octets.LF};
        case NewLine.CRLF:
          return new[] {Octets.CR, Octets.LF};
        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(newLine);
      }
    }

    public string GetEOLString()
    {
      switch (newLine) {
        case NewLine.CR:
          return new string(new[] {Chars.CR});
        case NewLine.LF:
          return new string(new[] {Chars.LF});
        case NewLine.CRLF:
          return new string(new[] {Chars.CR, Chars.LF});
        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(newLine);
      }
    }

    public string GetHeaderFoldingString()
    {
      switch (newLine) {
        case NewLine.CR:
          return new string(new[] {Chars.CR, Chars.HT});
        case NewLine.LF:
          return new string(new[] {Chars.LF, Chars.HT});
        case NewLine.CRLF:
          return new string(new[] {Chars.CR, Chars.LF, Chars.HT});
        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(newLine);
      }
    }

    private readonly bool readOnly;
    private int folding = 76;
    private NewLine newLine = NewLine.Standard;
  }
}
