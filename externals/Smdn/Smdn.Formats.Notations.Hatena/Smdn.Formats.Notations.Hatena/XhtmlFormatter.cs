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
using System.IO;
using System.Xml;

using Smdn.Formats.Notations;
using Smdn.Formats.Notations.Dom;
using Smdn.Formats.Notations.Hatena;
using Smdn.Formats.Notations.Hatena.Dom;

using Smdn.Xml;
using Smdn.Xml.Xhtml;

namespace Smdn.Formats.Notations.Hatena {
  public class XhtmlFormatter : Notations.XhtmlFormatter {
    public const string HatenaExtensionNamespace = "http://smdn.jp/works/tools/pakina/notations#hatena"; // XXX

    public XhtmlFormatter(Dictionary<string, string> options)
      : base(options)
    {
    }

    protected override void RegisterPrefixes()
    {
      base.RegisterPrefixes();
      base.NamespaceManager.AddNamespace("hatena", HatenaExtensionNamespace);
    }

    protected override IEnumerable<XmlNode> FormatNode(XhtmlDocument xhtml, Node node)
    {
      if (node is AsciiArt)
        return FormatAsciiArtNode(xhtml, node as AsciiArt);
      else if (node is SyntaxHighlightedCode)
        return FormatSyntaxHighlightedCodeNode(xhtml, node as SyntaxHighlightedCode);
      else
        return base.FormatNode(xhtml, node);
    }

    protected virtual IEnumerable<XmlNode> FormatAsciiArtNode(XhtmlDocument xhtml, AsciiArt aa)
    {
      var div = xhtml.CreateXhtmlElement("div");
      var firstLine = true;

      using (var reader = new StringReader(aa.ToString())) {
        for (;;) {
          var line = reader.ReadLine();

          if (line == null)
            break;

          if (firstLine)
            firstLine = false;
          else
            div.AppendChild(xhtml.CreateXhtmlElement("br"));

          div.AppendChild(xhtml.CreateTextNode(line));
        }
      }

      return new XmlNode[] {div};
    }

    private IEnumerable<XmlNode> FormatSyntaxHighlightedCodeNode(XhtmlDocument xhtml, SyntaxHighlightedCode code)
    {
      var nodes = base.FormatPreformattedNode(xhtml, code);

      XmlUtils.FindElement(nodes, "pre", W3CNamespaces.Xhtml).SetAttribute("hatena:filetype", HatenaExtensionNamespace, code.FileType);

      return nodes;
    }

    protected override IEnumerable<XmlNode> FormatBlockQuotationNode(XhtmlDocument xhtml, BlockQuotation blockquote)
    {
      var nodes = base.FormatBlockQuotationNode(xhtml, blockquote);

      if (blockquote.Cite != null) {
        var bq = XmlUtils.FindElement(nodes, "blockquote", W3CNamespaces.Xhtml);
        var cite = bq.AppendChild(xhtml.CreateXhtmlElement("p")).AppendChild(xhtml.CreateXhtmlElement("cite"));

        cite.AppendChildren(base.FormatAnchorNode(xhtml, new Anchor(blockquote.Cite, new[] {new Text(blockquote.Cite)})));
      }

      return nodes;
    }
  }
}