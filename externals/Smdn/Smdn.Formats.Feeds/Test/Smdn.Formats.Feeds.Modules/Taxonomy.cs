using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

// TODO: /rdf:RDF/taxo:topicのparse
namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class TaxonomyTest {
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

      using (var reader = new StreamReader("Rss10ModuleTaxonomyExample.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.AreEqual(1, channel.Items.Count);
      Assert.IsTrue(Taxonomy.Null != channel.Items[0].TaxonomyModule);
      Assert.AreEqual(3, channel.Items[0].TaxonomyModule.Topics.Length);
      Assert.AreEqual(new Uri("http://meerkat.oreillynet.com/?c=cat23"), channel.Items[0].TaxonomyModule.Topics[0]);
      Assert.AreEqual(new Uri("http://meerkat.oreillynet.com/?c=47"), channel.Items[0].TaxonomyModule.Topics[1]);
      Assert.AreEqual(new Uri("http://dmoz.org/Computers/Data_Formats/Markup_Languages/XML/"), channel.Items[0].TaxonomyModule.Topics[2]);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      entries[0].Modules.Add(Taxonomy.NamespaceUri, new Taxonomy());
      entries[0].TaxonomyModule.Topics = new[] {
        new Uri("http://example.com/?c=cat0"),
        new Uri("http://example.com/?c=cat1"),
        new Uri("http://example.com/?c=cat2"),
      };
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
      nsmgr.AddNamespace("taxo", Taxonomy.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/?c=cat0"),
                      document.SelectSingleNode("/rdf:RDF/rss:item/taxo:topics/rdf:Bag/rdf:li[1]/@resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/?c=cat1"),
                      document.SelectSingleNode("/rdf:RDF/rss:item/taxo:topics/rdf:Bag/rdf:li[2]/@resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/?c=cat2"),
                      document.SelectSingleNode("/rdf:RDF/rss:item/taxo:topics/rdf:Bag/rdf:li[3]/@resource", nsmgr).Value);
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
      nsmgr.AddNamespace("taxo", Taxonomy.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/?c=cat0"),
                      document.SelectSingleNode("/atom:feed/atom:entry/taxo:topics/rdf:Bag/rdf:li[1]/@resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/?c=cat1"),
                      document.SelectSingleNode("/atom:feed/atom:entry/taxo:topics/rdf:Bag/rdf:li[2]/@resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/?c=cat2"),
                      document.SelectSingleNode("/atom:feed/atom:entry/taxo:topics/rdf:Bag/rdf:li[3]/@resource", nsmgr).Value);
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
      nsmgr.AddNamespace("taxo", Taxonomy.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/?c=cat0"),
                      document.SelectSingleNode("/rss/channel/item/taxo:topics/rdf:Bag/rdf:li[1]/@resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/?c=cat1"),
                      document.SelectSingleNode("/rss/channel/item/taxo:topics/rdf:Bag/rdf:li[2]/@resource", nsmgr).Value);
      Assert.AreEqual(new Uri("http://example.com/?c=cat2"),
                      document.SelectSingleNode("/rss/channel/item/taxo:topics/rdf:Bag/rdf:li[3]/@resource", nsmgr).Value);
    }
  }
}