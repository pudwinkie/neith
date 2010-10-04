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
using System.IO;
using System.Reflection;
using System.Text;

using Smdn.Formats;

namespace Smdn.Net.Imap4.Protocol {
  public static class ImapDataConverter {
    /*
     * message-data    = nz-number SP ("EXPUNGE" / ("FETCH" SP msg-att))
     */
    private static ImapMessageAttributeBase CreateMessageAttributeInstance(Type type, long sequence)
    {
      if (type == typeof(ImapMessageDynamicAttribute))
        return new ImapMessageDynamicAttribute(sequence);
      else if (type == typeof(ImapMessageStaticAttribute))
        return new ImapMessageStaticAttribute(sequence);
      else if (type == typeof(ImapMessage))
        return new ImapMessage(sequence);
      else if (type == typeof(ImapMessageBody))
        return new ImapMessageBody(sequence);
      if (type == typeof(ImapMessageAttribute))
        return new ImapMessageAttribute(sequence);

      return (ImapMessageAttributeBase)Activator.CreateInstance(type,
                                                                BindingFlags.NonPublic | BindingFlags.Instance,
                                                                null,
                                                                new object[] {sequence},
                                                                null);
    }

    public static long ToFetchMessageData<TMessageAttribute>(IImapMessageAttributeCollection<TMessageAttribute> messages,
                                                             ImapData nzNumber,
                                                             ImapData msgAtt)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      var sequence = ImapDataConverter.ToNonZeroNumber(nzNumber);
      var message = messages.Find(sequence);

      if (message == null) {
        message = (TMessageAttribute)CreateMessageAttributeInstance(typeof(TMessageAttribute), sequence);

        messages.Add(message);
      }

      if (ToMsgAtt(message, msgAtt))
        return sequence;
      else
        return 0L;
    }

    public static void ToFetchMessageData<TMessageAttribute>(ref TMessageAttribute message, ImapData nzNumber, ImapData msgAtt)
      where TMessageAttribute : ImapMessageAttributeBase
    {
      if (message == null)
        message = (TMessageAttribute)CreateMessageAttributeInstance(typeof(TMessageAttribute), ToNonZeroNumber(nzNumber));

      ToMsgAtt(message, msgAtt);
    }

    public static bool ToMsgAtt(ImapMessageAttributeBase message, ImapData msgAtt)
    {
      /*
      msg-att         = "(" (msg-att-dynamic / msg-att-static)
                         *(SP (msg-att-dynamic / msg-att-static)) ")"
      msg-att-dynamic = "FLAGS" SP "(" [flag-fetch *(SP flag-fetch)] ")"
                          ; MAY change for a message
      msg-att-static  = "ENVELOPE" SP envelope / "INTERNALDATE" SP date-time /
                        "RFC822" [".HEADER" / ".TEXT"] SP nstring /
                        "RFC822.SIZE" SP number /
                        "BODY" ["STRUCTURE"] SP body /
                        "BODY" section ["<" number ">"] SP nstring /
                        "UID" SP uniqueid
                          ; MUST NOT change for a message

      section         = "[" [section-spec] "]"
      section-msgtext = "HEADER" / "HEADER.FIELDS" [".NOT"] SP header-list /
                        "TEXT"
                          ; top-level or MESSAGE/RFC822 part
      section-part    = nz-number *("." nz-number)
                          ; body part nesting
      section-spec    = section-msgtext / (section-part ["." section-text])
      section-text    = section-msgtext / "MIME"
                          ; text other than actual body part (headers, etc.)

      header-fld-name = astring
      header-list     = "(" header-fld-name *(SP header-fld-name) ")"
      */
      ExceptionIfInvalidFormat(msgAtt, ImapDataFormat.List);

      ImapMessageDynamicAttribute dynamicAttrs = null;
      ImapMessageStaticAttribute staticAttrs = null;
      ImapMessageBody body = null;

      var msg = message as ImapMessage;

      if (msg == null) {
        body = message as ImapMessageBody;

        var attr = message as ImapMessageAttribute;

        if (attr == null) {
          dynamicAttrs  = message as ImapMessageDynamicAttribute;
          staticAttrs   = message as ImapMessageStaticAttribute;
        }
        else {
          dynamicAttrs  = attr.GetDynamicAttributeImpl();
          staticAttrs   = attr.GetStaticAttributeImpl();
        }
      }
      else {
        dynamicAttrs  = msg.GetDynamicAttributeImpl();
        staticAttrs   = msg.GetStaticAttributeImpl();
        body          = msg;
      }

      var list = msgAtt.List;

      if (list.Length % 2 != 0) // must be 'att SP value'
        throw new ImapMalformedDataException(msgAtt);

      var converted = false;

      for (var index = 0; index < list.Length; index += 2) {
        ExceptionIfInvalidFormat(list[index], ImapDataFormat.Text);

        var att = list[index].GetTextAsByteString().ToUpper();

        /*
         * msg-att-dynamic
         */
        if (dynamicAttrs != null) {
          if (att.Equals("FLAGS")) {
            ExceptionIfInvalidFormat(list[index + 1], ImapDataFormat.List);
            dynamicAttrs.Flags = ToFlagFetch(list[index + 1].List);
            goto Done;
          }
          else if (att.Equals("MODSEQ")) {
            /*
             * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
             * http://tools.ietf.org/html/rfc4551
             *    fetch-mod-resp      = "MODSEQ" SP "(" permsg-modsequence ")"
             *    msg-att-dynamic     =/ fetch-mod-resp
             *    permsg-modsequence  = mod-sequence-value
             *                           ;; per message mod-sequence
             */
            ExceptionIfInvalidFormat(list[index + 1], ImapDataFormat.List);
            ExceptionIfTooFewData(list[index + 1].List, 1);
            dynamicAttrs.ModSeq = ToModSequenceValue(list[index + 1].List[0]);
            goto Done;
          }
        }

        /*
         * msg-att-static (except BODY[], RFC822, etc.)
         */
        if (staticAttrs != null) {
          if (att.Equals("ENVELOPE")) {
            staticAttrs.Envelope = ToEnvelope(list[index + 1]);
            goto Done;
          }
          else if (att.Equals("INTERNALDATE")) {
            staticAttrs.InternalDate = ToDateTime(list[index + 1]);
            goto Done;
          }
          else if (att.Equals("RFC822.SIZE")) {
            staticAttrs.Rfc822Size = ToNumber(list[index + 1]);
            goto Done;
          }
          else if (att.Equals("BODY") || att.Equals("BODYSTRUCTURE")) {
            staticAttrs.BodyStructure = ToBody(list[index + 1]);
            goto Done;
          }
          else if (att.Equals("UID")) {
            staticAttrs.Uid = ToUniqueId(list[index + 1]);
            goto Done;
          }
          else if (att.StartsWith("BINARY.SIZE")) {
            var sectionStart = att.IndexOf('[');
            var sectionEnd = att.IndexOf(']', sectionStart);

            staticAttrs.SetSectionBinarySize(att.Substring(sectionStart + 1, sectionEnd - sectionStart - 1).ToString(),
                                             ToNumber(list[index + 1]));
            goto Done;
          }
        }

        /*
         * msg-att-static (BODY[], RFC822, etc.)
         */
        if (body != null) {
          /*
           * RFC 3516 - IMAP4 Binary Content Extension
           * http://tools.ietf.org/html/rfc3516
           * 7. Formal Protocol Syntax
           *    msg-att-static =/  "BINARY" section-binary SP (nstring / literal8)
           *                       / "BINARY.SIZE" section-binary SP number
           */
          if (att.StartsWith("BODY[") ||
              att.Equals("RFC822") ||
              att.Equals("RFC822.HEADER") ||
              att.Equals("RFC822.TEXT") ||
              att.StartsWith("BINARY[")) {
            var text = list[index + 1];

            if (text.Format == ImapDataFormat.Text)
              body.SetContentData(att.ToString(), text);
            else if (text.Format == ImapDataFormat.Nil)
              body.SetContentData(att.ToString(), null);
            else
              throw new ImapMalformedDataException(string.Format("invalid data format; expected text or nil but was {0}", text.Format), text);

            goto Done;
          }
        }

        continue;

      Done:
        converted = true;
      } // for

      return converted;
    }

