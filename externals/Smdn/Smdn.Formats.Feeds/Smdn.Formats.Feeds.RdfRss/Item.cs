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

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds.RdfRss {
  // http://
  public class Item : Feeds.EntryBase, IEntry {
    public Uri Resource {
      get; set;
    }

    public Uri Link {
      get; set;
    }

    public string Title {
      get; set;
    }

    public string Description {
      get; set;
    }

    Uri IEntry.Link {
      get { return Link; }
      set { Link = value; }
    }

    string IEntry.Title {
      get { return Title; }
      set { Title = value; }
    }

    string IEntry.Description {
      get { return Description; }
      set { Description = value; }
    }

    DateTimeOffset? IEntry.Date {
      get
      {
        if (DublinCoreModule == DublinCore.Null)
          return null;
        else
          return DublinCoreModule.GetDate();
      }
      set
      {
        var dc = DublinCoreModule;

        if (value == null) {
          if (dc != DublinCore.Null)
            dc.SetDate(null);
        }
        else {
          if (dc == DublinCore.Null) {
            Modules.Add(DublinCore.NamespaceUri, new DublinCore());
            dc = DublinCoreModule;
          }
          dc.SetDate(value);
        }
      }
    }

    string IEntry.Id {
      get
      {
        if (Resource == null)
          return null;
        else
          return Resource.ToString();
      }
      set
      {
        if (value == null)
          Resource = null;
        else
          Resource = new Uri(value);
      }
    }

    IEnumerable<ModuleBase> IEntry.Modules {
      get { return base.Modules.Values; }
    }

    public Item()
    {
      this.Resource = null;
      this.Link = null;
      this.Title = null;
      this.Description = null;
    }
  }
}