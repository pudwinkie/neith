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

namespace Smdn.Formats.Feeds.Atom {
  /// <remarks>4.1.3. The "atom:content" Element</remarks>
  public class Content : IEquatable<Content>, IEquatable<string>
  {
    // 4.1.3. The "atom:content" Element
    //    The "atom:content" element either contains or links to the content of
    //    the entry.  The content of atom:content is Language-Sensitive.
    public string Value {
      get; set;
    }

    // 4.1.3.1. The "type" Attribute
    //    On the atom:content element, the value of the "type" attribute MAY be
    //    one of "text", "html", or "xhtml".  Failing that, it MUST conform to
    //    the syntax of a MIME media type, but MUST NOT be a composite type
    //    (see Section 4.2.6 of [MIMEREG]).  If neither the type attribute nor
    //    the src attribute is provided, Atom Processors MUST behave as though
    //    the type attribute were present with a value of "text".
    public string Type {
      get; set;
    }

    // 4.1.3.2. The "src" Attribute
    //    atom:content MAY have a "src" attribute, whose value MUST be an IRI
    //    reference [RFC3987].  If the "src" attribute is present, atom:content
    //    MUST be empty.  Atom Processors MAY use the IRI to retrieve the
    //    content and MAY choose to ignore remote content or to present it in a
    //    different manner than local content.
    // 
    //    If the "src" attribute is present, the "type" attribute SHOULD be
    //    provided and MUST be a MIME media type [MIMEREG], rather than "text",
    //    "html", or "xhtml".  The value is advisory; that is to say, when the
    //    corresponding URI (mapped from an IRI, if necessary) is dereferenced,
    //    if the server providing that content also provides a media type, the
    //    server-provided media type is authoritative.
    public Uri Src {
      get; set;
    }

    public Content()
      : this(null, null, null)
    {
    }

    public Content(string @value)
      : this(@value, "text", null)
    {
    }

    public Content(string @value, string type)
      : this(@value, type, null)
    {
    }

    public Content(string type, Uri src)
      : this(null, type, src)
    {
    }

    public Content(string @value, string type, Uri src)
    {
      this.Value = @value;
      this.Type = type;
      this.Src = src;
    }

    public static implicit operator Content(string content)
    {
      return new Content(content);
    }

    public static implicit operator string(Content content)
    {
      if (content == null)
        return null;
      else
        return content.Value;
    }

    public override string ToString()
    {
      return Value;
    }

    public XmlDocument ToXmlDocument()
    {
      if (Value == null)
        throw new FeedFormatException("Value is null");

      if (Type == null)
        // If neither the type attribute nor
        // the src attribute is provided, Atom Processors MUST behave as though
        // the type attribute were present with a value of "text".
        throw new FeedFormatException("Value is not xhtml");
      else if (string.Equals(Type, "text", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Type, "html", StringComparison.OrdinalIgnoreCase))
        throw new FeedFormatException("Value is not xhtml");

      // try to read as XML
      var xml = new XmlDocument();

      xml.LoadXml(Value);

      return xml;
    }

    public override int GetHashCode()
    {
      if (Value == null)
        return base.GetHashCode();
      else
        return Value.GetHashCode();
    }

    public override bool Equals(object o)
    {
      if (o is Content)
        return Equals(o as Content);
      else if (o is string)
        return Equals(o as string);
      else
        return false;
    }

    public bool Equals(string other)
    {
      return (Value == other);
    }

    public bool Equals(Content other)
    {
      return (this.Type == other.Type) && (this.Src == other.Src) && (this.Value == other.Value);
    }
  }
}
