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
  public abstract class Node {
    /// <summary>前にある兄弟ノード</summary>
    public Node Prev {
      get { return prev; }
      internal set
      {
        prev = value;
        if (prev != null) 
          prev.next = this;
      }
    }

    /// <summary>次にある兄弟ノード</summary>
    public Node Next {
      get { return next; }
      internal set
      {
        next = value;
        if (next != null)
          next.prev = this;
      }
    }

    /// <summary>親ノード</summary>
    public Node Parent {
      get { return parent; }
      internal set
      {
        parent = value;
      }
    }

    public override string ToString()
    {
      return string.Format("{{{0}}}", GetType().Name);
    }

    public IEnumerable<Node> GetNodeTraverser()
    {
      for (Node node = this; node != null; node = node.GetNextNode()) {
        yield return node;
      }
    }

    protected internal Node GetNextNode()
    {
      Node node = this;

      for (;;) {
        if (node is Container<Node> && (node as Container<Node>).Child != null) {
          node = (node as Container<Node>).Child;
        }
        else if (node.Next != null) {
          node = node.Next;
        }
        else {
          for (;;) {
            node = node.Parent;

            if (node == null) {
              return null;
            }
            else if (node.Next == null) {
              continue;
            }
            else {
              node = node.Next;
              break;
            }
          }
        }

        return node;
      }
    }

    protected internal Node GetPrevNode()
    {
      Node node = this;

      for (;;) {
        if (node.Prev != null) {
          node = node.Prev;

          for (;;) {
            if (node is Container<Node> && (node as Container<Node>).Child == null)
              break;
            else
              node = (node as Container<Node>).Nodes.LastNode;
          }
        }
        else if (node.Parent != null) {
          node = node.Parent;
          if (node == null)
            return null;
        }
        else {
          return null;
        }

        return node;
      }
    }

    private Node parent = null;
    private Node prev = null;
    private Node next = null;

#if DEBUG
    public static void Dump(IEnumerable<Node> nodes, int indent)
    {
      foreach (var node in nodes) {
        Dump(node, indent);
      }
    }

    public static void Dump(Node node, int indent)
    {
      Console.Error.WriteLine("{0}{1}", new string(' ', indent * 3), node);

      if (node is DefinitionListItem) {
        var item = node as DefinitionListItem;

        foreach (var subnode in item.Term) {
          Dump(subnode, indent + 1);
        }
        foreach (var subnode in item.Nodes) {
          Dump(subnode, indent + 1);
        }
      }
      else if (node is Table) {
        foreach (var subnode in (node as Table).Nodes) {
          Dump(subnode, indent + 1);
        }
      }
      else if (node is Container<Node>) {
        foreach (var subnode in (node as Container<Node>).Nodes) {
          Dump(subnode, indent + 1);
        }
      }
    }
#endif
  }
}
