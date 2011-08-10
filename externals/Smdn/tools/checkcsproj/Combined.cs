using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace checkcsproj {
  public static class CombinedProject {
    private static Regex requireFileRegex = new Regex(@"^(?<assm>[^/]+)/(?<path>.+)$", RegexOptions.Singleline);

    public static void CreateOrUpdate(FileInfo baseProjFile)
    {
      if (baseProjFile.Name.Contains("-combined"))
        return;

      var outputCsprojPath = string.Format("{0}-combined{1}",
                                           Path.GetFileNameWithoutExtension(baseProjFile.Name),
                                           baseProjFile.Extension);

      outputCsprojPath = Path.Combine(baseProjFile.DirectoryName, outputCsprojPath);

      string targetFramework = null;

      if (baseProjFile.Name.Contains("-netfx2.0")) {
        targetFramework = "netfx2.0";
      }
      else if (baseProjFile.Name.Contains("-netfx3.5")) {
        targetFramework = "netfx3.5";
      }
      else if (baseProjFile.Name.Contains("-netfx4.0")) {
        targetFramework = "netfx4.0";
      }

      var projFile = Project.Load(baseProjFile.FullName);

      /*
       * try get output csproj guid
       */
      var outputCsprojGuid = Guid.Empty;

      if (File.Exists(outputCsprojPath)) {
        var outputCsproj = Project.Load(outputCsprojPath);

        outputCsprojGuid = new Guid(outputCsproj.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:ProjectGuid/text()").Value);
      }

      /*
       * get ProjectReference
       */
      var referencingProjectPaths = new Dictionary<string, string>();

      foreach (XmlNode node in projFile.SelectNodes("/x:Project/x:ItemGroup/x:ProjectReference")) {
        var name = node.SelectSingleNode("x:Name/text()", projFile.Nsmgr).Value.Replace("-" + targetFramework, string.Empty);

        referencingProjectPaths.Add(name,
                                    node.SelectSingleNode("@Include", projFile.Nsmgr).Value);
      }

      /*
       * change Project/ItemGroup/Compile@Include
       */
      var compileItemGroupNode = projFile.SelectSingleNode("x:Project/x:ItemGroup[x:Compile]");
      var outputAbsPath = new Uri(Path.GetFullPath(outputCsprojPath));

      foreach (XmlElement node in compileItemGroupNode.SelectNodes("x:Compile", projFile.Nsmgr)) {
        var currentPath = node.GetAttribute("Include");
        var fileName = Path.GetFileName(currentPath);
        var currentAbsPath = Path.Combine(baseProjFile.DirectoryName, Path.GetDirectoryName(currentPath));

        node.SetAttribute("Include", Project.GetRelativePath(outputAbsPath, Path.Combine(currentAbsPath, fileName)).Replace("/", "\\"));
      }

      /*
       * change Project/PropertyGroup/OutputPath, Project/PropertyGroup/IntermediateOutputPath
       */
      foreach (XmlElement node in projFile.SelectNodes("x:Project/x:PropertyGroup/x:OutputPath")) {
        node.InnerText = node.InnerText.Replace(targetFramework, targetFramework + "-combined");
      }

      foreach (XmlElement node in projFile.SelectNodes("x:Project/x:PropertyGroup/x:IntermediateOutputPath")) {
        node.InnerText = node.InnerText.Replace(targetFramework, targetFramework + "-combined");
      }

      /*
       * include requiring files
       */
      IncludeRequiringFiles(projFile, projFile, targetFramework, outputAbsPath, referencingProjectPaths);

      /*
       * include embedded resource files
       */
      IncludeEmbeddedResourceFiles(projFile, targetFramework, outputAbsPath, referencingProjectPaths);

      /*
       * sort Project/ItemGroup/Compile by @Include
       */
      var compileTargets = new List<KeyValuePair<string, XmlElement>>();

      foreach (XmlElement node in compileItemGroupNode.SelectNodes("x:Compile", projFile.Nsmgr)) {
        compileTargets.Add(new KeyValuePair<string, XmlElement>(node.GetAttribute("Include"), node));
      }

      foreach (var pair in compileTargets) {
        compileItemGroupNode.RemoveChild(pair.Value);
      }

      compileTargets.Sort(delegate(KeyValuePair<string, XmlElement> x, KeyValuePair<string, XmlElement> y) {
        return string.Compare(x.Key, y.Key);
      });

      foreach (var pair in compileTargets) {
        compileItemGroupNode.AppendChild(pair.Value);
      }

      /*
       * update project guid
       */
      var projectGuidNode = projFile.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:ProjectGuid");
      var originalCsprojGuid = new Guid(projectGuidNode.InnerText);

      if (Guid.Empty.Equals(outputCsprojGuid) || originalCsprojGuid.Equals(outputCsprojGuid))
        outputCsprojGuid = Guid.NewGuid();

      projectGuidNode.InnerText = outputCsprojGuid.ToString("B").ToUpper();

      /*
       * remove unusing nodes
       */
      projFile.RemoveNodes("/x:Project/x:ItemGroup[x:ProjectReference]");
      projFile.RemoveNodes("/x:Project/x:ItemGroup[x:Folder]");
      projFile.RemoveNodes("/x:Project/x:ItemGroup[x:None]");

      /*
       * save
       */
      projFile.Save(outputCsprojPath);

      Console.WriteLine("combined proj created {0}", outputCsprojPath);

      /*
       * test proj
       */
      projFile.File = outputCsprojPath;
      projFile.SetBaseDirectory(outputCsprojPath);

      UpdateOrCreateTestProject(baseProjFile.FullName, targetFramework, projFile);
    }

    private static void IncludeRequiringFiles(Project target, Project including,
                                              string targetFramework,
                                              Uri outputAbsPath,
                                              Dictionary<string, string> referencingProjectPaths)
    {
      var requiresFileNode = including.SelectSingleNode("x:Project/x:ItemGroup/x:None[@Include='requires']");

      if (requiresFileNode == null)
        return;

      var compileItemGroupNode = target.SelectSingleNode("x:Project/x:ItemGroup[x:Compile]");

      if (target != including) {
        foreach (XmlNode compileItemNode in including.SelectNodes("x:Project/x:ItemGroup/x:Compile")) {
          var includeFile = compileItemNode.SelectSingleNode("@Include").Value;

          if (includeFile == "AssemblyInfo.cs")
            continue; // ignore

          var includeFilePath = Path.Combine(including.GetDirectory(),
                                             includeFile.Replace("\\", Path.DirectorySeparatorChar.ToString()));

          var node = compileItemGroupNode.AppendChild(target.Document.CreateElement("Compile", Project.MsbuildNs)) as XmlElement;

          node.SetAttribute("Include", Project.GetRelativePath(outputAbsPath, includeFilePath).Replace("/", "\\"));
        }

        /*
         * check AllowUnsafeBlocks
         */
        var allowUnsafeBlocksNode = including.SelectSingleNode("x:Project/x:PropertyGroup[@Condition]/x:AllowUnsafeBlocks");

        if (allowUnsafeBlocksNode != null) {
          var allowUnsafeBlocks = bool.Parse(allowUnsafeBlocksNode.SelectSingleNode("text()").Value);

          foreach (XmlNode propertyGroupNode in target.SelectNodes("x:Project/x:PropertyGroup[@Condition]")) {
            var node = propertyGroupNode.AppendChild(target.Document.CreateElement("AllowUnsafeBlocks", Project.MsbuildNs));

            node.AppendChild(target.Document.CreateTextNode(XmlConvert.ToString(allowUnsafeBlocks)));
          }
        }
      }

      foreach (var line in File.ReadAllLines(Path.Combine(including.GetDirectory(), "requires"))) {
        var match = requireFileRegex.Match(line);

        if (!match.Success)
          continue;

        string referenceProjectPath;

        if (!referencingProjectPaths.TryGetValue(match.Groups["assm"].Value, out referenceProjectPath))
          continue;

        var referenceProjectFullPath = Path.Combine(including.GetDirectory(),
                                                    referenceProjectPath.Replace("\\", Path.DirectorySeparatorChar.ToString()));

        var includeFilePath = Path.GetDirectoryName(Path.GetFullPath(referenceProjectFullPath));

        includeFilePath = Path.Combine(includeFilePath, match.Groups["path"].Value);

        var includeFileExtension = Path.GetExtension(includeFilePath);

        if (includeFileExtension == ".cs") {
          if (!File.Exists(includeFilePath))
            throw new FileNotFoundException("file not found", includeFilePath);

          var includeFileAbsPathString = Project.GetRelativePath(outputAbsPath, includeFilePath).Replace("/", "\\");

          if (compileItemGroupNode.SelectSingleNode(string.Format("x:Compile[@Include='{0}']", includeFileAbsPathString), target.Nsmgr) == null) {
            var node = compileItemGroupNode.AppendChild(target.Document.CreateElement("Compile", Project.MsbuildNs)) as XmlElement;

            node.SetAttribute("Include", includeFileAbsPathString);
          }
        }
        else if (includeFileExtension.EndsWith(".csproj")) {
          var includeCsprojFile = Path.Combine(including.GetDirectory(), includeFilePath);

          if (targetFramework != null)
            includeCsprojFile = includeCsprojFile.Replace(".csproj",
                                                          string.Format("-{0}.csproj", targetFramework));

          if (!File.Exists(includeCsprojFile))
            throw new FileNotFoundException("file not found", includeCsprojFile);

          IncludeRequiringFiles(target,
                                Project.Load(includeCsprojFile),
                                targetFramework,
                                outputAbsPath,
                                referencingProjectPaths);
        }
        else {
          Console.Error.WriteLine("not included: {0}", includeFilePath);
        }
      }
    }

    private static void IncludeEmbeddedResourceFiles(Project target,
                                                     string targetFramework,
                                                     Uri outputAbsPath,
                                                     Dictionary<string, string> referencingProjectPaths)
    {
      var embeddedResourceItemGroupNode = target.SelectSingleNode("x:Project/x:ItemGroup[x:EmbeddedResource]") as XmlElement;

      if (embeddedResourceItemGroupNode == null) {
        embeddedResourceItemGroupNode = target.Document.CreateElement("ItemGroup", Project.MsbuildNs);

        var refItemGroup = target.SelectSingleNode("x:Project/x:ItemGroup[x:Compile]");

        if (refItemGroup == null)
          target.Document.DocumentElement.AppendChild(embeddedResourceItemGroupNode);
        else
          refItemGroup.ParentNode.InsertAfter(embeddedResourceItemGroupNode, refItemGroup);
      }

      foreach (var referenceProjectPath in referencingProjectPaths.Values) {
        var referenceProjectFullPath = Path.Combine(target.GetDirectory(),
                                                    referenceProjectPath.Replace("\\", Path.DirectorySeparatorChar.ToString()));

        var referenceProject = Project.Load(referenceProjectFullPath);

        foreach (XmlNode node in referenceProject.SelectNodes("x:Project/x:ItemGroup/x:EmbeddedResource")) {
          var embeddedResourceItem = target.Document.ImportNode(node, true) as XmlElement;

          // check LogicalName
          var logicalNameNode = embeddedResourceItem.SelectSingleNode("x:LogicalName", target.Nsmgr);

          if (logicalNameNode == null) {
            var logicalName = string.Format("{0}.{1}",
                                            referenceProject.SelectSingleNode("x:Project/x:PropertyGroup/x:AssemblyName/text()").Value,
                                            embeddedResourceItem.GetAttribute("Include").Replace("\\", "."));

            logicalNameNode = target.Document.CreateElement("LogicalName", Project.MsbuildNs);
            logicalNameNode.AppendChild(target.Document.CreateTextNode(logicalName));

            embeddedResourceItem.AppendChild(logicalNameNode);
          }

          // change @Include
          var include = embeddedResourceItem.GetAttribute("Include");

          include = Project.GetRelativePath(outputAbsPath,
                                            Path.Combine(referenceProject.GetDirectory(),
                                                         include));

          embeddedResourceItem.SetAttribute("Include", include.Replace("/", "\\"));

          embeddedResourceItemGroupNode.AppendChild(embeddedResourceItem);
        }
      }
    }

    private static void UpdateOrCreateTestProject(string originalCsprojPath, string targetFramework, Project baseProject)
    {
      var originalTestProjectDirectory = Path.Combine(Path.GetDirectoryName(originalCsprojPath), "Test");
      var originalTestProjectFileName = Path.GetFileName(originalCsprojPath).Replace("-" + targetFramework, ".Tests-" + targetFramework);
      var originalTestProjectPath = Path.Combine(originalTestProjectDirectory, originalTestProjectFileName);

      if (!File.Exists(originalTestProjectPath))
        throw new FileNotFoundException("original test project not found", originalTestProjectPath);

      var testProject = Project.Load(originalTestProjectPath);
      var outputFileName = originalTestProjectPath.Replace(targetFramework, targetFramework + "-combined");

      testProject.File = Path.Combine(baseProject.GetDirectory(), outputFileName);
      testProject.SetBaseDirectory(Path.GetFullPath(testProject.File));

      var importNode = testProject.SelectSingleNode("/x:Project/x:Import");

      /*
       * try get output csproj guid
       */
      var outputCsprojGuid = Guid.Empty;

      if (File.Exists(testProject.File)) {
        var outputCsproj = Project.Load(testProject.File);

        outputCsprojGuid = new Guid(outputCsproj.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:ProjectGuid/text()").Value);
      }

      /*
       * update reference
       */
      testProject.RemoveNodes("/x:Project/x:ItemGroup[x:Reference]");

      var referenceItemGroup = testProject.Document.CreateElement("ItemGroup", Project.MsbuildNs);

      foreach (XmlElement element in baseProject.SelectNodes("/x:Project/x:ItemGroup/x:Reference")) {
        var reference = testProject.Document.CreateElement("Reference", Project.MsbuildNs);

        reference.SetAttribute("Include", element.GetAttribute("Include"));

        referenceItemGroup.AppendChild(reference);
      }

      var nunitReference = testProject.Document.CreateElement("Reference", Project.MsbuildNs);

      nunitReference.SetAttribute("Include", "nunit.framework, Version=2.4.8.0, Culture=neutral, PublicKeyToken=96d09a1eb7f44a77");

      referenceItemGroup.AppendChild(nunitReference);

      if (0 < referenceItemGroup.ChildNodes.Count)
        importNode.ParentNode.InsertBefore(referenceItemGroup, importNode);

      /*
       * update reference projects
       */
      testProject.RemoveNodes("/x:Project/x:ItemGroup[x:ProjectReference]");

      var projectReferenceItemGroup = testProject.Document.CreateElement("ItemGroup", Project.MsbuildNs);

      var projectReferenceBaseProject = testProject.Document.CreateElement("ProjectReference", Project.MsbuildNs);

      projectReferenceBaseProject.SetAttribute("Include", testProject.GetRelativePath(Path.GetFullPath(baseProject.File)).Replace("/", "\\"));
      projectReferenceBaseProject.AppendChild(testProject.Document.CreateElement("Project", Project.MsbuildNs)).InnerText =
        baseProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:ProjectGuid/text()").Value;
      projectReferenceBaseProject.AppendChild(testProject.Document.CreateElement("Name", Project.MsbuildNs)).InnerText =
        Path.GetFileNameWithoutExtension(baseProject.File);
        //Path.GetFileNameWithoutExtension(originalCsprojPath);

      projectReferenceItemGroup.AppendChild(projectReferenceBaseProject);

      foreach (XmlElement element in baseProject.SelectNodes("/x:Project/x:ItemGroup/x:ProjectReference")) {
        var projectReference = testProject.Document.CreateElement("ProjectReference", Project.MsbuildNs);
        var includeProject = Project.Load(Path.Combine(baseProject.GetDirectory(), element.GetAttribute("@Include")));

        projectReference.SetAttribute("Include", testProject.GetRelativePath(includeProject.File).Replace("/", "\\"));
        projectReference.AppendChild(testProject.Document.CreateElement("Project", Project.MsbuildNs)).InnerText =
          includeProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:ProjectGuid/text()").Value;
        projectReference.AppendChild(testProject.Document.CreateElement("Name", Project.MsbuildNs)).InnerText =
          Path.GetFileNameWithoutExtension(includeProject.File).Replace("/", "\\");

        projectReferenceItemGroup.AppendChild(projectReference);
      }

      if (0 < projectReferenceItemGroup.ChildNodes.Count)
        importNode.ParentNode.InsertBefore(projectReferenceItemGroup, importNode);

      /*
       * update compile file paths
       */
      foreach (XmlElement element in testProject.SelectNodes("/x:Project/x:ItemGroup/x:Compile")) {
        var includePath = element.GetAttribute("Include");

        includePath = Path.Combine(originalTestProjectDirectory, includePath);

        element.SetAttribute("Include", testProject.GetRelativePath(Path.GetFullPath(includePath)));
      }

      /*
       * update ProjectGuid,AssemblyName,RootNamespace
       */
      var projectGuidNode = testProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:ProjectGuid");
      var originalCsprojGuid = new Guid(projectGuidNode.InnerText);

      if (Guid.Empty.Equals(outputCsprojGuid) || originalCsprojGuid.Equals(outputCsprojGuid))
        outputCsprojGuid = Guid.NewGuid();

      projectGuidNode.InnerText = outputCsprojGuid.ToString("B").ToUpper();

      testProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:AssemblyName").InnerText =
        baseProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:AssemblyName/text()").Value + ".Tests";
      testProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:RootNamespace").InnerText =
        baseProject.SelectSingleNode("/x:Project/x:PropertyGroup[not(@Condition)]/x:RootNamespace/text()").Value + ".Tests";

      /*
       * change Project/PropertyGroup/OutputPath, Project/PropertyGroup/IntermediateOutputPath
       */
      foreach (XmlElement node in testProject.SelectNodes("x:Project/x:PropertyGroup/x:OutputPath")) {
        node.InnerText = node.InnerText.Replace(targetFramework, targetFramework + "-combined");
      }

      foreach (XmlElement node in testProject.SelectNodes("x:Project/x:PropertyGroup/x:IntermediateOutputPath")) {
        node.InnerText = node.InnerText.Replace(targetFramework, targetFramework + "-combined");
      }

      /*
       * remove unusing nodes
       */
      testProject.RemoveNodes("/x:Project/x:ItemGroup[x:Folder]");
      testProject.RemoveNodes("/x:Project/x:ItemGroup[x:None]");

      testProject.Save(testProject.File);

      Console.WriteLine("combined test-proj created {0}", testProject.File);
    }

  }
}
