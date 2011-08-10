using System;
using NUnit.Framework;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class IEntryTest {
    private void TestInterface(IEntry entry)
    {
      Assert.AreEqual(new Uri("http://localhost/1"), entry.Link);
      Assert.AreEqual("title", entry.Title);
      Assert.AreEqual("description", entry.Description);
      Assert.AreEqual(new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero), entry.Date);
      Assert.AreEqual("http://localhost/#1", entry.Id);
    }

    [Test]
    public void TestInterfaceAtom()
    {
      var atomEntry = new Atom.Entry();

      atomEntry.Links.Add(new Atom.Link(new Uri("http://localhost/1"), "alternate"));
      atomEntry.Title = new Atom.Text("title");
      atomEntry.Summary = new Atom.Text("description");
      atomEntry.Updated = new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero);
      atomEntry.Id = new Uri("http://localhost/#1");

      TestInterface(atomEntry);
    }

    [Test]
    public void TestInterfaceRdfRss()
    {
      var rssItem = new RdfRss.Item();

      rssItem.Link = new Uri("http://localhost/1");
      rssItem.Title = "title";
      rssItem.Description = "description";
      rssItem.Modules.Add(DublinCore.NamespaceUri, new DublinCore());
      rssItem.DublinCoreModule.Date = new DateTimeOffset?[] {new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero)};
      rssItem.Resource = new Uri("http://localhost/#1");

      TestInterface(rssItem);
    }

    [Test]
    public void TestInterfaceRss()
    {
      var rssItem = new Rss.Item();

      rssItem.Link = new Uri("http://localhost/1");
      rssItem.Title = "title";
      rssItem.Description = "description";
      rssItem.PubDate = new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero);
      rssItem.Guid = new Rss.Guid("http://localhost/#1", true);

      TestInterface(rssItem);
    }
  }
}