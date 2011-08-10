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
using System.Collections.Generic;
using System.IO;
using System.Text;

using Smdn.IO;

namespace Smdn.Formats.Mime.Decode {
 internal static class Parser {
    internal static MimeMessage Parse(Stream stream,
                                      EncodingSelectionCallback selectFallbackCharset)
    {
      return Parse(new LooseLineOrientedStream(stream), selectFallbackCharset);
    }

    private static MimeMessage Parse(LineOrientedStream stream,
                                     EncodingSelectionCallback selectFallbackCharset)
    {
      return ParseBody(stream,
                       ParseHeader(stream),
                       selectFallbackCharset);
    }

    private static MimeHeaderCollection ParseHeader(LineOrientedStream stream)
    {
      var headers = new MimeHeaderCollection();
      MimeHeader current = null;

      for (;;) {
        var lineBytes = stream.ReadLine(false);

        if (lineBytes == null)
          break; // unexpected end of stream

        var line = ByteString.CreateImmutable(lineBytes);

        if (line.IsEmpty)
          break; // end of headers

        if (line[0] == Octets.HT || line[0] == Octets.SP) { // LWSP-char
          // folding
          if (current == null)
            // ignore incorrect formed header
            continue;

          current.Value += Chars.SP;
          current.Value += line.TrimStart().ToString();
        }
        else {
          // field       =  field-name ":" [ field-body ] CRLF
          // field-name  =  1*<any CHAR, excluding CTLs, SPACE, and ":">
          var delim = line.IndexOf(MimeHeader.NameBodyDelimiter); // ':'

          if (delim < 0) {
            // ignore incorrect formed header
            current = null;
            continue;
          }

          var header = new MimeHeader(line.Substring(0, delim).TrimEnd().ToString(),
                                      line.Substring(delim + 1).TrimStart().ToString());

          headers.Add(header);

          current = header;
        }
      }

      return headers;
    }

    private static MimeMessage ParseBody(LineOrientedStream stream,
                                         MimeHeaderCollection headers,
                                         EncodingSelectionCallback selectFallbackCharset)
    {
      var message = new MimeMessage(headers);

      ParseContentType(message, selectFallbackCharset);
      ParseContentTransferEncoding(message);
      ParseContentDisposition(message);

      // read and parse content
      MemoryStream contentStream;

      if (message.MimeType == null || !message.MimeType.TypeEquals("multipart")) {
        contentStream = new MemoryStream(1024);

        stream.CopyTo(contentStream, MimeFormat.Standard.Folding);

        message.Content = contentStream;

        return message;
      }

      // multipart/*
      var parts = new List<MimeMessage>();
      var delimiter = ByteString.CreateImmutable("--" + message.Boundary);
      var closeDelimiter = ByteString.CreateImmutable("--" + message.Boundary + "--");
      MemoryStream body = null;
      ByteString line = null;
      ByteString lastLine = null;

      contentStream = new MemoryStream(1024);

      for (;;) {
        if (lastLine != null)
          contentStream.Write(lastLine.Segment.Array,
                              lastLine.Segment.Offset,
                              lastLine.Segment.Count);

        var l = stream.ReadLine();

        if (l == null)
          break;

        lastLine = line;
        line = ByteString.CreateImmutable(l);

        if (line.StartsWith(delimiter)) {
          if (lastLine != null) {
            if (ByteString.IsTerminatedByCRLF(lastLine))
              // CRLF "--" boundary
              contentStream.Write(lastLine.Segment.Array,
                                  lastLine.Segment.Offset,
                                  lastLine.Segment.Count - 2);
            else
              // LF "--" boundary or CR "--" boundary
              contentStream.Write(lastLine.Segment.Array,
                                  lastLine.Segment.Offset,
                                  lastLine.Segment.Count - 1);
          }

          contentStream.Position = 0;

          if (body == null)
            body = contentStream;
          else
            parts.Add(Parse(contentStream, selectFallbackCharset));

          if (line.StartsWith(closeDelimiter))
            break;
          else
            contentStream = new MemoryStream(1024);

          lastLine = null;
        }
      }

      message.Content = body;
      message.SubParts.AddRange(parts);

      return message;
    }

    private static void ParseContentType(MimeMessage message,
                                         EncodingSelectionCallback selectFallbackCharset)
    {
      const string headerName = "Content-Type";

      if (!message.Headers.Contains(headerName))
        return;

      var contentType = message.Headers[headerName];
      var mimeTypeString = contentType.GetValueWithoutParameter();

      if (!string.IsNullOrEmpty(mimeTypeString))
        message.MimeType = new MimeType(mimeTypeString);

      var charsetString = contentType.GetParameter("charset", true);

      if (!string.IsNullOrEmpty(charsetString))
        message.Charset = Charsets.FromString(charsetString, selectFallbackCharset);

      message.Boundary = contentType.GetParameter("boundary", true);
    }

    private static void ParseContentTransferEncoding(MimeMessage message)
    {
      message.TransferEncoding = ContentTransferEncodingMethod.SevenBit; // as default

      if (!message.Headers.Contains(ContentTransferEncoding.HeaderName))
        return;

      var contentTransferEncoding = message.Headers[ContentTransferEncoding.HeaderName].GetValueWithoutParameter();

      if (string.IsNullOrEmpty(contentTransferEncoding))
        return;

      message.TransferEncoding = ContentTransferEncoding.GetEncodingMethod(contentTransferEncoding);
    }

    private static void ParseContentDisposition(MimeMessage message)
    {
      const string headerName = "Content-Disposition";

      message.Disposition = MimeMessageDisposition.None; // as default

      if (!message.Headers.Contains(headerName))
        return;

      var contentDisposition = message.Headers[headerName].GetValueWithoutParameter();

      if (string.IsNullOrEmpty(contentDisposition))
        return;

      switch (contentDisposition.ToLowerInvariant()) {
        case "inline": message.Disposition = MimeMessageDisposition.Inline; break;
        case "attachment": message.Disposition = MimeMessageDisposition.Attachment; break;
        default:
          throw new NotSupportedException(string.Format("unsupported content disposition: '{0}'", contentDisposition));
      }
    }
  }
}