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

namespace Smdn.Formats.Feeds.Rss {
  internal class FormatterImpl : FormatterCore {
    private delegate void Formatting<T>(XmlElement element, T val);

    internal protected FormatterImpl()
    {
    }

    internal void Format(Channel c, XmlDocument document)
    {
      var nsmgr = new XmlNamespaceManager(document.NameTable);

      try {
        nsmgr.PushScope();

        // /rss
        var rss = (XmlElement)document.AppendChild(document.CreateElement("rss"));

        AppendAttribute(rss, "2.0", "version", null, true);

        // /rss/channel
        var channel = (XmlElement)rss.AppendChild(document.CreateElement("channel"));

        // Required channel elements
        AppendTextElement(channel, c.Title, "title", null, true);
        AppendTextElement(channel, ConvertUtils.ToStringNullable(c.Link), "link", null, true);
        AppendTextElement(channel, c.Description, "description", null, true);

        // Optional channel elements
        AppendTextElement(channel, c.Language, "language", null, false);
        AppendTextElement(channel, c.Copyright, "copyright", null, false);
        AppendTextElement(channel, c.ManagingEditor, "managingEditor", null, false);
        AppendTextElement(channel, c.WebMaster, "webMaster", null, false);
        AppendTextElement(channel, DateTimeConvert.ToRFC822DateTimeStringNullable(c.PubDate), "pubDate", null, false);
        AppendTextElement(channel, DateTimeConvert.ToRFC822DateTimeStringNullable(c.LastBuildDate), "lastBuildDate", null, false);
        foreach (var category in c.Categories) {
          AppendElement(channel, category, "category", FormatCategory);
        }
        AppendTextElement(channel, c.Generator, "generator", null, false);
        AppendTextElement(channel, ConvertUtils.ToStringNullable(c.Docs), "docs", null, false);
        AppendElement    (channel, c.Cloud, "cloud", FormatCloud);
        AppendTextElement(channel, ConvertUtils.ToStringNullable(c.Ttl), "ttl", null, false);
        AppendElement    (channel, c.Image, "image", FormatImage);
        AppendTextElement(channel, c.Rating, "rating", null, false);
        AppendElement    (channel, c.TextInput, "textInput", FormatTextInput);

        if (0 < c.SkipHours.Count) {
          if (24 < c.SkipHours.Count)
            throw new FeedFormatException("too many skip hours");
          var skipHours = channel.AppendChild(document.CreateElement("skipHours"));
          foreach (var skipHour in c.SkipHours) {
            AppendTextElement(skipHours, skipHour.ToString(), "hour", null, false);
          }
        }

        if (0 < c.SkipDays.Count) {
          if (7 < c.SkipDays.Count)
            throw new FeedFormatException("too many skip days");
          var skipDays = channel.AppendChild(document.CreateElement("skipDays"));
          foreach (var skipDay in c.SkipDays) {
            AppendTextElement(skipDays, skipDay.ToString(), "day", null, false);
          }
        }

        // modules
        Dictionary<string, string> moduleNamespaces;

        FormatModule(c, channel, out moduleNamespaces);

        // /rss/channel/item
        foreach (var i in c.Items) {
          var item = (XmlElement)channel.AppendChild(document.CreateElement("item"));

          // Required item elements
          if (i.Title == null && i.Description == null)
            throw new MandatoryValueMissingException("Title or Description is required");

          AppendTextElement(item, i.Title, "title", null, false);
          AppendTextElement(item, i.Description, "description", null, false);

          // Optional item elements
          AppendTextElement(item, ConvertUtils.ToStringNullable(i.Link), "link", null, false);
          AppendTextElement(item, DateTimeConvert.ToRFC822DateTimeStringNullable(i.PubDate), "pubDate", null, false);
          AppendTextElement(item, i.Author, "author", null, false);
          foreach (var category in i.Categories) {
            AppendElement(item, category, "category", FormatCategory);
          }
          AppendTextElement(item, ConvertUtils.ToStringNullable(i.Comments), "comments", null, false);
          AppendElement    (item, i.Enclosure, "enclosure", FormatEnclosure);
          AppendElement    (item, i.Guid, "guid", FormatGuid);
          AppendElement    (item, i.Source, "source", FormatSource);

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
          moduleNamespaces.Add(FeedNamespaces.Rdf, FeedPrefixes.Rdf);
          foreach (var pair in moduleNamespaces) {
            document.DocumentElement.SetAttribute("xmlns:" + pair.Value, pair.Key);
          }
        }
      }
      finally {
        nsmgr.PopScope();
      }
    }

    private void AppendText(XmlNode parent, string text)
    {
      parent.AppendChild(parent.OwnerDocument.CreateTextNode(text));
    }

    private void AppendElement<T>(XmlElement parent, T val, string name, Formatting<T> formatting)
    {
      if (val == null)
        return;

      formatting((XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement(name)), val);
    }

    private void FormatCategory(XmlElement element, Category category)
    {
      AppendText(element, category.Value);
      AppendAttribute(element, ConvertUtils.ToStringNullable(category.Domain), "domain", null, false);
    }

    private void FormatEnclosure(XmlElement element, Enclosure enclosure)
    {
      AppendAttribute(element, ConvertUtils.ToStringNullable(enclosure.Url), "url", null, true);
      AppendAttribute(element, ConvertUtils.ToStringNullable(enclosure.Length), "length", null, true);
      AppendAttribute(element, enclosure.Type, "type", null, true);
    }

    private void FormatGuid(XmlElement element, Guid guid)
    {
      AppendText(element, guid.Value);
      AppendAttribute(element, ConvertUtils.ToStringNullable(guid.IsPermaLink), "isPermaLink", null, false);
    }

    private void FormatSource(XmlElement element, Source source)
    {
      AppendText(element, source.Value);
      AppendAttribute(element, ConvertUtils.ToStringNullable(source.Url), "url", null, true);
    }

    private void FormatCloud(XmlElement element, Cloud cloud)
    {
      // TODO: impl
    }

    private void FormatImage(XmlElement element, Image image)
    {
      AppendTextElement(element, ConvertUtils.ToStringNullable(image.Url), "url", null, true);
      AppendTextElement(element, image.Title, "title", null, true);
      AppendTextElement(element, ConvertUtils.ToStringNullable(image.Link), "link", null, true);
      AppendTextElement(element, image.Description, "description", null, true);
      AppendTextElement(element, ConvertUtils.ToStringNullable(image.Width), "width", null, false);
      AppendTextElement(element, ConvertUtils.ToStringNullable(image.Height), "height", null, false);
    }

    private void FormatTextInput(XmlElement element, TextInput ti)
    {
      AppendTextElement(element, ti.Title, "title", null, true);
      AppendTextElement(element, ti.Description, "description", null, true);
      AppendTextElement(element, ti.Name, "name", null, true);
      AppendTextElement(element, ConvertUtils.ToStringNullable(ti.Link), "link", null, true);
    }
  }
}