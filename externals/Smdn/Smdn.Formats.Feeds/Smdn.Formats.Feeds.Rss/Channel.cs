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

namespace Smdn.Formats.Feeds.Rss {
  // RSS 2.0 Specification
  // http://cyber.law.harvard.edu/rss/rss.html
  public class Channel : Feeds.FeedBase, IFeed {
    public override MimeType MimeType {
      get { return FeedMimeTypes.Rss; }
    }

    // Required channel elements

    /// <summary>The name of the channel. It's how people refer to your service. If you have an HTML website that contains the same information as your RSS file, the title of your channel should be the same as the title of your website.</summary>
    public string Title {
      get; set;
    }

    /// <summary>The URL to the HTML website corresponding to the channel.</summary>
    public Uri Link {
      get; set;
    }

    /// <summary>Phrase or sentence describing the channel.</summary>
    public string Description {
      get; set;
    }

    // Optional channel elements

    /// <summary>The language the channel is written in. This allows aggregators to group all Italian language sites, for example, on a single page. A list of allowable values for this element, as provided by Netscape, is here. You may also use values defined by the W3C.</summary>
    public string Language {
      get; set;
    }

    /// <summary>Copyright notice for content in the channel.</summary>
    public string Copyright {
      get; set;
    }

    /// <summary>Email address for person responsible for editorial content.</summary>
    public string ManagingEditor {
      get; set;
    }

    /// <summary>Email address for person responsible for technical issues relating to channel.</summary>
    public string WebMaster {
      get; set;
    }

    /// <summary>The publication date for the content in the channel. For example, the New York Times publishes on a daily basis, the publication date flips once every 24 hours. That's when the pubDate of the channel changes. All date-times in RSS conform to the Date and Time Specification of RFC 822, with the exception that the year may be expressed with two characters or four characters (four preferred).</summary>
    public DateTimeOffset? PubDate {
      get; set;
    }

    /// <summary>The last time the content of the channel changed.</summary>
    public DateTimeOffset? LastBuildDate {
      get; set;
    }

    /// <summary>Specify one or more categories that the channel belongs to. Follows the same rules as the <item>-level category element. More info.</summary>
    public List<Category> Categories {
      get { return categories; }
    }

    /// <summary>A string indicating the program used to generate the channel.</summary>
    public string Generator {
      get; set;
    }

    /// <summary>A URL that points to the documentation for the format used in the RSS file. It's probably a pointer to this page. It's for people who might stumble across an RSS file on a Web server 25 years from now and wonder what it is.</summary>
    public Uri Docs {
      get; set;
    }

    /// <summary>Allows processes to register with a cloud to be notified of updates to the channel, implementing a lightweight publish-subscribe protocol for RSS feeds. More info here.</summary>
    public Cloud Cloud {
      get; set;
    }

    /// <summary>ttl stands for time to live. It's a number of minutes that indicates how long a channel can be cached before refreshing from the source. More info here.</summary>
    public int? Ttl {
      get; set;
    }

    /// <summary>Specifies a GIF, JPEG or PNG image that can be displayed with the channel. More info here.</summary>
    public Image Image {
      get; set;
    }

    /// <summary>The PICS rating for the channel.</summary>
    public string Rating {
      get; set;
    }

    /// <summary>Specifies a text input box that can be displayed with the channel. More info here.</summary>
    public TextInput TextInput {
      get; set;
    }

    /// <summary>A hint for aggregators telling them which hours they can skip. More info here.</summary>
    public List<int> SkipHours {
      get { return skipHours; }
    }

    /// <summary>A hint for aggregators telling them which days they can skip. More info here.</summary>
    public List<DayOfWeek> SkipDays {
      get { return skipDays; }
    }

    public List<Item> Items {
      get { return items; }
    }

    Uri IFeed.Uri {
      get { return null; }
      set { }
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
      get { return PubDate; }
      set { PubDate = value; }
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
      this.Title = null;
      this.Link = null;
      this.Description = null;
      this.Language = null;
      this.Copyright = null;
      this.ManagingEditor = null;
      this.WebMaster = null;
      this.PubDate = null;
      this.LastBuildDate = null;
      this.categories = new List<Category>();
      this.Generator = null;
      this.Docs = new Uri("http://cyber.law.harvard.edu/rss/rss.html");
      this.Cloud = null;
      this.Ttl = null;
      this.Image = null;
      this.Rating = null;
      this.TextInput = null;
      this.skipHours = new List<int>();
      this.skipDays = new List<DayOfWeek>();
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

    private /*readonly*/ List<Category> categories;
    private /*readonly*/ List<int> skipHours;
    private /*readonly*/ List<DayOfWeek> skipDays;
    private /*readonly*/ List<Item> items;
  }
}
