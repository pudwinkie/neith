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

namespace Smdn.Net.Imap4.Protocol.Client {
  // format converting methods for ImapResponseText
  public static class ImapResponseTextConverter {
    /*
     * RFC 3501 INTERNET MESSAGE ACCESS PROTOCOL - VERSION 4rev1
     * http://tools.ietf.org/html/rfc3501
     */

    /*
    resp-text-code  = "ALERT" /
                      "BADCHARSET" [SP "(" astring *(SP astring) ")" ] /
                      capability-data / "PARSE" /
                      "PERMANENTFLAGS" SP "("
                      [flag-perm *(SP flag-perm)] ")" /
                      "READ-ONLY" / "READ-WRITE" / "TRYCREATE" /
                      "UIDNEXT" SP nz-number / "UIDVALIDITY" SP nz-number /
                      "UNSEEN" SP nz-number /
                      atom [SP 1*<any TEXT-CHAR except "]">]
    */
    public static string[] FromBadCharset(ImapResponseText respText)
    {
      RejectMalformed(respText, ImapResponseCode.BadCharset, 0);

      if (respText.Arguments.Length == 0)
        return new string[] {};

      if (respText.Arguments[0].Format != ImapDataFormat.List)
        throw new ImapMalformedDataException(string.Format("invalid format: expected type is {0}, but was {1}",
                                                           ImapDataFormat.List,
                                                           respText.Arguments[0].Format),
                                             respText.Arguments[0]);

      return Array.ConvertAll<ImapData, string>(respText.Arguments[0].List, ImapDataConverter.ToAString);
    }

    public static ImapCapabilitySet FromCapability(ImapResponseText respText)
    {
      RejectMalformed(respText, ImapResponseCode.Capability, 0);

      return ImapDataConverter.ToCapability(respText.Arguments);
    }

    public static IImapMessageFlagSet FromPermanentFlags(ImapResponseText respText)
    {
      RejectMalformed(respText, ImapResponseCode.PermanentFlags, 0);

      if (respText.Arguments.Length < 1)
        return new ImapMessageFlagSet();

      if (respText.Arguments[0].Format != ImapDataFormat.List)
        throw new ImapMalformedDataException(string.Format("invalid format: expected type is {0}, but was {1}",
                                                           ImapDataFormat.List,
                                                           respText.Arguments[0].Format),
                                             respText.Arguments[0]);

      return ImapDataConverter.ToFlagPerm(respText.Arguments[0].List);
    }

    public static long FromUidNext(ImapResponseText respText)
    {
      RejectMalformed(respText, ImapResponseCode.UidNext, 1);

      return ImapDataConverter.ToNonZeroNumber(respText.Arguments[0]);
    }

    public static long FromUidValidity(ImapResponseText respText)
    {
      RejectMalformed(respText, ImapResponseCode.UidValidity, 1);

      return ImapDataConverter.ToNonZeroNumber(respText.Arguments[0]);
    }

    public static long FromUnseen(ImapResponseText respText)
    {
      RejectMalformed(respText, ImapResponseCode.Unseen, 1);

      return ImapDataConverter.ToNonZeroNumber(respText.Arguments[0]);
    }

    /*
     * RFC 2193 IMAP4 Mailbox Referrals
     * http://tools.ietf.org/html/rfc2193
     * 
     * RFC 2221 IMAP4 Login Referrals
     * http://tools.ietf.org/html/rfc2221
     */
    public static Uri[] FromReferral(ImapResponseText respText)
    {
      /*
       referral_response_code = "[" "REFERRAL" 1*(SPACE <url>) "]"
          ; See [RFC-1738] for <url> definition
      */
      RejectMalformed(respText, ImapResponseCode.Referral, 1);

      return Array.ConvertAll<ImapData, Uri>(respText.Arguments, ImapDataConverter.ToUri);
    }

