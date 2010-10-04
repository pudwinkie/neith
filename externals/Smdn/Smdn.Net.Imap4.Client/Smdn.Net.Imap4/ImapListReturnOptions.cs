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

namespace Smdn.Net.Imap4 {
  // combinable data item types:
  //   ImapCombinableDataItem
  //     => handles 'data item names' etc.
  //     ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //   * ImapListReturnOptions
  //       => handles 'LIST return options'
  //     ImapListSelectionOptions
  //       => handles 'LIST selection options'
  //     ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  public sealed class ImapListReturnOptions : ImapCombinableDataItem, IImapMultipleExtension {
    ImapCapability[] IImapMultipleExtension.RequiredCapabilities {
      get { return requiredCapabilities.ToArray(); }
    }

    internal List<ImapCapability> RequiredCapabilities {
      get { return this.requiredCapabilities; }
    }

    public static readonly ImapListReturnOptions Empty = new ImapListReturnOptions(new ImapString[] {});

    /*
     * http://tools.ietf.org/html/rfc5258
     * RFC 5258 - Internet Message Access Protocol version 4 - LIST Command Extensions
     * 3.2. Initial List of Return Options
     */

    /*
     *    SUBSCRIBED -  causes the LIST command to return subscription state
     *       for all matching mailbox names.  The "\Subscribed" attribute MUST
     *       be supported and MUST be accurately computed when the SUBSCRIBED
     *       return option is specified.  Further, all mailbox flags MUST be
     *       accurately computed (this differs from the behavior of the LSUB
     *       command).
     */
    public static readonly ImapListReturnOptions Subscribed = new ImapListReturnOptions("SUBSCRIBED");

    /*
     *    CHILDREN -  requests mailbox child information as originally proposed
     *       in [CMbox].  See Section 4, below, for details.  This option MUST
     *       be supported by all servers.
     */
    public static readonly ImapListReturnOptions Children = new ImapListReturnOptions("CHILDREN");

    /*
     * IMAP4 Extension for Returning STATUS Information in Extended LIST
     * http://tools.ietf.org/html/rfc5819
     */

    // 2. STATUS return option to LIST command
    public static ImapListReturnOptions StatusDataItems(ImapStatusDataItem statusDataItem)
    {
      return new ImapListReturnOptions(new[] {ImapCapability.ListStatus},
                                       "STATUS",
                                       statusDataItem);
    }

    /*
     * http://tools.ietf.org/html/draft-ietf-morg-list-specialuse-01
     * draft-ietf-morg-list-specialuse-01 - IMAP LIST extension for special-use mailboxes
     * 2. New mailbox flags identifying special-use mailboxes
     */
    public static readonly ImapListReturnOptions SpecialUse = new ImapListReturnOptions("SPECIAL-USE");

    public ImapListReturnOptions CombineWith(ImapListReturnOptions other)
    {
      return this + other;
    }

    public static ImapListReturnOptions Combine(ImapListReturnOptions x, ImapListReturnOptions y)
    {
      return x + y;
    }

    public static ImapListReturnOptions operator+ (ImapListReturnOptions x, ImapListReturnOptions y)
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

      return new ImapListReturnOptions(requiredCapabilities, GetCombinedItems(x.Items, y.Items));
    }

    private ImapListReturnOptions(params ImapString[] items)
      : this(new ImapCapability[] {ImapCapability.ListExtended}, items)
    {
    }

    private ImapListReturnOptions(IEnumerable<ImapCapability> requiredCapabilities, params ImapString[] items)
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