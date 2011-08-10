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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Smdn.Formats.Feeds.Modules;
using Smdn.Xml;

namespace Smdn.Formats.Feeds {
  public class GenericFeed : FeedBase, IFeed {
    public override MimeType MimeType {
      get { return FeedMimeTypes.Xml; }
    }

    public string Title {
      get; set;
    }

    public string Description {
      get; set;
    }

    public Atom.Link SiteLink {
      get { return siteLink; }
    }

    public Atom.Link FeedLink {
      get { return feedLink; }
    }

    public Atom.Link AlternativeLink {
      get { return alternativeLink; }
    }

    public Atom.Person Author {
      get; set;
    }

    public Atom.Generator Generator {
      get; set;
    }

    public DateTimeOffset? Updated {
      get; set;
    }

    public List<GenericEntry> Entries {
      get { return entries; }
    }

    Uri IFeed.Uri {
      get
      {
        if (FeedLink == null)
          return null;
        else
          return FeedLink.Href;
      }
      set
      {
        throw new NotSupportedException("readonly");
      }
    }

    Uri IFeed.Link {
      get
      {
        if (siteLink == null)
          return null;
        else
          return siteLink.Href;
      }
      set
      {
        throw new NotSupportedException("readonly");
      }
    }

    DateTimeOffset? IFeed.Date {
      get { return Updated; }
      set { Updated = value; }
    }

    IEnumerable<IEntry> IFeed.Entries {
      get
      {
        return entries.ConvertAll(delegate(GenericEntry e) {
          return (IEntry)e;
        });
        //return entries; // cannot implicity convert type
      }
    }

    IEnumerable<ModuleBase> IFeed.Modules {
      get { return base.Modules.Values; }
    }

    public GenericFeed(Atom.Link siteLink, Atom.Link feedLink, Atom.Link alternativeLink)
      : this(siteLink, feedLink, alternativeLink, new GenericEntry[] {})
    {
    }

    public GenericFeed(Atom.Link siteLink, Atom.Link feedLink, Atom.Link alternativeLink, IEnumerable<GenericEntry> entries)
    {
      if (siteLink == null)
        throw new ArgumentNullException("siteLink");
      else if (!Atom.Link.IsRelatedLink(siteLink))
        throw new ArgumentException(string.Format("rel of siteLink must be the value 'related', but was '{0}'", siteLink.Rel), "siteLink");
      else if (siteLink.Href == null)
        throw new ArgumentException("href of siteLink is null");

      if (feedLink == null)
        throw new ArgumentNullException("feedLink");
      else if (!Atom.Link.IsSelfLink(feedLink))
        throw new ArgumentException(string.Format("rel of feedLink must be the value 'self', but was '{0}'", feedLink.Rel), "feedLink");
      else if (feedLink.Href == null)
        throw new ArgumentException("href of feedLink is null");

      if (alternativeLink == null)
        throw new ArgumentNullException("alternativeLink");
      else if (!Atom.Link.IsAlternativeLink(alternativeLink))
        throw new ArgumentException(string.Format("rel of alternativeLink must be the value of null or 'alternate', but was '{0}'", alternativeLink.Rel), "alternativeLink");
      else if (alternativeLink.Href == null)
        throw new ArgumentException("href of alternativeLink is null");

      this.Title = null;
      this.Description = null;
      this.siteLink = siteLink;
      this.feedLink = feedLink;
      this.alternativeLink = alternativeLink;
      this.Author = null;
      this.Generator = null;
      this.Updated = null;
      this.entries = new List<GenericEntry>(entries);
    }

    public IEntry FindEntryByHash(byte[] hash)
    {
      throw new NotSupportedException();
    }

    public virtual IFeed ConvertTo(FeedVersion feedType)
    {
      feedType = feedType & FeedVersion.TypeMask;

      if (feedType == FeedVersion.Atom) {
        var feed = new Atom.Feed(entries.ConvertAll(delegate(GenericEntry e) {
          return (Atom.Entry)e.ConvertTo(feedType);
        }));

        foreach (var module in Modules) {
          feed.Modules.Add(module.Key, module.Value);
        }

        var atomFeedLink = feedLink.Clone();

        atomFeedLink.Type = feed.MimeType;

        feed.Title = Title;
        feed.Subtitle = Description;
        feed.Links.Add(atomFeedLink);
        feed.Links.Add(siteLink);
        feed.Links.Add(alternativeLink);
        if (this.Author != null) feed.Authors.Add(Author);
        feed.Id = feedLink.Href;
        feed.Updated = Updated;
        feed.Generator = Generator;

        return feed;
      }
      else if (feedType == FeedVersion.RdfRss) {
        var channel = new RdfRss.Channel(entries.ConvertAll(delegate(GenericEntry e) {
          return (RdfRss.Item)e.ConvertTo(feedType);
        }));

        foreach (var module in Modules) {
          channel.Modules.Add(module.Key, module.Value);
        }

        channel.Title = Title;
        if (Description != null) {
          if (!channel.Modules.ContainsKey(DublinCore.NamespaceUri))
            channel.Modules.Add(DublinCore.NamespaceUri, new DublinCore());
          channel.Description = Description;
        }
        channel.Uri = feedLink.Href;
        channel.Link = alternativeLink.Href;
        if (Updated != null) {
          if (!channel.Modules.ContainsKey(DublinCore.NamespaceUri))
            channel.Modules.Add(DublinCore.NamespaceUri, new DublinCore());
          channel.DublinCoreModule.SetDate(Updated);
        }
        if (Author != null) {
          if (!channel.Modules.ContainsKey(DublinCore.NamespaceUri))
            channel.Modules.Add(DublinCore.NamespaceUri, new DublinCore());
          channel.DublinCoreModule.Creator = new[] {string.Format("{0} (mailto:{1})", Author.Name, Author.EMail)};
        }

        return channel;
      }
      else if (feedType == FeedVersion.Rss) {
        var channel = new Rss.Channel(entries.ConvertAll(delegate(GenericEntry e) {
          return (Rss.Item)e.ConvertTo(feedType);
        }));

        foreach (var module in this.Modules) {
          channel.Modules.Add(module.Key, module.Value);
        }

        channel.Title = Title;
        channel.Description = Description;
        channel.Link = alternativeLink.Href;
        channel.LastBuildDate = Updated;
        channel.PubDate = Updated;
        if (Author != null)
          channel.WebMaster = string.Format("{0} ({1})", Author.EMail, Author.Name);
        if (this.Generator != null)
          channel.Generator = string.Concat(Generator.Value, "/", Generator.Version);

        return channel;
      }
      else {
        throw new VersionNotSupportedException("unsupported or invalid version");
      }
    }

