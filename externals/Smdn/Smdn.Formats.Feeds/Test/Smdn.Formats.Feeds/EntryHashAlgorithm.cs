using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Smdn.Formats.Feeds {
  [TestFixture]
  public class EntryHashAlgorithmTest {
    [Test]
    public void TestEntryHash()
    {
      var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
  <title type=""text"">title</title>
  <id>tag:example.org,2009:1</id>
  <entry>
    <title>0</title>
    <id>tag:example.org,2009:1.0</id>
  </entry>
  <entry>
    <title>1</title>
    <id>tag:example.org,2009:1.1</id>
  </entry>
  <entry>
    <title>2</title>
    <id>tag:example.org,2009:1.2</id>
  </entry>
  <entry>
    <title>3</title>
    <id>tag:example.org,2009:1.3</id>
  </entry>
  <entry>
    <title>4</title>
    <id>tag:example.org,2009:1.4</id>
  </entry>
</feed>
";

      using (var hasher = new EntryHashAlgorithm<SHA1>()) {
        var feed = Parser.Parse(xml, hasher);
        var hashes = new List<string>();

        foreach (var entry in feed.Entries) {
          Assert.IsNotNull(entry.Hash, "hash is not null");

          var hash = Convert.ToBase64String(entry.Hash);

          Assert.IsFalse(hashes.Contains(hash), "check hash collision: {0}", hash);

          hashes.Add(hash);
        }
      }
    }

    [Test]
    public void TestEntryHashNotGenerate()
    {
      using (var reader = new StreamReader("Rss10Example.xml")) {
        var feed = Parser.Parse(reader);

        foreach (var entry in feed.Entries) {
          Assert.IsNull(entry.Hash, "hash is null");
        }
      }
    }
  }
}
