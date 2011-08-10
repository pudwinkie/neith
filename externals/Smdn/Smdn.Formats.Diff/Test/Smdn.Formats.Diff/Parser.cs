using System;
using NUnit.Framework;

namespace Smdn.Formats.Diff {
  [TestFixture]
  public class ParserTests {
    [Test]
    public void TestParseUnifiedSingleEntry()
    {
      var diff = DiffDocument.Load("example.unified-1.txt", DiffFormat.Unified);

      var expectedDifference =
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
@@ -9,3 +8,6 @@
 The two are the same,
 But after they are produced,
   they have different names.
+They both may be called deep and profound.
+Deeper and more profound,
+The door of all subtleties!
";

      Assert.AreEqual(expectedDifference, diff.Difference);
      Assert.AreEqual(DiffFormat.Unified, diff.Format);
      Assert.AreEqual(1, diff.Entries.Count);

      var diffEntry = diff.Entries[0];

      var expectedEntryDifference = 
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
@@ -9,3 +8,6 @@
 The two are the same,
 But after they are produced,
   they have different names.
+They both may be called deep and profound.
+Deeper and more profound,
+The door of all subtleties!
";

      Assert.AreEqual("lao\t2002-02-21 23:30:39.942229878 -0800", diffEntry.DifferenceFrom);
      Assert.AreEqual("tzu\t2002-02-21 23:30:50.442260588 -0800", diffEntry.DifferenceTo);
      Assert.AreEqual(expectedEntryDifference, diffEntry.Difference);
      Assert.IsNotNull(diffEntry.HeaderLines);
      Assert.AreEqual(0, diffEntry.HeaderLines.Length);

      Assert.AreEqual(2, diffEntry.Hunks.Count);

      var diffHunk = diffEntry.Hunks[0];

      var expectedHunkDifference1 = @"@@ -1,7 +1,6 @@
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

      Assert.AreEqual(expectedHunkDifference1, diffHunk.Difference);
      Assert.AreEqual(1, diffHunk.FromRange.Start);
      Assert.AreEqual(7, diffHunk.FromRange.Count);
      Assert.AreEqual(1, diffHunk.ToRange.Start);
      Assert.AreEqual(6, diffHunk.ToRange.Count);

      diffHunk = diffEntry.Hunks[1];

      var expectedHunkDifference2 = @"@@ -9,3 +8,6 @@
 The two are the same,
 But after they are produced,
   they have different names.
+They both may be called deep and profound.
+Deeper and more profound,
+The door of all subtleties!
";

      Assert.AreEqual(expectedHunkDifference2, diffHunk.Difference);
      Assert.AreEqual(9, diffHunk.FromRange.Start);
      Assert.AreEqual(3, diffHunk.FromRange.Count);
      Assert.AreEqual(8, diffHunk.ToRange.Start);
      Assert.AreEqual(6, diffHunk.ToRange.Count);
    }

