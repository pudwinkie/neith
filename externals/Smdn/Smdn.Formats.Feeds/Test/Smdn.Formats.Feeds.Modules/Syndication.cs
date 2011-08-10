using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class SyndicationTest {
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

      using (var reader = new StreamReader("Rss10ModuleSyndicationExample.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.IsTrue(Syndication.Null != channel.SyndicationModule);
      Assert.AreEqual(Syndication.Period.Hourly, channel.SyndicationModule.UpdatePeriod);
      Assert.AreEqual(2, channel.SyndicationModule.UpdateFrequency);
      Assert.AreEqual(new DateTimeOffset(2000, 1, 1, 12, 0, 0, TimeSpan.Zero),
                      channel.SyndicationModule.UpdateBase);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      feed.Modules.Add(Syndication.NamespaceUri, new Syndication());
      feed.SyndicationModule.UpdatePeriod = Syndication.Period.Weekly;
      feed.SyndicationModule.UpdateFrequency = 30;
      feed.SyndicationModule.UpdateBase = new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Local);
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
      nsmgr.AddNamespace("sy", Syndication.NamespaceUri);

      Assert.AreEqual("weekly",
                      document.SelectSingleNode("/rdf:RDF/rss:channel/sy:updatePeriod/text()", nsmgr).Value);
      Assert.AreEqual("30",
                      document.SelectSingleNode("/rdf:RDF/rss:channel/sy:updateFrequency/text()", nsmgr).Value);
      Assert.AreEqual("2008-02-25T15:01:12.0000000" + DateTimeConvert.GetCurrentTimeZoneOffsetString(true),
                      document.SelectSingleNode("/rdf:RDF/rss:channel/sy:updateBase/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatAtom10()
    {
      var feed = ModuleBaseTest.CreateTestAtomFeed();

      AddModules(feed, feed.Entries.ToArray());

      var document = feed.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace("atom", FeedNamespaces.Atom_1_0);
      nsmgr.AddNamespace("sy", Syndication.NamespaceUri);

      Assert.AreEqual("weekly",
                      document.SelectSingleNode("/atom:feed/sy:updatePeriod/text()", nsmgr).Value);
      Assert.AreEqual("30",
                      document.SelectSingleNode("/atom:feed/sy:updateFrequency/text()", nsmgr).Value);
      Assert.AreEqual("2008-02-25T15:01:12.0000000" + DateTimeConvert.GetCurrentTimeZoneOffsetString(true),
                      document.SelectSingleNode("/atom:feed/sy:updateBase/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatRss20()
    {
      var channel = ModuleBaseTest.CreateTestRssChannel();

      AddModules(channel, channel.Items.ToArray());

      var document = channel.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace(string.Empty, string.Empty);
      nsmgr.AddNamespace("sy", Syndication.NamespaceUri);

      Console.WriteLine(document.InnerXml);
      Assert.AreEqual("weekly",
                      document.SelectSingleNode("/rss/channel/sy:updatePeriod/text()", nsmgr).Value);
      Assert.AreEqual("30",
                      document.SelectSingleNode("/rss/channel/sy:updateFrequency/text()", nsmgr).Value);
      Assert.AreEqual("2008-02-25T15:01:12.0000000" + DateTimeConvert.GetCurrentTimeZoneOffsetString(true),
                      document.SelectSingleNode("/rss/channel/sy:updateBase/text()", nsmgr).Value);
    }
  }
}