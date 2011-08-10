using System;
using NUnit.Framework;

using Smdn.Formats.Feeds.Modules;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class IFeedTest {
    private void TestInterface(IFeed feed)
    {
      Assert.AreEqual(new Uri("http://localhost/"), feed.Link);
      if (feed is Rss.Channel)
        Assert.IsNull(feed.Uri);
      else
        Assert.AreEqual(new Uri("http://localhost/rss"), feed.Uri);
      Assert.AreEqual("title", feed.Title);
      Assert.AreEqual("description", feed.Description);
      Assert.AreEqual(new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero), feed.Date);
    }

    [Test]
    public void TestInterfaceAtom()
    {
      var atomFeed = new Atom.Feed();

      atomFeed.Links.Add(new Atom.Link(new Uri("http://localhost/"), "alternate"));
      atomFeed.Links.Add(new Atom.Link(new Uri("http://localhost/rss"), "self"));
      atomFeed.Title = new Atom.Text("title");
      atomFeed.Subtitle = new Atom.Text("description");
      atomFeed.Updated = new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero);

      TestInterface(atomFeed);
    }

    [Test]
    public void TestInterfaceRdfRss()
    {
      var rssChannel = new RdfRss.Channel();

      rssChannel.Link = new Uri("http://localhost/");
      rssChannel.Uri = new Uri("http://localhost/rss");
      rssChannel.Title = "title";
      rssChannel.Description = "description";
      rssChannel.Modules.Add(DublinCore.NamespaceUri, new DublinCore());
      rssChannel.DublinCoreModule.Date = new DateTimeOffset?[]{new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero)};

      TestInterface(rssChannel);
    }

    [Test]
    public void TestInterfaceRss()
    {
      var rssChannel = new Rss.Channel();

      rssChannel.Link = new Uri("http://localhost/");
      rssChannel.Title = "title";
      rssChannel.Description = "description";
      rssChannel.PubDate = new DateTimeOffset(2009, 1, 1, 0, 0, 0, TimeSpan.Zero);

      TestInterface(rssChannel);
    }
  }
}
