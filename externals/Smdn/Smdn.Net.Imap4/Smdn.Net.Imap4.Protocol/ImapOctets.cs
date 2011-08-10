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

namespace Smdn.Net.Imap4.Protocol {
  public static class ImapOctets {
    public const byte DQuote        = 0x22; // '"'
    public const byte OpenParen     = 0x28; // '('
    public const byte CloseParen    = 0x29; // ')'
    public const byte OpenBrace     = 0x7b; // '{'
    public const byte CloseBrace    = 0x7d; // '}'
    public const byte OpenBracket   = 0x5b; // '['
    public const byte CloseBracket  = 0x5d; // ']'
    public const byte BackSlash     = 0x5c; // '\'
    public const byte Asterisk      = 0x2a; // '*'
    public const byte Plus          = 0x2b; // '+'
    public const byte Tilde         = 0x7e; // '~'

    // atom            = 1*ATOM-CHAR
    // ATOM-CHAR       = <any CHAR except atom-specials>
    // atom-specials   = "(" / ")" / "{" / SP / CTL / list-wildcards /
    //                   quoted-specials / resp-specials
    // list-wildcards  = "%" / "*"
    // quoted-specials = DQUOTE / "\"
    // resp-specials   = "]"
    // CTL ::= <any ASCII control character and DEL,
    //          0x00 - 0x1f, 0x7f>
    private static readonly char[] atomCharExceptions = new[] {
      '(', ')', '{', ' ',
      '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
      '\x08', '\x09', '\x0a', '\x0b', '\x0c', '\x0d', '\x0e', '\x0f',
      '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
      '\x18', '\x19', '\x1a', '\x1b', '\x1c', '\x1d', '\x1e', '\x1f',
      '\x7f',
      '%', '*', // list-wildcards
      '"', '\\', // quoted-specials
      ']', // resp-specials
    };

    // astring         = 1*ASTRING-CHAR / string
    // ASTRING-CHAR    = ATOM-CHAR / resp-specials
    private static readonly char[] astringCharExceptions = new[] {
      '(', ')', '{', ' ',
      '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
      '\x08', '\x09', '\x0a', '\x0b', '\x0c', '\x0d', '\x0e', '\x0f',
      '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
      '\x18', '\x19', '\x1a', '\x1b', '\x1c', '\x1d', '\x1e', '\x1f',
      '\x7f',
      '%', '*', // list-wildcards
      '"', '\\', // quoted-specials
    };

    // list-char       = ATOM-CHAR / list-wildcards / resp-specials
    private static readonly char[] listCharExceptions = new[] {
      '(', ')', '{', ' ',
      '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
      '\x08', '\x09', '\x0a', '\x0b', '\x0c', '\x0d', '\x0e', '\x0f',
      '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
      '\x18', '\x19', '\x1a', '\x1b', '\x1c', '\x1d', '\x1e', '\x1f',
      '\x7f',
      '"', '\\', // quoted-specials
    };

    // atom            = 1*ATOM-CHAR
    public static int IndexOfNonAtomChar(string str)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      return str.IndexOfAny(atomCharExceptions);
    }

    // mailbox         = "INBOX" / astring
    public static int IndexOfNonAstringChar(string str)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      return str.IndexOfAny(astringCharExceptions);
    }

    // list-mailbox    = 1*list-char / string
    public static int IndexOfNonListChar(string str)
    {
      if (str == null)
        throw new ArgumentNullException("str");

      return str.IndexOfAny(listCharExceptions);
    }
  }
}
