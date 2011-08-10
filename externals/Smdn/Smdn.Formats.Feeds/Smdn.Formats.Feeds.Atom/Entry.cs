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

namespace Smdn.Formats.Feeds.Atom {
  // http://tools.ietf.org/html/rfc4287
  // 4.1.2. The "atom:entry" Element
  public class Entry : Feeds.EntryBase, IEntry {
    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST contain one or more atom:author elements,
    //       unless the atom:entry contains an atom:source element that
    //       contains an atom:author element or, in an Atom Feed Document, the
    //       atom:feed element contains an atom:author element itself.
    // 4.2.1. The "atom:author" Element
    //    The "atom:author" element is a Person construct that indicates the
    //    author of the entry or feed.
    public List<Person> Authors {
      get { return authors; }
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MAY contain any number of atom:category
    //       elements.
    // 4.2.2. The "atom:category" Element
    //    The "atom:category" element conveys information about a category
    //    associated with an entry or feed.  This specification assigns no
    //    meaning to the content (if any) of this element.
    public List<Category> Categories {
      get { return categories; }
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST NOT contain more than one atom:content
    //       element.
    public Content Content {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MAY contain any number of atom:contributor
    //       elements.
    // 4.2.3. The "atom:contributor" Element
    //    The "atom:contributor" element is a Person construct that indicates a
    //    person or other entity who contributed to the entry or feed.
    public List<Person> Contributors {
      get { return contributors; }
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST contain exactly one atom:id element.
    // 4.2.6. The "atom:id" Element
    //    The "atom:id" element conveys a permanent, universally unique
    //    identifier for an entry or feed.
    public Uri Id {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements that contain no child atom:content element
    //       MUST contain at least one atom:link element with a rel attribute
    //       value of "alternate".
    //    o  atom:entry elements MUST NOT contain more than one atom:link
    //       element with a rel attribute value of "alternate" that has the
    //       same combination of type and hreflang attribute values.
    //    o  atom:entry elements MAY contain additional atom:link elements
    //       beyond those described above.
    // 4.2.7. The "atom:link" Element
    //    The "atom:link" element defines a reference from an entry or feed to
    //    a Web resource.  This specification assigns no meaning to the content
    //    (if any) of this element.
    public List<Link> Links {
      get { return links; }
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST NOT contain more than one atom:published
    //       element.
    // 4.2.9. The "atom:published" Element
    //    The "atom:published" element is a Date construct indicating an
    //    instant in time associated with an event early in the life cycle of
    //    the entry.
    public DateTimeOffset? Published {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST NOT contain more than one atom:rights
    //       element.
    // 4.2.10. The "atom:rights" Element
    //    The "atom:rights" element is a Text construct that conveys
    //    information about rights held in and over an entry or feed.
    public Text Rights {
      get; set;
    }

    // 4.2.11. The "atom:source" Element
    //    If an atom:entry is copied from one feed into another feed, then the
    //    source atom:feed's metadata (all child elements of atom:feed other
    //    than the atom:entry elements) MAY be preserved within the copied
    //    entry by adding an atom:source child element, if it is not already
    //    present in the entry, and including some or all of the source feed's
    //    Metadata elements as the atom:source element's children.  Such
    //    metadata SHOULD be preserved if the source atom:feed contains any of
    //    the child elements atom:author, atom:contributor, atom:rights, or
    //    atom:category and those child elements are not present in the source
    //    atom:entry.
    // TODO: Feed以外での表現
    public Feed Source {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST contain an atom:summary element in either
    //       of the following cases:
    //       *  the atom:entry contains an atom:content that has a "src"
    //          attribute (and is thus empty).
    //       *  the atom:entry contains content that is encoded in Base64;
    //          i.e., the "type" attribute of atom:content is a MIME media type
    //          [MIMEREG], but is not an XML media type [RFC3023], does not
    //          begin with "text/", and does not end with "/xml" or "+xml".
    //    o  atom:entry elements MUST NOT contain more than one atom:summary
    //       element.
    // 4.2.13. The "atom:summary" Element
    //    The "atom:summary" element is a Text construct that conveys a short
    //    summary, abstract, or excerpt of an entry.
    public Text Summary {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST contain exactly one atom:title element.
    // 4.2.14. The "atom:title" Element
    //    The "atom:title" element is a Text construct that conveys a human-
    //    readable title for an entry or feed.
    public Text Title {
      get; set;
    }

    // 4.1.2. The "atom:entry" Element
    //    o  atom:entry elements MUST contain exactly one atom:updated element.
    // 4.2.15. The "atom:updated" Element
    //    The "atom:updated" element is a Date construct indicating the most
    //    recent instant in time when an entry or feed was modified in a way
    //    the publisher considers significant.  Therefore, not all
    //    modifications necessarily result in a changed atom:updated value.
    public DateTimeOffset? Updated {
      get; set;
    }

    Uri IEntry.Link {
      get
      {
        var alternative = links.Find(Link.IsAlternativeLink);
        if (alternative == null)
          return null;
        else
          return alternative.Href;
      }
      set
      {
        var alternative = links.Find(Link.IsAlternativeLink);
        if (alternative != null)
          links.Remove(alternative);
        links.Add(new Link(value));
      }
    }

    string IEntry.Title {
      get { return (string)Title; }
      set { Title = (Text)value; }
    }

    string IEntry.Description {
      get { return (string)Summary; }
      set { Summary = (Text)value; }
    }

    DateTimeOffset? IEntry.Date {
      get { return Updated; }
      set { Updated = value; }
    }

    string IEntry.Id {
      get
      {
        if (Id == null)
          return null;
        else
          return Id.ToString();
      }
      set
      {
        if (value == null)
          Id = null;
        else
          Id = new Uri(value);
      }
    }

    IEnumerable<ModuleBase> IEntry.Modules {
      get { return base.Modules.Values; }
    }

    public Entry()
    {
      this.authors = new List<Person>();
      this.categories = new List<Category>();
      this.Content = null;
      this.contributors = new List<Person>();
      this.Id = null;
      this.links = new List<Link>();
      this.Published = null;
      this.Rights = null;
      this.Source = null;
      this.Summary = null;
      this.Title = null;
      this.Updated = null;
    }

    private /*readonly*/ List<Person> authors;
    private /*readonly*/ List<Category> categories;
    private /*readonly*/ List<Person> contributors;
    private /*readonly*/ List<Link> links;
  }
}