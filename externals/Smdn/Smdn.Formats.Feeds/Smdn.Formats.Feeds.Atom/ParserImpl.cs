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

namespace Smdn.Formats.Feeds.Atom {
  internal class ParserImpl : ParserCore {
    private delegate T Parsing<T>(XmlNode node, XmlNamespaceManager nsmgr);

    internal protected ParserImpl(FeedVersion version)
      : base(version)
    {
    }

    internal protected override IFeed Parse(XmlDocument atom, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      var nsmgr = new XmlNamespaceManager(atom.NameTable);

      try {
        nsmgr.PushScope();

        if (Version == FeedVersion.Atom10)
          nsmgr.AddNamespace("atom", FeedNamespaces.Atom_1_0);
        else if (Version == FeedVersion.Atom03)
          nsmgr.AddNamespace("atom", FeedNamespaces.Atom_0_3);
        else
          throw new VersionNotSupportedException("unsupported atom version");

        var feedNode = atom.SelectSingleNode("/atom:feed", nsmgr);

        // parse /atom:feed/atom:entry
        var feed = new Feed(feedNode.ConvertNodesTo<Entry>("atom:entry", nsmgr, delegate(XmlNode entryNode) {
          var entry = new Entry();

          if (!discardSourceXml)
            entry.SourceNode = entryNode;

          entry.Authors.AddRange     (entryNode.ConvertNodesTo<Person>("atom:author", nsmgr, ParsePerson));
          entry.Categories.AddRange  (entryNode.ConvertNodesTo<Category>("atom:category", nsmgr, ParseCategory));
          entry.Content             = entryNode.ConvertSingleNodeTo<Content>("atom:content", nsmgr,ParseContent);
          entry.Contributors.AddRange(entryNode.ConvertNodesTo<Person>("atom:contributor", nsmgr, ParsePerson));
          entry.Id                  = entryNode.GetSingleNodeValueOf<Uri>("atom:id/text()", nsmgr, ConvertUtils.ToUriNullable);
          entry.Links.AddRange       (entryNode.ConvertNodesTo<Link>("atom:link", nsmgr, ParseLink));
          entry.Published           = entryNode.GetSingleNodeValueOf<DateTimeOffset?>("atom:published/text()", nsmgr, null, DateTimeConvert.FromW3CDateTimeOffsetStringNullable);
          entry.Rights              = entryNode.GetSingleNodeValueOf("atom:rights/text()", nsmgr);
          entry.Source              = null; // TODO: parse
          entry.Summary             = entryNode.ConvertSingleNodeTo<Text>("atom:summary", nsmgr, ParseText);
          entry.Title               = entryNode.ConvertSingleNodeTo<Text>("atom:title", nsmgr, ParseText);
          entry.Updated             = entryNode.GetSingleNodeValueOf<DateTimeOffset?>("atom:updated/text()", nsmgr, null, DateTimeConvert.FromW3CDateTimeOffsetStringNullable);

          ParseModules(entry, entryNode, nsmgr);

          if (hasher != null) {
            hasher.Initialize();

            entry.Hash = hasher.ComputeHash(entry, entryNode);
          }

          return entry;
        }));

        // parse /atom:feed
        if (!discardSourceXml)
          feed.SourceNode = feedNode;

        feed.Authors.AddRange        (feedNode.ConvertNodesTo<Person>("atom:author", nsmgr, ParsePerson));
        feed.Categories.AddRange     (feedNode.ConvertNodesTo<Category>("atom:category", nsmgr, ParseCategory));
        feed.Contributors.AddRange   (feedNode.ConvertNodesTo<Person>("atom:contributor", nsmgr, ParsePerson));
        feed.Generator              = feedNode.ConvertSingleNodeTo<Generator>("atom:generator", nsmgr, ParseGenerator);
        feed.Icon                   = feedNode.GetSingleNodeValueOf<Uri>("atom:icon/text()", nsmgr, ConvertUtils.ToUriNullable);
        feed.Id                     = feedNode.GetSingleNodeValueOf<Uri>("atom:id/text()", nsmgr, ConvertUtils.ToUriNullable);
        feed.Links.AddRange          (feedNode.ConvertNodesTo<Link>("atom:link", nsmgr, ParseLink));
        feed.Logo                   = feedNode.GetSingleNodeValueOf<Uri>("atom:id/text()", nsmgr, ConvertUtils.ToUriNullable);
        feed.Rights                 = feedNode.GetSingleNodeValueOf("atom:rights/text()", nsmgr);
        feed.Subtitle               = feedNode.ConvertSingleNodeTo<Text>("atom:subtitle", nsmgr, ParseText);
        feed.Title                  = feedNode.ConvertSingleNodeTo<Text>("atom:title", nsmgr, ParseText);
        feed.Updated                = feedNode.GetSingleNodeValueOf<DateTimeOffset?>("atom:updated/text()", nsmgr, null, DateTimeConvert.FromW3CDateTimeOffsetStringNullable);

        ParseModules(feed, feedNode, nsmgr);

        return feed;
      }
      finally {
        nsmgr.PopScope();
      }
    }

