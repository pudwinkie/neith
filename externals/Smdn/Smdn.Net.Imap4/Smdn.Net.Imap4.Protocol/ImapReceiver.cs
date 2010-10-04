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
using System.IO;
using System.Text;

using Smdn.Formats;
using Smdn.IO;

namespace Smdn.Net.Imap4.Protocol {
  public abstract class ImapReceiver {
    private LineOrientedBufferedStream stream;

    protected ImapReceiver(LineOrientedBufferedStream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      this.stream = stream;
    }

    protected ByteString Receive()
    {
      var line = stream.ReadLine(true);

      if (line == null)
        throw new ImapConnectionException("stream closed");

      return new ByteString(line);
    }

    protected interface IParsingContext {
    }

    private class ParsingContext : IParsingContext {
      private List<ImapData>[] nestedData = new List<ImapData>[1];

      public ParsingContext()
      {
        nestedData[0] = new List<ImapData>(16); // XXX: initial capacity
      }

      public void AddData(ImapData data)
      {
        nestedData[nest].Add(data);
      }

      public ImapData[] GetParsed()
      {
        return nestedData[0].ToArray();
      }

      /*
       * parenthesized
       */
      public bool ParenthesesClosed {
        get { return nest == 0; }
      }

      private int nest = 0;

      public void OpenParenthesis()
      {
        nest++;

        if (nestedData.Length <= nest)
          Array.Resize(ref nestedData, nest + 1);

        nestedData[nest] = new List<ImapData>(16); // XXX: initial capacity
      }

      public void CloseParenthesis()
      {
        if (nest == 0)
          throw new ImapMalformedDataException("unopened parenthesis");

        nestedData[nest - 1].Add(ImapData.CreateListData(nestedData[nest].ToArray()));
        nestedData[nest].Clear();

        nest--;
      }
    }

    private static readonly ByteString nil = new ByteString("NIL");
    private static readonly ByteString crlf = new ByteString(Octets.CRLF);

    protected ImapData[] ParseDataNonTerminatedText(ByteString text, int offset)
    {
      IParsingContext discard = null;

      return ParseData(text + crlf, offset, ref discard);
    }

    protected ImapData[] ParseData(ByteString line, int offset, ref IParsingContext context)
    {
      var index = offset;
      var c = context as ParsingContext;

      if (c == null) {
        context = c = new ParsingContext();
      }
      else {
        if (line.Length - index == 2/*CRLF*/) {
          if (c.ParenthesesClosed) {
            // end of data
            context = null;
            return c.GetParsed();
          }
          else {
            throw new ImapMalformedDataException("unclosed parenthesis");
          }
        }
        else if (line[index] == Octets.SP) {
          index++;
        }
      }

      var bytes = line.ByteArray;

      for (;;) {
        if (bytes[index] == Octets.SP)
          // skip SP
          index = line.IndexOfNot(Octets.SP, index);

        switch (bytes[index]) {
          case ImapOctets.DQuote: {
            // quoted
            c.AddData(ImapData.CreateTextData(ParseQuotedString(line, ref index)));
            break;
          }

          case ImapOctets.OpenParen: {
            // parenthesized begin
            c.OpenParenthesis();
            index++; // '('
            break;
          }

          case ImapOctets.CloseParen: {
            // parenthesized end
            c.CloseParenthesis();
            index++; // ')'
            break;
          }

          case ImapOctets.OpenBrace:
          case ImapOctets.Tilde: {
            // literal/literal8
            ImapData parsed;

            var isLiteral = ParseLiteral((bytes[index] == ImapOctets.Tilde), line, ref index, out parsed);

            c.AddData(parsed);

            if (isLiteral)
              return null; // incomplete
            else
              break;
          }

          default: {
            // non-quoted
            var text = ParseNonQuotedString(line, ref index);

            if (nil.EqualsIgnoreCase(text))
              c.AddData(ImapData.CreateNilData());
            else
              c.AddData(ImapData.CreateTextData(text));

            break;
          }
        }

        if (line.Length - index == 2/*CRLF*/) {
          if (c.ParenthesesClosed) {
            // end of data
            context = null;
            return c.GetParsed();
          }
          else {
            return null; // incomplete
          }
        }
        else {
          if (bytes[index] == Octets.SP)
            index++;
        }
      } // for
    }

