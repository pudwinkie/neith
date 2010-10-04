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
using System.Text;

namespace Smdn.Net.Imap4 {
  public struct ImapPartialRange : IEquatable<ImapPartialRange>, IFormattable {
    public long Start {
      get { return start; }
      set
      {
        if (value < 0L || 0x100000000 <= value)
          throw new ArgumentOutOfRangeException("Start", value, "must be an non-zero unsigned 32-bit integer");
        start = value;
      }
    }

    public long? Length {
      get { return length; }
      set
      {
        if (value.HasValue) {
          if (value.Value <= 0L || 0x100000000 <= value.Value)
            throw new ArgumentOutOfRangeException("Length", value.Value, "must be an unsigned 32-bit integer");
          if (0x100000000 <= start + value.Value)
            throw new ArgumentOutOfRangeException("Length", value.Value, "(Start + Length) must be an unsigned 32-bit integer");
        }

        length = value;
      }
    }

    public bool IsLengthSpecified {
      get { return length.HasValue; }
    }

    public ImapPartialRange(long start)
      : this()
    {
      Start = start;
      Length = null;
    }

    public ImapPartialRange(long start, long length)
      : this()
    {
      Start = start;
      Length = length;
    }

    public override bool Equals(object obj)
    {
      if (obj is ImapPartialRange)
        return Equals((ImapPartialRange)obj);
      else
        return false;
    }

    public bool Equals(ImapPartialRange other)
    {
      return (this.start == other.start && this.length == other.length);
    }

    public override int GetHashCode()
    {
      return ToString().GetHashCode();
    }

    public override string ToString()
    {
      return ToString(null, null);
    }

    public string ToString(string format)
    {
      return ToString(format, null);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      bool checkLength;
      string open = "<";
      string close = ">";

      switch (format) {
        case "u": // IMAP-URL style
          checkLength = false;
          open = "/;PARTIAL=";
          close = null;
          break;

        case "f": // fetch-att style
          checkLength = true;
          break;

        default:
          checkLength = false;
          break;
      }

      if (checkLength && !length.HasValue)
        throw new FormatException("length must be specified");

      var sb = new StringBuilder(64);

      sb.Append(open);
      sb.Append(start);

      if (length.HasValue) {
        sb.Append('.');
        sb.Append(length.Value);
      }

      sb.Append(close);

      return sb.ToString();
    }

    private long start;
    private long? length;
  }
}
