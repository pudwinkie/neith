// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Smdn.Windows.Forms {
  public class DropDownList<TItem> : ComboBox {
    public DropDownList()
      : base()
    {
      base.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    protected override void OnDropDownStyleChanged(EventArgs e)
    {
      base.DropDownStyle = ComboBoxStyle.DropDownList;

      base.OnDropDownStyleChanged(e);
    }

    public new TItem SelectedItem {
      get
      {
        if (base.SelectedItem == null)
          return default(TItem);
        else
          return (TItem)base.SelectedItem;
      }
      set { base.SelectedItem = value; }
    }

    public new IList<TItem> Items {
      get
      {
        if (items == null || items.BaseList != base.Items)
          items = new ItemCollection(base.Items);

        return items;
      }
    }

    private ItemCollection items = null;

    private class ItemCollection : IList<TItem> {
      public IList BaseList {
        get { return baseList; }
      }

      public ItemCollection(IList baseList)
      {
        this.baseList = baseList;
      }

      public int IndexOf(TItem item)
      {
        return baseList.IndexOf(item);
      }

      public void Insert(int index, TItem item)
      {
        baseList.Insert(index, item);
      }

      public void RemoveAt(int index)
      {
        baseList.RemoveAt(index);
      }

      public TItem this[int index] {
        get { return (TItem)baseList[index]; }
        set { baseList[index] = value; }
      }

      public void Add(TItem item)
      {
        baseList.Add(item);
      }

      public void Clear()
      {
        baseList.Clear();
      }

      public bool Contains(TItem item)
      {
        return baseList.Contains(item);
      }

      public void CopyTo(TItem[] array, int arrayIndex)
      {
        baseList.CopyTo(array, arrayIndex);
      }

      public int Count {
        get { return baseList.Count; }
      }

      public bool IsReadOnly {
        get { return baseList.IsReadOnly; }
      }

      public bool Remove(TItem item)
      {
        if (Contains(item)) {
          baseList.Remove(item);
          return true;
        }

        return false;
      }

      public IEnumerator<TItem> GetEnumerator()
      {
        foreach (TItem item in baseList) {
          yield return item;
        }
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return baseList.GetEnumerator();
      }

      private IList baseList;
    }
  }
}
