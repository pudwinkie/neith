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
using System.Collections;
using System.Collections.Generic;

namespace Smdn.Net.Imap4 {
  // seq-number      = nz-number / "*"
  // seq-range       = seq-number ":" seq-number
  // sequence-set    = (seq-number / seq-range) *("," sequence-set)
  public abstract class ImapSequenceSet : IEnumerable<long> {
    public virtual bool IsUidSet {
      get; private set;
    }

    public virtual bool IsEmpty {
      get { return false; }
    }

    public abstract bool IsSingle { get; }
    /*
    public abstract long PossibleMin { get; }
    public abstract long PossibleMax { get; }
    */

    protected ImapSequenceSet(bool isUidSet)
    {
      this.IsUidSet = isUidSet;
    }

    public virtual long ToNumber()
    {
      if (IsSingle)
        return ToArray()[0];
      else
        throw new NotSupportedException("can't get single number from non-single set");
    }

    public abstract long[] ToArray();
    public abstract IEnumerator<long> GetEnumerator();
    public override abstract string ToString();

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public virtual IEnumerable<ImapSequenceSet> SplitIntoEach(int count)
    {
      if (count <= 0)
        throw new ArgumentOutOfRangeException("count", count, "must be non-zero positive number");

      var numbers = new long[count];
      var index = 0;
      var sequential = true;

      foreach (var number in this) {
        if (0 < index)
          sequential &= (numbers[index - 1] == number - 1L);

        numbers[index++] = number;

        if (count <= index) {
          if (sequential)
            yield return CreateRangeSet(IsUidSet, numbers[0], number);
          else
            yield return CreateSet(IsUidSet, numbers);

          index = 0;
          sequential = true;
        }
      }

      if (0 < index) {
        if (sequential)
          yield return CreateRangeSet(IsUidSet, numbers[0], numbers[index - 1]);
        else
          yield return CreateSet(IsUidSet, numbers, 0, index);
      }
    }

    public static ImapSequenceSet FromUri(Uri uri)
    {
      var uid = ImapStyleUriParser.GetUid(uri);

      if (uid == 0L)
        throw new ArgumentException("invalid IMAP-URI form; URI has no UID", "uri");

      return CreateUidSet(uid);
    }

#region "create from:* set"
    public static ImapSequenceSet CreateFromSet(long from)
    {
      return CreateFromSet(false, from);
    }

    public static ImapSequenceSet CreateUidFromSet(long from)
    {
      return CreateFromSet(true, from);
    }

    public static ImapSequenceSet CreateFromSet(bool uidSet, long from)
    {
      return new FromSequenceSet(uidSet, from);
    }
#endregion

#region "create *:to set"
    public static ImapSequenceSet CreateToSet(long to)
    {
      return CreateToSet(false, to);
    }

    public static ImapSequenceSet CreateUidToSet(long to)
    {
      return CreateToSet(true, to);
    }

    public static ImapSequenceSet CreateToSet(bool uidSet, long to)
    {
      return new ToSequenceSet(uidSet, to);
    }
#endregion

#region "create from:to set"
    public static ImapSequenceSet CreateRangeSet(long from, long to)
    {
      return CreateRangeSet(false, from, to);
    }

    public static ImapSequenceSet CreateUidRangeSet(long from, long to)
    {
      return CreateRangeSet(true, from, to);
    }

    public static ImapSequenceSet CreateRangeSet(bool uidSet, long from, long to)
    {
      if (from == to)
        return new SetSequenceSet(uidSet, new[] {from});
      else
        return new RangeSequenceSet(uidSet, from, to);
    }
#endregion

#region "create * set"
    public static ImapSequenceSet CreateAllSet()
    {
      return CreateAllSet(false);
    }

    public static ImapSequenceSet CreateUidAllSet()
    {
      return CreateAllSet(true);
    }

    public static ImapSequenceSet CreateAllSet(bool uid)
    {
      return new AllSequenceSet(uid);
    }
#endregion

#region "create num*(,num) set"
    public static ImapSequenceSet CreateSet(long number, params long[] numbers)
    {
      return CreateSet(false, number, numbers);
    }

    public static ImapSequenceSet CreateUidSet(long number, params long[] numbers)
    {
      return CreateSet(true, number, numbers);
    }

    public static ImapSequenceSet CreateSet(bool uidSet, long number, params long[] numbers)
    {
      return CreateSet(uidSet, numbers.Prepend(number));
    }

    public static ImapSequenceSet CreateSet(long[] numbers)
    {
      return CreateSet(false, numbers);
    }

    public static ImapSequenceSet CreateUidSet(long[] numbers)
    {
      return CreateSet(true, numbers);
    }

    public static ImapSequenceSet CreateSet(bool uidSet, long[] numbers)
    {
      if (numbers == null)
        throw new ArgumentNullException("numbers");

      return new SetSequenceSet(uidSet, (long[])numbers.Clone());
    }

    public static ImapSequenceSet CreateSet(long[] numbers, int start, int count)
    {
      return CreateSet(false, numbers, start, count);
    }

