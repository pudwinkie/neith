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

namespace Smdn.Net.Imap4 {
  // combinable data item types:
  //   ImapCombinableDataItem
  //     => handles 'data item names' etc.
  //     ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //   * ImapGetMetadataOptions
  //       => handles 'GETMETADATA command options'
  //     ImapListReturnOptions
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

  public sealed class ImapGetMetadataOptions : ImapCombinableDataItem {
    /*
     * RFC 5464 - The IMAP METADATA Extension
     * http://tools.ietf.org/html/rfc5464
     */

    /*
     * 4.2.1. MAXSIZE GETMETADATA Command Option
     *    When the MAXSIZE option is specified with the GETMETADATA command, it
     *    restricts which entry values are returned by the server.  Only entry
     *    values that are less than or equal in octet size to the specified
     *    MAXSIZE limit are returned.  If there are any entries with values
     *    larger than the MAXSIZE limit, the server MUST include the METADATA
     *    LONGENTRIES response code in the tagged OK response for the
     *    GETMETADATA command.  The METADATA LONGENTRIES response code returns
     *    the size of the biggest entry value requested by the client that
     *    exceeded the MAXSIZE limit.
     */
    public static ImapGetMetadataOptions MaxSize(long maxSize)
    {
      if (maxSize < 0L)
        throw new ArgumentOutOfRangeException("maxSize", maxSize, "must be zero or positive number");

      return new ImapGetMetadataOptions("MAXSIZE", maxSize.ToString());
    }

    /*
     * 4.2.2. DEPTH GETMETADATA Command Option
     *    When the DEPTH option is specified with the GETMETADATA command, it
     *    extends the list of entry values returned by the server.  For each
     *    entry name specified in the GETMETADATA command, the server returns
     *    the value of the specified entry name (if it exists), plus all
     *    entries below the entry name up to the specified DEPTH.  Three values
     *    are allowed for DEPTH:
     * 
     *    "0" - no entries below the specified entry are returned
     *    "1" - only entries immediately below the specified entry are returned
     *    "infinity" -  all entries below the specified entry are returned
     * 
     *    Thus, "depth 1" for an entry "/a" will match "/a" as well as its
     *    children entries (e.g., "/a/b"), but will not match grandchildren
     *    entries (e.g., "/a/b/c").
     */
    public static readonly ImapGetMetadataOptions Depth0 = new ImapGetMetadataOptions("DEPTH", "0");
    public static readonly ImapGetMetadataOptions Depth1 = new ImapGetMetadataOptions("DEPTH", "1");
    public static readonly ImapGetMetadataOptions DepthInfinity = new ImapGetMetadataOptions("DEPTH", "infinity");

    public ImapGetMetadataOptions CombineWith(ImapGetMetadataOptions other)
    {
      return this + other;
    }

    public static ImapGetMetadataOptions Combine(ImapGetMetadataOptions x, ImapGetMetadataOptions y)
    {
      return x + y;
    }

    public static ImapGetMetadataOptions operator+ (ImapGetMetadataOptions x, ImapGetMetadataOptions y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      return new ImapGetMetadataOptions(GetCombinedItems(x.Items, y.Items));
    }

    private ImapGetMetadataOptions(params ImapString[] items)
      : base(items)
    {
    }

    protected override ImapStringList GetCombined()
    {
      return ToParenthesizedString();
    }
  }
}
