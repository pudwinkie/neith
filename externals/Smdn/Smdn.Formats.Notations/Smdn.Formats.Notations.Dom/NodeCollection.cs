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
  public partial class NodeCollection<TNode> where TNode : Node {
    /// <summary>コレクション内の最初のノード</summary>
    internal TNode FirstNode {
      get { return parent.Child; }
    }

    /// <summary>コレクション内の最後のノード</summary>
    internal TNode LastNode {
      get
      {
        if (parent.Child == null)
          return null;

        for (var node = parent.Child; ; node = node.Next as TNode) {
          if (node.Next == null)
            return node;
        }
      }
    }

    internal NodeCollection(Container<TNode> parent)
    {
      if (parent == null)
        throw new ArgumentNullException("parent");

      this.parent = parent;
    }

    public void AddRange(IEnumerable<TNode> nodes)
    {
      int count = Count;

      foreach (var node in nodes) {
        InsertInternal(count++, node);
      }
    }

    public void RemoveRange(IEnumerable<TNode> nodes)
    {
      foreach (var node in nodes) {
        if (!Contains(node))
          throw new InvalidOperationException("collection is not containing node");

        RemoveInternal(node);
      }
    }

    public TNode[] ToArray()
    {
      var arr = new TNode[Count];

      CopyTo(arr, 0);

      return arr;
    }

    public void InsertBefore(TNode refNode, TNode insertNode)
    {
      if (refNode == null)
        throw new ArgumentNullException("refNode");
      if (insertNode == null)
        throw new ArgumentNullException("insertNode");

      var index = IndexOf(refNode);

      if (index < 0)
        throw new ArgumentException("refNode is not in collection", "refNode");

      Insert(index, insertNode);
    }

    private Container<TNode> parent;
  }
}
