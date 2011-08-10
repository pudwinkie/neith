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
using System.Xml;
using System.Collections.Generic;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  public abstract class EntryBase {
    public byte[] Hash {
      get; internal set;
    }

    public XmlNode SourceNode {
      get; internal set;
    }

    public IDictionary<string, ModuleBase> Modules {
      get { return modules; }
    }

    public Annotation AnnotationModule {
      get
      {
        if (modules.ContainsKey(Annotation.NamespaceUri))
          return modules[Annotation.NamespaceUri] as Annotation;
        else
          return Annotation.Null;
      }
    }

    public Content ContentModule {
      get
      {
        if (modules.ContainsKey(Content.NamespaceUri))
          return modules[Content.NamespaceUri] as Content;
        else
          return Content.Null;
      }
    }

    public DublinCore DublinCoreModule {
      get
      {
        if (modules.ContainsKey(DublinCore.NamespaceUri))
          return modules[DublinCore.NamespaceUri] as DublinCore;
        else
          return DublinCore.Null;
      }
    }

    public Image ImageModule {
      get
      {
        if (modules.ContainsKey(Image.NamespaceUri))
          return modules[Image.NamespaceUri] as Image;
        else
          return Image.Null;
      }
    }

    public Taxonomy TaxonomyModule {
      get
      {
        if (modules.ContainsKey(Taxonomy.NamespaceUri))
          return modules[Taxonomy.NamespaceUri] as Taxonomy;
        else
          return Taxonomy.Null;
      }
    }

    public Trackback TrackbackModule {
      get
      {
        if (modules.ContainsKey(Trackback.NamespaceUri))
          return modules[Trackback.NamespaceUri] as Trackback;
        else
          return Trackback.Null;
      }
    }

    protected EntryBase()
    {
      this.Hash = null;
    }

    private /*readonly*/ Dictionary<string, ModuleBase> modules = new Dictionary<string, ModuleBase>();
  }
}
