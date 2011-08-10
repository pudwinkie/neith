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

namespace Smdn.Formats.Notations.Dom {
  public partial class NodeCollection<TNode> : ICollection<TNode> where TNode : Node {
    public int Count {
      get
      {
        int count = 0;

#pragma warning disable 168
        foreach (var node in this) {
          count++;
        }
#pragma warning restore 168

        return count;
      }
    }

    public bool IsReadOnly {
      get { return false; }
    }

    public void Clear()
    {
      foreach (var node in this) {
        node.Parent = null;
      }

      parent.Child = null;
    }

    public void Add(TNode node)
    {
      Insert(Count, node);
    }

    public bool Remove(TNode node)
    {
      if (Contains(node)) {
        RemoveInternal(node);
        return true;
      }

      return false;
    }

    public bool Contains(TNode node)
    {
      foreach (var n in this) {
        if (n == node)
          return true;
      }

      return false;
    }

    public void CopyTo(TNode[] array, int index)
    {
      foreach (var node in this) {
        array.SetValue(node, index++);
      }
    }
  }
}
