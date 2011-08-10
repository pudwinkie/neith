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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Smdn.Formats.Notations.Dom;

using Smdn.Xml;
using Smdn.Xml.Xhtml;

namespace Smdn.Formats.Notations {
  public abstract class XhtmlFormatter : IFormatter<XhtmlDocument> {
    protected XmlNamespaceManager NamespaceManager {
      get { return nsmgr; }
    }

    private XmlNamespaceManager nsmgr = null;

    protected XhtmlFormatter(Dictionary<string, string> options)
    {
    }

    public XhtmlDocument Format(Document document)
    {
      return Format(document, "body", string.Empty, W3CNamespaces.Xhtml);
    }

    public XhtmlDocument Format(Document document, string documentElement, string prefixDocumentElement, string nsUriDocumentElement)
    {
      var xhtml = new XhtmlDocument();

      try {
        nsmgr = new XmlNamespaceManager(xhtml.NameTable);
        nsmgr.PushScope();

        if (!string.IsNullOrEmpty(prefixDocumentElement) && string.IsNullOrEmpty(nsUriDocumentElement))
          nsmgr.AddNamespace(prefixDocumentElement, nsUriDocumentElement);

        RegisterPrefixes();

        var body = (XmlElement)xhtml.AppendChild(xhtml.CreateElement(documentElement, nsUriDocumentElement));
        var namespaces = new Dictionary<string, XmlAttribute>();

        foreach (string ns in nsmgr) {
          if (string.Empty.Equals(ns) || string.Equals(ns, "xml", StringComparison.Ordinal)) {
            continue;
          }
          else if (string.Equals(ns, "xmlns", StringComparison.Ordinal)) {
            body.SetAttribute("xmlns", W3CNamespaces.Xhtml);
          }
          else {
            var attr = body.Attributes.Append(xhtml.CreateAttribute("xmlns:" + ns));

            attr.Value = nsmgr.LookupNamespace(ns);

            namespaces.Add(attr.Value, attr);
          }
        }

        PreFormatDocument(xhtml, document);

        foreach (var node in document.Nodes) {
          body.AppendChildren(FormatNode(xhtml, node));
        }

        foreach (var ns in namespaces) {
          if (body.SelectSingleNode(string.Format("//*[namespace-uri()='{0}']", ns.Key)) == null)
            body.Attributes.Remove(ns.Value);
        }

        return xhtml;
      }
      finally {
        nsmgr.PopScope();
        nsmgr = null;
      }
    }

    protected virtual void RegisterPrefixes()
    {
      nsmgr.AddNamespace(string.Empty, W3CNamespaces.Xhtml);
    }

    protected virtual void PreFormatDocument(XhtmlDocument xhtml, Document document)
    {
      // do nothing here
    }

    protected virtual IEnumerable<XmlNode> FormatNode(XhtmlDocument xhtml, Node node)
    {
      if (node is Text) {
        return FormatTextNode(xhtml, node as Text);
      }
      else if (node is Preformatted) {
        return FormatPreformattedNode(xhtml, node as Preformatted);
      }
      else if (node is Table) {
        return new[] {FormatTableNode(xhtml, node as Table)};
      }
      else if (node is Container<Node>) {
        var container = node as Container<Node>;
        if (container is ListItem)
          return FormatListNode(xhtml, container as ListItem);
        else if (node is DefinitionListItem)
          return FormatDefinitionListNode(xhtml, node as DefinitionListItem);
        else if (container is Header)
          return FormatHeaderNode(xhtml, container as Header);
        else if (container is BlockQuotation)
          return FormatBlockQuotationNode(xhtml, container as BlockQuotation);
        else if (container is Paragraph)
          return FormatParagraphNode(xhtml, container as Paragraph);
        else if (container is Section)
          return FormatSectionNode(xhtml, container as Section);
        else if (container is Anchor)
          return FormatAnchorNode(xhtml, container as Anchor);
        else if (container is Emphasis)
          return FormatEmphasisNode(xhtml, container as Emphasis);
        else if (container is InsertedText)
          return FormatInsertedTextNode(xhtml, container as InsertedText);
        else if (container is DeletedText)
          return FormatDeletedTextNode(xhtml, container as DeletedText);
        else if (container is Inline)
          return FormatNodes(xhtml, container.Nodes);
        else if (node is Annotation)
          return FormatAnnotationNode(xhtml, node as Annotation);
        else
          return ProcessUnformattedNode(xhtml, container);
      }
      else if (node is HorizontalRule) {
        return new[] {xhtml.CreateXhtmlElement("hr")};
      }
      else if (node is ForcedLineBreak) {
        return new[] {xhtml.CreateXhtmlElement("br")};
      }
      else if (node is EmptyLine) {
        return new XmlNode[] {};
      }
      else if (node is EntityReference) {
        return FormatEntityReferenceNode(xhtml, node as EntityReference);
      }
      else if (node is InlineFrame) {
        return FormatInlineFrameNode(xhtml, node as InlineFrame);
      }
      else if (node is Image) {
        return FormatImageNode(xhtml, node as Image);
      }
      else {
        return ProcessUnformattedNode(xhtml, node);
      }
    }

