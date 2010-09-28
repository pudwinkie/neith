using System;
using NUnit.Framework;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class ImapBodyStructureUtilsTests {
    private IImapBodyStructure BuildMultiPart()
    {
      var part1 = new ImapSinglePartBodyStructure("1", new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);

      var part21 = new ImapSinglePartBodyStructure("2.1", new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);
      var part22 = new ImapSinglePartBodyStructure("2.2", new MimeType("text", "html"), null, null, null, "7bit", 0L, 0L);
      var part2 = new ImapMultiPartBodyStructure("2",
                                                 new[] {part21, part22},
                                                 "alternative");

      var part31 = new ImapSinglePartBodyStructure("3.1", new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);
      var part3Envelope = new ImapEnvelope(null, null, null, null, null, null, null, null, null, null);
      var part3 = new ImapMessageRfc822BodyStructure("3", new MimeType("message", "rfc822"), null, null, null, "7bit", 0L, part3Envelope, part31, 0L);

      return new ImapMultiPartBodyStructure(string.Empty,
                                            new IImapBodyStructure[] {part1, part2, part3},
                                            "mixed");
    }

    [Test]
    public void TestTraverseSinglePart()
    {
      var single = new ImapSinglePartBodyStructure(string.Empty, new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);

      ImapBodyStructureUtils.Traverse(single, delegate(IImapBodyStructure s) {
        Assert.Fail("enumerated");
      });
    }

    [Test]
    public void TestTraverseMultiPart()
    {
      var expectedSections = new[] {"1", "2", "2.1", "2.2", "3", "3.1"};
      var index = 0;

      ImapBodyStructureUtils.Traverse(BuildMultiPart(), delegate(IImapBodyStructure s) {
        Assert.AreEqual(expectedSections[index++], s.Section);
      });
    }

    [Test]
    public void TestFindSection1()
    {
      var structure = BuildMultiPart();
      var found = structure.FindSection(3, 1);

      Assert.IsNotNull(found);
      Assert.AreEqual("3.1", found.Section);
      Assert.AreEqual(MimeType.TextPlain, found.MediaType);

      found = structure.FindSection(4, 1);

      Assert.IsNull(found);

      Assert.IsNull(structure.FindSection());
    }

    [Test]
    public void TestFindSection2()
    {
      var structure = BuildMultiPart();
      var found = structure.FindSection("3");

      Assert.IsNotNull(found);
      Assert.AreEqual("3", found.Section);
      Assert.AreEqual(new MimeType("message/rfc822"), found.MediaType);

      found = structure.FindSection("4.1");

      Assert.IsNull(found);

      Assert.IsNull(structure.FindSection((string)null));
      Assert.IsNull(structure.FindSection(string.Empty));
    }

    [Test]
    public void TestFind()
    {
      var structure = BuildMultiPart();
      var found = structure.Find(delegate(IImapBodyStructure s) {
        return s is ImapMessageRfc822BodyStructure;
      });

      Assert.IsNotNull(found);
      Assert.AreEqual("3", found.Section);

      found = structure.Find(delegate(IImapBodyStructure s) {
        return s is ImapExtendedMessageRfc822BodyStructure;
      });

      Assert.IsNull(found);
    }

    [Test]
    public void TestFindAll()
    {
      var structure = BuildMultiPart();
      var found = structure.FindAll(delegate(IImapBodyStructure s) {
        return !s.IsMultiPart;
      });

      var expectedSections = new[] {"1", "2.1", "2.2", "3", "3.1"};
      var index = 0;

      Assert.IsNotNull(found);

      foreach (var s in found) {
        Assert.IsNotNull(s);
        Assert.AreEqual(expectedSections[index++], s.Section);
      }

      found = structure.FindAll(delegate(IImapBodyStructure s) {
        return s is IImapBodyStructureExtension;
      });

      Assert.IsNotNull(found);
      Assert.AreEqual(0, found.Count());
    }

    [Test]
    public void TestFindByMediaType()
    {
      var structure = BuildMultiPart();
      var found = structure.Find(new MimeType("MESSAGE/RFC822"));

      Assert.IsNotNull(found);
      Assert.AreEqual("3", found.Section);

      found = structure.Find(MimeType.ApplicationOctetStream);

      Assert.IsNull(found);
    }

    [Test]
    public void TestFindAllByMediaType()
    {
      var structure = BuildMultiPart();
      var found = structure.FindAll(MimeType.TextPlain);

      var expectedSections = new[] {"1", "2.1", "3.1"};
      var index = 0;

      Assert.IsNotNull(found);

      foreach (var s in found) {
        Assert.IsNotNull(s);
        Assert.AreEqual(expectedSections[index++], s.Section);
      }

      found = structure.FindAll(MimeType.ApplicationOctetStream);

      Assert.IsNotNull(found);
      Assert.AreEqual(0, found.Count());
    }

    [Test]
    public void TestGetRootStructure()
    {
      var structure = BuildMultiPart();

      Assert.AreSame(structure, structure.GetRootStructure());
      Assert.AreSame(structure, structure.FindSection("1").GetRootStructure());
      Assert.AreSame(structure, structure.FindSection("2").GetRootStructure());
      Assert.AreSame(structure, structure.FindSection("2.1").GetRootStructure());
      Assert.AreSame(structure, structure.FindSection("2.2").GetRootStructure());
      Assert.AreSame(structure, structure.FindSection("3").GetRootStructure());
      Assert.AreSame(structure, structure.FindSection("3.1").GetRootStructure());
    }
  }
}
