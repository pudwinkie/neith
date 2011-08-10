using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

using Smdn.Xml;

namespace Smdn.Formats.Playlists {
  [TestFixture]
  public class PlaylistTests {
    private void UsingTempFile(string filename, string content, Action<string> action)
    {
      UsingTempFile(filename, content, null, action);
    }

    private void UsingTempFile(string filename, string content, Encoding encoding, Action<string> action)
    {
      try {
        if (File.Exists(filename))
          File.Delete(filename);

        File.WriteAllText(filename, content, encoding ?? Encoding.Default);

        action(filename);
      }
      finally {
        if (File.Exists(filename))
          File.Delete(filename);
      }
    }

    [Test]
    public void LoadFromM3UTest()
    {
      var content = @"
/home/test/music1.m4a
#EXTINF:120,track title
/home/test/music2.m4a
";

      UsingTempFile("test.m3u", content, delegate(string file) {
        var m3u = Playlist.LoadFrom(file);

        Assert.AreEqual(2, m3u.Entries.Count);

        Assert.IsNotNull(m3u.Entries[0].Location);
        Assert.AreEqual("/home/test/music1.m4a", m3u.Entries[0].FileLocation);
        Assert.IsNull(m3u.Entries[0].Title);
        Assert.IsNull(m3u.Entries[0].Duration);

        Assert.IsNotNull(m3u.Entries[1].Location);
        Assert.AreEqual("/home/test/music2.m4a", m3u.Entries[1].FileLocation);
        Assert.IsNull(m3u.Entries[1].Title); // extinf must be parsed as comment
        Assert.IsNull(m3u.Entries[1].Duration);
      });
    }