    protected virtual IEnumerable<XmlNode> ProcessUnformattedNode(XhtmlDocument xhtml, Node node)
    {
      return new XmlNode[] {xhtml.CreateComment(node.ToString())};
    }

    protected virtual IEnumerable<XmlNode> FormatNodes(XhtmlDocument xhtml, IEnumerable<Node> nodes)
    {
      var xmlNodes = new List<XmlNode>();

      foreach (var node in nodes) {
        xmlNodes.AddRange(FormatNode(xhtml, node));
      }

      return xmlNodes;
    }

    private IEnumerable<XmlNode> FormatTextNode(XhtmlDocument xhtml, Text text)
    {
      if (text is Comment)
        return new[] {xhtml.CreateComment(text.Value)};
      else
        return new[] {xhtml.CreateTextNode(text.Value)};
    }

    protected IEnumerable<XmlNode> FormatPreformattedNode(XhtmlDocument xhtml, Preformatted preformatted)
    {
      XmlNode pre;
      XmlNode parent;

      if (preformatted is BlockCode) {
        pre = xhtml.CreateXhtmlPre(true);
        parent = pre.AppendChild(xhtml.CreateXhtmlElement("code"));
      }
      else {
        parent = pre = xhtml.CreateXhtmlPre();
      }

      //if (preformatted.NeedEscape)
        parent.AppendChild(xhtml.CreateTextNode(preformatted.ToString()));
      //else
        //pre.AppendChild(xhtml.ImportNode());
        //throw new NotSupportedException("pre-formatted text block which contains XHTML tags is not supported");

      return new XmlNode[] {pre};
    }

    private IEnumerable<XmlNode> FormatDefinitionListNode(XhtmlDocument xhtml, DefinitionListItem dli)
    {
      if (dli is DefinitionList) {
        return new[] {AppendChildren(xhtml.CreateXhtmlElement("dl"), FormatDefinitionListItemNodes(xhtml, dli.Nodes))};
      }
      else {
        return ProcessUnformattedNode(xhtml, dli);
      }
    }

    private IEnumerable<XmlNode> FormatDefinitionListItemNodes(XhtmlDocument xhtml, IEnumerable<Node> items)
    {
      var nodes = new List<XmlNode>();
      XmlNode lastdd = null;

      foreach (var item in items) {
        if (item is DefinitionList) {
          var dl = FormatDefinitionListNode(xhtml, item as DefinitionList);

          if (lastdd == null)
            nodes.AddRange(dl);
          else
            lastdd.AppendChildren(dl);
        }
        else if (item is DefinitionListItem) {
          var dt = AppendChildren(xhtml.CreateXhtmlElement("dt"), FormatNodes(xhtml, (item as DefinitionListItem).Term));
          var dd = AppendChildren(xhtml.CreateXhtmlElement("dd"), FormatNodes(xhtml, (item as DefinitionListItem).Nodes));

          // cannot omit end tag for dt/dd
          if (dt.ChildNodes.Count == 0)
            dt.AppendChild(xhtml.CreateTextNode(string.Empty));
          if (dd.ChildNodes.Count == 0)
            dd.AppendChild(xhtml.CreateTextNode(string.Empty));

          nodes.Add(dt);
          nodes.Add(dd);

          lastdd = dd;
        }
        else {
          nodes.AddRange(ProcessUnformattedNode(xhtml, item));
        }
      }

      return nodes;
    }

    private IEnumerable<XmlNode> FormatListNode(XhtmlDocument xhtml, ListItem li)
    {
      if (li is OrderedList)
        return new[] {AppendChildren(xhtml.CreateXhtmlElement("ol"), FormatListItemNodes(xhtml, li.Nodes))};
      else if (li is UnorderedList)
        return new[] {AppendChildren(xhtml.CreateXhtmlElement("ul"), FormatListItemNodes(xhtml, li.Nodes))};
      else 
        return ProcessUnformattedNode(xhtml, li);
    }

    private IEnumerable<XmlNode> FormatListItemNodes(XhtmlDocument xhtml, IEnumerable<Node> items)
    {
      var nodes = new List<XmlNode>();

      foreach (var item in items) {
        if (item is ListItem)
          nodes.Add(AppendChildren(xhtml.CreateXhtmlElement("li"), FormatNodes(xhtml, (item as ListItem).Nodes)));
        else
          nodes.AddRange(ProcessUnformattedNode(xhtml, item));
      }

      return nodes;
    }

