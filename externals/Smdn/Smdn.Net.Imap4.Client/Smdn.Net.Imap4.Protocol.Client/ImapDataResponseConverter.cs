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

using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4.Protocol.Client {
  // format converting methods for ImapDataResponse
  public static class ImapDataResponseConverter {
    /*
     * 7.2. Server Responses - Server and Mailbox Status
     */

    // 7.2.1 CAPABILITY Response
    //   Contents:   capability listing
    //   Example:    S: * CAPABILITY IMAP4rev1 STARTTLS AUTH=GSSAPI XPIG-LATIN
    public static ImapCapabilityList FromCapability(ImapDataResponse data)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      capability-data = "CAPABILITY" *(SP capability) SP "IMAP4rev1"
                        *(SP capability)
                          ; Servers MUST implement the STARTTLS, AUTH=PLAIN,
                          ; and LOGINDISABLED capabilities
                          ; Servers which offer RFC 1730 compatibility MUST
                          ; list "IMAP4" as the first capability.
      */
      RejectMalformed(data, ImapDataResponseType.Capability, 0);

      return ImapDataConverter.ToCapability(data.Data);
    }

    // 7.2.2 LIST Response
    //   Contents:   name attributes
    //               hierarchy delimiter
    //               name
    //   Example:    S: * LIST (\Noselect) "/" ~/Mail/foo
    public static ImapMailboxList FromList(ImapDataResponse data)
    {
      RejectMalformed(data, ImapDataResponseType.List, 0);

      return FromListOrLsub(data);
    }

    // 7.2.3 LSUB Response
    //   Contents:   name attributes
    //               hierarchy delimiter
    //               name
    //   Example:    S: * LSUB () "." #news.comp.mail.misc
    public static ImapMailboxList FromLsub(ImapDataResponse data)
    {
      RejectMalformed(data, ImapDataResponseType.Lsub, 0);

      return FromListOrLsub(data);
    }

    /*
     * Gimap XLIST capability extension
     */
    public static ImapMailboxList FromXList(ImapDataResponse data)
    {
      RejectMalformed(data, ImapDataResponseType.XList, 0);

      return FromListOrLsub(data);
    }

    private static ImapMailboxList FromListOrLsub(ImapDataResponse data)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      mailbox-data    =  "FLAGS" SP flag-list / "LIST" SP mailbox-list /
                         "LSUB" SP mailbox-list / "SEARCH" *(SP nz-number) /
                         "STATUS" SP mailbox SP "(" [status-att-list] ")" /
                         number SP "EXISTS" / number SP "RECENT"
      */
      return ImapDataConverter.ToMailboxList(data.Data);
    }

    // 7.2.4 STATUS Response
    //   Contents:   name
    //               status parenthesized list
    //   Example:    S: * STATUS blurdybloop (MESSAGES 231 UIDNEXT 44292)
    public static ImapStatusAttributeList FromStatus(ImapDataResponse data)
    {
      string discard;

      return FromStatus(data, out discard);
    }

    public static ImapStatusAttributeList FromStatus(ImapDataResponse data, out string mailbox)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      mailbox-data    =  "FLAGS" SP flag-list / "LIST" SP mailbox-list /
                         "LSUB" SP mailbox-list / "SEARCH" *(SP nz-number) /
                         "STATUS" SP mailbox SP "(" [status-att-list] ")" /
                         number SP "EXISTS" / number SP "RECENT"
      */
      RejectMalformed(data, ImapDataResponseType.Status, 2);

      if (data.Data[0].Format != ImapDataFormat.Text ||
          data.Data[1].Format != ImapDataFormat.List)
        throw new ImapMalformedDataException("invalid format");

      mailbox = ImapDataConverter.ToMailbox(data.Data[0]);

      return ImapDataConverter.ToStatusAttList(data.Data[1].List);
    }

    // 7.2.5 SEARCH Response
    //   Contents:   zero or more numbers
    //   Example:    S: * SEARCH 2 3 6
    public static ImapMatchedSequenceSet FromSearch(ImapDataResponse data, bool uidSet)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      mailbox-data    =  "FLAGS" SP flag-list / "LIST" SP mailbox-list /
                         "LSUB" SP mailbox-list / "SEARCH" *(SP nz-number) /
                         "STATUS" SP mailbox SP "(" [status-att-list] ")" /
                         number SP "EXISTS" / number SP "RECENT"
      */
      RejectMalformed(data, ImapDataResponseType.Search, 0);

      return FromSearchOrSort(data, uidSet);
    }

    // http://tools.ietf.org/html/rfc5256
    // BASE.7.2.SORT. SORT Response
    //   Data:       zero or more numbers
    //   Example:    S: * SORT 2 3 6
    public static ImapMatchedSequenceSet FromSort(ImapDataResponse data, bool uidSet)
    {
      /*
      sort-data       = "SORT" *(SP nz-number)
      */
      RejectMalformed(data, ImapDataResponseType.Sort, 0);

      return FromSearchOrSort(data, uidSet);
    }

    private static ImapMatchedSequenceSet FromSearchOrSort(ImapDataResponse data, bool uidSet)
    {
      /*
       * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
       * http://tools.ietf.org/html/rfc4551
       * 
       * 3.5. Modified SEARCH Untagged Response
       *    Data:       zero or more numbers
       *                mod-sequence value (omitted if no match)
       * 
       *    mailbox-data        =/ "SEARCH" [1*(SP nz-number) SP
       *                           search-sort-mod-seq]
       *    search-sort-mod-seq = "(" "MODSEQ" SP mod-sequence-value ")"
       */
      if (data.Data.Length == 0)
        return ImapMatchedSequenceSet.CreateEmpty(uidSet);

      var last = data.Data.Length - 1;

      if (data.Data[last].Format == ImapDataFormat.List) {
        if (data.Data[last].List.Length < 2 || !ImapDataConverter.IsTextEqualsToCaseInsensitive(data.Data[last].List[0], "MODSEQ"))
          throw new ImapMalformedDataException("invalid format", data.Data[last]);

        var numbers = new long[last];

        for (var i = 0; i < numbers.Length; i++) {
          numbers[i] = ImapDataConverter.ToNonZeroNumber(data.Data[i]);
        }

        return new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(uidSet, numbers),
                                          ImapDataConverter.ToModSequenceValue(data.Data[last].List[1]));
      }
      else {
        return new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(uidSet,
                                                                    Array.ConvertAll<ImapData, long>(data.Data, ImapDataConverter.ToNonZeroNumber)));
      }
    }

    // 7.2.6 FLAGS Response
    //   Contents:   flag parenthesized list
    //   Example:    S: * FLAGS (\Answered \Flagged \Deleted \Seen \Draft)
    public static IImapMessageFlagSet FromFlags(ImapDataResponse data)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      mailbox-data    =  "FLAGS" SP flag-list / "LIST" SP mailbox-list /
                         "LSUB" SP mailbox-list / "SEARCH" *(SP nz-number) /
                         "STATUS" SP mailbox SP "(" [status-att-list] ")" /
                         number SP "EXISTS" / number SP "RECENT"
      */
      RejectMalformed(data, ImapDataResponseType.Flags, 1);

      return ImapDataConverter.ToFlagList(data.Data[0]);
    }

    /*
     * 7.3. Server Responses - Mailbox Size
     */

    // 7.3.1. EXISTS Response
    //   Contents:   none
    //   Example:    S: * 23 EXISTS
    public static long FromExists(ImapDataResponse data)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      mailbox-data    =  "FLAGS" SP flag-list / "LIST" SP mailbox-list /
                         "LSUB" SP mailbox-list / "SEARCH" *(SP nz-number) /
                         "STATUS" SP mailbox SP "(" [status-att-list] ")" /
                         number SP "EXISTS" / number SP "RECENT"
      */
      RejectMalformed(data, ImapDataResponseType.Exists, 1);

      return ImapDataConverter.ToNumber(data.Data[0]);
    }

    // 7.3.2. RECENT Response
    //   Contents:   none
    //   Example:    S: * 5 RECENT
    public static long FromRecent(ImapDataResponse data)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      mailbox-data    =  "FLAGS" SP flag-list / "LIST" SP mailbox-list /
                         "LSUB" SP mailbox-list / "SEARCH" *(SP nz-number) /
                         "STATUS" SP mailbox SP "(" [status-att-list] ")" /
                         number SP "EXISTS" / number SP "RECENT"
      */
      RejectMalformed(data, ImapDataResponseType.Recent, 1);

      return ImapDataConverter.ToNumber(data.Data[0]);
    }

    /*
     * 7.4. Server Responses - Message Status
     */

    // 7.4.1. EXPUNGE Response
    //   Contents:   none
    //   Example:    S: * 44 EXPUNGE
    public static long FromExpunge(ImapDataResponse data)
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      message-data    = nz-number SP ("EXPUNGE" / ("FETCH" SP msg-att))
      */
      RejectMalformed(data, ImapDataResponseType.Expunge, 1);

      return ImapDataConverter.ToNonZeroNumber(data.Data[0]);
    }

    // 7.4.2. FETCH Response
    //   Contents:   message data
    //   Example:    S: * 23 FETCH (FLAGS (\Seen) RFC822.SIZE 44827)
    public static TMessageAttribute FromFetch<TMessageAttribute>(ImapDataResponse data)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      RejectMalformed(data, ImapDataResponseType.Fetch, 2);

      TMessageAttribute message = null;

      ImapDataConverter.ToFetchMessageData<TMessageAttribute>(ref message, data.Data[0], data.Data[1]);

      return message;
    }

    public static long FromFetch<TMessageAttribute>(IImapMessageAttributeCollection<TMessageAttribute> messages,
                                                    ImapDataResponse data)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      /*
      response-data   = "*" SP (resp-cond-state / resp-cond-bye /
                        mailbox-data / message-data / capability-data) CRLF
      message-data    = nz-number SP ("EXPUNGE" / ("FETCH" SP msg-att))
      */
      RejectMalformed(data, ImapDataResponseType.Fetch, 2);

      return ImapDataConverter.ToFetchMessageData(messages, data.Data[0], data.Data[1]);
    }

    /*
     * RFC 2087 - IMAP4 QUOTA extension
     * http://tools.ietf.org/html/rfc2087
     * 
     * 5.1. QUOTA Response
     *    Data:       quota root name
     *                list of resource names, usages, and limits
     * 
     *       This response occurs as a result of a GETQUOTA or GETQUOTAROOT
     *       command. The first string is the name of the quota root for which
     *       this quota applies.
     * 
     *       The name is followed by a S-expression format list of the resource
     *       usage and limits of the quota root.  The list contains zero or
     *       more triplets.  Each triplet conatins a resource name, the current
     *       usage of the resource, and the resource limit.
     * 
     *       Resources not named in the list are not limited in the quota root.
     *       Thus, an empty list means there are no administrative resource
     *       limits in the quota root.
     */
    public static ImapQuota FromQuota(ImapDataResponse data)
    {
      /* 
       *    quota_response  ::= "QUOTA" SP astring SP quota_list
       *    quota_list      ::= "(" #quota_resource ")"
       */
      RejectMalformed(data, ImapDataResponseType.Quota, 2);

      return new ImapQuota(ImapDataConverter.ToAString(data.Data[0]),
                           ImapDataConverter.ToQuotaList(data.Data[1]));
    }

    /*
     * RFC 2087 - IMAP4 QUOTA extension
     * http://tools.ietf.org/html/rfc2087
     * 
     * 5.2. QUOTAROOT Response
     *    Data:       mailbox name
     *                zero or more quota root names
     * 
     *       This response occurs as a result of a GETQUOTAROOT command.  The
     *       first string is the mailbox and the remaining strings are the
     *       names of the quota roots for the mailbox.
     */
    public static string FromQuotaRoot(ImapDataResponse data, out string[] quotaRootNames)
    {
      /* 
       *    quotaroot_response
       *                    ::= "QUOTAROOT" SP astring *(SP astring)
       */
      RejectMalformed(data, ImapDataResponseType.QuotaRoot, 1);

      var ret = ImapDataConverter.ToMailbox(data.Data[0]);

      if (data.Data.Length == 1)
        quotaRootNames = new string[0];
      else
        quotaRootNames = Array.ConvertAll<ImapData, string>(data.Data.Slice(1),
                                                            ImapDataConverter.ToAString);

      return ret;
    }

    /*
     * RFC 5161 The IMAP ENABLE Extension
     * http://tools.ietf.org/html/rfc5161
     */

    // 3.2. The ENABLED Response
    //    Contents:   capability listing
    // 
    //    The ENABLED response occurs as a result of an ENABLE command.  The
    //    capability listing contains a space-separated listing of capability
    //    names that the server supports and that were successfully enabled.
    //    The ENABLED response may contain no capabilities, which means that no
    //    extensions listed by the client were successfully enabled.
    public static ImapCapabilityList FromEnabled(ImapDataResponse data)
    {
      /*
      response-data =/ "*" SP enable-data CRLF
      enable-data   = "ENABLED" *(SP capability)
      */
      RejectMalformed(data, ImapDataResponseType.Enabled, 0);

      return ImapDataConverter.ToCapability(data.Data);
    }

    /*
     * RFC 2971 IMAP4 ID extension
     * http://tools.ietf.org/html/rfc2971
     */

    // 3.2. ID Response
    //    Contents:   server parameter list
    // 
    //    In response to an ID command issued by the client, the server replies
    //    with a tagged response containing information on its implementation.
    //    The format is the same as the client list.
    public static IDictionary<string, string> FromId(ImapDataResponse data)
    {
      /*
      id_response ::= "ID" SPACE id_params_list
      */
      RejectMalformed(data, ImapDataResponseType.Id, 1);

      return ImapDataConverter.ToIdParamsList(data.Data[0]);
    }

    // http://tools.ietf.org/html/rfc5256
    // BASE.7.2.THREAD. THREAD Response
    //   Data:       zero or more threads
    //   Example:    S: * THREAD (2)(3 6 (4 23)(44 7 96))
    //   Example:    S: * THREAD ((3)(5))
    public static ImapThreadList FromThread(ImapDataResponse data, bool uidSet)
    {
      /*
      thread-data     = "THREAD" [SP 1*thread-list]
      */
      RejectMalformed(data, ImapDataResponseType.Thread, 0);

      return ImapDataConverter.ToThreadList(uidSet, data.Data);
    }

    /*
     * RFC 2342 IMAP4 Namespace
     * http://tools.ietf.org/html/rfc2342
     */

    // 5. NAMESPACE Command
    //   Response:  an untagged NAMESPACE response that contains the prefix
    //                and hierarchy delimiter to the server's Personal
    //                Namespace(s), Other Users' Namespace(s), and Shared
    //                Namespace(s) that the server wishes to expose. The
    //                response will contain a NIL for any namespace class
    //                that is not available. Namespace_Response_Extensions
    //                MAY be included in the response.
    //                Namespace_Response_Extensions which are not on the IETF
    //                standards track, MUST be prefixed with an "X-".
    //   Example 5.4:
    //   S: * NAMESPACE (("" "/")) (("~" "/")) (("#shared/" "/")
    //      ("#public/" "/")("#ftp/" "/")("#news." "."))
    public static ImapNamespace FromNamespace(ImapDataResponse data)
    {
      /*
       Namespace_Response = "*" SP "NAMESPACE" SP Namespace SP Namespace SP
          Namespace
      */
      RejectMalformed(data, ImapDataResponseType.Namespace, 3);

      return new ImapNamespace(ImapDataConverter.ToNamespace(data.Data[0]),
                               ImapDataConverter.ToNamespace(data.Data[1]),
                               ImapDataConverter.ToNamespace(data.Data[2]));
    }

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     * 3.3. LANGUAGE Response
     *    Contents:  A list of one or more language tags.
     */
    public static string[] FromLanguage(ImapDataResponse data)
    {
      /*
       *     response-payload  =/ language-data
       *     language-data     = "LANGUAGE" SP "(" lang-tag-quoted *(SP
       *                       lang-tag-quoted) ")"
       */
      RejectMalformed(data, ImapDataResponseType.Language, 1);

      if (data.Data[0].Format != ImapDataFormat.List)
        throw new ImapMalformedDataException("invalid format");

      return Array.ConvertAll<ImapData, string>(data.Data[0].List, ImapDataConverter.ToLangTagQuoted);
    }

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     * 4.8. COMPARATOR Response
     *   Contents:  The active comparator.  An optional list of available
     *              matching comparators
     */
    public static ImapCollationAlgorithm FromComparator(ImapDataResponse data, out ImapCollationAlgorithm[] matchingComparators)
    {
      /*
       * 
       *     comparator-data   = "COMPARATOR" SP comp-sel-quoted [SP "("
       *                         comp-id-quoted *(SP comp-id-quoted) ")"]
       *     comp-order-quoted = astring
       *         ; Once any literal wrapper or quoting is removed, this
       *         ; follows the collation-order rule from [RFC4790]
       *     comp-sel-quoted   = astring
       *         ; Once any literal wrapper or quoting is removed, this
       *         ; follows the collation-selected rule from [RFC4790]
       */
      RejectMalformed(data, ImapDataResponseType.Comparator, 1);

      var activeComparator = ImapDataConverter.ToCompSelQuoted(data.Data[0]);

      if (2 <= data.Data.Length) {
        if (data.Data[1].Format != ImapDataFormat.List)
          throw new ImapMalformedDataException("invalid format");

        matchingComparators = Array.ConvertAll<ImapData, ImapCollationAlgorithm>(data.Data[1].List,
                                                                                 ImapDataConverter.ToCompOrderQuoted);
      }
      else {
        matchingComparators = null;
      }

      return activeComparator;
    }

    /*
     * RFC 4466 - Collected Extensions to IMAP4 ABNF
     * http://tools.ietf.org/html/rfc4466
     * 
     * 2.6.2. ESEARCH untagged response
     *    Contents:   one or more search-return-data pairs
     */
    public static ImapMatchedSequenceSet FromESearch(ImapDataResponse data)
    {
      /*
       *    esearch-response  = "ESEARCH" [search-correlator] [SP "UID"]
       *                         *(SP search-return-data)
       *                       ;; Note that SEARCH and ESEARCH responses
       *                       ;; SHOULD be mutually exclusive,
       *                       ;; i.e., only one of the response types
       *                       ;; should be
       *                       ;; returned as a result of a command.
       */
      RejectMalformed(data, ImapDataResponseType.ESearch, 0);

      var index = 0;
      var uid = false;
      string tag = null;

      if (index < data.Data.Length && data.Data[index].Format == ImapDataFormat.List) {
        // search-correlator
        tag = ImapDataConverter.ToSearchCorrelator(data.Data[index]);

        index++;
      }

      if (index < data.Data.Length &&
          data.Data[index].Format == ImapDataFormat.Text &&
          ImapDataConverter.IsTextEqualsToCaseInsensitive(data.Data[index], "UID")) {
        uid = true;

        index++;
      }

      // search-return-data
      return ImapDataConverter.ToSearchReturnData(data.Data, tag, uid, index);
    }

    /*
     * RFC 5464 - The IMAP METADATA Extension
     * http://tools.ietf.org/html/rfc5464
     */
    public static string[] FromMetadataEntryList(ImapDataResponse data, out string mailbox)
    {
      /*
       *       metadata-resp     = "METADATA" SP mailbox SP
       *                           (entry-values / entry-list)
       *                           ; empty string for mailbox implies
       *                           ; server annotation.
       */
      RejectMalformed(data, ImapDataResponseType.Metadata, 2);

      if (data.Data[0].Format != ImapDataFormat.Text)
        throw new ImapMalformedDataException("invalid format");

      mailbox = ImapDataConverter.ToMailbox(data.Data[0]);

      return ImapDataConverter.ToEntryList(data.Data.Slice(1));
    }

    public static ImapMetadata[] FromMetadataEntryValues(ImapDataResponse data, out string mailbox)
    {
      /*
       *       metadata-resp     = "METADATA" SP mailbox SP
       *                           (entry-values / entry-list)
       *                           ; empty string for mailbox implies
       *                           ; server annotation.
       */
      RejectMalformed(data, ImapDataResponseType.Metadata, 2);

      if (data.Data[0].Format != ImapDataFormat.Text)
        throw new ImapMalformedDataException("invalid format");

      mailbox = ImapDataConverter.ToMailbox(data.Data[0]);

      return ImapDataConverter.ToEntryValues(data.Data[1]);
    }

    private static void RejectMalformed(ImapDataResponse data, ImapDataResponseType expectedType, int expectedLength)
    {
      if (data.Type != expectedType)
        throw new ImapMalformedDataException(string.Format("expected data type is {0}, but was {1}", expectedType, data.Type));

      if (data.Data.Length < expectedLength)
        throw new ImapMalformedDataException(string.Format("too few data counts; expected is {0} but was {1}", expectedLength, data.Data.Length));
    }
  }
}
