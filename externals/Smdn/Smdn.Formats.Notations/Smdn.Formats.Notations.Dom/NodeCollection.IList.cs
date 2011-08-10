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
  public partial class NodeCollection<TNode> : IList<TNode> where TNode : Node {
    public TNode this[int index] {
      get
      {
        foreach (var n in this) {
          if (index-- == 0)
            return n;
        }

        return null;
      }

      set { Set(index, value); }
    }

    private void Set(int index, TNode setNode)
    {
      var node = this[index];

      if (node.Prev != null)
        node.Prev.Next = setNode;

      if (node.Next != null)
        node.Next.Prev = setNode;

      if (node == parent.Child)
        parent.Child = setNode;

      setNode.Parent = parent;

      node.Parent = null;
      node.Next = null;
      node.Prev = null;
    }

    public int IndexOf(TNode node)
    {
      int index = 0;

      foreach (var n in this) {
        if (n == node)
          return index;

        index++;
      }

      return -1;
    }

    public void Insert(int index, TNode insertNode)
    {
      InsertInternal(index, insertNode);
    }

    private void InsertInternal(int index, TNode insertNode)
    {
      if (index < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("index", index);

      int count = Count;

      if (count < index)
        throw new ArgumentOutOfRangeException("index");

      if (Contains(insertNode))
        throw new InvalidOperationException(string.Format("already inserted (index: {0}, node: {1})", index, insertNode));

      if (parent.Child == null) {
        insertNode.Next = null;
        insertNode.Prev = null;

        parent.Child = insertNode;
      }
      else {
        var prev = this[index - 1];
        var next = (count == index) ? null : this[index]; 

        // リンクをつなぎ変える
        if (0 == index)
          parent.Child = insertNode;

        insertNode.Prev = prev;
        if (prev != null)
          prev.Next = insertNode;

        insertNode.Next = next;
        if (next != null)
          next.Prev = insertNode;
      }
    
      insertNode.Parent = parent;
    }

    public void RemoveAt(int index)
    {
      if (index < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("index", index);

      int count = Count;

      if (count <= index)
        throw new ArgumentOutOfRangeException("index");

      RemoveInternal(this[index]);
    }

    private void RemoveInternal(TNode node)
    {
      // リンクをつなぎ変える
      if (node == parent.Child)
        parent.Child = node.Next as TNode;

      if (node.Prev != null)
        node.Prev.Next = node.Next;

      if (node.Next != null)
        node.Next.Prev = node.Prev;

      node.Parent = null;
      node.Prev = null;
      node.Next = null;
    }
  }
}