    protected override void Format(XmlDocument document)
    {
      throw new NotSupportedException();
    }

#region "save"
    public void Save(FeedVersion feedType, string file)
    {
      Save(feedType, file, Encoding.UTF8);
    }

    public void Save(FeedVersion feedType, string file, Encoding encoding)
    {
      Save(feedType, file, CreateWriterDefaultSettings(encoding));
    }

    public void Save(FeedVersion feedType, string file, XmlWriterSettings settings)
    {
      using (var stream = File.OpenWrite(file)) {
        stream.SetLength(0L);

        Save(feedType, stream, settings);
      }
    }

    public void Save(FeedVersion feedType, Stream stream)
    {
      Save(feedType, stream, Encoding.UTF8);
    }

    public void Save(FeedVersion feedType, Stream stream, Encoding encoding)
    {
      Save(feedType, stream, CreateWriterDefaultSettings(encoding));
    }

    public void Save(FeedVersion feedType, Stream stream, XmlWriterSettings settings)
    {
      ToXmlDocument(feedType, settings.Encoding).WriteTo(stream, settings);
    }

    public void Save(FeedVersion feedType, TextWriter writer)
    {
      Save(feedType, writer, Encoding.UTF8);
    }

    public void Save(FeedVersion feedType, TextWriter writer, Encoding encoding)
    {
      Save(feedType, writer, CreateWriterDefaultSettings(encoding));
    }

    public void Save(FeedVersion feedType, TextWriter writer, XmlWriterSettings settings)
    {
      ToXmlDocument(feedType, settings.Encoding).WriteTo(writer, settings);
    }

    public void Save(FeedVersion feedType, XmlWriter writer)
    {
      ToXmlDocument(feedType, writer.Settings.Encoding ?? Encoding.UTF8).WriteTo(writer);

      writer.Flush();
    }
#endregion

    public XmlDocument ToXmlDocument(FeedVersion feedType)
    {
      return ToXmlDocument(feedType, Encoding.UTF8);
    }

    public virtual XmlDocument ToXmlDocument(FeedVersion feedType, Encoding encoding)
    {
      var f = ConvertTo(feedType);
      var xml = f.ToXmlDocument(encoding);

      if (f is Atom.Feed)
        return xml;

      var formatter = new Atom.FormatterImpl();
      XmlNode insertBeforeNode = null;

      if (f is Rss.Channel) {
        insertBeforeNode = xml.SelectSingleNode("/rss/channel/item");
      }
      else if (f is RdfRss.Channel) {
        var nsmgr = new XmlNamespaceManager(xml.NameTable);

        nsmgr.PushScope();
        nsmgr.AddNamespace("rss", FeedNamespaces.Rss_1_0);
        nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

        insertBeforeNode = xml.SelectSingleNode("/rdf:RDF/rss:channel/rss:items", nsmgr);

        nsmgr.PopScope();
      }

      xml.DocumentElement.SetAttribute("xmlns:atom", Feeds.FeedNamespaces.Atom_1_0);

      if (FeedLink != null) {
        var feedLink = FeedLink.Clone();

        feedLink.Type = f.MimeType;

        formatter.FormatLink((XmlElement)insertBeforeNode.ParentNode.InsertBefore(xml.CreateElement("atom:link", Feeds.FeedNamespaces.Atom_1_0), insertBeforeNode),
                             feedLink);
      }

      if (SiteLink != null)
        formatter.FormatLink((XmlElement)insertBeforeNode.ParentNode.InsertBefore(xml.CreateElement("atom:link", Feeds.FeedNamespaces.Atom_1_0), insertBeforeNode),
                             SiteLink);

      return xml;
    }

    public string ToString(FeedVersion feedType)
    {
      using (var writer = new StringWriter(new StringBuilder(4096))) {
        Save(feedType, writer);

        return writer.ToString();
      }
    }

    private /*readonly*/ Atom.Link siteLink;
    private /*readonly*/ Atom.Link feedLink;
    private /*readonly*/ Atom.Link alternativeLink;
    private /*readonly*/ List<GenericEntry> entries;
  }
}
