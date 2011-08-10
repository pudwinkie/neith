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
  /// <remarks>4.2.4. The "atom:generator" Element</remarks>
  public class Generator {
    //    The content of this element, when present, MUST be a string that is a
    //    human-readable name for the generating agent.  Entities such as
    //    "&amp;" and "&lt;" represent their corresponding characters ("&" and
    //    "<" respectively), not markup.
    public string Value {
      get; set;
    }

    //    The atom:generator element MAY have a "uri" attribute whose value
    //    MUST be an IRI reference [RFC3987].  When dereferenced, the resulting
    //    URI (mapped from an IRI, if necessary) SHOULD produce a
    //    representation that is relevant to that agent.
    public Uri Uri {
      get; set;
    }

    //    The atom:generator element MAY have a "version" attribute that
    //    indicates the version of the generating agent.
    public string Version {
      get; set;
    }

    public Generator()
      : this(null, null, null)
    {
    }

    public Generator(string @value)
      : this(@value, null, null)
    {
    }

    public Generator(string @value, Uri uri, string version)
    {
      this.Value = @value;
      this.Uri = uri;
      this.Version = version;
    }
  }
}