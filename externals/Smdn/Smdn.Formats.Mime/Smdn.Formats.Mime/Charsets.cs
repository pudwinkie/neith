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
using System.Text;

namespace Smdn.Formats.Mime {
  public class Charsets {
    public static Encoding ASCII {
      get { return Encoding.ASCII; }
    }

    public static Encoding UTF7 {
      get { return Encoding.UTF7; }
    }

    public static Encoding UTF8 {
      get { return UTF8NoBom; }
    }

    public static Encoding Latin1 {
      get { return ISO8859_1; }
    }

    public static Encoding JIS {
      get { return ISO2022JP; }
    }

    public static Encoding ShiftJIS {
      get { return Shift_JIS; }
    }

    public static Encoding FromString(string name)
    {
      return FromString(name, null);
    }

    public static Encoding FromString(string name, EncodingSelectionCallback selectFallbackCharset)
    {
      return EncodingUtils.GetEncodingThrowException(name, selectFallbackCharset);
    }

    public static string ToString(Encoding encoding)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      return encoding.BodyName;
    }

    internal static readonly Encoding ISO8859_1 = Encoding.GetEncoding("ISO-8859-1");
    internal static readonly Encoding ISO2022JP = Encoding.GetEncoding("ISO-2022-JP");
    internal static readonly Encoding Shift_JIS  = Encoding.GetEncoding("shift_jis");
    internal static readonly Encoding UTF8NoBom = new UTF8Encoding(false, true);
  }
}
