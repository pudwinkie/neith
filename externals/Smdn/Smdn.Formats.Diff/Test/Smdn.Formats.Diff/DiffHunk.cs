using System;
using System.IO;
using NUnit.Framework;

namespace Smdn.Formats.Diff {
  [TestFixture]
  public class DiffHunkTests {
    [Test]
    public void TestGetTexts()
    {
      var diffText =
"--- lao\t2002-02-21 23:30:39.942229878 -0800\n" +
"+++ tzu\t2002-02-21 23:30:50.442260588 -0800\n" +
@"@@ -1,7 +1,6 @@
-The Way that can be told of is not the eternal Way;
-The name that can be named is not the eternal name.
 The Nameless is the origin of Heaven and Earth;
-The Named is the mother of all things.
+The named is the mother of all things.
+
 Therefore let there always be non-being,
   so we may see their subtlety,
 And let there always be being,
";
      var diff = DiffDocument.Load(new StringReader(diffText), DiffFormat.Unified);
      var hunk = diff.Entries[0].Hunks[0];

      DiffLine[] originalLines, modifiedLines;

      var lineCount = hunk.GetLines(out originalLines, out modifiedLines);

      Assert.IsNotNull(originalLines);
      Assert.IsNotNull(modifiedLines);
      Assert.AreEqual(8, lineCount, "line counts");
      Assert.AreEqual(lineCount, originalLines.Length, "line count equality (original)");
      Assert.AreEqual(lineCount, modifiedLines.Length, "line count equality (modified)");

      var expected = new[] {
        new {OLineNum = 1, MLineNum = 0, OStatus = DiffLineStatus.Removed,     MStatus = DiffLineStatus.NonExistent, Text = new[] {"The Way that can be told of is not the eternal Way;", null}},
        new {OLineNum = 2, MLineNum = 0, OStatus = DiffLineStatus.Removed,     MStatus = DiffLineStatus.NonExistent, Text = new[] {"The name that can be named is not the eternal name.", null}},
        new {OLineNum = 3, MLineNum = 1, OStatus = DiffLineStatus.NotChanged,  MStatus = DiffLineStatus.NotChanged,  Text = new[] {"The Nameless is the origin of Heaven and Earth;"}},
        new {OLineNum = 4, MLineNum = 2, OStatus = DiffLineStatus.Modified,    MStatus = DiffLineStatus.Modified,    Text = new[] {"The Named is the mother of all things.", "The named is the mother of all things."}},
        new {OLineNum = 0, MLineNum = 3, OStatus = DiffLineStatus.NonExistent, MStatus = DiffLineStatus.Added,       Text = new[] {null, ""}},
        new {OLineNum = 5, MLineNum = 4, OStatus = DiffLineStatus.NotChanged,  MStatus = DiffLineStatus.NotChanged,  Text = new[] {"Therefore let there always be non-being,"}},
        new {OLineNum = 6, MLineNum = 5, OStatus = DiffLineStatus.NotChanged,  MStatus = DiffLineStatus.NotChanged,  Text = new[] {"  so we may see their subtlety,"}},
        new {OLineNum = 7, MLineNum = 6, OStatus = DiffLineStatus.NotChanged,  MStatus = DiffLineStatus.NotChanged,  Text = new[] {"And let there always be being,"}},
      };

      for (var i = 0; i < expected.Length; i++) {
        Assert.AreEqual(expected[i].OStatus, originalLines[i].Status, "original line #{0} status", i);
        Assert.AreEqual(expected[i].MStatus, modifiedLines[i].Status, "modified line #{0} status", i);

        Assert.AreEqual(expected[i].OLineNum, originalLines[i].LineNumber, "original line #{0} number", i);
        Assert.AreEqual(expected[i].MLineNum, modifiedLines[i].LineNumber, "modified line #{0} number", i);

        if (expected[i].OStatus == DiffLineStatus.NotChanged) {
          Assert.AreEqual(expected[i].Text[0], originalLines[i].Text, "original line #{0} content", i);
          Assert.AreEqual(expected[i].Text[0], modifiedLines[i].Text, "modified line #{0} content", i);
        }
        else {
          Assert.AreEqual(expected[i].Text[0], originalLines[i].Text, "original line #{0} content", i);
          Assert.AreEqual(expected[i].Text[1], modifiedLines[i].Text, "modified line #{0} content", i);
        }
      }
    }
  }
}
