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

using Smdn.Collections;

namespace Smdn.Net.Imap4.WebClients {
  public class ImapThreadTree : IEnumerable<ImapThreadTree> {
    public bool IsRoot {
      get { return MessageAttribute == null; }
    }

    [CLSCompliant(false)]
    public IImapMessageAttribute MessageAttribute {
      get; private set;
    }

    public ImapThreadTree Parent {
      get; private set;
    }

    public ImapThreadTree[] Children {
      get; private set;
    }

    [CLSCompliant(false)]
    public ImapThreadTree(bool isRoot, IImapMessageAttribute messageAttribute, ImapThreadTree[] children)
    {
      if (!isRoot && messageAttribute == null)
        throw new ArgumentNullException("messageAttribute");

      if (children == null)
        throw new ArgumentNullException("children");

      if (isRoot) {
        this.Parent = null;
        this.MessageAttribute = null;
      }
      else {
        this.MessageAttribute = messageAttribute;
      }

      foreach (var child in children) {
        child.Parent = this;
      }

      this.Children = children;
    }

    public IEnumerator<ImapThreadTree> GetEnumerator()
    {
      return (Children as IEnumerable<ImapThreadTree>).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return Children.GetEnumerator();
    }

    public void Traverse(Action<ImapThreadTree> action)
    {
      if (action == null)
        throw new ArgumentNullException("action");

      foreach (var child in IEnumerableExtensions.EnumerateDepthFirst(this)) {
        action(child);
      }
    }
  }
}
