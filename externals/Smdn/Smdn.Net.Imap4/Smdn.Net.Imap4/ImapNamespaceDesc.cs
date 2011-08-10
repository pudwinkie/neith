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

using Smdn.Collections;
using Smdn.Formats;

namespace Smdn.Net.Imap4 {
  public sealed class ImapNamespaceDesc : ICloneable {
    public string Prefix {
      get; private set;
    }

    public string HierarchyDelimiter {
      get; private set;
    }

    public IDictionary<string, string[]> Extensions {
      get; private set;
    }

    /*
     * RFC 5255 - Internet Message Access Protocol Internationalization
     * http://tools.ietf.org/html/rfc5255
     * 3.4. TRANSLATION Extension to the NAMESPACE Response
     */
    public string TranslatedPrefix {
      get
      {
        string[] translatedPrefix;

        if (Extensions.TryGetValue("TRANSLATION", out translatedPrefix))
          return ModifiedUTF7.Decode(translatedPrefix[0]);
        else
          return Prefix;
      }
    }

    public ImapNamespaceDesc(string prefix)
      : this(prefix, null, new Dictionary<string, string[]>())
    {
    }

    public ImapNamespaceDesc(string prefix, string hierarchyDelimiter)
      : this(prefix, hierarchyDelimiter, new Dictionary<string, string[]>())
    {
    }

    public ImapNamespaceDesc(string prefix, string hierarchyDelimiter, IDictionary<string, string[]> extensions)
    {
      if (prefix == null)
        throw new ArgumentNullException("prefix");
      if (extensions == null)
        throw new ArgumentNullException("extensions");

      this.Prefix = prefix;
      this.HierarchyDelimiter = hierarchyDelimiter;
      this.Extensions = extensions.AsReadOnly();
    }

    object ICloneable.Clone()
    {
      return this.Clone();
    }

    public ImapNamespaceDesc Clone()
    {
      var cloned = (ImapNamespaceDesc)MemberwiseClone();
      var extensions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

      foreach (var pair in Extensions) {
        extensions.Add(pair.Key, (string[])pair.Value.Clone());
      }

      cloned.Extensions = extensions.AsReadOnly();

      return cloned;
    }

    public override string ToString()
    {
      return string.Format("{{Prefix={0}, HierarchyDelimiter={1}, Extensions={2}}}",
                           Prefix,
                           HierarchyDelimiter,
                           Extensions);
    }
  }
}
