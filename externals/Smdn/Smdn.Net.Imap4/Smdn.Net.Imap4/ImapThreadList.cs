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
#if NET_3_5
using System.Linq;
#endif

using Smdn.Collections;

namespace Smdn.Net.Imap4 {
  // http://tools.ietf.org/html/rfc5256
  public sealed class ImapThreadList : IEnumerable<ImapThreadList> {
    /// <summary>sequence number or UID</summary>
    public long Number {
      get { return number; }
    }

    public ImapThreadList[] Children {
      get { return children; }
    }

    public bool IsUidList {
      get { return isUidList; }
    }

    public bool IsRoot {
      get { return number == 0; }
    }

    internal static ImapThreadList CreateRootedList(bool isUidList, ImapThreadList[] children)
    {
      return new ImapThreadList(true, isUidList, 0, children);
    }

    internal ImapThreadList(bool isUidList, long number)
      : this(false, number, new ImapThreadList[] {})
    {
    }

    internal ImapThreadList(bool isUidList, long number, ImapThreadList child)
      : this(false, isUidList, number, new ImapThreadList[] {child})
    {
      if (child == null)
        throw new ArgumentNullException("child");
    }

    internal ImapThreadList(bool isUidList, long number, ImapThreadList[] children)
      : this(false, isUidList, number, children)
    {
    }

    private ImapThreadList(bool root, bool isUidList, long number, ImapThreadList[] children)
    {
      if (root)
        number = 0;
      else if (number <= 0)
        throw ExceptionUtils.CreateArgumentMustBeNonZeroPositive("number", number);

      if (children == null)
        throw new ArgumentNullException("children");

      this.isUidList = isUidList;
      this.number = number;
      this.children = children;
    }

    public IEnumerator<ImapThreadList> GetEnumerator()
    {
      return (children as IEnumerable<ImapThreadList>).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return children.GetEnumerator();
    }

    public ImapSequenceSet ToSequenceSet()
    {
      var sequenceOrUidSet = IEnumerableExtensions.EnumerateDepthFirst(this).Select(delegate(ImapThreadList list) {
        return list.number;
      }).ToArray();

      return ImapSequenceSet.CreateSet(isUidList, sequenceOrUidSet);
    }

    public void Traverse(Action<ImapThreadList> action)
    {
      if (action == null)
        throw new ArgumentNullException("action");

      foreach (var child in IEnumerableExtensions.EnumerateDepthFirst(this)) {
        action(child);
      }
    }

    public override string ToString ()
    {
      return string.Format("{{IsUidList={0}, Number={1}, Children={2}}}",
                           isUidList,
                           number,
                           string.Join(", ", Array.ConvertAll(children, delegate(ImapThreadList child) {
                             return child.ToString();
                           }))
                           );
    }

    private readonly bool isUidList;
    private readonly long number;
    private /*readonly*/ ImapThreadList[] children;
  }
}