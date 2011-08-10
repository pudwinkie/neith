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

namespace Smdn.Formats.Feeds.Atom {
  /// <remarks>4.2.7. The "atom:link" Element</remarks>
  public class Link : ICloneable {
    public static bool IsAlternativeLink(Link l)
    {
      return (l.Rel == null || string.Equals(l.Rel, "alternate", StringComparison.Ordinal));
    }

    public static Link CreateAlternative(Uri href)
    {
      return CreateAlternative(href, null, null);
    }

    public static Link CreateAlternative(Uri href, MimeType type)
    {
      return CreateAlternative(href, type, null);
    }

    public static Link CreateAlternative(Uri href, MimeType type, string title)
    {
      return new Link(href, "alternate", type, title);
    }

    public static bool IsSelfLink(Link l)
    {
      return string.Equals(l.Rel, "self", StringComparison.Ordinal);
    }

    public static Link CreateSelf(Uri href)
    {
      return CreateSelf(href, null, null);
    }

    public static Link CreateSelf(Uri href, MimeType type)
    {
      return CreateSelf(href, type, null);
    }

    public static Link CreateSelf(Uri href, MimeType type, string title)
    {
      return new Link(href, "self", type, title);
    }

    public static bool IsRelatedLink(Link l)
    {
      return string.Equals(l.Rel, "related", StringComparison.Ordinal);
    }

    public static Link CreateRelated(Uri href)
    {
      return CreateRelated(href, null, null);
    }

    public static Link CreateRelated(Uri href, MimeType type)
    {
      return CreateRelated(href, type, null);
    }

    public static Link CreateRelated(Uri href, MimeType type, string title)
    {
      return new Link(href, "related", type, title);
    }

    // 4.2.7.1. The "href" Attribute
    //    The "href" attribute contains the link's IRI. atom:link elements MUST
    //    have an href attribute, whose value MUST be a IRI reference
    //    [RFC3987].
    /// <remarks>4.2.7.1. The "href" Attribute</remarks>
    public Uri Href {
      get; set;
    }

    // 4.2.7.2. The "rel" Attribute
    //    atom:link elements MAY have a "rel" attribute that indicates the link
    //    relation type.  If the "rel" attribute is not present, the link
    //    element MUST be interpreted as if the link relation type is
    //    "alternate".
    // 
    //    This document defines five initial values for the Registry of Link
    //    Relations:
    /// <remarks>4.2.7.2. The "rel" Attribute</remarks>
    public string Rel {
      get; set;
    }

    // 4.2.7.3. The "type" Attribute
    //    On the link element, the "type" attribute's value is an advisory
    //    media type: it is a hint about the type of the representation that is
    //    expected to be returned when the value of the href attribute is
    //    dereferenced.  Note that the type attribute does not override the
    //    actual media type returned with the representation.  Link elements
    //    MAY have a type attribute, whose value MUST conform to the syntax of
    //    a MIME media type [MIMEREG].
    /// <remarks>4.2.7.3. The "type" Attribute</remarks>
    public MimeType Type {
      get; set;
    }

    // 4.2.7.4. The "hreflang" Attribute
    //    The "hreflang" attribute's content describes the language of the
    //    resource pointed to by the href attribute.  When used together with
    //    the rel="alternate", it implies a translated version of the entry.
    //    Link elements MAY have an hreflang attribute, whose value MUST be a
    //    language tag [RFC3066].
    /// <remarks>4.2.7.4. The "hreflang" Attribute</remarks>
    public string HrefLang {
      get; set;
    }

    // 4.2.7.5. The "title" Attribute
    //    The "title" attribute conveys human-readable information about the
    //    link.  The content of the "title" attribute is Language-Sensitive.
    //    Entities such as "&amp;" and "&lt;" represent their corresponding
    //    characters ("&" and "<", respectively), not markup.  Link elements
    //    MAY have a title attribute.
    /// <remarks>4.2.7.5. The "title" Attribute</remarks>
    public string Title {
      get; set;
    }

    // 4.2.7.6. The "length" Attribute
    //    The "length" attribute indicates an advisory length of the linked
    //    content in octets; it is a hint about the content length of the
    //    representation returned when the IRI in the href attribute is mapped
    //    to a URI and dereferenced.  Note that the length attribute does not
    //    override the actual content length of the representation as reported
    //    by the underlying protocol.  Link elements MAY have a length
    //    attribute.
    /// <remarks>4.2.7.6. The "length" Attribute</remarks>
    public int? Length {
      get; set;
    }

    public Link()
    {
      this.Href = null;
      this.Rel = null;
      this.Type = null;
      this.HrefLang = null;
      this.Title = null;
      this.Length = null;
    }

    public Link(string href)
      : this(new Uri(href))
    {
    }

    public Link(Uri href)
      : this(href, "alternate")
    {
    }

    public Link(string href, string rel)
      : this(new Uri(href), rel)
    {
    }

    public Link(Uri href, string rel)
      : this(href, rel, null, null, null, null)
    {
    }

    public Link(string href, string rel, MimeType type, string title)
      : this(new Uri(href), rel, type, title)
    {
    }

    public Link(Uri href, string rel, MimeType type, string title)
      : this(href, rel, type, null, title, null)
    {
    }

    public Link(string href, string rel, MimeType type, string hrefLang, string title, int? length)
      : this(new Uri(href), rel, type, hrefLang, title, length)
    {
    }

    public Link(Uri href, string rel, MimeType type, string hrefLang, string title, int? length)
    {
      if (href == null)
        throw new ArgumentNullException("href");

      this.Href = href;
      this.Rel = rel;
      this.Type = type;
      this.HrefLang = hrefLang;
      this.Title = title;
      this.Length = length;
    }

    public Link Clone()
    {
      var cloned = (Link)MemberwiseClone();

      cloned.Href = new Uri(this.Href.ToString());

      return cloned;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }
  }
}
