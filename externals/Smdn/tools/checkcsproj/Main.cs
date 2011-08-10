using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace checkcsproj
{
  class MainClass
  {
    private static readonly string defaultPropertyGroupXPath = "/x:Project/x:PropertyGroup[not(@Condition)]/";

    public static void Main(string[] args)
    {
#if DEBUG
      args = new[] {"/srv/files/projects/netfx4/trunk/Smdn/Smdn-netfx2.0.csproj"};
#endif

      string baseCsprojPath = null;
      var combineProjects = new List<string>();

      for (var i = 0; i < args.Length; i++) {
        switch (args[i]) {
          case "--base":
            baseCsprojPath = args[++i];
            break;
          case "--combine":
            combineProjects.Add(args[++i]);
            break;
        }
      }

      var baseCsproj = Project.Load(baseCsprojPath);
      var libRootDir = Directory.GetParent(Path.GetDirectoryName(baseCsprojPath));
      var csprojFiles = new List<FileInfo>();
      var libCsprojMap = new Dictionary<string, List<FileInfo>>();

      foreach (var libDir in libRootDir.GetDirectories("Smdn*")) {
        foreach (var file in libDir.GetFiles("*.csproj", SearchOption.TopDirectoryOnly)) {
          csprojFiles.Add(file);

          if (!libCsprojMap.ContainsKey(libDir.Name))
            libCsprojMap[libDir.Name] = new List<FileInfo>();

          libCsprojMap[libDir.Name].Add(file);
        }
      }

      // create or update combined first
      foreach (var pair in libCsprojMap) {
        if (combineProjects.Contains(pair.Key)) {
          foreach (var file in pair.Value) {
            CombinedProject.CreateOrUpdate(file);
          }
        }
      }

      csprojFiles.Sort(delegate(FileInfo x, FileInfo y) {
        return string.Compare(x.Name, y.Name);
      });

      foreach (var csprojFile in csprojFiles) {
        Console.WriteLine("checking {0} ({1})", csprojFile.Name, csprojFile.DirectoryName);

        Check(baseCsproj, csprojFile);
      }

      foreach (var pair in libCsprojMap) {
        Project baseLibProj = null;
        var targettedLibProjs = new List<Project>();

        foreach (var csproj in pair.Value) {
          if (csproj.Name.Contains("-combined"))
            continue; // ignore

          if (csproj.Name.Contains("-netfx2.0") ||
              csproj.Name.Contains("-netfx3.5") ||
              csproj.Name.Contains("-netfx4.0")) {
            var proj = Project.Load(csproj.FullName);

            if (baseLibProj == null) {
              baseLibProj = proj;
            }
            else {
#if false
              var baseVersion = baseLibProj.SelectSingleNode(defaultPropertyGroupXPath + "x:TargetFrameworkVersion/text()").Value;
              var version = proj.SelectSingleNode(defaultPropertyGroupXPath + "x:TargetFrameworkVersion/text()").Value;

              if (0 < string.Compare(baseVersion, version))
#endif
              if (File.GetLastWriteTime(baseLibProj.File) < File.GetLastWriteTime(proj.File)) {
                targettedLibProjs.Add(baseLibProj);
                baseLibProj = proj;
              }
              else {
                targettedLibProjs.Add(proj);
              }
            }
          }
        }

        if (baseLibProj == null)
          continue;

        if (0 < targettedLibProjs.Count)
          Check(baseLibProj, targettedLibProjs);

        /*
         * test projects
         */
        var testDir = new DirectoryInfo(Path.Combine(baseLibProj.GetDirectory(), "Test"));

        if (!testDir.Exists)
          continue;

        Project baseTestLibProj = null;
        var targettedTestLibProjs = new List<Project>();

        foreach (var testCsproj in testDir.GetFiles("*.csproj")) {
          if (testCsproj.Name.Contains("-combined"))
            continue; // ignore

          if (testCsproj.Name.Contains("-netfx2.0") ||
              testCsproj.Name.Contains("-netfx3.5") ||
              testCsproj.Name.Contains("-netfx4.0")) {
            var proj = Project.Load(testCsproj.FullName);

            if (baseTestLibProj == null) {
              baseTestLibProj = proj;
            }
            else {
#if false
              var baseVersion = baseLibProj.SelectSingleNode(defaultPropertyGroupXPath + "x:TargetFrameworkVersion/text()").Value;
              var version = proj.SelectSingleNode(defaultPropertyGroupXPath + "x:TargetFrameworkVersion/text()").Value;

              if (0 < string.Compare(baseVersion, version))
#endif
              if (File.GetLastWriteTime(baseTestLibProj.File) < File.GetLastWriteTime(proj.File)) {
                targettedTestLibProjs.Add(baseTestLibProj);
                baseTestLibProj = proj;
              }
              else {
                targettedTestLibProjs.Add(proj);
              }
            }
          }
        }

        if (baseTestLibProj != null && 0 < targettedTestLibProjs.Count)
          Check(baseTestLibProj, targettedTestLibProjs);
      }
    }

    private static void Check(Project baseCsproj, FileInfo csprojFile)
    {
      var csproj = Project.Load(csprojFile.FullName);

      string projPostfix = null;
      string expectedTargetFramework = null;
      string[] expectedDefineConstants = new string[0];

      if (csprojFile.Name.Contains("-netfx2.0")) {
        projPostfix = "netfx2.0";
        expectedTargetFramework = "v2.0";
        expectedDefineConstants = new[] {"NET_2_0"};
      }
      else if (csprojFile.Name.Contains("-netfx3.5")) {
        projPostfix = "netfx3.5";
        expectedTargetFramework = "v3.5";
        expectedDefineConstants = new[] {"NET_2_0", "NET_3_5"};
      }
      else if (csprojFile.Name.Contains("-netfx4.0")) {
        projPostfix = "netfx4.0";
        expectedTargetFramework = "v4.0";
        expectedDefineConstants = new[] {"NET_2_0", "NET_3_5", "NET_4_0"};
      }

      if (csprojFile.Name.Contains("-combined")) {
        projPostfix = projPostfix + "-combined";
      }

      if (projPostfix == null)
        Console.Error.WriteLine("  invalid file name: {0}", csprojFile.Name);

      var assemblyName = csproj.SelectSingleNode(defaultPropertyGroupXPath + "x:AssemblyName/text()").Value;

      AreNodeValueEqual(csproj, defaultPropertyGroupXPath + "x:AssemblyName/text()",  csprojFile.Directory.Name);
      AreNodeValueEqual(csproj, defaultPropertyGroupXPath + "x:OutputType/text()", "Library");
      AreNodeValueEqual(baseCsproj, csproj, defaultPropertyGroupXPath + "x:OutputPath/text()");

      var expectedRootNamespace = assemblyName.StartsWith("Smdn.Core.")
        ? "Smdn"
        : csprojFile.Directory.Name;

      AreNodeValueEqual(csproj, defaultPropertyGroupXPath + "x:RootNamespace/text()",  expectedRootNamespace);

      if (expectedTargetFramework != null)
        AreNodeValueEqual(csproj, defaultPropertyGroupXPath + "x:TargetFrameworkVersion/text()", expectedTargetFramework);

      foreach (var condition in new[] {"Release", "Debug"}) {
        var conditionalPropertyGroup = string.Format("/x:Project/x:PropertyGroup[contains(@Condition, \"{0}|\")]/", condition);

        AreNodeValueEqual(csproj, conditionalPropertyGroup + "x:RootNamespace/text()", csprojFile.Directory.Name);
        AreNodeValueEqual(baseCsproj, csproj, conditionalPropertyGroup + "x:CheckForOverflowUnderflow/text()");
        AreNodeValueEqual(baseCsproj, csproj, conditionalPropertyGroup + "x:DebugType/text()");
        AreNodeValueEqual(baseCsproj, csproj, conditionalPropertyGroup + "x:Optimize/text()");
        AreNodeValueEqual(baseCsproj, csproj, conditionalPropertyGroup + "x:WarningLevel/text()");

        if (projPostfix == null) {
          // TODO
        }
        else {
          var expectedOutputPath = string.Format(@"..\build\bin\{0}\{1}\", condition, projPostfix);

          AreNodeValueEqual(csproj, conditionalPropertyGroup + "x:OutputPath/text()", expectedOutputPath);
        }

        if (expectedTargetFramework == null) {
          // TODO
        }
        else {
          var expectedDefineConstantList = new List<string>(expectedDefineConstants);

          if (condition == "Debug")
            expectedDefineConstantList.Add("DEBUG");

          if (assemblyName.StartsWith("Smdn.Net"))
            expectedDefineConstantList.Add("TRACE");

          var defineConstantsXpath = conditionalPropertyGroup + "x:DefineConstants/text()";
          var actualDefineConstantList = new List<string>(csproj.SelectSingleNode(defineConstantsXpath).Value.Split(';'));

          expectedDefineConstantList.Sort();
          actualDefineConstantList.Sort();

          var sortedExpectedDefineConstant = string.Join(";", expectedDefineConstantList.ToArray());
          var sortedActualDefineConstant = string.Join(";", actualDefineConstantList.ToArray());

          if (!string.Equals(sortedExpectedDefineConstant, sortedActualDefineConstant))
            PrintDifference(sortedExpectedDefineConstant, sortedActualDefineConstant, defineConstantsXpath);
        }
      }

      // check configurations
      foreach (XmlNode node in csproj.SelectNodes("/x:Project/x:PropertyGroup[@Condition]")) {
        if (!(node.Attributes["Condition"].Value.Contains("'Release|") || node.Attributes["Condition"].Value.Contains("'Debug|")))
          Console.WriteLine("  unknown condition: {0}", node.Attributes["Condition"].Value);
      }

      // check other property
      foreach (XmlNode node in csproj.SelectNodes(defaultPropertyGroupXPath + "*")) {
        var nodePath = defaultPropertyGroupXPath + "x:" + node.Name;
        if (null == baseCsproj.SelectSingleNode(nodePath))
          Console.WriteLine("  extra property: {0,-20} ({1})", node.InnerText, nodePath);
      }

      foreach (var condition in new[] {"Release", "Debug"}) {
        var conditionalPropertyGroup = string.Format("/x:Project/x:PropertyGroup[contains(@Condition, \"{0}|\")]/", condition);
        foreach (XmlNode node in csproj.SelectNodes(conditionalPropertyGroup + "*")) {
          var nodePath = conditionalPropertyGroup + "x:" + node.Name;
          if (null == baseCsproj.SelectSingleNode(nodePath))
            Console.WriteLine("  extra property: {0,-20} ({1})", node.InnerText, nodePath);
        }
      }
    }

    private static void AreNodeValueEqual(Project project, string xpath, string expected)
    {
      var node = project.SelectSingleNode(xpath);
      var val = (node == null) ? null : node.Value;

      if (!string.Equals(expected, val))
        PrintDifference(expected, val, xpath);
    }

    private static void AreNodeValueEqual(Project x, Project y, string xpath)
    {
      var node = x.SelectSingleNode(xpath);

      if (node == null)
        AreNodeValueEqual(y, xpath, null);
      else
        AreNodeValueEqual(y, xpath, node.Value);
    }

    private static void PrintDifference(string expected, string actual, string xpath)
    {
      Console.WriteLine("  different: {0,-20} {1,-20} ({2})", expected ?? "(null)", actual ?? "(null)", xpath);
    }

    private static void Check(Project baseProject, IEnumerable<Project> targettedProjects)
    {
      foreach (var destProject in targettedProjects) {
        var targetFrameworkVersion = destProject.SelectSingleNode(defaultPropertyGroupXPath + "x:TargetFrameworkVersion/text()").Value;
        var merged = false;

        // check project guid
        var destProjectGuidNode = destProject.SelectSingleNode(defaultPropertyGroupXPath + "x:ProjectGuid") as XmlElement;

        var baseProjectGuid = baseProject.SelectSingleNode(defaultPropertyGroupXPath + "x:ProjectGuid/text()").Value;

        if (baseProjectGuid == destProjectGuidNode.InnerText) {
          destProjectGuidNode.InnerText = Guid.NewGuid().ToString("B").ToUpper();

          merged = true;
        }

        // check ItemGroup[Compile]
        var baseCompileItemGroupNode = baseProject.SelectSingleNode("/x:Project/x:ItemGroup[x:Compile]");
        var destCompileItemGroupNode = destProject.SelectSingleNode("/x:Project/x:ItemGroup[x:Compile]");

        if (baseCompileItemGroupNode.InnerXml != destCompileItemGroupNode.InnerXml) {
          destCompileItemGroupNode.RemoveAll();

          foreach (XmlElement baseCompile in baseCompileItemGroupNode.SelectNodes("x:Compile", baseProject.Nsmgr)) {
            var destCompile = destProject.Document.CreateElement("Compile", Project.MsbuildNs);

            destCompile.SetAttribute("Include", baseCompile.GetAttribute("Include"));

            destCompileItemGroupNode.AppendChild(destCompile);
          }

          merged = true;
        }

        // check ItemGroup[Reference]
        var baseReferenceItemGroupNode = baseProject.SelectSingleNode("/x:Project/x:ItemGroup[x:Reference]");
        var destReferenceItemGroupNode = destProject.SelectSingleNode("/x:Project/x:ItemGroup[x:Reference]");

        var isReferenceItemGroupDifferent = false;

        foreach (XmlElement baseReference in baseReferenceItemGroupNode.SelectNodes("x:Reference", baseProject.Nsmgr)) {
          var path = string.Format("x:Reference[@Include='{0}']", baseReference.GetAttribute("Include"));

          if (destReferenceItemGroupNode.SelectSingleNode(path, destProject.Nsmgr) == null)
            isReferenceItemGroupDifferent = true;
        }

        if (isReferenceItemGroupDifferent) {
          var destReferenceItemGroupRemoved = false;

          foreach (XmlElement baseReference in baseReferenceItemGroupNode.SelectNodes("x:Reference", baseProject.Nsmgr)) {
            var reference = baseReference.GetAttribute("Include");

            if (reference == "System.Core" && targetFrameworkVersion == "v2.0")
              continue;

            if (!destReferenceItemGroupRemoved) {
              destReferenceItemGroupNode.RemoveAll();

              destReferenceItemGroupRemoved = true;
              merged = true;
            }

            var destReference = destProject.Document.CreateElement("Reference", Project.MsbuildNs);

            destReference.SetAttribute("Include", reference);

            destReferenceItemGroupNode.AppendChild(destReference);

            if (baseReference.GetAttribute("Include") == "System" && targetFrameworkVersion != "v2.0") {
              var destReferenceSystemCore = destProject.Document.CreateElement("Reference", Project.MsbuildNs);

              destReferenceSystemCore.SetAttribute("Include", "System.Core");

              destReferenceItemGroupNode.AppendChild(destReferenceSystemCore);
            }
          }
        }

        if (merged) {
          destProject.Save(destProject.File);

          Console.WriteLine("updated: {0}", destProject.File);
        }
      }
    }
  }
}