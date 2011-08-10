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

namespace Smdn.Formats.Feeds.Rss {
  internal class ParserImpl : ParserCore {
    internal protected ParserImpl(FeedVersion version)
      : base(version)
    {
    }

    internal protected override IFeed Parse(XmlDocument rss, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      var nsmgr = new XmlNamespaceManager(rss.NameTable);
      var channelNode = rss.SelectSingleNode("/rss/channel");

      // parse /rss/channel/item
      var channel = new Channel(channelNode.ConvertNodesTo("item", delegate(XmlNode itemNode) {
        var item = new Item();

        if (!discardSourceXml)
          item.SourceNode = itemNode;

        item.Title              = itemNode.GetSingleNodeValueOf("title/text()");
        item.Link               = itemNode.GetSingleNodeValueOf<Uri>("link/text()", ConvertUtils.ToUriNullable);
        item.Description        = itemNode.GetSingleNodeValueOf("description/text()");
        item.Categories.AddRange (itemNode.ConvertNodesTo<Category>("category", ParseCategory));
        item.Comments           = itemNode.GetSingleNodeValueOf<Uri>("comments/text()", ConvertUtils.ToUriNullable);
        item.Enclosure          = itemNode.ConvertSingleNodeTo<Enclosure>("enclosure", ParseEnclosure);
        item.Guid               = itemNode.ConvertSingleNodeTo<Guid>("guid", ParseGuid);
        item.Author             = itemNode.GetSingleNodeValueOf("author/text()");
        item.PubDate            = itemNode.GetSingleNodeValueOf<DateTimeOffset?>("pubDate/text()", null, null, DateTimeConvert.FromRFC822DateTimeOffsetStringNullable);
        item.Source             = itemNode.ConvertSingleNodeTo<Source>("source", ParseSource);

        ParseModules(item, itemNode, nsmgr);

        if (hasher != null) {
          hasher.Initialize();

          item.Hash = hasher.ComputeHash(item, itemNode);
        }

        return item;
      }));

      // parse /rss/channel
      if (!discardSourceXml)
        channel.SourceNode = channelNode;

      // Required channel elements
      channel.Title            = channelNode.GetSingleNodeValueOf("title/text()");
      channel.Link             = channelNode.GetSingleNodeValueOf<Uri>("link/text()", ConvertUtils.ToUriNullable);
      channel.Description      = channelNode.GetSingleNodeValueOf("description/text()");

      // Optional channel elements
      channel.Language          = channelNode.GetSingleNodeValueOf("language/text()");
      channel.Copyright         = channelNode.GetSingleNodeValueOf("copyright/text()");
      channel.ManagingEditor    = channelNode.GetSingleNodeValueOf("managingEditor/text()");
      channel.WebMaster         = channelNode.GetSingleNodeValueOf("webMaster/text()");
      channel.PubDate           = channelNode.GetSingleNodeValueOf<DateTimeOffset?>("pubDate/text()", null, null, DateTimeConvert.FromRFC822DateTimeOffsetStringNullable);
      channel.LastBuildDate     = channelNode.GetSingleNodeValueOf<DateTimeOffset?>("lastBuildDate/text()", null, null, DateTimeConvert.FromRFC822DateTimeOffsetStringNullable);
      channel.Categories.AddRange(channelNode.ConvertNodesTo<Category>("category", ParseCategory));
      channel.Generator         = channelNode.GetSingleNodeValueOf("generator/text()");
      channel.Docs              = channelNode.GetSingleNodeValueOf<Uri>("docs/text()", ConvertUtils.ToUriNullable);
      channel.Cloud             = channelNode.ConvertSingleNodeTo<Cloud>("cloud", ParseCloud);
      channel.Ttl               = channelNode.GetSingleNodeValueOf<int?>("ttl/text()", ConvertUtils.ToInt32Nullable);
      channel.Image             = channelNode.ConvertSingleNodeTo<Image>("image", ParseImage);
      channel.Rating            = channelNode.GetSingleNodeValueOf("rating/text()");
      channel.TextInput         = channelNode.ConvertSingleNodeTo<TextInput>("textInput", ParseTextInput);
      channel.SkipHours.AddRange (channelNode.ConvertNodesTo("skipHours/hour", delegate(XmlNode node) {
        return int.Parse(node.InnerText);
      }));
      channel.SkipDays.AddRange  (channelNode.ConvertNodesTo("skipDays/day", delegate(XmlNode node) {
        return EnumUtils.Parse<DayOfWeek>(node.InnerText);
      }));

      ParseModules(channel, channelNode, nsmgr);

      return channel;
    }

    private static Category ParseCategory(XmlNode node)
    {
      if (node == null)
        return null;

      return new Category(node.InnerText,
                          node.GetSingleNodeValueOf<Uri>("@domain", ConvertUtils.ToUriNullable));
    }

    private static Enclosure ParseEnclosure(XmlNode node)
    {
      if (node == null)
        return null;

      return new Enclosure(node.GetSingleNodeValueOf<Uri>("@url", ConvertUtils.ToUriNullable),
                           node.GetSingleNodeValueOf<int?>("@length", null, null, ConvertUtils.ToInt32Nullable),
                           node.GetSingleNodeValueOf("@type", null));
    }

    private static Guid ParseGuid(XmlNode node)
    {
      if (node == null)
        return null;

      return new Guid(node.InnerText,
                      node.GetSingleNodeValueOf<bool?>("@isPermaLink", null, null, ConvertUtils.ToBooleanNullable));
    }

    private Source ParseSource(XmlNode node)
    {
      if (node == null)
        return null;

      return new Source(node.InnerText,
                        node.GetSingleNodeValueOf<Uri>("@url", ConvertUtils.ToUriNullable));
    }

    private static Cloud ParseCloud(XmlNode node)
    {
      // TODO: impl
      return null;
    }

    private static Image ParseImage(XmlNode node)
    {
      if (node == null)
        return null;

      return new Image(node.GetSingleNodeValueOf<Uri>("url/text()", ConvertUtils.ToUriNullable),
                       node.GetSingleNodeValueOf("title/text()", null),
                       node.GetSingleNodeValueOf<Uri>("link/text()", ConvertUtils.ToUriNullable),
                       node.GetSingleNodeValueOf("description/text()", null),
                       node.GetSingleNodeValueOf<int?>("width/text()", null, null, ConvertUtils.ToInt32Nullable),
                       node.GetSingleNodeValueOf<int?>("height/text()", null, null, ConvertUtils.ToInt32Nullable));
    }

    private static TextInput ParseTextInput(XmlNode node)
    {
      if (node == null)
        return null;

      return new TextInput(node.GetSingleNodeValueOf("title/text()", null),
                           node.GetSingleNodeValueOf("description/text()", null),
                           node.GetSingleNodeValueOf("name/text()", null),
                           node.GetSingleNodeValueOf<Uri>("link/text()", ConvertUtils.ToUriNullable));
    }
  }
}