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
using Smdn.Formats.Notations.PukiWiki;
using Smdn.Formats.Notations.PukiWiki.Dom;

using Smdn.Xml;
using Smdn.Xml.Xhtml;

namespace Smdn.Formats.Notations.PukiWiki {
  public class XhtmlFormatter : Notations.XhtmlFormatter {
    public const string PukiWikiExtensionNamespace = "http://smdn.jp/works/tools/pakina/notations#pukiwiki"; // XXX

    public XhtmlFormatter(Dictionary<string, string> options)
      : base(options)
    {
    }

    protected override void RegisterPrefixes()
    {
      base.RegisterPrefixes();
      base.NamespaceManager.AddNamespace("pukiwiki", PukiWikiExtensionNamespace);
    }

    protected override IEnumerable<XmlNode> FormatNode(XhtmlDocument xhtml, Node node)
    {
      if (node is Plugin)
        return FormatPluginNode(xhtml, node as Plugin);
      else if (node is AlignedParagraph)
        return FormatAlignedParagraphNode(xhtml, node as AlignedParagraph);
      else if (node is Alias)
        return FormatAliasNode(xhtml, node as Alias);
      else if (node is HeaderAnchor)
        return FormatHeaderAnchor(xhtml, node as HeaderAnchor);
      else
        return base.FormatNode(xhtml, node);
    }

    private readonly string[] refParams = new[] {
      "nolink",
      "left",
      "center",
      "right",
      "around",
      "wrap",
      "nowrap",
    };

    protected virtual IEnumerable<XmlNode> FormatPluginNode(XhtmlDocument xhtml, Plugin plugin)
    {
      switch (plugin.Name) {
        case "t":
          return new[] {xhtml.CreateTextNode("\t")};

        case "br":
          return new[] {xhtml.CreateXhtmlElement("br")};

        case "hr":
          return new[] {xhtml.CreateXhtmlElement("hr")};

        case "heart":
          return new[] {xhtml.CreateEntityReference("hearts")};

        case "new":
          return new[] {AppendChildren(xhtml.CreateXhtmlElement("span"), FormatNodes(xhtml, plugin.Nodes))};

        case "aname": {
          var id = plugin.Arguments[0];
          XmlNode node;

          if (plugin is InlinePlugin) {
            if (plugin.Nodes.Count == 0)
              node = AppendChildren(xhtml.CreateXhtmlElement("a", id, null), FormatNodes(xhtml, plugin.Nodes));
            else
              node = AppendChildren(xhtml.CreateXhtmlElement("span", id, null), FormatNodes(xhtml, plugin.Nodes));
          }
          else {
            node = AppendChildren(xhtml.CreateXhtmlElement("div", id, null), FormatNodes(xhtml, plugin.Nodes));
          }

          return new[] {node};
        }

        // face mark
        case "smile":
        case "bigsmile":
        case "huh":
        case "oh":
        case "wink":
        case "sad":
        case "worried": {
          var node = xhtml.CreateXhtmlElement("span");

          switch (plugin.Name) {
            case "oh": case "sad": case "worried":
              // 'WHITE FROWNING FACE' (U+2639)
              node.AppendChild(xhtml.CreateTextNode(((char)0x2639).ToString()));
              break;
            case "huh": case "wink":
              // 'BLACK SMILING FACE' (U+263B)
              node.AppendChild(xhtml.CreateTextNode(((char)0x263b).ToString()));
              break;
            default:
              // 'WHITE SMILING FACE' (U+263A)
              node.AppendChild(xhtml.CreateTextNode(((char)0x263a).ToString()));
              break;
          }

          node.AppendChild(xhtml.CreateXhtmlElement("sup")).AppendChild(xhtml.CreateTextNode(plugin.Name));

          return new[] {node};
        }

        case "sub": {
          return new[] {AppendChildren(xhtml.CreateXhtmlElement("sub"), FormatNodes(xhtml, plugin.Nodes))};
        }

        case "sup": {
          return new[] {AppendChildren(xhtml.CreateXhtmlElement("sup"), FormatNodes(xhtml, plugin.Nodes))};
        }

        case "ruby": {
          var ruby = xhtml.CreateXhtmlElement("ruby");

          ruby.AppendChild(xhtml.CreateXhtmlElement("rb")).AppendChildren(FormatNodes(xhtml, plugin.Nodes));
          ruby.AppendChild(xhtml.CreateXhtmlElement("rp")).AppendChild(xhtml.CreateTextNode("("));
          ruby.AppendChild(xhtml.CreateXhtmlElement("rt")).AppendChild(xhtml.CreateTextNode(plugin.Arguments[0]));
          ruby.AppendChild(xhtml.CreateXhtmlElement("rp")).AppendChild(xhtml.CreateTextNode(")"));

          return new[] {ruby};
        }

        case "ref":
        {
          string filename = null;
          string title = null;
          string align = null;

          foreach (var arg in plugin.Arguments) {
            var handled = false;

            foreach (var refparam in refParams) {
              if (arg != refparam)
                continue;

              switch (refparam) {
                case "left":
                case "center":
                case "right":
                  align = refparam;
                  break;
                default:
                  Console.Error.WriteLine("ref parameter '{0}' is not formatted", refparam);
                  break;
              }

              handled = true;
            }

            if (handled)
              continue;

            if (filename == null) {
              filename = arg;
              continue;
            }

            if (title == null) {
              title = arg;
              continue;
            }
          }

          XmlNode node = null;

          switch (Path.GetExtension(filename).ToLower()) {
            case ".jpg":
            case ".png":
            case ".gif":
            case ".tif":
            case ".bmp":
              node = xhtml.CreateXhtmlImage(filename, title ?? filename, title ?? filename);
              break;
            default:
              node = xhtml.CreateXhtmlAnchor(filename, title ?? filename);
              node.AppendChild(xhtml.CreateTextNode(title ?? filename));
              break;
          }
          if (plugin is InlinePlugin) {
            return new[] {node};
          }
          else {
            var parentNode = xhtml.CreateXhtmlElement("div");

            if (!string.IsNullOrEmpty(align))
              parentNode.SetAttribute("style", string.Format("text-align: {0};", align));

            parentNode.AppendChild(node);

            return new[] {parentNode};
          }
        }

        case "color":
        {
          var node = xhtml.CreateXhtmlElement((plugin is InlinePlugin) ? "span" : "div");

          node.AppendChildren(FormatNodes(xhtml, plugin.Nodes));

          string style;

          if (plugin.Arguments.Length == 1)
            style = string.Format("color: {0};", plugin.Arguments[0]);
          else if (plugin.Arguments.Length == 2)
            style = string.Format("color: {0}; background-color: {1};", plugin.Arguments[0], plugin.Arguments[1]);
          else
            style = null;

          if (style != null)
            node.SetAttribute("style", style);

          return new[] {node};
        }

        case "size": {
          var node = xhtml.CreateXhtmlElement((plugin is InlinePlugin) ? "span" : "div");

          node.AppendChildren(FormatNodes(xhtml, plugin.Nodes));
          node.SetAttribute("style", string.Format("font-size: {0}px;", plugin.Arguments[0]));

          return new[] {node};
        }

        default:
        {
          var unformatted = xhtml.CreateElement("pukiwiki:plugin", PukiWikiExtensionNamespace);

          unformatted.SetAttribute("pukiwiki:type", PukiWikiExtensionNamespace, (plugin is InlinePlugin) ? "inline" : "block");
          unformatted.SetAttribute("pukiwiki:name", PukiWikiExtensionNamespace, plugin.Name);
          unformatted.SetAttribute("pukiwiki:arguments", PukiWikiExtensionNamespace, Csv.ToJoined(plugin.Arguments));

          if (plugin is BlockPlugin) {
            unformatted.AppendChild(xhtml.CreateTextNode(string.Format("#{0}", plugin.Name)));

            if (plugin is MultilineBlockPlugin) {
              unformatted.AppendChild(xhtml.CreateTextNode("{{"));
              unformatted.AppendChild(xhtml.CreateCDataSection((plugin as MultilineBlockPlugin).LastArgument));
              unformatted.AppendChild(xhtml.CreateTextNode("}}"));
            }
          }
          else {
            if (0 < plugin.Nodes.Count)
              unformatted.AppendChildren(FormatNodes(xhtml, plugin.Nodes));
            else
              unformatted.AppendChild(xhtml.CreateTextNode(string.Format("&{0};", plugin.Name)));
          }

          return new[] {unformatted};
        }
      }
    }