    public static ImapStatusAttributeList ToStatusAttList(ImapData[] statusAttList)
    {
      /*
      status-att      = "MESSAGES" / "RECENT" / "UIDNEXT" / "UIDVALIDITY" /
                        "UNSEEN"
      status-att-list =  status-att SP number *(SP status-att SP number)
      */

      /*
       * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
       * http://tools.ietf.org/html/rfc4551
       * 
       *    status-att-val      =/ "HIGHESTMODSEQ" SP mod-sequence-valzer
       *                           ;; extends non-terminal defined in [IMAPABNF].
       *                           ;; Value 0 denotes that the mailbox doesn't
       *                           ;; support persistent mod-sequences
       *                           ;; as described in Section 3.1.2
       */
      var attList = new ImapStatusAttributeList();

      if (statusAttList.Length % 2 != 0) // must be 'att SP value'
        throw new ImapMalformedDataException();

      for (var index = 0; index < statusAttList.Length; index += 2) {
        var att = ToString(statusAttList[index]);

        switch (att.ToUpperInvariant()) {
          case "MESSAGES":      attList.Messages      = ToNumber(statusAttList[index + 1]); break;
          case "RECENT":        attList.Recent        = ToNumber(statusAttList[index + 1]); break;
          case "UIDNEXT":       attList.UidNext       = ToNumber(statusAttList[index + 1]); break;
          case "UIDVALIDITY":   attList.UidValidity   = ToNumber(statusAttList[index + 1]); break;
          case "UNSEEN":        attList.Unseen        = ToNumber(statusAttList[index + 1]); break;
          case "HIGHESTMODSEQ": attList.HighestModSeq = ToModSequenceValzer(statusAttList[index + 1]); break;
          default:
            //Trace.Info("unsupported status-att: {0}", att);
            break;
        }
      }

      return attList;
    }

    public static ImapCapability ToCapability(ImapData capability)
    {
      /*
      capability      = ("AUTH=" auth-type) / atom
                          ; New capabilities MUST begin with "X" or be
                          ; registered with IANA as standard or
                          ; standards-track
       */
      ExceptionIfInvalidFormat(capability, ImapDataFormat.Text);

      return ImapCapability.GetKnownOrCreate(capability.GetTextAsString());
    }

    public static ImapCapabilityList ToCapability(ImapData[] list)
    {
      var capabilities = new ImapCapabilityList();

      foreach (var data in list) {
        var capa = ToCapability(data);

        if (!capabilities.Has(capa))
          capabilities.Add(capa);
      }

      return capabilities;
    }

    public static IImapMessageFlagSet ToFlagList(ImapData flagList)
    {
      /*
      flag            = "\Answered" / "\Flagged" / "\Deleted" /
                        "\Seen" / "\Draft" / flag-keyword / flag-extension
                          ; Does not include "\Recent"
      flag-extension  = "\" atom
                          ; Future expansion.  Client implementations
                          ; MUST accept flag-extension flags.  Server
                          ; implementations MUST NOT generate
                          ; flag-extension flags except as defined by
                          ; future standard or standards-track
                          ; revisions of this specification.
      flag-fetch      = flag / "\Recent"
      flag-keyword    = atom
      flag-list       = "(" [flag *(SP flag)] ")"
      flag-perm       = flag / "\*"
      */
      ExceptionIfInvalidFormat(flagList, ImapDataFormat.List);

      return ToFlags(flagList.List);
    }

    public static IImapMessageFlagSet ToFlagFetch(ImapData[] flags)
    {
      return ToFlags(flags, ImapMessageFlag.FetchFlags);
    }

    public static IImapMessageFlagSet ToFlagPerm(ImapData[] flags)
    {
      return ToFlags(flags, ImapMessageFlag.PermittedFlags);
    }

    public static IImapMessageFlagSet ToFlags(ImapData[] flags)
    {
      return ToFlags(flags, null);
    }

    private static IImapMessageFlagSet ToFlags(ImapData[] flags, IImapMessageFlagSet permittedSystemFlags)
    {
      var flagsList = new ImapMessageFlagList();

#pragma warning disable 642
      foreach (var flag in flags) {
        ExceptionIfInvalidFormat(flag, ImapDataFormat.Text);

        var f = ImapMessageFlag.GetKnownOrCreate(flag.GetTextAsString());

        if (permittedSystemFlags != null && f.IsSystemFlag && !permittedSystemFlags.Has(f))
          //Trace.Info("ignored invalid system flag: {0}", f);
          ;
        else if (!flagsList.Has(f))
          flagsList.Add(f);
      }
#pragma warning restore 642

      return flagsList.AsReadOnly();
    }

    public static ImapMailboxList ToMailboxList(ImapData[] mailboxList)
    {
      /*
      mailbox-list    = "(" [mbx-list-flags] ")" SP
                         (DQUOTE QUOTED-CHAR DQUOTE / nil) SP mailbox
      */
      ExceptionIfTooFewData(mailboxList, 3);

      var list = new ImapMailboxList(ToMbxListFlags(mailboxList[0].List),
                                     ToNString(mailboxList[1]),
                                     ToMailbox(mailboxList[2]));

      if (3 < mailboxList.Length) {
        /*
         * http://tools.ietf.org/html/rfc5258
         * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
         * 6. Formal Syntax
         * 
         *    mailbox-list =  "(" [mbx-list-flags] ")" SP
         *                (DQUOTE QUOTED-CHAR DQUOTE / nil) SP mailbox
         *                [SP mbox-list-extended]
         *                ; This is the list information pointed to by the ABNF
         *                ; item "mailbox-data", which is defined in [IMAP4]
         */
        var mboxListExtended = ToMboxListExtended(mailboxList[3]);

        if (mboxListExtended.ContainsKey("CHILDINFO"))
          list.ChildInfo = ToChildinfoExtendedItem(mboxListExtended["CHILDINFO"]);
      }

      return list;
    }

