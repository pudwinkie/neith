using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class AnnotationTest {
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

      using (var reader = new StreamReader("Rss10ModuleAnnotationExample.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.AreEqual(4, channel.Items.Count);

      Assert.AreSame(Annotation.Null, channel.Items[0].AnnotationModule);

      Assert.IsTrue(Annotation.Null != channel.Items[1].AnnotationModule);
      Assert.AreEqual(new Uri("http://www.monkeyfist.com/discuss/1"), channel.Items[1].AnnotationModule.Reference);

      Assert.IsTrue(Annotation.Null != channel.Items[2].AnnotationModule);
      Assert.AreEqual(new Uri("http://www.monkeyfist.com/discuss/2"), channel.Items[2].AnnotationModule.Reference);

      Assert.IsTrue(Annotation.Null != channel.Items[3].AnnotationModule);
      Assert.AreEqual(new Uri("http://www.monkeyfist.com/discuss/2"), channel.Items[3].AnnotationModule.Reference);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      entries[0].Modules.Add(Annotation.NamespaceUri, new Annotation());
      entries[0].AnnotationModule.Reference = new Uri("http://example.com/entry0/annotation/");
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
      nsmgr.AddNamespace("an", Annotation.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/entry0/annotation/"),
                      document.SelectSingleNode("/rdf:RDF/rss:item/an:reference/@rdf:resource", nsmgr).Value);
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
      nsmgr.AddNamespace("an", Annotation.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/entry0/annotation/"),
                      document.SelectSingleNode("/atom:feed/atom:entry/an:reference/@rdf:resource", nsmgr).Value);
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
      nsmgr.AddNamespace("an", Annotation.NamespaceUri);

      Assert.AreEqual(new Uri("http://example.com/entry0/annotation/"),
                      document.SelectSingleNode("/rss/channel/item/an:reference/@rdf:resource", nsmgr).Value);
    }
  }
}