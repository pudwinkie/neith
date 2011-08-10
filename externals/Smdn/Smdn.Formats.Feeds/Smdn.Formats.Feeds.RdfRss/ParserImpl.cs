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

namespace Smdn.Formats.Feeds.RdfRss {
  internal class ParserImpl : ParserCore {
    internal protected ParserImpl(FeedVersion version)
      : base(version)
    {
    }

    internal protected override IFeed Parse(XmlDocument rss, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      var nsmgr = new XmlNamespaceManager(rss.NameTable);

      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("rss", FeedNamespaces.Rss_1_0);
        nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

        var channelNode = rss.SelectSingleNode("/rdf:RDF/rss:channel", nsmgr);

        var seqNode = channelNode.SelectSingleNode("rss:items/rdf:Seq", nsmgr);

        var itemNodes = (seqNode == null)
          ? GetItemNodes(rss, nsmgr) // parse /rdf:RDF/rss:item
          : GetItemNodesFromRdfSeq(seqNode, nsmgr); // parse /rdf:RDF/rss:channel/rss:items/rdf:Seq

        // parse /rdf:RDF/rss:item
        var channel = new Channel(itemNodes.ConvertAll(delegate(XmlNode itemNode) {
          var item = new Item();

          if (!discardSourceXml)
            item.SourceNode = itemNode;

          item.Resource        = itemNode.GetSingleNodeValueOf<Uri>("rdf:about/text()", nsmgr, ConvertUtils.ToUriNullable); // XXX
          item.Link            = itemNode.GetSingleNodeValueOf<Uri>("rss:link/text()", nsmgr, ConvertUtils.ToUriNullable);
          item.Title           = itemNode.GetSingleNodeValueOf("rss:title/text()", nsmgr);
          item.Description     = itemNode.GetSingleNodeValueOf("rss:description/text()", nsmgr);

          ParseModules(item, itemNode, nsmgr);

          if (hasher != null) {
            hasher.Initialize();

            item.Hash = hasher.ComputeHash(item, itemNode);
          }

          return item;
        }));

        // parse /rdf:RDF/rss:channel
        if (!discardSourceXml)
          channel.SourceNode = channelNode;

        channel.Link           = channelNode.GetSingleNodeValueOf<Uri>("rss:link/text()", nsmgr, ConvertUtils.ToUriNullable);
        channel.Title          = channelNode.GetSingleNodeValueOf("rss:title/text()", nsmgr);
        channel.Description    = channelNode.GetSingleNodeValueOf("rss:description/text()", nsmgr);

        ParseModules(channel, channelNode, nsmgr);

        return channel;
      }
      finally {
        nsmgr.PopScope();
      }
    }

    private static List<XmlNode> GetItemNodes(XmlDocument rss, XmlNamespaceManager nsmgr)
    {
      var nodes = new SortedDictionary<int, XmlNode>();

      foreach (XmlNode itemNode in rss.SelectNodes("/rdf:RDF/rss:item", nsmgr)) {
        var positionNode = itemNode.SelectSingleNode("@position", nsmgr);

        if (positionNode != null) {
          var position = int.Parse(positionNode.Value);
          nodes.Add(position, itemNode);
        }
      }

      return new List<XmlNode>(nodes.Values);
    }

    private static List<XmlNode> GetItemNodesFromRdfSeq(XmlNode seqNode, XmlNamespaceManager nsmgr)
    {
      var itemResources = new List<string>();

      foreach (XmlAttribute itemResourceAttr in seqNode.SelectNodes("rdf:li/@rdf:resource", nsmgr)) {
        itemResources.Add(itemResourceAttr.Value);
      }

      if (itemResources.Count == 0) {
        // TODO: resource属性にローカルな名前空間が指定されていた場合
        foreach (XmlAttribute itemResourceAttr in seqNode.SelectNodes("rdf:li/@resource", nsmgr)) {
          itemResources.Add(itemResourceAttr.Value);
        }
      }

      return itemResources.ConvertAll(delegate(string about) {
        return seqNode.OwnerDocument.SelectSingleNode(string.Format("/rdf:RDF/rss:item[@rdf:about='{0}']", about), nsmgr);
      });
    }
  }
}
