// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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

namespace Smdn.Net.MessageAccessProtocols {
  public class StringEnumList<TStringEnum> :
    IStringEnumSet<TStringEnum>,
    ICollection<TStringEnum>
    where TStringEnum : class, IStringEnum
  {
    public TStringEnum this[string @value] {
      get { return values[@value]; }
    }

    public bool IsReadOnly {
      get; private set;
    }

    public int Count {
      get { return values.Count; }
    }

    public StringEnumList()
      : this(false)
    {
    }

    public StringEnumList(bool isReadOnly)
    {
      IsReadOnly = isReadOnly;

      this.values = new Dictionary<string, TStringEnum>(16, StringComparer.OrdinalIgnoreCase);
    }

    public StringEnumList(IEnumerable<TStringEnum> values)
      : this(false, values)
    {
    }

    public StringEnumList(bool isReadOnly, IEnumerable<TStringEnum> values)
      : this(isReadOnly)
    {
      if (values == null)
        throw new ArgumentNullException("values");

      InternalAddRange(values);
    }

    public bool Has(string @value)
    {
      return values.ContainsKey(@value);
    }

    public bool Has(TStringEnum @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      return values.ContainsKey(@value.Value);
    }

    public TStringEnum Find(string @value)
    {
      TStringEnum found;

      if (values.TryGetValue(@value, out found))
        return found;
      else
        return null;
    }

#region "ICollection"
    bool ICollection<TStringEnum>.Contains(TStringEnum @value)
    {
      return values.ContainsKey(@value.Value);
    }

    public void Add(TStringEnum @value)
    {
      if (@value == null)
        throw new ArgumentNullException("value");

      CheckReadOnly();

      values.Add(@value.Value, @value);
    }

    public void AddRange(IEnumerable<TStringEnum> values)
    {
      if (values == null)
        throw new ArgumentNullException("values");

      CheckReadOnly();

      InternalAddRange(values);
    }

    private void InternalAddRange(IEnumerable<TStringEnum> values)
    {
      foreach (var val in values) {
        this.values.Add(val.Value, val);
      }
    }

    public bool Remove(TStringEnum @value)
    {
      CheckReadOnly();

      if (Has(@value))
        return values.Remove(@value.Value);
      else
        return false;
    }

    public void Clear()
    {
      CheckReadOnly();

      values.Clear();
    }

    public void CopyTo(TStringEnum[] array, int arrayIndex)
    {
      values.Values.CopyTo(array, arrayIndex);
    }
#endregion

#region "IEnumerable"
    public IEnumerator<TStringEnum> GetEnumerator()
    {
      return values.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return values.Values.GetEnumerator();
    }
#endregion

    public TStringEnum[] ToArray()
    {
      var vals = new TStringEnum[values.Count];

      values.Values.CopyTo(vals, 0);

      return vals;
    }

    public string[] ToStringArray()
    {
      return Array.ConvertAll(ToArray(), delegate(TStringEnum val) {
        return val.Value;
      });
    }

    public override string ToString()
    {
      return string.Join(", ", ToStringArray());
    }

    private void CheckReadOnly()
    {
      if (IsReadOnly)
        throw new NotSupportedException("list is readonly");
    }

    private Dictionary<string, TStringEnum> values;
  }
}
