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

using Smdn.Security.Authentication.Sasl;

namespace Smdn.Net.Imap4 {
  // enum types:
  //   ImapStringEnum : ImapString
  //     => handles string constants
  //     ImapAuthenticationMechanism
  //       => handles authentication mechanisms
  //   * ImapCapability
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
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  public class ImapCapability : ImapStringEnum {
    public static readonly ImapCapabilityList AllCapabilities;

    /*
     * Internet Message Access Protocol (IMAP) 4 Capabilities Registry
     * http://www.iana.org/assignments/imap4-capabilities
     */
    public static readonly ImapCapability /* [RFC2087] IMAP4 QUOTA extension */
                                      Quota = new ImapCapability("QUOTA");
    public static readonly ImapCapability /* [RFC2088] IMAP4 non-synchronizing literals */
                                      LiteralNonSync = new ImapCapability("LITERAL+");
    public static readonly ImapCapability /* [RFC2177] IMAP4 IDLE command */
                                      Idle = new ImapCapability("IDLE");
    public static readonly ImapCapability /* [RFC2193] IMAP4 Mailbox Referrals */
                                      MailboxReferrals = new ImapCapability("MAILBOX-REFERRALS");
    public static readonly ImapCapability /* [RFC2221] IMAP4 Login Referrals */
                                      LoginReferrals = new ImapCapability("LOGIN-REFERRALS");
    public static readonly ImapCapability /* [RFC2342] IMAP4 Namespace */
                                      Namespace = new ImapCapability("NAMESPACE");
    public static readonly ImapCapability /* [RFC2595] Using TLS with IMAP, POP3 and ACAP */
                                      StartTls = new ImapCapability("STARTTLS");
    public static readonly ImapCapability /* [RFC2595] Using TLS with IMAP, POP3 and ACAP */
                                      LoginDisabled = new ImapCapability("LOGINDISABLED");
    public static readonly ImapCapability /* [RFC2971] IMAP4 ID extension */
                                      ID = new ImapCapability("ID");
    public static readonly ImapCapability /* [RFC3348] The Internet Message Action Protocol (IMAP4) Child Mailbox Extension */
                                      Children = new ImapCapability("CHILDREN");
    public static readonly ImapCapability /* [RFC3501] INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1 */
                                      Imap4Rev1 = new ImapCapability("IMAP4rev1");
    public static readonly ImapCapability /* [RFC3502] Internet Message Access Protocol (IMAP) - MULTIAPPEND Extension */
                                      MultiAppend = new ImapCapability("MULTIAPPEND");
    public static readonly ImapCapability /* [RFC3516] IMAP4 Binary Content Extension */
                                      Binary = new ImapCapability("BINARY");
    public static readonly ImapCapability /* [RFC3691] Internet Message Access Protocol (IMAP) UNSELECT command */
                                      Unselect = new ImapCapability("UNSELECT");
    public static readonly ImapCapability /* [RFC4314] IMAP4 ACL extension */
                                      Acl = new ImapCapability("ACL");
    //public static readonly ImapCapability /* [RFC4314] IMAP4 ACL extension */
    //                                  Rights(1*LOWER-ALPHA) = new ImapCapability("RIGHTS=");
    public static readonly ImapCapability /* [RFC4315] Internet Message Access Protocol (IMAP) - UIDPLUS extension */
                                      UidPlus = new ImapCapability("UIDPLUS");
    public static readonly ImapCapability /* [RFC4467] Internet Message Access Protocol (IMAP) - URLAUTH Extension */
                                      UrlAuth = new ImapCapability("URLAUTH");
    public static readonly ImapCapability /* [RFC4469] Internet Message Access Protocol (IMAP) CATENATE Extension */
                                      Catenate = new ImapCapability("CATENATE");
    public static readonly ImapCapability /* [RFC4551] IMAP Extension for Conditional STORE operation or Quick Flag Changes Resynchronization */
                                      CondStore = new ImapCapability("CONDSTORE");
    public static readonly ImapCapability /* [RFC4731] IMAP4 Extension to SEARCH Command for Controlling What Kind of Information Is Returned */
                                      ESearch = new ImapCapability("ESEARCH");
    public static readonly ImapCapability /* [RFC4959] IMAP Extension for SASL Initial Client Response */
                                      SaslIR = new ImapCapability("SASL-IR");
    public static readonly ImapCapability /* [RFC4978] The IMAP COMPRESS Extension */
                                      CompressDeflate = new ImapCapability("COMPRESS=DEFLATE");
    public static readonly ImapCapability /* [RFC5032] WITHIN Search extension to the IMAP Protocol */
                                      Within = new ImapCapability("WITHIN");
    public static readonly ImapCapability /* [RFC5161] The IMAP ENABLE Extension */
                                      Enable = new ImapCapability("ENABLE");
    public static readonly ImapCapability /* [RFC5162] IMAP4 Extensions for Quick Mailbox Resynchronization */
                                      QuickResync = new ImapCapability("QRESYNC");
    public static readonly ImapCapability /* [RFC5182] IMAP extension for referencing the last SEARCH result */
                                      Searchres = new ImapCapability("SEARCHRES");
    public static readonly ImapCapability /* [RFC5255] Internet Message Access Protocol Internationalization */
                                      I18NLevel1 = new ImapCapability("I18NLEVEL=1");
    public static readonly ImapCapability /* [RFC5255] Internet Message Access Protocol Internationalization */
                                      I18NLevel2 = new ImapCapability("I18NLEVEL=2");
    public static readonly ImapCapability /* [RFC5255] Internet Message Access Protocol Internationalization */
                                      Language = new ImapCapability("LANGUAGE");
    public static readonly ImapCapability /* [RFC5256] Internet Message Access Protocol - SORT and THREAD Extensions */
                                      Sort = new ImapCapability("SORT");
    public static readonly ImapCapability /* [RFC5256] Internet Message Access Protocol - SORT and THREAD Extensions */
                                      ThreadOrderedSubject = new ImapCapability("THREAD=ORDEREDSUBJECT");
    public static readonly ImapCapability /* [RFC5256] Internet Message Access Protocol - SORT and THREAD Extensions */
                                      ThreadReferences = new ImapCapability("THREAD=REFERENCES");
    public static readonly ImapCapability /* [RFC5257] Internet Message Access Protocol - ANNOTATE Extension */
                                      AnnotateExperiment1 = new ImapCapability("ANNOTATE-EXPERIMENT-1");
    public static readonly ImapCapability /* [RFC5258] Internet Message Access Protocol version 4 - LIST Command Extensions */
                                      ListExtended = new ImapCapability("LIST-EXTENDED");
    public static readonly ImapCapability /* [RFC5259] Internet Message Access Protocol - CONVERT Extension */
                                      Convert = new ImapCapability("CONVERT");
    public static readonly ImapCapability /* [RFC5267] Contexts for IMAP4 */
                                      ContextSearch = new ImapCapability("CONTEXT=SEARCH");
    public static readonly ImapCapability /* [RFC5267] Contexts for IMAP4 */
                                      ContextSort = new ImapCapability("CONTEXT=SORT");
    public static readonly ImapCapability /* [RFC5267] Contexts for IMAP4 */
                                      ESort = new ImapCapability("ESORT");
    public static readonly ImapCapability /* [RFC5464] The IMAP METADATA Extension */
                                      Metadata = new ImapCapability("METADATA");
    public static readonly ImapCapability /* [RFC5464] The IMAP METADATA Extension */
                                      MetadataServer = new ImapCapability("METADATA-SERVER");
    public static readonly ImapCapability /* [RFC5465] The IMAP NOTIFY Extension */
                                      Notify = new ImapCapability("NOTIFY");
    public static readonly ImapCapability /* [RFC5466] IMAP4 extension for named searches (filters) */
                                      Filters = new ImapCapability("FILTERS");
    public static readonly ImapCapability /* [RFC5524] xtended URLFETCH for Binary and Converted Parts */
                                      UrlAuthBinary = new ImapCapability("URLAUTH=BINARY");
    public static readonly ImapCapability /* [RFC5738] IMAP Support for UTF-8 */
                                      UTF8Accept = new ImapCapability("UTF8=ACCEPT");
    public static readonly ImapCapability /* [RFC5738] IMAP Support for UTF-8 */
                                      UTF8All = new ImapCapability("UTF8=ALL");
    public static readonly ImapCapability /* [RFC5738] IMAP Support for UTF-8 */
                                      UTF8Append = new ImapCapability("UTF8=APPEND");
    public static readonly ImapCapability /* [RFC5738] IMAP Support for UTF-8 */
                                      UTF8Only = new ImapCapability("UTF8=ONLY");
    public static readonly ImapCapability /* [RFC5738] IMAP Support for UTF-8 */
                                      UTF8User = new ImapCapability("UTF8=USER");
    public static readonly ImapCapability /* [RFC5819] IMAP4 Extension for Returning STATUS Information in Extended LIST */
                                      ListStatus = new ImapCapability("LIST-STATUS");

    /*
     * obsolete
     */
    public static readonly ImapCapability /* [RFC1730] INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4 */
                                      Imap4 = new ImapCapability("IMAP4");

    /*
     * draft
     */
    public static readonly ImapCapability /* [draft-ietf-morg-inthread-00] The IMAP SEARCH=INTHREAD and THREAD=REFS Extensions */
                                      SearchInThread = new ImapCapability("SEARCH=INTHREAD");
    public static readonly ImapCapability /* [draft-ietf-morg-inthread-00] The IMAP SEARCH=INTHREAD and THREAD=REFS Extensions) */
                                      ThreadRefs = new ImapCapability("THREAD=REFS");
    public static readonly ImapCapability /* [draft-ietf-morg-list-specialuse-01] IMAP LIST extension for special-use mailboxes */
                                      CreateSpecialUse = new ImapCapability("CREATE-SPECIAL-USE");
    public static readonly ImapCapability /* [draft-ietf-morg-sortdisplay-03] Display-based Address Sorting for the IMAP4 SORT Extension */
                                      SortDisplay = new ImapCapability("SORT=DISPLAY");

    /*
     * extended capabilities
     */
    public static readonly ImapCapability /* Gimap XLIST capability extension */
                                      GimapXlist = new ImapCapability("XLIST");
    public static readonly ImapCapability /* Gimap X-GM-EXT-1 capability extension */
                                      GimapGmExt1 = new ImapCapability("X-GM-EXT-1");
    public static readonly ImapCapability /* Gimap XYZZY capability extension */
                                      GimapXyzzy = new ImapCapability("XYZZY");

    public static readonly ImapCapability /* UW imapd SCAN capability extension (http://mailman2.u.washington.edu/pipermail/imap-protocol/2007-May/000532.html) */
                                      UWImapdScan = new ImapCapability("SCAN");

    public static readonly ImapCapability /* Netscape extension (http://www.phwinfo.com/forum/comp-mail-imap/140122-x-netscape-what-imap-server.html) */
                                      NetscapeExtension = new ImapCapability("X-NETSCAPE");

    static ImapCapability()
    {
      var capabilities = new List<ImapCapability>(GetDefinedConstants<ImapCapability>());

      foreach (var saslMechansim in SaslMechanisms.AllMechanisms) {
        capabilities.Add(new ImapCapability("AUTH=" + saslMechansim));
      }

      AllCapabilities = new ImapCapabilityList(true, capabilities);
    }

    internal static ImapCapability GetKnownOrCreate(string capability)
    {
      if (AllCapabilities.Has(capability))
        return AllCapabilities[capability];
      else
        //Trace.Verbose("unknown capability: {0}", capability);
        return new ImapCapability(capability);
    }

    public ImapCapability(string val)
      : base(val)
    {
    }
  }
}