    public static IDictionary<string, ImapData> ToMboxListExtended(ImapData mboxListExtended)
    {
      /*
       * http://tools.ietf.org/html/rfc5258
       * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
       * 6. Formal Syntax
       * 
       *    mbox-list-extended =  "(" [mbox-list-extended-item
       *                *(SP mbox-list-extended-item)] ")"
       *    mbox-list-extended-item =  mbox-list-extended-item-tag SP
       *                tagged-ext-val
       *    mbox-list-extended-item-tag =  astring
       *                ; The content MUST conform to either "eitem-vendor-tag"
       *                ; or "eitem-standard-tag" ABNF productions.
       *                ; A tag registration template is described in this
       *                ; document in Section 9.5.
       * 
       *    tagged-ext-val =  tagged-ext-simple /
       *                "(" [tagged-ext-comp] ")"
       *    tagged-ext-simple =  sequence-set / number
       *    tagged-ext-comp =  astring /
       *                tagged-ext-comp *(SP tagged-ext-comp) /
       *                "(" tagged-ext-comp ")"
       *                ; Extensions that follow this general
       *                ; syntax should use nstring instead of
       *                ; astring when appropriate in the context
       *                ; of the extension.
       *                ; Note that a message set or a "number"
       *                ; can always be represented as an "atom".
       *                ; A URL should be represented as
       *                ; a "quoted" string.
       */
      ExceptionIfInvalidFormat(mboxListExtended, ImapDataFormat.List);

      var list = mboxListExtended.List;

      if (list.Length % 2 != 0)
        throw new ImapMalformedDataException("must be list of tag-value pairs", mboxListExtended);

      var taggedItems = new Dictionary<string, ImapData>(StringComparer.OrdinalIgnoreCase);

      for (var i = 0; i < list.Length; i += 2) {
        var tag = ToAString(list[i]);

        if (taggedItems.ContainsKey(tag))
          throw new ImapMalformedDataException("tag already contained", mboxListExtended);

        taggedItems.Add(tag, list[i + 1]);
      }

      return taggedItems;
    }

    private static ImapMailboxList.ListExtendedChildInfo ToChildinfoExtendedItem(ImapData childinfoExtendedItem)
    {
      /*
       * http://tools.ietf.org/html/rfc5258
       * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
       * 6. Formal Syntax
       * 
       *    childinfo-extended-item =  "CHILDINFO" SP "("
       *                list-select-base-opt-quoted
       *                *(SP list-select-base-opt-quoted) ")"
       *                ; Extended data item (mbox-list-extended-item)
       *                ; returned when the RECURSIVEMATCH
       *                ; selection option is specified.
       *                ; Note 1: the CHILDINFO tag can be returned
       *                ; with and without surrounding quotes, as per
       *                ; mbox-list-extended-item-tag production.
       *                ; Note 2: The selection options are always returned
       *                ; quoted, unlike their specification in
       *                ; the extended LIST command.
       * 
       *    list-select-base-opt =  "SUBSCRIBED" / option-extension
       *                ; options that can be used by themselves
       */
      ExceptionIfInvalidFormat(childinfoExtendedItem, ImapDataFormat.List);
      ExceptionIfTooFewData(childinfoExtendedItem.List, 1);

      var childInfo = new ImapMailboxList.ListExtendedChildInfo();

      foreach (var extval in childinfoExtendedItem.List) {
        if (extval.Format == ImapDataFormat.Text && IsTextEqualsTo(extval, "SUBSCRIBED"))
          childInfo.Subscribed = true;
      }

      return childInfo;
    }

    public static string ToMailbox(ImapData data)
    {
      /*
      mailbox         = "INBOX" / astring
                          ; INBOX is case-insensitive.  All case variants of
                          ; INBOX (e.g., "iNbOx") MUST be interpreted as INBOX
                          ; not as an astring.  An astring which consists of
                          ; the case-insensitive sequence "I" "N" "B" "O" "X"
                          ; is considered to be INBOX and not an astring.
                          ;  Refer to section 5.1 for further
                          ; semantic details of mailbox names.
      */
      return ModifiedUTF7.Decode(ToAString(data));
    }

    public static IImapMailboxFlagSet ToMbxListFlags(ImapData[] list)
    {
      /*
      mbx-list-flags  = *(mbx-list-oflag SP) mbx-list-sflag
                        *(SP mbx-list-oflag) /
                        mbx-list-oflag *(SP mbx-list-oflag)
      mbx-list-oflag  = "\Noinferiors" / flag-extension
                          ; Other flags; multiple possible per LIST response
      mbx-list-sflag  = "\Noselect" / "\Marked" / "\Unmarked"
                          ; Selectability flags; only one per LIST response
      */

      var flags = new ImapMailboxFlagList();

      foreach (var data in list) {
        ExceptionIfInvalidFormat(data, ImapDataFormat.Text);

        var flag = ImapMailboxFlag.GetKnownOrCreate(data.GetTextAsString());

        if (!flags.Has(flag))
          flags.Add(flag);
      }

      return flags.AsReadOnly();
    }

    public static ImapEnvelope ToEnvelope(ImapData envelope)
    {
      /*
      envelope        = "(" env-date SP env-subject SP env-from SP
                        env-sender SP env-reply-to SP env-to SP env-cc SP
                        env-bcc SP env-in-reply-to SP env-message-id ")"
      env-bcc         = "(" 1*address ")" / nil
      env-cc          = "(" 1*address ")" / nil
      env-date        = nstring
      env-from        = "(" 1*address ")" / nil
      env-in-reply-to = nstring
      env-message-id  = nstring
      env-reply-to    = "(" 1*address ")" / nil
      env-sender      = "(" 1*address ")" / nil
      env-subject     = nstring
      env-to          = "(" 1*address ")" / nil
      */
      ExceptionIfInvalidFormat(envelope, ImapDataFormat.List);
      ExceptionIfTooFewData(envelope.List, 10);

      return new ImapEnvelope(ToNString(envelope.List[0]), // env-date
                              ToNString(envelope.List[1]), // env-subject
                              ToNilOrAddressList(envelope.List[2]), // env-from
                              ToNilOrAddressList(envelope.List[3]), // env-sender
                              ToNilOrAddressList(envelope.List[4]), // env-reply-to
                              ToNilOrAddressList(envelope.List[5]), // env-to
                              ToNilOrAddressList(envelope.List[6]), // env-cc
                              ToNilOrAddressList(envelope.List[7]), // env-bcc
                              ToNString(envelope.List[8]), // env-in-reply-to
                              ToNString(envelope.List[9])); // env-message-id
    }

    private static ImapAddress[] ToNilOrAddressList(ImapData nilOrAddressList)
    {
      if (nilOrAddressList.Format == ImapDataFormat.Nil)
        //return null;
        return new ImapAddress[0];
      if (nilOrAddressList.Format == ImapDataFormat.List)
        return Array.ConvertAll<ImapData, ImapAddress>(nilOrAddressList.List, ToAddress);
      else
        throw new ImapMalformedDataException("must be nil or list", nilOrAddressList);
    }

    public static ImapAddress ToAddress(ImapData address)
    {
      /*
      address         = "(" addr-name SP addr-adl SP addr-mailbox SP
                        addr-host ")"
      addr-adl        = nstring
                          ; Holds route from [RFC-2822] route-addr if
                          ; non-NIL
      addr-host       = nstring
                          ; NIL indicates [RFC-2822] group syntax.
                          ; Otherwise, holds [RFC-2822] domain name
      addr-mailbox    = nstring
                          ; NIL indicates end of [RFC-2822] group; if
                          ; non-NIL and addr-host is NIL, holds
                          ; [RFC-2822] group name.
                          ; Otherwise, holds [RFC-2822] local-part
                          ; after removing [RFC-2822] quoting
      addr-name       = nstring
                          ; If non-NIL, holds phrase from [RFC-2822]
                          ; mailbox after removing [RFC-2822] quoting
      */
      ExceptionIfInvalidFormat(address, ImapDataFormat.List);
      ExceptionIfTooFewData(address.List, 4);

      return new ImapAddress(ToNString(address.List[0]),
                             ToNString(address.List[1]),
                             ToNString(address.List[2]),
                             ToNString(address.List[3]));
    }