    [Test]
    public void TestParseUnifiedMultipleEntry()
    {
      var diff = DiffDocument.Load("example.unified-2.txt", DiffFormat.Unified);

      var expectedDifference =
@"Index: Test/Smdn.Formats.Diff.Tests.csproj
===================================================================
" + "--- Test/Smdn.Formats.Diff.Tests.csproj\t(リビジョン 3014)\n" +
"+++ Test/Smdn.Formats.Diff.Tests.csproj\t(作業コピー)\n" +
@"@@ -58,4 +58,15 @@
       <Name>Smdn.Formats.Diff</Name>
     </ProjectReference>
   </ItemGroup>
+  <ItemGroup>
+    <None Include=""example.unified-1.txt"">
+      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
+    </None>
+  </ItemGroup>
+  <ItemGroup>
+    <Folder Include=""Smdn.Formats.Diff\"" />
+  </ItemGroup>
+  <ItemGroup>
+    <Compile Include=""Smdn.Formats.Diff\Parser.cs"" />
+  </ItemGroup>
 </Project>

Index: Smdn.Formats.Diff.csproj
===================================================================
" + "--- Smdn.Formats.Diff.csproj\t(リビジョン 3014)\n" +
"+++ Smdn.Formats.Diff.csproj\t(作業コピー)\n" +
@"@@ -42,6 +42,13 @@
   -->
   <ItemGroup>
     <Compile Include=""AssemblyInfo.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffDocument.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffEntry.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffFormat.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffHunk.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\Parser.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\IDiffEntity.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffHunkRange.cs"" />
   </ItemGroup>
   <ItemGroup>
     <Reference Include=""System"" />
@@ -62,4 +69,7 @@
       <Name>Smdn</Name>
     </ProjectReference>
   </ItemGroup>
+  <ItemGroup>
+    <Folder Include=""Smdn.Formats.Diff\"" />
+  </ItemGroup>
 </Project>
";

      Assert.AreEqual(expectedDifference, diff.Difference);
      Assert.AreEqual(DiffFormat.Unified, diff.Format);
      Assert.AreEqual(2, diff.Entries.Count);

      var entry1 = diff.Entries[0];

      Assert.AreEqual("Test/Smdn.Formats.Diff.Tests.csproj\t(リビジョン 3014)",
                      entry1.DifferenceFrom);
      Assert.AreEqual("Test/Smdn.Formats.Diff.Tests.csproj\t(作業コピー)",
                      entry1.DifferenceTo);
      Assert.IsNotNull(entry1.HeaderLines);
      Assert.AreEqual(2, entry1.HeaderLines.Length);
      Assert.AreEqual("Index: Test/Smdn.Formats.Diff.Tests.csproj",
                      entry1.HeaderLines[0]);
      Assert.AreEqual("===================================================================",
                      entry1.HeaderLines[1]);
      Assert.AreEqual(1, entry1.Hunks.Count);

      var expectedEntry1Hunk1Difference =
@"@@ -58,4 +58,15 @@
       <Name>Smdn.Formats.Diff</Name>
     </ProjectReference>
   </ItemGroup>
+  <ItemGroup>
+    <None Include=""example.unified-1.txt"">
+      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
+    </None>
+  </ItemGroup>
+  <ItemGroup>
+    <Folder Include=""Smdn.Formats.Diff\"" />
+  </ItemGroup>
+  <ItemGroup>
+    <Compile Include=""Smdn.Formats.Diff\Parser.cs"" />
+  </ItemGroup>
 </Project>
";

      Assert.AreEqual(expectedEntry1Hunk1Difference, entry1.Hunks[0].Difference);
      Assert.AreEqual(58, entry1.Hunks[0].FromRange.Start);
      Assert.AreEqual(4,  entry1.Hunks[0].FromRange.Count);
      Assert.AreEqual(58, entry1.Hunks[0].ToRange.Start);
      Assert.AreEqual(15, entry1.Hunks[0].ToRange.Count);

      var entry2 = diff.Entries[1];

      Assert.AreEqual("Smdn.Formats.Diff.csproj\t(リビジョン 3014)",
                      entry2.DifferenceFrom);
      Assert.AreEqual("Smdn.Formats.Diff.csproj\t(作業コピー)",
                      entry2.DifferenceTo);
      Assert.AreEqual(3, entry2.HeaderLines.Length);
      Assert.AreEqual("",
                      entry2.HeaderLines[0]);
      Assert.AreEqual("Index: Smdn.Formats.Diff.csproj",
                      entry2.HeaderLines[1]);
      Assert.AreEqual("===================================================================",
                      entry2.HeaderLines[2]);
      Assert.AreEqual(2, entry2.Hunks.Count);

      var expectedEntry2Hunk1Difference =
@"@@ -42,6 +42,13 @@
   -->
   <ItemGroup>
     <Compile Include=""AssemblyInfo.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffDocument.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffEntry.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffFormat.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffHunk.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\Parser.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\IDiffEntity.cs"" />
+    <Compile Include=""Smdn.Formats.Diff\DiffHunkRange.cs"" />
   </ItemGroup>
   <ItemGroup>
     <Reference Include=""System"" />
";

      Assert.AreEqual(expectedEntry2Hunk1Difference, entry2.Hunks[0].Difference);
      Assert.AreEqual(42, entry2.Hunks[0].FromRange.Start);
      Assert.AreEqual(6,  entry2.Hunks[0].FromRange.Count);
      Assert.AreEqual(42, entry2.Hunks[0].ToRange.Start);
      Assert.AreEqual(13, entry2.Hunks[0].ToRange.Count);

      var expectedEntry2Hunk2Difference =
@"@@ -62,4 +69,7 @@
       <Name>Smdn</Name>
     </ProjectReference>
   </ItemGroup>
+  <ItemGroup>
+    <Folder Include=""Smdn.Formats.Diff\"" />
+  </ItemGroup>
 </Project>
";
      Assert.AreEqual(expectedEntry2Hunk2Difference, entry2.Hunks[1].Difference);
      Assert.AreEqual(62, entry2.Hunks[1].FromRange.Start);
      Assert.AreEqual(4,  entry2.Hunks[1].FromRange.Count);
      Assert.AreEqual(69, entry2.Hunks[1].ToRange.Start);
      Assert.AreEqual(7,  entry2.Hunks[1].ToRange.Count);
    }
  }
}
