using System;
using NUnit.Framework;

namespace Smdn.Net.Imap4 {
  [TestFixture]
  public class IImapBodyStructureTests {
    [Test]
    public void TestParent()
    {
      var part1 = new ImapSinglePartBodyStructure("1", new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);

      var part21 = new ImapSinglePartBodyStructure("2.1", new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);
      var part21extended = new ImapExtendedSinglePartBodyStructure(part21, null, null, null, null, null);
      var part22 = new ImapSinglePartBodyStructure("2.2", new MimeType("text", "html"), null, null, null, "7bit", 0L, 0L);
      var part2 = new ImapMultiPartBodyStructure("2",
                                                 new[] {part21extended, part22},
                                                 "alternative");

      var part31 = new ImapSinglePartBodyStructure("3.1", new MimeType("text", "plain"), null, null, null, "7bit", 0L, 0L);
      var part3Envelope = new ImapEnvelope(null, null, null, null, null, null, null, null, null, null);
      var part3 = new ImapMessageRfc822BodyStructure("3", new MimeType("message", "rfc822"), null, null, null, "7bit", 0L, part3Envelope, part31, 0L);

      var root = new ImapMultiPartBodyStructure(string.Empty,
                                                new IImapBodyStructure[] {part1, part2, part3},
                                                "mixed");

      Assert.IsNull(root.ParentStructure);

      Assert.AreSame(root, part1.ParentStructure);
      Assert.AreSame(root, part2.ParentStructure);
      Assert.AreSame(root, part3.ParentStructure);

      Assert.AreSame(part2, part21extended.ParentStructure);
      Assert.AreSame(part2, part22.ParentStructure);

      Assert.AreSame(part3, part31.ParentStructure);
    }
  }
}
