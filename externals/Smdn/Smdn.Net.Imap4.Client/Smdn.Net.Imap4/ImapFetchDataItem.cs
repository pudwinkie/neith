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
  // combinable data item types:
  //   ImapCombinableDataItem
  //     => handles 'data item names' etc.
  //   * ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //     ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  // 6.4.5. FETCH Command
  //   message data item names or macro
  public sealed class ImapFetchDataItem : ImapCombinableDataItem, IImapExtension {
    // ALL
    //    Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE)
    public static readonly ImapFetchDataItem All
      = new ImapFetchDataItem("ALL", true, "FLAGS", "INTERNALDATE", "RFC822.SIZE", "ENVELOPE");

    // FAST
    //    Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE)
    public static readonly ImapFetchDataItem Fast
      = new ImapFetchDataItem("FAST", true, "FLAGS", "INTERNALDATE", "RFC822.SIZE");

    // FULL
    //    Macro equivalent to: (FLAGS INTERNALDATE RFC822.SIZE ENVELOPE
    //    BODY)
    public static readonly ImapFetchDataItem Full
      = new ImapFetchDataItem("FULL", true, "FLAGS", "INTERNALDATE", "RFC822.SIZE", "ENVELOPE", "BODY");

    internal static readonly ImapFetchDataItem Extensible
      = new ImapFetchDataItem("FLAGS", "INTERNALDATE", "RFC822.SIZE", "ENVELOPE", "BODYSTRUCTURE");

    public static ImapFetchDataItem FromMacro(ImapFetchDataItemMacro macro)
    {
      switch (macro) {
        case ImapFetchDataItemMacro.All: return All;
        case ImapFetchDataItemMacro.Fast: return Fast;
        case ImapFetchDataItemMacro.Full: return Full;
        case ImapFetchDataItemMacro.Extensible: return Extensible;
        default: throw ExceptionUtils.CreateNotSupportedEnumValue(macro);
      }
    }

    // BODY
    //     Non-extensible form of BODYSTRUCTURE.
    public static readonly ImapFetchDataItem Body
      = new ImapFetchDataItem("BODY");

    // BODY[<section>]<<partial>>
    //     The text of a particular body section.  The section
    //     specification is a set of zero or more part specifiers
    //     delimited by periods.  A part specifier is either a part number
    //     or one of the following: HEADER, HEADER.FIELDS,
    //     HEADER.FIELDS.NOT, MIME, and TEXT.  An empty section
    //     specification refers to the entire message, including the
    //     header.
    public static ImapFetchDataItem BodyText()
    {
      return BodyTextInternal(false, null, null);
    }

    public static ImapFetchDataItem BodyText(string section)
    {
      return BodyTextInternal(false, section, null);
    }

    public static ImapFetchDataItem BodyText(long partialStart, long partialLength)
    {
      return BodyTextInternal(false, null, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BodyText(ImapPartialRange? @partial)
    {
      return BodyTextInternal(false, null, @partial);
    }

    public static ImapFetchDataItem BodyText(string section, long partialStart, long partialLength)
    {
      return BodyTextInternal(false, section, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BodyText(string section, ImapPartialRange? @partial)
    {
      return BodyTextInternal(false, section, @partial);
    }

    // BODY.PEEK[<section>]<<partial>>
    //     An alternate form of BODY[<section>] that does not implicitly
    //     set the \Seen flag.
    public static ImapFetchDataItem BodyPeek()
    {
      return BodyTextInternal(true, null, null);
    }

    public static ImapFetchDataItem BodyPeek(string section)
    {
      return BodyTextInternal(true, section, null);
    }

    public static ImapFetchDataItem BodyPeek(long partialStart, long partialLength)
    {
      return BodyTextInternal(true, null, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BodyPeek(ImapPartialRange? @partial)
    {
      return BodyTextInternal(true, null, @partial);
    }

    public static ImapFetchDataItem BodyPeek(string section, long partialStart, long partialLength)
    {
      return BodyTextInternal(true, section, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BodyPeek(string section, ImapPartialRange? @partial)
    {
      return BodyTextInternal(true, section, @partial);
    }

    public static ImapFetchDataItem BodyText(bool peek)
    {
      return BodyTextInternal(peek, null, null);
    }

    public static ImapFetchDataItem BodyText(bool peek, string section)
    {
      return BodyTextInternal(peek, section, null);
    }

    public static ImapFetchDataItem BodyText(bool peek, long partialStart, long partialLength)
    {
      return BodyTextInternal(peek, null, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BodyText(bool peek, ImapPartialRange? @partial)
    {
      return BodyTextInternal(peek, null, @partial);
    }

    public static ImapFetchDataItem BodyText(bool peek, string section, long partialStart, long partialLength)
    {
      return BodyTextInternal(peek, section, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BodyText(bool peek, string section, ImapPartialRange? @partial)
    {
      return BodyTextInternal(peek, section, @partial);
    }

    // TODO: HEADER, HEADER.FIELDS, HEADER.FIELDS.NOT, MIME, and TEXT
    private static ImapFetchDataItem BodyTextInternal(bool peek, string section, ImapPartialRange? @partial)
    {
      if (@partial == null) {
        return new ImapFetchDataItem(string.Concat((peek ? "BODY.PEEK[" : "BODY["), section, "]"));
      }
      else { 
        try {
          return new ImapFetchDataItem(string.Concat((peek ? "BODY.PEEK[" : "BODY["), section, "]", @partial.Value.ToString("f", null)));
        }
        catch (FormatException ex) {
          throw new ArgumentException(ex.Message, "partial");
        }
      }
    }

    // BODYSTRUCTURE
    //     The [MIME-IMB] body structure of the message.  This is computed
    //     by the server by parsing the [MIME-IMB] header fields in the
    //     [RFC-2822] header and [MIME-IMB] headers.
    public static readonly ImapFetchDataItem BodyStructure
      = new ImapFetchDataItem("BODYSTRUCTURE");

    // ENVELOPE
    //     The envelope structure of the message.  This is computed by the
    //     server by parsing the [RFC-2822] header into the component
    //     parts, defaulting various fields as necessary.
    public static readonly ImapFetchDataItem Envelope
      = new ImapFetchDataItem("ENVELOPE");

    // FLAGS
    //     The flags that are set for this message.
    public static readonly ImapFetchDataItem Flags
      = new ImapFetchDataItem("FLAGS");

    // INTERNALDATE
    //     The internal date of the message.
    public static readonly ImapFetchDataItem InternalDate
      = new ImapFetchDataItem("INTERNALDATE");

    // RFC822
    //     Functionally equivalent to BODY[], differing in the syntax of
    //     the resulting untagged FETCH data (RFC822 is returned).
    public static readonly ImapFetchDataItem Rfc822
      = new ImapFetchDataItem("RFC822");

    // RFC822.HEADER
    //     Functionally equivalent to BODY.PEEK[HEADER], differing in the
    //     syntax of the resulting untagged FETCH data (RFC822.HEADER is
    //     returned).
    public static readonly ImapFetchDataItem Rfc822Header
      = new ImapFetchDataItem("RFC822.HEADER");

    // RFC822.SIZE
    //     The [RFC-2822] size of the message.
    public static readonly ImapFetchDataItem Rfc822Size
      = new ImapFetchDataItem("RFC822.SIZE");

    // RFC822.TEXT
    //     Functionally equivalent to BODY[TEXT], differing in the syntax
    //     of the resulting untagged FETCH data (RFC822.TEXT is returned).
    public static readonly ImapFetchDataItem Rfc822Text
      = new ImapFetchDataItem("RFC822.TEXT");

    // UID
    //     The unique identifier for the message.
    public static readonly ImapFetchDataItem Uid
      = new ImapFetchDataItem("UID");

    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     */
    public static readonly ImapFetchDataItem ModSeq
      = new ImapFetchDataItem(ImapCapability.CondStore, "MODSEQ");

    /*
     * RFC 3516 - IMAP4 Binary Content Extension
     * http://tools.ietf.org/html/rfc3516
     * 4.2. FETCH Command Extensions
     */

    /*
     *       BINARY.SIZE<section-binary>
     *          Requests the decoded size of the section (i.e., the size to
     *          expect in response to the corresponding FETCH BINARY request).
     */ 
    public static ImapFetchDataItem BinarySize(string sectionBinary)
    {
      if (sectionBinary == null)
        throw new ArgumentNullException("sectionBinary");
      if (sectionBinary.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("sectionBinary");

      return new ImapFetchDataItem(ImapCapability.Binary, string.Concat("BINARY.SIZE[", sectionBinary, "]"));
    }

    /*
     *       BINARY.PEEK<section-binary>[<partial>]
     *          An alternate form of FETCH BINARY that does not implicitly set
     *          the \Seen flag.
     */
    public static ImapFetchDataItem BinaryPeek(string sectionBinary)
    {
      return BinaryInternal(true, sectionBinary, null);
    }

    public static ImapFetchDataItem BinaryPeek(string sectionBinary, long partialStart, long partialLength)
    {
      return BinaryInternal(true, sectionBinary, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem BinaryPeek(string sectionBinary, ImapPartialRange? @partial)
    {
      return BinaryInternal(true, sectionBinary, @partial);
    }

    /*
     *       BINARY<section-binary>[<partial>]
     *          Requests that the specified section be transmitted after
     *          performing CTE-related decoding.
     */
    public static ImapFetchDataItem Binary(string sectionBinary)
    {
      return BinaryInternal(false, sectionBinary, null);
    }

    public static ImapFetchDataItem Binary(string sectionBinary, long partialStart, long partialLength)
    {
      return BinaryInternal(false, sectionBinary, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem Binary(string sectionBinary, ImapPartialRange? @partial)
    {
      return BinaryInternal(false, sectionBinary, @partial);
    }

    public static ImapFetchDataItem Binary(bool peek, string sectionBinary)
    {
      return BinaryInternal(peek, sectionBinary, null);
    }

    public static ImapFetchDataItem Binary(bool peek, string sectionBinary, long partialStart, long partialLength)
    {
      return BinaryInternal(peek, sectionBinary, new ImapPartialRange(partialStart, partialLength));
    }

    public static ImapFetchDataItem Binary(bool peek, string sectionBinary, ImapPartialRange? @partial)
    {
      return BinaryInternal(peek, sectionBinary, @partial);
    }

    private static ImapFetchDataItem BinaryInternal(bool peek, string sectionBinary, ImapPartialRange? @partial)
    {
      if (sectionBinary == null)
        throw new ArgumentNullException("sectionBinary");
      if (sectionBinary.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("sectionBinary");

      if (@partial == null) {
        return new ImapFetchDataItem(ImapCapability.Binary, string.Concat((peek ? "BINARY.PEEK[" : "BINARY["), sectionBinary, "]"));
      }
      else {
        try {
          return new ImapFetchDataItem(ImapCapability.Binary,
                                       string.Concat((peek ? "BINARY.PEEK[" : "BINARY["), sectionBinary, "]", @partial.Value.ToString("f", null)));
        }
        catch (FormatException ex) {
          throw new ArgumentException(ex.Message, "partial");
        }
      }
    }

    private ImapFetchDataItem(ImapString macro, bool isMacro, params ImapString[] equivalentDataItemNames)
      : base(macro)
    {
      this.equivalent = equivalentDataItemNames;
    }

    private ImapFetchDataItem(params ImapString[] dataItemNames)
      : this(null, dataItemNames)
    {
    }

    private ImapFetchDataItem(ImapCapability requiredCapability, params ImapString[] dataItemNames)
      : base(dataItemNames)
    {
      this.equivalent = null;
      this.requiredCapability = requiredCapability;
    }

    public ImapFetchDataItem CombineWith(ImapFetchDataItem dataItem)
    {
      return this + dataItem;
    }

    public static ImapFetchDataItem Combine(ImapFetchDataItem x, ImapFetchDataItem y)
    {
      return x + y;
    }

    public static ImapFetchDataItem operator+ (ImapFetchDataItem x, ImapFetchDataItem y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      var xx = x.equivalent ?? x.Items;
      var yy = y.equivalent ?? y.Items;

      return new ImapFetchDataItem(x.requiredCapability ?? y.requiredCapability, GetCombinedItems(xx, yy));
    }

    public static ImapFetchDataItem FromUri(Uri uri)
    {
      return BodyPeek(ImapStyleUriParser.GetSection(uri), ImapStyleUriParser.GetPartial(uri));
    }

    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (requiredCapability != null)
          yield return requiredCapability;
      }
    }

    protected override ImapStringList GetCombined()
    {
      if (equivalent == null)
        // message data item names
        return ToParenthesizedString();
      else
        // macro
        return ToStringList();
    }

    private /*readonly*/ ImapString[] equivalent;
    private /*readonly*/ ImapCapability requiredCapability;
  }
}