    private IEnumerable<XmlNode> FormatHeaderNode(XhtmlDocument xhtml, Header h)
    {
      return new[] {AppendChildren(xhtml.CreateXhtmlElement(string.Format("h{0}", h.Level)), FormatNodes(xhtml, h.Nodes))};
    }

    protected virtual IEnumerable<XmlNode> FormatBlockQuotationNode(XhtmlDocument xhtml, BlockQuotation blockquote)
    {
      var bq = xhtml.CreateXhtmlElement("blockquote");

      if (blockquote.Cite != null)
        bq.SetAttribute("cite", blockquote.Cite);

      bq.AppendChild(xhtml.CreateXhtmlElement("div")).AppendChildren(FormatNodes(xhtml, blockquote.Nodes));

      return new[] {bq};
    }

    private IEnumerable<XmlNode> FormatParagraphNode(XhtmlDocument xhtml, Paragraph p)
    {
      return new[] {AppendChildren(xhtml.CreateXhtmlElement("p"), FormatNodes(xhtml, p.Nodes))};
    }

    protected virtual IEnumerable<XmlNode> FormatSectionNode(XhtmlDocument xhtml, Section section)
    {
      return new[] {AppendChildren(xhtml.CreateXhtmlElement("div"), FormatNodes(xhtml, section.Nodes))};
    }

    protected IEnumerable<XmlNode> FormatAnchorNode(XhtmlDocument xhtml, Anchor a)
    {
      return new[] {AppendChildren(xhtml.CreateXhtmlAnchor(Uri.EscapeUriString(a.Href), a.Id, a.Title, null), FormatNodes(xhtml, a.Nodes))};
    }

    private IEnumerable<XmlNode> FormatEmphasisNode(XhtmlDocument xhtml, Emphasis em)
    {
      if (em is StrongEmphasis)
        return new[] {AppendChildren(xhtml.CreateXhtmlElement("strong"), FormatNodes(xhtml, em.Nodes))};
      else
        return new[] {AppendChildren(xhtml.CreateXhtmlElement("em"), FormatNodes(xhtml, em.Nodes))};
    }

    private IEnumerable<XmlNode> FormatInsertedTextNode(XhtmlDocument xhtml, InsertedText ins)
    {
      return new[] {AppendChildren(xhtml.CreateXhtmlElement("ins"), FormatNodes(xhtml, ins.Nodes))};
    }

    private IEnumerable<XmlNode> FormatDeletedTextNode(XhtmlDocument xhtml, DeletedText del)
    {
      return new XmlNode[] {AppendChildren(xhtml.CreateXhtmlElement("del"), FormatNodes(xhtml, del.Nodes))};
    }

    private XmlNode FormatTableNode(XhtmlDocument xhtml, Table table)
    {
      var tbl = xhtml.CreateXhtmlElement("table");

      tbl.AppendChildren(FormatTableItemNodes(xhtml, table.Nodes));

      if (table.Summary != null)
        tbl.SetAttribute("summary", table.Summary);

      return tbl;
    }

    private IEnumerable<XmlNode> FormatTableItemNodes(XhtmlDocument xhtml, IEnumerable<TableItem> items)
    {
      var nodes = new List<XmlNode>();

      // check table format
      TableHeaderRow theadRow = null;
      TableFooterRow tfootRow = null;
      TableCaption caption = null;
      var tbodyRows = new List<TableRow>();

      foreach (var item in items) {
        if (item is TableHeaderRow) {
          if (theadRow != null)
            throw new InvalidOperationException("thead should appear exactly one");
          theadRow = item as TableHeaderRow;
        }
        else if (item is TableFooterRow) {
          if (tfootRow != null)
            throw new InvalidOperationException("tfoot should appear exactly one");
          tfootRow = item as TableFooterRow;
        }
        else if (item is TableCaption) {
          if (caption != null)
            throw new InvalidOperationException("caption should appear exactly one");
          caption = item as TableCaption;
        }
        else if (item is TableRow) {
          tbodyRows.Add(item as TableRow);
        }
        else {
          throw new InvalidOperationException("can't contain non TableItem types");
        }
      }


      // merge colspan
      var theadCols = theadRow == null ? null : FormatTableColspan(theadRow.Nodes.ToArray());
      var tfootCols = tfootRow == null ? null : FormatTableColspan(tfootRow.Nodes.ToArray());

      // caption, thead, tfoot, tbodyの順にノードを追加する
      if (caption != null)
        nodes.Add(AppendChildren(xhtml.CreateXhtmlElement("caption"), FormatNodes(xhtml, caption.Nodes)));

      if (theadCols != null)
        nodes.Add(AppendChildren(xhtml.CreateXhtmlElement("thead"), new[] {AppendChildren(xhtml.CreateXhtmlElement("tr"), FormatTableColumnNodes(xhtml, theadCols))}));

      if (tfootCols != null)
        nodes.Add(AppendChildren(xhtml.CreateXhtmlElement("tfoot"), new[] {AppendChildren(xhtml.CreateXhtmlElement("tr"), FormatTableColumnNodes(xhtml, tfootCols))}));

      if (0 < tbodyRows.Count) {
        var tbody = xhtml.CreateXhtmlElement("tbody");

        foreach (var row in tbodyRows) {
          var tbodyCols = FormatTableColspan(row.Nodes.ToArray());

          tbody.AppendChild(xhtml.CreateXhtmlElement("tr")).AppendChildren(FormatTableColumnNodes(xhtml, tbodyCols));
        }

        nodes.Add(tbody);
      }

      return nodes;
    }

