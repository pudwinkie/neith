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
#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

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
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //   * ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  // 6.4.6. STORE Command
  //   message data item name
  //   value for message data item
  public sealed class ImapStoreDataItem : ImapCombinableDataItem {

    // FLAGS <flag list>
    //     Replace the flags for the message (other than \Recent) with the
    //     argument.  The new value of the flags is returned as if a FETCH
    //     of those flags was done.
    public static ImapStoreDataItem ReplaceFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return ReplaceFlags(new string[] {}, flags.Prepend(flag));
    }

    public static ImapStoreDataItem ReplaceFlags(string keyword, params string[] keywords)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");
      if (keyword.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("keyword");

      return ReplaceFlags(keywords.Prepend(keyword), new ImapMessageFlag[] {});
    }

    public static ImapStoreDataItem ReplaceFlags(string[] keywords, params ImapMessageFlag[] flags)
    {
      return ReplaceFlags(new ImapMessageFlagSet(keywords, flags));
    }

    public static ImapStoreDataItem ReplaceFlags(IImapMessageFlagSet flags)
    {
      return FlagsCommon("FLAGS", flags);
    }

    // FLAGS.SILENT <flag list>
    //     Equivalent to FLAGS, but without returning a new value.
    public static ImapStoreDataItem ReplaceFlagsSilent(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return ReplaceFlagsSilent(new string[] {}, flags.Prepend(flag));
    }

    public static ImapStoreDataItem ReplaceFlagsSilent(string keyword, params string[] keywords)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");
      if (keyword.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("keyword");

      return ReplaceFlagsSilent(keywords.Prepend(keyword), new ImapMessageFlag[] {});
    }

    public static ImapStoreDataItem ReplaceFlagsSilent(string[] keywords, params ImapMessageFlag[] flags)
    {
      return ReplaceFlagsSilent(new ImapMessageFlagSet(keywords, flags));
    }

    public static ImapStoreDataItem ReplaceFlagsSilent(IImapMessageFlagSet flags)
    {
      return FlagsCommon("FLAGS.SILENT", flags);
    }

    // +FLAGS <flag list>
    //     Add the argument to the flags for the message.  The new value
    //     of the flags is returned as if a FETCH of those flags was done.
    public static ImapStoreDataItem AddFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return AddFlags(new string[] {}, flags.Prepend(flag));
    }

    public static ImapStoreDataItem AddFlags(string keyword, params string[] keywords)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");
      if (keyword.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("keyword");

      return AddFlags(keywords.Prepend(keyword), new ImapMessageFlag[] {});
    }

    public static ImapStoreDataItem AddFlags(string[] keywords, params ImapMessageFlag[] flags)
    {
      return AddFlags(new ImapMessageFlagSet(keywords, flags));
    }

    public static ImapStoreDataItem AddFlags(IImapMessageFlagSet flags)
    {
      return FlagsCommon("+FLAGS", flags);
    }

    // +FLAGS.SILENT <flag list>
    //     Equivalent to +FLAGS, but without returning a new value.
    public static ImapStoreDataItem AddFlagsSilent(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return AddFlagsSilent(new string[] {}, flags.Prepend(flag));
    }

    public static ImapStoreDataItem AddFlagsSilent(string keyword, params string[] keywords)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");
      if (keyword.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("keyword");

      return AddFlagsSilent(keywords.Prepend(keyword), new ImapMessageFlag[] {});
    }

    public static ImapStoreDataItem AddFlagsSilent(string[] keywords, params ImapMessageFlag[] flags)
    {
      return AddFlagsSilent(new ImapMessageFlagSet(keywords, flags));
    }

    public static ImapStoreDataItem AddFlagsSilent(IImapMessageFlagSet flags)
    {
      return FlagsCommon("+FLAGS.SILENT", flags);
    }

    // -FLAGS <flag list>
    //     Remove the argument from the flags for the message.  The new
    //     value of the flags is returned as if a FETCH of those flags was
    //     done.
    public static ImapStoreDataItem RemoveFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return RemoveFlags(new string[] {}, flags.Prepend(flag));
    }

    public static ImapStoreDataItem RemoveFlags(string keyword, params string[] keywords)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");
      if (keyword.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("keyword");

      return RemoveFlags(keywords.Prepend(keyword), new ImapMessageFlag[] {});
    }

    public static ImapStoreDataItem RemoveFlags(string[] keywords, params ImapMessageFlag[] flags)
    {
      return RemoveFlags(new ImapMessageFlagSet(keywords, flags));
    }

    public static ImapStoreDataItem RemoveFlags(IImapMessageFlagSet flags)
    {
      return FlagsCommon("-FLAGS", flags);
    }

    // -FLAGS.SILENT <flag list>
    //     Equivalent to -FLAGS, but without returning a new value.
    public static ImapStoreDataItem RemoveFlagsSilent(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      if (flag == null)
        throw new ArgumentNullException("flag");

      return RemoveFlagsSilent(new string[] {}, flags.Prepend(flag));
    }

    public static ImapStoreDataItem RemoveFlagsSilent(string keyword, params string[] keywords)
    {
      if (keyword == null)
        throw new ArgumentNullException("keyword");
      if (keyword.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("keyword");

      return RemoveFlagsSilent(keywords.Prepend(keyword), new ImapMessageFlag[] {});
    }

    public static ImapStoreDataItem RemoveFlagsSilent(string[] keywords, params ImapMessageFlag[] flags)
    {
      return RemoveFlagsSilent(new ImapMessageFlagSet(keywords, flags));
    }

    public static ImapStoreDataItem RemoveFlagsSilent(IImapMessageFlagSet flags)
    {
      return FlagsCommon("-FLAGS.SILENT", flags);
    }

    // common construction method
    private static ImapStoreDataItem FlagsCommon(ImapString itemName, IImapMessageFlagSet flags)
    {
      if (flags == null)
        throw new ArgumentNullException("flags");
      else if (flags.Count == 0)
        throw new ArgumentException("must contain at least one keyword or flag", "flags");

      return new ImapStoreDataItem(itemName, flags.GetNonApplicableFlagsRemoved().ToArray());
    }

    public ImapString ItemName {
      get { return itemName; }
    }

    private ImapStoreDataItem(ImapString itemName, ImapString[] items)
      : base(items)
    {
      this.itemName = itemName;
    }

    protected override ImapStringList GetCombined()
    {
      return ToParenthesizedString();
    }

    public override string ToString()
    {
      return string.Concat(itemName, " ", GetCombined());
    }

    private ImapString itemName;
  }
}