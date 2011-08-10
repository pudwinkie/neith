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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Smdn.Formats.Mime.Decode {
 internal static class Decoder {
    internal static MimeMessage Decode(MimeMessage message)
    {
      var decodedHeaders = Decode(message.Headers);

      var decodedMessage = new MimeMessage(decodedHeaders, message.Content, message.SubParts.ConvertAll<MimeMessage>(Decode));

      decodedMessage.Charset = message.Charset;
      decodedMessage.MimeType = message.MimeType;
      decodedMessage.Boundary = message.Boundary;
      decodedMessage.TransferEncoding = message.TransferEncoding;
      decodedMessage.Disposition = message.Disposition;

      return decodedMessage;
    }

    private static MimeHeaderCollection Decode(MimeHeaderCollection headers)
    {
      var decodedHeaders = new MimeHeaderCollection();

      foreach (var header in headers) {
        decodedHeaders.Add(new MimeHeader(header.Name, MimeEncoding.Decode(header.Value)));
      }

      return decodedHeaders;
    }

    internal static Stream CreateContentReadingStream(MimeMessage message)
    {
      if (message.Content == null)
        throw new InvalidOperationException("content is null");

      message.Content.Position = 0L;

      return ContentTransferEncoding.CreateDecodingStream(message.Content,
                                                          message.TransferEncoding);
    }

    internal static StreamReader CreateContentTextReader(MimeMessage message)
    {
      if (message.Content == null)
        throw new InvalidOperationException("content is null");

      message.Content.Position = 0L;

      return ContentTransferEncoding.CreateTextReader(message.Content,
                                                      message.TransferEncoding,
                                                      message.Charset ?? Charsets.ISO8859_1);
    }

    internal static BinaryReader CreateContentBinaryReader(MimeMessage message)
    {
      if (message.Content == null)
        throw new InvalidOperationException("content is null");

      message.Content.Position = 0L;

      return ContentTransferEncoding.CreateBinaryReader(message.Content,
                                                        message.TransferEncoding,
                                                        message.Charset);
    }
  }
}