    private TableColumn[] FormatTableColspan(Node[] columns)
    {
      var cols = new TableColumn[columns.Length];

      // convert types
      for (var i = 0; i < columns.Length; i++) {
        if (!(columns[i] is TableColumn))
          throw new InvalidOperationException("can't contain non TableColumn types");
        cols[i] = columns[i] as TableColumn;
      }

      // format
      for (var i = 0; i < cols.Length; i++) {
        if (cols[i] == null)
          continue;

        if (0 < cols[i].ColSpan) {
          var span = cols[i].ColSpan;

          if (span == 1) {
            for (var j = i + 1; j < cols.Length; j++) {
              cols[i].ColSpan++;

              if (cols[j].ColSpan == 1) {
                cols[j] = null;
                continue;
              }
              else {
                cols[j].ColSpan = cols[i].ColSpan;
                cols[i] = cols[j];
                cols[j] = null;
                break;
              }
            }
          }
          else {
            throw new NotImplementedException();
          }
        }
        else if (cols[i].ColSpan < 0) {
          throw new NotImplementedException();
        }
      }

      return cols;
    }

    protected virtual IEnumerable<XmlNode> FormatTableColumnNodes(XhtmlDocument xhtml, IEnumerable<Node> columns)
    {
      var nodes = new List<XmlNode>();

      foreach (var column in columns) {
        if (column is TableColumn) {
          var col = column as TableColumn;
          var node = xhtml.CreateXhtmlElement(col is TableHeaderColumn ? "th" : "td");

          node.AppendChildren(FormatNodes(xhtml, col.Nodes));

          if (1 < col.RowSpan)
            node.SetAttribute("rowspan", col.RowSpan.ToString());
          if (1 < col.ColSpan)
            node.SetAttribute("colspan", col.ColSpan.ToString());
          if (!string.IsNullOrEmpty(col.Alignment))
            node.SetAttribute("align", col.Alignment);
          if (!string.IsNullOrEmpty(col.Style))
            node.SetAttribute("style", col.Style);

          nodes.Add(node);
        }
        else if (column != null) {
          nodes.AddRange(ProcessUnformattedNode(xhtml, column));
        }
      }

      return nodes;
    }

    protected virtual IEnumerable<XmlNode> FormatAnnotationNode(XhtmlDocument xhtml, Annotation a)
    {
      return new[] {AppendChildren(xhtml.CreateXhtmlElement("em"), FormatNodes(xhtml, a.Nodes))};
    }

    protected virtual IEnumerable<XmlNode> FormatEntityReferenceNode(XhtmlDocument xhtml, EntityReference entity)
    {
      return new[] {xhtml.CreateEntityReference(entity.Name)};
    }

    protected virtual IEnumerable<XmlNode> FormatInlineFrameNode(XhtmlDocument xhtml, InlineFrame iframe)
    {
      var node = xhtml.CreateXhtmlElement("iframe");

      node.SetAttribute("src", Uri.EscapeUriString(iframe.SourceUri.ToString()));
      node.SetAttribute("scrolling", iframe.Scrolling ? "yes" : "no");
      node.SetAttribute("frameborder", iframe.FrameBorder ? "1" : "0");

      if (iframe.Width != null)
        node.SetAttribute("width", iframe.Width);
      if (iframe.Height != null)
        node.SetAttribute("height", iframe.Height);

      foreach (var attr in iframe.Attributes) {
        node.SetAttribute(attr.Key, attr.Value);
      }

      return new[] {node};
    }

    protected virtual IEnumerable<XmlNode> FormatImageNode(XhtmlDocument xhtml, Image img)
    {
      return new[] {xhtml.CreateXhtmlImage(Uri.EscapeUriString(img.Source), img.AlternativeText, img.Title, img.Width, img.Height)};
    }

    private static XmlNode AppendChildren(XmlNode node, IEnumerable<XmlNode> newChildren)
    {
      node.AppendChildren(newChildren);

      return node;
    }
  }
}
