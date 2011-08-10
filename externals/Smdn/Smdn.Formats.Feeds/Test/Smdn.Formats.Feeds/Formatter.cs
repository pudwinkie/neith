using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class FormatterTest {
    [SetUp]
    public void Setup()
    {
      // nothing to do
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    private Atom.Feed CreateAtom10Feed()
    {
      var feed = new Atom.Feed();

      feed.Authors.Add(new Atom.Person("author1", new Uri("http://localhost/"), "author1@localhost"));
      feed.Authors.Add(new Atom.Person("author2"));
      feed.Categories.Add(new Atom.Category("category1"));
      feed.Categories.Add(new Atom.Category("category2", new Uri("http://localhost/categories"), null));
      feed.Contributors.Add(new Atom.Person("contributor1", null, "contributor1@localhost (contrib1)"));
      feed.Generator = new Atom.Generator("Smdn.Formats.Feeds", new Uri("http://smdn.invisiblefulmoon.net/"), "0.1");
      feed.Icon = new Uri("http://localhost/icon.ico");
      feed.Id = new Uri("tag:author1@localhost,2008-02:author1");
      feed.Links.Add(new Atom.Link("http://localhost/index.html"));
      feed.Links.Add(new Atom.Link("http://localhost/atom.xml", "self", MimeType.CreateApplicationType("atom+xml"), "ja-jp", "atom feed", null));
      feed.Logo = new Uri("http://localhost/logo.png");
      feed.Rights = "copyright(c) localhost";
      feed.Subtitle = new Atom.Text("atom <b>1.0</b> example", Atom.TextType.Html);
      feed.Title = "atom 1.0";
      feed.Updated = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);

      return feed;
    }

    [Test]
    public void TestFormatAtom10Feed()
    {
      var feed = CreateAtom10Feed();
      var xml = feed.ToXmlDocument();
      var nsmgr = new XmlNamespaceManager(xml.NameTable);

      nsmgr.AddNamespace("a", FeedNamespaces.Atom_1_0);

      var nullstr = "(null)";

      foreach (var pathvalue in new [] {
        new {Value = "author1", Path = "/a:feed/a:author[1]/a:name/text()"},
        new {Value = "http://localhost/", Path = "/a:feed/a:author[1]/a:uri/text()"},
        new {Value = "author1@localhost", Path = "/a:feed/a:author[1]/a:email/text()"},
        new {Value = "author2", Path = "/a:feed/a:author[2]/a:name/text()"},
        new {Value = nullstr, Path = "/a:feed/a:author[2]/a:uri"},
        new {Value = nullstr, Path = "/a:feed/a:author[2]/a:email"},
        new {Value = "category1", Path = "/a:feed/a:category[1]/@term"},
        new {Value = nullstr, Path = "/a:feed/a:category[1]/@scheme"},
        new {Value = nullstr, Path = "/a:feed/a:category[1]/@title"},
        new {Value = "category2", Path = "/a:feed/a:category[2]/@term"},
        new {Value = "http://localhost/categories", Path = "/a:feed/a:category[2]/@scheme"},
        new {Value = "contributor1", Path = "/a:feed/a:contributor[1]/a:name/text()"},
        new {Value = nullstr, Path = "/a:feed/a:contributor[1]/a:uri"},
        new {Value = "contributor1@localhost (contrib1)", Path = "/a:feed/a:contributor[1]/a:email/text()"},
        new {Value = "Smdn.Formats.Feeds", Path = "/a:feed/a:generator/text()"},
        new {Value = "http://smdn.invisiblefulmoon.net/", Path = "/a:feed/a:generator/@uri"},
        new {Value = "0.1", Path = "/a:feed/a:generator/@version"},
        new {Value = "http://localhost/icon.ico", Path = "/a:feed/a:icon/text()"},
        new {Value = "tag:author1@localhost,2008-02:author1", Path = "/a:feed/a:id/text()"},
        new {Value = "http://localhost/index.html", Path = "/a:feed/a:link[1]/@href"},
        new {Value = "alternate", Path = "/a:feed/a:link[1]/@rel"},
        new {Value = "http://localhost/atom.xml", Path = "/a:feed/a:link[2]/@href"},
        new {Value = "self", Path = "/a:feed/a:link[2]/@rel"},
        new {Value = "application/atom+xml", Path = "/a:feed/a:link[2]/@type"},
        new {Value = "ja-jp", Path = "/a:feed/a:link[2]/@hreflang"},
        new {Value = nullstr, Path = "/a:feed/a:link[2]/@length"},
        new {Value = "http://localhost/logo.png", Path = "/a:feed/a:logo/text()"},
        new {Value = "copyright(c) localhost", Path ="/a:feed/a:rights/text()" },
        new {Value = "atom <b>1.0</b> example", Path = "/a:feed/a:subtitle/text()"},
        new {Value = "html", Path = "/a:feed/a:subtitle/@type"},
        new {Value = "atom 1.0", Path = "/a:feed/a:title/text()"},
        new {Value = "text", Path = "/a:feed/a:title/@type"},
        new {Value = "2008-02-25T15:01:12.0000000+00:00", Path = "/a:feed/a:updated/text()"},
        new {Value = nullstr, Path ="/a:feed/a:entry"},
      }) {
        var node = xml.SelectSingleNode(pathvalue.Path, nsmgr);

        if (pathvalue.Value == nullstr) {
          Assert.IsNull(node, pathvalue.Path);
        }
        else {
          Assert.IsNotNull(node, pathvalue.Path);
          Assert.AreEqual(pathvalue.Value,
                          node.Value,
                          pathvalue.Path);
        }
      }
    }

    [Test]
    public void TestToStringAtom10Feed()
    {
      var feed = CreateAtom10Feed();
      var xml = new XmlDocument();

      xml.LoadXml(feed.ToString());

      Assert.IsTrue(Parser.Parse(xml) is Atom.Feed);
    }

    [Test]
    public void TestFormatAtom10Entry()
    {
      var feed = new Atom.Feed();

      feed.Authors.Add(new Atom.Person("author"));
      feed.Links.Add(new Atom.Link("http://localhost/index.html"));
      feed.Title = "atom 1.0";
      feed.Id = new Uri("tag:author@localhost,2008-02:author");
      feed.Updated = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);

      Atom.Entry entry = new Atom.Entry();
      
      entry.Authors.Add(new Atom.Person("author1"));
      entry.Content = new Atom.Content("<div><p>content</p></div>", "xhtml");
      entry.Id = new Uri("http://localhost/;id=1");
      entry.Links.Add(new Atom.Link("http://localhost/entry1"));
      entry.Published = new DateTime(2008, 2, 25, 14, 1, 12, DateTimeKind.Utc);
      entry.Summary = null;
      entry.Title = "entry1";
      entry.Updated = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);

      feed.Entries.Add(entry);

      entry = new Atom.Entry();

      entry.Authors.Add(new Atom.Person("author2"));
      entry.Content = new Atom.Content("<p><b>content</b></p>", "xhtml");
      entry.Id = new Uri("http://localhost/;id=2");
      entry.Links.Add(new Atom.Link("http://localhost/entry2"));
      entry.Published = new DateTime(2008, 2, 25, 14, 1, 12, DateTimeKind.Utc);
      entry.Summary = null;
      entry.Title = "entry2";
      entry.Updated = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);

      feed.Entries.Add(entry);

      var xml = feed.ToXmlDocument();
      var nsmgr = new XmlNamespaceManager(xml.NameTable);

      nsmgr.AddNamespace("a", FeedNamespaces.Atom_1_0);
      nsmgr.AddNamespace("x", FeedNamespaces.Xhtml);

      var nullstr = "(null)";

      foreach (var pathvalue in new [] {
        new {Value = "author1", Path = "/a:feed/a:entry[1]/a:author/a:name/text()"},
        new {Value = "xhtml", Path = "/a:feed/a:entry[1]/a:content/@type"},
        new {Value = nullstr, Path = "/a:feed/a:entry[1]/a:content/@src"},
        new {Value = "content", Path = "/a:feed/a:entry[1]/a:content/x:div/x:p/text()"},
        new {Value = "http://localhost/;id=1", Path = "/a:feed/a:entry[1]/a:id/text()"},
        new {Value = "alternate", Path = "/a:feed/a:entry[1]/a:link/@rel"},
        new {Value = "http://localhost/entry1", Path = "/a:feed/a:entry[1]/a:link/@href"},
        new {Value = nullstr, Path = "/a:feed/a:entry[1]/a:summary"},

        new {Value = "xhtml", Path = "/a:feed/a:entry[2]/a:content/@type"},
        new {Value = nullstr, Path = "/a:feed/a:entry[1]/a:content/@src"},
        new {Value = "content", Path = "/a:feed/a:entry[2]/a:content/x:div/x:p/x:b/text()"},
        new {Value = nullstr, Path = "/a:feed/a:entry[2]/a:summary"},
      }) {
        var node = xml.SelectSingleNode(pathvalue.Path, nsmgr);

        if (pathvalue.Value == nullstr) {
          Assert.IsNull(node, pathvalue.Path);
        }
        else {
          Assert.IsNotNull(node, pathvalue.Path);
          Assert.AreEqual(pathvalue.Value,
                          node.Value,
                          pathvalue.Path);
        }
      }
    }

    private RdfRss.Channel CreateRss10Channel()
    {
      var channel = new RdfRss.Channel();

      channel.Title = "rss 1.0";
      channel.Description = "rss 1.0 example";
      channel.Link = new Uri("http://localhost/index.html");
      channel.Uri = new Uri("http://localhost/rss.rdf");

      return channel;
    }

    [Test]
    public void TestFormatRss10Channel()
    {
      var channel = CreateRss10Channel();
      var xml = channel.ToXmlDocument();
      var nsmgr = new XmlNamespaceManager(xml.NameTable);

      nsmgr.AddNamespace("rss", FeedNamespaces.Rss_1_0);
      nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

      var nullstr = "(null)";

      foreach (var pathvalue in new [] {
        new {Value = "http://localhost/rss.rdf", Path = "/rdf:RDF/rss:channel/@rdf:about"},
        new {Value = "rss 1.0", Path = "/rdf:RDF/rss:channel/rss:title/text()"},
        new {Value = "rss 1.0 example", Path = "/rdf:RDF/rss:channel/rss:description/text()"},
        new {Value = "http://localhost/index.html", Path = "/rdf:RDF/rss:channel/rss:link/text()"},
      }) {
        var node = xml.SelectSingleNode(pathvalue.Path, nsmgr);

        if (pathvalue.Value == nullstr) {
          Assert.IsNull(node, pathvalue.Path);
        }
        else {
          Assert.IsNotNull(node, pathvalue.Path);
          Assert.AreEqual(pathvalue.Value,
                          node.Value,
                          pathvalue.Path);
        }
      }
    }

    [Test]
    public void TestToStringRss10Channel()
    {
      var channel = CreateRss10Channel();
      var xml = new XmlDocument();

      xml.LoadXml(channel.ToString());

      Assert.IsTrue(Parser.Parse(xml) is RdfRss.Channel);
    }

    [Test]
    public void TestFormatRss10Item()
    {
      var channel = new RdfRss.Channel();

      channel.Title = "rss 1.0";
      channel.Description = "rss 1.0 example";
      channel.Link = new Uri("http://localhost/index.html");
      channel.Uri = new Uri("http://localhost/rss.rdf");

      var item = new RdfRss.Item();

      item.Title = "entry1";
      item.Description = "description1";
      item.Link = new Uri("http://localhost/?entry=1");
      item.Resource = new Uri("http://localhost/?entry=1");

      channel.Items.Add(item);

      item = new RdfRss.Item();

      item.Title = "entry2";
      item.Description = "description2";
      item.Link = new Uri("http://localhost/?entry=2");
      item.Resource = new Uri("http://localhost/?entry=2");

      channel.Items.Add(item);

      var xml = channel.ToXmlDocument();
      var nsmgr = new XmlNamespaceManager(xml.NameTable);

      nsmgr.AddNamespace("rss", FeedNamespaces.Rss_1_0);
      nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

      var nullstr = "(null)";

      foreach (var pathvalue in new [] {
        new {Value = "http://localhost/?entry=1", Path = "/rdf:RDF/rss:channel/rss:items/rdf:Seq/rdf:li[1]/@rdf:resource"},
        new {Value = "http://localhost/?entry=2", Path = "/rdf:RDF/rss:channel/rss:items/rdf:Seq/rdf:li[2]/@rdf:resource"},

        new {Value = "entry1", Path = "/rdf:RDF/rss:item[@rdf:about='http://localhost/?entry=1']/rss:title/text()"},
        new {Value = "description1", Path = "/rdf:RDF/rss:item[@rdf:about='http://localhost/?entry=1']/rss:description/text()"},
        new {Value = "http://localhost/?entry=1", Path = "/rdf:RDF/rss:item[@rdf:about='http://localhost/?entry=1']/rss:link/text()"},

        new {Value = "entry2", Path = "/rdf:RDF/rss:item[@rdf:about='http://localhost/?entry=2']/rss:title/text()"},
        new {Value = "description2", Path = "/rdf:RDF/rss:item[@rdf:about='http://localhost/?entry=2']/rss:description/text()"},
        new {Value = "http://localhost/?entry=2", Path = "/rdf:RDF/rss:item[@rdf:about='http://localhost/?entry=2']/rss:link/text()"},
      }) {
        var node = xml.SelectSingleNode(pathvalue.Path, nsmgr);

        if (pathvalue.Value == nullstr) {
          Assert.IsNull(node, pathvalue.Path);
        }
        else {
          Assert.IsNotNull(node, pathvalue.Path);
          Assert.AreEqual(pathvalue.Value,
                          node.Value,
                          pathvalue.Path);
        }
      }
    }

    private Rss.Channel CreateRss20Channel()
    {
      var channel = new Rss.Channel();

      channel.Title = "rss 2.0";
      channel.Link = new Uri("http://localhost/index.html");
      channel.Description = "rss 2.0 example";
      channel.Language = "ja-jp";
      channel.Copyright = "copyright";
      channel.ManagingEditor = "editor@example.com (editor)";
      channel.WebMaster = "webmaster@example.com (webmaster)";
      channel.PubDate = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);
      channel.LastBuildDate = new DateTime(2008, 2, 25, 16, 1, 12, DateTimeKind.Utc);
      channel.Categories.Add(new Rss.Category("category1"));
      channel.Categories.Add(new Rss.Category("category2", new Uri("http://example.com/categories/")));
      channel.Generator = "Smdn.Formats.Feeds";
      //channel.Docs; use default
      // TODO: channel.Cloud
      channel.Ttl = 72;
      channel.Image = new Rss.Image(new Uri("http://localhost/image.png"),
                                            "logo",
                                            new Uri("http://localhost/index.html"),
                                            "site logo",
                                            88,
                                            31);
      // TODO: channel.Rating
      channel.TextInput = new Rss.TextInput("comment", "comment to this page", "ti", new Uri("http://localhost/comment.cgi"));
      channel.SkipHours.Add(4);
      channel.SkipHours.Add(5);
      channel.SkipHours.Add(6);
      channel.SkipDays.Add(DayOfWeek.Saturday);
      channel.SkipDays.Add(DayOfWeek.Sunday);

      return channel;
    }

    [Test]
    public void TestFormatRss20Channel()
    {
      var channel = CreateRss20Channel();
      var xml = channel.ToXmlDocument();
      var nsmgr = new XmlNamespaceManager(xml.NameTable);

      //nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

      var nullstr = "(null)";

      foreach (var pathvalue in new [] {
        new {Value = "rss 2.0", Path = "/rss/channel/title/text()"},
        new {Value = "http://localhost/index.html", Path = "/rss/channel/link/text()"},
        new {Value = "rss 2.0 example", Path = "/rss/channel/description/text()"},
        new {Value = "ja-jp", Path = "/rss/channel/language/text()"},
        new {Value = "copyright", Path = "/rss/channel/copyright/text()"},
        new {Value = "editor@example.com (editor)", Path = "/rss/channel/managingEditor/text()"},
        new {Value = "webmaster@example.com (webmaster)", Path = "/rss/channel/webMaster/text()"},
        new {Value = "Mon, 25 Feb 2008 15:01:12 +0000", Path = "/rss/channel/pubDate/text()"},
        new {Value = "Mon, 25 Feb 2008 16:01:12 +0000", Path = "/rss/channel/lastBuildDate/text()"},
        new {Value = "category1", Path = "/rss/channel/category[1]/text()"},
        new {Value = nullstr, Path = "/rss/channel/category[1]/@domain"},
        new {Value = "category2", Path = "/rss/channel/category[2]/text()"},
        new {Value = "http://example.com/categories/", Path = "/rss/channel/category[2]/@domain"},
        new {Value = "Smdn.Formats.Feeds", Path = "/rss/channel/generator/text()"},
        new {Value = "http://cyber.law.harvard.edu/rss/rss.html", Path = "/rss/channel/docs/text()"},
        new {Value = "72", Path = "/rss/channel/ttl/text()"},
        new {Value = "http://localhost/image.png", Path = "/rss/channel/image/url/text()"},
        new {Value = "logo", Path = "/rss/channel/image/title/text()"},
        new {Value = "http://localhost/index.html", Path = "/rss/channel/image/link/text()"},
        new {Value = "site logo", Path = "/rss/channel/image/description/text()"},
        new {Value = "88", Path = "/rss/channel/image/width/text()"},
        new {Value = "31", Path = "/rss/channel/image/height/text()"},
        new {Value = "comment", Path = "/rss/channel/textInput/title//text()"},
        new {Value = "comment to this page", Path = "/rss/channel/textInput/description/text()"},
        new {Value = "ti", Path = "/rss/channel/textInput/name/text()"},
        new {Value = "http://localhost/comment.cgi", Path = "/rss/channel/textInput/link/text()"},
        new {Value = "4", Path = "/rss/channel/skipHours/hour[1]/text()"},
        new {Value = "5", Path = "/rss/channel/skipHours/hour[2]/text()"},
        new {Value = "6", Path = "/rss/channel/skipHours/hour[3]/text()"},
        new {Value = "Saturday", Path = "/rss/channel/skipDays/day[1]/text()"},
        new {Value = "Sunday", Path = "/rss/channel/skipDays/day[2]/text()"},
      }) {
        var node = xml.SelectSingleNode(pathvalue.Path, nsmgr);

        if (pathvalue.Value == nullstr) {
          Assert.IsNull(node, pathvalue.Path);
        }
        else {
          Assert.IsNotNull(node, pathvalue.Path);
          Assert.AreEqual(pathvalue.Value,
                          node.Value,
                          pathvalue.Path);
        }
      }
    }

    [Test]
    public void TestToStringRss20Channel()
    {
      var channel = CreateRss20Channel();
      var xml = new XmlDocument();

      xml.LoadXml(channel.ToString());

      Assert.IsTrue(Parser.Parse(xml) is Rss.Channel);
    }

    [Test]
    public void TestFormatRss20Item()
    {
      var channel = new Rss.Channel();

      channel.Title = "rss 2.0";
      channel.Link = new Uri("http://localhost/index.html");
      channel.Description = "rss 2.0 example";
 
      var item = new Rss.Item();

      item.Title = "entry1";
      item.Link = new Uri("http://localhost/entry1/");
      item.Description = "description1";
      item.Author = "author";
      item.Categories.Add(new Rss.Category("category1"));
      item.Comments = new Uri("http://localhost/comment.cgi?entry=1");
      item.Guid = new Rss.Guid("http://localhost/entry1/", true);
      item.PubDate = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Utc);
      item.Source = new Rss.Source("example source", new Uri("http://example.com/rss.xml"));

      channel.Items.Add(item);

      item = new Rss.Item();

      item.Title = "entry2";
      item.Link = new Uri("http://localhost/entry2/");
      item.Description = "description2";
      item.Guid = new Rss.Guid("http://localhost/entry2/");
      item.PubDate = new DateTime(2008, 2, 25, 16, 1, 12, DateTimeKind.Utc);

      channel.Items.Add(item);

      var xml = channel.ToXmlDocument();
      var nsmgr = new XmlNamespaceManager(xml.NameTable);

      //nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);

      var nullstr = "(null)";

      foreach (var pathvalue in new [] {
        new {Value = "entry1", Path = "/rss/channel/item[1]/title/text()"},
        new {Value = "http://localhost/entry1/", Path = "/rss/channel/item[1]/link/text()"},
        new {Value = "description1", Path = "/rss/channel/item[1]/description/text()"},
        new {Value = "author", Path = "/rss/channel/item[1]/author/text()"},
        new {Value = "category1", Path = "/rss/channel/item[1]/category[1]/text()"},
        new {Value = nullstr, Path = "/rss/channel/item[1]/category[1]/@domain"},
        new {Value = "http://localhost/comment.cgi?entry=1", Path = "/rss/channel/item[1]/comments/text()"},
        new {Value = "http://localhost/entry1/", Path = "/rss/channel/item[1]/guid/text()"},
        new {Value = "true", Path = "/rss/channel/item[1]/guid/@isPermaLink"},
        new {Value = "Mon, 25 Feb 2008 15:01:12 +0000", Path = "/rss/channel/item[1]/pubDate/text()"},
        new {Value = "example source", Path = "/rss/channel/item[1]/source/text()"},
        new {Value = "http://example.com/rss.xml", Path = "/rss/channel/item[1]/source/@url"},

        new {Value = "entry2", Path = "/rss/channel/item[2]/title/text()"},
        new {Value = "http://localhost/entry2/", Path = "/rss/channel/item[2]/link/text()"},
        new {Value = "description2", Path = "/rss/channel/item[2]/description/text()"},
        new {Value = "http://localhost/entry2/", Path = "/rss/channel/item[2]/guid/text()"},
        new {Value = nullstr, Path = "/rss/channel/item[2]/guid/@isPermaLink"},
        new {Value = "Mon, 25 Feb 2008 16:01:12 +0000", Path = "/rss/channel/item[2]/pubDate/text()"},
      }) {

        var node = xml.SelectSingleNode(pathvalue.Path, nsmgr);

        if (pathvalue.Value == nullstr) {
          Assert.IsNull(node, pathvalue.Path);
        }
        else {
          Assert.IsNotNull(node, pathvalue.Path);
          Assert.AreEqual(pathvalue.Value,
                          node.Value,
                          pathvalue.Path);
        }
      }
    }
  }
}