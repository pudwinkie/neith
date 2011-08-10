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
using System.Security.Cryptography;

using Smdn.Formats;

namespace Smdn.Net.Pop3.Protocol {
  public static class ApopDigest {
    public static string Calculate(string timestamp, string sharedSecret)
    {
      if (timestamp == null)
        throw new ArgumentNullException("timestamp");
      if (timestamp.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("timestamp");
      if (sharedSecret == null)
        throw new ArgumentNullException("sharedSecret");
      if (sharedSecret.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("sharedSecret");

      return Calculate(ByteString.CreateImmutable(timestamp),
                       ByteString.CreateImmutable(sharedSecret));
    }

    public static string Calculate(ByteString timestamp, ByteString sharedSecret)
    {
      if (timestamp == null)
        throw new ArgumentNullException("timestamp");
      if (timestamp.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("timestamp");
      if (sharedSecret == null)
        throw new ArgumentNullException("sharedSecret");
      if (sharedSecret.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("sharedSecret");

      using (var hasher = MD5.Create()) {
        /*
         * The `digest' parameter is calculated by applying
         * the MD5 algorithm [RFC1321] to a string consisting of the
         * timestamp (including angle-brackets) followed by a shared
         * secret.
         * 
         * The `digest' parameter
         * itself is a 16-octet value which is sent in hexadecimal
         * format, using lower-case ASCII characters.
         */
        var str = timestamp + sharedSecret;

        return Hexadecimals.ToLowerString(hasher.ComputeHash(str.Segment.Array,
                                                             str.Segment.Offset,
                                                             str.Segment.Count));
      }
    }
  }
}
