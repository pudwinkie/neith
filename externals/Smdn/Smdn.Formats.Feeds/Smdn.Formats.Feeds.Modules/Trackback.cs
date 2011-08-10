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
  // RDF Site Summary 1.0 Modules: Trackback
  //   http://madskills.com/public/xml/rss/module/trackback/
  public class Trackback : ModuleBase {
    public const string Prefix = "trackback";
    public const string NamespaceUri = "http://madskills.com/public/xml/rss/module/trackback/";

    public static readonly Trackback Null = new Trackback();

    public override string ModulePrefix {
      get { return Prefix; }
    }

    public override string ModuleNamespaceUri {
      get { return NamespaceUri; }
    }

    protected internal override bool IsNull {
      get { return this == Null; }
    }

    /// <summary>trackback:ping is a sub-element of an RSS item, and contains the item's TrackBack URL. Each RSS item may contain only one instance of trackback:ping</summary>
    public Uri Ping {
      get; set;
    }

    /// <summary>trackback:about is a sub-element of an RSS item, and contains a TrackBack URL that was pinged in reference to this RSS item. Each RSS item may contain zero or more instances of trackback:about.</summary>
    public Uri About {
      get; set;
    }

    internal protected override void Parse(EntryBase entry, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("tb", NamespaceUri);
        nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

        About = parent.GetSingleNodeValueOf<Uri>("tb:about/@rdf:resource", nsmgr, ConvertUtils.ToUriNullable)
             ?? parent.GetSingleNodeValueOf<Uri>("tb:about/text()", nsmgr, ConvertUtils.ToUriNullable); // for RSS 2.0
        Ping  = parent.GetSingleNodeValueOf<Uri>("tb:ping/@rdf:resource", nsmgr, ConvertUtils.ToUriNullable)
             ?? parent.GetSingleNodeValueOf<Uri>("tb:ping/text()", nsmgr, ConvertUtils.ToUriNullable); // for RSS 2.0
      }
      finally {
        nsmgr.PopScope();
      }
    }

    internal protected override void Format(EntryBase entry, XmlNode parent)
    {
      foreach (var pair in new[] {
        new {LocalName = "ping",  Value = ConvertUtils.ToStringNullable(Ping)},
        new {LocalName = "about", Value = ConvertUtils.ToStringNullable(About)},
      }) {
        if (pair.Value == null)
          continue;

        var tr = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, pair.LocalName, NamespaceUri));

        if (entry is Rss.Item)
          // RSS 2.0
          tr.AppendChild(parent.OwnerDocument.CreateTextNode(pair.Value));
        else
          tr.SetAttribute(FeedPrefixes.Rdf + ":resource", FeedNamespaces.Rdf, pair.Value);
      }
    }
  }
}