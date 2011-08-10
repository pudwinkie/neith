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
  //     ImapListReturnOptions
  //       => handles 'LIST return options'
  //   * ImapListSelectionOptions
  //       => handles 'LIST selection options'
  //     ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  public sealed class ImapListSelectionOptions : ImapCombinableDataItem, IImapExtension {
    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get { return this.requiredCapabilities; }
    }

    public static readonly ImapListSelectionOptions Empty = new ImapListSelectionOptions(new ImapString[] {});

    /*
     * http://tools.ietf.org/html/rfc5258
     * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
     * 3.1. Initial List of Selection Options
     */

    /*
     *    SUBSCRIBED -  causes the LIST command to list subscribed names,
     *       rather than the existing mailboxes.  This will often be a subset
     *       of the actual mailboxes.  It's also possible for this list to
     *       contain the names of mailboxes that don't exist.  In any case, the
     *       list MUST include exactly those mailbox names that match the
     *       canonical list pattern and are subscribed to.
     */
    public static readonly ImapListSelectionOptions Subscribed = new ImapListSelectionOptions("SUBSCRIBED");

    /*
     *    REMOTE -  causes the LIST command to show remote mailboxes as well as
     *       local ones, as described in [MBRef].  This option is intended to
     *       replace the RLIST command and, in conjunction with the SUBSCRIBED
     *       selection option, the RLSUB command.
     */
    public static readonly ImapListSelectionOptions Remote = new ImapListSelectionOptions("REMOTE");

    /*
     *    RECURSIVEMATCH -  this option forces the server to return information
     *       about parent mailboxes that don't match other selection options,
     *       but have some submailboxes that do.  Information about children is
     *       returned in the CHILDINFO extended data item, as described in
     *       Section 3.5.
     */
    public static readonly ImapListSelectionOptions RecursiveMatch = new ImapListSelectionOptions("RECURSIVEMATCH");

    /*
     * draft-ietf-morg-list-specialuse-06 - IMAP LIST extension for special-use mailboxes
     * http://tools.ietf.org/html/draft-ietf-morg-list-specialuse-06
     * 2. New mailbox attributes identifying special-use mailboxes
     */
    public static readonly ImapListSelectionOptions SpecialUse =
      new ImapListSelectionOptions(new[] {ImapCapability.ListExtended, ImapCapability.SpecialUse},
                                   "SPECIAL-USE");

    public ImapListSelectionOptions CombineWith(ImapListSelectionOptions other)
    {
      return this + other;
    }

    public static ImapListSelectionOptions Combine(ImapListSelectionOptions x, ImapListSelectionOptions y)
    {
      return x + y;
    }

    public static ImapListSelectionOptions operator+ (ImapListSelectionOptions x, ImapListSelectionOptions y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      var requiredCapabilities = new ImapCapabilitySet(x.requiredCapabilities);

      requiredCapabilities.UnionWith(y.requiredCapabilities);

      return new ImapListSelectionOptions(requiredCapabilities, GetCombinedItems(x.Items, y.Items));
    }

    private ImapListSelectionOptions(params ImapString[] items)
      : this(new ImapCapabilitySet(new[] {ImapCapability.ListExtended}), items)
    {
    }
  
    private ImapListSelectionOptions(ImapCapability[] requiredCapabilities, params ImapString[] items)
      : this(new ImapCapabilitySet(requiredCapabilities), items)
    {
    }

    private ImapListSelectionOptions(ImapCapabilitySet requiredCapabilities, params ImapString[] items)
      : base(items)
    {
      this.requiredCapabilities = requiredCapabilities;
    }

    protected override ImapStringList GetCombined()
    {
      return ToParenthesizedString();
    }

    private readonly ImapCapabilitySet requiredCapabilities;
  }
}