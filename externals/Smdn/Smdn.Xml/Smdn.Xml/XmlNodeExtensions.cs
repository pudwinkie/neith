// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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

namespace Smdn.Xml {
  public static class XmlNodeExtensions {
    public delegate T NodeConverter<T>(XmlNode node, XmlNamespaceManager nsmgr);

    public static XmlElement AppendElement(this XmlNode node, string name)
    {
      if (node is XmlDocument)
        return node.AppendChild((node as XmlDocument).CreateElement(name)) as XmlElement;
      else
        return node.AppendChild(node.OwnerDocument.CreateElement(name)) as XmlElement;
    }

    public static XmlText AppendText(this XmlNode node, string text)
    {
      if (node is XmlDocument)
        throw new InvalidOperationException();
      else
        return node.AppendChild(node.OwnerDocument.CreateTextNode(text)) as XmlText;
    }

    public static void AppendChildren(this XmlNode node, IEnumerable<XmlNode> newChildren)
    {
      foreach (var newChild in newChildren) {
        node.AppendChild(newChild);
      }
    }

    public static void PrependChildren(this XmlNode node, IEnumerable<XmlNode> newChildren)
    {
      if (node.FirstChild == null)
        AppendChildren(node, newChildren);
      else
        InsertBefore(node, newChildren, node.FirstChild);
    }

    public static void InsertBefore(this XmlNode node, IEnumerable<XmlNode> newChildren, XmlNode refChild)
    {
      foreach (var newChild in newChildren) {
        node.InsertBefore(newChild, refChild);
      }
    }

    public static void RemoveNonAttributes(this XmlNode node)
    {
      var nodes = new List<XmlNode>();

      foreach (XmlNode n in node.ChildNodes) {
        if (!(n is XmlAttribute))
          nodes.Add(n);
      }

      foreach (var n in nodes) {
        node.RemoveChild(n);
      }
    }

    public static void RemoveSelf(this XmlNode node)
    {
      if (node == null)
        return;

      if (node.ParentNode != null)
        node.ParentNode.RemoveChild(node);
    }

    public static void RemoveChidlren(this XmlNode node, IEnumerable<XmlNode> oldChildren)
    {
      if (node == null)
        return;

      foreach (var oldChild in oldChildren) {
        node.RemoveChild(oldChild);
      }
    }

    public static void PutOutChildNodes(this XmlNode node)
    {
      if (node == null)
        return;

      var parent = node.ParentNode;
      var nextSibling = node.NextSibling;

      var children = new List<XmlNode>();

      foreach (XmlNode child in node.ChildNodes) {
        children.Add(child);
      }

      node.RemoveAll();

      parent.RemoveChild(node);

      if (nextSibling == null)
        parent.AppendChildren(children);
      else
        parent.InsertBefore(children, nextSibling);
    }

    public static string GetSingleNodeValueOf(this XmlNode node, string xpath)
    {
      return GetSingleNodeValueOf(node, xpath, null);
    }

    public static string GetSingleNodeValueOf(this XmlNode node, string xpath, XmlNamespaceManager nsmgr)
    {
      var selectedNode = node.SelectSingleNode(xpath, nsmgr);

      if (selectedNode == null)
        return null;
      else
        return selectedNode.Value;
    }

    public static IEnumerable<string> GetNodeValuesOf(this XmlNode node, string xpath)
    {
      return GetNodeValuesOf(node, xpath, null);
    }

    public static IEnumerable<string> GetNodeValuesOf(this XmlNode node, string xpath, XmlNamespaceManager nsmgr)
    {
      foreach (XmlNode selectedNode in node.SelectNodes(xpath, nsmgr)) {
        yield return selectedNode.Value;
      }
    }

    public static T GetSingleNodeValueOf<T>(this XmlNode node, string xpath, Converter<string, T> convert)
    {
      return GetSingleNodeValueOf(node, xpath, null, convert);
    }

    public static T GetSingleNodeValueOf<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, Converter<string, T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      return convert(GetSingleNodeValueOf(node, xpath, nsmgr));
    }

    /// <remarks>This method returns <paramref name="defaultValue"/> if node not found or error occurred.</remarks>
    public static T GetSingleNodeValueOf<T>(this XmlNode node, string xpath, T defaultValue, Converter<string, T> convert)
    {
      return GetSingleNodeValueOf(node, xpath, null, defaultValue, convert);
    }

    /// <remarks>This method returns <paramref name="defaultValue"/> if node not found or error occurred.</remarks>
    public static T GetSingleNodeValueOf<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, T defaultValue, Converter<string, T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      var val = GetSingleNodeValueOf(node, xpath, nsmgr);

      try {
        if (val != null)
          return convert(val);
      }
      catch {
        // ignore exceptions
      }

