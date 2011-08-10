using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class ContentTest {
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

      using (var reader = new StreamReader("Rss10ModuleContentExampleDraft20.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.AreEqual(1, channel.Items.Count);

      Assert.IsTrue(Content.Null != channel.Items[0].ContentModule);
      Assert.AreEqual("<p>What a <em>beautiful</em> day!</p>", channel.Items[0].ContentModule.Encoded);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      entries[0].Modules.Add(Content.NamespaceUri, new Content());
      entries[0].ContentModule.Encoded = "<p>encoded content</p>";
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
      nsmgr.AddNamespace("content", Content.NamespaceUri);

      Assert.AreEqual("<p>encoded content</p>",
                      document.SelectSingleNode("/rdf:RDF/rss:item/content:encoded/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatAtom10()
    {
      var feed = ModuleBaseTest.CreateTestAtomFeed();

      AddModules(feed, feed.Entries.ToArray());

      var document = feed.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace("atom", FeedNamespaces.Atom_1_0);
      nsmgr.AddNamespace("content", Content.NamespaceUri);

      Assert.AreEqual("<p>encoded content</p>",
                      document.SelectSingleNode("/atom:feed/atom:entry/content:encoded/text()", nsmgr).Value);
    }


    [Test]
    public void TestFormatRss20()
    {
      var channel = ModuleBaseTest.CreateTestRssChannel();

      AddModules(channel, channel.Items.ToArray());

      var document = channel.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace(string.Empty, string.Empty);
      nsmgr.AddNamespace("content", Content.NamespaceUri);

      Assert.AreEqual("<p>encoded content</p>",
                      document.SelectSingleNode("/rss/channel/item/content:encoded/text()", nsmgr).Value);
    }
  }
}