    public static IImapBodyStructure ToBody(ImapData body)
    {
      return ToBody(body, new BodyStructureSectionIndices());
    }

    private class BodyStructureSectionIndices {
      public void Increment()
      {
        sectionIndices[level]++;
      }

      public int NestIn()
      {
        level++;

        if (sectionIndices.Length <= level)
          Array.Resize(ref sectionIndices, level + 1);

        sectionIndices[level] = 0;

        return level - 1;
      }

      public void NestOut(int nestLevel)
      {
        level = nestLevel;
      }

      public override string ToString()
      {
        if (level == 0)
          return string.Empty;

        var section = new StringBuilder();

        for (var i = 1; i <= level; i++) {
          if (i != 1)
            section.Append('.');

          section.Append(sectionIndices[i]);
        }

        return section.ToString();
      }

      private int level = 0;
      private int[] sectionIndices = new int[] {0};
    }

    private static IImapBodyStructure ToBody(ImapData body, BodyStructureSectionIndices sectionIndices)
    {
      /*
      body            = "(" (body-type-1part / body-type-mpart) ")"
      body-extension  = nstring / number /
                         "(" body-extension *(SP body-extension) ")"
                          ; Future expansion.  Client implementations
                          ; MUST accept body-extension fields.  Server
                          ; implementations MUST NOT generate
                          ; body-extension fields except as defined by
                          ; future standard or standards-track
                          ; revisions of this specification.
      body-ext-1part  = body-fld-md5 [SP body-fld-dsp [SP body-fld-lang
                        [SP body-fld-loc *(SP body-extension)]]]
                          ; MUST NOT be returned on non-extensible
                          ; "BODY" fetch
      body-ext-mpart  = body-fld-param [SP body-fld-dsp [SP body-fld-lang
                        [SP body-fld-loc *(SP body-extension)]]]
                          ; MUST NOT be returned on non-extensible
                          ; "BODY" fetch
      body-fields     = body-fld-param SP body-fld-id SP body-fld-desc SP
                        body-fld-enc SP body-fld-octets
      body-fld-desc   = nstring
      body-fld-dsp    = "(" string SP body-fld-param ")" / nil
      body-fld-enc    = (DQUOTE ("7BIT" / "8BIT" / "BINARY" / "BASE64"/
                        "QUOTED-PRINTABLE") DQUOTE) / string
      body-fld-id     = nstring
      body-fld-lang   = nstring / "(" string *(SP string) ")"
      body-fld-loc    = nstring
      body-fld-lines  = number
      body-fld-md5    = nstring
      body-fld-octets = number
      body-fld-param  = "(" string SP string *(SP string SP string) ")" / nil
      body-type-1part = (body-type-basic / body-type-msg / body-type-text)
                        [SP body-ext-1part]
      body-type-basic = media-basic SP body-fields
                          ; MESSAGE subtype MUST NOT be "RFC822"
      body-type-mpart = 1*body SP media-subtype
                        [SP body-ext-mpart]
      body-type-msg   = media-message SP body-fields SP envelope
                        SP body SP body-fld-lines
      body-type-text  = media-text SP body-fields SP body-fld-lines

      media-basic     = ((DQUOTE ("APPLICATION" / "AUDIO" / "IMAGE" /
                        "MESSAGE" / "VIDEO") DQUOTE) / string) SP
                        media-subtype
                          ; Defined in [MIME-IMT]
      media-message   = DQUOTE "MESSAGE" DQUOTE SP DQUOTE "RFC822" DQUOTE
                          ; Defined in [MIME-IMT]
      media-subtype   = string
                          ; Defined in [MIME-IMT]
      media-text      = DQUOTE "TEXT" DQUOTE SP media-subtype
                          ; Defined in [MIME-IMT]
       */
      ExceptionIfInvalidFormat(body, ImapDataFormat.List);
      ExceptionIfTooFewData(body.List, 1);

      if (body.List[0].Format == ImapDataFormat.Text)
        return ToBodyType1Part(body.List, sectionIndices);
      else if (body.List[0].Format == ImapDataFormat.List)
        return ToBodyTypeMPart(body.List, sectionIndices);
      else
        throw new ImapMalformedDataException(body);
    }

    private static IImapBodyStructure ToBodyType1Part(ImapData[] bodyType1Part, BodyStructureSectionIndices sectionIndices)
    {
      /*
      body            = "(" (body-type-1part / body-type-mpart) ")"
      body-type-1part = (body-type-basic / body-type-msg / body-type-text)
                        [SP body-ext-1part]
      body-type-basic = media-basic SP body-fields
                          ; MESSAGE subtype MUST NOT be "RFC822"
      body-type-msg   = media-message SP body-fields SP envelope
                        SP body SP body-fld-lines
      body-type-text  = media-text SP body-fields SP body-fld-lines

      media-basic     = ((DQUOTE ("APPLICATION" / "AUDIO" / "IMAGE" /
                        "MESSAGE" / "VIDEO") DQUOTE) / string) SP
                        media-subtype
                          ; Defined in [MIME-IMT]
      media-message   = DQUOTE "MESSAGE" DQUOTE SP DQUOTE "RFC822" DQUOTE
                          ; Defined in [MIME-IMT]
      media-text      = DQUOTE "TEXT" DQUOTE SP media-subtype
                          ; Defined in [MIME-IMT]
      */

      // basic fields
      //   type
      //     [0] type:        media-basic / media-text / media-message
      //     [1] subtype:     media-subtype
      //   body-fields
      //     [2] parameters:  body-fld-param
      //     [3] id:          body-fld-id
      //     [4] description: body-fld-desc
      //     [5] encoding:    body-fld-enc
      //     [6] size:        body-fld-octets
      //   <body-type-msg>       <body-type-text>       <body-type-basic>
      //     [7] envelope          [7] body-fld-lines     [7] (body-ext-1part)
      //     [8] body              [8] (body-ext-1part)
      //     [9] body-fld-lines
      //     [10] (body-ext-1part)
      ExceptionIfTooFewData(bodyType1Part, 7);

      sectionIndices.Increment();

      var index = 0;

      // type
      var mediaType = new MimeType(ToString(bodyType1Part[index++]),  // [0] type
                                   ToString(bodyType1Part[index++])); // [1] subtype

      // body-fields
      IImapBodyStructure basicFields = null;
      bool isMessage = false;

      if (mediaType.EqualsIgnoreCase("message/rfc822")) {
        var section = sectionIndices.ToString();
        var currentLevel = sectionIndices.NestIn();

        isMessage = true;

        // body-type-msg
        basicFields = new ImapMessageRfc822BodyStructure(section,
                                                         mediaType,
                                                         // body-fields
                                                         ToBodyFldParam(bodyType1Part[index++]),  // [2] parameters
                                                         ToNString(bodyType1Part[index++]),       // [3] id
                                                         ToNString(bodyType1Part[index++]),       // [4] description
                                                         ToNString(bodyType1Part[index++]),       // [5] encoding
                                                         ToNumber(bodyType1Part[index++]),        // [6] size
                                                         ToEnvelope(bodyType1Part[index++]),      // [7] envelope
                                                         ToBody(bodyType1Part[index++],           // [8] body
                                                                sectionIndices),
                                                         ToNumber(bodyType1Part[index++]));       // [9] body-fld-lines

        sectionIndices.NestOut(currentLevel);
      }
      else {
        // body-type-basic, body-type-text
        basicFields = new ImapSinglePartBodyStructure(sectionIndices.ToString(),
                                                      mediaType,
                                                      // body-fields
                                                      ToBodyFldParam(bodyType1Part[index++]),  // [2] parameters
                                                      ToNString(bodyType1Part[index++]),       // [3] id
                                                      ToNString(bodyType1Part[index++]),       // [4] description
                                                      ToNString(bodyType1Part[index++]),       // [5] encoding
                                                      ToNumber(bodyType1Part[index++]),        // [6] size
                                                      mediaType.TypeEqualsIgnoreCase("text")
                                                        // body-type-text
                                                        ? ToNumber(bodyType1Part[index++]) // [7] body-fld-lines
                                                        : 0L
                                                      );
      }

      if (bodyType1Part.Length == index)
        return basicFields;

      /*
       * extension data
       * 
       * body-ext-1part  = body-fld-md5 [SP body-fld-dsp [SP body-fld-lang
       *                   [SP body-fld-loc *(SP body-extension)]]]
       *                     ; MUST NOT be returned on non-extensible
       *                     ; "BODY" fetch
       * body-fld-md5    = nstring
       * body-fld-dsp    = "(" string SP body-fld-param ")" / nil
       * body-fld-lang   = nstring / "(" string *(SP string) ")"
       * body-fld-loc    = nstring
       * body-extension  = nstring / number /
       *                    "(" body-extension *(SP body-extension) ")"
       *                     ; Future expansion.  Client implementations
       *                     ; MUST accept body-extension fields.  Server
       *                     ; implementations MUST NOT generate
       *                     ; body-extension fields except as defined by
       *                     ; future standard or standards-track
       *                     ; revisions of this specification.
       */
      var extlen = bodyType1Part.Length;

      if (isMessage)
        return new ImapExtendedMessageRfc822BodyStructure(basicFields as ImapMessageRfc822BodyStructure,
                                                          (index < extlen) ? ToNString(bodyType1Part[index++]) : null,
                                                          (index < extlen) ? ToBodyFldDsp(bodyType1Part[index++]) : null,
                                                          (index < extlen) ? ToBodyFldLang(bodyType1Part[index++]) : null,
                                                          (index < extlen) ? ToBodyFldLoc(bodyType1Part[index++]) : null,
                                                          (index < extlen) ? bodyType1Part.Slice(index++) : null);
      else
        return new ImapExtendedSinglePartBodyStructure(basicFields as ImapSinglePartBodyStructure,
                                                       (index < extlen) ? ToNString(bodyType1Part[index++]) : null,
                                                       (index < extlen) ? ToBodyFldDsp(bodyType1Part[index++]) : null,
                                                       (index < extlen) ? ToBodyFldLang(bodyType1Part[index++]) : null,
                                                       (index < extlen) ? ToBodyFldLoc(bodyType1Part[index++]) : null,
                                                       (index < extlen) ? bodyType1Part.Slice(index++) : null);
    }