    public static ImapSequenceSet CreateUidSet(long[] numbers, int start, int count)
    {
      return CreateSet(true, numbers, start, count);
    }

    public static ImapSequenceSet CreateSet(bool uidSet, long[] numbers, int start, int count)
    {
      if (numbers == null)
        throw new ArgumentNullException("numbers");
      if (count <= 0)
        throw new ArgumentOutOfRangeException("count", count, "must be non-zero positive number");
      if (start < 0)
        throw new ArgumentOutOfRangeException("start", start, "must be zero or positive number");
      if (numbers.Length < start + count)
        throw new ArgumentOutOfRangeException("count", count, "length < start + count");

      return new SetSequenceSet(uidSet, numbers.Slice(start, count));
    }
#endregion

#region "create combined set"
    public ImapSequenceSet CombineWith(ImapSequenceSet other)
    {
      return this + other;
    }

    public static ImapSequenceSet Combine(ImapSequenceSet x, ImapSequenceSet y)
    {
      return x + y;
    }

    public static ImapSequenceSet operator+ (ImapSequenceSet x, ImapSequenceSet y)
    {
      if (x == null)
        return y;
      else if (y == null)
        return x;

      if (x.IsUidSet != y.IsUidSet)
        throw new ArgumentException("can't contain simultaneously both UID set and sequence set", "x");

      var xx = x as ImapMatchedSequenceSet;
      var yy = y as ImapMatchedSequenceSet;

      if (xx != null) {
        if (xx.IsSavedResult)
          throw new ArgumentException("can't combine with saved result", "x");
        x = xx.SequenceSet;
      }

      if (yy != null) {
        if (yy.IsSavedResult)
          throw new ArgumentException("can't combine with saved result", "y");
        y = yy.SequenceSet;
      }

      var allSets = new List<ImapSequenceSet>();
      var cx = x as CombinedSequenceSet;
      var cy = y as CombinedSequenceSet;

      if (cx == null)
        allSets.Add(x);
      else
        allSets.AddRange(cx.Sets);

      if (cy == null)
        allSets.Add(y);
      else
        allSets.AddRange(cy.Sets);

      return GetRegularizedSet(x.IsUidSet, allSets.ToArray());
    }

    private static ImapSequenceSet GetRegularizedSet(bool uid, ImapSequenceSet[] sets)
    {
      var containsAll = false;
      var combiningSets = new List<ImapSequenceSet>(sets.Length);

      foreach (var s in sets) {
        if (s is AllSequenceSet) {
          containsAll = true;
          break;
        }
        else if (!s.IsEmpty) {
          combiningSets.Add(s);
        }
      }

      // TODO: regularize others

      if (containsAll)
        return new AllSequenceSet(uid);
      else
        return new CombinedSequenceSet(uid, combiningSets.ToArray());
    }
#endregion

#region "sequence set classes"
    private interface IFromSequenceSet {
      long From { get; }
    }

    private interface IToSequenceSet {
      long To { get; }
    }

    // seq-number:*
    private class FromSequenceSet : ImapSequenceSet, IFromSequenceSet {
      public long From {
        get; private set;
      }

      public override bool IsSingle {
        get { return false; }
      }

      /*
      public override long PossibleMax {
        get { return long.MaxValue; }
      }

      public override long PossibleMin {
        get { return From; }
      }
      */

      public FromSequenceSet(bool isUidSet, long from)
        : base(isUidSet)
      {
        if (from <= 0)
          throw new ArgumentOutOfRangeException("from", from, "must be non-zero positive number");

        this.From = from;
      }

      public override IEnumerator<long> GetEnumerator()
      {
        throw new NotSupportedException("can't enumerate 'n:*' set");
      }

      public override long[] ToArray()
      {
        throw new NotSupportedException("can't create array from 'n:*' set");
      }

      public override string ToString()
      {
        return string.Concat(From, ":*");
      }
    }

    // *:seq-number
    private class ToSequenceSet : ImapSequenceSet, IToSequenceSet {
      public long To {
        get; private set;
      }

      public override bool IsSingle {
        get { return false; }
      }

      /*
      public override long PossibleMax {
        get { return To; }
      }

      public override long PossibleMin {
        get { return 1L; }
      }
      */

      public ToSequenceSet(bool isUidSet, long to)
        : base(isUidSet)
      {
        if (to <= 0)
          throw new ArgumentOutOfRangeException("to", to, "must be non-zero positive number");

        this.To = to;
      }

      public override IEnumerator<long> GetEnumerator()
      {
        throw new NotSupportedException("can't enumerate '*:n' set");
      }

      public override long[] ToArray()
      {
        throw new NotSupportedException("can't create array from '*:n' set");
      }

      public override string ToString()
      {
        return string.Concat("*:", To);
      }
    }

    // seq-number:seq-number
    private class RangeSequenceSet : ImapSequenceSet, IFromSequenceSet, IToSequenceSet {
      public long From {
        get; private set;
      }

      public long To {
        get; private set;
      }

      public override bool IsSingle {
        get { return false /*From == To*/; }
      }

      /*
      public override long PossibleMax {
        get { return To; }
      }

      public override long PossibleMin {
        get { return From; }
      }
      */

