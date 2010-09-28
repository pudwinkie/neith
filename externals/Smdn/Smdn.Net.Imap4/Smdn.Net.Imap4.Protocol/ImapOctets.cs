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
  }
}
