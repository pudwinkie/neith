using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Formats.Ini {
  [TestFixture()]
  public class ParserTest {
    private IniDocument document = null;
    private string testDocument = @"
############################
# sample ini file
############################
default=123

  [section1]
  key = value

  [section2]
    key=value

[section1]
key2=value2

[pairs]
; comment line" +
"key1=        value" +
"\tkey2=      value    value" +
"key3\t= value ; comment" +
@"
key4 =value
key5=value=test
key6=   value == test
key7 = value ;comment
key8 = value ; comment

[inline comment]
url1=http://example.com/;param
url2=http://example.com/;param ; end with comment
";
    [SetUp]
    public void Setup()
    {
      var reader = new StringReader(testDocument);

      document = IniDocument.Load(reader);
    }

    [TearDown]
    public void TearDown()
    {
      // nothing to do
    }

    [Test]
    public void ParseSections()
    {
      Assert.AreEqual(5, document.Sections.Count());
      Assert.AreEqual("", document[""].Name);
      Assert.AreEqual("section1", document["section1"].Name);
      Assert.AreEqual("section2", document["section2"].Name);
    }

    [Test]
    public void ParseEntries()
    {
      Assert.AreEqual(1, document[""].Entries.Count);
      Assert.AreEqual(2, document["section1"].Entries.Count);
      Assert.AreEqual(1, document["section2"].Entries.Count);
    }

    [Test]
    public void ParseEntry()
    {
      foreach (var entry in document["pairs"]) {
        Assert.IsFalse(entry.Key.StartsWith(" "), "key mustn't start with SP");
        Assert.IsFalse(entry.Key.EndsWith(" "), "key mustn't end with SP");
        Assert.IsFalse(entry.Key.StartsWith("\t"), "key mustn't start with TAB");
        Assert.IsFalse(entry.Key.EndsWith("\t"), "key mustn't end with TAB");
        Assert.IsFalse(entry.Key.Contains("="), "key mustn't contain separator");
        Assert.IsFalse(entry.Value.StartsWith(" "), "value mustn't start with SP");
        Assert.IsFalse(entry.Value.EndsWith(" "), "value mustn't end with SP");
        Assert.IsFalse(entry.Value.StartsWith("\t"), "value mustn't start with TAB");
        Assert.IsFalse(entry.Value.EndsWith("\t"), "value mustn't end with TAB");
        Assert.IsFalse(entry.Value.Contains("comment"), "value mustn't contain comment");
      }

      Assert.IsTrue(document["inline comment"]["url1"] == document["inline comment"]["url2"]);
    }
  }
}
