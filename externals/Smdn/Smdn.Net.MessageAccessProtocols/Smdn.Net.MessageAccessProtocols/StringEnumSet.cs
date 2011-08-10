// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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

namespace Smdn.Net.MessageAccessProtocols {
  public class StringEnumSet<TStringEnum> :
    ISet<TStringEnum>
    where TStringEnum : class, IStringEnum
  {
    public bool IsReadOnly {
      get; private set;
    }

    public int Count {
      get { return set.Count; }
    }

    public IEqualityComparer<IStringEnum> Comparer {
      get { return comparer; }
    }

    public StringEnumSet()
      : this(false, null, (IEqualityComparer<string>)null)
    {
    }

    public StringEnumSet(bool isReadOnly)
      : this(isReadOnly, null, (IEqualityComparer<string>)null)
    {
    }

    public StringEnumSet(bool isReadOnly, IEqualityComparer<string> comparer)
      : this(isReadOnly, null, comparer == null ? null : new StringEnumComparer(comparer))
    {
    }

    public StringEnumSet(IEnumerable<TStringEnum> items)
      : this(false, items, (IEqualityComparer<string>)null)
    {
    }

    public StringEnumSet(IEnumerable<TStringEnum> items, IEqualityComparer<string> comparer)
      : this(false, items, comparer)
    {
    }

    public StringEnumSet(bool isReadOnly, IEnumerable<TStringEnum> items, IEqualityComparer<string> comparer)
      : this(isReadOnly, items, comparer == null ? null : new StringEnumComparer(comparer))
    {
      if (items == null)
        throw new ArgumentNullException("items");
    }

    private StringEnumSet(bool isReadOnly, IEnumerable<TStringEnum> items, StringEnumComparer comparer)
    {
      IsReadOnly = isReadOnly;

      this.comparer = comparer ?? new StringEnumComparer(StringComparer.OrdinalIgnoreCase);
      this.set = new Dictionary<string, TStringEnum>(this.comparer);

      if (items != null)
        InternalAddRange(items);
    }

    public bool Contains(string @value)
    {
      return set.ContainsKey(@value);
    }

    public TStringEnum Find(string @value)
    {
      TStringEnum item;

      if (set.TryGetValue(@value, out item))
        return item;
      else
        return null;
    }

    public bool TryGet(string @value, out TStringEnum item)
    {
      return set.TryGetValue(@value, out item);
    }

    public void AddRange(IEnumerable<TStringEnum> items)
    {
      if (items == null)
        throw new ArgumentNullException("items");

      CheckReadOnly();

      InternalAddRange(items);
    }

    private void InternalAddRange(IEnumerable<TStringEnum> items)
    {
      foreach (var item in items) {
        if (item == null)
          throw new ArgumentException("contains null");

        set[item.Value] = item;
      }
    }

    public int RemoveWhere(Predicate<TStringEnum> match)
    {
      if (match == null)
        throw new ArgumentNullException("match");

      CheckReadOnly();

      var removal = this.set.Values.Where(new Func<TStringEnum, bool>(match)).ToList();

      foreach (var item in removal) {
        this.set.Remove(item.Value);
      }

      return removal.Count;
    }

#region "ISet"
    public bool Add(TStringEnum item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      CheckReadOnly();

      try {
        set.Add(item.Value, item);
        return true;
      }
      catch (ArgumentException) {
        return false;
      }
    }

    void ICollection<TStringEnum>.Add(TStringEnum item)
    {
      Add(item);
    }

    public bool Remove(TStringEnum item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      CheckReadOnly();

      return set.Remove(item.Value);
    }

    public void Clear()
    {
      CheckReadOnly();

      set.Clear();
    }

    public void CopyTo(TStringEnum[] array, int arrayIndex)
    {
      set.Values.CopyTo(array, arrayIndex);
    }

    public bool Contains(TStringEnum item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      return set.ContainsKey(item.Value);
    }

    IEnumerator<TStringEnum> IEnumerable<TStringEnum>.GetEnumerator()
    {
      return set.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return set.Values.GetEnumerator();
    }

    private StringEnumSet<TStringEnum> ToSet(IEnumerable<TStringEnum> other)
    {
      var s = other as StringEnumSet<TStringEnum>;

      if (s == null || !s.comparer.Equals(this.comparer))
        return new StringEnumSet<TStringEnum>(false, other, this.comparer);
      else
        return s;
    }

    public void ExceptWith(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      CheckReadOnly();

      foreach (var item in other) {
        this.set.Remove(item.Value);
      }
    }

    public void IntersectWith(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      //CheckReadOnly();

      var otherSet = ToSet(other);

      RemoveWhere(delegate(TStringEnum item) {
        return !otherSet.Contains(item);
      });
    }

    public void SymmetricExceptWith(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      CheckReadOnly();

      var otherSet = ToSet(other);

      foreach (var item in otherSet) {
        if (this.set.ContainsKey(item.Value))
          this.set.Remove(item.Value);
        else
          this.set.Add(item.Value, item);
      }
    }

    public void UnionWith(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      CheckReadOnly();

      foreach (var item in other) {
        try {
          this.set.Add(item.Value, item);
        }
        catch (ArgumentException) {
          // ignore exceptions
        }
      }
    }

    public bool IsProperSubsetOf(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToSet(other);

      if (this.set.Count == 0)
        return (otherSet.Count == 0);

      if (otherSet.Count <= this.set.Count)
        return false;

      foreach (var item in this) {
        if (!otherSet.set.ContainsKey(item.Value))
          return false;
      }

      return true;
    }

    public bool IsSubsetOf(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToSet(other);

      if (otherSet.Count < this.set.Count)
        return false;

      foreach (var item in this) {
        if (!otherSet.set.ContainsKey(item.Value))
          return false;
      }

      return true;
    }

    public bool IsProperSupersetOf(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToSet(other);

      if (this.set.Count <= otherSet.Count)
        return false;

      foreach (var item in otherSet) {
        if (!this.set.ContainsKey(item.Value))
          return false;
      }

      return true;
    }

    public bool IsSupersetOf(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToSet(other);

      if (this.set.Count < otherSet.Count)
        return false;

      foreach (var item in otherSet) {
        if (!this.set.ContainsKey(item.Value))
          return false;
      }

      return true;
    }

    public bool Overlaps(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToSet(other);

      foreach (var item in otherSet) {
        if (this.set.ContainsKey(item.Value))
          return true;
      }

      return false;
    }

    public bool SetEquals(IEnumerable<TStringEnum> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToSet(other);

      if (this.set.Count != otherSet.Count)
        return false;

      foreach (var item in otherSet) {
        if (!this.set.ContainsKey(item.Value))
          return false;
      }

      return true;
    }
#endregion

    public TStringEnum[] ToArray()
    {
      var items = new TStringEnum[set.Count];

      set.Values.CopyTo(items, 0);

      return items;
    }

    public string[] ToStringArray()
    {
      return Array.ConvertAll(ToArray(), delegate(TStringEnum val) {
        return val.Value;
      });
    }

    public override string ToString()
    {
#if NET_4_0
      return string.Join(", ", set.Values.Select(val => val.Value));
#else
      return string.Join(", ", ToStringArray());
#endif
    }

    private void CheckReadOnly()
    {
      if (IsReadOnly)
        throw new NotSupportedException("set is readonly");
    }

    private StringEnumComparer comparer;
    private Dictionary<string, TStringEnum> set;
  }
}
