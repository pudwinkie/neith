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
  /// <remarks>3.1. Text Constructs</remarks>
  public class Text : IEquatable<Text>, IEquatable<string>
  {
    public string Value {
      get; set;
    }

    // 3.1.1. The "type" Attribute
    //    Text constructs MAY have a "type" attribute.  When present, the value
    //    MUST be one of "text", "html", or "xhtml".  If the "type" attribute
    //    is not provided, Atom Processors MUST behave as though it were
    //    present with a value of "text".  Unlike the atom:content element
    //    defined in Section 4.1.3, MIME media types [MIMEREG] MUST NOT be used
    //    as values for the "type" attribute on Text constructs.
    public TextType? Type {
      get; set;
    }

    public Text()
      : this(null, null)
    {
    }

    public Text(string text)
      : this(text, TextType.Text)
    {
    }

    public Text(string text, TextType? type)
    {
      this.Value = text;
      this.Type = type;
    }

    public static implicit operator Text(string text)
    {
      return new Text(text);
    }

    public static implicit operator string(Text text)
    {
      if (text == null)
        return null;
      else
        return text.Value;
    }

    public override string ToString()
    {
      return Value;
    }

    public XmlDocument ToXmlDocument()
    {
      if (Value == null)
        throw new FeedFormatException("Value is null");
      if (Type == null || Type != TextType.Xhtml)
        throw new FeedFormatException("Value is not xhtml");

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
      if (o is Text)
        return Equals(o as Text);
      else if (o is string)
        return Equals(o as string);
      else
        return false;
    }

    public bool Equals(string other)
    {
      return (Value == other);
    }

    public bool Equals(Text other)
    {
      return (this.Type == other.Type) && (this.Value == other.Value);
    }
  }
}