    private static IDictionary<string, string> ToBodyFldParam(ImapData bodyFldParam)
    {
      /*
      body-fld-param  = "(" string SP string *(SP string SP string) ")" / nil
      */
      if (bodyFldParam.Format == ImapDataFormat.Nil)
        return null;

      ExceptionIfInvalidFormat(bodyFldParam, ImapDataFormat.List);

      if (bodyFldParam.List.Length % 2 != 0)
        throw new ImapMalformedDataException("must be list of attribute/value pairs", bodyFldParam);

      var list = bodyFldParam.List;
      var pairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      for (var param = 0; param < list.Length; param += 2) {
        pairs.Add(ToString(list[param]), ToString(list[param + 1]));
      }

      return pairs;
    }

    private static ImapBodyDisposition ToBodyFldDsp(ImapData bodyFldDsp)
    {
      /*
       * body-fld-dsp    = "(" string SP body-fld-param ")" / nil
       */
      if (bodyFldDsp.Format == ImapDataFormat.List) {
        ExceptionIfTooFewData(bodyFldDsp.List, 2);

        return new ImapBodyDisposition(ToString(bodyFldDsp.List[0]),
                                       ToBodyFldParam(bodyFldDsp.List[1]));
      }
      else if (bodyFldDsp.Format == ImapDataFormat.Nil) {
        return null;
      }
      else {
        throw new ImapMalformedDataException("must be list or nil", bodyFldDsp);
      }
    }

    private static string[] ToBodyFldLang(ImapData bodyFldLang)
    {
      /*
       * body-fld-lang   = nstring / "(" string *(SP string) ")"
       */
      if (bodyFldLang.Format == ImapDataFormat.List) {
        ExceptionIfTooFewData(bodyFldLang.List, 1);

        return Array.ConvertAll<ImapData, string>(bodyFldLang.List, ToString);
      }
      else {
        var str = ToNString(bodyFldLang);

        if (str == null)
          return null;
        else
          return new[] {str};
      }
    }

    private static Uri ToBodyFldLoc(ImapData bodyFldLoc)
    {
      /*
       * body-fld-loc    = nstring
       */
      if (bodyFldLoc.Format == ImapDataFormat.Nil)
        return null;
      else
        return ToUri(bodyFldLoc);
    }

    private static ImapMultiPartBodyStructure ToBodyTypeMPart(ImapData[] bodyTypeMPart, BodyStructureSectionIndices sectionIndices)
    {
      /*
      body            = "(" (body-type-1part / body-type-mpart) ")"
      body-type-mpart = 1*body SP media-subtype
                        [SP body-ext-mpart]
      media-subtype   = string
                          ; Defined in [MIME-IMT]
      body-ext-mpart  = body-fld-param [SP body-fld-dsp [SP body-fld-lang
                        [SP body-fld-loc *(SP body-extension)]]]
                          ; MUST NOT be returned on non-extensible
                          ; "BODY" fetch
      */
      ExceptionIfTooFewData(bodyTypeMPart, 2);

      sectionIndices.Increment();

      var currentLevel = sectionIndices.NestIn();
      var index = 0;

      // body
      var nestedBodyStructures = new List<IImapBodyStructure>();

      for (; index < bodyTypeMPart.Length; index++) {

        if (bodyTypeMPart[index].Format == ImapDataFormat.List)
          nestedBodyStructures.Add(ToBody(bodyTypeMPart[index], sectionIndices));
        else if (bodyTypeMPart[index].Format == ImapDataFormat.Text)
          break;
        else
          throw new ImapMalformedDataException(bodyTypeMPart[index]);
      }

      if (bodyTypeMPart.Length <= index)
        throw new ImapMalformedDataException("missing media-subtype");

      sectionIndices.NestOut(currentLevel);

      var basicFields = new ImapMultiPartBodyStructure(sectionIndices.ToString(),
                                                       nestedBodyStructures.ToArray(),
                                                       ToString(bodyTypeMPart[index++])); // media-subtype

      if (bodyTypeMPart.Length == index)
        return basicFields;

      /*
       * extension data
       * 
       * body-ext-mpart  = body-fld-param [SP body-fld-dsp [SP body-fld-lang
       *                   [SP body-fld-loc *(SP body-extension)]]]
       *                     ; MUST NOT be returned on non-extensible
       *                     ; "BODY" fetch
       * body-fld-dsp    = "(" string SP body-fld-param ")" / nil
       * body-fld-lang   = nstring / "(" string *(SP string) ")"
       * body-fld-loc    = nstring
       * body-extension  = nstring / number /
       *                    "(" body-extension *(SP body-extension) ")"
       *                     ; Future expansion.  Client implementations
       *                     ; MUST accept body-extension fields.  Server
       *                     ; implementations MUST NOT generate
       *                     ; body-extension fields except as defined by
       *                     ; future standard or standards-track
       *                     ; revisions of this specification.
       */
      var extlen = bodyTypeMPart.Length;

      return new ImapExtendedMultiPartBodyStructure(basicFields,
                                                    (index < extlen) ? ToBodyFldParam(bodyTypeMPart[index++]) : null,
                                                    (index < extlen) ? ToBodyFldDsp(bodyTypeMPart[index++]) : null,
                                                    (index < extlen) ? ToBodyFldLang(bodyTypeMPart[index++]) : null,
                                                    (index < extlen) ? ToBodyFldLoc(bodyTypeMPart[index++]) : null,
                                                    (index < extlen) ? bodyTypeMPart.Slice(index++) : null);
    }

