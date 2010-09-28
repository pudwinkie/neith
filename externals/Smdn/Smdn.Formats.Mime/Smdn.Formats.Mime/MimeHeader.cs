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
using System.Text;

namespace Smdn.Formats.Mime {
  public class MimeHeader : IHeaderFieldBody {
    internal const char NameBodyDelimiter = ':';
    internal const string NameBodyFormattingDelimiter = ": ";

    public static MimeHeader CreateContentType(MimeType mimeType)
    {
      return CreateContentType(null, mimeType, null, null, null);
    }

    public static MimeHeader CreateContentType(MimeType mimeType, Encoding charset)
    {
      return CreateContentType(null, mimeType, charset, null, null);
    }

    public static MimeHeader CreateContentType(MimeType mimeType, Encoding charset, string boundary)
    {
      return CreateContentType(null, mimeType, charset, boundary, null);
    }

    public static MimeHeader CreateContentType(MimeType mimeType, Encoding charset, string boundary, string name)
    {
      return CreateContentType(null, mimeType, charset, boundary, name);
    }

    public static MimeHeader CreateContentType(MimeFormat format, MimeType mimeType, Encoding charset)
    {
      return CreateContentType(format, mimeType, charset, null, null);
    }

    public static MimeHeader CreateContentType(MimeFormat format, MimeType mimeType, Encoding charset, string boundary)
    {
      return CreateContentType(format, mimeType, charset, boundary, null);
    }

    public static MimeHeader CreateContentType(MimeFormat format, MimeType mimeType, Encoding charset, string boundary, string name)
    {
      const string headerName = "Content-Type";

      if (mimeType == null)
        throw new ArgumentNullException("mimeType");

      var fragments = new List<MimeHeaderFragment>();

      fragments.Add(new MimeHeaderFragment(mimeType.ToString()));

      if (charset != null)
        fragments.Add(new MimeHeaderParameterFragment("charset", Charsets.ToString(charset)));

      if (boundary != null)
        fragments.Add(new MimeHeaderParameterFragment("boundary", boundary));

      if (name != null)
        // TODO: name*n*
        fragments.Add(new MimeHeaderParameterFragment("name", MimeEncoding.Encode(name, MimeEncodingMethod.Base64, Charsets.UTF8)));

      return new MimeHeader(headerName, format, fragments);
    }

    public static MimeHeader CreateContentTransferEncoding(ContentTransferEncodingMethod transferEncoding)
    {
      return CreateContentTransferEncoding(null, transferEncoding);
    }

    public static MimeHeader CreateContentTransferEncoding(MimeFormat format, ContentTransferEncodingMethod transferEncoding)
    {
      return new MimeHeader(ContentTransferEncoding.HeaderName,
                            format,
                            new[] {new MimeHeaderFragment(ContentTransferEncoding.GetEncodingName(transferEncoding))});
    }

    public static MimeHeader CreateContentDisposition(MimeMessageDisposition disposition, string filename)
    {
      const string headerName = "Content-Disposition";

      var fragments = new List<MimeHeaderFragment>();

      switch (disposition) {
        case MimeMessageDisposition.Inline: fragments.Add(new MimeHeaderFragment("inline")); break;
        case MimeMessageDisposition.Attachment: fragments.Add(new MimeHeaderFragment("attachment")); break;
        default: throw new ArgumentException("disposition must be inline or attachment", "disposition");
      }

      if (!string.IsNullOrEmpty(filename))
        // TODO: filename*n*
        fragments.Add(new MimeHeaderParameterFragment("filename", MimeEncoding.Encode(filename, MimeEncodingMethod.Base64, Charsets.UTF8)));

      return new MimeHeader(headerName, fragments);
    }

    public string Name {
      get { return name; }
    }

    internal int Index {
      get; set;
    }

    public MimeFormat Format {
      get; set;
    }

    public string Value {
      get; set;
    }

    public MimeEncodingMethod Encoding {
      get; set;
    }

    public Encoding Charset {
      get; set;
    }

    public MimeHeader(string name)
      : this(name, null, MimeFormat.Standard, MimeEncodingMethod.None, null)
    {
    }

    public MimeHeader(string name, string @value)
      : this(name, @value, MimeFormat.Standard, MimeEncodingMethod.None, null)
    {
    }

    public MimeHeader(string name, string @value, MimeFormat format)
      : this(name, @value, format, MimeEncodingMethod.None, null)
    {
    }

