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
using System.IO;
using System.Collections.Generic;
using System.Text;

using Smdn.Formats;
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4 {
  // combinable data item types:
  //   ImapCombinableDataItem
  //     => handles 'data item names' etc.
  //     ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //   * ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  // 6.4.4. SEARCH Command
  //   searching criteria
  public sealed class ImapSearchCriteria : ImapCombinableDataItem, IImapMultipleExtension, IImapUrlSearchQuery {
    //  <sequence set>
    //     Messages with message sequence numbers corresponding to the
    //     specified message sequence number set.
    public static ImapSearchCriteria SequenceSet(ImapSequenceSet sequenceSet)
    {
      if (sequenceSet == null)
        throw new ArgumentNullException("sequenceSet");
      else if (sequenceSet.IsEmpty)
        throw new ArgumentException("empty set", "sequenceSet");
      else if (!IsSavedResult(sequenceSet) && sequenceSet.IsUidSet)
        throw new ArgumentException("not sequence set", "sequenceSet");

      return new ImapSearchCriteria(sequenceSet.ToString());
    }

    // UID <sequence set>
    //     Messages with unique identifiers corresponding to the specified
    //     unique identifier set.  Sequence set ranges are permitted.
    public static ImapSearchCriteria Uid(ImapSequenceSet uidSet)
    {
      if (uidSet == null)
        throw new ArgumentNullException("uidSet");
      else if (uidSet.IsEmpty)
        throw new ArgumentException("empty set", "uidSet");
      else if (!IsSavedResult(uidSet) && !uidSet.IsUidSet)
        throw new ArgumentException("not uid set", "uidSet");

      return new ImapSearchCriteria("UID", uidSet.ToString());
    }

    private static bool IsSavedResult(ImapSequenceSet sequenceOrUidSet)
    {
      var matchedSet = sequenceOrUidSet as ImapMatchedSequenceSet;

      if (matchedSet == null)
        return false;
      else
        return matchedSet.IsSavedResult;
    }

    public static ImapSearchCriteria SequenceOrUidSet(ImapSequenceSet sequenceOrUidSet)
    {
      if (sequenceOrUidSet == null)
        throw new ArgumentNullException("sequenceOrUidSet");
      else if (sequenceOrUidSet.IsEmpty)
        throw new ArgumentException("empty set", "sequenceOrUidSet");

      if (sequenceOrUidSet.IsUidSet)
        return new ImapSearchCriteria("UID", sequenceOrUidSet.ToString());
      else
        return new ImapSearchCriteria(sequenceOrUidSet.ToString());
    }

    // ALL
    //     All messages in the mailbox; the default initial key for
    //     ANDing.
    public static readonly ImapSearchCriteria All
      = new ImapSearchCriteria("ALL");

    // ANSWERED
    //     Messages with the \Answered flag set.
    public static readonly ImapSearchCriteria Answered
      = new ImapSearchCriteria("ANSWERED");

    // BCC <string>
    //     Messages that contain the specified string in the envelope
    //     structure's BCC field.
    public static ImapSearchCriteria Bcc(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("BCC", new ImapLiteralString(@value));
    }

    // BEFORE <date>
    //     Messages whose internal date (disregarding time and timezone)
    //     is earlier than the specified date.
    public static ImapSearchCriteria Before(DateTime date)
    {
      return new ImapSearchCriteria("BEFORE", ImapDateTimeFormat.ToDateString(date));
    }

    // BODY <string>
    //     Messages that contain the specified string in the body of the
    //     message.
    public static ImapSearchCriteria Body(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("BODY", new ImapLiteralString(@value));
    }

    // CC <string>
    //     Messages that contain the specified string in the envelope
    //     structure's CC field.
    public static ImapSearchCriteria Cc(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("CC", new ImapLiteralString(@value));
    }

    // DELETED
    //     Messages with the \Deleted flag set.
    public static readonly ImapSearchCriteria Deleted
      = new ImapSearchCriteria("DELETED");

    // DRAFT
    //     Messages with the \Draft flag set.
    public static readonly ImapSearchCriteria Draft
      = new ImapSearchCriteria("DRAFT");

    // FLAGGED
    //     Messages with the \Flagged flag set.
    public static readonly ImapSearchCriteria Flagged
      = new ImapSearchCriteria("FLAGGED");

    // FROM <string>
    //     Messages that contain the specified string in the envelope
    //     structure's FROM field.
    public static ImapSearchCriteria From(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("FROM", new ImapLiteralString(@value));
    }

    // HEADER <field-name> <string>
    //     Messages that have a header with the specified field-name (as
    //     defined in [RFC-2822]) and that contains the specified string
    //     in the text of the header (what comes after the colon).  If the
    //     string to search is zero-length, this matches all messages that
    //     have a header line with the specified field-name regardless of
    //     the contents.
    public static ImapSearchCriteria Header(string fieldName, string @value)
    {
      if (fieldName == null)
        throw new ArgumentNullException("fieldName");
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("HEADER", new ImapQuotedString(fieldName), new ImapLiteralString(@value));
    }

    // KEYWORD <flag>
    //     Messages with the specified keyword flag set.
    public static ImapSearchCriteria Keyword(ImapMessageFlag flag)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return new ImapSearchCriteria("KEYWORD", flag.ToString());
    }

    public static ImapSearchCriteria Keyword(string keyword)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");

      return new ImapSearchCriteria("KEYWORD", ImapMessageFlag.GetValidatedKeyword(keyword));
    }

    // LARGER <n>
    //     Messages with an [RFC-2822] size larger than the specified
    //     number of octets.
    public static ImapSearchCriteria Larger(long size)
    {
      if (size < 0L)
        throw new ArgumentOutOfRangeException("size", size, "must be zero or positive number");

      return new ImapSearchCriteria("LARGER", size.ToString());
    }

    // NEW
    //     Messages that have the \Recent flag set but not the \Seen flag.
    //     This is functionally equivalent to "(RECENT UNSEEN)".
    public static readonly ImapSearchCriteria New
      = new ImapSearchCriteria("NEW");

    // OLD
    //     Messages that do not have the \Recent flag set.  This is
    //     functionally equivalent to "NOT RECENT" (as opposed to "NOT
    //     NEW").
    public static readonly ImapSearchCriteria Old
      = new ImapSearchCriteria("OLD");

    // ON <date>
    //     Messages whose internal date (disregarding time and timezone)
    //     is within the specified date.
    public static ImapSearchCriteria On(DateTime date)
    {
      return new ImapSearchCriteria("ON", ImapDateTimeFormat.ToDateString(date));
    }

    // RECENT
    //     Messages that have the \Recent flag set.
    public static readonly ImapSearchCriteria Recent
      = new ImapSearchCriteria("RECENT");

    // SEEN
    //     Messages that have the \Seen flag set.
    public static readonly ImapSearchCriteria Seen
      = new ImapSearchCriteria("SEEN");

    // SENTBEFORE <date>
    //     Messages whose [RFC-2822] Date: header (disregarding time and
    //     timezone) is earlier than the specified date.
    public static ImapSearchCriteria SentBefore(DateTime date)
    {
      return new ImapSearchCriteria("SENTBEFORE", ImapDateTimeFormat.ToDateString(date));
    }

    // SENTON <date>
    //     Messages whose [RFC-2822] Date: header (disregarding time and
    //     timezone) is within the specified date.
    public static ImapSearchCriteria SentOn(DateTime date)
    {
      return new ImapSearchCriteria("SENTON", ImapDateTimeFormat.ToDateString(date));
    }

    // SENTSINCE <date>
    //     Messages whose [RFC-2822] Date: header (disregarding time and
    //     timezone) is within or later than the specified date.
    public static ImapSearchCriteria SentSince(DateTime date)
    {
      return new ImapSearchCriteria("SENTSINCE", ImapDateTimeFormat.ToDateString(date));
    }

    // SINCE <date>
    //     Messages whose internal date (disregarding time and timezone)
    //     is within or later than the specified date.
    public static ImapSearchCriteria Since(DateTime date)
    {
      return new ImapSearchCriteria("SINCE", ImapDateTimeFormat.ToDateString(date));
    }

    // SMALLER <n>
    //     Messages with an [RFC-2822] size smaller than the specified
    //     number of octets.
    public static ImapSearchCriteria Smaller(long size)
    {
      if (size < 0L)
        throw new ArgumentOutOfRangeException("size", size, "must be zero or positive number");

      return new ImapSearchCriteria("SMALLER", size.ToString());
    }

    // SUBJECT <string>
    //     Messages that contain the specified string in the envelope
    //     structure's SUBJECT field.
    public static ImapSearchCriteria Subject(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("SUBJECT", new ImapLiteralString(@value));
    }

    // TEXT <string>
    //     Messages that contain the specified string in the header or
    //     body of the message.
    public static ImapSearchCriteria Text(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("TEXT", new ImapLiteralString(@value));
    }

    // TO <string>
    //     Messages that contain the specified string in the envelope
    //     structure's TO field.
    public static ImapSearchCriteria To(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria("TO", new ImapLiteralString(@value));
    }

    // UNANSWERED
    //     Messages that do not have the \Answered flag set.
    public static readonly ImapSearchCriteria Unanswered
      = new ImapSearchCriteria("UNANSWERED");

    // UNDELETED
    //     Messages that do not have the \Deleted flag set.
    public static readonly ImapSearchCriteria Undeleted
      = new ImapSearchCriteria("UNDELETED");

    // UNDRAFT
    //     Messages that do not have the \Draft flag set.
    public static readonly ImapSearchCriteria Undraft
      = new ImapSearchCriteria("UNDRAFT");

    // UNFLAGGED
    //     Messages that do not have the \Flagged flag set.
    public static readonly ImapSearchCriteria Unflagged
      = new ImapSearchCriteria("UNFLAGGED");

    // UNKEYWORD <flag>
    //     Messages that do not have the specified keyword flag set.
    public static ImapSearchCriteria Unkeyword(ImapMessageFlag flag)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return new ImapSearchCriteria("UNKEYWORD", flag.ToString());
    }

    public static ImapSearchCriteria Unkeyword(string keyword)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");

      return new ImapSearchCriteria("UNKEYWORD", ImapMessageFlag.GetValidatedKeyword(keyword));
    }

    // UNSEEN
    //     Messages that do not have the \Seen flag set.
    public static readonly ImapSearchCriteria Unseen
      = new ImapSearchCriteria("UNSEEN");

    // NOT <search-key>
    //     Messages that do not match the specified search key.
    public ImapSearchCriteria Not()
    {
      return !this;
    }

    public static ImapSearchCriteria Not(ImapSearchCriteria c)
    {
      return !c;
    }

    public static ImapSearchCriteria operator! (ImapSearchCriteria c)
    {
      if (c == null)
        throw new ArgumentNullException("c");

      return new ImapSearchCriteria(c.requiredCapabilities,
                                    "NOT", new ImapParenthesizedString(c.Items));
    }

    // OR <search-key1> <search-key2>
    //     Messages that match either search key.
    public ImapSearchCriteria Or(ImapSearchCriteria searchingCriteria)
    {
      return this | searchingCriteria;
    }

    public static ImapSearchCriteria Or(ImapSearchCriteria x, ImapSearchCriteria y)
    {
      return x | y;
    }

    public static ImapSearchCriteria operator| (ImapSearchCriteria x, ImapSearchCriteria y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      return new ImapSearchCriteria(MergeRequiredCapabilities(x, y),
                                    "OR", new ImapParenthesizedString(x.Items), new ImapParenthesizedString(y.Items));
    }

    // AND
    public ImapSearchCriteria And(ImapSearchCriteria searchingCriteria)
    {
      return this & searchingCriteria;
    }

    public static ImapSearchCriteria And(ImapSearchCriteria x, ImapSearchCriteria y)
    {
      return x & y;
    }

    public static ImapSearchCriteria operator& (ImapSearchCriteria x, ImapSearchCriteria y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      return new ImapSearchCriteria(MergeRequiredCapabilities(x, y),
                                    GetCombinedItems(x, y));
    }

    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     */

    // 3.4. MODSEQ Search Criterion in SEARCH
    //    The MODSEQ criterion for the SEARCH command allows a client to search
    //    for the metadata items that were modified since a specified moment.
    // 
    //    Syntax:  MODSEQ [<entry-name> <entry-type-req>] <mod-sequence-valzer>
    [CLSCompliant(false)]
    public static ImapSearchCriteria ModSeq(ulong modificationSequence)
    {
      return ModSeq(modificationSequence, null, null);
    }

    [CLSCompliant(false)]
    public static ImapSearchCriteria ModSeqSharedEntry(ulong modificationSequence, ImapMessageFlag flag)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return ModSeq(modificationSequence, flag, "shared");
    }

    [CLSCompliant(false)]
    public static ImapSearchCriteria ModSeqPrivateEntry(ulong modificationSequence, ImapMessageFlag flag)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return ModSeq(modificationSequence, flag, "priv");
    }

    [CLSCompliant(false)]
    public static ImapSearchCriteria ModSeqAllEntry(ulong modificationSequence, ImapMessageFlag flag)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return ModSeq(modificationSequence, flag, "all");
    }

    private static ImapSearchCriteria ModSeq(ulong modSequenceValzer, ImapMessageFlag entryFlagName, ImapString entryType)
    {
      if (entryType == null)
        return new ImapSearchCriteria(new[] {ImapCapability.CondStore},
                                      "MODSEQ",
                                      modSequenceValzer.ToString());
      else
        return new ImapSearchCriteria(new[] {ImapCapability.CondStore},
                                      "MODSEQ",
                                      new ImapQuotedString(string.Format("/flags/{0}", entryFlagName.ToString())),
                                      entryType,
                                      modSequenceValzer.ToString());
    }

    /*
     * RFC 5032 - WITHIN Search Extension to the IMAP Protocol
     * http://tools.ietf.org/html/rfc5032
     */

    // 3. Formal Syntax
    //    This document extends RFC 3501 [RFC3501] with two new search keys:
    //    OLDER <interval> and YOUNGER <interval>.
    // 
    //    search-key =/ ( "OLDER" / "YOUNGER" ) SP nz-number
    //                   ; search-key defined in RFC 3501
    public static ImapSearchCriteria Older(TimeSpan interval)
    {
      return Older((long)interval.TotalSeconds);
    }

    public static ImapSearchCriteria Older(long intervalSeconds)
    {
      if (intervalSeconds <= 0L)
        throw new ArgumentOutOfRangeException("intervalSeconds", intervalSeconds, "must be non-zero positive number");

      return new ImapSearchCriteria(new[] {ImapCapability.Within}, "OLDER", intervalSeconds.ToString());
    }

    public static ImapSearchCriteria Younger(TimeSpan interval)
    {
      return Younger((long)interval.TotalSeconds);
    }

    public static ImapSearchCriteria Younger(long intervalSeconds)
    {
      if (intervalSeconds <= 0L)
        throw new ArgumentOutOfRangeException("intervalSeconds", intervalSeconds, "must be non-zero positive number");

      return new ImapSearchCriteria(new[] {ImapCapability.Within}, "YOUNGER", intervalSeconds.ToString());
    }

    /*
     * RFC 5466 - IMAP4 Extension for Named Searches (Filters)
     * http://tools.ietf.org/html/rfc5466
     *
     * 4. Formal Syntax
     *    search-key            =/  "FILTER" SP filter-name
     *                          ;; New SEARCH criterion for referencing filters
     * 
     *    filter-name           =  1*<any ATOM-CHAR except "/">
     *                          ;; Note that filter-name disallows UTF-8 or
     *                          ;; the following characters: "(", ")", "{",
     *                          ;; " ", "%", "*", "]".  See definition of
     *                          ;; ATOM-CHAR [RFC3501].
     */
    public static ImapSearchCriteria Filter(string filterName)
    {
      return new ImapSearchCriteria(new[] {ImapCapability.Filters}, "FILTER", filterName);
    }

    /*
     * draft-ietf-morg-inthread-00 - The IMAP SEARCH=INTHREAD and THREAD=REFS Extensions
     * http://tools.ietf.org/html/draft-ietf-morg-inthread-00
     */

    // 3.1. The INTHREAD Search Key
    //     INTHREAD takes one argument, which is another search key.
    // 
    //     The INTHREAD search-key matches a message if its subsidiary search-
    //     key matches at least one message in the same thread as the message.
    // 
    //     This command finds all messages in an entire thread concerning the
    //     meetings where fizzle was discussed:
    // 
    //          C: a UID SEARCH INTHREAD (SUBJECT meeting BODY fizzle)
    // 
    //     This command threads all threads containing at least one message
    //     from fred@example.com:
    // 
    //          C: a UID THREAD REFS utf-8 INTHREAD FROM <fred@example.com>
    public static readonly ImapSearchCriteria InThread
      = new ImapSearchCriteria(new[] {ImapCapability.SearchInThread, ImapCapability.ThreadRefs}, "INTHREAD");

    // 3.2. The THREADROOT Search Key
    //     The THREADROOT search key matches a message if that message does not
    //     have any extant parent according to the active threading algorithm
    //     (see section 3.5).
    // 
    //     This command finds the roots of all threads containing unread
    //     messages:
    // 
    //          C: a UID SEARCH THREADROOT INTHREAD UNSEEN
    public static readonly ImapSearchCriteria ThreadRoot
      = new ImapSearchCriteria(new[] {ImapCapability.SearchInThread, ImapCapability.ThreadRefs}, "THREADROOT");

    // 3.3. The THREADLEAF Search Key
    //     The THREADLEAF search key matches a message if that message has no
    //     extant children in the same mailbox, according to the active
    //     threading algorithm.
    // 
    //     Note that THEADLEAF interacts badly with THREAD=ORDEREDSUBJECT.
    //     THREAD=ORDEREDSUBJECT is defined such that every message is either a
    //     root or a leaf, there are no intermediate nodes.
    // 
    //     This command finds all messages that were (also) sent to me, and to
    //     which noone has answered:
    // 
    //          C: a UID SEARCH THREADLEAF OR TO <me@example.com> CC
    //             <me@example.com>
    public static readonly ImapSearchCriteria ThreadLeaf
      = new ImapSearchCriteria(new[] {ImapCapability.SearchInThread, ImapCapability.ThreadRefs}, "THREADLEAF");

    // 3.4. The MESSAGEID Search Key
    //     The MESSAGEID search key takes a sigle argument, and matches a
    //     message if that message's normalized nessage-id is the same as the
    //     argument.
    // 
    //     This command finds all in the same thread as
    //     <4321.1234321@example.com>:
    // 
    //          C: a UID SEARCH INTHREAD MESSAGEID <4321.1234321@example.com>
    public static ImapSearchCriteria MessageId(string @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return new ImapSearchCriteria(new[] {ImapCapability.SearchInThread, ImapCapability.ThreadRefs},
                                    "MESSAGEID",
                                    string.Concat("<", @value, ">"));
    }

    public static ImapSearchCriteria FromUri(Uri uri)
    {
      bool discard1;
      string discard2;

      return FromUri(uri, true, false, false, out discard1, out discard2);
    }

    public static ImapSearchCriteria FromUri(Uri uri, bool splitcCharset, out bool containsLiteral, out string charset)
    {
      return FromUri(uri, true, false, splitcCharset, out containsLiteral, out charset);
    }

    private static ByteString charsetSpecification = new ByteString("CHARSET ");

    private static ImapSearchCriteria FromUri(Uri uri,
                                              bool convertLiteral,
                                              bool synchronizedLiteral,
                                              bool splitCharset,
                                              out bool containsLiteral,
                                              out string charset)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      containsLiteral = false;
      charset = null;

      var q = uri.Query;

      if (q.Length == 0)
        return null;
      else if (q.Length == 1) // '?'
        return new ImapSearchCriteria(string.Empty);

      /*
       * http://tools.ietf.org/html/rfc5092
       * RFC 5092 - IMAP URL Scheme
       * 
       *    Note that quoted strings and non-synchronizing literals [LITERAL+]
       *    are allowed in the <enc-search> content; however, synchronizing
       *    literals are not allowed, as their presence would effectively mean
       *    that the agent interpreting IMAP URLs needs to parse an <enc-search>
       *    content, find all synchronizing literals, and perform proper command
       *    continuation request handling (see Sections 4.3 and 7 of [IMAP4]).
       */
      var query = PercentEncoding.Decode(q.Substring(1), false);
      var len = query.Length;
      var convertedQuery = new ByteStringBuilder(len);

      if (splitCharset) {
        var queryString = new ByteString(query);

        if (queryString.StartsWithIgnoreCase(charsetSpecification)) {
          // CHARSET<SP>astring<SP>
          var posEndOfCharset = queryString.IndexOf(Octets.SP, charsetSpecification.Length);

          if (posEndOfCharset < 0) {
            throw new ArgumentException("search criteria contains invalid charset specification", "uri");
          }
          else {
            charset = queryString.Substring(charsetSpecification.Length,
                                            posEndOfCharset - charsetSpecification.Length).ToString();

            query = queryString.Substring(posEndOfCharset + 1).ByteArray;
            len = query.Length;
          }
        }
      }

      for (var i = 0; i < len;) {
        if (query[i] == ImapOctets.DQuote) {
          /*
           * quoted
           */
          var start = i;

          for (;;) {
            if (++i == len)
              throw new ArgumentException("search criteria contains unclosed quoted string", "uri");

            if (query[i] == ImapOctets.DQuote) {
              break;
            }
            else if (query[i] == ImapOctets.BackSlash) {
              if (++i == len || !(query[i] == ImapOctets.DQuote || query[i] == ImapOctets.BackSlash))
                throw new ArgumentException("search criteria contains invalid quoted string", "uri");
            }
          }

          i++;

          convertedQuery.Append(query, start, i - start);
        }
        else if (query[i] == ImapOctets.OpenBrace) {
          /*
           * literal
           */
          var start = i;
          var isLiteralSynchronizing = false;
          var literalLength = 0;

          for (;;) {
            if (++i == len)
              throw new ArgumentException("search criteria contains incomplete literal", "uri");

            if (Octets.IsDecimalNumber(query[i])) {
              literalLength = literalLength * 10 + (query[i] - 0x30 /* '0' */);
              // TODO: check length
            }
            else if (query[i] == ImapOctets.CloseBrace) {
              // {xxx}
              isLiteralSynchronizing = true;
              break;
            }
            else if (query[i] == ImapOctets.Plus) {
              // {xxx+}
              if (++i == len || query[i] != ImapOctets.CloseBrace)
                throw new ArgumentException("search criteria contains incomplete non-synchronized literal", "uri");
              isLiteralSynchronizing = false;
              break;
            }
            else {
              throw new ArgumentException("search criteria contains invalid literal", "uri");
            }
          }

          if (++i == len || query[i] != Octets.CR)
            throw new ArgumentException("search criteria contains incomplete literal (CR not found)", "uri");
          if (++i == len || query[i] != Octets.LF)
            throw new ArgumentException("search criteria contains incomplete literal (LF not found)", "uri");
          if (++i == len && 0 < literalLength)
            throw new ArgumentException("search criteria contains incomplete literal (unexpected EOL)", "uri");

          containsLiteral = true;

          if (convertLiteral) {
            if (synchronizedLiteral)
              convertedQuery.Append(string.Format("{{{0}}}\x0d\x0a", literalLength));
            else
              convertedQuery.Append(string.Format("{{{0}+}}\x0d\x0a", literalLength));

            convertedQuery.Append(query, i, literalLength);
          }
          else {
            if (synchronizedLiteral != isLiteralSynchronizing)
              throw new ArgumentException(synchronizedLiteral
                                          ? "search criteria contains non-synchronizing literal"
                                          : "search criteria contains synchronizing literal",
                                          "uri");

            convertedQuery.Append(query, start, i - start + literalLength);
          }

          i += literalLength;
        }
        else {
          convertedQuery.Append(query[i++]);
        }
      }

      return new ImapSearchCriteria(new ImapPreformattedString(convertedQuery.ToByteArray()));
    }

    private static IEnumerable<ImapCapability> MergeRequiredCapabilities(ImapSearchCriteria x, ImapSearchCriteria y)
    {
      var requiredCapabilities = new List<ImapCapability>(x.requiredCapabilities);

      foreach (var cap in y.requiredCapabilities) {
        if (!requiredCapabilities.Contains(cap))
          requiredCapabilities.Add(cap);
      }

      return requiredCapabilities;
    }

    ImapCapability[] IImapMultipleExtension.RequiredCapabilities {
      get { return requiredCapabilities.ToArray(); }
    }

    internal List<ImapCapability> RequiredCapabilities {
      get { return requiredCapabilities; }
    }

    private ImapSearchCriteria(params ImapString[] items)
      : this(new ImapCapability[] {}, items)
    {
    }

    private ImapSearchCriteria(IEnumerable<ImapCapability> requiredCapabilities, params ImapString[] items)
      : base(items)
    {
      this.requiredCapabilities.AddRange(requiredCapabilities);
    }

