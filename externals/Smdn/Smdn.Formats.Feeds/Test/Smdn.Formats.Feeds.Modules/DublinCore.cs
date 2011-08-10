using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Smdn.Formats.Feeds.Modules {
  [TestFixture]
  public class DublinCoreTest {
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

      using (var reader = new StreamReader("Rss10ModuleDublinCoreExample.xml")) {
        channel = Parser.Parse(reader) as RdfRss.Channel;
      }

      Assert.IsNotNull(channel);

      Assert.IsTrue(DublinCore.Null != channel.DublinCoreModule);
      Assert.AreEqual(new[] {"The O'Reilly Network"}, channel.DublinCoreModule.Publisher);
      Assert.AreEqual(new[] {"Rael Dornfest (mailto:rael@oreilly.com)"}, channel.DublinCoreModule.Creator);
      Assert.AreEqual(new[] {"Copyright © 2000 O'Reilly & Associates, Inc."}, channel.DublinCoreModule.Rights);
      Assert.AreEqual(new DateTimeOffset?[] {new DateTimeOffset(2000, 1, 1, 12, 0, 0, TimeSpan.Zero)},
                      channel.DublinCoreModule.Date);

      Assert.AreEqual(1, channel.Items.Count);

      Assert.IsTrue(DublinCore.Null != channel.Items[0].DublinCoreModule);
      Assert.AreEqual(new[] {
                      Environment.NewLine +
                      "      XML is placing increasingly heavy loads on the existing technical" +
                      Environment.NewLine +
                      "      infrastructure of the Internet." +
                      Environment.NewLine +
                      "    "},
                      channel.Items[0].DublinCoreModule.Description);
      Assert.AreEqual(new[] {"The O'Reilly Network"}, channel.Items[0].DublinCoreModule.Publisher);
      Assert.AreEqual(new[] {"Simon St.Laurent (mailto:simonstl@simonstl.com)"}, channel.Items[0].DublinCoreModule.Creator);
      Assert.AreEqual(new[] {"Copyright © 2000 O'Reilly & Associates, Inc."}, channel.Items[0].DublinCoreModule.Rights);
      Assert.AreEqual(new[] {"XML"}, channel.Items[0].DublinCoreModule.Subject);
    }

    private void AddModules(FeedBase feed, EntryBase[] entries)
    {
      entries[0].Modules.Add(DublinCore.NamespaceUri, new DublinCore());
      entries[0].DublinCoreModule.Contributor = new[] {"Contributor"};
      entries[0].DublinCoreModule.Coverage = new[] {"Coverage"};
      entries[0].DublinCoreModule.Creator = new[] {"Creator"};
      entries[0].DublinCoreModule.Date = new DateTimeOffset?[] {new DateTime(2008, 2, 25, 15, 1, 12, DateTimeKind.Local)};
      entries[0].DublinCoreModule.Description = new[] {"Description"};
      entries[0].DublinCoreModule.FileFormat = new[] {"Format"};
      entries[0].DublinCoreModule.Identifier = new[] {"Identifier"};
      entries[0].DublinCoreModule.Language = new[] {"Language"};
      entries[0].DublinCoreModule.Publisher = new[] {"Publisher"};
      entries[0].DublinCoreModule.Relation = new[] {"Relation"};
      entries[0].DublinCoreModule.Rights = new[] {"Rights"};
      entries[0].DublinCoreModule.Source = new[] {"Source"};
      entries[0].DublinCoreModule.Subject = new[] {"Subject1", "Subject2", "Subject3"};
      entries[0].DublinCoreModule.Title = new[] {"Title"};
      entries[0].DublinCoreModule.Type = new[] {"Type"};
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
      nsmgr.AddNamespace("dc", DublinCore.NamespaceUri);

      Assert.AreEqual("Contributor",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:contributor/text()", nsmgr).Value);
      Assert.AreEqual("Coverage",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:coverage/text()", nsmgr).Value);
      Assert.AreEqual("Creator",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:creator/text()", nsmgr).Value);
      Assert.AreEqual("2008-02-25T15:01:12.0000000" + DateTimeConvert.GetCurrentTimeZoneOffsetString(true),
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:date/text()", nsmgr).Value);
      Assert.AreEqual("Description",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:description/text()", nsmgr).Value);
      Assert.AreEqual("Format",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:format/text()", nsmgr).Value);
      Assert.AreEqual("Identifier",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:identifier/text()", nsmgr).Value);
      Assert.AreEqual("Language",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:language/text()", nsmgr).Value);
      Assert.AreEqual("Publisher",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:publisher/text()", nsmgr).Value);
      Assert.AreEqual("Relation",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:relation/text()", nsmgr).Value);
      Assert.AreEqual("Rights",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:rights/text()", nsmgr).Value);
      Assert.AreEqual("Source",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:source/text()", nsmgr).Value);
      Assert.AreEqual("Subject1",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:subject[1]/text()", nsmgr).Value);
      Assert.AreEqual("Subject2",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:subject[2]/text()", nsmgr).Value);
      Assert.AreEqual("Subject3",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:subject[3]/text()", nsmgr).Value);
      Assert.AreEqual("Title",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:title/text()", nsmgr).Value);
      Assert.AreEqual("Type",
                      document.SelectSingleNode("/rdf:RDF/rss:item/dc:type/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatAtom10()
    {
      var feed = ModuleBaseTest.CreateTestAtomFeed();

      AddModules(feed, feed.Entries.ToArray());

      var document = feed.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace("atom", FeedNamespaces.Atom_1_0);
      nsmgr.AddNamespace("dc", DublinCore.NamespaceUri);

      Assert.AreEqual("Contributor",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:contributor/text()", nsmgr).Value);
      Assert.AreEqual("Coverage",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:coverage/text()", nsmgr).Value);
      Assert.AreEqual("Creator",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:creator/text()", nsmgr).Value);
      Assert.AreEqual("2008-02-25T15:01:12.0000000" + DateTimeConvert.GetCurrentTimeZoneOffsetString(true),
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:date/text()", nsmgr).Value);
      Assert.AreEqual("Description",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:description/text()", nsmgr).Value);
      Assert.AreEqual("Format",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:format/text()", nsmgr).Value);
      Assert.AreEqual("Identifier",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:identifier/text()", nsmgr).Value);
      Assert.AreEqual("Language",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:language/text()", nsmgr).Value);
      Assert.AreEqual("Publisher",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:publisher/text()", nsmgr).Value);
      Assert.AreEqual("Relation",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:relation/text()", nsmgr).Value);
      Assert.AreEqual("Rights",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:rights/text()", nsmgr).Value);
      Assert.AreEqual("Source",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:source/text()", nsmgr).Value);
      Assert.AreEqual("Subject1",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:subject[1]/text()", nsmgr).Value);
      Assert.AreEqual("Subject2",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:subject[2]/text()", nsmgr).Value);
      Assert.AreEqual("Subject3",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:subject[3]/text()", nsmgr).Value);
      Assert.AreEqual("Title",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:title/text()", nsmgr).Value);
      Assert.AreEqual("Type",
                      document.SelectSingleNode("/atom:feed/atom:entry/dc:type/text()", nsmgr).Value);
    }

    [Test]
    public void TestFormatRss20()
    {
      var channel = ModuleBaseTest.CreateTestRssChannel();

      AddModules(channel, channel.Items.ToArray());

      var document = channel.ToXmlDocument();

      var nsmgr = new XmlNamespaceManager(document.NameTable);

      nsmgr.AddNamespace(string.Empty, string.Empty);
      nsmgr.AddNamespace("dc", DublinCore.NamespaceUri);

      Assert.AreEqual("Contributor",
                      document.SelectSingleNode("/rss/channel/item/dc:contributor/text()", nsmgr).Value);
      Assert.AreEqual("Coverage",
                      document.SelectSingleNode("/rss/channel/item/dc:coverage/text()", nsmgr).Value);
      Assert.AreEqual("Creator",
                      document.SelectSingleNode("/rss/channel/item/dc:creator/text()", nsmgr).Value);
      Assert.AreEqual("2008-02-25T15:01:12.0000000" + DateTimeConvert.GetCurrentTimeZoneOffsetString(true),
                      document.SelectSingleNode("/rss/channel/item/dc:date/text()", nsmgr).Value);
      Assert.AreEqual("Description",
                      document.SelectSingleNode("/rss/channel/item/dc:description/text()", nsmgr).Value);
      Assert.AreEqual("Format",
                      document.SelectSingleNode("/rss/channel/item/dc:format/text()", nsmgr).Value);
      Assert.AreEqual("Identifier",
                      document.SelectSingleNode("/rss/channel/item/dc:identifier/text()", nsmgr).Value);
      Assert.AreEqual("Language",
                      document.SelectSingleNode("/rss/channel/item/dc:language/text()", nsmgr).Value);
      Assert.AreEqual("Publisher",
                      document.SelectSingleNode("/rss/channel/item/dc:publisher/text()", nsmgr).Value);
      Assert.AreEqual("Relation",
                      document.SelectSingleNode("/rss/channel/item/dc:relation/text()", nsmgr).Value);
      Assert.AreEqual("Rights",
                      document.SelectSingleNode("/rss/channel/item/dc:rights/text()", nsmgr).Value);
      Assert.AreEqual("Source",
                      document.SelectSingleNode("/rss/channel/item/dc:source/text()", nsmgr).Value);
      Assert.AreEqual("Subject1",
                      document.SelectSingleNode("/rss/channel/item/dc:subject[1]/text()", nsmgr).Value);
      Assert.AreEqual("Subject2",
                      document.SelectSingleNode("/rss/channel/item/dc:subject[2]/text()", nsmgr).Value);
      Assert.AreEqual("Subject3",
                      document.SelectSingleNode("/rss/channel/item/dc:subject[3]/text()", nsmgr).Value);
      Assert.AreEqual("Title",
                      document.SelectSingleNode("/rss/channel/item/dc:title/text()", nsmgr).Value);
      Assert.AreEqual("Type",
                      document.SelectSingleNode("/rss/channel/item/dc:type/text()", nsmgr).Value);
    }
  }
}