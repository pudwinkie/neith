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
using System.IO;
using System.Xml;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  public static class Parser {
    public static IFeed Parse(string document)
    {
      return Parse(document, true, null);
    }

    public static IFeed Parse(string document, bool discardSourceXml)
    {
      return Parse(document, discardSourceXml, null);
    }

    public static IFeed Parse(string document, EntryHashAlgorithm hasher)
    {
      return Parse(document, true, hasher);
    }

    public static IFeed Parse(string document, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      return Parse(new StringReader(document), discardSourceXml, hasher);
    }

    public static IFeed Parse(TextReader reader)
    {
      return Parse(reader, true, null);
    }

    public static IFeed Parse(TextReader reader, bool discardSourceXml)
    {
      return Parse(reader, discardSourceXml, null);
    }

    public static IFeed Parse(TextReader reader, EntryHashAlgorithm hasher)
    {
      return Parse(reader, true, hasher);
    }

    public static IFeed Parse(TextReader reader, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      var xml = new XmlDocument();

      try {
        xml.Load(reader);
      }
      catch (XmlException ex) {
        throw new FeedFormatException("invalid xml", ex);
      }

      return Parse(xml, discardSourceXml, hasher);
    }

    public static IFeed Parse(Stream stream)
    {
      return Parse(stream, true, null);
    }

    public static IFeed Parse(Stream stream, EntryHashAlgorithm hasher)
    {
      return Parse(stream, true, hasher);
    }

    public static IFeed Parse(Stream stream, bool discardSourceXml)
    {
      return Parse(stream, discardSourceXml, null);
    }

    public static IFeed Parse(Stream stream, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      var xml = new XmlDocument();

      try {
        xml.Load(stream);
      }
      catch (XmlException ex) {
        throw new FeedFormatException("invalid xml", ex);
      }

      return Parse(xml, discardSourceXml, hasher);
    }

    public static IFeed Parse(XmlDocument xml)
    {
      return Parse(xml, true, null);
    }

    public static IFeed Parse(XmlDocument xml, bool discardSourceXml)
    {
      return Parse(xml, discardSourceXml, null);
    }

    public static IFeed Parse(XmlDocument xml, EntryHashAlgorithm hasher)
    {
      return Parse(xml, true, hasher);
    }

    public static IFeed Parse(XmlDocument xml, bool discardSourceXml, EntryHashAlgorithm hasher)
    {
      if (xml == null)
        throw new ArgumentNullException("xml");

      var version = DetermineVersion(xml);
      ParserCore parser;

      switch (version) {
        case FeedVersion.Rss091:
        case FeedVersion.Rss092:
        case FeedVersion.Rss093:
        case FeedVersion.Rss094:
        case FeedVersion.Rss20:
          parser = new Rss.ParserImpl(version);
          break;

        case FeedVersion.Rss10:
          parser = new RdfRss.ParserImpl(version);
          break;

        case FeedVersion.Atom03:
        case FeedVersion.Atom10:
          parser = new Atom.ParserImpl(version);
          break;

        default:
          parser = null;
          break;
      }

      if (parser == null)
        throw new VersionNotSupportedException("unsupported feed version");

      return parser.Parse(xml, discardSourceXml, hasher);
    }

    private static FeedVersion DetermineVersion(XmlDocument xml)
    {
      var root = xml.DocumentElement;

      if (string.Equals(root.LocalName, "rss", StringComparison.Ordinal) && string.IsNullOrEmpty(root.NamespaceURI)) {
        if (root.Attributes["version"] == null)
          return FeedVersion.Unknown;

        // RSS
        switch (root.Attributes["version"].Value) {
          case "0.91":
            return FeedVersion.Rss091;
          case "0.92":
            return FeedVersion.Rss092;
          case "0.93":
            return FeedVersion.Rss093;
          case "0.94":
            return FeedVersion.Rss094;
          case "2.0":
            return FeedVersion.Rss20;
          default:
            return FeedVersion.Unknown;
        }
      }
      else if (string.Equals(root.LocalName, "RDF", StringComparison.Ordinal) && root.NamespaceURI == FeedNamespaces.Rdf) {
        // RDF
        var nsmgr = new XmlNamespaceManager(xml.NameTable);

        try {
          nsmgr.PushScope();
          nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);
          nsmgr.AddNamespace("rss", FeedNamespaces.Rss_1_0);

          if (root.SelectSingleNode("/rdf:RDF/rss:channel", nsmgr) == null)
            return FeedVersion.Unknown;
          else
            // RDF-RSS
            return FeedVersion.Rss10;
        }
        finally {
          nsmgr.PopScope();
        }
      }
      else if (string.Equals(root.LocalName, "feed", StringComparison.Ordinal)) {
        if (root.NamespaceURI == FeedNamespaces.Atom_1_0) {
          // Atom 1.0
          return FeedVersion.Atom10;
        }
        else if (root.NamespaceURI == FeedNamespaces.Atom_0_3) {
          // Atom 0.3?
          switch (root.Attributes["version"].Value) {
            case "0.3":
              // Atom 0.3
              return FeedVersion.Atom03;
            default:
              return FeedVersion.Unknown;
          }
        }
        else {
          return FeedVersion.Unknown;
        }
      }

      return FeedVersion.Unknown;
    }
  }
}