      public RangeSequenceSet(bool isUidSet, long from, long to)
        : base(isUidSet)
      {
        if (from <= 0)
          throw new ArgumentOutOfRangeException("from", from, "must be non-zero positive number");
        if (to <= from)
          throw new ArgumentOutOfRangeException("to", to, "must be greater than from value");

        this.From = from;
        this.To = to;
      }

      public override IEnumerator<long> GetEnumerator()
      {
        for (var i = From; i <= To; i++) {
          yield return i;
        }
      }

      public override long[] ToArray()
      {
        var arr = new long[To - From + 1];
        var index = 0;

        for (var i = From; i <= To; i++) {
          arr[index++] = i;
        }

        return arr;
      }

      public override string ToString()
      {
        return string.Concat(From, ":", To);
      }
    }

    // *
    private class AllSequenceSet : ImapSequenceSet {
      public override bool IsSingle {
        get { return false; }
      }

      /*
      public override long PossibleMax {
        get { return long.MaxValue; }
      }

      public override long PossibleMin {
        get { return 1L; }
      }
      */

      public AllSequenceSet(bool isUidSet)
        : base(isUidSet)
      {
      }

      public override IEnumerator<long> GetEnumerator()
      {
        throw new NotSupportedException("can't enumerate '*' set");
      }

      public override long[] ToArray()
      {
        throw new NotSupportedException("can't create array from '*' set");
      }

      public override string ToString()
      {
        return "*";
      }
    }

    // seq-number *("," sequence-set)
    private class SetSequenceSet : ImapSequenceSet {
      public override bool IsEmpty {
        get { return Set.Length == 0; }
      }

      public long[] Set {
        get; private set;
      }

      public override bool IsSingle {
        get { return Set.Length == 1; }
      }

      /*
      public override long PossibleMax {
        get { return Set[Set.Length - 1]; // must be sorted }
      }

      public override long PossibleMin {
        get { return Set[0]; // must be sorted }
      }
      */

      public SetSequenceSet(bool isUidSet, long[] @set)
        : base(isUidSet)
      {
        if (@set == null)
          throw new ArgumentNullException("set");

        for (var i = 0; i < @set.Length; i++) {
          if (@set[i] <= 0)
            throw new ArgumentOutOfRangeException("sets",
                                                  @set[i],
                                                  string.Format("must be non-zero positive number (index: {0})", i));
        }

        this.Set = @set;
      }

      public override long ToNumber()
      {
        if (Set.Length == 0)
          throw new NotSupportedException("can't get single number from empty set");
        else if (Set.Length == 1)
          return Set[0];
        else
          throw new NotSupportedException("can't get single number from non-single set");
      }

      public override IEnumerator<long> GetEnumerator()
      {
        foreach (var num in Set) {
          yield return num;
        }
      }

      public override long[] ToArray()
      {
        return Set.Clone() as long[];
      }

      public override string ToString()
      {
        return string.Join(",", Array.ConvertAll(Set, delegate(long val) {
          return val.ToString();
        }));
      }
    }

    // (seq-number / seq-range) *("," sequence-set)
    private class CombinedSequenceSet : ImapSequenceSet {
      private static bool IsSingleSet(ImapSequenceSet s)
      {
        return s.IsSingle;
      }

      public ImapSequenceSet[] Sets {
        get; private set;
      }

      public override bool IsSingle {
        get { return Array.TrueForAll(Sets, IsSingleSet); }
      }

      /*
      public override long PossibleMax {
        get
        {
          var max = 1L;

          foreach (var @set in Sets) {
            if (max < @set.PossibleMax)
              max = @set.PossibleMax;
          }

          return max;
        }
      }

      public override long PossibleMin {
        get
        {
          var min = long.MaxValue;

          foreach (var @set in Sets) {
            if (@set.PossibleMin < min)
              min = @set.PossibleMin;
          }

          return min;
        }
      }
      */

      public CombinedSequenceSet(bool isUidSet, ImapSequenceSet[] sets)
        : base(isUidSet)
      {
        if (sets == null)
          throw new ArgumentNullException("sets");

        foreach (var s in sets) {
          if (s.IsUidSet != isUidSet)
            throw new ArgumentException("can't contain simultaneously both UID set and sequence set", "sets");
        }

        this.Sets = sets;
      }

      public override long ToNumber()
      {
        var singles = Array.FindAll(Sets, IsSingleSet);

        if (singles.Length == 1)
          return singles[0].ToNumber();
        else
          throw new NotSupportedException("can't get single number from non-single set");
      }

      public override IEnumerator<long> GetEnumerator()
      {
        foreach (var s in Sets) {
          foreach (var num in s) {
            yield return num;
          }
        }
      }

      public override long[] ToArray()
      {
        var arr = new List<long>();

        foreach (var s in Sets) {
          arr.AddRange(s.ToArray());
        }

        return arr.ToArray();
      }

      public override string ToString()
      {
        return string.Join(",", Array.ConvertAll(Sets, delegate(ImapSequenceSet @set) {
          return @set.ToString();
        }));
      }
    }
#endregion
  }
}