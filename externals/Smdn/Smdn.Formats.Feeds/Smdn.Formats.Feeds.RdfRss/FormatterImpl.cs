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

namespace Smdn.Formats.Feeds.RdfRss {
  internal class FormatterImpl : FormatterCore {
    internal protected FormatterImpl()
    {
    }

    internal void Format(Channel c, XmlDocument document)
    {
      var nsmgr = new XmlNamespaceManager(document.NameTable);

      try {
        const string rssns = FeedNamespaces.Rss_1_0;
        const string rdfns = FeedNamespaces.Rdf;

        nsmgr.PushScope();
        nsmgr.AddNamespace(string.Empty, rssns);
        nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

        // /rdf:RDF
        var rdf = (XmlElement)document.AppendChild(document.CreateElement("rdf:RDF", rdfns));

        rdf.SetAttribute("xmlns", rssns);
        rdf.SetAttribute("xmlns:rdf", rdfns);

        // /rdf:RDF/rss:channel
        var channel = (XmlElement)rdf.AppendChild(document.CreateElement("channel", rssns));

        // Required channel elements
        AppendAttribute  (channel, ConvertUtils.ToStringNullable(c.Uri), "rdf:about", rdfns, true);
        AppendTextElement(channel, c.Title, "title", rssns, true);
        AppendTextElement(channel, ConvertUtils.ToStringNullable(c.Link), "link", rssns, true);
        AppendTextElement(channel, c.Description, "description", rssns, true);

        // modules
        Dictionary<string, string> moduleNamespaces;

        FormatModule(c, channel, out moduleNamespaces);

        // /rdf:RDF/rss:channel/rss:items
        var items = channel.AppendChild(document.CreateElement("items", rssns));
        var seq = items.AppendChild(document.CreateElement("rdf:Seq", rdfns));

        foreach (var i in c.Items) {
          // /rdf:RDF/rss:channel/rss:items/rdf:Seq/rdf:li
          var li = (XmlElement)seq.AppendChild(document.CreateElement("rdf:li", rdfns));

          // /rdf:RDF/rss:item
          var item = (XmlElement)rdf.AppendChild(document.CreateElement("item", rssns));

          AppendAttribute(li,   ConvertUtils.ToStringNullable(i.Resource), "rdf:resource", rdfns, true);
          AppendAttribute(item, ConvertUtils.ToStringNullable(i.Resource), "rdf:about", rdfns, true);

          AppendTextElement(item, i.Title, "title", rssns, true);
          AppendTextElement(item, ConvertUtils.ToStringNullable(i.Link), "link", rssns, true);
          AppendTextElement(item, i.Description, "description", rssns, true);

          // modules
          Dictionary<string, string> entryModuleNamespaces;

          FormatModule(i, item, out entryModuleNamespaces);

          foreach (var pair in entryModuleNamespaces) {
            if (!moduleNamespaces.ContainsKey(pair.Key))
              moduleNamespaces.Add(pair.Key, pair.Value);
          }
        }

        // module namespaces
        if (0 < moduleNamespaces.Count) {
          foreach (var pair in moduleNamespaces) {
            document.DocumentElement.SetAttribute("xmlns:" + pair.Value, pair.Key);
          }
        }
      }
      finally {
        nsmgr.PopScope();
      }
    }
  }
}