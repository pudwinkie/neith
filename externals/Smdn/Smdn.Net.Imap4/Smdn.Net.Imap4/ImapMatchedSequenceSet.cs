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
  public class ImapMatchedSequenceSet : ImapSequenceSet {
    internal ImapSequenceSet SequenceSet {
      get; set;
    }

    public override bool IsUidSet {
      get { return SequenceSet.IsUidSet; }
    }

    public override bool IsEmpty {
      get { return !IsSavedResult && (SequenceSet == null || SequenceSet.IsEmpty); }
    }

    public override bool IsSingle {
      get { return IsSavedResult ? false : SequenceSet.IsSingle; }
    }

    /*
     * RFC 4466 - Collected Extensions to IMAP4 ABNF
     * http://tools.ietf.org/html/rfc4466
     */
    //// <value>the highest mod-sequence for all messages being returned</value>
    [CLSCompliant(false)]
    public ulong? HighestModSeq {
      get; internal set;
    }

    /*
     * RFC 4551 - IMAP Extension for Conditional STORE Operation or Quick Flag Changes Resynchronization
     * http://tools.ietf.org/html/rfc4551
     */
    public string Tag {
      get; internal set;
    }

    public long? Min {
      get; internal set;
    }

    public long? Max {
      get; internal set;
    }

    public long? Count {
      get; internal set;
    }

    /*
     * RFC 5182 - IMAP Extension for Referencing the Last SEARCH Result
     * http://tools.ietf.org/html/rfc5182
     */
    public bool IsSavedResult {
      get; internal set;
    }

    public static ImapMatchedSequenceSet CreateEmpty(bool uid)
    {
      return new ImapMatchedSequenceSet(ImapSequenceSet.CreateSet(uid, new long[] {}));
    }

    public static ImapMatchedSequenceSet CreateSavedResult(ImapMatchedSequenceSet result)
    {
      var saved = (ImapMatchedSequenceSet)result.MemberwiseClone();

      saved.IsSavedResult = true;

      return saved;
    }

    public ImapMatchedSequenceSet(ImapSequenceSet sequenceSet)
      : base(sequenceSet.IsUidSet)
    {
      this.SequenceSet = sequenceSet;
      this.IsSavedResult = false;
      this.Tag = null;
      this.HighestModSeq = null;
      this.Min = null;
      this.Max = null;
      this.Count = null;
    }

    [CLSCompliant(false)]
    public ImapMatchedSequenceSet(ImapSequenceSet sequenceSet, ulong highestModSeq)
      : this(sequenceSet)
    {
      this.HighestModSeq = highestModSeq;
    }

    public override long ToNumber()
    {
      if (IsSavedResult)
        throw new NotSupportedException("can't get single number from saved result");
      else
        return SequenceSet.ToNumber();
    }

    public override IEnumerator<long> GetEnumerator()
    {
      if (IsSavedResult)
        throw new NotSupportedException("can't enumerate saved result");
      else
        return SequenceSet.GetEnumerator();
    }

    public override IEnumerable<ImapSequenceSet> SplitIntoEach(int count)
    {
      if (IsSavedResult)
        return new[] {this};
      else
        return SequenceSet.SplitIntoEach(count);
    }

    public override long[] ToArray()
    {
      if (IsSavedResult)
        throw new NotSupportedException("can't create array from saved result");
      else
        return SequenceSet.ToArray();
    }

    public override string ToString()
    {
      // RFC 5182 - IMAP Extension for Referencing the Last SEARCH Result
      // http://tools.ietf.org/html/rfc5182
      // 1. Introduction
      //    The client can use the "$" marker to reference the content of this
      //    internal variable.
      if (IsSavedResult)
        return "$";
      else
        return SequenceSet.ToString();
    }
  }
}