    public static ImapSequenceSet ToUidSet(ImapData sequenceSet)
    {
      return ToSequenceSet(true, sequenceSet);
    }

    public static ImapSequenceSet ToSequenceSet(ImapData sequenceSet)
    {
      return ToSequenceSet(false, sequenceSet);
    }

    private static readonly ByteString seqNumberWildcard = new ByteString((byte)'*');

    public static ImapSequenceSet ToSequenceSet(bool uid, ImapData sequenceSet)
    {
      // seq-number      = nz-number / "*"
      // seq-range       = seq-number ":" seq-number
      // sequence-set    = (seq-number / seq-range) *("," sequence-set)
      ExceptionIfInvalidFormat(sequenceSet, ImapDataFormat.Text);

      var sets = sequenceSet.GetTextAsByteString().Split(',');
      var numbers = new List<long>(sets.Length);
      ImapSequenceSet result = null;

      for (var i = 0;;) {
        ImapSequenceSet current = null;

        var sep = sets[i].IndexOf(':');

        if (0 <= sep) {
          var from = sets[i].Substring(0, sep);
          var to = sets[i].Substring(sep + 1);

          if (from == seqNumberWildcard) // *:number
            current = ImapSequenceSet.CreateToSet(uid, ToNonZeroNumber(to));
          else if (to == seqNumberWildcard) // number:*
            current = ImapSequenceSet.CreateFromSet(uid, ToNonZeroNumber(from));
          else // number:number
            current = ImapSequenceSet.CreateRangeSet(uid, ToNonZeroNumber(from), ToNonZeroNumber(to));
        }
        else if (sets[i] == seqNumberWildcard) {
          // *
          current = ImapSequenceSet.CreateAllSet(uid);
        }
        else {
          // number
          numbers.Add(ToNonZeroNumber(sets[i]));
        }

        i++;

        if (current != null || i == sets.Length) {
          if (0 < numbers.Count) {
            result = ImapSequenceSet.Combine(result, ImapSequenceSet.CreateSet(uid, numbers.ToArray()));
            numbers.Clear();
          }

          if (current != null)
            result = ImapSequenceSet.Combine(result, current);
        }

        if (i == sets.Length)
          break;
      }

      return result;
    }

    public static ImapThreadList ToThreadList(bool uid, ImapData[] threadLists)
    {
      var threads = new List<ImapThreadList>();

      foreach (var threadList in threadLists) {
        threads.AddRange(ToThreadList(uid, threadList));
      }

      return ImapThreadList.CreateRootedList(uid, threads.ToArray());
    }

    public static ImapThreadList[] ToThreadList(bool uid, ImapData threadList)
    {
      /*
         thread-list     = "(" (thread-members / thread-nested) ")"
         thread-members  = nz-number *(SP nz-number) [SP thread-nested]
         thread-nested   = 2*thread-list
      */
      ExceptionIfInvalidFormat(threadList, ImapDataFormat.List);
      ExceptionIfTooFewData(threadList.List, 1);

      if (threadList.List[0].Format == ImapDataFormat.Text)
        return new ImapThreadList[] {ToThreadMembers(uid, threadList.List)};
      else if (threadList.List[0].Format == ImapDataFormat.List)
        return ToThreadNested(uid, threadList.List, 0);
      else
        throw new ImapMalformedDataException(threadList);
    }

    public static ImapThreadList ToThreadMembers(bool uid, ImapData[] threadMembers)
    {
      /*
         thread-list     = "(" (thread-members / thread-nested) ")"
         thread-members  = nz-number *(SP nz-number) [SP thread-nested]
         thread-nested   = 2*thread-list
      */
      var index = 0;

      // thread-members
      var members = new Stack<long>();

      for (; index < threadMembers.Length; index++) {
        if (threadMembers[index].Format == ImapDataFormat.List)
          break;

        ExceptionIfInvalidFormat(threadMembers[index], ImapDataFormat.Text);

        members.Push(ToNonZeroNumber(threadMembers[index]));
      }

      var threadList = (index < threadMembers.Length) ?
        new ImapThreadList(uid, members.Pop(), ToThreadNested(uid, threadMembers, index)) : // thread-nested
        new ImapThreadList(uid, members.Pop());

      while (0 < members.Count) {
        threadList = new ImapThreadList(uid, members.Pop(), threadList);
      }

      return threadList;
    }

    public static ImapThreadList[] ToThreadNested(bool uid, ImapData[] list)
    {
      return ToThreadNested(uid, list, 0);
    }

    private static ImapThreadList[] ToThreadNested(bool uid, ImapData[] threadNested, int index)
    {
      /*
         thread-list     = "(" (thread-members / thread-nested) ")"
         thread-members  = nz-number *(SP nz-number) [SP thread-nested]
         thread-nested   = 2*thread-list
      */
      if (threadNested.Length - index < 2)
        throw new ImapMalformedDataException("at least 2 thread-lists is required");

      var nested = new List<ImapThreadList>();

      for (var i = 0; index < threadNested.Length; i++, index++) {
        nested.AddRange(ToThreadList(uid, threadNested[index]));
      }

      return nested.ToArray();
    }

    /*
     * RFC 2087 - IMAP4 QUOTA extension
     * http://tools.ietf.org/html/rfc2087
     */
    public static ImapQuotaResource[] ToQuotaList(ImapData quotaList)
    {
      /*
       *    quota_list      ::= "(" #quota_resource ")"
       *    quota_resource  ::= atom SP number SP number
       */
      ExceptionIfInvalidFormat(quotaList, ImapDataFormat.List);

      var list = quotaList.List;

      if (list.Length % 3 != 0)
        throw new ImapMalformedDataException("must be list of triplets", quotaList);

      var quotaResources = new List<ImapQuotaResource>(list.Length / 3);

      for (var i = 0; i < list.Length;) {
        quotaResources.Add(new ImapQuotaResource(ToAString(list[i++]),
                                                 ToNumber(list[i++]),
                                                 ToNumber(list[i++])));
      }

      return quotaResources.ToArray();
    }

    /*
     * RFC 5464 - The IMAP METADATA Extension
     * http://tools.ietf.org/html/rfc5464
     */
    public static string[] ToEntryList(ImapData[] entryList)
    {
      /*
       *       entry-list        = entry *(SP entry)
       *                           ; list of entries used in unsolicited
       *                           ; METADATA response
       *       entry             = astring
       *                           ; slash-separated path to entry
       *                           ; MUST NOT contain "*" or "%"
       */
      ExceptionIfTooFewData(entryList, 1);

      return Array.ConvertAll<ImapData, string>(entryList, ToAString);
    }

