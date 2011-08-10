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
using System.Reflection;
using System.Xml;

namespace Smdn.Formats.Feeds.Modules {
  public abstract class ModuleBase {
    public static readonly Dictionary<string, Type> KnowModules;

    static ModuleBase()
    {
      // TODO readonly
      KnowModules = new Dictionary<string, Type>() {
        {Annotation.NamespaceUri, typeof(Annotation)},
        {Content.NamespaceUri, typeof(Content)},
        {DublinCore.NamespaceUri, typeof(DublinCore)},
        {Image.NamespaceUri, typeof(Image)},
        {Syndication.NamespaceUri, typeof(Syndication)},
        {Taxonomy.NamespaceUri, typeof(Taxonomy)},
        {Trackback.NamespaceUri, typeof(Trackback)},
      };
    }

    public static ModuleBase GetModule(string namespaceUri)
    {
      if (KnowModules.ContainsKey(namespaceUri))
        return KnowModules[namespaceUri].InvokeMember(null,
                                                      BindingFlags.CreateInstance,
                                                      null,
                                                      null,
                                                      null) as ModuleBase;
      else
        return null;
    }

    public abstract string ModulePrefix {
      get;
    }

    public abstract string ModuleNamespaceUri {
      get;
    }

    protected internal abstract bool IsNull {
      get;
    }

    protected ModuleBase()
    {
    }

    internal protected virtual void Parse(FeedBase feed, XmlNode parent, XmlNamespaceManager nsmgr) {}
    internal protected virtual void Parse(EntryBase entry, XmlNode parent, XmlNamespaceManager nsmgr) {}

    internal protected virtual void Format(FeedBase feed, XmlNode parent) {}
    internal protected virtual void Format(EntryBase entry, XmlNode parent) {}
  }
}