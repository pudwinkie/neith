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
  //   * ImapMailboxFlag
  //       => handles mailbox flags
  //     ImapMessageFlag
  //       => handles system flags and keywords
  //     ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  public sealed class ImapMailboxFlag : ImapStringEnum, IImapExtension {
    public static readonly ImapMailboxFlagSet AllFlags;

    /*
     * RFC 3501 INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1
     * http://tools.ietf.org/html/rfc3501
     */
    public static readonly ImapMailboxFlag Marked       = new ImapMailboxFlag(@"\Marked");
    public static readonly ImapMailboxFlag Unmarked     = new ImapMailboxFlag(@"\Unmarked");
    public static readonly ImapMailboxFlag NoInferiors  = new ImapMailboxFlag(@"\Noinferiors");
    public static readonly ImapMailboxFlag NoSelect     = new ImapMailboxFlag(@"\Noselect");

    /*
     * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
     * http://tools.ietf.org/html/rfc5258
     */
    public static readonly ImapMailboxFlag HasChildren    = new ImapMailboxFlag(@"\HasChildren", ImapCapability.ListExtended); // CHILDREN
    public static readonly ImapMailboxFlag HasNoChildren  = new ImapMailboxFlag(@"\HasNoChildren", ImapCapability.ListExtended); // CHILDREN
    public static readonly ImapMailboxFlag NonExistent    = new ImapMailboxFlag(@"\NonExistent", ImapCapability.ListExtended);
    public static readonly ImapMailboxFlag Subscribed     = new ImapMailboxFlag(@"\Subscribed", ImapCapability.ListExtended);
    public static readonly ImapMailboxFlag Remote         = new ImapMailboxFlag(@"\Remote", ImapCapability.ListExtended);

    /*
     * draft-ietf-morg-list-specialuse-06 - IMAP LIST extension for special-use mailboxes
     * http://tools.ietf.org/html/draft-ietf-morg-list-specialuse-06
     * 2. New mailbox attributes identifying special-use mailboxes
     */
    public static readonly ImapMailboxFlag Drafts   = new ImapMailboxFlag(@"\Drafts");
    public static readonly ImapMailboxFlag Flagged  = new ImapMailboxFlag(@"\Flagged");
    public static readonly ImapMailboxFlag Junk     = new ImapMailboxFlag(@"\Junk");
    public static readonly ImapMailboxFlag Sent     = new ImapMailboxFlag(@"\Sent");
    public static readonly ImapMailboxFlag Trash    = new ImapMailboxFlag(@"\Trash");
    public static readonly ImapMailboxFlag All      = new ImapMailboxFlag(@"\All");
    public static readonly ImapMailboxFlag Archive  = new ImapMailboxFlag(@"\Archive");

    public static readonly ImapMailboxFlagSet UseFlags = new ImapMailboxFlagSet(true, new[] {
      All, Archive, Drafts, Flagged, Junk, Sent, Trash,
    });

    /*
     * Gimap XLIST
     */
    public static readonly ImapMailboxFlag GimapAllMail = new ImapMailboxFlag(@"\AllMail");
    public static readonly ImapMailboxFlag GimapInbox   = new ImapMailboxFlag(@"\Inbox");
    public static readonly ImapMailboxFlag GimapSpam    = new ImapMailboxFlag(@"\Spam");
    public static readonly ImapMailboxFlag GimapStarred = new ImapMailboxFlag(@"\Starred");

    static ImapMailboxFlag()
    {
      AllFlags = new ImapMailboxFlagSet(true, GetDefinedConstants<ImapMailboxFlag>());
    }

    internal static ImapMailboxFlag GetKnownOrCreate(string flag)
    {
      ImapMailboxFlag f;

      if (AllFlags.TryGet(flag, out f))
        return f;
      else
        //Trace.Verbose("unknown mailbox flag: {0}", flag);
        return new ImapMailboxFlag(flag);
    }

    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (requiredCapability != null)
          yield return requiredCapability;
      }
    }

    internal ImapMailboxFlag(string flag)
      : this(flag, null)
    {
    }

    internal ImapMailboxFlag(string flag, ImapCapability requiredCapability)
      : base(flag)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      this.requiredCapability = requiredCapability;
    }

    private readonly ImapCapability requiredCapability;
  }
}
