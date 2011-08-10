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
  //   * ImapCollationAlgorithm
  //       => handles COMPARATOR extension comparator order
  //     ImapCompressionMechanism
  //       => handles COMPRESS extension compression mechanism
  //     ImapMailboxFlag
  //       => handles mailbox flags
  //     ImapMessageFlag
  //       => handles system flags and keywords
  //     ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  /*
   * RFC 5255 - Internet Message Access Protocol Internationalization
   * http://tools.ietf.org/html/rfc5255
   */
  public sealed class ImapCollationAlgorithm : ImapStringEnum, IImapExtension {
    public static readonly ImapStringEnumSet<ImapCollationAlgorithm> AllAlgorithms;

    public static readonly ImapCollationAlgorithm Default = new ImapCollationAlgorithm("default");

    /*
     * Collation Registry for Internet Application Protocols
     * http://www.iana.org/assignments/collation/collation-index.html
     */
    public static readonly ImapCollationAlgorithm AsciiNumeric = new ImapCollationAlgorithm("i;ascii-numeric");
    public static readonly ImapCollationAlgorithm AsciiCasemap = new ImapCollationAlgorithm("i;ascii-casemap");
    public static readonly ImapCollationAlgorithm Octet = new ImapCollationAlgorithm("i;octet");
    public static readonly ImapCollationAlgorithm UnicodeCasemap = new ImapCollationAlgorithm("i;unicode-casemap");

    static ImapCollationAlgorithm()
    {
      AllAlgorithms = CreateDefinedConstantsSet<ImapCollationAlgorithm>();
    }

    internal static ImapCollationAlgorithm GetKnownOrCreate(string algorithm)
    {
      ImapCollationAlgorithm algo;

      if (AllAlgorithms.TryGet(algorithm, out algo))
        return algo;
      else
        //Trace.Verbose("unknown collation algorithm: {0}", algorithm);
        return new ImapCollationAlgorithm(algorithm);
    }

    /*
     * instance members
     */
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get { yield return ImapCapability.I18NLevel2; }
    }

    public ImapCollationAlgorithm(string collationAlgorithm)
      : base(collationAlgorithm)
    {
    }
  }
}
