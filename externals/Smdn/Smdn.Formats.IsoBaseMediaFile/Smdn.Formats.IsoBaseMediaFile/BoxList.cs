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
using System.Collections.ObjectModel;

namespace Smdn.Formats.IsoBaseMediaFile {
  public class BoxList : BoxList<Box> {
    public BoxList(IBoxContainer container)
      : base(container)
    {
    }

    public BoxList(IBoxContainer container, IEnumerable<Box> boxes)
      : base(container, boxes)
    {
    }
  }

  public class BoxList<TBox> : Collection<TBox>, IEnumerable<Box> where TBox : Box {
    public BoxList(IBoxContainer container)
    {
      if (container == null)
        throw new ArgumentNullException("container");

      this.container = container;
    }

    public BoxList(IBoxContainer container, IEnumerable<TBox> boxes)
      : this(container)
    {
      AddRange(boxes);
    }

    IEnumerator<Box> IEnumerable<Box>.GetEnumerator()
    {
      foreach (var box in this) {
        yield return box;
      }
    }

    public void AddRange(IEnumerable<TBox> boxes)
    {
      foreach (var box in boxes) {
        Add(box);
      }
    }

    public TBox[] ToArray()
    {
      var array = new TBox[base.Count];

      CopyTo(array, 0);

      return array;
    }

    protected override void ClearItems()
    {
      foreach (var box in this) {
        box.ContainedIn = null;
      }

      base.ClearItems();
    }

    protected override void InsertItem(int index, TBox item)
    {
      item.ContainedIn = container;

      base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
      this[index].ContainedIn = null;

      base.RemoveItem(index);
    }

    protected override void SetItem(int index, TBox item)
    {
      if (index < Count)
        this[index].ContainedIn = null;

      item.ContainedIn = container;

      base.SetItem(index, item);
    }

    private IBoxContainer container;
  }
}
