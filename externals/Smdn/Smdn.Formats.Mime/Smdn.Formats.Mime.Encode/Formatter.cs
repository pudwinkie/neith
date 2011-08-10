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
using System.IO;
using System.Text;

namespace Smdn.Formats.Mime.Encode {
 internal static class Formatter {
    public static void Format(MimeMessage message, Stream stream)
    {
      var multipart = 0 < message.SubParts.Count;

      if (multipart && message.Boundary == null)
        throw new NullReferenceException("message is multipart, but boudnary was null");

      if (!message.Headers.Contains("MIME-Version"))
        message.Headers.Insert(0, new MimeHeader("MIME-Version", "1.0"));

      if (!message.Headers.Contains("Content-Type"))
        message.Headers.Insert(1, MimeHeader.CreateContentType(message.Format,
                                                               message.MimeType ?? (multipart ? MimeType.MultipartMixed : MimeType.TextPlain),
                                                               message.Charset,
                                                               multipart ? message.Boundary : null));

      if (!message.Headers.Contains("Content-Transfer-Encoding"))
        message.Headers.Insert(1, MimeHeader.CreateContentTransferEncoding(message.Format, message.TransferEncoding));

      var textWriter = new StreamWriter(stream, Charsets.ISO8859_1);
      var eol = (message.Format ?? MimeFormat.Unspecified).GetEOLString();

      textWriter.NewLine = eol;

      foreach (var header in message.Headers) {
        textWriter.WriteLine(header.ToString());
      }

      if (multipart) {
        var delimiter = string.Concat(eol, "--", message.Boundary);

        foreach (var part in message.SubParts) {
          textWriter.WriteLine(delimiter);
          textWriter.Flush();

          Format(part, stream);
        }

        textWriter.WriteLine(string.Concat(delimiter, "--"));
        textWriter.Flush();
      }
      else {
        textWriter.WriteLine();
        textWriter.Flush();

        if (message.Content != null) {
          message.Content.WriteTo(stream);
          stream.Flush();
        }
      }
    }
  }
}