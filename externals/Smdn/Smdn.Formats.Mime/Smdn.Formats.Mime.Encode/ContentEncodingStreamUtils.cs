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
  public static class ContentEncodingStreamUtils {
    public static Stream CreateEncodingStream(Stream contentStream, string transferEncoding)
    {
      if (transferEncoding == null)
        throw new ArgumentNullException("transferEncoding");

      return CreateEncodingStream(contentStream, ContentTransferEncoding.GetEncodingMethod(transferEncoding), MimeFormat.Unspecified);
    }

    public static Stream CreateEncodingStream(Stream contentStream, ContentTransferEncodingMethod transferEncoding)
    {
      return CreateEncodingStream(contentStream, transferEncoding, MimeFormat.Unspecified);
    }

    public static Stream CreateEncodingStream(Stream contentStream, ContentTransferEncodingMethod transferEncoding, MimeFormat format)
    {
      if (contentStream == null)
        throw new ArgumentNullException("contentStream");
      if (format == null)
        throw new ArgumentNullException("format");

      switch (transferEncoding) {
        case ContentTransferEncodingMethod.SevenBit:
        case ContentTransferEncodingMethod.EightBit:
        case ContentTransferEncodingMethod.Binary:
          return contentStream;
        case ContentTransferEncodingMethod.Base64:
          return new Base64ContentEncodingStream(contentStream, format);
        case ContentTransferEncodingMethod.QuotedPrintable:
          return new QuotedPrintableContentEncodingStream(contentStream, format);
        case ContentTransferEncodingMethod.UUEncode:
        case ContentTransferEncodingMethod.GZip64:
        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(transferEncoding);
      }
    }
  }
}
