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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Xml;

namespace Smdn.Formats.Feeds.Modules {
  // RDF Site Summary 1.0 Modules: Dublin Core
  //   http://dublincore.org/documents/2008/01/14/dcmi-terms/
  //   http://dublincore.org/documents/dcmi-terms/
  //   http://web.resource.org/rss/1.0/modules/dc/
  public class DublinCore : ModuleBase {
    public const string Prefix = "dc";
    public const string NamespaceUri = "http://purl.org/dc/elements/1.1/";

    public static readonly DublinCore Null = new DublinCore();

    public override string ModulePrefix {
      get { return Prefix; }
    }

    public override string ModuleNamespaceUri {
      get { return NamespaceUri; }
    }

    protected internal override bool IsNull {
      get { return this == Null; }
    }

    /// <summary>An entity responsible for making contributions to the resource.</summary>
    /// <remarks>Examples of a Contributor include a person, an organization, or a service. Typically, the name of a Contributor should be used to indicate the entity.<remarks>
    public string[] Contributor {
      get; set;
    }

    /// <summary>The spatial or temporal topic of the resource, the spatial applicability of the resource, or the jurisdiction under which the resource is relevant.</summary>
    /// <remarks>Spatial topic and spatial applicability may be a named place or a location specified by its geographic coordinates. Temporal topic may be a named period, date, or date range. A jurisdiction may be a named administrative entity or a geographic place to which the resource applies. Recommended best practice is to use a controlled vocabulary such as the Thesaurus of Geographic Names [TGN]. Where appropriate, named places or time periods can be used in preference to numeric identifiers such as sets of coordinates or date ranges.<remarks>
    public string[] Coverage {
      get; set;
    }

    /// <summary>An entity primarily responsible for making the resource.</summary>
    /// <remarks>Examples of a Creator include a person, an organization, or a service. Typically, the name of a Creator should be used to indicate the entity.<remarks>
    public string[] Creator {
      get; set;
    }

    /// <summary>A point or period of time associated with an event in the lifecycle of the resource.</summary>
    /// <remarks>Date may be used to express temporal information at any level of granularity. Recommended best practice is to use an encoding scheme, such as the W3CDTF profile of ISO 8601 [W3CDTF].<remarks>
    public DateTimeOffset?[] Date {
      get; set;
    }

    internal DateTimeOffset? GetDate()
    {
      if (Date == null || Date.Length == 0)
        return null;
      else
        return Date[0];
    }

    internal void SetDate(DateTimeOffset? val)
    {
      if (val == null)
        Date = null;
      else
        Date = new[] {val};
    }

    /// <summary>An account of the resource.</summary>
    /// <remarks>Description may include but is not limited to: an abstract, a table of contents, a graphical representation, or a free-text account of the resource.<remarks>
    public string[] Description {
      get; set;
    }

    /// <summary>The file format, physical medium, or dimensions of the resource.</summary>
    /// <remarks>Examples of dimensions include size and duration. Recommended best practice is to use a controlled vocabulary such as the list of Internet Media Types [MIME].<remarks>
    public string[] FileFormat {
      get; set;
    }

    /// <summary>An unambiguous reference to the resource within a given context.</summary>
    /// <remarks>Recommended best practice is to identify the resource by means of a string conforming to a formal identification system.<remarks>
    public string[] Identifier {
      get; set;
    }

    /// <summary>A language of the resource.</summary>
    /// <remarks>Recommended best practice is to use a controlled vocabulary such as RFC 4646 [RFC4646].
    public string[] Language {
      get; set;
    }

    /// <summary>An entity responsible for making the resource available.</summary>
    /// <remarks>Examples of a Publisher include a person, an organization, or a service. Typically, the name of a Publisher should be used to indicate the entity.<remarks>
    public string[] Publisher {
      get; set;
    }

    /// <summary>A related resource.</summary>
    /// <remarks>Recommended best practice is to identify the related resource by means of a string conforming to a formal identification system.<remarks>
    public string[] Relation {
      get; set;
    }

    /// <summary>Information about rights held in and over the resource.</summary>
    /// <remarks>Typically, rights information includes a statement about various property rights associated with the resource, including intellectual property rights.<remarks>
    public string[] Rights {
      get; set;
    }

    /// <summary>A related resource from which the described resource is derived.</summary>
    /// <remarks>The described resource may be derived from the related resource in whole or in part. Recommended best practice is to identify the related resource by means of a string conforming to a formal identification system.<remarks>
    public string[] Source {
      get; set;
    }