    [Test]
    public void LoadFromEXTM3UTest()
    {
      var content = @"#EXTM3U
/home/test/music1.m4a
#EXTINF:,track title
file:///home/test/music2.m4a
#EXTINF:-1,
C:\Users\ユーザ\Musics\music3.m4a
#EXTINF:120,track title
#comment
file:///C:/Users/ユーザ/Musics/music4.m4a
http://example.com/streaming.asx
";

      UsingTempFile("test.m3u", content, delegate(string file) {
        var m3u = Playlist.LoadFrom(file);

        Assert.AreEqual(5, m3u.Entries.Count);

        Assert.IsNotNull(m3u.Entries[0].Location);
        Assert.AreEqual("/home/test/music1.m4a", m3u.Entries[0].FileLocation);
        Assert.IsNull(m3u.Entries[0].Title); // extinf not exist
        Assert.AreEqual(null, m3u.Entries[0].Duration);

        Assert.IsNotNull(m3u.Entries[1].Location);
        Assert.AreEqual("/home/test/music2.m4a", m3u.Entries[1].FileLocation);
        Assert.AreEqual("track title", m3u.Entries[1].Title);
        Assert.AreEqual(null, m3u.Entries[1].Duration);

        Assert.IsNotNull(m3u.Entries[2].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music3.m4a", m3u.Entries[2].FileLocation);
        Assert.IsEmpty(m3u.Entries[2].Title); // extinf exists
        Assert.AreEqual(null, m3u.Entries[2].Duration);

        Assert.IsNotNull(m3u.Entries[3].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music4.m4a", m3u.Entries[3].FileLocation);
        Assert.AreEqual("track title", m3u.Entries[3].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(120), m3u.Entries[3].Duration);

        Assert.IsNotNull(m3u.Entries[4].Location);
        Assert.AreEqual(new Uri("http://example.com/streaming.asx"), m3u.Entries[4].Location);
      });
    }

    [Test]
    public void LoadFromPLSTest()
    {
      var content = @"[playlist]
NumberOfEntries=5
File1=/home/test/music1.m4a
Title4=track title
File2=file:///home/test/music2.m4a
File3=C:\Users\ユーザ\Musics\music3.m4a
Length3=120
Length4=120
File4=file:///C:/Users/ユーザ/Musics/music4.m4a
Title2=track title
Version=2
File5=http://example.com/streaming.asx
";

      UsingTempFile("test.pls", content, delegate(string file) {
        var pls = Playlist.LoadFrom(file);

        Assert.AreEqual(5, pls.Entries.Count);

        Assert.IsNotNull(pls.Entries[0].Location);
        Assert.AreEqual("/home/test/music1.m4a", pls.Entries[0].FileLocation);
        Assert.IsNull(pls.Entries[0].Title);
        Assert.AreEqual(null, pls.Entries[0].Duration);

        Assert.IsNotNull(pls.Entries[1].Location);
        Assert.AreEqual("/home/test/music2.m4a", pls.Entries[1].FileLocation);
        Assert.AreEqual("track title", pls.Entries[1].Title);
        Assert.AreEqual(null, pls.Entries[1].Duration);

        Assert.IsNotNull(pls.Entries[2].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music3.m4a", pls.Entries[2].FileLocation);
        Assert.IsNull(pls.Entries[2].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(120), pls.Entries[2].Duration);

        Assert.IsNotNull(pls.Entries[3].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music4.m4a", pls.Entries[3].FileLocation);
        Assert.AreEqual("track title", pls.Entries[3].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(120), pls.Entries[3].Duration);

        Assert.IsNotNull(pls.Entries[4].Location);
        Assert.AreEqual(new Uri("http://example.com/streaming.asx"), pls.Entries[4].Location);
      });
    }

    [Test, ExpectedException(typeof(InvalidDataException))]
    public void LoadFromPLSNumberOfEntriesNotExistTest()
    {
      var content = @"[playlist]
File1=/home/test/music1.m4a
";
      UsingTempFile("test.pls", content, delegate(string file) {
        Playlist.LoadFrom(file);
      });
    }

    [Test, ExpectedException(typeof(InvalidDataException))]
    public void LoadFromPLSPlaylistSectionNotFoundTest()
    {
      var content = @"File1=/home/test/music1.m4a
";

      UsingTempFile("test.pls", content, delegate(string file) {
        Playlist.LoadFrom(file);
      });
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void LoadFromPLSVersionExistButNotSupportedTest()
    {
      var content = @"[playlist]
Version=1
NumberOfEntries=1
File1=/home/test/music1.m4a
";
      UsingTempFile("test.pls", content, delegate(string file) {
        Playlist.LoadFrom(file);
      });
    }

    [Test, ExpectedException(typeof(InvalidDataException))]
    public void LoadFromPLSIndexGreaterThanNumberOfEntries()
    {
      var content = @"[playlist]
NumberOfEntries=1
File2=/home/test/music1.m4a
";
      UsingTempFile("test.pls", content, delegate(string file) {
        Playlist.LoadFrom(file);
      });
    }


    [Test]
    public void LoadFromXSPFVersion1Test()
    {
      var content = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<playlist version=""1"" xmlns=""http://xspf.org/ns/0/"">
  <title>title of playlist</title>
  <creator>creator of playlist</creator>
  <date>2009-11-03T17:16:22+09:00</date>
  <trackList>
    <track>
      <location>file:///home/test/music1.m4a</location>
    </track>
    <track>
      <title>track title</title>
      <location>file:///home/test/music2.m4a</location>
    </track>
    <track>
      <duration>120000</duration>
      <location>file:///C:/Users/%E3%83%A6%E3%83%BC%E3%82%B6/Musics/music3.m4a</location>
    </track>
    <track>
      <duration>120000</duration>
      <title>track title</title>
      <location>file:///C:/Users/%E3%83%A6%E3%83%BC%E3%82%B6/Musics/music4.m4a</location>
      <album>album title</album>
      <trackNum>3</trackNum>
    </track>
    <track>
      <location>http://example.com/streaming.asx</location>
    </track>
  </trackList>
</playlist>
";

      UsingTempFile("test.xspf", content, delegate(string file) {
        var xspf = Playlist.LoadFrom(file);

        Assert.AreEqual("title of playlist", xspf.Title);
        Assert.AreEqual("creator of playlist", xspf.Creator);
        Assert.AreEqual(new DateTimeOffset(2009, 11, 3, 17, 16, 22, new TimeSpan(+9, 0, 0)), xspf.CreationDate);

        Assert.AreEqual(5, xspf.Entries.Count);

        Assert.IsNotNull(xspf.Entries[0].Location);
        Assert.AreEqual("/home/test/music1.m4a", xspf.Entries[0].FileLocation);
        Assert.IsNull(xspf.Entries[0].Title);
        Assert.AreEqual(null, xspf.Entries[0].Duration);

        Assert.IsNotNull(xspf.Entries[1].Location);
        Assert.AreEqual("/home/test/music2.m4a", xspf.Entries[1].FileLocation);
        Assert.AreEqual("track title", xspf.Entries[1].Title);
        Assert.AreEqual(null, xspf.Entries[1].Duration);

        Assert.IsNotNull(xspf.Entries[2].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music3.m4a", xspf.Entries[2].FileLocation);
        Assert.IsNull(xspf.Entries[2].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(120), xspf.Entries[2].Duration);

        Assert.IsNotNull(xspf.Entries[3].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music4.m4a", xspf.Entries[3].FileLocation);
        Assert.AreEqual("track title", xspf.Entries[3].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(120), xspf.Entries[3].Duration);
        Assert.AreEqual("album title", xspf.Entries[3].AlbumTitle);
        Assert.AreEqual(3, xspf.Entries[3].TrackNumber);

        Assert.IsNotNull(xspf.Entries[4].Location);
        Assert.AreEqual(new Uri("http://example.com/streaming.asx"), xspf.Entries[4].Location);
      });
    }

    [Test]
    public void LoadFromWPLTest()
    {
      var content = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<smil>
  <head>
    <title>title of playlist</title>
    <meta name=""Author"" content=""author of playlist"" />
    <meta name=""Generator"" content=""generator of playlist"" />
  </head>
  <body>
    <seq>
      <media src=""/home/test/music1.m4a"" />
      <media src=""file:///home/test/music2.m4a"" />
      <media src=""C:\Users\ユーザ\Musics\music3.m4a"" />
      <media src=""file:///C:/Users/ユーザ/Musics/music4.m4a"" />
      <media src=""http://example.com/streaming.asx"" />
    </seq>
  </body>
</smil>
";

      UsingTempFile("test.wpl", content, delegate(string file) {
        var wpl = Playlist.LoadFrom(file);

        Assert.AreEqual("title of playlist", wpl.Title);
        Assert.AreEqual("author of playlist", wpl.Author);
        Assert.AreEqual("generator of playlist", wpl.Generator);

        Assert.AreEqual(5, wpl.Entries.Count);

        Assert.IsNotNull(wpl.Entries[0].Location);
        Assert.AreEqual("/home/test/music1.m4a", wpl.Entries[0].FileLocation);
        Assert.IsNotNull(wpl.Entries[1].Location);
        Assert.AreEqual("/home/test/music2.m4a", wpl.Entries[1].FileLocation);
        Assert.IsNotNull(wpl.Entries[2].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music3.m4a", wpl.Entries[2].FileLocation);
        Assert.IsNotNull(wpl.Entries[3].Location);
        Assert.AreEqual(@"C:\Users\ユーザ\Musics\music4.m4a", wpl.Entries[3].FileLocation);
        Assert.IsNotNull(wpl.Entries[4].Location);
        Assert.AreEqual(new Uri("http://example.com/streaming.asx"), wpl.Entries[4].Location);
      });
    }

    private void UsingPlaylist(string file, Action<string, Playlist> action)
    {
      try {
        if (File.Exists(file))
          File.Delete(file);

        var playlist = new Playlist(new[] {
          new PlaylistEntry(new Uri("file:///home/test/music1.m4a")) {Duration = null, Title = null},
          new PlaylistEntry(new Uri("/home/music2.m4a")) {Duration = TimeSpan.FromSeconds(120), Title = null},
          new PlaylistEntry(new Uri("file:///home/test2/music3.m4a")) {Duration = null, Title = "track title"},
          new PlaylistEntry(new Uri("/home/test2/test3/music4.m4a")) {Duration = TimeSpan.FromSeconds(120), Title = "track title", AlbumTitle = "album title", TrackNumber = 3},
          new PlaylistEntry(new Uri(@"C:\Users\ユーザ\Music\トラック - アルバム.wma")) {Duration = null, Title = "トラックタイトル", AlbumTitle = "アルバムタイトル"},
          new PlaylistEntry(new Uri("http://example.com/streaming.asx")),
        });

        playlist.Title = "title of playlist";
        playlist.Author = "author of playlist";
        playlist.Generator = "generator of playlist";
        playlist.Creator = "creator of playlist";
        playlist.CreationDate = new DateTimeOffset(2009, 11, 3, 17, 2, 52, 0, new TimeSpan(+9, 0, 0));

        action(file, playlist);
      }
      finally {
        if (File.Exists(file))
          File.Delete(file);
      }
    }

    [Test]
    public void SaveAsM3UTest()
    {
      UsingPlaylist("test.m3u", delegate(string file, Playlist playlist) {
        playlist.Save(file);

        var actual = File.ReadAllText(file, Encoding.Default).TrimEnd();
        var expected = @"#EXTM3U
/home/test/music1.m4a
#EXTINF:120,
/home/music2.m4a
#EXTINF:-1,track title
/home/test2/music3.m4a
#EXTINF:120,track title
/home/test2/test3/music4.m4a
#EXTINF:-1,トラックタイトル
C:\Users\ユーザ\Music\トラック - アルバム.wma
http://example.com/streaming.asx
".TrimEnd();

        Assert.AreEqual(expected, actual);
      });
    }

    [Test, Ignore("not works")]
    public void SaveAsM3UWithBaseUriTest()
    {
      UsingPlaylist("test.m3u", delegate(string file, Playlist playlist) {
        playlist.Save(file, new Uri("/home/test/"));

        var actual = File.ReadAllText(file, Encoding.Default).TrimEnd();
        var expected = @"#EXTM3U
music1.m4a
#EXTINF:120,
../music2.m4a
#EXTINF:-1,track title
../test2/music3.m4a
#EXTINF:120,track title
../test2/test3/music4.m4a
#EXTINF:-1,トラックタイトル
C:\Users\ユーザ\Music\トラック - アルバム.wma
http://example.com/streaming.asx
".TrimEnd();

        Assert.AreEqual(expected, actual);
      });
    }

    [Test]
    public void SaveAsPLSTest()
    {
      UsingPlaylist("test.pls", delegate(string file, Playlist playlist) {
        playlist.Save(file);

        var actual = File.ReadAllText(file, Encoding.Default).TrimEnd();
        var expected = @"[playlist]
Version=2
NumberOfEntries=6

File1=/home/test/music1.m4a

File2=/home/music2.m4a
Length2=120

File3=/home/test2/music3.m4a
Title3=track title

File4=/home/test2/test3/music4.m4a
Title4=track title
Length4=120

File5=C:\Users\ユーザ\Music\トラック - アルバム.wma
Title5=トラックタイトル

File6=http://example.com/streaming.asx
".TrimEnd();

        Assert.AreEqual(expected, actual);
      });
    }

    [Test, Ignore("not works")]
    public void SaveAsPLSWithBaseUriTest()
    {
      UsingPlaylist("test.pls", delegate(string file, Playlist playlist) {
        playlist.Save(file, new Uri("/home/test/"));

        var actual = File.ReadAllText(file, Encoding.Default).TrimEnd();
        var expected = @"[playlist]
Version=2
NumberOfEntries=6

File1=music1.m4a

File2=../music2.m4a
Length2=120

File3=../test2/music3.m4a
Title3=track title

File4=../test2/test3/music4.m4a
Title4=track title
Length4=120

File5=C:\Users\ユーザ\Music\トラック - アルバム.wma
Title5=トラックタイトル

File6=http://example.com/streaming.asx
".TrimEnd();

        Assert.AreEqual(expected, actual);
      });
    }

    [Test]
    public void SaveAsXSPFTest()
    {
      UsingPlaylist("test.xspf", delegate(string file, Playlist playlist) {
        playlist.Save(file);

        var xml = new XmlDocument();

        xml.Load(file);

        var nsmgr = new XmlNamespaceManager(xml.NameTable);

        nsmgr.AddNamespace("xspf", "http://xspf.org/ns/0/");

        AreNodeValueEquals("1", xml, "/xspf:playlist/@version", nsmgr);

        AreNodeValueEquals("title of playlist", xml, "/xspf:playlist/xspf:title/text()", nsmgr);
        AreNodeValueEquals("creator of playlist", xml, "/xspf:playlist/xspf:creator/text()", nsmgr);
        AreNodeValueEquals("2009-11-03T17:02:52.0000000+09:00", xml, "/xspf:playlist/xspf:date/text()", nsmgr);

        AreNodeValueEquals("file:///home/test/music1.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[1]/xspf:location/text()", nsmgr);
        AreNodeValueEquals(null, xml, "/xspf:playlist/xspf:trackList/xspf:track[1]/xspf:title/text()", nsmgr);
        AreNodeValueEquals(null, xml, "/xspf:playlist/xspf:trackList/xspf:track[1]/xspf:duration/text()", nsmgr);

        AreNodeValueEquals("file:///home/music2.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[2]/xspf:location/text()", nsmgr);
        AreNodeValueEquals(null, xml, "/xspf:playlist/xspf:trackList/xspf:track[2]/xspf:title/text()", nsmgr);
        AreNodeValueEquals("120000", xml, "/xspf:playlist/xspf:trackList/xspf:track[2]/xspf:duration/text()", nsmgr);

        AreNodeValueEquals("file:///home/test2/music3.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[3]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("track title", xml, "/xspf:playlist/xspf:trackList/xspf:track[3]/xspf:title/text()", nsmgr);
        AreNodeValueEquals(null, xml, "/xspf:playlist/xspf:trackList/xspf:track[3]/xspf:duration/text()", nsmgr);

        AreNodeValueEquals("file:///home/test2/test3/music4.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[4]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("track title", xml, "/xspf:playlist/xspf:trackList/xspf:track[4]/xspf:title/text()", nsmgr);
        AreNodeValueEquals("120000", xml, "/xspf:playlist/xspf:trackList/xspf:track[4]/xspf:duration/text()", nsmgr);
        AreNodeValueEquals("album title", xml, "/xspf:playlist/xspf:trackList/xspf:track[4]/xspf:album/text()", nsmgr);
        AreNodeValueEquals("3", xml, "/xspf:playlist/xspf:trackList/xspf:track[4]/xspf:trackNum/text()", nsmgr);

        AreNodeValueEquals("file:///C:/Users/%E3%83%A6%E3%83%BC%E3%82%B6/Music/%E3%83%88%E3%83%A9%E3%83%83%E3%82%AF%20-%20%E3%82%A2%E3%83%AB%E3%83%90%E3%83%A0.wma", xml, "/xspf:playlist/xspf:trackList/xspf:track[5]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("トラックタイトル", xml, "/xspf:playlist/xspf:trackList/xspf:track[5]/xspf:title/text()", nsmgr);
        AreNodeValueEquals("アルバムタイトル", xml, "/xspf:playlist/xspf:trackList/xspf:track[5]/xspf:album/text()", nsmgr);

        AreNodeValueEquals("http://example.com/streaming.asx", xml, "/xspf:playlist/xspf:trackList/xspf:track[6]/xspf:location/text()", nsmgr);
      });
    }

    [Test, Ignore("not works")]
    public void SaveAsXSPFWithBaseUriTest()
    {
      UsingPlaylist("test.xspf", delegate(string file, Playlist playlist) {
        playlist.Save(file, new Uri("/home/test/"));

        var xml = new XmlDocument();

        xml.Load(file);

        var nsmgr = new XmlNamespaceManager(xml.NameTable);

        nsmgr.AddNamespace("xspf", "http://xspf.org/ns/0/");

        AreNodeValueEquals("music1.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[1]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("../music2.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[2]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("../test2/music3.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[3]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("../test2/test3/music4.m4a", xml, "/xspf:playlist/xspf:trackList/xspf:track[4]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("file:///C:/Users/%E3%83%A6%E3%83%BC%E3%82%B6/Music/%E3%83%88%E3%83%A9%E3%83%83%E3%82%AF%20-%20%E3%82%A2%E3%83%AB%E3%83%90%E3%83%A0.wma", xml, "/xspf:playlist/xspf:trackList/xspf:track[5]/xspf:location/text()", nsmgr);
        AreNodeValueEquals("http://example.com/streaming.asx", xml, "/xspf:playlist/xspf:trackList/xspf:track[6]/xspf:location/text()", nsmgr);
      });
    }

    [Test]
    public void SaveAsWPLTest()
    {
      UsingPlaylist("test.wpl", delegate(string file, Playlist playlist) {
        playlist.Save(file);

        var xml = new XmlDocument();

        xml.Load(file);

        AreNodeValueEquals("title of playlist", xml, "/smil/head/title/text()");
        AreNodeValueEquals("author of playlist", xml, "/smil/head/meta[@name='Author']/@content");
        AreNodeValueEquals("generator of playlist", xml, "/smil/head/meta[@name='Generator']/@content");

        AreNodeValueEquals("/home/test/music1.m4a", xml, "/smil/body/seq/media[1]/@src");
        AreNodeValueEquals("/home/music2.m4a", xml, "/smil/body/seq/media[2]/@src");
        AreNodeValueEquals("/home/test2/music3.m4a", xml, "/smil/body/seq/media[3]/@src");
        AreNodeValueEquals("/home/test2/test3/music4.m4a", xml, "/smil/body/seq/media[4]/@src");
        AreNodeValueEquals(@"C:\Users\ユーザ\Music\トラック - アルバム.wma", xml, "/smil/body/seq/media[5]/@src");
        AreNodeValueEquals("http://example.com/streaming.asx", xml, "/smil/body/seq/media[6]/@src");
      });
    }

    [Test, Ignore("not works")]
    public void SaveAsWPLWithBaseUriTest()
    {
      UsingPlaylist("test.wpl", delegate(string file, Playlist playlist) {
        playlist.Save(file, new Uri("/home/test/"));

        var xml = new XmlDocument();

        xml.Load(file);

        AreNodeValueEquals("music1.m4a", xml, "/smil/body/seq/media[1]/@src");
        AreNodeValueEquals("../music2.m4a", xml, "/smil/body/seq/media[2]/@src");
        AreNodeValueEquals("../test2/music3.m4a", xml, "/smil/body/seq/media[3]/@src");
        AreNodeValueEquals("../test2/test3/music4.m4a", xml, "/smil/body/seq/media[4]/@src");
        AreNodeValueEquals(@"C:\Users\ユーザ\Music\トラック - アルバム.wma", xml, "/smil/body/seq/media[5]/@src");
        AreNodeValueEquals("http://example.com/streaming.asx", xml, "/smil/body/seq/media[6]/@src");
      });
    }

    private static void AreNodeValueEquals(string expected, XmlNode node, string xpath)
    {
      AreNodeValueEquals(expected, node, xpath, null);
    }

    private static void AreNodeValueEquals(string expected, XmlNode node, string xpath, XmlNamespaceManager nsmgr)
    {
      Assert.AreEqual(expected, node.GetSingleNodeValueOf(xpath, nsmgr), string.Format("value of {0}", xpath));
    }

    [Test, ExpectedException(typeof(NotSupportedException))]
    public void SaveAsUnsupportedFormatTest()
    {
      UsingPlaylist("test.hoge", delegate(string file, Playlist playlist) {
        playlist.Save(file);
      });
    }
  }
}
