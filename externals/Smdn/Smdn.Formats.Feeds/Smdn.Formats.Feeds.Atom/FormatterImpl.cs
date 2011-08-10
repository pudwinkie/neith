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

namespace Smdn.Formats.Feeds.Atom {
  internal class FormatterImpl : FormatterCore {
    private delegate void Formatting<T>(XmlElement element, T val);

    internal protected FormatterImpl()
    {
    }

    internal void Format(Feed f, XmlDocument document)
    {
      var nsmgr = new XmlNamespaceManager(document.NameTable);

      try {
        const string atomns = FeedNamespaces.Atom_1_0;

        nsmgr.PushScope();
        nsmgr.AddNamespace(string.Empty, atomns);

        // /feed
        var feed = (XmlElement)document.AppendChild(document.CreateElement("feed", atomns));

        feed.SetAttribute("xmlns", atomns);

        AppendElement    (feed, f.Authors, "author", atomns, false, FormatPerson);
        AppendElement    (feed, f.Categories, "category", atomns, false, FormatCategory);
        AppendElement    (feed, f.Contributors, "contributor", atomns, false, FormatPerson);
        AppendElement    (feed, f.Generator, "generator", atomns, false, FormatGenerator);
        AppendTextElement(feed, ConvertUtils.ToStringNullable(f.Icon), "icon", atomns, false);
        AppendTextElement(feed, ConvertUtils.ToStringNullable(f.Id), "id", atomns, true);
        if (!f.Links.Exists(Link.IsAlternativeLink))
          throw new MandatoryValueMissingException("at least one atom:link element with a rel attribute value of 'alternate' is required");
        AppendElement    (feed, f.Links, "link", atomns, false, FormatLink);
        AppendTextElement(feed, ConvertUtils.ToStringNullable(f.Logo), "logo", atomns, false);
        AppendElement    (feed, f.Rights, "rights", atomns, false, FormatText);
        AppendElement    (feed, f.Subtitle, "subtitle", atomns, false,  FormatText);
        AppendElement    (feed, f.Title, "title", atomns, true,  FormatText);
        AppendTextElement(feed, DateTimeConvert.ToW3CDateTimeStringNullable(f.Updated), "updated", atomns, true);

        // modules
        Dictionary<string, string> moduleNamespaces;

        FormatModule(f, feed, out moduleNamespaces);

        // /feed/entry
        foreach (var e in f.Entries) {
          var entry = (XmlElement)feed.AppendChild(document.CreateElement("entry", atomns));

          //    o  atom:entry elements MUST contain an atom:summary element in either
          //       of the following cases:
          //       *  the atom:entry contains an atom:content that has a "src"
          //          attribute (and is thus empty).
          //       *  the atom:entry contains content that is encoded in Base64;
          //          i.e., the "type" attribute of atom:content is a MIME media type
          //          [MIMEREG], but is not an XML media type [RFC3023], does not
          //          begin with "text/", and does not end with "/xml" or "+xml".
          if (e.Content != null && e.Summary == null) {
            if (e.Content.Src != null)
              throw new MandatoryValueMissingException("if content/@src is set, summary is required");
            if (e.Content.Type != null) {
              if (e.Content.Type.Contains("/")) {
                if (!(e.Content.Type.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                      e.Content.Type.EndsWith("/xml", StringComparison.OrdinalIgnoreCase) ||
                      e.Content.Type.EndsWith("+xml", StringComparison.OrdinalIgnoreCase)))
                  throw new MandatoryValueMissingException("if content/@type is not an XML media type, summary is required");
              }
            }
          }

          if (f.Authors.Count == 0 && e.Authors.Count == 0)
            throw new MandatoryValueMissingException("/feed/author or /feed/entry/author is required");

          AppendElement    (entry, e.Authors, "author", atomns, false, FormatPerson);
          AppendElement    (entry, e.Categories, "category", atomns, false, FormatCategory);
          AppendElement    (entry, e.Content, "content", atomns, false, FormatContent);
          AppendElement    (entry, e.Contributors, "contributor", atomns, false, FormatPerson);
          AppendTextElement(entry, ConvertUtils.ToStringNullable(e.Id), "id", atomns, true);
          if (!e.Links.Exists(Link.IsAlternativeLink))
            throw new MandatoryValueMissingException("at least one atom:link element with a rel attribute value of 'alternate' is required");
          AppendElement    (entry, e.Links, "link", atomns, false, FormatLink);
          AppendTextElement(entry, DateTimeConvert.ToW3CDateTimeStringNullable(e.Published), "published", atomns, false);
          AppendElement    (entry, e.Rights, "rights", atomns, false, FormatText);
          //AppendElement    (entry, e.Source, "source", atomns, false); // TODO: format
          AppendElement    (entry, e.Summary, "summary", atomns, false, FormatText);
          AppendElement    (entry, e.Title, "title", atomns, true, FormatText);
          AppendTextElement(entry, DateTimeConvert.ToW3CDateTimeStringNullable(e.Updated), "updated", atomns, true);

          // modules
          Dictionary<string, string> entryModuleNamespaces;

          FormatModule(e, entry, out entryModuleNamespaces);

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

    private static IEnumerable<XmlElement> AppendElement<T>(XmlElement parent, IEnumerable<T> list, string name, string namespaceUri, bool required, Formatting<T> formatting)
      where T : class
    {
      var elements = new List<XmlElement>();
      var count = 0;

      foreach (var val in list) {
        elements.Add(AppendElement(parent, val, name, namespaceUri, false, formatting));
        count++;
      }

      if (count == 0 && required)
        throw new MandatoryValueMissingException(string.Format("{0} is required element", name));

      return elements;
    }

    private static XmlElement AppendElement<T>(XmlElement parent, T val, string name, string namespaceUri, bool required, Formatting<T> formatting)
      where T : class
    {
      if (val == null) {
        if (required)
          throw new MandatoryValueMissingException(string.Format("{0} is required element", name));
        else
          return null;
      }

      var element = parent.AppendChild(parent.OwnerDocument.CreateElement(name, namespaceUri)) as XmlElement;

      formatting(element, val);

      return element;
    }

    private void AppendText(XmlNode parent, string text)
    {
      parent.AppendChild(parent.OwnerDocument.CreateTextNode(text));
    }

    internal void FormatCategory(XmlElement element, Category category)
    {
      AppendAttribute(element, category.Term, "term", null, true);
      AppendAttribute(element, ConvertUtils.ToStringNullable(category.Scheme), "scheme", null, false);
      AppendAttribute(element, category.Label, "label", null, false);
    }

    internal void FormatContent(XmlElement element, Content content)
    {
      if (content.Type != null &&
          (content.Type.StartsWith("multipart", StringComparison.OrdinalIgnoreCase) ||
           content.Type.StartsWith("message", StringComparison.OrdinalIgnoreCase)))
        throw new FeedFormatException("composite media types are not allowed");

      AppendAttribute(element, content.Type, "type", null, true);

      if (content.Src == null) {
        if (string.Equals(content.Type, "html", StringComparison.OrdinalIgnoreCase))
          element.AppendChild(element.OwnerDocument.CreateCDataSection(content.Value));
        else if (string.Equals(content.Type, "xhtml", StringComparison.OrdinalIgnoreCase))
          AppendXhtmlNodeTree(element, content.Value);
        else
          // Type == null, "text", or else
          AppendText(element, content.Value);
      }
      else {
        if (content.Value != null)
          throw new FeedFormatException("if the Src was set, Value must be null");
        AppendAttribute(element, ConvertUtils.ToStringNullable(content.Src), "src", null, true);
      }
    }

    internal void FormatGenerator(XmlElement element, Generator generator)
    {
      AppendText(element, generator.Value);
      AppendAttribute(element, ConvertUtils.ToStringNullable(generator.Uri), "uri", null, false);
      AppendAttribute(element, generator.Version, "version", null, false);
    }

    internal void FormatLink(XmlElement element, Link link)
    {
      AppendAttribute(element, ConvertUtils.ToStringNullable(link.Href), "href", null, true);
      AppendAttribute(element, link.Rel, "rel", null, false);
      AppendAttribute(element, link.Type == null ? null : link.Type.ToString(), "type", null, false);
      AppendAttribute(element, link.HrefLang, "hreflang", null, false);
      AppendAttribute(element, link.Title, "title", null, false);
      AppendAttribute(element, ConvertUtils.ToStringNullable(link.Length), "length", null, false);
    }

    internal void FormatPerson(XmlElement element, Person person)
    {
      AppendTextElement(element, person.Name, "name", FeedNamespaces.Atom_1_0, true);
      AppendTextElement(element, ConvertUtils.ToStringNullable(person.Uri), "uri", FeedNamespaces.Atom_1_0, false);
      AppendTextElement(element, person.EMail, "email", FeedNamespaces.Atom_1_0, false);
    }

    internal void FormatText(XmlElement element, Text text)
    {
      TextType type;

      if (text.Type == null)
        type = TextType.Text;
      else
        type = text.Type.Value;

      if (type == TextType.Text) {
        AppendText(element, text.Value);
        AppendAttribute(element, "text", "type", null, false);
      }
      else if (type == TextType.Html) {
        element.AppendChild(element.OwnerDocument.CreateCDataSection(text.Value));
        AppendAttribute(element, "html", "type", null, false);
      }
      else {
        AppendAttribute(element, "xhtml", "type", null, false);
        AppendXhtmlNodeTree(element, text.Value);
      }
    }

    private void AppendXhtmlNodeTree(XmlElement element, string xhtml)
    {
      var insertDocument = new XmlDocument();
      var insertNsmgr = new XmlNamespaceManager(insertDocument.NameTable);

      insertNsmgr.AddNamespace(string.Empty, "http://www.w3.org/1999/xhtml");

      var reader = new XmlTextReader(xhtml,
                                      XmlNodeType.Element,
                                      new XmlParserContext(null, insertNsmgr, null, XmlSpace.None));

      insertDocument.Load(reader);

      var imported = element.OwnerDocument.ImportNode(insertDocument.DocumentElement, true);

      if (imported.LocalName != "div") {
        var div = element.OwnerDocument.CreateElement("div", FeedNamespaces.Xhtml);
        div.AppendChild(imported);
        imported = div;
      }

      element.AppendChild(imported);
    }
  }
}