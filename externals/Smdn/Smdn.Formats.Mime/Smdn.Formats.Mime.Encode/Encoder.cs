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
using System.IO;
using System.Security.Cryptography;

namespace Smdn.Formats.Mime.Encode {
 internal static class Encoder {
    internal static MimeMessage Encode(MimeMessage message)
    {
      var encodedHeaders = new MimeHeaderCollection();

      foreach (var header in message.Headers) {
        encodedHeaders.Add(Encode(header));
      }

      var encodedMessage = new MimeMessage(encodedHeaders, message.Content, message.SubParts.ConvertAll<MimeMessage>(Encode));

      encodedMessage.Format = message.Format;
      encodedMessage.Charset = message.Charset;
      encodedMessage.MimeType = message.MimeType;
      encodedMessage.Boundary = message.Boundary;
      encodedMessage.TransferEncoding = message.TransferEncoding;
      encodedMessage.Disposition = message.Disposition;

      return encodedMessage;
    }

    private static MimeHeader Encode(MimeHeader header)
    {
      var format = header.Format ?? MimeFormat.Unspecified;

      return new MimeHeader(header.Name, EncodeHeaderFieldBody(header, format.Folding - 1/*HT*/, header.Name.Length + 2, format.GetHeaderFoldingString()));
    }

    internal static string Encode(MimeHeaderFragment fragment, MimeFormat format)
    {
      format = format ?? MimeFormat.Unspecified;

      return EncodeHeaderFieldBody(fragment, format.Folding - 1/*HT*/, 0, format.GetHeaderFoldingString());
    }

    private static string EncodeHeaderFieldBody(IHeaderFieldBody body, int foldingLimit, int foldingOffset, string foldingString)
    {
      switch (body.Encoding) {
        case MimeEncodingMethod.None:
          if (body.Charset == null)
            return body.Value;
          else
            return Charsets.ISO8859_1.GetString(body.Charset.GetBytes(body.Value));

        case MimeEncodingMethod.Base64:
        case MimeEncodingMethod.QuotedPrintable: {
          if (0 < foldingLimit) {
            if (body.Charset == null)
              return MimeEncoding.Encode(body.Value, body.Encoding, foldingLimit, foldingOffset, foldingString);
            else
              return MimeEncoding.Encode(body.Value, body.Encoding, body.Charset, foldingLimit, foldingOffset, foldingString);
          }
          else {
            if (body.Charset == null)
              return MimeEncoding.Encode(body.Value, body.Encoding);
            else
              return MimeEncoding.Encode(body.Value, body.Encoding, body.Charset);
          }
        }

        default:
          throw new NotSupportedException("unsupported encoding");
      }
    }

    internal static Stream CreateContentWritingStream(MimeMessage message)
    {
      var format = (message.Format ?? MimeFormat.Unspecified);

      message.Content = new MemoryStream(1024);

      return ContentEncodingStreamUtils.CreateEncodingStream(message.Content, message.TransferEncoding, format);
    }

    internal static StreamWriter CreateContentTextWriter(MimeMessage message)
    {
      if (message.TransferEncoding == ContentTransferEncodingMethod.Binary)
        throw new InvalidOperationException("can't create TextWriter from message of binary transfer encoding");

      var writer = new StreamWriter(CreateContentWritingStream(message), message.Charset ?? Charsets.ISO8859_1);

      writer.NewLine = (message.Format ?? MimeFormat.Unspecified).GetEOLString();

      return writer;
    }

    internal static BinaryWriter CreateContentBinaryWriter(MimeMessage message)
    {
      if (message.Charset == null)
        return new BinaryWriter(CreateContentWritingStream(message));
      else
        return new BinaryWriter(CreateContentWritingStream(message), message.Charset);
    }
  }
}