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
using System.Text;

namespace Smdn.Net.Imap4 {
  // combinable data item types:
  //   ImapCombinableDataItem
  //     => handles 'data item names' etc.
  //     ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //     ImapSearchCriteria
  //       => handles 'searching criteria'
  //   * ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  // http://tools.ietf.org/html/rfc5256
  // BASE.6.4.SORT. SORT Command
  public sealed class ImapSortCriteria : ImapCombinableDataItem, IImapMultipleExtension {
    ImapCapability[] IImapMultipleExtension.RequiredCapabilities {
      get { return requiredCapabilities.ToArray(); }
    }

    internal List<ImapCapability> RequiredCapabilities {
      get { return this.requiredCapabilities; }
    }

    // ARRIVAL
    //    Internal date and time of the message.  This differs from the
    //    ON criteria in SEARCH, which uses just the internal date.
    public static readonly ImapSortCriteria Arrival        = Create("ARRIVAL");
    public static readonly ImapSortCriteria ArrivalReverse = CreateReversed("ARRIVAL");

    // CC
    //    [IMAP] addr-mailbox of the first "cc" address.
    public static readonly ImapSortCriteria Cc        = Create("CC");
    public static readonly ImapSortCriteria CcReverse = CreateReversed("CC");

    // DATE
    //    Sent date and time, as described in section 2.2.
    public static readonly ImapSortCriteria Date        = Create("DATE");
    public static readonly ImapSortCriteria DateReverse = CreateReversed("DATE");

    // FROM
    //    [IMAP] addr-mailbox of the first "From" address.
    public static readonly ImapSortCriteria From        = Create("FROM");
    public static readonly ImapSortCriteria FromReverse = CreateReversed("FROM");

    // SIZE
    //    Size of the message in octets.
    public static readonly ImapSortCriteria Size        = Create("SIZE");
    public static readonly ImapSortCriteria SizeReverse = CreateReversed("SIZE");

    // SUBJECT
    //    Base subject text.
    public static readonly ImapSortCriteria Subject        = Create("SUBJECT");
    public static readonly ImapSortCriteria SubjectReverse = CreateReversed("SUBJECT");

    // TO
    //    [IMAP] addr-mailbox of the first "To" address.
    public static readonly ImapSortCriteria To        = Create("TO");
    public static readonly ImapSortCriteria ToReverse = CreateReversed("TO");

    /*
     * RFC 4551 - Display-based Address Sorting for the IMAP4 SORT Extension
     * http://tools.ietf.org/html/draft-ietf-morg-sortdisplay-03
     */
    public static readonly ImapSortCriteria DisplayFrom = new ImapSortCriteria(new[] {ImapCapability.Sort, ImapCapability.SortDisplay}, "DISPLAYFROM");
    public static readonly ImapSortCriteria DisplayTo   = new ImapSortCriteria(new[] {ImapCapability.Sort, ImapCapability.SortDisplay}, "DISPLAYTO");

    public ImapSortCriteria CombineWith(ImapSortCriteria criteria)
    {
      return this + criteria;
    }

    public static ImapSortCriteria Combine(ImapSortCriteria x, ImapSortCriteria y)
    {
      return x + y;
    }

    public static ImapSortCriteria operator+ (ImapSortCriteria x, ImapSortCriteria y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      var requiredCapabilities = new List<ImapCapability>(x.requiredCapabilities);

      foreach (var cap in y.requiredCapabilities) {
        if (!requiredCapabilities.Contains(cap))
          requiredCapabilities.Add(cap);
      }

      return new ImapSortCriteria(requiredCapabilities, GetCombinedItems(x.Items, y.Items));
    }

    private static ImapSortCriteria Create(ImapString criteria)
    {
      return new ImapSortCriteria(new[] {ImapCapability.Sort}, criteria);
    }

    private static ImapSortCriteria CreateReversed(ImapString criteria)
    {
      // REVERSE
      //    Followed by another sort criterion, has the effect of that
      //    criterion but in reverse (descending) order.
      //       Note: REVERSE only reverses a single criterion, and does not
      //       affect the implicit "sequence number" sort criterion if all
      //       other criteria are identical.  Consequently, a sort of
      //       REVERSE SUBJECT is not the same as a reverse ordering of a
      //       SUBJECT sort.  This can be avoided by use of additional
      //       criteria, e.g., SUBJECT DATE vs. REVERSE SUBJECT REVERSE
      //       DATE.  In general, however, it's better (and faster, if the
      //       client has a "reverse current ordering" command) to reverse
      //       the results in the client instead of issuing a new SORT.
      return new ImapSortCriteria(new[] {ImapCapability.Sort}, "REVERSE", criteria);
    }

    private ImapSortCriteria(IEnumerable<ImapCapability> requiredCapabilities, params ImapString[] items)
      : base(items)
    {
      this.requiredCapabilities = new List<ImapCapability>(requiredCapabilities);
    }

    protected override ImapStringList GetCombined()
    {
      return ToParenthesizedString();
    }

    private List<ImapCapability> requiredCapabilities;
  }
}