    public static ImapMetadata[] ToEntryValues(ImapData entryValues)
    {
      /* 
       *       entry-values      = "(" entry-value *(SP entry-value) ")"
       *       entry-value       = entry SP value
       *       entry             = astring
       *                           ; slash-separated path to entry
       *                           ; MUST NOT contain "*" or "%"
       *       value             = nstring / literal8
       */
      ExceptionIfInvalidFormat(entryValues, ImapDataFormat.List);

      var list = entryValues.List;

      if (list.Length % 2 != 0)
        throw new ImapMalformedDataException("must be list of entry-value pairs", entryValues);

      var values = new List<ImapMetadata>(list.Length / 2);

      for (var i = 0; i < list.Length; i += 2) {
        values.Add(new ImapMetadata(ImapMetadata.SplitEntryName(ToAString(list[i])),
                                    ToNString(list[i + 1])));
      }

      return values.ToArray();
    }

    /*
     * RFC 2971 IMAP4 ID extension
     * http://tools.ietf.org/html/rfc2971
     */
    public static IDictionary<string, string> ToIdParamsList(ImapData idParamsList)
    {
      // 3.3. Defined Field Values
      //    Strings are not case-sensitive.
      // 4. Formal Syntax
      //      id_params_list ::= "(" #(string SPACE nstring) ")" / nil
      //          ;; list of field value pairs
      var paramsList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      if (idParamsList.Format == ImapDataFormat.Nil)
        return paramsList;

      ExceptionIfInvalidFormat(idParamsList, ImapDataFormat.List);

      var list = idParamsList.List;

      if (list.Length % 2 != 0)
        throw new ImapMalformedDataException("must be list of field value pairs", idParamsList);

      for (var i = 0; i < list.Length; i += 2) {
        var field = ToString(list[i]);

        if (paramsList.ContainsKey(field))
          throw new ImapMalformedDataException("field already contained", idParamsList);

        paramsList.Add(field, ToNString(list[i + 1]));
      }

      return paramsList;
    }

    /*
     * RFC 2342 IMAP4 Namespace
     * http://tools.ietf.org/html/rfc2342
     */
    public static ImapNamespaceDesc[] ToNamespace(ImapData @namespace)
    {
      /*
       Namespace = nil / "(" 1*( "(" string SP  (<"> QUOTED_CHAR <"> /
          nil) *(Namespace_Response_Extension) ")" ) ")"
       Namespace_Response_Extension = SP string SP "(" string *(SP string)
          ")"

       string = <string>
          ; <string> as defined in [RFC-2060]
          ; Note that  the namespace prefix is to a mailbox and following
          ; IMAP4 convention, any international string in the NAMESPACE
          ; response MUST be of modified UTF-7 format as described in
          ;  [RFC-2060].
      */
      if (@namespace.Format == ImapDataFormat.Nil)
        return new ImapNamespaceDesc[] {};

      ExceptionIfInvalidFormat(@namespace, ImapDataFormat.List);
      ExceptionIfTooFewData(@namespace.List, 1);

      var list = @namespace.List;

      var namespaces = new List<ImapNamespaceDesc>();

      foreach (var nslist in list) {
        ExceptionIfInvalidFormat(nslist, ImapDataFormat.List);
        ExceptionIfTooFewData(nslist.List, 1);

        var prefix = ModifiedUTF7.Decode(ToString(nslist.List[0]));

        if (2 <= nslist.List.Length) {
          var delimiter = ToString(nslist.List[1]);

          if (3 <= nslist.List.Length)
            namespaces.Add(new ImapNamespaceDesc(prefix, delimiter, ToNamespaceResponseExtension(nslist.List, 2, nslist.List.Length - 2)));
          else
            namespaces.Add(new ImapNamespaceDesc(prefix, delimiter));
        }
        else {
          namespaces.Add(new ImapNamespaceDesc(prefix));
        }
      }

      return namespaces.ToArray();
    }

    private static IDictionary<string, string[]> ToNamespaceResponseExtension(ImapData[] extensions, int index, int count)
    {
      if (count % 2 != 0)
        throw new ImapMalformedDataException("must be list of name value pairs");

      var exts = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

      for (var i = 0; i < count; i += 2, index += 2) {
        ExceptionIfInvalidFormat(extensions[index + 1], ImapDataFormat.List);

        exts.Add(ToString(extensions[index]), Array.ConvertAll<ImapData, string>(extensions[index + 1].List, ToString));
      }

      return exts;
    }

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     */
    public static string ToLangTagQuoted(ImapData langTagQuoted)
    {
      /*
       *     language-data     = "LANGUAGE" SP "(" lang-tag-quoted *(SP
       *                       lang-tag-quoted) ")"
       *     lang-range-quoted = astring
       *         ; Once any literal wrapper or quoting is removed, this
       *         ; follows the language-range rule in [RFC4647]
       *     lang-tag-quoted   = astring
       *         ; Once any literal wrapper or quoting is removed, this follows
       *         ; the Language-Tag rule in [RFC4646]
       */
      return ToAString(langTagQuoted);
    }

    public static ImapCollationAlgorithm ToCompSelQuoted(ImapData compSelQuoted)
    {
      /*
       *     comp-sel-quoted   = astring
       *         ; Once any literal wrapper or quoting is removed, this
       *         ; follows the collation-selected rule from [RFC4790]
       */
      return ImapCollationAlgorithm.GetKnownOrCreate(ToAString(compSelQuoted));
    }

    public static ImapCollationAlgorithm ToCompOrderQuoted(ImapData compOrderQuoted)
    {
      /*
       *     comp-order-quoted = astring
       *         ; Once any literal wrapper or quoting is removed, this
       *         ; follows the collation-order rule from [RFC4790]
       */
      return ImapCollationAlgorithm.GetKnownOrCreate(ToAString(compOrderQuoted));
    }

    /*
     * RFC 4466 - Collected Extensions to IMAP4 ABNF
     * http://tools.ietf.org/html/rfc4466
     */
    public static string ToSearchCorrelator(ImapData searchCorrelator)
    {
      /*
       *    search-correlator  = SP "(" "TAG" SP tag-string ")"
       */
      ExceptionIfInvalidFormat(searchCorrelator, ImapDataFormat.List);
      ExceptionIfTooFewData(searchCorrelator.List, 2);

      if (!IsTextEqualsToCaseInsensitive(searchCorrelator.List[0], "TAG"))
        throw new ImapMalformedDataException(searchCorrelator.List[0]);

      return ToString(searchCorrelator.List[1]);
    }

