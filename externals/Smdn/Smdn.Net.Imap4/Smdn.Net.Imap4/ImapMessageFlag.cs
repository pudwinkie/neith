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

namespace Smdn.Net.Imap4 {
  // enum types:
  //   ImapStringEnum : ImapString
  //     => handles string constants
  //     ImapAuthenticationMechanism
  //       => handles authentication mechanisms
  //     ImapCapability
  //       => handles capability constants
  //     ImapCompressionMechanism
  //       => handles COMPRESS extension compression mechanism
  //     ImapMailboxFlag
  //       => handles mailbox flags
  //   * ImapMessageFlag
  //       => handles system flags and keywords
  //     ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  public sealed class ImapMessageFlag : ImapStringEnum, IImapExtension {
    public static readonly ImapMessageFlagList AllFlags;

    internal static readonly ImapMessageFlagList FetchFlags;
    internal static readonly ImapMessageFlagList PermittedFlags;

    /*
     * RFC 3501 INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1
     * http://tools.ietf.org/html/rfc3501
     */
    public static readonly ImapMessageFlag AllowedCreateKeywords = new ImapMessageFlag(@"\*", true);

    public static readonly ImapMessageFlag Seen     = new ImapMessageFlag(@"\Seen", true);
    public static readonly ImapMessageFlag Answered = new ImapMessageFlag(@"\Answered", true);
    public static readonly ImapMessageFlag Flagged  = new ImapMessageFlag(@"\Flagged", true);
    public static readonly ImapMessageFlag Deleted  = new ImapMessageFlag(@"\Deleted", true);
    public static readonly ImapMessageFlag Draft    = new ImapMessageFlag(@"\Draft", true);
    public static readonly ImapMessageFlag Recent   = new ImapMessageFlag(@"\Recent", true);

    static ImapMessageFlag()
    {
      AllFlags = new ImapMessageFlagList(true, GetDefinedConstants<ImapMessageFlag>());

      FetchFlags = new ImapMessageFlagList(true, new[] {
        ImapMessageFlag.Answered,
        ImapMessageFlag.Flagged,
        ImapMessageFlag.Deleted,
        ImapMessageFlag.Seen,
        ImapMessageFlag.Draft,
        ImapMessageFlag.Recent,
      });

      PermittedFlags = new ImapMessageFlagList(true, new[] {
        ImapMessageFlag.Answered,
        ImapMessageFlag.Flagged,
        ImapMessageFlag.Deleted,
        ImapMessageFlag.Seen,
        ImapMessageFlag.Draft,
        ImapMessageFlag.AllowedCreateKeywords,
      });
    }

    internal static ImapMessageFlag GetKnownOrCreate(string flag)
    {
      if (AllFlags.Has(flag))
        return AllFlags[flag];

      /*
      if (flag.StartsWith(@"\"))
        Trace.Verbose("unknown system flag: {0}", flag);
      */

      return new ImapMessageFlag(flag);
    }

    // atom            = 1*ATOM-CHAR
    // ATOM-CHAR       = <any CHAR except atom-specials>
    // atom-specials   = "(" / ")" / "{" / SP / CTL / list-wildcards /
    //                   quoted-specials / resp-specials
    // flag-keyword    = atom
    // list-wildcards  = "%" / "*"
    // quoted-specials = DQUOTE / "\"
    // resp-specials   = "]"
    // CTL ::= <any ASCII control character and DEL,
    //          0x00 - 0x1f, 0x7f>
    internal static readonly char[] InvalidKeywordChars = new[] {
      '(', ')', '{', ' ', '%', '*', '"', '\\', ']',
      '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x0a', '\x0b', '\x0c', '\x0d', '\x0e', '\x0f',
      '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1a', '\x1b', '\x1c', '\x1d', '\x1e', '\x1f',
      '\x7f',
    };

    public static bool IsValidKeyword(string keyword)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");

      string discard;

      return IsValidKeyword(keyword, out discard);
    }

    /// <returns>validated keyword if valid</returns>
    public static string GetValidatedKeyword(string keyword)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");

      string errorReason;

      if (IsValidKeyword(keyword, out errorReason))
        return keyword;
      else
        throw new ArgumentException(errorReason, "keyword");
    }

    private static bool IsValidKeyword(string keyword, out string errorReason)
    {
      errorReason = null;

      if (keyword.Length == 0) {
        errorReason = "keyword must be non-empty string";
        return false;
      }

      var index = keyword.IndexOfAny(InvalidKeywordChars);

      if (0 <= index) {
        errorReason = string.Format("keyword '{0}' contains invalid charactor at {1}", keyword, index);
        return false;
      }

      return true;
    }

    ImapCapability IImapExtension.RequiredCapability {
      get { return requiredCapability; }
    }

    public bool IsSystemFlag {
      get { return base.Value.StartsWith(@"\", StringComparison.Ordinal); }
    }

    public ImapMessageFlag(string keyword)
      : this(keyword, null)
    {
    }

    public ImapMessageFlag(string keyword, ImapCapability requiredCapability)
      : this(GetValidatedKeyword(keyword), true, requiredCapability)
    {
    }

    private ImapMessageFlag(string keyword, bool validated)
      : this(keyword, validated, null)
    {
    }

    private ImapMessageFlag(string keyword, bool validated, ImapCapability requiredCapability)
      : base(keyword)
    {
      this.requiredCapability = requiredCapability;
    }

    private /*readonly*/ ImapCapability requiredCapability;
  }
}