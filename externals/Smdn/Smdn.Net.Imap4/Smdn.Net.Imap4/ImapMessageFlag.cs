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

using Smdn.Net.Imap4.Protocol;

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
    public static readonly ImapMessageFlagSet AllFlags;

    internal static readonly ImapMessageFlagSet FetchFlags;
    internal static readonly ImapMessageFlagSet PermittedFlags;
    internal static readonly ImapMessageFlagSet NonApplicableFlags;

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
      AllFlags = new ImapMessageFlagSet(true, GetDefinedConstants<ImapMessageFlag>());

      FetchFlags = new ImapMessageFlagSet(true, new[] {
        ImapMessageFlag.Answered,
        ImapMessageFlag.Flagged,
        ImapMessageFlag.Deleted,
        ImapMessageFlag.Seen,
        ImapMessageFlag.Draft,
        ImapMessageFlag.Recent,
      });

      PermittedFlags = new ImapMessageFlagSet(true, new[] {
        ImapMessageFlag.Answered,
        ImapMessageFlag.Flagged,
        ImapMessageFlag.Deleted,
        ImapMessageFlag.Seen,
        ImapMessageFlag.Draft,
        ImapMessageFlag.AllowedCreateKeywords,
      });

      NonApplicableFlags = new ImapMessageFlagSet(true, new[] {
        ImapMessageFlag.Recent,
        ImapMessageFlag.AllowedCreateKeywords,
      });
    }

    internal static ImapMessageFlag GetKnownOrCreate(string flag)
    {
      ImapMessageFlag f;

      if (AllFlags.TryGet(flag, out f))
        return f;

      /*
      if (flag.StartsWith(@"\"))
        Trace.Verbose("unknown system flag: {0}", flag);
      */

      return new ImapMessageFlag(flag);
    }

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
      /*
       * flag            = "\Answered" / "\Flagged" / "\Deleted" /
       *                   "\Seen" / "\Draft" / flag-keyword / flag-extension
       * flag-extension  = "\" atom
       * flag-keyword    = atom
       */
      errorReason = null;

      if (keyword.Length == 0) {
        errorReason = "keyword must be non-empty string";
        return false;
      }

      var index = ImapOctets.IndexOfNonAtomChar(keyword);

      if (0 <= index) {
        errorReason = string.Format("keyword '{0}' contains invalid charactor at {1}", keyword, index);
        return false;
      }

      return true;
    }

    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (requiredCapability != null)
          yield return requiredCapability;
      }
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

    private readonly ImapCapability requiredCapability;
  }
}