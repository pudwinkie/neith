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

using Smdn.Formats.Notations.Dom;

namespace Smdn.Formats.Notations.Hatena.Dom {
  public class KeywordLink : Anchor {
    public string Keyword {
      get; private set;
    }

    public string Group {
      get; private set;
    }

    public bool IsGroup {
      get { return Group != null; }
    }

    public KeywordLink(string keyword, IEnumerable<Node> nodes)
      : this(false, null, keyword, nodes)
    {
    }

    public KeywordLink(string group, string keyword, IEnumerable<Node> nodes)
      : this(true, group, keyword, nodes)
    {
    }

    private KeywordLink(bool isGroup, string group, string keyword, IEnumerable<Node> nodes)
      : base(GetUri(isGroup, group, keyword), keyword, nodes)
    {
      this.Group = isGroup ? group : null;
      this.Keyword = keyword;
    }

    private static string GetUri(bool isGroup, string group, string keyword)
    {
      if (isGroup && string.IsNullOrEmpty(group)) // XXX: is empty
        throw new ArgumentNullException("group");
      if (string.IsNullOrEmpty(keyword)) // XXX: is empty
        throw new ArgumentNullException("keyword");

      return isGroup
        ? string.Format("http://{0}.g.hatena.ne.jp/keyword/{1}", group, keyword)
        : "http://d.hatena.ne.jp/keyword/" + keyword;
    }
  }
}
