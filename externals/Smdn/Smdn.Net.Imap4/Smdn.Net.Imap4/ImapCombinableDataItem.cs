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
  // * ImapCombinableDataItem
  //     => handles 'data item names' etc.
  //     ImapFetchDataItem
  //       => handles 'message data item names or macro'
  //     ImapSearchCriteria
  //       => handles 'searching criteria'
  //     ImapSortCriteria
  //       => handles 'sort criteria'
  //     ImapStatusDataItem
  //       => handles 'status data item names'
  //     ImapStoreDataItem
  //       => handles 'message data item name' and 'value for message data item'

  public abstract class ImapCombinableDataItem : ImapString, IEquatable<ImapCombinableDataItem> {
    protected ImapString[] Items {
      get { return items; }
    }

    protected ImapCombinableDataItem(params ImapString[] items)
    {
      this.items = items;
    }

    protected static ImapString[] GetCombinedItems(ImapCombinableDataItem x, ImapCombinableDataItem y)
    {
      return GetCombinedItems(x.items, y.items);
    }

    protected static ImapString[] GetCombinedItems(ImapString[] x, ImapString[] y)
    {
      if (x == null)
        throw new ArgumentNullException("x");
      if (y == null)
        throw new ArgumentNullException("y");

      return x.Concat(y);
    }

    public virtual void Traverse(Action<ImapString> action) {
      foreach (var item in items) {
        var combinable = item as ImapCombinableDataItem;

        if (combinable == null) {
          var list = item as ImapStringList;

          if (list == null)
            action(item);
          else
            list.Traverse(action);
        }
        else {
          combinable.Traverse(action);
        }
      }
    }

    public virtual bool ContainsOneOf(ImapCombinableDataItem combinable)
    {
      if (combinable == null)
        throw new ArgumentNullException("combinable");

      foreach (var item in items) {
        foreach (var i in combinable.items) {
          if (i == item)
            return true;
        }
      }

      return false;
    }

#region "equatable"
    public override bool Equals(object obj)
    {
      if (obj is ImapCombinableDataItem)
        return Equals(obj as ImapCombinableDataItem);
      else
        return base.Equals(obj);
    }

    public virtual bool Equals(ImapCombinableDataItem other)
    {
      if (null == (object)other)
        return false;
      else
        return Object.ReferenceEquals(this, other) ||
          (GetType() == other.GetType() && Equals(other.ToString()));
    }

    public override bool Equals(ImapString other)
    {
      if (null == (object)other)
        return false;
      else
        return Equals(other.ToString());
    }

    public override bool Equals(string other)
    {
      return string.Equals(ToString(), other);
    }

    public override int GetHashCode()
    {
      return ToString().GetHashCode();
    }
#endregion

#region "conversion"
    protected abstract ImapStringList GetCombined();

    internal ImapStringList InternalGetCombined()
    {
      return GetCombined();
    }

    protected ImapParenthesizedString ToParenthesizedString()
    {
      return new ImapParenthesizedString(items);
    }

    protected ImapStringList ToStringList()
    {
      return new ImapStringList(items);
    }

    public override string ToString()
    {
      return GetCombined().ToString();
    }
#endregion

    private /*readonly*/ ImapString[] items;
  }
}