#region "IImapUrlSearchQuery"
    byte[] IImapUrlSearchQuery.GetEncodedQuery(Encoding charset, out bool charsetSpecified)
    {
      charsetSpecified = (charset != null && SetCharset(charset));

      Traverse(delegate(ImapString criterion) {
        var literal = criterion as IImapLiteralString;

        if (literal != null)
          literal.Options = ImapLiteralOptions.NonSynchronizing;
      });

      return SearchCriteriaEncoder.Encode(this);
    }

    private class SearchCriteriaEncoder : ImapSender {
      public static byte[] Encode(ImapSearchCriteria criteria)
      {
        return (new SearchCriteriaEncoder()).EncodeCriteria(criteria);
      }

      private SearchCriteriaEncoder()
        : base(new LineOrientedBufferedStream(new MemoryStream()))
      {
      }

      private byte[] EncodeCriteria(ImapSearchCriteria criteria)
      {
        Enqueue(new[] {criteria});
        Enqueue(Octets.CRLF);

        Send();

        try {
          var innerStream = Stream.InnerStream as MemoryStream;

          innerStream.Close();

          var ret = new ByteString(innerStream.ToArray());

          return ret.Substring(0, ret.Length - 2 /*CRLF*/).ByteArray;
        }
        finally {
          Stream.Close();
        }
      }
    }
#endregion

    internal bool SetCharset(Encoding charset)
    {
      var charsetSpecified = false;

      Traverse(delegate(ImapString criterion) {
        var literal = criterion as ImapLiteralString;

        if (literal != null) {
          literal.Charset = charset;
          charsetSpecified = true;
        }
      });

      return charsetSpecified;
    }

    protected override ImapStringList GetCombined()
    {
      return ToStringList();
    }

    private /*readonly*/ List<ImapCapability> requiredCapabilities = new List<ImapCapability>();
  }
}