    private static Category ParseCategory(XmlNode node, XmlNamespaceManager nsmgr)
    {
      if (node == null)
        return null;

      return new Category(node.GetSingleNodeValueOf("@term", nsmgr),
                          node.GetSingleNodeValueOf<Uri>("@scheme", nsmgr, ConvertUtils.ToUriNullable),
                          node.GetSingleNodeValueOf("@label", nsmgr));
    }

    private static Content ParseContent(XmlNode node, XmlNamespaceManager nsmgr)
    {
      if (node == null)
        return null;

      var type = node.GetSingleNodeValueOf("@type", nsmgr);
      var src =  node.GetSingleNodeValueOf<Uri>("@src", nsmgr, ConvertUtils.ToUriNullable);

      if (type != null &&
          (string.Equals(type, "text", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(type, "html", StringComparison.OrdinalIgnoreCase)))
        return new Content(node.InnerText, type, src);
      else
        return new Content(node.InnerXml, type, src);
    }

    private static Generator ParseGenerator(XmlNode node, XmlNamespaceManager nsmgr)
    {
      if (node == null)
        return null;

      return new Generator(node.InnerText,
                           node.GetSingleNodeValueOf<Uri>("@uri", nsmgr, ConvertUtils.ToUriNullable),
                           node.GetSingleNodeValueOf("@version", nsmgr));
    }

    private static Link ParseLink(XmlNode node, XmlNamespaceManager nsmgr)
    {
      if (node == null)
        return null;

      return new Link(node.GetSingleNodeValueOf<Uri>("@href", nsmgr, ConvertUtils.ToUriNullable),
                      node.GetSingleNodeValueOf("@rel", nsmgr),
                      node.GetSingleNodeValueOf<MimeType>("@type", nsmgr, ToMimeTypeIgnoreException),
                      node.GetSingleNodeValueOf("@hreflang", nsmgr),
                      node.GetSingleNodeValueOf("@title", nsmgr),
                      node.GetSingleNodeValueOf<int?>("@length", nsmgr, null, ConvertUtils.ToInt32Nullable));
    }

    private static Person ParsePerson(XmlNode node, XmlNamespaceManager nsmgr)
    {
      if (node == null)
        return null;

      return new Person(node.GetSingleNodeValueOf("atom:name/text()", nsmgr),
                        node.GetSingleNodeValueOf<Uri>("atom:uri/text()", nsmgr, ConvertUtils.ToUriNullable),
                        node.GetSingleNodeValueOf("atom:email/text()", nsmgr));
    }

    private static Text ParseText(XmlNode node, XmlNamespaceManager nsmgr)
    {
      if (node == null)
        return null;

      var type = node.GetSingleNodeValueOf<TextType?>("@type", nsmgr, ConvertUtils.ToEnumNullable<TextType>);

      if (type != null && type.Value == TextType.Xhtml)
        return new Text(node.InnerXml, type);
      else
        return new Text(node.InnerText, type);
    }

    private static MimeType ToMimeTypeIgnoreException(string s)
    {
      if (s == null)
        return null;

      try { return new MimeType(s); }
      catch { return null; }
    }
  }
}
