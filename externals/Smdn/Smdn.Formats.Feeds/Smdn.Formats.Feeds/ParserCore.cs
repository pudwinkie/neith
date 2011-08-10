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

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  internal abstract class ParserCore {
    protected FeedVersion Version {
      get { return version; }
    }

    internal protected ParserCore(FeedVersion version)
    {
      this.version = version;
    }

    internal protected abstract IFeed Parse(XmlDocument document, bool discardSourceXml, EntryHashAlgorithm hasher);

    protected void ParseModules(FeedBase feed, XmlNode feedNode, XmlNamespaceManager nsmgr)
    {
      foreach (var namespaceUri in ModuleBase.KnowModules.Keys) {
        if (feedNode.SelectSingleNode(string.Format(".//*[namespace-uri()='{0}']", namespaceUri)) == null)
          continue;

        var module = ModuleBase.GetModule(namespaceUri);

        feed.Modules.Add(namespaceUri, module);

        module.Parse(feed, feedNode, nsmgr);
      }
    }

    protected void ParseModules(EntryBase entry, XmlNode entryNode, XmlNamespaceManager nsmgr)
    {
      foreach (var namespaceUri in ModuleBase.KnowModules.Keys) {
        if (entryNode.SelectSingleNode(string.Format(".//*[namespace-uri()='{0}']", namespaceUri)) == null)
          continue;

        var module = ModuleBase.GetModule(namespaceUri);

        entry.Modules.Add(namespaceUri, module);

        module.Parse(entry, entryNode, nsmgr);
      }
    }

    private readonly FeedVersion version;
  }
}