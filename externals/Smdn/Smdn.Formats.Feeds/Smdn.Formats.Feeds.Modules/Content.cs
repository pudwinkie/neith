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
using System.Xml;

using Smdn.Xml;

namespace Smdn.Formats.Feeds.Modules {
  // RDF Site Summary 1.0 Modules: Content
  //   http://web.resource.org/rss/1.0/modules/syndication/
  public class Content : ModuleBase {
    public const string Prefix = "content";
    public const string NamespaceUri = "http://purl.org/rss/1.0/modules/content/";

    public static readonly Content Null = new Content();

    public override string ModulePrefix {
      get { return Prefix; }
    }

    public override string ModuleNamespaceUri {
      get { return NamespaceUri; }
    }

    protected internal override bool IsNull {
      get { return this == Null; }
    }

    /// <summary>An element whose contents are the entity-encoded or CDATA-escaped version of the content of the item.</summary>
    public string Encoded {
      get; set;
    }

    internal protected override void Parse(EntryBase entry, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("content", NamespaceUri);

        Encoded = parent.GetSingleNodeValueOf("content:encoded/text()", nsmgr);
      }
      finally {
        nsmgr.PopScope();
      }
    }

    internal protected override void Format(EntryBase entry, XmlNode parent)
    {
      if (Encoded == null)
        return;

      parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, "encoded", NamespaceUri))
            .AppendChild(parent.OwnerDocument.CreateCDataSection(Encoded));
    }
  }
}