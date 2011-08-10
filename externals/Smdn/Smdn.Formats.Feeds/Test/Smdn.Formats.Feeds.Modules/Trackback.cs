using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

// TODO: /rdf:RDF/taxo:topicのparse
namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class TrackbackTest {
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
    public void TestParseRss10()
    {
      RdfRss.Channel channel;

      using (var reader = new StreamReader("Rss10ModuleTrackbackExample.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.AreEqual(1, channel.Items.Count);
      Assert.IsTrue(Trackback.Null != channel.Items[0].TrackbackModule);
      Assert.AreEqual(new Uri("http://bar.com/tb.cgi?tb_id=rssplustrackback"), channel.Items[0].TrackbackModule.Ping);
      Assert.AreEqual(new Uri("http://foo.com/trackback/tb.cgi?tb_id=20020923"), channel.Items[0].TrackbackModule.About);
    }

    [Test]
    public void TestParseRss20()
    {
      Rss.Channel channel;

      using (var reader = new StreamReader("Rss20ModuleTrackbackExample.xml")) {
        channel = Parser.Parse(reader.ReadToEnd()) as Rss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.AreEqual(1, channel.Items.Count);
      Assert.IsTrue(Trackback.Null != channel.Items[0].TrackbackModule);
      Assert.AreEqual(new Uri("http://bar.com/tb.cgi?tb_id=rssplustrackback"), channel.Items[0].TrackbackModule.Ping);
      Assert.AreEqual(new Uri("http://foo.com/trackback/tb.cgi?tb_id=20020923"), channel.Items[0].TrackbackModule.About);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      entries[0].Modules.Add(Trackback.NamespaceUri, new Trackback());
      entries[0].TrackbackModule.Ping = new Uri("http://example.com/ping.cgi?id=0");
      entries[0].TrackbackModule.About = new Uri("http://example.com/tb.cgi?id=0");
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
      nsmgr.AddNamespace("tr", Trackback.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/ping.cgi?id=0"),
                      document.SelectSingleNode("/rdf:RDF/rss:item/tr:ping/@rdf:resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/tb.cgi?id=0"),
                      document.SelectSingleNode("/rdf:RDF/rss:item/tr:about/@rdf:resource", nsmgr).Value);
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
      nsmgr.AddNamespace("tr", Trackback.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/ping.cgi?id=0"),
                      document.SelectSingleNode("/atom:feed/atom:entry/tr:ping/@rdf:resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/tb.cgi?id=0"),
                      document.SelectSingleNode("/atom:feed/atom:entry/tr:about/@rdf:resource", nsmgr).Value);
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
      nsmgr.AddNamespace("tr", Trackback.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/ping.cgi?id=0"),
                      document.SelectSingleNode("/rss/channel/item/tr:ping/text()", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/tb.cgi?id=0"),
                      document.SelectSingleNode("/rss/channel/item/tr:about/text()", nsmgr).Value);
    }
  }
}