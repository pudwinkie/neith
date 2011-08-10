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

namespace Smdn.Formats.Feeds.Rss {
  // RSS 2.0 Specification
  // http://cyber.law.harvard.edu/rss/rss.html
  public class Item : Feeds.EntryBase, IEntry {
    // All elements of an item are optional, however at least one of title or description must be present.

    /// <summary>The title of the item.</summary>
    public string Title {
      get; set;
    }

    /// <summary>The URL of the item.</summary>
    public Uri Link {
      get; set;
    }

    /// <summary>The item synopsis.</summary>
    public string Description {
      get; set;
    }

    /// <summary>Email address of the author of the item. More.</summary>
    public string Author {
      get; set;
    }

    /// <summary>Includes the item in one or more categories. More.</summary>
    public List<Category> Categories {
      get { return categories; }
    }

    /// <summary>URL of a page for comments relating to the item. More.</summary>
    public Uri Comments {
      get; set;
    }

    /// <summary>Describes a media object that is attached to the item. More.</summary>
    public Enclosure Enclosure {
      get; set;
    }

    /// <summary>A string that uniquely identifies the item. More.</summary>
    public Guid Guid {
      get; set;
    }

    /// <summary>Indicates when the item was published. More.</summary>
    public DateTimeOffset? PubDate {
      get; set;
    }

    /// <summary>The RSS channel that the item came from. More.</summary>
    public Source Source {
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
      // use dc:date
      get { return PubDate; }
      set { PubDate = value; }
    }

    string IEntry.Id {
      get
      {
        if (Guid == null)
          return null;
        else
          return Guid.Value;
      }
      set
      {
        if (value == null)
          Guid = null;
        else
          Guid = new Guid(value);
      }
    }

    IEnumerable<ModuleBase> IEntry.Modules {
      get { return base.Modules.Values; }
    }

    public Item()
    {
      this.Title = null;
      this.Link = null;
      this.Description = null;
      this.Author = null;
      this.categories = new List<Category>();
      this.Comments = null;
      this.Enclosure = null;
      this.Guid = null;
      this.PubDate = null;
      this.Source = null;
    }

    private readonly List<Category> categories;
  }
}
