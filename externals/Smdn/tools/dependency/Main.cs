using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace dependency {
  class ProjectEx : Project {
    public string Name;
    public List<string> Depends = new List<string>();
    public List<string> Refers = new List<string>();
    public List<string> ReferedBy = new List<string>();

    public new static ProjectEx Load(string file)
    {
      var proj = Project.Load(file);
      var projex = new ProjectEx();

      projex.Nsmgr = proj.Nsmgr;
      projex.Document = proj.Document;

      projex.Name = projex.SelectSingleNode("/x:Project/x:PropertyGroup/x:AssemblyName/text()").Value;

      foreach (XmlNode node in projex.SelectNodes("/x:Project/x:ItemGroup/x:Reference/@Include")) {
        projex.Depends.Add(node.Value);
      }

      foreach (XmlNode node in projex.SelectNodes("/x:Project/x:ItemGroup/x:ProjectReference/x:Name/text()")) {
        projex.ReferedBy.Add(node.Value);
      }

      return projex;
    }
  }

  class MainClass {
    public static void Main(string[] args)
    {
#if DEBUG
      args = new[] {"../../../../"};
#endif
      var libRootDir = Directory.GetParent(args[0]);

      var csprojFiles = new List<FileInfo>();

      foreach (var libDir in libRootDir.GetDirectories("Smdn*")) {
        csprojFiles.AddRange(libDir.GetFiles("*.csproj", SearchOption.TopDirectoryOnly));
      }

      /*
       * read csproj
       */
      var projects = new Dictionary<string, ProjectEx>();

      foreach (var csprojFile in csprojFiles) {
        var proj = ProjectEx.Load(csprojFile.FullName);

        projects.Add(proj.Name, proj);
      }

      foreach (var proj in projects.Values) {
        foreach (var r in proj.Refers) {
          ProjectEx dependsOnProj;

          if (projects.TryGetValue(r, out dependsOnProj))
            dependsOnProj.ReferedBy.Add(proj.Name);
        }
      }

      Console.WriteLine("[Reference]");
      foreach (var proj in projects.Values) {
        if (proj.Refers.Count == 0)
          PrintReferenceTree(proj, 0, projects);
      }

      Console.WriteLine();
      Console.WriteLine("[Dependency]");
      foreach (var proj in projects.Values) {
        PrintDependencyTree(proj, 0, projects);
      }
    }

    private static void PrintDependencyTree(ProjectEx proj, int nest, Dictionary<string, ProjectEx> projects)
    {
      Console.WriteLine("{0}{1}",
                        new string(' ', nest * 4),
                        proj.Name);

      var indent = new string(' ', (nest + 1) * 4);

      if (nest == 0) {
        foreach (var dll in proj.Depends) {
          Console.Write(indent);
          Console.WriteLine("{0}", dll);
        }
      }

      foreach (var r in proj.Refers) {
        PrintDependencyTree(projects[r], nest + 1, projects);
      }
    }

    private static void PrintReferenceTree(ProjectEx proj, int nest, Dictionary<string, ProjectEx> projects)
    {
      Console.WriteLine("{0}{1}",
                        new string(' ', nest * 4),
                        proj.Name);

      foreach (var r in proj.ReferedBy) {
        PrintReferenceTree(projects[r], nest + 1, projects);
      }
    }
  }
}