    /// <summary>The topic of the resource.</summary>
    /// <remarks>Typically, the subject will be represented using keywords, key phrases, or classification codes. Recommended best practice is to use a controlled vocabulary. To describe the spatial or temporal topic of the resource, use the Coverage element.<remarks>
    public string[] Subject {
      get; set;
    }

    /// <summary>A name given to the resource.</summary>
    public string[] Title {
      get; set;
    }

    /// <summary>The nature or genre of the resource.</summary>
    /// <remarks>Recommended best practice is to use a controlled vocabulary such as the DCMI Type Vocabulary [DCMITYPE]. To describe the file format, physical medium, or dimensions of the resource, use the Format element.<remarks>
    public string[] Type {
      get; set;
    }

    internal protected override void Parse(FeedBase feed, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      Parse(parent, nsmgr);
    }

    internal protected override void Parse(EntryBase entry, XmlNode parent, XmlNamespaceManager nsmgr)
    {
      Parse(parent, nsmgr);
    }

    private void Parse(XmlNode parent, XmlNamespaceManager nsmgr)
    {
      try {
        nsmgr.PushScope();
        nsmgr.AddNamespace("dc", NamespaceUri);

        Contributor = parent.GetNodeValuesOf("dc:contributor/text()", nsmgr).ToArray();
        Coverage    = parent.GetNodeValuesOf("dc:coverage/text()", nsmgr).ToArray();
        Creator     = parent.GetNodeValuesOf("dc:creator/text()", nsmgr).ToArray();
        Date        = parent.GetNodeValuesOf<DateTimeOffset?>("dc:date/text()", nsmgr, DateTimeConvert.FromW3CDateTimeOffsetStringNullable).ToArray();
        Description = parent.GetNodeValuesOf("dc:description/text()", nsmgr).ToArray();
        FileFormat  = parent.GetNodeValuesOf("dc:format/text()", nsmgr).ToArray();
        Identifier  = parent.GetNodeValuesOf("dc:identifier/text()", nsmgr).ToArray();
        Language    = parent.GetNodeValuesOf("dc:language/text()", nsmgr).ToArray();
        Publisher   = parent.GetNodeValuesOf("dc:publisher/text()", nsmgr).ToArray();
        Relation    = parent.GetNodeValuesOf("dc:relation/text()", nsmgr).ToArray();
        Rights      = parent.GetNodeValuesOf("dc:rights/text()", nsmgr).ToArray();
        Source      = parent.GetNodeValuesOf("dc:source/text()", nsmgr).ToArray();
        Subject     = parent.GetNodeValuesOf("dc:subject/text()", nsmgr).ToArray();
        Title       = parent.GetNodeValuesOf("dc:title/text()", nsmgr).ToArray();
        Type        = parent.GetNodeValuesOf("dc:type/text()", nsmgr).ToArray();
      }
      finally {
        nsmgr.PopScope();
      }
    }

    internal protected override void Format(FeedBase feed, XmlNode parent)
    {
      Format(parent);
    }

    internal protected override void Format(EntryBase entry, XmlNode parent)
    {
      Format(parent);
    }

    private void Format(XmlNode parent)
    {
      foreach (var pair in new[] {
        new {LocalName = "contributor", Values = Contributor},
        new {LocalName = "coverage",    Values = Coverage},
        new {LocalName = "creator",     Values = Creator},
        new {LocalName = "date",        Values = Array.ConvertAll<DateTimeOffset?, string>(Date, DateTimeConvert.ToW3CDateTimeStringNullable)},
        new {LocalName = "description", Values = Description},
        new {LocalName = "format",      Values = FileFormat},
        new {LocalName = "identifier",  Values = Identifier},
        new {LocalName = "language",    Values = Language},
        new {LocalName = "publisher",   Values = Publisher},
        new {LocalName = "relation",    Values = Relation},
        new {LocalName = "rights",      Values = Rights},
        new {LocalName = "source",      Values = Source},
        new {LocalName = "subject",     Values = Subject},
        new {LocalName = "title",       Values = Title},
        new {LocalName = "type",        Values = Type},
      }) {
        if (pair.Values == null || pair.Values.Length == 0)
          continue;

        foreach (var val in pair.Values) {
          parent.AppendChild(parent.OwnerDocument.CreateElement(Prefix, pair.LocalName, NamespaceUri))
                .AppendChild(parent.OwnerDocument.CreateTextNode(val));
        }
      }
    }
  }
}