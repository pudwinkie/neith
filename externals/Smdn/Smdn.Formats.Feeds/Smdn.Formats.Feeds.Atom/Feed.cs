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

namespace Smdn.Formats.Feeds.Atom {
  // http://tools.ietf.org/html/rfc4287
  /// <remarks>4.1.1. The "atom:feed" Element</remarks>
  public class Feed : Feeds.FeedBase, IFeed {
    public override MimeType MimeType {
      get { return FeedMimeTypes.Atom; }
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST contain one or more atom:author elements,
    //       unless all of the atom:feed element's child atom:entry elements
    //       contain at least one atom:author element.
    // 4.2.1. The "atom:author" Element
    //    The "atom:author" element is a Person construct that indicates the
    //    author of the entry or feed.
    public List<Person> Authors {
      get { return authors; }
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MAY contain any number of atom:category
    //       elements.
    // 4.2.2. The "atom:category" Element
    //    The "atom:category" element conveys information about a category
    //    associated with an entry or feed.  This specification assigns no
    //    meaning to the content (if any) of this element.
    public List<Category> Categories {
      get { return categories; }
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MAY contain any number of atom:contributor
    //       elements.
    // 4.2.3. The "atom:contributor" Element
    //    The "atom:contributor" element is a Person construct that indicates a
    //    person or other entity who contributed to the entry or feed.
    public List<Person> Contributors {
      get { return contributors; }
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST NOT contain more than one atom:generator
    //       element.
    // 4.2.4. The "atom:generator" Element
    //    The "atom:generator" element's content identifies the agent used to
    //    generate a feed, for debugging and other purposes.
    public Generator Generator {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST NOT contain more than one atom:icon
    //       element.
    // 4.2.5. The "atom:icon" Element
    //    The "atom:icon" element's content is an IRI reference [RFC3987] that
    //    identifies an image that provides iconic visual identification for a
    //    feed.
    public Uri Icon {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST contain exactly one atom:id element.
    // 4.2.6. The "atom:id" Element
    //    The "atom:id" element conveys a permanent, universally unique
    //    identifier for an entry or feed.
    public Uri Id {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements SHOULD contain one atom:link element with a rel
    //       attribute value of "self".  This is the preferred URI for
    //       retrieving Atom Feed Documents representing this Atom feed.
    //    o  atom:feed elements MUST NOT contain more than one atom:link
    //       element with a rel attribute value of "alternate" that has the
    //       same combination of type and hreflang attribute values.
    //    o  atom:feed elements MAY contain additional atom:link elements
    //       beyond those described above.
    // 4.2.7. The "atom:link" Element
    //    The "atom:link" element defines a reference from an entry or feed to
    //    a Web resource.  This specification assigns no meaning to the content
    //    (if any) of this element.
    public List<Link> Links {
      get { return links; }
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST NOT contain more than one atom:logo
    //       element.
    // 4.2.8. The "atom:logo" Element
    //    The "atom:logo" element's content is an IRI reference [RFC3987] that
    //    identifies an image that provides visual identification for a feed.
    public Uri Logo {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST NOT contain more than one atom:rights
    //       element.
    // 4.2.10. The "atom:rights" Element
    //    The "atom:rights" element is a Text construct that conveys
    //    information about rights held in and over an entry or feed.
    public Text Rights {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST NOT contain more than one atom:subtitle
    //       element.
    // 4.2.12. The "atom:subtitle" Element
    //    The "atom:subtitle" element is a Text construct that conveys a human-
    //    readable description or subtitle for a feed.
    public Text Subtitle {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST contain exactly one atom:title element.
    // 4.2.14. The "atom:title" Element
    //    The "atom:title" element is a Text construct that conveys a human-
    //    readable title for an entry or feed.
    public Text Title {
      get; set;
    }

    // 4.1.1. The "atom:feed" Element
    //    o  atom:feed elements MUST contain exactly one atom:updated element.
    // 4.2.15. The "atom:updated" Element
    //    The "atom:updated" element is a Date construct indicating the most
    //    recent instant in time when an entry or feed was modified in a way
    //    the publisher considers significant.  Therefore, not all
    //    modifications necessarily result in a changed atom:updated value.
    public DateTimeOffset? Updated {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    public List<Entry> Entries {
      get { return entries; }
    }

    Uri IFeed.Uri {
      get
      {
        var self = Links.Find(Link.IsSelfLink);
        if (self == null)
          return null;
        else
          return self.Href;
      }
      set
      {
        var self = Links.Find(Link.IsSelfLink);
        if (self != null)
          Links.Remove(self);
        Links.Add(new Link(value));
      }
    }

    Uri IFeed.Link {
      get
      {
        var alternative = Links.Find(Link.IsAlternativeLink);
        if (alternative == null)
          return null;
        else
          return alternative.Href;
      }
      set
      {
        var alternative = Links.Find(Link.IsAlternativeLink);
        if (alternative != null)
          Links.Remove(alternative);
        Links.Add(new Link(value));
      }
    }

    string IFeed.Title {
      get { return (string)Title; }
      set { Title = (Text)value; }
    }

    string IFeed.Description {
      get { return (string)Subtitle; }
      set { Subtitle = (Text)value; }
    }

    DateTimeOffset? IFeed.Date {
      get { return Updated; }
      set { Updated = value; }
    }

    IEnumerable<IEntry> IFeed.Entries {
      get
      {
        return entries.ConvertAll(delegate(Entry e) {
          return (IEntry)e;
        });
        //return entries; // cannot implicity convert type
      }
    }

    IEnumerable<ModuleBase> IFeed.Modules {
      get { return base.Modules.Values; }
    }

    public Feed()
      : this(new Entry[] {})
    {
    }

    public Feed(IEnumerable<Entry> entries)
    {
      this.authors = new List<Person>();
      this.categories = new List<Category>();
      this.contributors = new List<Person>();
      this.Generator= null;
      this.Icon = null;
      this.Id = null;
      this.links = new List<Link>();
      this.Logo = null;
      this.Rights = null;
      this.Subtitle = null;
      this.Title = null;
      this.Updated = null;
      this.entries = new List<Entry>(entries);
    }

    public IEntry FindEntryByHash(byte[] hash)
    {
      foreach (var entry in entries) {
        if (ArrayExtensions.EqualsAll(entry.Hash, hash))
          return entry;
      }

      return null;
    }

    protected override void Format(XmlDocument document)
    {
      (new FormatterImpl()).Format(this, document);
    }

    private /*readonly*/ List<Person> authors;
    private /*readonly*/ List<Category> categories;
    private /*readonly*/ List<Person> contributors;
    private /*readonly*/ List<Link> links;
    private /*readonly*/ List<Entry> entries;
  }
}