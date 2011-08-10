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

namespace Smdn.Formats.Feeds.Atom {
  /// <remarks>4.2.2. The "atom:category" Element</remarks>
  public class Category {
    // 4.2.2.1. The "term" Attribute
    //    The "term" attribute is a string that identifies the category to
    //    which the entry or feed belongs.  Category elements MUST have a
    //    "term" attribute.
    /// <remarks>4.2.2.1. The "term" Attribute</remarks>
    public string Term {
      get; set;
    }

    // 4.2.2.2. The "scheme" Attribute
    //    The "scheme" attribute is an IRI that identifies a categorization
    //    scheme.  Category elements MAY have a "scheme" attribute.
    /// <remarks>4.2.2.2. The "scheme" Attribute</remarks>
    public Uri Scheme {
      get; set;
    }

    // 4.2.2.3. The "label" Attribute
    //    The "label" attribute provides a human-readable label for display in
    //    end-user applications.  The content of the "label" attribute is
    //    Language-Sensitive.  Entities such as "&amp;" and "&lt;" represent
    //    their corresponding characters ("&" and "<", respectively), not
    //    markup.  Category elements MAY have a "label" attribute.
    /// <remarks>4.2.2.3. The "label" Attribute</remarks>
    public string Label {
      get; set;
    }

    public Category()
      : this(null, null, null)
    {
    }

    public Category(string term)
      : this(term, null, null)
    {
    }

    public Category(string term, Uri scheme, string label)
    {
      this.Term = term;
      this.Scheme = scheme;
      this.Label = label;
    }
  }
}