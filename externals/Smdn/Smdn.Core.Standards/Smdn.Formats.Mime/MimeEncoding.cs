// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using Smdn.Security.Cryptography;

namespace Smdn.Formats.Mime {
  /*
   * http://tools.ietf.org/html/rfc2047
   * RFC 2047 - MIME (Multipurpose Internet Mail Extensions) Part Three: Message Header Extensions for Non-ASCII Text
   * 2. Syntax of encoded-words
   * 3. Character sets
   * 4. Encodings

   * encoded-word = "=?" charset "?" encoding "?" encoded-text "?="
   * charset = token    ; see section 3
   * encoding = token   ; see section 4
   * token = 1*<Any CHAR except SPACE, CTLs, and especials>
   * especials = "(" / ")" / "<" / ">" / "@" / "," / ";" / ":" / "
   *             <"> / "/" / "[" / "]" / "?" / "." / "="
   * encoded-text = 1*<Any printable ASCII character other than "?"
   *                   or SPACE>
   *               ; (but see "Use of encoded-words in message
   *               ; headers", section 5)
   */
  public static class MimeEncoding {
    public static string Encode(string str, MimeEncodingMethod encoding)
    {
      return Encode(str, encoding, Encoding.ASCII, false, 0, 0, null);
    }

    public static string Encode(string str, MimeEncodingMethod encoding, Encoding charset)
    {
      return Encode(str, encoding, charset, false, 0, 0, null);
    }

    public static string Encode(string str, MimeEncodingMethod encoding, int foldingLimit, int foldingOffset)
    {
      return Encode(str, encoding, Encoding.ASCII, true, foldingLimit, foldingOffset, mimeEncodingFoldingString);
    }

    public static string Encode(string str, MimeEncodingMethod encoding, int foldingLimit, int foldingOffset, string foldingString)
    {
      return Encode(str, encoding, Encoding.ASCII, true, foldingLimit, foldingOffset, foldingString);
    }

    public static string Encode(string str, MimeEncodingMethod encoding, Encoding charset, int foldingLimit, int foldingOffset)
    {
      return Encode(str, encoding, charset, true, foldingLimit, foldingOffset, mimeEncodingFoldingString);
    }

    public static string Encode(string str, MimeEncodingMethod encoding, Encoding charset, int foldingLimit, int foldingOffset, string foldingString)
    {
      return Encode(str, encoding, charset, true, foldingLimit, foldingOffset, foldingString);
    }

    private static readonly string mimeEncodingFoldingString = Chars.CRLF + Chars.HT;
    private static readonly byte[] mimeEncodingPostamble = new byte[] {0x3f, 0x3d}; // "?="

