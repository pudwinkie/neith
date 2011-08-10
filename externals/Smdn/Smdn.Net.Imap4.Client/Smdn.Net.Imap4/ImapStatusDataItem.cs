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
  //     ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //     ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //   * ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  // 6.3.10. STATUS Command
  //   status data item names
  public sealed class ImapStatusDataItem : ImapCombinableDataItem, IImapExtension {
    /// <summary>This field is equivalent to "MESSAGES RECENT UIDXNEXT UIDVALIDITY UNSEEN", contains no status data items defined in extensions.</summary>
    public static readonly ImapStatusDataItem StandardAll
      = new ImapStatusDataItem(null, "MESSAGES", "RECENT", "UIDNEXT", "UIDVALIDITY", "UNSEEN");

    // MESSAGES
    public static readonly ImapStatusDataItem Messages
      = new ImapStatusDataItem(null, "MESSAGES");

    // RECENT
    public static readonly ImapStatusDataItem Recent
      = new ImapStatusDataItem(null, "RECENT");

    // UIDNEXT
    public static readonly ImapStatusDataItem UidNext
      = new ImapStatusDataItem(null, "UIDNEXT");

    // UIDVALIDITY
    public static readonly ImapStatusDataItem UidValidity
      = new ImapStatusDataItem(null, "UIDVALIDITY");

    // UNSEEN
    public static readonly ImapStatusDataItem Unseen
      = new ImapStatusDataItem(null, "UNSEEN");

    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     */
    public static readonly ImapStatusDataItem HighestModSeq
      = new ImapStatusDataItem(ImapCapability.CondStore, "HIGHESTMODSEQ");

    public ImapStatusDataItem CombineWith(ImapStatusDataItem dataItem)
    {
      return this + dataItem;
    }

    public static ImapStatusDataItem Combine(ImapStatusDataItem x, ImapStatusDataItem y)
    {
      return x + y;
    }

    public static ImapStatusDataItem operator+ (ImapStatusDataItem x, ImapStatusDataItem y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      return new ImapStatusDataItem(x.requiredCapability ?? y.requiredCapability, GetCombinedItems(x, y));
    }

    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (requiredCapability != null)
          yield return requiredCapability;
      }
    }

    private ImapStatusDataItem(ImapCapability requiredCapability, params ImapString[] items)
      : base(items)
    {
      this.requiredCapability = requiredCapability;
    }

    protected override ImapStringList GetCombined()
    {
      return ToParenthesizedString();
    }

    private /*readonly*/ ImapCapability requiredCapability;
  }
}