    public static ImapMatchedSequenceSet ToSearchReturnData(ImapData[] searchReturnData, string tag, bool uid, int startIndex)
    {
      /*
       *    search-return-data = search-modifier-name SP search-return-value
       *                         ;; Note that not every SEARCH return option
       *                         ;; is required to have the corresponding
       *                         ;; ESEARCH return data.
       *    search-return-value = tagged-ext-val
       *                         ;; Data for the returned search option.
       *                         ;; A single "nz-number"/"number" value
       *                         ;; can be returned as an atom (i.e., without
       *                         ;; quoting).  A sequence-set can be returned
       *                         ;; as an atom as well.
       *    search-modifier-name = tagged-ext-label
       */
      if ((searchReturnData.Length - startIndex) % 2 != 0)
        throw new ImapMalformedDataException("must be list of name/value pairs");

      long? min = null;
      long? max = null;
      long? count = null;
      ulong? modSeq = null;
      ImapSequenceSet all = null;

      for (var index = startIndex; index < searchReturnData.Length; index += 2) {
        switch (ToString(searchReturnData[index]).ToUpperInvariant()) {
          /*
           * RFC 4731 - IMAP4 Extension to SEARCH Command for Controlling What Kind of Information Is Returned
           * http://tools.ietf.org/html/rfc4731
           *      search-return-data = "MIN" SP nz-number /
           *                           "MAX" SP nz-number /
           *                           "ALL" SP sequence-set /
           *                           "COUNT" SP number
           *                           ;; conforms to the generic
           *                           ;; search-return-data syntax defined
           *                           ;; in [IMAPABNF]
           * 
           *      When the CONDSTORE [CONDSTORE] IMAP extension is also supported,
           *      the ABNF is updated as follows:
           * 
           *      search-return-data =/ "MODSEQ" SP mod-sequence-value
           *                           ;; mod-sequence-value is defined
           *                           ;; in [CONDSTORE]
           */
          case "MIN":
            min = ToNonZeroNumber(searchReturnData[index + 1]);
            break;

          case "MAX":
            max = ToNonZeroNumber(searchReturnData[index + 1]);
            break;

          case "ALL":
            all = ToSequenceSet(uid, searchReturnData[index + 1]);
            break;

          case "COUNT":
            count = ToNonZeroNumber(searchReturnData[index + 1]);
            break;

          case "MODSEQ":
            modSeq = ToModSequenceValue(searchReturnData[index + 1]);
            break;
        }
      }

      var returnData = (all == null)
        ? ImapMatchedSequenceSet.CreateEmpty(uid)
        : new ImapMatchedSequenceSet(all);

      returnData.Tag = tag;
      returnData.Max = max;
      returnData.Min = min;
      returnData.Count = count;
      returnData.HighestModSeq = modSeq;

      return returnData;
    }

    public static DateTimeOffset ToDateTime(ImapData dateTime)
    {
      ExceptionIfInvalidFormat(dateTime, ImapDataFormat.Text);

      return ImapDateTimeFormat.FromDateTimeString(dateTime.GetTextAsString());
    }

    public static string ToString(ImapData @string)
    {
      ExceptionIfInvalidFormat(@string, ImapDataFormat.Text);

      return @string.GetTextAsString();
    }

    public static string ToAString(ImapData astring)
    {
      /*
      astring         = 1*ASTRING-CHAR / string
      ASTRING-CHAR   = ATOM-CHAR / resp-specials
      atom            = 1*ATOM-CHAR
      ATOM-CHAR       = <any CHAR except atom-specials>
      atom-specials   = "(" / ")" / "{" / SP / CTL / list-wildcards /
                        quoted-specials / resp-specials
      */
      ExceptionIfInvalidFormat(astring, ImapDataFormat.Text);

      return astring.GetTextAsString();
    }

    public static string ToNString(ImapData nstring)
    {
      /*
      nstring         = string / nil
      */
      if (nstring.Format == ImapDataFormat.Text)
        return nstring.GetTextAsString();
      else if (nstring.Format == ImapDataFormat.Nil)
        return null;
      else
        throw new ImapMalformedDataException("nstring must be nil or text", nstring);
    }

    [CLSCompliant(false)]
    public static ulong ToModSequenceValue(ImapData modSequenceValue)
    {
      /*
       mod-sequence-value  = 1*DIGIT
                              ;; Positive unsigned 64-bit integer
                              ;; (mod-sequence)
                              ;; (1 <= n < 18,446,744,073,709,551,615)
      */
      var val = ToModSequenceValzer(modSequenceValue);

      if (val == 0)
        throw new ImapMalformedDataException(string.Format("mod-sequence-value must be non-zero positive number, but was {0}", val));

      return val;
    }

    [CLSCompliant(false)]
    public static ulong ToModSequenceValzer(ImapData modSequenceValzer)
    {
      /*
        mod-sequence-valzer = "0" / mod-sequence-value
      */
      ExceptionIfInvalidFormat(modSequenceValzer, ImapDataFormat.Text);

      return modSequenceValzer.GetTextAsNumber();
    }

    public static long ToUniqueId(ImapData uniqueId)
    {
      /*
      uniqueid        = nz-number
                          ; Strictly ascending
      */
      return ToNonZeroNumber(uniqueId);
    }

    public static long ToNonZeroNumber(ImapData nzNumber)
    {
      var val = ToNumber(nzNumber);

      if (val == 0)
        throw new ImapMalformedDataException(string.Format("nz-number must be non-zero positive number, but was {0}", val));

      return val;
    }

    internal static long ToNonZeroNumber(string nzNumber)
    {
      /*
      nz-number       = digit-nz *DIGIT
                          ; Non-zero unsigned 32-bit integer
                          ; (0 < n < 4,294,967,296)
      */
      var val = ToNumber(nzNumber);

      if (val == 0L)
        throw new ImapMalformedDataException(string.Format("nz-number must be non-zero positive number, but was {0}", val));

      return val;
    }

    internal static long ToNonZeroNumber(ByteString nzNumber)
    {
      var val = checked((long)nzNumber.ToUInt64());

      if (val == 0L)
        throw new ImapMalformedDataException(string.Format("nz-number must be non-zero positive number, but was {0}", val));

      return val;
    }

    public static long ToNumber(ImapData number)
    {
      ExceptionIfInvalidFormat(number, ImapDataFormat.Text);

      return checked((long)number.GetTextAsNumber());
    }

    internal static long ToNumber(string number)
    {
      /*
      number          = 1*DIGIT
                          ; Unsigned 32-bit integer
                          ; (0 <= n < 4,294,967,296)
      */
      try {
        var val = long.Parse(number);

        if (val < 0)
          throw new ImapMalformedDataException(string.Format("number must be zero or positive number, but was {0}", val));

        return val;
      }
      catch (FormatException) {
        throw new ImapMalformedDataException(number);
      }
    }

    public static Uri ToUri(ImapData uri)
    {
      ExceptionIfInvalidFormat(uri, ImapDataFormat.Text);

      try {
        return new Uri(uri.GetTextAsString());
      }
      catch {
        throw new ImapMalformedDataException(uri);
      }
    }

    /*
     * utility methods
     */

    public static bool IsTextEqualsTo(ImapData data, string text)
    {
      ExceptionIfInvalidFormat(data, ImapDataFormat.Text);

      return string.Equals(data.GetTextAsString(), text, StringComparison.Ordinal);
    }

    public static bool IsTextEqualsToCaseInsensitive(ImapData data, string text)
    {
      ExceptionIfInvalidFormat(data, ImapDataFormat.Text);

      return string.Equals(data.GetTextAsString(), text, StringComparison.OrdinalIgnoreCase);
    }

    private static void ExceptionIfInvalidFormat(ImapData data, ImapDataFormat expectedFormat)
    {
      if (data.Format != expectedFormat)
        throw new ImapMalformedDataException(string.Format("invalid data format; expected format is {0} but was {1}", expectedFormat, data.Format), data);
    }

    private static void ExceptionIfTooFewData(ImapData[] data, int expectedCount)
    {
      if (data.Length < expectedCount)
        throw new ImapMalformedDataException(string.Format("too few data counts; expected is {0} but was {1}", expectedCount, data.Length));
    }
  }
}