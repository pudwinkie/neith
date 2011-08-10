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
using System.Xml;

namespace Smdn.Formats.Feeds {
  internal abstract class FormatterCore {
    internal protected FormatterCore()
    {
    }

    protected void AppendAttribute(XmlElement parent, string @value, string name, string namespaceUri, bool required)
    {
      if (@value == null) {
        if (required)
          throw new MandatoryValueMissingException(string.Format("{0} is required element", name));
        else
          return;
      }

      parent.SetAttribute(name, namespaceUri, @value);
    }

    protected XmlElement AppendTextElement(XmlNode parent, string text, string name, string namespaceUri, bool required)
    {
      if (text == null) {
        if (required)
          throw new MandatoryValueMissingException(string.Format("{0} is required element", name));
        else
          return null;
      }

      var element = parent.AppendChild(parent.OwnerDocument.CreateElement(name, namespaceUri)) as XmlElement;

      element.AppendChild(parent.OwnerDocument.CreateTextNode(text));

      return element;
    }

    protected void FormatModule(FeedBase feed, XmlNode feedNode, out Dictionary<string, string> moduleNamespaces)
    {
      moduleNamespaces = new Dictionary<string, string>();

      foreach (var module in feed.Modules.Values) {
        if (!module.IsNull) {
          module.Format(feed, feedNode);
          if (!moduleNamespaces.ContainsKey(module.ModuleNamespaceUri))
            moduleNamespaces.Add(module.ModuleNamespaceUri, module.ModulePrefix);
        }
      }
    }

    protected void FormatModule(EntryBase entry, XmlNode entryNode, out Dictionary<string, string> moduleNamespaces)
    {
      moduleNamespaces = new Dictionary<string, string>();

      foreach (var module in entry.Modules.Values) {
        if (!module.IsNull) {
          module.Format(entry, entryNode);
          if (!moduleNamespaces.ContainsKey(module.ModuleNamespaceUri))
            moduleNamespaces.Add(module.ModuleNamespaceUri, module.ModulePrefix);
        }
      }
    }
  }
}