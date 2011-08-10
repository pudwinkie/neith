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
  // RFC 4315 Internet Message Access Protocol (IMAP) - UIDPLUS extension
  // http://tools.ietf.org/html/rfc4315
  // COPYUID response code
  public class ImapCopiedUidSet : ImapSequenceSet {
    public override bool IsUidSet {
      get { return true; }
    }

    public override bool IsEmpty {
      get { return CopiedUidSet.IsEmpty; }
    }

    public override bool IsSingle {
      get { return CopiedUidSet.IsSingle; }
    }

    /// <value>the UIDVALIDITY of the destination mailbox</value>
    public long UidValidity {
      get; private set;
    }

    /// <value>the UIDs of the message(s) in the source mailbox that were copied</value>
    public ImapSequenceSet CopiedUidSet {
      get; private set;
    }

    /// <value>the UID assigned to the appended message in the destination mailbox</value>
    public ImapSequenceSet AssignedUidSet {
      get; private set;
    }

    internal ImapCopiedUidSet(long uidValidity, ImapSequenceSet copiedUidSet, ImapSequenceSet assignedUidSet)
      : base(true)
    {
      this.UidValidity = uidValidity;
      this.CopiedUidSet = copiedUidSet;
      this.AssignedUidSet = assignedUidSet;
    }

    public override long ToNumber()
    {
      return CopiedUidSet.ToNumber();
    }

    public override long[] ToArray()
    {
      return CopiedUidSet.ToArray();
    }

    public override IEnumerator<long> GetEnumerator()
    {
      return CopiedUidSet.GetEnumerator();
    }

    public override IEnumerable<ImapSequenceSet> SplitIntoEach(int count)
    {
      return CopiedUidSet.SplitIntoEach(count);
    }

    public override string ToString()
    {
      return CopiedUidSet.ToString();
    }
  }
}