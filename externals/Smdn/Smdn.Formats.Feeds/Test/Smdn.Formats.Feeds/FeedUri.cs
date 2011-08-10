using System;
using NUnit.Framework;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class FeedUriTest {
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

    [Test, ExpectedException(typeof(UriFormatException))]
    public void TestInvalidScheme()
    {
      new FeedUri("file:///tmp/test.xml");
    }

    [Test]
    public void TestFeedScheme()
    {
      var uri = new FeedUri("feed://example.com/rss.xml");

      Assert.AreEqual(FeedUri.UriSchemeHttpFeed, uri.Scheme);
      Assert.AreEqual("feed:http://example.com/rss.xml", uri.ToString());
    }

    [Test]
    public void TestFeedHttpScheme()
    {
      var uri = new FeedUri("feed:http://example.com/rss.xml");

      Assert.AreEqual(FeedUri.UriSchemeHttpFeed, uri.Scheme);
      Assert.AreEqual("feed:http://example.com/rss.xml", uri.ToString());
    }

    [Test]
    public void TestFeedHttpsScheme()
    {
      var uri = new FeedUri("feed:https://example.com/rss.xml");

      Assert.AreEqual(FeedUri.UriSchemeHttpsFeed, uri.Scheme);
      Assert.AreEqual("feed:https://example.com/rss.xml", uri.ToString());
    }

    [Test]
    public void TestFeedFtpScheme()
    {
      var uri = new FeedUri("feed:ftp://example.com/rss.xml");

      Assert.AreEqual(FeedUri.UriSchemeFtpFeed, uri.Scheme);
      Assert.AreEqual("feed:ftp://example.com/rss.xml", uri.ToString());
    }
  }
}