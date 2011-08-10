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
  /// <remarks>3.2. Person Constructs</remarks>
  //    A Person construct is an element that describes a person,
  //    corporation, or similar entity (hereafter, 'person').
  public class Person {
    /// <remarks>3.2.1. The "atom:name" Element</remarks>
    //    The "atom:name" element's content conveys a human-readable name for
    //    the person.  The content of atom:name is Language-Sensitive.  Person
    //    constructs MUST contain exactly one "atom:name" element.
    public string Name {
      get; set;
    }

    /// <remarks>3.2.2. The "atom:uri" Element</remarks>
    //    The "atom:uri" element's content conveys an IRI associated with the
    //    person.  Person constructs MAY contain an atom:uri element, but MUST
    //    NOT contain more than one.  The content of atom:uri in a Person
    //    construct MUST be an IRI reference [RFC3987].
    public Uri Uri {
      get; set;
    }

    /// <remarks>3.2.3. The "atom:email" Element</remarks>
    //    The "atom:email" element's content conveys an e-mail address
    //    associated with the person.  Person constructs MAY contain an
    //    atom:email element, but MUST NOT contain more than one.  Its content
    //    MUST conform to the "addr-spec" production in [RFC2822].
    public string EMail {
      get; set;
    }

    public Person()
      : this(null, null, null)
    {
    }

    public Person(string name)
      : this(name, null, null)
    {
    }

    public Person(string name, Uri uri, string email)
    {
      this.Name = name;
      this.Uri = uri;
      this.EMail = email;
    }
  }
}