      return defaultValue;
    }

    public static IEnumerable<T> GetNodeValuesOf<T>(this XmlNode node, string xpath, Converter<string, T> convert)
    {
      return GetNodeValuesOf(node, xpath, null, convert);
    }

    public static IEnumerable<T> GetNodeValuesOf<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, Converter<string, T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      foreach (XmlNode selectedNode in node.SelectNodes(xpath, nsmgr)) {
        yield return convert(selectedNode.Value);
      }
    }

    public static T ConvertSingleNodeTo<T>(this XmlNode node, string xpath, Converter<XmlNode, T> convert)
    {
      return ConvertSingleNodeTo(node, xpath, null, convert);
    }

    public static T ConvertSingleNodeTo<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, Converter<XmlNode, T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      return convert(node.SelectSingleNode(xpath, nsmgr));
    }

    public static T ConvertSingleNodeTo<T>(this XmlNode node, string xpath, NodeConverter<T> convert)
    {
      return ConvertSingleNodeTo(node, xpath, null, convert);
    }

    public static T ConvertSingleNodeTo<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, NodeConverter<T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      return convert(node.SelectSingleNode(xpath, nsmgr), nsmgr);
    }

    public static IEnumerable<T> ConvertNodesTo<T>(this XmlNode node, string xpath, Converter<XmlNode, T> convert)
    {
      return ConvertNodesTo(node, xpath, null, convert);
    }

    public static IEnumerable<T> ConvertNodesTo<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, Converter<XmlNode, T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      foreach (XmlNode selectedNode in node.SelectNodes(xpath, nsmgr)) {
        yield return convert(selectedNode);
      }
    }

    public static IEnumerable<T> ConvertNodesTo<T>(this XmlNode node, string xpath, NodeConverter<T> convert)
    {
      return ConvertNodesTo(node, xpath, null, convert);
    }

    public static IEnumerable<T> ConvertNodesTo<T>(this XmlNode node, string xpath, XmlNamespaceManager nsmgr, NodeConverter<T> convert)
    {
      if (convert == null)
        throw new ArgumentNullException("convert");

      foreach (XmlNode selectedNode in node.SelectNodes(xpath, nsmgr)) {
        yield return convert(selectedNode, nsmgr);
      }
    }

    public static void WriteTo(this XmlNode node, string file, XmlWriterSettings settings)
    {
      if (file == null)
        throw new ArgumentNullException("file");

      if (settings == null)
        WriteTo(node, XmlWriter.Create(file));
      else
        WriteTo(node, XmlWriter.Create(file, settings));
    }

    public static void WriteContentTo(this XmlNode node, string file, XmlWriterSettings settings)
    {
      if (file == null)
        throw new ArgumentNullException("file");

      if (settings == null)
        WriteContentTo(node, XmlWriter.Create(file));
      else
        WriteContentTo(node, XmlWriter.Create(file, settings));
    }

    public static void WriteTo(this XmlNode node, StringBuilder builder, XmlWriterSettings settings)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");

      if (settings == null)
        WriteTo(node, XmlWriter.Create(builder));
      else
        WriteTo(node, XmlWriter.Create(builder, settings));
    }

    public static void WriteContentTo(this XmlNode node, StringBuilder builder, XmlWriterSettings settings)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");

      if (settings == null)
        WriteContentTo(node, XmlWriter.Create(builder));
      else
        WriteContentTo(node, XmlWriter.Create(builder, settings));
    }

    public static void WriteTo(this XmlNode node, TextWriter writer, XmlWriterSettings settings)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");

      if (settings == null)
        WriteTo(node, XmlWriter.Create(writer));
      else
        WriteTo(node, XmlWriter.Create(writer, settings));
    }

    public static void WriteContentTo(this XmlNode node, TextWriter writer, XmlWriterSettings settings)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");

      if (settings == null)
        WriteContentTo(node, XmlWriter.Create(writer));
      else
        WriteContentTo(node, XmlWriter.Create(writer, settings));
    }

    public static void WriteTo(this XmlNode node, Stream stream, XmlWriterSettings settings)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      if (settings == null)
        WriteTo(node, XmlWriter.Create(stream));
      else
        WriteTo(node, XmlWriter.Create(stream, settings));
    }

    public static void WriteContentTo(this XmlNode node, Stream stream, XmlWriterSettings settings)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      if (settings == null)
        WriteContentTo(node, XmlWriter.Create(stream));
      else
        WriteContentTo(node, XmlWriter.Create(stream, settings));
    }

    private static void WriteTo(XmlNode node, XmlWriter writer)
    {
      node.WriteTo(writer);

      writer.Flush();
    }

    private static void WriteContentTo(XmlNode node, XmlWriter writer)
    {
      node.WriteContentTo(writer);

      writer.Flush();
    }
  }
}
