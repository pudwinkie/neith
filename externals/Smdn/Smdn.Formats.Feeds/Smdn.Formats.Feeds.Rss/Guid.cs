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

namespace Smdn.Formats.Feeds.Rss {
  // <guid> sub-element of <item>
  // guid stands for globally unique identifier. It's a string that uniquely identifies the item. When present, an aggregator may choose to use this string to determine if an item is new.
  // There are no rules for the syntax of a guid. Aggregators must view them as a string. It's up to the source of the feed to establish the uniqueness of the string.
  public class Guid : IEquatable<Guid> {
    public string Value {
      get; set;
    }

    // It has one optional attribute, domain, a string that identifies a categorization taxonomy. 
    public bool? IsPermaLink {
      get; set;
    }

    public Guid()
      : this(null, null)
    {
    }

    public Guid(string @value)
      : this(@value, null)
    {
    }

    public Guid(string @value, bool? isPermaLink)
    {
      this.Value = @value;
      this.IsPermaLink = isPermaLink;
    }

    public override bool Equals(object o)
    {
      if (o is Guid)
        return Equals(o as Guid);
      else
        return false;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public bool Equals(Guid other)
    {
      return (this.Value == other.Value) && (this.IsPermaLink == other.IsPermaLink);
    }
  }
}
