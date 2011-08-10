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
  // RDF Site Summary 1.0 Modules: Image
  //   http://web.resource.org/rss/1.0/modules/image/
  public class Image : ModuleBase {
    public enum Size {
      Small, // recommended
      Medium,
      Large,
    }

    public const string Prefix = "image";
    public const string NamespaceUri = "http://purl.org/rss/1.0/modules/image/";

    public static readonly Image Null = new Image();

    public override string ModulePrefix {
      get { return Prefix; }
    }

    public override string ModuleNamespaceUri {
      get { return NamespaceUri; }
    }

    protected internal override bool IsNull {
      get { return this == Null; }
    }

    /// <summary></summary>
    public Uri Item {
      get; set;
    }

    public int? ItemWidth {
      get; set;
    }

    public int? ItemHeight {
      get; set;
    }

    /// <summary>Provides a favicon for this RSS channel or item. A favicon is a fixed size icon that can be used within user interfaces such as tree controls, bookmark entries, etc.</summary>
    public Uri Favicon {
      get; set;
    }

    public Size? FaviconSize {
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
        nsmgr.AddNamespace("image", NamespaceUri);
        nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

        var faviconNode = parent.SelectSingleNode("image:favicon", nsmgr);
        var itemNode = parent.SelectSingleNode("image:item", nsmgr);

        if (faviconNode == null && itemNode == null)
          return;

        var about = (faviconNode ?? itemNode).GetSingleNodeValueOf<Uri>("@rdf:about", nsmgr, ConvertUtils.ToUriNullable);

        if (faviconNode == null) {
          Item        = about;
          ItemWidth   = itemNode.GetSingleNodeValueOf<int?>("image:width/text()", nsmgr, null, ConvertUtils.ToInt32Nullable);
          ItemHeight  = itemNode.GetSingleNodeValueOf<int?>("image:height/text()", nsmgr, null, ConvertUtils.ToInt32Nullable);
        }
        else {
          Favicon     = about;
          FaviconSize = faviconNode.GetSingleNodeValueOf<Size?>("@image:size", nsmgr, null, ConvertUtils.ToEnumNullable<Size>);
        }
      }
      finally {
        nsmgr.PopScope();
      }
    }

    internal protected override void Format(FeedBase feed, XmlNode parent)
    {
      if (Favicon == null)
        return;

      var favicon = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, "favicon", NamespaceUri));

      favicon.SetAttribute(FeedPrefixes.Rdf + ":about", FeedNamespaces.Rdf, ConvertUtils.ToStringNullable(Favicon));

      if (FaviconSize != null)
        favicon.SetAttribute(Prefix + ":size", NamespaceUri, FaviconSize.Value.ToString().ToLowerInvariant());
    }

    internal protected override void Format(EntryBase entry, XmlNode parent)
    {
      if (Item == null)
        return;

      var item = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, "item", NamespaceUri));

      item.SetAttribute(FeedPrefixes.Rdf + ":about", FeedNamespaces.Rdf, ConvertUtils.ToStringNullable(Item));

      if (ItemWidth != null)
        item.AppendChild(parent.OwnerDocument.CreateElement(Prefix, "width", NamespaceUri))
            .AppendChild(parent.OwnerDocument.CreateTextNode(ConvertUtils.ToStringNullable(ItemWidth)));

      if (ItemHeight != null)
        item.AppendChild(parent.OwnerDocument.CreateElement(Prefix, "height", NamespaceUri))
            .AppendChild(parent.OwnerDocument.CreateTextNode(ConvertUtils.ToStringNullable(ItemHeight)));
    }
  }
}