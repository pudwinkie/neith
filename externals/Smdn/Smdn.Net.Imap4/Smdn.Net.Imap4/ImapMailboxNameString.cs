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

using Smdn.Formats;
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4 {
  // string types:
  //   ImapString
  //     => handles 'string'
  //     ImapQuotedString
  //       => handles 'quoted'
  //       ImapMailboxNameString (internal)
  //         => handles 'mailbox name'
  //     ImapLiteralString
  //       => handles 'literal'
  //     ImapNilString
  //       => handles 'NIL'
  //     ImapStringList
  //       => list of ImapString
  //       ImapParenthesizedString
  //         => handles 'parenthesized list'
  //     ImapStringEnum
  //       => string enumeration type
  //     ImapCombinableDataItem
  //       => combinable data item type

  public class ImapMailboxNameString : ImapString {
    public static ImapMailboxNameString CreateListMailboxName(string mailboxName)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return new ImapMailboxNameString(mailboxName,
                                       ImapOctets.IndexOfNonListChar);
    }

    public static ImapMailboxNameString CreateMailboxName(string mailboxName)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");

      return new ImapMailboxNameString(mailboxName,
                                       ImapOctets.IndexOfNonAstringChar);
    }

    public static ImapMailboxNameString CreateMailboxNameNonEmpty(string mailboxName)
    {
      if (mailboxName == null)
        throw new ArgumentNullException("mailboxName");
      if (mailboxName.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("mailboxName");

      return new ImapMailboxNameString(mailboxName,
                                       ImapOctets.IndexOfNonAstringChar);
    }

    private ImapMailboxNameString(string val, Func<string, int> indexOfEscapedChar)
      : base(val)
    {
      this.indexOfEscapedChar = indexOfEscapedChar;
    }

    protected internal override string GetEscaped()
    {
      if (Value.Length == 0)
        return "\"\""; // DQUOTE DQUOTE

      var encoded = ModifiedUTF7.Encode(Value);

      if (0 <= indexOfEscapedChar(encoded))
        return string.Concat('"',
                             encoded.Replace("\\", "\\\\").Replace("\"", "\\\""),
                             '"');
      else
        return encoded;
    }

    private Func<string, int> indexOfEscapedChar;
  }
}
