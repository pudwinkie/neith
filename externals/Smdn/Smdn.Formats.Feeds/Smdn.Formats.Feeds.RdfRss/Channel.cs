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

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds.RdfRss {
  // http://
  public class Channel : Feeds.FeedBase, IFeed {
    public override MimeType MimeType {
      get { return FeedMimeTypes.RdfRss; }
    }

    public Uri Uri {
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

    public List<Item> Items {
      get { return items; }
    }

    Uri IFeed.Uri {
      get { return this.Uri; }
      set { this.Uri = value; }
    }

    Uri IFeed.Link {
      get { return Link; }
      set { Link = value; }
    }

    string IFeed.Title {
      get { return Title; }
      set { Title = value; }
    }

    string IFeed.Description {
      get { return Description; }
      set { Description = value; }
    }

    DateTimeOffset? IFeed.Date {
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

    IEnumerable<IEntry> IFeed.Entries {
      get
      {
        return items.ConvertAll(delegate(Item i) {
          return (IEntry)i;
        });
        //return items; // cannot implicity convert type
      }
    }

    IEnumerable<ModuleBase> IFeed.Modules {
      get { return base.Modules.Values; }
    }

    public Channel()
      : this(new Item[] {})
    {
    }

    public Channel(IEnumerable<Item> items)
    {
      this.Uri = null;
      this.Link = null;
      this.Title = null;
      this.Description = null;
      this.items = new List<Item>(items);
    }

    public IEntry FindEntryByHash(byte[] hash)
    {
      foreach (var item in items) {
        if (ArrayExtensions.EqualsAll(item.Hash, hash))
          return item;
      }

      return null;
    }

    protected override void Format(XmlDocument document)
    {
      (new FormatterImpl()).Format(this, document);
    }

    private /*readonly*/ List<Item> items;
  }
}