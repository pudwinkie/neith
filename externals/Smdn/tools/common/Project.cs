using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class Project {
  public const string MsbuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";

  public XmlDocument Document;
  public XmlNamespaceManager Nsmgr;
  public string File;
  public Uri BaseUri {
    get { return baseUri; }
  }

  private Uri baseUri;

  public void SetBaseDirectory(string path)
  {
    baseUri = new Uri(path);
  }

  public XmlNode SelectSingleNode(string xpath)
  {
    return Document.SelectSingleNode(xpath, Nsmgr);
  }

  public XmlNodeList SelectNodes(string xpath)
  {
    return Document.SelectNodes(xpath, Nsmgr);
  }

  public string GetDirectory()
  {
    return Path.GetDirectoryName(File);
  }

  public void RemoveNodes(string xpath)
  {
    var nodes = new List<XmlNode>();

    foreach (XmlNode node in Document.SelectNodes(xpath, Nsmgr)) {
      nodes.Add(node);
    }

    foreach (var node in nodes) {
      node.ParentNode.RemoveChild(node);
    }
  }

  public void Save(string file)
  {
    var encoding = new System.Text.UTF8Encoding(false); // no bom
    var settings = new XmlWriterSettings();

    settings.Encoding = encoding;
    settings.NewLineChars = "\r\n";
    settings.Indent = true;
    settings.IndentChars = "  ";

    using (var stream = System.IO.File.OpenWrite(file)) {
      stream.SetLength(0L);

      var writer = XmlWriter.Create(stream, settings);

      Document.WriteTo(writer);

      writer.Flush();

      stream.WriteByte(0x0d);
      stream.WriteByte(0x0a);
    }
  }

  public static Project Load(string file)
  {
    var project = new Project();

    project.File = file;

    project.Document = new XmlDocument();
    project.Document.Load(file);

    project.Nsmgr = new XmlNamespaceManager(project.Document.NameTable);
    project.Nsmgr.AddNamespace("x", MsbuildNs);

    project.SetBaseDirectory(Path.GetFullPath(file));

    return project;
  }

  public string GetRelativePath(string targetAbsolutePath)
  {
    return GetRelativePath(new Uri(targetAbsolutePath));
  }

  public string GetRelativePath(Uri targetAbsolutePath)
  {
    return GetRelativePath(baseUri, targetAbsolutePath);
  }

  public static string GetRelativePath(string baseAbsolutePath, string targetAbsolutePath)
  {
    return GetRelativePath(new Uri(baseAbsolutePath), new Uri(targetAbsolutePath));
  }

  public static string GetRelativePath(Uri baseAbsolutePath, string targetAbsolutePath)
  {
    return GetRelativePath(baseAbsolutePath, new Uri(targetAbsolutePath));
  }

  public static string GetRelativePath(Uri baseAbsolutePath, Uri targetAbsolutePath)
  {
    return baseAbsolutePath.MakeRelativeUri(targetAbsolutePath).ToString();
  }
}