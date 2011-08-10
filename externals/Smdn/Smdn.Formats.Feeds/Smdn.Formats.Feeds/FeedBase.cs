// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Smdn.Formats.Feeds.Modules;
using Smdn.Xml;

namespace Smdn.Formats.Feeds {
  public abstract class FeedBase {
    public abstract MimeType MimeType { get; }

    public XmlNode SourceNode {
      get; internal set;
    }

    public IDictionary<string, ModuleBase> Modules {
      get { return modules; }
    }

    public DublinCore DublinCoreModule {
      get
      {
        if (modules.ContainsKey(DublinCore.NamespaceUri))
          return modules[DublinCore.NamespaceUri] as DublinCore;
        else
          return DublinCore.Null;
      }
    }

    public Syndication SyndicationModule {
      get
      {
        if (modules.ContainsKey(Syndication.NamespaceUri))
          return modules[Syndication.NamespaceUri] as Syndication;
        else
          return Syndication.Null;
      }
    }

    public Image ImageModule {
      get
      {
        if (modules.ContainsKey(Image.NamespaceUri))
          return modules[Image.NamespaceUri] as Image;
        else
          return Image.Null;
      }
    }

#region "save"
    public void Save(string file)
    {
      Save(file, Encoding.UTF8);
    }

    public void Save(string file, Encoding encoding)
    {
      Save(file, CreateWriterDefaultSettings(encoding));
    }

    public void Save(string file, XmlWriterSettings settings)
    {
      using (var stream = File.OpenWrite(file)) {
        stream.SetLength(0L);

        Save(stream, settings);
      }
    }

    public void Save(Stream stream)
    {
      Save(stream, Encoding.UTF8);
    }

    public void Save(Stream stream, Encoding encoding)
    {
      Save(stream, CreateWriterDefaultSettings(encoding));
    }

    public void Save(Stream stream, XmlWriterSettings settings)
    {
      ToXmlDocument(settings.Encoding).WriteTo(stream, settings);
    }

    public void Save(TextWriter writer)
    {
      Save(writer, Encoding.UTF8);
    }

    public void Save(TextWriter writer, Encoding encoding)
    {
      Save(writer, CreateWriterDefaultSettings(encoding));
    }

    public void Save(TextWriter writer, XmlWriterSettings settings)
    {
      ToXmlDocument(settings.Encoding).WriteTo(writer, settings);
    }

    public void Save(XmlWriter writer)
    {
      ToXmlDocument(writer.Settings.Encoding ?? Encoding.UTF8).WriteTo(writer);

      writer.Flush();
    }

    protected static XmlWriterSettings CreateWriterDefaultSettings(Encoding encoding)
    {
      var settings = new XmlWriterSettings();

      settings.Encoding = encoding;
      settings.Indent = true;
      settings.IndentChars = " ";
      settings.NewLineChars = "\n";

      return settings;
    }
#endregion

    public XmlDocument ToXmlDocument()
    {
      return ToXmlDocument(Encoding.UTF8);
    }

    public XmlDocument ToXmlDocument(Encoding encoding)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      var document = new XmlDocument();

      document.AppendChild(document.CreateXmlDeclaration("1.0", encoding.WebName, "yes"));

      Format(document);

      return document;
    }

    protected abstract void Format(XmlDocument document);

    public override string ToString()
    {
      using (var writer = new StringWriter(new StringBuilder(4096))) {
        Save(writer);

        return writer.ToString();
      }
    }

    private /*readonly*/ Dictionary<string, ModuleBase> modules = new Dictionary<string, ModuleBase>();
  }
}
