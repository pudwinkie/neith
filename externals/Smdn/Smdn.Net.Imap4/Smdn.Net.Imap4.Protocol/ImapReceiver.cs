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

      return ByteString.CreateImmutable(line);
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

    private static readonly ByteString nil = ByteString.CreateImmutable("NIL");
    private static readonly ByteString crlf = ByteString.CreateImmutable(Octets.GetCRLF());

    protected ImapData[] ParseDataNonTerminatedText(ByteString text, int offset)
    {
      IParsingContext discard = null;

      return ParseData(ByteString.ConcatImmutable(text, crlf), offset, ref discard);
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

      for (;;) {
        if (line[index] == Octets.SP)
          // skip SP
          index = line.IndexOfNot(Octets.SP, index);

        switch (line[index]) {
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

            var isLiteral = ParseLiteral((line[index] == ImapOctets.Tilde), line, ref index, out parsed);

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
          if (line[index] == Octets.SP)
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
      if (literal8) {
        /*
         * RFC 3516 - IMAP4 Binary Content Extension
         * http://tools.ietf.org/html/rfc3516
         * 4.2. FETCH Command Extensions
         *    literal8       =   "~{" number "}" CRLF *OCTET
         *                       ; <number> represents the number of OCTETs
         *                       ; in the response string.
         */
        if (line[index + 1] != ImapOctets.OpenBrace) {
          // not literal8
          parsed = ImapData.CreateTextData(ParseNonQuotedString(line, ref index));
          return false;
        }

        index++; // '~'
      }

      index++; // '{'

      var number = 0L;

      for (;;) {
        var o = line[index];

        if (0x30 <= o && o <= 0x39)
          // number
          number = checked((number * 10L) + (o - 0x30 /* '0' */));
        else if (o == ImapOctets.CloseBrace)
          break; // '}'
        else
          throw new ImapMalformedDataException("malformed literal (invalid number)");

        index++;
      }

      if (line.Length - ++index != 2/*CRLF*/)
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

        parsed = ImapData.CreateTextData(ByteString.CreateImmutable(literal));
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

      var bytes = line.Segment.Array;
      var offset = line.Segment.Offset + index;
      var dequoted = new byte[bytes.Length];
      var len = 0;

      for (;;) {
        var escape = (bytes[offset] == ImapOctets.BackSlash);

        if (escape)
          offset++; // skip quoted-specials

        if (bytes[offset] == Octets.CR) {
          index = (offset - line.Segment.Offset);
          return ByteString.CreateImmutable(dequoted, 0, len);
        }
        else if (!escape && bytes[offset] == ImapOctets.DQuote) {
          index = (offset - line.Segment.Offset) + 1/*DQUOTE*/;
          return ByteString.CreateImmutable(dequoted, 0, len);
        }
        else {
          dequoted[len++] = bytes[offset]; // QUOTED-CHAR
        }

        offset++;
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
      var bytes = line.Segment.Array;
      var offset = line.Segment.Offset + index;
      var start = index;

      for (;;) {
        if (bytes[offset] == Octets.SP ||
            bytes[offset] == Octets.CR ||
            bytes[offset] == ImapOctets.CloseParen) {
          return line.Substring(start, index - start);
        }
        else if (bytes[offset] == ImapOctets.OpenBracket) {
          // skip section
          for (;;) {
            index++;
            offset++;

            if (index == line.Length)
              throw new ImapMalformedDataException("unclosed section bracket");
            else if (bytes[offset] == ImapOctets.CloseBracket)
              break;
          }
        }

        index++;
        offset++;
      }
    }
  }
}
