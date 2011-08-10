using System;
using System.Text;
using System.IO;
using System.Xml;

class MainClass
{
  public static void Main(string[] args)
  {
    var csproj = new XmlDocument();

    csproj.Load(args[0]);

    var nsmgr = new XmlNamespaceManager(csproj.NameTable);

    nsmgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

    var standardHeaderNode = csproj.SelectSingleNode("/x:Project/x:ProjectExtensions/x:MonoDevelop/x:Properties/x:Policies/x:StandardHeader/@Text", nsmgr);

    var standardHeaderReader = new StringReader(standardHeaderNode.Value);
    var standardHeader = new StringBuilder();

    for (;;) {
      var line = standardHeaderReader.ReadLine();

      if (line == null)
        break;

      standardHeader.AppendFormat("// {0}", line);
      standardHeader.Append(Environment.NewLine);
    }

    standardHeader.Replace("${AuthorName}", "smdn");
    standardHeader.Replace("${AuthorEmail}", "smdn@smdn.jp");
    standardHeader.Replace("${Year}", DateTime.Now.ToString("yyyy"));
    standardHeader.Replace("${CopyrightHolder}", "smdn");

    var basePath = Path.GetDirectoryName(args[0]);

    foreach (XmlNode node in csproj.SelectNodes("/x:Project/x:ItemGroup/x:Compile/@Include", nsmgr)) {
      if (node.Value.StartsWith("."))
        continue;
      if (Path.GetExtension(node.Value) != ".cs")
        continue;

      var sourcePath = Path.Combine(basePath, node.Value.Replace("\\", "/"));

      var source = new StringBuilder(standardHeader.ToString());

      source.Append(Environment.NewLine);
      source.Append(File.ReadAllText(sourcePath));

      File.WriteAllText(sourcePath, source.ToString());

      Console.WriteLine("processed {0}", sourcePath);
    }

    Console.WriteLine("done");
  }
}