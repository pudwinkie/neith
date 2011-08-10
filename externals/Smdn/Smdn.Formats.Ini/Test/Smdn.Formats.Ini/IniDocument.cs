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
  public class IniDocumentTests {
    private IniDocument document = null;
    private string testDocument = @"
default=デフォルト
;comment
[section2]
#comment
url=http://example.com/;param
[section1]
key1=      value1    
key2   =  value2=hoge";

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
    public void TestConstructWithDefaultComparer()
    {
      var doc = IniDocument.Load(new StringReader(@"
[section1]
index=1
"));

      Assert.AreEqual("1", doc["section1"]["index"]);
      Assert.AreEqual("1", doc["Section1"]["index"]);
      Assert.AreEqual("1", doc["SECTION1"]["index"]);
      Assert.AreEqual("1", doc["section1"]["index"]);
      Assert.AreEqual("1", doc["section1"]["Index"]);
      Assert.AreEqual("1", doc["section1"]["INDEX"]);
    }

    [Test]
    public void TestConstructWithCaseSensitiveComparer()
    {
      var doc = IniDocument.Load(new StringReader(@"
[section1]
index=1
"), StringComparer.Ordinal);

      Assert.AreEqual("1", doc["section1"]["index"]);
      Assert.AreEqual(null, doc["Section1"]["index"]);
      Assert.AreEqual(null, doc["SECTION1"]["index"]);
      Assert.AreEqual("1", doc["section1"]["index"]);
      Assert.AreEqual(null, doc["section1"]["Index"]);
      Assert.AreEqual(null, doc["section1"]["INDEX"]);
    }

    [Test]
    public void GetExistSection()
    {
      Assert.IsNotNull(document["section1"]);
      Assert.IsNotNull(document["section2"]);

      Assert.AreEqual("section1", document["section1"].Name);
      Assert.AreEqual("section2", document["section2"].Name);
    }

    [Test]
    public void GetExistEntry()
    {
      Assert.AreEqual(2, document["section1"].Entries.Count);
      Assert.AreEqual(1, document["section2"].Entries.Count);

      Assert.AreEqual("value1", document["section1"]["key1"]);
      Assert.AreEqual("value2=hoge", document["section1"]["key2"]);

      Assert.AreEqual("http://example.com/;param", document["section2"]["url"]);
    }

    [Test]
    public void GetNonExistentSection()
    {
      Assert.AreEqual(0, document["NonExistent"].Entries.Count);
    }

    [Test]
    public void GetNonExistentEntry()
    {
      Assert.IsNull(document["NonExistent"]["NonExistent"]);
    }

    [Test]
    public void TestDefaultSections()
    {
      Assert.IsNotNull(document[null]);
      Assert.IsTrue(document[null].IsDefaultSection);
      Assert.AreEqual(string.Empty, document[null].Name);
      Assert.AreEqual(document.DefaultSection, document[null]);
      
      Assert.IsNotNull(document[string.Empty]);
      Assert.IsTrue(document[string.Empty].IsDefaultSection);
      Assert.AreEqual(string.Empty, document[string.Empty].Name);
      Assert.AreEqual(document.DefaultSection, document[string.Empty]);

      Assert.IsNotNull(document[""]);
      Assert.IsTrue(document[""].IsDefaultSection);
      Assert.AreEqual(string.Empty, document[""].Name);
      Assert.AreEqual(document.DefaultSection, document[""]);

      Assert.IsNotNull(document.DefaultSection);
      Assert.IsTrue(document.DefaultSection.IsDefaultSection);
      Assert.AreEqual(string.Empty, document.DefaultSection.Name);
      Assert.AreEqual("デフォルト", document[""]["default"]);
    }

    [Test]
    public void EnumerateSections()
    {
      int count = 0;
      foreach (var section in document) {
        Assert.IsTrue(section is IniSection);
        count++;
      }
      Assert.AreEqual(3, count);
    }

    [Test]
    public void EnumerateEntries()
    {
      int count = 0;
      foreach (var entry in document[""]) {
        Assert.IsInstanceOfType(typeof(KeyValuePair<string, string>), entry);
        count++;
      }
      Assert.AreEqual(1, count);
    }

    [Test]
    public void TestRemove()
    {
      document.Remove("section1");
      Assert.AreEqual(2, document.Sections.Count());

      document.Remove("section2");
      Assert.AreEqual(1, document.Sections.Count());

      document.Remove(null);
      Assert.AreEqual(1, document.Sections.Count());

      document.Remove(string.Empty);
      Assert.AreEqual(1, document.Sections.Count());
    }
  }
}
