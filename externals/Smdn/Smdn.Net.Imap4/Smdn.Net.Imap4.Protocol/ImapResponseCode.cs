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
  //   * Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  [Serializable]
  public sealed class ImapResponseCode : ImapStringEnum, IEquatable<ImapResponseCode> {
    public static readonly ImapStringEnumSet<ImapResponseCode> AllCodes;

#region "RFC 3501 INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1"
    /*
     * RFC 3501 INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1
     * http://tools.ietf.org/html/rfc3501
     * 
     * 7.1. Server Responses - Status Responses
     */

    /*
     *      ALERT
     *         The human-readable text contains a special alert that MUST be
     *         presented to the user in a fashion that calls the user's
     *         attention to the message.
     */
    public static readonly ImapResponseCode Alert = new ImapResponseCode("ALERT");
    /*
     *      BADCHARSET
     *         Optionally followed by a parenthesized list of charsets.  A
     *         SEARCH failed because the given charset is not supported by
     *         this implementation.  If the optional list of charsets is
     *         given, this lists the charsets that are supported by this
     *         implementation.
     */
    public static readonly ImapResponseCode BadCharset = new ImapResponseCode("BADCHARSET", true);
    /*
     *      CAPABILITY
     *         Followed by a list of capabilities.  This can appear in the
     *         initial OK or PREAUTH response to transmit an initial
     *         capabilities list.  This makes it unnecessary for a client to
     *         send a separate CAPABILITY command if it recognizes this
     *         response.
     */
    public static readonly ImapResponseCode Capability = new ImapResponseCode("CAPABILITY", true);
    /*
     *      PARSE
     *         The human-readable text represents an error in parsing the
     *         [RFC-2822] header or [MIME-IMB] headers of a message in the
     *         mailbox.
     */
    public static readonly ImapResponseCode Parse = new ImapResponseCode("PARSE");
    /*
     *      PERMANENTFLAGS
     *         Followed by a parenthesized list of flags, indicates which of
     *         the known flags the client can change permanently.  Any flags
     *         that are in the FLAGS untagged response, but not the
     *         PERMANENTFLAGS list, can not be set permanently.  If the client
     *         attempts to STORE a flag that is not in the PERMANENTFLAGS
     *         list, the server will either ignore the change or store the
     *         state change for the remainder of the current session only.
     *         The PERMANENTFLAGS list can also include the special flag \*,
     *         which indicates that it is possible to create new keywords by
     *         attempting to store those flags in the mailbox.
     */
    public static readonly ImapResponseCode PermanentFlags = new ImapResponseCode("PERMANENTFLAGS", true);
    /*
     *      READ-ONLY
     *         The mailbox is selected read-only, or its access while selected
     *         has changed from read-write to read-only.
     */
    public static readonly ImapResponseCode ReadOnly = new ImapResponseCode("READ-ONLY");
    /*
     *      READ-WRITE
     *         The mailbox is selected read-write, or its access while
     *         selected has changed from read-only to read-write.
     */
    public static readonly ImapResponseCode ReadWrite = new ImapResponseCode("READ-WRITE");
    /*
     *      TRYCREATE
     *         An APPEND or COPY attempt is failing because the target mailbox
     *         does not exist (as opposed to some other reason).  This is a
     *         hint to the client that the operation can succeed if the
     *         mailbox is first created by the CREATE command.
     */
    public static readonly ImapResponseCode TryCreate = new ImapResponseCode("TRYCREATE");
    /*
     *      UIDNEXT
     *         Followed by a decimal number, indicates the next unique
     *         identifier value.  Refer to section 2.3.1.1 for more
     *         information.
     */
    public static readonly ImapResponseCode UidNext = new ImapResponseCode("UIDNEXT", true);
    /*
     *      UIDVALIDITY
     *         Followed by a decimal number, indicates the unique identifier
     *         validity value.  Refer to section 2.3.1.1 for more information.
     */
    public static readonly ImapResponseCode UidValidity = new ImapResponseCode("UIDVALIDITY", true);
    /*
     *      UNSEEN
     *         Followed by a decimal number, indicates the number of the first
     *         message without the \Seen flag set.
     */
    public static readonly ImapResponseCode Unseen = new ImapResponseCode("UNSEEN", true);
#endregion

#region "RFC 2193 IMAP4 Mailbox Referrals, RFC 2221 IMAP4 Login Referrals"
    /*
     * RFC 2193 IMAP4 Mailbox Referrals
     * http://tools.ietf.org/html/rfc2193
     * 
     * 4.1. SELECT, EXAMINE, DELETE, SUBSCRIBE, UNSUBSCRIBE, STATUS and APPEND
     *      Referrals
     *    An IMAP4 server MAY respond to the SELECT, EXAMINE, DELETE,
     *    SUBSCRIBE, UNSUBSCRIBE, STATUS or APPEND command with one or more
     *    IMAP mailbox referrals to indicate to the client that the mailbox is
     *    hosted on a remote server.
     *
     * 4.2. CREATE Referrals
     *    An IMAP4 server MAY respond to the CREATE command with one or more
     *    IMAP mailbox referrals, if it wishes to direct the client to issue
     *    the CREATE against another server.  The server can employ any means,
     *    such as examining the hierarchy of the specified mailbox name, in
     *    determining which server the mailbox should be created on.
     *
     * 4.3. RENAME Referrals
     *    An IMAP4 server MAY respond to the RENAME command with one or more
     *    pairs of IMAP mailbox referrals.  In each pair of IMAP mailbox
     *    referrals, the first one is an URL to the existing mailbox name and
     *    the second is an URL to the requested new mailbox name.
     *
     * 4.4. COPY Referrals
     *    An IMAP4 server MAY respond to the COPY command with one or more IMAP
     *    mailbox referrals.  This indicates that the destination mailbox is on
     *    a remote server.  To achieve the same behavior of a server COPY, the
     *    client MAY issue the constituent FETCH and APPEND commands against
     *    both servers.
     *
     * 6. Formal Syntax
     *    The following syntax specification uses the augmented Backus-Naur
     *    Form (BNF) as described in [ABNF].
     * 
     *    list_mailbox = <list_mailbox> as defined in [RFC-2060]
     *    mailbox = <mailbox> as defined in [RFC-2060]
     *    mailbox_referral = <tag> SPACE "NO" SPACE
     *       <referral_response_code> (text / text_mime2)
     *       ; See [RFC-2060] for <tag>, text and text_mime2 definition
     *    referral_response_code = "[" "REFERRAL" 1*(SPACE <url>) "]"
     *       ; See [RFC-1738] for <url> definition
     * 
     * RFC 2221 IMAP4 Login Referrals
     * http://tools.ietf.org/html/rfc2221
     * 
     * 4.1. LOGIN and AUTHENTICATE Referrals
     *    An IMAP4 server MAY respond to a LOGIN or AUTHENTICATE command with a
     *    home server referral if it wishes to direct the user to another IMAP4
     *    server.
     * 
     * 4.2. BYE at connection startup referral
     *    An IMAP4 server MAY respond with an untagged BYE and a REFERRAL
     *    response code that contains an IMAP URL to a home server if it is not
     *    willing to accept connections and wishes to direct the client to
     *    another IMAP4 server.
     * 
     * 5. Formal Syntax
     *    resp_text_code =/ "REFERRAL" SPACE <imapurl>
     *       ; See [IMAP-URL] for definition of <imapurl>
     *       ; See [RFC-2060] for base definition of resp_text_code
     */
     public static readonly ImapResponseCode Referral = new ImapResponseCode("REFERRAL", true);
#endregion

#region "RFC 3516 - IMAP4 Binary Content Extension"
    /*
     * RFC 3516 - IMAP4 Binary Content Extension
     * http://tools.ietf.org/html/rfc3516
     * 4.4. APPEND Command Extensions
     *    If the destination mailbox does not support the storage of binary
     *    content, the server MUST fail the request and issue a "NO" response
     *    that contains the "UNKNOWN-CTE" extended response code.
     */
    public static readonly ImapResponseCode UnknownCte = new ImapResponseCode("UNKNOWN-CTE");
#endregion

#region "RFC 4315 Internet Message Access Protocol (IMAP) - UIDPLUS extension"
    /*
     * RFC 4315 Internet Message Access Protocol (IMAP) - UIDPLUS extension
     * http://tools.ietf.org/html/rfc4315
     * 
     * 3. Additional Response Codes
     */

    /*
     *    APPENDUID
     * 
     *       Followed by the UIDVALIDITY of the destination mailbox and the UID
     *       assigned to the appended message in the destination mailbox,
     *       indicates that the message has been appended to the destination
     *       mailbox with that UID.
     * 
     *       If the server also supports the [MULTIAPPEND] extension, and if
     *       multiple messages were appended in the APPEND command, then the
     *       second value is a UID set containing the UIDs assigned to the
     *       appended messages, in the order they were transmitted in the
     *       APPEND command.  This UID set may not contain extraneous UIDs or
     *       the symbol "*".
     * 
     *          Note: the UID set form of the APPENDUID response code MUST NOT
     *          be used if only a single message was appended.  In particular,
     *          a server MUST NOT send a range such as 123:123.  This is
     *          because a client that does not support [MULTIAPPEND] expects
     *          only a single UID and not a UID set.
     * 
     *       UIDs are assigned in strictly ascending order in the mailbox
     *       (refer to [IMAP], section 2.3.1.1) and UID ranges are as in
     *       [IMAP]; in particular, note that a range of 12:10 is exactly
     *       equivalent to 10:12 and refers to the sequence 10,11,12.
     * 
     *       This response code is returned in a tagged OK response to the
     *       APPEND command.
     */
    public static readonly ImapResponseCode AppendUid = new ImapResponseCode("APPENDUID");
    /*
     *    COPYUID
     * 
     *       Followed by the UIDVALIDITY of the destination mailbox, a UID set
     *       containing the UIDs of the message(s) in the source mailbox that
     *       were copied to the destination mailbox and containing the UIDs
     *       assigned to the copied message(s) in the destination mailbox,
     *       indicates that the message(s) have been copied to the destination
     *       mailbox with the stated UID(s).
     * 
     *       The source UID set is in the order the message(s) were copied; the
     *       destination UID set corresponds to the source UID set and is in
     *       the same order.  Neither of the UID sets may contain extraneous
     *       UIDs or the symbol "*".
     * 
     *       UIDs are assigned in strictly ascending order in the mailbox
     *       (refer to [IMAP], section 2.3.1.1) and UID ranges are as in
     *       [IMAP]; in particular, note that a range of 12:10 is exactly
     *       equivalent to 10:12 and refers to the sequence 10,11,12.
     * 
     *       This response code is returned in a tagged OK response to the COPY
     *       command.
     */
    public static readonly ImapResponseCode CopyUid = new ImapResponseCode("COPYUID");
    /* 
     *    UIDNOTSTICKY
     * 
     *       The selected mailbox is supported by a mail store that does not
     *       support persistent UIDs; that is, UIDVALIDITY will be different
     *       each time the mailbox is selected.  Consequently, APPEND or COPY
     *       to this mailbox will not return an APPENDUID or COPYUID response
     *       code.
     * 
     *       This response code is returned in an untagged NO response to the
     *       SELECT command.
     * 
     *          Note: servers SHOULD NOT have any UIDNOTSTICKY mail stores.
     *          This facility exists to support legacy mail stores in which it
     *          is technically infeasible to support persistent UIDs.  This
     *          should be avoided when designing new mail stores.
     */
    public static readonly ImapResponseCode UidNotSticky = new ImapResponseCode("UIDNOTSTICKY");
#endregion

#region "RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization"
    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     * 
     * 3. IMAP Protocol Changes
     */

    /*
     * 3.1.1. HIGHESTMODSEQ Response Code
     * 
     *    This document adds a new response code that is returned in the OK
     *    untagged response for the SELECT and EXAMINE commands.  A server
     *    supporting the persistent storage of mod-sequences for the mailbox
     *    MUST send the OK untagged response including HIGHESTMODSEQ response
     *    code with every successful SELECT or EXAMINE command:
     * 
     *       OK [HIGHESTMODSEQ <mod-sequence-value>]
     * 
     *       where <mod-sequence-value> is the highest mod-sequence value of
     *       all messages in the mailbox.  When the server changes UIDVALIDITY
     *       for a mailbox, it doesn't have to keep the same HIGHESTMODSEQ for
     *       the mailbox.
     * 
     *    A disconnected client can use the value of HIGHESTMODSEQ to check if
     *    it has to refetch metadata from the server.  If the UIDVALIDITY value
     *    has changed for the selected mailbox, the client MUST delete the
     *    cached value of HIGHESTMODSEQ.  If UIDVALIDITY for the mailbox is the
     *    same, and if the HIGHESTMODSEQ value stored in the client's cache is
     *    less than the value returned by the server, then some metadata items
     *    on the server have changed since the last synchronization, and the
     *    client needs to update its cache.  The client MAY use SEARCH MODSEQ
     *    (Section 3.4) to find out exactly which metadata items have changed.
     *    Alternatively, the client MAY issue FETCH with the CHANGEDSINCE
     *    modifier (Section 3.3.1) in order to fetch data for all messages that
     *    have metadata items changed since some known modification sequence.
     */
    public static readonly ImapResponseCode HighestModSeq = new ImapResponseCode("HIGHESTMODSEQ");

    /* 
     * 3.1.2. NOMODSEQ Response Code
     * 
     *    A server that doesn't support the persistent storage of mod-sequences
     *    for the mailbox MUST send the OK untagged response including NOMODSEQ
     *    response code with every successful SELECT or EXAMINE command.  A
     *    server that returned NOMODSEQ response code for a mailbox, which
     *    subsequently receives one of the following commands while the mailbox
     *    is selected:
     * 
     *       -  a FETCH command with the CHANGEDSINCE modifier,
     *       -  a FETCH or SEARCH command that includes the MODSEQ message data
     *          item, or
     *       -  a STORE command with the UNCHANGEDSINCE modifier
     * 
     *    MUST reject any such command with the tagged BAD response.
     */
    public static readonly ImapResponseCode NoModSeq = new ImapResponseCode("NOMODSEQ");

    /*
     * 3.2. STORE and UID STORE Commands
     * 
     *       When the server finished performing the operation on all the
     *       messages in the message set, it checks for a non-empty list of
     *       messages that failed the UNCHANGESINCE test.  If this list is
     *       non-empty, the server MUST return in the tagged response a
     *       MODIFIED response code.  The MODIFIED response code includes the
     *       message set (for STORE) or set of UIDs (for UID STORE) of all
     *       messages that failed the UNCHANGESINCE test.
     */
    public static readonly ImapResponseCode Modified = new ImapResponseCode("MODIFIED");

#endregion

#region "RFC 4978 - The IMAP COMPRESS Extension"
    /*
     * RFC 4978 - The IMAP COMPRESS Extension
     * http://tools.ietf.org/html/rfc4978
     * 
     * 3. The COMPRESS Command
     *   If the server responds NO because it knows that the same mechanism is
     *   active already (e.g., because TLS has negotiated the same mechanism),
     *   it MUST send COMPRESSIONACTIVE as resp-text-code (see [RFC3501],
     *   Section 7.1), and the resp-text SHOULD say which layer compresses.
     */
    public static readonly ImapResponseCode CompressionActive = new ImapResponseCode("COMPRESSIONACTIVE");
#endregion

#region "RFC 5182 - IMAP Extension for Referencing the Last SEARCH Result"
    /*
     * RFC 5182 - IMAP Extension for Referencing the Last SEARCH Result
     * http://tools.ietf.org/html/rfc5182
     */

    /*
     * 2.5. Refusing to Save Search Results
     * 
     *    In some cases, the server MAY refuse to save a SEARCH (SAVE) result,
     *    for example, if an internal limit on the number of saved results is
     *    reached.
     * 
     *    In this case, the server MUST return a tagged NO response containing
     *    the NOTSAVED response code and set the search result variable to the
     *    empty sequence, as described in Section 2.1.
     */
    public static readonly ImapResponseCode NotSaved = new ImapResponseCode("NOTSAVED");
#endregion

#region "RFC 5255 - Internet Message Access Protocol Internationalization"
    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     */

    /*
     * 4.9. BADCOMPARATOR Response Code
     *    This response code SHOULD be returned as a result of server failing
     *    an IMAP command (returning NO), when the server knows that none of
     *    the specified comparators match the requested comparator(s).
     */
    public static readonly ImapResponseCode BadComparator = new ImapResponseCode("BADCOMPARATOR");
#endregion

#region "RFC 5464 - The IMAP METADATA Extension"
    /*
     * RFC 5464 - The IMAP METADATA Extension
     * http://tools.ietf.org/html/rfc5464
     */

    public static readonly ImapResponseCode Metadata = new ImapResponseCode("METADATA", true);

    /*
     * 4.2.1. MAXSIZE GETMETADATA Command Option
     *    If there are any entries with values
     *    larger than the MAXSIZE limit, the server MUST include the METADATA
     *    LONGENTRIES response code in the tagged OK response for the
     *    GETMETADATA command.  The METADATA LONGENTRIES response code returns
     *    the size of the biggest entry value requested by the client that
     *    exceeded the MAXSIZE limit.
     */
    public static readonly ImapResponseCode MetadataLongEntries = new ImapResponseCode("METADATA", "LONGENTRIES", false);

    /*
     * 
     * 4.3. SETMETADATA Command
     *    If the server is unable to set an annotation because the size of its
     *    value is too large, the server MUST return a tagged NO response with
     *    a "[METADATA MAXSIZE NNN]" response code when NNN is the maximum
     *    octet count that it is willing to accept.
     */
    public static readonly ImapResponseCode MetadataMaxSize = new ImapResponseCode("METADATA", "MAXSIZE", true);

    /*
     * 4.3. SETMETADATA Command
     *    If the server is unable to set a new annotation because the maximum
     *    number of allowed annotations has already been reached, the server
     *    MUST return a tagged NO response with a "[METADATA TOOMANY]" response
     *    code.
     */
    public static readonly ImapResponseCode MetadataTooMany = new ImapResponseCode("METADATA", "TOOMANY", false);

    /*
     * 4.3. SETMETADATA Command
     *    If the server is unable to set a new annotation because it does not
     *    support private annotations on one of the specified mailboxes, the
     *    server MUST return a tagged NO response with a "[METADATA NOPRIVATE]"
     *    response code.
     */
    public static readonly ImapResponseCode MetadataNoPrivate = new ImapResponseCode("METADATA", "NOPRIVATE", false);
#endregion

#region "RFC 5464 - The IMAP METADATA Extension"
    /*
     * RFC 5466 - IMAP4 Extension for Named Searches (Filters)
     * http://tools.ietf.org/html/rfc5466
     * 
     * 3.1. FILTER SEARCH Criterion
     *    A reference to a nonexistent or unaccessible (e.g., due to access
     *    control restrictions) filter MUST cause failure of the SEARCH command
     *    with the tagged NO response that includes the UNDEFINED-FILTER
     *    response code followed by the name of the nonexistent/unaccessible
     *    filter.
     */
    public static readonly ImapResponseCode UndefinedFilter = new ImapResponseCode("UNDEFINED-FILTER", true);
#endregion

#region "RFC 5530 - IMAP Response Codes"
    /*
     * RFC 5530 - IMAP Response Codes
     * http://tools.ietf.org/html/rfc5530
     * 
     * 3. Response Codes
     */

    /*
     *    UNAVAILABLE
     *          Temporary failure because a subsystem is down.  For example, an
     *          IMAP server that uses a Lightweight Directory Access Protocol
     *          (LDAP) or Radius server for authentication might use this
     *          response code when the LDAP/Radius server is down.
     * 
     *          C: a LOGIN "fred" "foo"
     *          S: a NO [UNAVAILABLE] User's backend down for maintenance
     */
    public static readonly ImapResponseCode Unavailable = new ImapResponseCode("UNAVAILABLE");
    /*
     *    AUTHENTICATIONFAILED
     *          Authentication failed for some reason on which the server is
     *          unwilling to elaborate.  Typically, this includes "unknown
     *          user" and "bad password".
     * 
     *          This is the same as not sending any response code, except that
     *          when a client sees AUTHENTICATIONFAILED, it knows that the
     *          problem wasn't, e.g., UNAVAILABLE, so there's no point in
     *          trying the same login/password again later.
     * 
     *          C: b LOGIN "fred" "foo"
     *          S: b NO [AUTHENTICATIONFAILED] Authentication failed
     */
    public static readonly ImapResponseCode AuthenticationFailed = new ImapResponseCode("AUTHENTICATIONFAILED");
    /*
     *    AUTHORIZATIONFAILED
     *          Authentication succeeded in using the authentication identity,
     *          but the server cannot or will not allow the authentication
     *          identity to act as the requested authorization identity.  This
     *          is only applicable when the authentication and authorization
     *          identities are different.
     * 
     *          C: c1 AUTHENTICATE PLAIN
     *          [...]
     *          S: c1 NO [AUTHORIZATIONFAILED] No such authorization-ID
     * 
     *          C: c2 AUTHENTICATE PLAIN
     *          [...]
     *          S: c2 NO [AUTHORIZATIONFAILED] Authenticator is not an admin
     */
    public static readonly ImapResponseCode AuthorizationFailed = new ImapResponseCode("AUTHORIZATIONFAILED");
    /*
     *    EXPIRED
     *          Either authentication succeeded or the server no longer had the
     *          necessary data; either way, access is no longer permitted using
     *          that passphrase.  The client or user should get a new
     *          passphrase.
     * 
     *          C: d login "fred" "foo"
     *          S: d NO [EXPIRED] That password isn't valid any more
     */
    public static readonly ImapResponseCode Expired = new ImapResponseCode("EXPIRED");
    /*
     *    PRIVACYREQUIRED
     *          The operation is not permitted due to a lack of privacy.  If
     *          Transport Layer Security (TLS) is not in use, the client could
     *          try STARTTLS (see Section 6.2.1 of [RFC3501]) and then repeat
     *          the operation.
     * 
     *          C: d login "fred" "foo"
     *          S: d NO [PRIVACYREQUIRED] Connection offers no privacy
     * 
     *          C: d select inbox
     *          S: d NO [PRIVACYREQUIRED] Connection offers no privacy
     */
    public static readonly ImapResponseCode PrivacyRequired = new ImapResponseCode("PRIVACYREQUIRED");
    /*
     *    CONTACTADMIN
     *          The user should contact the system administrator or support
     *          desk.
     * 
     *          C: e login "fred" "foo"
     *          S: e OK [CONTACTADMIN]
     */
    public static readonly ImapResponseCode ContactAdmin = new ImapResponseCode("CONTACTADMIN");
    /*
     *    NOPERM
     *          The access control system (e.g., Access Control List (ACL), see
     *          [RFC4314]) does not permit this user to carry out an operation,
     *          such as selecting or creating a mailbox.
     * 
     *          C: f select "/archive/projects/experiment-iv"
     *          S: f NO [NOPERM] Access denied
     */
    public static readonly ImapResponseCode NoPerm = new ImapResponseCode("NOPERM");
    /*
     *    INUSE
     *          An operation has not been carried out because it involves
     *          sawing off a branch someone else is sitting on.  Someone else
     *          may be holding an exclusive lock needed for this operation, or
     *          the operation may involve deleting a resource someone else is
     *          using, typically a mailbox.
     * 
     *          The operation may succeed if the client tries again later.
     * 
     *          C: g delete "/archive/projects/experiment-iv"
     *          S: g NO [INUSE] Mailbox in use
     */
    public static readonly ImapResponseCode InUse = new ImapResponseCode("INUSE");
    /*
     *    EXPUNGEISSUED
     *          Someone else has issued an EXPUNGE for the same mailbox.  The
     *          client may want to issue NOOP soon.  [RFC2180] discusses this
     *          subject in depth.
     * 
     *          C: h search from fred@example.com
     *          S: * SEARCH 1 2 3 5 8 13 21 42
     *          S: h OK [EXPUNGEISSUED] Search completed
     */
    public static readonly ImapResponseCode ExpungeIssued = new ImapResponseCode("EXPUNGEISSUED");
    /*
     *    CORRUPTION
     *          The server discovered that some relevant data (e.g., the
     *          mailbox) are corrupt.  This response code does not include any
     *          information about what's corrupt, but the server can write that
     *          to its logfiles.
     * 
     *          C: i select "/archive/projects/experiment-iv"
     *          S: i NO [CORRUPTION] Cannot open mailbox
     */
    public static readonly ImapResponseCode Corruption = new ImapResponseCode("CORRUPTION");
    /*
     *    SERVERBUG
     *          The server encountered a bug in itself or violated one of its
     *          own invariants.
     * 
     *          C: j select "/archive/projects/experiment-iv"
     *          S: j NO [SERVERBUG] This should not happen
     */
    public static readonly ImapResponseCode ServerBug = new ImapResponseCode("SERVERBUG");
    /*
     *    CLIENTBUG
     *          The server has detected a client bug.  This can accompany all
     *          of OK, NO, and BAD, depending on what the client bug is.
     * 
     *          C: k1 select "/archive/projects/experiment-iv"
     *          [...]
     *          S: k1 OK [READ-ONLY] Done
     *          C: k2 status "/archive/projects/experiment-iv" (messages)
     *          [...]
     *          S: k2 OK [CLIENTBUG] Done
     */
    public static readonly ImapResponseCode ClientBug = new ImapResponseCode("CLIENTBUG");
    /*
     *    CANNOT
     *          The operation violates some invariant of the server and can
     *          never succeed.
     * 
     *          C: l create "///////"
     *          S: l NO [CANNOT] Adjacent slashes are not supported
     */
    public static readonly ImapResponseCode CanNot = new ImapResponseCode("CANNOT");
    /*
     *    LIMIT
     *          The operation ran up against an implementation limit of some
     *          kind, such as the number of flags on a single message or the
     *          number of flags used in a mailbox.
     * 
     *          C: m STORE 42 FLAGS f1 f2 f3 f4 f5 ... f250
     *          S: m NO [LIMIT] At most 32 flags in one mailbox supported
     */
    public static readonly ImapResponseCode Limit = new ImapResponseCode("LIMIT");
    /*
     *    OVERQUOTA
     *          The user would be over quota after the operation.  (The user
     *          may or may not be over quota already.)
     * 
     *          Note that if the server sends OVERQUOTA but doesn't support the
     *          IMAP QUOTA extension defined by [RFC2087], then there is a
     *          quota, but the client cannot find out what the quota is.
     * 
     *          C: n1 uid copy 1:* oldmail
     *          S: n1 NO [OVERQUOTA] Sorry
     * 
     *          C: n2 uid copy 1:* oldmail
     *          S: n2 OK [OVERQUOTA] You are now over your soft quota
     */
    public static readonly ImapResponseCode OverQuota = new ImapResponseCode("OVERQUOTA");
    /*
     *    ALREADYEXISTS
     *          The operation attempts to create something that already exists,
     *          such as when the CREATE or RENAME directories attempt to create
     *          a mailbox and there is already one of that name.
     * 
     *          C: o RENAME this that
     *          S: o NO [ALREADYEXISTS] Mailbox "that" already exists
     */
    public static readonly ImapResponseCode AlreadyExists = new ImapResponseCode("ALREADYEXISTS");
    /*
     *    NONEXISTENT
     *          The operation attempts to delete something that does not exist.
     *          Similar to ALREADYEXISTS.
     * 
     *          C: p RENAME this that
     *          S: p NO [NONEXISTENT] No such mailbox
     */
    public static readonly ImapResponseCode NonExistent = new ImapResponseCode("NONEXISTENT");
#endregion

#region "draft-ietf-morg-list-specialuse-06 - IMAP LIST extension for special-use mailboxes"
    /*
     * draft-ietf-morg-list-specialuse-06 - IMAP LIST extension for special-use mailboxes
     * http://tools.ietf.org/html/draft-ietf-morg-list-specialuse-06
     * 
     * 3. Extension to IMAP CREATE command to set special-use attributes
     */

    /*
     *    If the server can not create a mailbox with the designated special
     *    use defined, for whatever reason, it MUST NOT create the mailbox, and
     *    MUST respond to the CREATE command with a tagged NO response.  If the
     *    reason for the failure is related to the special-use attribute (the
     *    specified special use is not supported or cannot be assigned to the
     *    specified mailbox), the server SHOULD include the new "USEATTR"
     *    response code in the tagged response (see Section 5.3 for an
     *    example).
     */
    public static readonly ImapResponseCode UseAttr = new ImapResponseCode("USEATTR");
#endregion

    static ImapResponseCode()
    {
      AllCodes = CreateDefinedConstantsSet<ImapResponseCode>();
    }

    public static ImapResponseCode GetKnownOrCreate(string code)
    {
      return GetKnownOrCreate(code, null);
    }

    public static ImapResponseCode GetKnownOrCreate(string code, ImapData subcode)
    {
      var c = (subcode == null || subcode.Format != ImapDataFormat.Text)
        ? code
        : string.Concat(code, " ", subcode.GetTextAsString());

      ImapResponseCode respCode;

      if (AllCodes.TryGet(c, out respCode))
        return respCode;
      else if (AllCodes.TryGet(code, out respCode))
        return respCode;
      else
        //Smdn.Net.Imap4.Client.Trace.Verbose("unknown response code: {0}", code);
        return new ImapResponseCode(code);
    }

    public bool HasArguments {
      get { return hasArguments; }
    }

    internal ImapResponseCode(string code)
      : this(code, null, false)
    {
    }

    internal ImapResponseCode(string code, bool hasArguments)
      : this(code, null, hasArguments)
    {
    }

    internal ImapResponseCode(string code, string subcode, bool hasArguments)
      : base(subcode == null ? code : string.Concat(code, " ", subcode))
    {
      if (code == null)
        throw new ArgumentNullException("code");

      this.subcode = subcode;
      this.hasArguments = hasArguments;
    }

#region "equatable"
    public static bool operator == (ImapResponseCode x, ImapResponseCode y)
    {
      if (Object.ReferenceEquals(x, y))
        return true;
      else if ((object)x == null || (object)y == null)
        return false;
      else
        return x.Equals(y);
    }

    public static bool operator != (ImapResponseCode x, ImapResponseCode y)
    {
      return !(x == y);
    }

    public override bool Equals(object obj)
    {
      var respCode = obj as ImapResponseCode;

      if (respCode == null)
        return base.Equals(obj);
      else
        return Equals(respCode);
    }

    public bool Equals(ImapResponseCode other)
    {
      if (other == null)
        return false;

      return (this.subcode == other.subcode) && base.Equals((ImapStringEnum)other);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
#endregion

    private readonly string subcode;
    private readonly bool hasArguments;
  }
}
