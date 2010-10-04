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
  //   * ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  public class ImapSearchResultOptions : ImapCombinableDataItem, IImapMultipleExtension {
    ImapCapability[] IImapMultipleExtension.RequiredCapabilities {
      get { return requiredCapabilities.ToArray(); }
    }

    internal List<ImapCapability> RequiredCapabilities {
      get { return this.requiredCapabilities; }
    }

    /*
     * http://tools.ietf.org/html/rfc4731
     * RFC 4731 - IMAP4 Extension to SEARCH Command for Controlling What Kind of Information Is Returned
     * 3.1. New SEARCH/UID SEARCH Result Options
     */

    //       MIN
    //          Return the lowest message number/UID that satisfies the SEARCH
    //          criteria.
    // 
    //          If the SEARCH results in no matches, the server MUST NOT
    //          include the MIN result option in the ESEARCH response; however,
    //          it still MUST send the ESEARCH response.
    public static readonly ImapSearchResultOptions Min
      = new ImapSearchResultOptions(new[] {ImapCapability.ESearch, ImapCapability.ESort}, "MIN");

    //       MAX
    //          Return the highest message number/UID that satisfies the SEARCH
    //          criteria.
    // 
    //          If the SEARCH results in no matches, the server MUST NOT
    //          include the MAX result option in the ESEARCH response; however,
    //          it still MUST send the ESEARCH response.
    public static readonly ImapSearchResultOptions Max
      = new ImapSearchResultOptions(new[] {ImapCapability.ESearch, ImapCapability.ESort}, "MAX");

    //       ALL
    //          Return all message numbers/UIDs that satisfy the SEARCH
    //          criteria.  Unlike regular (unextended) SEARCH, the messages are
    //          always returned using the sequence-set syntax.  A sequence-set
    //          representation may be more compact and can be used as is in a
    //          subsequent command that accepts sequence-set.  Note, the client
    //          MUST NOT assume that messages/UIDs will be listed in any
    //          particular order.
    // 
    //          If the SEARCH results in no matches, the server MUST NOT
    //          include the ALL result option in the ESEARCH response; however,
    //          it still MUST send the ESEARCH response.
    public static readonly ImapSearchResultOptions All
      = new ImapSearchResultOptions(new[] {ImapCapability.ESearch, ImapCapability.ESort}, "ALL");

    //       COUNT
    //          Return number of the messages that satisfy the SEARCH criteria.
    //          This result option MUST always be included in the ESEARCH
    //          response.
    public static readonly ImapSearchResultOptions Count
      = new ImapSearchResultOptions(new[] {ImapCapability.ESearch, ImapCapability.ESort}, "COUNT");

    /*
     * RFC 5182 - IMAP Extension for Referencing the Last SEARCH Result
     * http://tools.ietf.org/html/rfc5182
     */

    //    The SEARCH result reference extension defines a new SEARCH result
    //    option [IMAPABNF] "SAVE" that tells the server to remember the result
    //    of the SEARCH or UID SEARCH command (as well as any command based on
    //    SEARCH, e.g., SORT and THREAD [SORT]) and store it in an internal
    //    variable that we will reference as the "search result variable".
    public static readonly ImapSearchResultOptions Save
      = new ImapSearchResultOptions(new[] {ImapCapability.ESearch, ImapCapability.ESort, ImapCapability.Searchres}, "SAVE");

    /*
     * draft-ietf-morg-inthread-00 - The IMAP SEARCH=INTHREAD and THREAD=REFS Extensions
     * http://tools.ietf.org/html/draft-ietf-morg-inthread-00
     */

    // 3.5. The THREAD=* Search Return Option(s)
    //     The THREAD=* search return options enables the client to select
    //     which threading algorithm the server uses when processing INTHREAD,
    //     THREADROOT and THREADLEAF as part of a SEARCH command. If THREAD=*
    //     isn't specified, then the default for the SEARCH command is
    //     THREAD=REFS.
    public static ImapSearchResultOptions ThreadingAlgorithm(ImapThreadingAlgorithm algorithm)
    {
      return new ImapSearchResultOptions(new[] {ImapCapability.ESearch, ImapCapability.ESort, ImapCapability.SearchInThread, ImapCapability.ThreadRefs},
                                          string.Concat("THREAD=", algorithm));
    }

    public ImapSearchResultOptions CombineWith(ImapSearchResultOptions other)
    {
      return this + other;
    }

    public static ImapSearchResultOptions Combine(ImapSearchResultOptions x, ImapSearchResultOptions y)
    {
      return x + y;
    }

    public static ImapSearchResultOptions operator+ (ImapSearchResultOptions x, ImapSearchResultOptions y)
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

      return new ImapSearchResultOptions(requiredCapabilities, GetCombinedItems(x.Items, y.Items));
    }

    private ImapSearchResultOptions(IEnumerable<ImapCapability> requiredCapabilities, params ImapString[] items)
      : base(items)
    {
      this.requiredCapabilities.AddRange(requiredCapabilities);
    }

    protected override ImapStringList GetCombined()
    {
      return ToParenthesizedString();
    }

    private /*readonly*/ List<ImapCapability> requiredCapabilities = new List<ImapCapability>();
  }
}