    private bool ParseLiteral(bool literal8, ByteString line, ref int index, out ImapData parsed)
    {
      // literal         = "{" number "}" CRLF *CHAR8
      //                ; Number represents the number of CHAR8s
      // CHAR8           = %x01-ff ; any OCTET except NUL, %x00
      // number          = 1*DIGIT
      //                     ; Unsigned 32-bit integer
      //                     ; (0 <= n < 4,294,967,296)
      var bytes = line.ByteArray;

      if (literal8) {
        /*
         * RFC 3516 - IMAP4 Binary Content Extension
         * http://tools.ietf.org/html/rfc3516
         * 4.2. FETCH Command Extensions
         *    literal8       =   "~{" number "}" CRLF *OCTET
         *                       ; <number> represents the number of OCTETs
         *                       ; in the response string.
         */
        if (bytes[index + 1] != ImapOctets.OpenBrace) {
          // not literal8
          parsed = ImapData.CreateTextData(ParseNonQuotedString(line, ref index));
          return false;
        }

        index++; // '~'
      }

      index++; // '{'

      var number = 0L;

      for (;;) {
        var num = bytes[index] - 0x30; // '0'

        if (0 <= num && num <= 9)
          // number
          number = checked((number * 10L) + num);
        else if (bytes[index] == ImapOctets.CloseBrace)
          break; // '}'
        else
          throw new ImapMalformedDataException("malformed literal (invalid number)");

        index++;
      }

      if (bytes.Length - ++index != 2/*CRLF*/)
        throw new ImapMalformedDataException("malformed literal (extra data before CRLF)");

      /*
       * to avoid allocating buffers on Large Object Heap
       * 
       * http://msdn.microsoft.com/en-us/magazine/cc534993.aspx
       * http://tirania.org/blog/archive/2009/Nov-09.html
       */
      const long literalAllocationThreshold = 40960L; // 40kBytes

      if (number <= literalAllocationThreshold) {
        var literal = new byte[number];

        if (stream.Read(literal, 0, (int)number) != literal.Length)
          throw new ImapMalformedDataException("unexpected EOF");

        parsed = ImapData.CreateTextData(new ByteString(literal));
      }
      else {
        var literal = new ChunkedMemoryStream();

        if (stream.Read(literal, number) != number)
          throw new ImapMalformedDataException("unexpected EOF");

        literal.Position = 0L;

        parsed = ImapData.CreateTextData(literal);
      }

      return true;
    }

    private ByteString ParseQuotedString(ByteString line, ref int index)
    {
      // quoted          = DQUOTE *QUOTED-CHAR DQUOTE
      // QUOTED-CHAR     = <any TEXT-CHAR except quoted-specials> / 
      //                   "\" quoted-specials
      // TEXT-CHAR       = <any CHAR except CR and LF>
      // CHAR            = <any 7-bit US-ASCII character except NUL,
      //                   0x01 - 0x7f>
      // quoted-specials = DQUOTE / "\"
      // date-time       = DQUOTE date-day-fixed "-" date-month "-" date-year

      index++; // DQUOTE

      var bytes = line.ByteArray;
      var dequoted = new byte[bytes.Length];
      var len = 0;

      for (;;) {
        var escape = (bytes[index] == ImapOctets.BackSlash);

        if (escape)
          index++; // skip quoted-specials

        if (bytes[index] == Octets.CR) {
          return new ByteString(dequoted, 0, len);
        }
        else if (!escape && bytes[index] == ImapOctets.DQuote) {
          index++; // DQUOTE
          return new ByteString(dequoted, 0, len);
        }
        else {
          dequoted[len++] = bytes[index]; // QUOTED-CHAR
        }

        index++;
      }
    }

    private ByteString ParseNonQuotedString(ByteString line, ref int index)
    {
      // atom            = 1*ATOM-CHAR
      // ATOM-CHAR       = <any CHAR except atom-specials>
      // atom-specials   = "(" / ")" / "{" / SP / CTL / list-wildcards /
      //                   quoted-specials / resp-specials
      // number          = 1*DIGIT
      // list-wildcards  = "%" / "*"
      // quoted-specials = DQUOTE / "\"
      // resp-specials   = "]"

      // section         = "[" [section-spec] "]"
      // section-msgtext = "HEADER" / "HEADER.FIELDS" [".NOT"] SP header-list /
      //                   "TEXT"
      //                     ; top-level or MESSAGE/RFC822 part
      // section-part    = nz-number *("." nz-number)
      //                     ; body part nesting
      // section-spec    = section-msgtext / (section-part ["." section-text])
      // section-text    = section-msgtext / "MIME"
      //                     ; text other than actual body part (headers, etc.)
      var bytes = line.ByteArray;
      var start = index;

      for (;;) {
        if (bytes[index] == Octets.SP ||
            bytes[index] == Octets.CR ||
            bytes[index] == ImapOctets.CloseParen) {
          return new ByteString(bytes, start, index - start);
        }
        else if (bytes[index] == ImapOctets.OpenBracket) {
          // skip section
          for (;;) {
            if (++index == bytes.Length)
              throw new ImapMalformedDataException("unclosed section bracket");
            else if (bytes[index] == ImapOctets.CloseBracket)
              break;
          }
        }

        index++;
      }
    }
  }
}