    private static string Encode(string str, MimeEncodingMethod encoding, Encoding charset, bool doFold, int foldingLimit, int foldingOffset, string foldingString)
    {
      if (str == null)
        throw new ArgumentNullException("str");
      if (charset == null)
        throw new ArgumentNullException("charset");
      if (doFold) {
        if (foldingLimit < 1)
          throw new ArgumentOutOfRangeException("foldingLimit", foldingLimit, "must be greater than 1");
        if (foldingOffset < 0)
          throw new ArgumentOutOfRangeException("foldingOffset", foldingOffset, "must be greater than zero");
        if (foldingLimit <= foldingOffset)
          throw new ArgumentOutOfRangeException("foldingOffset", foldingOffset, "must be less than foldingLimit");
        if (foldingString == null)
          throw new ArgumentNullException("foldingString");
      }

      ICryptoTransform transform;
      char encodingChar;

      switch (encoding) {
        case MimeEncodingMethod.Base64:
          transform = new ToBase64Transform();
          encodingChar = 'b';
          break;
        case MimeEncodingMethod.QuotedPrintable:
          transform = new ToQuotedPrintableTransform();
          encodingChar = 'q';
          break;
        default:
          throw new System.ComponentModel.InvalidEnumArgumentException("encoding", (int)encoding, typeof(MimeEncodingMethod));
      }

      var preambleText = string.Format("=?{0}?{1}?", charset.BodyName, encodingChar);

      if (!doFold)
        return preambleText + transform.TransformStringTo(str, charset) + "?=";

      // folding
      var ret = new StringBuilder();
      var preamble = Encoding.ASCII.GetBytes(preambleText);
      var firstLine = true;
      var inputCharBuffer = str.ToCharArray();
      var inputCharOffset = 0;
      var outputBuffer = new byte[foldingLimit];
      var ambleLength = preamble.Length + mimeEncodingPostamble.Length;
      var outputLimit = foldingLimit - (foldingOffset + ambleLength);

      if (outputLimit <= 0)
        throw new ArgumentOutOfRangeException("foldingLimit", foldingLimit, "too short");

      // copy preamble to buffer
      Buffer.BlockCopy(preamble, 0, outputBuffer, 0, preamble.Length);

      for (;;) {
        var inputBlockSizeLimit = (outputLimit * transform.InputBlockSize) / transform.OutputBlockSize - 1;
        var transformCharCount = 0;
        var outputCount = preamble.Length;

        // decide char count to transform
        for (transformCharCount = inputBlockSizeLimit / charset.GetMaxByteCount(1);; transformCharCount++) {
          if (inputCharBuffer.Length <= inputCharOffset + transformCharCount) {
            transformCharCount = inputCharBuffer.Length - inputCharOffset;
            break;
          }

          if (inputBlockSizeLimit <= charset.GetByteCount(inputCharBuffer, inputCharOffset, transformCharCount + 1))
            break;
        }

        // transform chars
        byte[] transformed = null;

        for (;;) {
          var t = transform.TransformBytes(charset.GetBytes(inputCharBuffer, inputCharOffset, transformCharCount));

          if (transformed == null || t.Length <= outputLimit) {
            transformed = t;

            if (inputCharBuffer.Length <= inputCharOffset + transformCharCount + 1)
              break;

            transformCharCount++;
            continue;
          }
          else {
            transformCharCount--;
            break;
          }
        }

        if (outputBuffer.Length < ambleLength + transformed.Length)
          throw new ArgumentOutOfRangeException("foldingLimit",
                                                foldingLimit,
                                                string.Format("too short, at least {0} is required", ambleLength + transformed.Length));

        // copy transformed chars to buffer
        Buffer.BlockCopy(transformed, 0, outputBuffer, outputCount, transformed.Length);

        outputCount += transformed.Length;

        // copy postanble to buffer
        Buffer.BlockCopy(mimeEncodingPostamble, 0, outputBuffer, outputCount, mimeEncodingPostamble.Length);

        outputCount += mimeEncodingPostamble.Length;

        ret.Append(Encoding.ASCII.GetString(outputBuffer, 0, outputCount));

        inputCharOffset += transformCharCount;

        if (inputCharOffset < inputCharBuffer.Length) {
          ret.Append(foldingString);

          if (firstLine) {
            outputLimit = foldingLimit - ambleLength;
            firstLine = false;
          }
        }
        else {
          break;
        }
      }

      return ret.ToString();
    }

    public static string DecodeNullable(string str)
    {
      if (str == null)
        return null;
      else
        return Decode(str);
    }

    public static string Decode(string str)
    {
      MimeEncodingMethod discard1;
      Encoding discard2;

      return Decode(str, out discard1, out discard2);
    }

    public static string DecodeNullable(string str, out MimeEncodingMethod encoding, out Encoding charset)
    {
      if (str == null) {
        encoding = MimeEncodingMethod.None;
        charset = null;

        return null;
      }
      else {
        return Decode(str, out encoding, out charset);
      }
    }

    private static readonly Regex mimeEncodedWordRegex = new Regex(@"\s*\=\?([^?]+)\?([^?]+)\?([^\?\s]+)\?\=\s*", RegexOptions.Singleline);

    public static string Decode(string str, out MimeEncodingMethod encoding, out Encoding charset)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      charset = null;
      encoding = MimeEncodingMethod.None;

      Encoding lastCharset = null;
      var lastEncoding = MimeEncodingMethod.None;

      lock (mimeEncodedWordRegex) {
        var ret = mimeEncodedWordRegex.Replace(str, delegate(Match m) {
          // charset
          lastCharset = EncodingUtils.GetEncoding(m.Groups[1].Value);

          if (lastCharset == null)
            throw new FormatException(string.Format("{0} is an unsupported or invalid charset", m.Groups[1].Value));

          // encoding
          ICryptoTransform transform;

          switch (m.Groups[2].Value) {
            case "b":
            case "B":
              lastEncoding = MimeEncodingMethod.Base64;
              transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
              break;
            case "q":
            case "Q":
              lastEncoding = MimeEncodingMethod.QuotedPrintable;
              transform = new FromQuotedPrintableTransform();
              break;
            default:
              throw new FormatException(string.Format("{0} is an invalid encoding", m.Groups[2].Value));
          }

          return transform.TransformStringFrom(m.Groups[3].Value, lastCharset);
        });

        charset = lastCharset;
        encoding = lastEncoding;

        return ret;
      }
    }
  }
}
