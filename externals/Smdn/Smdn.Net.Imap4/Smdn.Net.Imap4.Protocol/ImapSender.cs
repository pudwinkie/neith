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
using System.Collections;
using System.Collections.Generic;

using Smdn.Formats;

namespace Smdn.Net.Imap4.Protocol {
  public abstract class ImapSender {
    internal protected int UnsentFragments {
      get { return fragments.Count; }
    }

    protected LineOrientedBufferedStream Stream {
      get { return stream; }
    }

    protected ImapSender(LineOrientedBufferedStream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      this.stream = stream;
    }

    public void Send()
    {
      for (;;) {
        if (fragments.Count == 0)
          return;

        var fragment = fragments.Dequeue();

        var bytes = fragment as byte[];

        if (bytes == null) {
          var s = fragment as Stream;

          if (s == null /* || fragment == null*/)
            return; // wait continuation
          else
            stream.Write(s);
        }
        else {
          stream.WriteToCrLf(bytes);
        }
      }
    }

    protected void ClearUnsent()
    {
      fragments.Clear();
    }

    protected void Enqueue(byte[] bytes)
    {
      fragments.Enqueue(bytes);
    }

    private static readonly byte[] BytesDelimiter = new[] {Octets.SP};
    private static readonly byte[] BytesOpenParen = new[] {ImapOctets.OpenParen};
    private static readonly byte[] BytesCloseParen = new[] {ImapOctets.CloseParen};

    protected void Enqueue(IEnumerable<ImapString> args)
    {
      var delim = false;

      foreach (var arg in args) {
        if (delim)
          fragments.Enqueue(BytesDelimiter); // SP
        else
          delim = true;

        var str = (arg is ImapCombinableDataItem) ? (arg as ImapCombinableDataItem).InternalGetCombined() : arg;

        if (str is ImapStringList) {
          var parenthesize = (str is ImapParenthesizedString);

          if (parenthesize)
            fragments.Enqueue(BytesOpenParen);

          Enqueue(str as ImapStringList);

          if (parenthesize)
            fragments.Enqueue(BytesCloseParen);
        }
        else {
          if (str is ImapQuotedString) {
            // quoted          = DQUOTE *QUOTED-CHAR DQUOTE
            var quoted = string.Concat("\"",
                                       (str as ImapQuotedString).Escaped,
                                       "\"");

            fragments.Enqueue(NetworkTransferEncoding.Transfer7Bit.GetBytes(quoted));
          }
          else if (str is IImapLiteralString) {
            /*
             * RFC 3501 - INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1
             * http://tools.ietf.org/html/rfc3501
             * 
             *    literal         = "{" number "}" CRLF *CHAR8
             *                        ; Number represents the number of CHAR8s
             * 
             * IMAP4 non-synchronizing literals
             * http://tools.ietf.org/html/rfc2088
             * 
             * RFC 3516 - IMAP4 Binary Content Extension
             * http://tools.ietf.org/html/rfc3516
             * 
             * RFC 4466 - Collected Extensions to IMAP4 ABNF
             * http://tools.ietf.org/html/rfc4466
             *
             *    literal8        = "~{" number ["+"] "}" CRLF *OCTET
             *                       ;; A string that might contain NULs.
             *                       ;; <number> represents the number of OCTETs
             *                       ;; in the response string.
             *                       ;; The "+" is only allowed when both LITERAL+ and
             *                       ;; BINARY extensions are supported by the server.
             */
            var literal = str as IImapLiteralString;
            var stream = literal.GetLiteralStream();

            var nonSync = (literal.Options & ImapLiteralOptions.SynchronizationMode) == ImapLiteralOptions.NonSynchronizing;
            var literal8 = (literal.Options & ImapLiteralOptions.LiteralMode) == ImapLiteralOptions.Literal8;
            var literalSizeString = string.Concat(literal8 ? "~{" : "{",
                                                  stream.Length.ToString(),
                                                  nonSync ? "+}\x0d\x0a" : "}\x0d\x0a");

            fragments.Enqueue(NetworkTransferEncoding.Transfer7Bit.GetBytes(literalSizeString));

            if (!nonSync)
              fragments.Enqueue(null); // wait continuation

            // *CHAR8 / *OCTET
            fragments.Enqueue(stream);
          }
          else if (str is ImapPreformattedString) {
            fragments.Enqueue((str as ImapPreformattedString).Octets);
          }
          else {
            // atom            = 1*ATOM-CHAR
            fragments.Enqueue(NetworkTransferEncoding.Transfer7Bit.GetBytes((string)str));
          }
        }
      }
    }

    private /*readonly*/ LineOrientedBufferedStream stream;
    private /*readonly*/ Queue fragments = new Queue();
  }
}