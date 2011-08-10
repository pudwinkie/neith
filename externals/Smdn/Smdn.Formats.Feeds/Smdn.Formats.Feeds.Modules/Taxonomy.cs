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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Xml;

namespace Smdn.Formats.Feeds.Modules {
  // RDF Site Summary 1.0 Modules: Taxonomy
  //   http://web.resource.org/rss/1.0/modules/taxonomy/
  public class Taxonomy : ModuleBase {
    public const string Prefix = "taxo";
    public const string NamespaceUri = "http://purl.org/rss/1.0/modules/taxonomy/";

    public static readonly Taxonomy Null = new Taxonomy();

    public override string ModulePrefix {
      get { return Prefix; }
    }

    public override string ModuleNamespaceUri {
      get { return NamespaceUri; }
    }

    protected internal override bool IsNull {
      get { return this == Null; }
    }

    /// <summary> This element gives (using an rdf:Bag/rdf:li structure) a list of topics. These topics may or may not be defined within the scope of the current channel. The location of documents describing the topics may optionally be given using a generic include module to be defined.</summary>
    public Uri[] Topics {
      get; set;
    }

    internal protected override void Parse(FeedBase feed, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      Parse(parent, nsmgr);
    }

    internal protected override void Parse(EntryBase entry, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      Parse(parent, nsmgr);
    }

    private void Parse(XmlNode parent, XmlNamespaceManager nsmgr)
    {
      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("taxo", NamespaceUri);
        nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

        Topics = parent.GetNodeValuesOf<Uri>("taxo:topics/rdf:Bag/rdf:li/@resource", nsmgr, delegate(string val) {
          try {
            return new Uri(val);
          }
          catch {
            // ignore exceptions
            return null;
          }
        }).ToArray();
      }
      finally {
        nsmgr.PopScope();
      }
    }

    internal protected override void Format(EntryBase entry, XmlNode parent)
    {
      if (Topics == null || Topics.Length == 0)
        return;

      var taxo = parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, "topics", NamespaceUri));
      var bag = taxo.AppendChild(parent.OwnerDocument.CreateElement(FeedPrefixes.Rdf, "Bag", FeedNamespaces.Rdf));

      foreach (var topic in Topics) {
        var li = (XmlElement)bag.AppendChild(parent.OwnerDocument.CreateElement(FeedPrefixes.Rdf, "li", FeedNamespaces.Rdf));
        li.SetAttribute("resource", ConvertUtils.ToStringNullable(topic));
      }
    }
  }
}