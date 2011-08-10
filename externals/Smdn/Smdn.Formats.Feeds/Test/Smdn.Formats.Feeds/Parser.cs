using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using NUnit.Framework;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class ParserTest {
    private EntryHashAlgorithm<MD5> hasher = null;

    [SetUp]
    public void Setup()
    {
      // nothing to do
      hasher = new EntryHashAlgorithm<MD5>();
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
      hasher.Clear();
      hasher = null;
    }

    [Test]
    public void TestParseRss10()
    {
      RdfRss.Channel channel;

      using (var reader = new StreamReader("Rss10Example.xml")) {
        channel = Parser.Parse(reader, hasher) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNull(channel.SourceNode);
      Assert.AreEqual("Meerkat", channel.Title);
      Assert.AreEqual("Meerkat: An Open Wire Service", channel.Description);
      Assert.AreEqual(new Uri("http://meerkat.oreillynet.com"), channel.Link);

      Assert.IsFalse(channel.DublinCoreModule == DublinCore.Null);
      Assert.IsFalse(channel.SyndicationModule == Syndication.Null);

      Assert.AreEqual(1, channel.Items.Count, "item count");

      Assert.IsNull(channel.Items[0].SourceNode);
      Assert.AreEqual("XML: A Disruptive Technology", channel.Items[0].Title);
      Assert.AreEqual(new Uri("http://c.moreover.com/click/here.pl?r123"),
                      channel.Items[0].Link);

      Assert.IsFalse(channel.Items[0].DublinCoreModule == DublinCore.Null);

      Assert.IsNotNull(channel.Items[0].Hash);
      Assert.AreEqual(channel.Items[0], channel.FindEntryByHash(channel.Items[0].Hash));
    }

    [Test]
    public void TestParseRss10KeepSourceXml()
    {
      RdfRss.Channel channel;

      using (var reader = new StreamReader("Rss10Example.xml")) {
        channel = Parser.Parse(reader, false, hasher) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNotNull(channel.SourceNode);

      Assert.AreEqual(1, channel.Items.Count, "item count");
      Assert.IsNotNull(channel.Items[0].SourceNode);
    }

    [Test]
    public void TestParseRss091()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss091Example.xml")) {
        channel = Parser.Parse(reader, hasher) as Rss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNull(channel.SourceNode);
      Assert.AreEqual("WriteTheWeb", channel.Title);
      Assert.AreEqual(new Uri("http://writetheweb.com"), channel.Link);
      Assert.AreEqual("News for web users that write back", channel.Description);
      Assert.AreEqual("en-us", channel.Language);
      Assert.AreEqual("Copyright 2000, WriteTheWeb team.", channel.Copyright);
      Assert.AreEqual("editor@writetheweb.com", channel.ManagingEditor);
      Assert.AreEqual("webmaster@writetheweb.com", channel.WebMaster);

      Assert.AreEqual("WriteTheWeb", channel.Image.Title);
      Assert.AreEqual(new Uri("http://writetheweb.com/images/mynetscape88.gif"), channel.Image.Url);
      Assert.AreEqual(new Uri("http://writetheweb.com"), channel.Image.Link);
      Assert.AreEqual(88, channel.Image.Width);
      Assert.AreEqual(31, channel.Image.Height);
      Assert.AreEqual("News for web users that write back", channel.Image.Description);

      Assert.AreEqual(6, channel.Items.Count, "item count");

      Assert.IsNull(channel.Items[0].SourceNode);
      Assert.AreEqual("Giving the world a pluggable Gnutella", channel.Items[0].Title);
      Assert.AreEqual(new Uri("http://writetheweb.com/read.php?item=24"),
                      channel.Items[0].Link);
      Assert.AreEqual("WorldOS is a framework on which to build programs that work like Freenet or Gnutella -allowing distributed applications using peer-to-peer routing.",
                      channel.Items[0].Description);

      Assert.IsNull(channel.Items[1].SourceNode);
      Assert.AreEqual("Syndication discussions hot up", channel.Items[1].Title);
      Assert.AreEqual(new Uri("http://writetheweb.com/read.php?item=23"),
                      channel.Items[1].Link);
      Assert.AreEqual("After a period of dormancy, the Syndication mailing list has become active again, with contributions from leaders in traditional media and Web syndication.",
                      channel.Items[1].Description);

      Assert.IsNotNull(channel.Items[0].Hash);
      Assert.AreEqual(channel.Items[0], channel.FindEntryByHash(channel.Items[0].Hash));
    }

    [Test]
    public void TestParseRss092()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss092Example.xml")) {
        channel = Parser.Parse(reader, hasher) as Rss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNull(channel.SourceNode);
      Assert.AreEqual("Dave Winer: Grateful Dead", channel.Title);
      Assert.AreEqual(new Uri("http://www.scripting.com/blog/categories/gratefulDead.html"), channel.Link);
      Assert.AreEqual("A high-fidelity Grateful Dead song every day. This is where we're experimenting with enclosures on RSS news items that download when you're not using your computer. If it works (it will) it will be the end of the Click-And-Wait multimedia experience on the Internet. ", channel.Description);
      Assert.AreEqual(new DateTimeOffset(2001, 04, 13, 19, 23, 2, TimeSpan.Zero),
                      channel.LastBuildDate);
      Assert.AreEqual(new Uri("http://backend.userland.com/rss092"),
                      channel.Docs);
      Assert.AreEqual("dave@userland.com (Dave Winer)", channel.ManagingEditor);
      Assert.AreEqual("dave@userland.com (Dave Winer)", channel.WebMaster);
      Assert.AreEqual(null, channel.Cloud); // TODO

      Assert.AreEqual(22, channel.Items.Count, "item count");

      Assert.IsNull(channel.Items[0].SourceNode);
      Assert.AreEqual(new Uri("http://www.scripting.com/mp3s/weatherReportDicksPicsVol7.mp3"),
                      channel.Items[0].Enclosure.Url);
      Assert.AreEqual(6182912, channel.Items[0].Enclosure.Length);
      Assert.AreEqual("audio/mpeg", channel.Items[0].Enclosure.Type);

      Assert.IsNotNull(channel.Items[0].Hash);
      Assert.AreEqual(channel.Items[0], channel.FindEntryByHash(channel.Items[0].Hash));
    }

    [Test]
    public void TestParseRss093()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss093Example.xml")) {
        channel = Parser.Parse(reader, hasher) as Rss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNull(channel.SourceNode);
      Assert.AreEqual("Example Channel", channel.Title);
      Assert.AreEqual(new Uri("http://example.com/"), channel.Link);
      Assert.AreEqual("an example feed", channel.Description);
      Assert.AreEqual("en", channel.Language);
      Assert.AreEqual("(PICS-1.1 \"http://www.classify.org/safesurf/\" l r (SS~~000 1))", channel.Rating);
      // ignore expirationDate

      Assert.AreEqual("Search this site:", channel.TextInput.Title);
      Assert.AreEqual("Find:", channel.TextInput.Description);
      Assert.AreEqual("q", channel.TextInput.Name);
      Assert.AreEqual(new Uri("http://example.com/search"), channel.TextInput.Link);

      Assert.AreEqual(1, channel.SkipHours.Count, "skip hours");
      Assert.AreEqual(24, channel.SkipHours[0]);

      Assert.AreEqual(1, channel.Items.Count);

      Assert.IsNull(channel.Items[0].SourceNode);

      Assert.IsNotNull(channel.Items[0].Hash);
      Assert.AreEqual(channel.Items[0], channel.FindEntryByHash(channel.Items[0].Hash));
    }

    [Test]
    public void TestParseRss094()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss094Example.xml")) {
        channel = Parser.Parse(reader, hasher) as Rss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNull(channel.SourceNode);

      Assert.AreEqual(1, channel.Items.Count);

      Assert.IsNull(channel.Items[0].SourceNode);
    }

    [Test]
    public void TestParseRss20()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss20Example.xml")) {
        channel = Parser.Parse(reader, hasher) as Rss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNull(channel.SourceNode);
      Assert.AreEqual("Liftoff News", channel.Title);
      Assert.AreEqual(new Uri("http://liftoff.msfc.nasa.gov/"), channel.Link);
      Assert.AreEqual("Liftoff to Space Exploration.", channel.Description);
      Assert.AreEqual("en-us", channel.Language);
      Assert.AreEqual(new DateTimeOffset(2003, 06, 10, 4, 0, 0, TimeSpan.Zero),
                      channel.PubDate);
      Assert.AreEqual(new DateTimeOffset(2003, 06, 10, 9, 41, 1, TimeSpan.Zero),
                      channel.LastBuildDate);
      Assert.AreEqual("Weblog Editor 2.0", channel.Generator);
      Assert.AreEqual("editor@example.com", channel.ManagingEditor);
      Assert.AreEqual("webmaster@example.com", channel.WebMaster);

      Assert.AreEqual(4, channel.Items.Count, "item count");

      Assert.IsNull(channel.Items[0].SourceNode);
      Assert.AreEqual("Star City", channel.Items[0].Title);
      Assert.AreEqual(new Uri("http://liftoff.msfc.nasa.gov/news/2003/news-starcity.asp"),
                      channel.Items[0].Link);
      Assert.AreEqual("How do Americans get ready to work with Russians aboard the International Space Station? They take a crash course in culture, language and protocol at Russia's <a href=\"http://howe.iki.rssi.ru/GCTC/gctc_e.htm\">Star City</a>.",
                      channel.Items[0].Description);
      Assert.AreEqual(new DateTimeOffset(2003, 06, 03, 9, 39, 21, TimeSpan.Zero),
                      channel.Items[0].PubDate);
      Assert.AreEqual(new Rss.Guid("http://liftoff.msfc.nasa.gov/2003/06/03.html#item573"),
                      channel.Items[0].Guid);

      Assert.IsNotNull(channel.Items[0].Hash);
      Assert.AreEqual(channel.Items[0], channel.FindEntryByHash(channel.Items[0].Hash));

      Assert.IsNull(channel.Items[1].SourceNode);
      Assert.IsNull(channel.Items[1].Title);
      Assert.IsNull(channel.Items[1].Link);
      Assert.AreEqual("Sky watchers in Europe, Asia, and parts of Alaska and Canada will experience a <a href=\"http://science.nasa.gov/headlines/y2003/30may_solareclipse.htm\">partial eclipse of the Sun</a> on Saturday, May 31st.",
                      channel.Items[1].Description);
      Assert.AreEqual(new DateTimeOffset(2003, 05, 30, 11, 06, 42, TimeSpan.Zero),
                      channel.Items[1].PubDate);
      Assert.AreEqual(new Rss.Guid("http://liftoff.msfc.nasa.gov/2003/05/30.html#item572"),
                      channel.Items[1].Guid);

      Assert.IsNotNull(channel.Items[1].Hash);
      Assert.AreEqual(channel.Items[1], channel.FindEntryByHash(channel.Items[1].Hash));
    }

    [Test]
    public void TestParseRss20KeepSourceXml()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss20Example.xml")) {
        channel = Parser.Parse(reader, false, hasher) as Rss.Channel;
      }

      Assert.IsNotNull(channel);
      Assert.IsNotNull(channel.SourceNode);

      Assert.AreEqual(4, channel.Items.Count, "item count");
      Assert.IsNotNull(channel.Items[0].SourceNode);
    }

    [Test]
    public void TestParseAtom03()
    {
      Atom.Feed feed;

      using (var reader = new StreamReader("Atom03Example.xml")) {
        feed = Parser.Parse(reader, hasher) as Atom.Feed;
      }

      Assert.IsNotNull(feed);
      Assert.IsNull(feed.SourceNode);

      Assert.AreEqual("dive into mark", (string)feed.Title);
      Assert.AreEqual(1, feed.Links.Count);
      Assert.AreEqual("alternate", feed.Links[0].Rel);
      Assert.AreEqual(new MimeType("text/html"), feed.Links[0].Type);
      Assert.AreEqual(new Uri("http://diveintomark.org/"), feed.Links[0].Href);
      Assert.AreEqual(1, feed.Authors.Count);
      Assert.AreEqual("Mark Pilgrim", feed.Authors[0].Name);

      Assert.AreEqual(1, feed.Entries.Count);
      Assert.IsNull(feed.Entries[0].SourceNode);
      Assert.AreEqual("Atom 0.3 snapshot", (string)feed.Entries[0].Title);
      Assert.AreEqual(1, feed.Entries[0].Links.Count);
      Assert.AreEqual("alternate", feed.Entries[0].Links[0].Rel);
      Assert.AreEqual(new MimeType("text/html"), feed.Entries[0].Links[0].Type);
      Assert.AreEqual(new Uri("http://diveintomark.org/2003/12/13/atom03"), feed.Entries[0].Links[0].Href);
      Assert.AreEqual(new Uri("tag:diveintomark.org,2003:3.2397"), feed.Entries[0].Id);

      Assert.IsNotNull(feed.Entries[0].Hash);
      Assert.AreEqual(feed.Entries[0], feed.FindEntryByHash(feed.Entries[0].Hash));
    }

    [Test]
    public void TestParseAtom10()
    {
      Atom.Feed feed;

      using (var reader = new StreamReader("Atom10Example.xml")) {
        feed = Parser.Parse(reader, hasher) as Atom.Feed;
      }

      Assert.IsNotNull(feed);
      Assert.IsNull(feed.SourceNode);

      Assert.AreEqual(new Atom.Text("dive into mark"), feed.Title);
      Assert.AreEqual(new Atom.Text("\n    A <em>lot</em> of effort\n" +
                                    "    went into making this effortless\n" +
                                    "  ",
                                    Atom.TextType.Html),
                      feed.Subtitle);
      Assert.AreEqual(new DateTimeOffset(2005, 07, 31, 12, 29, 29, TimeSpan.Zero),
                      feed.Updated);
      Assert.AreEqual(new Uri("tag:example.org,2003:3"), feed.Id);
      Assert.AreEqual(2, feed.Links.Count);
      Assert.AreEqual("alternate", feed.Links[0].Rel);
      Assert.AreEqual(new MimeType("text/html"), feed.Links[0].Type);
      Assert.AreEqual("en", feed.Links[0].HrefLang);
      Assert.AreEqual(new Uri("http://example.org/"), feed.Links[0].Href);
      Assert.AreEqual("self", feed.Links[1].Rel);
      Assert.AreEqual(new MimeType("application/atom+xml"), feed.Links[1].Type);
      Assert.AreEqual(new Uri("http://example.org/feed.atom"), feed.Links[1].Href);
      Assert.AreEqual(Atom.TextType.Text, feed.Rights.Type);
      Assert.AreEqual("Copyright (c) 2003, Mark Pilgrim", feed.Rights.Value);
      Assert.AreEqual(new Uri("http://www.example.com/"), feed.Generator.Uri);
      Assert.AreEqual("1.0", feed.Generator.Version);
      Assert.AreEqual("Example Toolkit", feed.Generator.Value.Trim());

      Assert.AreEqual(1, feed.Entries.Count);
      Assert.IsNull(feed.Entries[0].SourceNode);
      Assert.AreEqual("Atom draft-07 snapshot", (string)feed.Entries[0].Title);
      Assert.AreEqual(2, feed.Entries[0].Links.Count);
      Assert.AreEqual("alternate", feed.Entries[0].Links[0].Rel);
      Assert.AreEqual(new MimeType("text/html"), feed.Entries[0].Links[0].Type);
      Assert.AreEqual(new Uri("http://example.org/2005/04/02/atom"), feed.Entries[0].Links[0].Href);
      Assert.AreEqual("enclosure", feed.Entries[0].Links[1].Rel);
      Assert.AreEqual(new MimeType("audio/mpeg"), feed.Entries[0].Links[1].Type);
      Assert.AreEqual(1337, feed.Entries[0].Links[1].Length);
      Assert.AreEqual(new Uri("http://example.org/audio/ph34r_my_podcast.mp3"), feed.Entries[0].Links[1].Href);
      Assert.AreEqual(new Uri("tag:example.org,2003:3.2397"), feed.Entries[0].Id);
      Assert.AreEqual(new DateTimeOffset(2005, 07, 31, 12, 29, 29, TimeSpan.Zero),
                      feed.Entries[0].Updated);
      Assert.AreEqual(new DateTimeOffset(2003, 12, 13, 8, 29, 29, TimeSpan.FromHours(-4)),
                      feed.Entries[0].Published);
      Assert.AreEqual(1, feed.Entries[0].Authors.Count);
      Assert.AreEqual("Mark Pilgrim", feed.Entries[0].Authors[0].Name);
      Assert.AreEqual(new Uri("http://example.org/"), feed.Entries[0].Authors[0].Uri);
      Assert.AreEqual("f8dy@example.com", feed.Entries[0].Authors[0].EMail);
      Assert.AreEqual(2, feed.Entries[0].Contributors.Count);
      Assert.AreEqual("Sam Ruby", feed.Entries[0].Contributors[0].Name);
      Assert.IsNull(feed.Entries[0].Contributors[0].Uri);
      Assert.IsNull(feed.Entries[0].Contributors[0].EMail);
      Assert.AreEqual("Joe Gregorio", feed.Entries[0].Contributors[1].Name);
      Assert.AreEqual("xhtml", feed.Entries[0].Content.Type.ToLowerInvariant());

      var contentDocument = feed.Entries[0].Content.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(contentDocument.NameTable);
      nsmgr.AddNamespace("x", "http://www.w3.org/1999/xhtml");

      var text = contentDocument.SelectSingleNode("/x:div/x:p/x:i/text()", nsmgr);

      Assert.IsNotNull(text);
      Assert.AreEqual("[Update: The Atom draft is finished.]", text.Value);

      Assert.IsNotNull(feed.Entries[0].Hash);
      Assert.AreEqual(feed.Entries[0], feed.FindEntryByHash(feed.Entries[0].Hash));
    }

    [Test]
    public void TestParseAtom10KeepSourceXml()
    {
      Atom.Feed feed;

      using (var reader = new StreamReader("Atom10Example.xml")) {
        feed = Parser.Parse(reader, false, hasher) as Atom.Feed;
      }

      Assert.IsNotNull(feed);
      Assert.IsNotNull(feed.SourceNode);

      Assert.AreEqual(1, feed.Entries.Count);
      Assert.IsNotNull(feed.Entries[0].SourceNode);
    }

    [Test]
    public void TestParseXmlExceptionWithTextReader()
    {
      var xml = @"<?xml version=""1.1"" encoding=""utf-8""?>";

      using (var reader = new StringReader(xml)) {
        try {
          Parser.Parse(reader);
          Assert.Fail("exception not thrown");
        }
        catch (FeedFormatException ex) {
          if (!(ex.InnerException is XmlException))
            Assert.Fail("invalid exception thrown");
        }
      }
    }

    [Test]
    public void TestParseXmlExceptionWithStream()
    {
      var xml = @"<?xml version=""1.1"" encoding=""utf-8""?>";

      using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
        try {
          Parser.Parse(stream);
          Assert.Fail("exception not thrown");
        }
        catch (FeedFormatException ex) {
          if (!(ex.InnerException is XmlException))
            Assert.Fail("invalid exception thrown");
        }
      }
    }

    [Test, ExpectedException(typeof(VersionNotSupportedException))]
    public void TestParseUnsupportedAtomVersion()
    {
      using (var reader = new StringReader(@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed version=""0.1"" xmlns=""http://purl.org/atom/ns#""/>")) {
        Parser.Parse(reader);
      }
    }

    [Test, ExpectedException(typeof(VersionNotSupportedException))]
    public void TestParseUnsupportedRssVersion()
    {
      using (var reader = new StringReader(@"<?xml version=""1.0""?>
<rss version=""3.0""/>")) {
        Parser.Parse(reader);
      }
    }

    [Test, ExpectedException(typeof(VersionNotSupportedException))]
    public void TestParseUnsupportedRdfRssVersion()
    {
      using (var reader = new StringReader(@"<?xml version=""1.0"" encoding=""utf-8""?> 
<rdf:RDF 
xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
xmlns=""http://purl.org/rss/1.1/""
/>")) {
        Parser.Parse(reader);
      }
    }



    [Test]
    public void TestParse_Example_HatenaBookmark()
    {
      IFeed feed;

      using (var reader = new StreamReader("Example_HatenaBookmark.xml")) {
        feed = Parser.Parse(reader);

        Assert.IsInstanceOfType(typeof(RdfRss.Channel), feed);
      }

      Assert.AreEqual("はてなブックマーク - 新着ブックマーク - smdn.invisiblefulmoon.net", feed.Title);

      var channel = feed as RdfRss.Channel;

      Assert.AreEqual(22, channel.Items.Count);

      var item = channel.Items[1];

      Assert.AreEqual(new DateTimeOffset(2009, 11, 27, 11, 14, 43, TimeSpan.FromHours(+9)), item.DublinCoreModule.Date[0]);

      Assert.IsFalse(DublinCore.Null == item.DublinCoreModule);
      Assert.AreEqual(new[] {"Ubuntu", "Karmic"}, item.DublinCoreModule.Subject);

      Assert.IsFalse(Taxonomy.Null == item.TaxonomyModule);
      Assert.AreEqual(new[] {
                        new Uri("http://b.hatena.ne.jp/t/Ubuntu"),
                        new Uri("http://b.hatena.ne.jp/t/Karmic"),
                      }, item.TaxonomyModule.Topics);
    }
  }
}