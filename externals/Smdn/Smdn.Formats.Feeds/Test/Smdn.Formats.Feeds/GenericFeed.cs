using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class GenericFeedTests {
    private GenericFeed CreateTestFeed()
    {
      var sitelink = Atom.Link.CreateRelated(new Uri("http://localhost/"));
      var feedlink = Atom.Link.CreateSelf(new Uri("http://localhost/feed"));
      var altlink = Atom.Link.CreateAlternative(new Uri("http://localhost/updated"));

      var feed = new GenericFeed(sitelink, feedlink, altlink);

      feed.Title = "title";
      feed.Description = "description";
      feed.Author = new Atom.Person("author", null, "author@localhost");
      feed.Generator = new Atom.Generator("Smdn.Formats.Feeds", null, "1.0");
      feed.Updated = new DateTime(2009, 7, 11, 18, 0, 0, 0, DateTimeKind.Utc);

      feed.Modules.Add(Modules.Syndication.NamespaceUri, new Modules.Syndication());
      feed.SyndicationModule.UpdateBase = feed.Updated;
      feed.SyndicationModule.UpdateFrequency = 2;
      feed.SyndicationModule.UpdatePeriod = Modules.Syndication.Period.Weekly;

      for (var i = 0; i < 2; i++) {
        var entry = new GenericEntry(new Uri(string.Format("http://localhost/{0}", i)));

        entry.Title = string.Format("title{0}", i);
        entry.Description = string.Format("description{0}", i);
        entry.Published = new DateTime(2009, 7, 11, 18, 0, i, 0, DateTimeKind.Utc);

        entry.Modules.Add(Modules.Annotation.NamespaceUri, new Modules.Annotation());

        entry.AnnotationModule.Reference = new Uri(string.Format("http://localhost/annotation{0}", i));

        feed.Entries.Add(entry);
      }

      return feed;
    }

    private string ToString(GenericFeed feed, FeedVersion version)
    {
      using (var writer = new StringWriter(new StringBuilder(1024))) {
        var settings = new XmlWriterSettings();

        settings.Encoding = Encoding.UTF8;
        settings.Indent = true;
        settings.IndentChars = "  ";
        settings.NewLineChars = "\n";

        feed.Save(version, writer, settings);

        return writer.ToString();
      }
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void TestSaveGenericFeed()
    {
      using (var stream = new MemoryStream()) {
        CreateTestFeed().Save(stream);
      }
    }

    [Test]
    public void TestFormatAtom()
    {
      Assert.AreEqual(File.ReadAllText("GenericFeedSampleAtomFormat.xml").TrimEnd(),
                      ToString(CreateTestFeed(), FeedVersion.Atom).TrimEnd());
    }

    [Test]
    public void TestFormatRdfRss()
    {
      Assert.AreEqual(File.ReadAllText("GenericFeedSampleRdfRssFormat.xml").TrimEnd(),
                      ToString(CreateTestFeed(), FeedVersion.RdfRss).TrimEnd());
    }

    [Test]
    public void TestFormatRss()
    {
      Assert.AreEqual(File.ReadAllText("GenericFeedSampleRssFormat.xml").TrimEnd(),
                      ToString(CreateTestFeed(), FeedVersion.Rss).TrimEnd());
    }
  }
}