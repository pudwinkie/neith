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

namespace Smdn.Net.Imap4.Protocol.Client {
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
  //     ImapMessageFlag
  //       => handles system flags and keywords
  //     ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //   * Protocol.ImapDataResponseType
  //       => handles server response types

  [Serializable]
  public sealed class ImapDataResponseType : ImapStringEnum {
    public static readonly ImapStringEnumSet<ImapDataResponseType> AllTypes;

    public static readonly ImapDataResponseType InvalidOrUnknown = new ImapDataResponseType("\r\n"); // XXX

    /*
     * RFC 3501 INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1
     * http://tools.ietf.org/html/rfc3501
     */
    // 7.2. Server Responses - Server and Mailbox Status
    // 7.2.1. CAPABILITY Response
    public static readonly ImapDataResponseType Capability  = new ImapDataResponseType("CAPABILITY");
    // 7.2.2. LIST Response
    public static readonly ImapDataResponseType List        = new ImapDataResponseType("LIST");
    // 7.2.3. LSUB Response
    public static readonly ImapDataResponseType Lsub        = new ImapDataResponseType("LSUB");
    // 7.2.4 STATUS Response
    public static readonly ImapDataResponseType Status      = new ImapDataResponseType("STATUS");
    // 7.2.5. SEARCH Response
    public static readonly ImapDataResponseType Search      = new ImapDataResponseType("SEARCH");
    // 7.2.6. FLAGS Response
    public static readonly ImapDataResponseType Flags       = new ImapDataResponseType("FLAGS");

    // 7.3. Server Responses - Mailbox Size
    // 7.3.1. EXISTS Response
    public static readonly ImapDataResponseType Exists      = new ImapDataResponseType("EXISTS");
    // 7.3.2. RECENT Response
    public static readonly ImapDataResponseType Recent      = new ImapDataResponseType("RECENT");

    // 7.4. Server Responses - Message Status
    // 7.4.1. EXPUNGE Response
    public static readonly ImapDataResponseType Expunge     = new ImapDataResponseType("EXPUNGE");
    // 7.4.2. FETCH Response
    public static readonly ImapDataResponseType Fetch       = new ImapDataResponseType("FETCH");

    internal static readonly ImapStringEnumSet<ImapDataResponseType> SizeStatusTypes
      = new ImapStringEnumSet<ImapDataResponseType>(new[] {
        ImapDataResponseType.Exists,
        ImapDataResponseType.Recent,
        ImapDataResponseType.Expunge,
        ImapDataResponseType.Fetch,
      });

    /*
     * RFC 2087 - IMAP4 QUOTA extension
     * http://tools.ietf.org/html/rfc2087
     */
    // 5.1. QUOTA Response
    public static readonly ImapDataResponseType Quota = new ImapDataResponseType("QUOTA");

    // 5.2. QUOTAROOT Response
    public static readonly ImapDataResponseType QuotaRoot = new ImapDataResponseType("QUOTAROOT");

    /*
     * RFC 2342 IMAP4 Namespace
     * http://tools.ietf.org/html/rfc2342
     */
    // 5. NAMESPACE Command
    public static readonly ImapDataResponseType Namespace = new ImapDataResponseType("NAMESPACE");

    /*
     * RFC 2971 IMAP4 ID extension
     * http://tools.ietf.org/html/rfc2971
     */
    // 3.2. ID Response
    public static readonly ImapDataResponseType Id = new ImapDataResponseType("ID");

    /*
     * RFC 4466 - Collected Extensions to IMAP4 ABNF
     * http://tools.ietf.org/html/rfc4466
     */
    // 2.6.2. ESEARCH untagged response
    public static readonly ImapDataResponseType ESearch = new ImapDataResponseType("ESEARCH");

    /*
     * RFC 5161 The IMAP ENABLE Extension
     * http://tools.ietf.org/html/rfc5161
     */
    // 3.2. The ENABLED Response
    public static readonly ImapDataResponseType Enabled = new ImapDataResponseType("ENABLED");

    /*
     * RFC 5256 Internet Message Access Protocol - SORT and THREAD Extensions
     * http://tools.ietf.org/html/rfc5256
     */
    // BASE.7.2.THREAD. THREAD Response
    // BASE.7.2.SORT. SORT Response
    public static readonly ImapDataResponseType Thread  = new ImapDataResponseType("THREAD");
    public static readonly ImapDataResponseType Sort    = new ImapDataResponseType("SORT");

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     */
    // 3.3. LANGUAGE Response
    public static readonly ImapDataResponseType Language = new ImapDataResponseType("LANGUAGE");
    // 4.8. COMPARATOR Response
    public static readonly ImapDataResponseType Comparator = new ImapDataResponseType("COMPARATOR");

    /*
     * RFC 5464 - The IMAP METADATA Extension
     * http://tools.ietf.org/html/rfc5464
     */
    // 4.4. METADATA Response
    public static readonly ImapDataResponseType Metadata = new ImapDataResponseType("METADATA");

    /*
     * Gimap XLIST capability extension
     */
    public static readonly ImapDataResponseType XList = new ImapDataResponseType("XLIST");

    static ImapDataResponseType()
    {
      AllTypes = CreateDefinedConstantsSet<ImapDataResponseType>();
    }

    internal ImapDataResponseType(string type)
      : base(type)
    {
      if (type == null)
        throw new ArgumentNullException("type");
    }
  }
}