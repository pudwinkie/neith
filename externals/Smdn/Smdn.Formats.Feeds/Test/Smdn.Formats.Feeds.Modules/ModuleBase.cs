using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Smdn.Formats.Feeds.Modules {
  //[TestFixture]
  public class ModuleBaseTest {
    public static RdfRss.Channel CreateTestRdfRssChannel()
    {
      var channel = new RdfRss.Channel();

      channel.Uri = new Uri("http://example.com/feed.xml");
      channel.Description = "description";
      channel.Link = new Uri("http://example.com/index.html");
      channel.Title = "title";

      channel.Items.Add(new RdfRss.Item());
      channel.Items[0].Description = "description";
      channel.Items[0].Link = new Uri("http://example.com/entry0/");
      channel.Items[0].Resource = new Uri("http://example.com/entry0/");
      channel.Items[0].Title = "entry0";

      return channel;
    }

    public static Atom.Feed CreateTestAtomFeed()
    {
      var feed = new Atom.Feed();

      feed.Authors.Add(new Atom.Person("author1", new Uri("http://localhost/"), "author1@localhost"));
      feed.Links.Add(new Atom.Link("http://localhost/index.html"));
      feed.Title = "atom 1.0";
      feed.Updated = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);
      feed.Id = new Uri("tag:author@localhost,2008-02:author");

      var entry = new Atom.Entry();

      entry.Authors.Add(new Atom.Person("author1"));
      entry.Content = new Atom.Content("<div><p>content</p></div>", "xhtml");
      entry.Links.Add(new Atom.Link("http://localhost/entry1"));
      entry.Published = new DateTime(2008, 2, 25, 14, 1, 12, DateTimeKind.Utc);
      entry.Title = "entry1";
      entry.Updated = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);
      entry.Id = new Uri("http://localhost/;id=1");

      feed.Entries.Add(entry);

      return feed;
    }

    public static Rss.Channel CreateTestRssChannel()
    {
      var channel = new Rss.Channel();

      channel.Description = "description";
      channel.Link = new Uri("http://example.com/index.html");
      channel.Title = "title";

      channel.Items.Add(new Rss.Item());
      channel.Items[0].Description = "description";
      channel.Items[0].Link = new Uri("http://example.com/entry0/");
      channel.Items[0].Title = "entry0";

      return channel;
    }
  }
}