    public MimeHeader(string name, string @value, MimeEncodingMethod encoding)
      : this(name, @value, MimeFormat.Standard, encoding, null)
    {
    }

    public MimeHeader(string name, string @value, MimeEncodingMethod encoding, Encoding charset)
      : this(name, @value, MimeFormat.Standard, encoding, charset)
    {
    }

    public MimeHeader(string name, string @value, MimeFormat format, MimeEncodingMethod encoding)
      : this(name, @value, format, encoding, null)
    {
    }

    public MimeHeader(string name, string @value, MimeFormat format, MimeEncodingMethod encoding, Encoding charset)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      else if (name.Length == 0)
        throw new ArgumentException("length of name is zero", "name");

      // TODO: check name
      // field-name  =  1*<any CHAR, excluding CTLs, SPACE, and ":">
      this.name = name;
      this.Index = 0;
      this.Format = format;
      this.Value = @value;
      this.Encoding = encoding;
      this.Charset = charset;
    }

    public MimeHeader(string name, IEnumerable<MimeHeaderFragment> headerFragments)
      : this(name, string.Empty, MimeFormat.Standard, MimeEncodingMethod.None, null)
    {
      SetValue(null, headerFragments);
    }

    public MimeHeader(string name, MimeFormat format, IEnumerable<MimeHeaderFragment> headerFragments)
      : this(name, string.Empty, format, MimeEncodingMethod.None, null)
    {
      SetValue(format, headerFragments);
    }

    public void SetValue(MimeFormat format, IEnumerable<MimeHeaderFragment> headerFragments)
    {
      if (headerFragments == null)
        throw new ArgumentNullException("headerFragments");

      Format = format;
      Encoding = MimeEncodingMethod.None;
      Charset  = null;
      Value = EncodeFragments(headerFragments);
    }

    private string EncodeFragments(IEnumerable<MimeHeaderFragment> fragments)
    {
      var sb = new StringBuilder();
      var first = true;
      var currentLineLength = name.Length + NameBodyFormattingDelimiter.Length;
      var encodedSplitter = new[] {Chars.CRLF + Chars.HT};
      var format = Format ?? MimeFormat.Unspecified;

      foreach (var fragment in fragments) {
        var encodedLines = Encode.Encoder.Encode(fragment, format).Split(encodedSplitter, StringSplitOptions.None);

        for (var line = 0; line < encodedLines.Length; line++) {
          // delimiter
          if (line == 0 && fragment is MimeHeaderParameterFragment) {
            sb.Append(';');
            currentLineLength += 1;
          }

          // folding
          if (0 < format.Folding && format.Folding <= currentLineLength + encodedLines[line].Length) {
            sb.Append(format.GetHeaderFoldingString());
            sb.Append(encodedLines[line]);

            currentLineLength = 1/*TAB*/ + encodedLines[line].Length;
          }
          else {
            if (!first && line == 0) {
              // delimiter
              sb.Append(Chars.SP);
              currentLineLength = 1;
            }

            sb.Append(encodedLines[line]);

            currentLineLength += encodedLines[line].Length;
          }
        }

        first = false;
      } // foreach fragment

      return sb.ToString();
    }

    public bool IsNameEquals(string name)
    {
      return string.Equals(this.name, name, StringComparison.InvariantCultureIgnoreCase);
    }

    public string GetParameter(string name)
    {
      return GetParameter(name, true);
    }

    public string GetParameter(string name, bool dequote)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      if (Value == null)
        return null;

      name = name.ToLowerInvariant();

      var paramStarts = Value.IndexOf(name + "=", StringComparison.InvariantCultureIgnoreCase);

      if (paramStarts < 0)
        return null;

      var valueStarts = paramStarts + name.Length + 1;
      var valueEnds = Value.IndexOf(';', valueStarts + 1);
      string val;

      if (valueEnds == -1)
        val = Value.Substring(valueStarts);
      else
        val = Value.Substring(valueStarts, valueEnds - valueStarts).Trim();

      // if quoted
      if (dequote && val.StartsWith("\"") && val.EndsWith("\""))
        val = val.Substring(1, val.Length - 2);

      return val;
    }

    public string GetValueWithoutParameter()
    {
      if (Value == null)
        return null;

      var delim = Value.IndexOfAny(new[] {';', ' '});

      if (delim < 0)
        return Value;
      else
        return Value.Substring(0, delim);
    }

    public override string ToString()
    {
      return string.Format("{0}{1}{2}", name, NameBodyFormattingDelimiter, Value);
    }

    private string name;
  }
}