    private IEnumerable<XmlNode> FormatAlignedParagraphNode(XhtmlDocument xhtml, AlignedParagraph p)
    {
      var node = xhtml.CreateXhtmlElement("div");

      node.AppendChildren(FormatNodes(xhtml, p.Nodes));
      node.SetAttribute("style", string.Format("text-align: {0};", p.Alignment));

      return new[] {node};
    }

    protected virtual IEnumerable<XmlNode> FormatAliasNode(XhtmlDocument xhtml, Alias alias)
    {
      var unformatted = xhtml.CreateElement("pukiwiki:alias", PukiWikiExtensionNamespace);

      unformatted.SetAttribute("pukiwiki:name", PukiWikiExtensionNamespace, alias.Name);
      unformatted.SetAttribute("pukiwiki:aliasto", PukiWikiExtensionNamespace, alias.AliasTo);

      if (string.IsNullOrEmpty(alias.AliasTo))
        unformatted.AppendChild(xhtml.CreateTextNode(string.Format("[[{0}]]", alias.Name)));
      else
        unformatted.AppendChild(xhtml.CreateTextNode(string.Format("[[{0}>{1}]]", alias.Name, alias.AliasTo)));

      return new[] {unformatted};
    }

    private IEnumerable<XmlNode> FormatHeaderAnchor(XhtmlDocument xhtml, HeaderAnchor anchor)
    {
      var a = XmlUtils.FindElement(base.FormatAnchorNode(xhtml, anchor), "a", W3CNamespaces.Xhtml);

      a.AppendChild(xhtml.CreateEntityReference("dagger"));
      a.SetAttribute("class", "anchor_super");

      return new[] {a};
    }

    private static XmlNode AppendChildren(XmlNode node, IEnumerable<XmlNode> newChildren)
    {
      node.AppendChildren(newChildren);

      return node;
    }
  }
}