    /*
     * RFC 4315 Internet Message Access Protocol (IMAP) - UIDPLUS extension
     * http://tools.ietf.org/html/rfc4315
     */
    public static ImapAppendedUidSet FromAppendUid(ImapResponseText respText)
    {
      /*
       *    resp-code-apnd  = "APPENDUID" SP nz-number SP append-uid
       *    append-uid      = uniqueid
       * 
       *    Servers that support [MULTIAPPEND] will have the following extension
       *    to the above rules:
       * 
       *    append-uid      =/ uid-set
       *                      ; only permitted if client uses [MULTIAPPEND]
       *                      ; to append multiple messages.
       */
      RejectMalformed(respText, ImapResponseCode.AppendUid, 2);

      return new ImapAppendedUidSet(ImapDataConverter.ToNonZeroNumber(respText.Arguments[0]),
                                    ImapDataConverter.ToUidSet(respText.Arguments[1]));
    }

    public static ImapCopiedUidSet FromCopyUid(ImapResponseText respText)
    {
      /*
       *    resp-code-copy  = "COPYUID" SP nz-number SP uid-set SP uid-set
       */
      RejectMalformed(respText, ImapResponseCode.CopyUid, 3);

      return new ImapCopiedUidSet(ImapDataConverter.ToNonZeroNumber(respText.Arguments[0]),
                                  ImapDataConverter.ToUidSet(respText.Arguments[1]),
                                  ImapDataConverter.ToUidSet(respText.Arguments[2]));
    }

    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     */
    [CLSCompliant(false)]
    public static ulong FromHighestModSeq(ImapResponseText respText)
    {
      /*
       *    resp-text-code      =/ "HIGHESTMODSEQ" SP mod-sequence-value /
       *                           "NOMODSEQ" /
       *                           "MODIFIED" SP set
       */
      RejectMalformed(respText, ImapResponseCode.HighestModSeq, 1);

      return ImapDataConverter.ToModSequenceValue(respText.Arguments[0]);
    }

    public static ImapSequenceSet FromModified(ImapResponseText respText, bool uid)
    {
      /*
       *    resp-text-code      =/ "HIGHESTMODSEQ" SP mod-sequence-value /
       *                           "NOMODSEQ" /
       *                           "MODIFIED" SP set
       */
      RejectMalformed(respText, ImapResponseCode.Modified, 1);

      return ImapDataConverter.ToSequenceSet(uid, respText.Arguments[0]);
    }

    /*
     * RFC 5464 - The IMAP METADATA Extension
     * http://tools.ietf.org/html/rfc5464
     */
    public static long FromMetadataLongEntries(ImapResponseText respText)
    {
      /*
       *       resp-text-code    =/ "METADATA" SP "LONGENTRIES" SP number
       *                              ; new response codes for GETMETADATA
       */
      RejectMalformed(respText, ImapResponseCode.MetadataLongEntries, 2);

      return ImapDataConverter.ToNumber(respText.Arguments[1]);
    }

    public static long FromMetadataMaxSize(ImapResponseText respText)
    {
      /*
       *       resp-text-code    =/ "METADATA" SP ("MAXSIZE" SP number /
       *                                           "TOOMANY" / "NOPRIVATE")
       *                           ; new response codes for SETMETADATA
       *                           ; failures
       */
      RejectMalformed(respText, ImapResponseCode.MetadataMaxSize, 2);

      return ImapDataConverter.ToNumber(respText.Arguments[1]);
    }

    /*
     * RFC 5466 - IMAP4 Extension for Named Searches (Filters)
     * http://tools.ietf.org/html/rfc5466
     */
    public static string FromUndefinedFilter(ImapResponseText respText)
    {
      /*
       *    resp-text-code        =/  "UNDEFINED-FILTER" SP filter-name
       *    filter-name           =  1*<any ATOM-CHAR except "/">
       *                          ;; Note that filter-name disallows UTF-8 or
       *                          ;; the following characters: "(", ")", "{",
       *                          ;; " ", "%", "*", "]".  See definition of
       *                          ;; ATOM-CHAR [RFC3501].
       */
      RejectMalformed(respText, ImapResponseCode.UndefinedFilter, 1);

      return ImapDataConverter.ToAString(respText.Arguments[0]);
    }

    private static void RejectMalformed(ImapResponseText respText, ImapResponseCode expectedCode, int expectedLength)
    {
      if (respText.Code != expectedCode)
        throw new ImapMalformedDataException(string.Format("expected response code is {0}, but was {1}", expectedCode, respText.Code));

      if (respText.Arguments.Length < expectedLength)
        throw new ImapMalformedDataException(string.Format("too few argument counts; expected is {0} but was {1}", expectedLength, respText.Arguments.Length));
    }
  }
}
