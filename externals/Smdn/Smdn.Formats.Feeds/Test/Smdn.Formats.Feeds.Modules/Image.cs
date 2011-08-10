using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class ImageTest {
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

    [Test]
    public void TestParse()
    {
      RdfRss.Channel channel;

      using (var reader = new StreamReader("Rss10ModuleImageExample.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.IsTrue(Image.Null != channel.ImageModule);
      Assert.AreEqual(new Uri("http://www.kuro5hin.org/favicon.ico"), channel.ImageModule.Favicon);
      Assert.AreEqual(Image.Size.Small, channel.ImageModule.FaviconSize);

      Assert.AreEqual(1, channel.Items.Count);
      Assert.IsTrue(Image.Null != channel.Items[0].ImageModule);
      Assert.AreEqual(new Uri("http://www.kuro5hin.org/images/topics/culture.jpg"), channel.Items[0].ImageModule.Item);
      Assert.AreEqual(80, channel.Items[0].ImageModule.ItemWidth);
      Assert.AreEqual(50, channel.Items[0].ImageModule.ItemHeight);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      feed.Modules.Add(Image.NamespaceUri, new Image());
      feed.ImageModule.Favicon = new Uri("http://example.com/favicon.ico");
      feed.ImageModule.FaviconSize = Image.Size.Large;

      entries[0].Modules.Add(Image.NamespaceUri, new Image());
      entries[0].ImageModule.Item = new Uri("http://example.com/image.png");
      entries[0].ImageModule.ItemWidth = 640;
      entries[0].ImageModule.ItemHeight = 480;
    }


    [Test]
    public void TestFormatRss10()
    {
      var channel = ModuleBaseTest.CreateTestRdfRssChannel();

      AddModules(channel, channel.Items.ToArray());

      var document = channel.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace("rss", FeedNamespaces.Rss_1_0);
      nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);
      nsmgr.AddNamespace("image", Image.NamespaceUri);

      Assert.AreEqual("http://example.com/favicon.ico",
                      document.SelectSingleNode("/rdf:RDF/rss:channel/image:favicon/@rdf:about", nsmgr).Value);
      Assert.AreEqual("large",
                      document.SelectSingleNode("/rdf:RDF/rss:channel/image:favicon/@image:size", nsmgr).Value);

      Assert.AreEqual("http://example.com/image.png",
                      document.SelectSingleNode("/rdf:RDF/rss:item/image:item/@rdf:about", nsmgr).Value);
      Assert.AreEqual("640",
                      document.SelectSingleNode("/rdf:RDF/rss:item/image:item/image:width/text()", nsmgr).Value);
      Assert.AreEqual("480",
                      document.SelectSingleNode("/rdf:RDF/rss:item/image:item/image:height/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatAtom10()
    {
      var feed = ModuleBaseTest.CreateTestAtomFeed();

      AddModules(feed, feed.Entries.ToArray());

      var document = feed.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace("atom", FeedNamespaces.Atom_1_0);
      nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);
      nsmgr.AddNamespace("image", Image.NamespaceUri);

      Assert.AreEqual("http://example.com/favicon.ico",
                      document.SelectSingleNode("/atom:feed/image:favicon/@rdf:about", nsmgr).Value);
      Assert.AreEqual("large",
                      document.SelectSingleNode("/atom:feed/image:favicon/@image:size", nsmgr).Value);

      Assert.AreEqual("http://example.com/image.png",
                      document.SelectSingleNode("/atom:feed/atom:entry/image:item/@rdf:about", nsmgr).Value);
      Assert.AreEqual("640",
                      document.SelectSingleNode("/atom:feed/atom:entry/image:item/image:width/text()", nsmgr).Value);
      Assert.AreEqual("480",
                      document.SelectSingleNode("/atom:feed/atom:entry/image:item/image:height/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatRss20()
    {
      var channel = ModuleBaseTest.CreateTestRssChannel();

      AddModules(channel, channel.Items.ToArray());

      var document = channel.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace(string.Empty, string.Empty);
      nsmgr.AddNamespace("rdf", FeedNamespaces.Rdf);
      nsmgr.AddNamespace("image", Image.NamespaceUri);

      Assert.AreEqual("http://example.com/favicon.ico",
                      document.SelectSingleNode("/rss/channel/image:favicon/@rdf:about", nsmgr).Value);
      Assert.AreEqual("large",
                      document.SelectSingleNode("/rss/channel/image:favicon/@image:size", nsmgr).Value);

      Assert.AreEqual("http://example.com/image.png",
                      document.SelectSingleNode("/rss/channel/item/image:item/@rdf:about", nsmgr).Value);
      Assert.AreEqual("640",
                      document.SelectSingleNode("/rss/channel/item/image:item/image:width/text()", nsmgr).Value);
      Assert.AreEqual("480",
                      document.SelectSingleNode("/rss/channel/item/image:item/image:height/text()", nsmgr).Value);

    }
  }
}