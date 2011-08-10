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
using System.Text.RegularExpressions;

using Smdn.Formats.Notations.Dom;

namespace Smdn.Formats.Notations {
  public abstract class WikiLikeNotationParserBase : Parser {
    protected delegate Node InlineNotationParser(Match match);

    protected enum InlineNotationPriority : int {
      High = 1,
      Mid = 0,
      Low = -1,
    }

    protected class InlineNotationHandler {
      public Regex Regex;
      public InlineNotationParser Parser;
      public InlineNotationPriority Priority = InlineNotationPriority.Mid;
    }

    protected int LineNumber {
      get { return lineNumber; }
    }

    private int lineNumber = 0;

    protected Dictionary<string, InlineNotationHandler> InlineNotationHandlers {
      get { return inlineNotationHandlerDictionary; }
    }

    private Dictionary<string, InlineNotationHandler> inlineNotationHandlerDictionary = new Dictionary<string, InlineNotationHandler>();
    private List<InlineNotationHandler> inlineNotationHandlers;

    protected WikiLikeNotationParserBase(Dictionary<string, string> options)
    {
      inlineNotationHandlerDictionary.Add("entity reference", new InlineNotationHandler {
        Priority = InlineNotationPriority.Low,
        Regex = new Regex(@"&(?(#)#(?<hex>x?)(?<number>[0-9a-fA-F]+)|(?<name>\w+));"),
        Parser = delegate(Match match) {
          if (match.Groups["name"].Success)
            return new EntityReference(match.Groups["name"].Value);
          else
            return new Text(((match.Groups["hex"].Length == 0)
              ? (char)Convert.ToUInt16(match.Groups["number"].Value, 10)
              : (char)Convert.ToUInt16(match.Groups["number"].Value, 16)).ToString());
        },
      });
      inlineNotationHandlerDictionary.Add("http hyper link", new InlineNotationHandler {
        // TODO: ftp, news
        Priority = InlineNotationPriority.Low,
        Regex = new Regex(@"s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+(?<![\)\}\]])"),
        Parser = delegate(Match match) { return new Anchor(match.Value, new[] {new Text(match.Value)}); },
      });
      inlineNotationHandlerDictionary.Add("mailto hyper link", new InlineNotationHandler {
        // TODO: more strict
        Priority = InlineNotationPriority.Low,
        Regex = new Regex(@"mailto:[a-zA-Z0-9_\-\.]+@[a-zA-Z0-9_\-\.]+\.[a-zA-Z0-9_\-\.]+"),
        Parser = delegate(Match match) { return new Anchor(match.Value, new[] {new Text(match.Value)}); },
      });
    }

    private void PrepareInlineNotationHandlers()
    {
      inlineNotationHandlers = new List<InlineNotationHandler>(inlineNotationHandlerDictionary.Values);

      inlineNotationHandlers.Sort(delegate(InlineNotationHandler x, InlineNotationHandler y) {
        //return -((int)x.Priority - (int)y.Priority);
        return ((int)y.Priority - (int)x.Priority);
      });
    }

    public override Document Parse(TextReader reader)
    {
      PrepareInlineNotationHandlers();

      var document = new Document();

      ParseAll(reader, document);

      return document;
    }

    private void ParseAll(TextReader reader, Document document)
    {
      ParseBlock(reader, document);
    }

    private void ParseBlock(TextReader reader, Document document)
    {
      try {
        for (lineNumber = 0; ; lineNumber++) {
          var line = reader.ReadLine();

          if (line == null)
            break;

          if (line.EndsWith("\u000c", StringComparison.Ordinal)) {
            PreParseLine(line.Substring(0, line.Length - 1), document);

            var e = new ParserFormFeedEventArgs();

            OnFormFeed(e);

            if (!e.Cancel)
              return;
          }
          else {
            PreParseLine(line, document);
          }
        }
      }
      catch (Exception ex) {
        throw new Exception(string.Format("parsing exception at line {0}", lineNumber), ex);
      }
    }

    protected virtual void PreParseLine(string line, Document document)
    {
      Node parsed = null;

      if (0 < line.Length) {
        // nestable blocks
        if (TryParseNestableBlock(line, document))
          return;

        // unnestable blocks
        if (line[0] == '*')
          parsed = ParseHeaderBlock(line);
        else
          parsed = ParseLine(line, document);
      }
      else {
        parsed = ParseLine(line, document);
      }

      if (parsed != null)
        document.Nodes.Add(parsed);
    }

    protected virtual Node ParseLine(string line, Document document)
    {
      return ProcessUnparsedString(line);
    }

    protected virtual bool TryParseNestableBlock(string line, Document document)
    {
      switch (line[0]) {
        case '+':
          return TryParseOrderedListBlock(line, int.MaxValue, document);

        case '-':
          return TryParseUnorderedListBlock(line, int.MaxValue, document);

        default:
          return false;
      }
    }

    protected virtual bool TryParseOrderedListBlock(string line, int maxNest, Document document)
    {
      ParseOrderedListBlock(line, maxNest, document);

      return true;
    }

    private void ParseOrderedListBlock(string line, int maxNest, Container<Node> parent)
    {
      ParseEnumListBlock<OrderedList>(line, '+', maxNest, parent);
    }

    protected virtual bool TryParseUnorderedListBlock(string line, int maxNest, Document document)
    {
      ParseUnorderedListBlock(line, maxNest, document);

      return true;
    }

    private void ParseUnorderedListBlock(string line, int maxNest, Container<Node> parent)
    {
      ParseEnumListBlock<UnorderedList>(line, '-', maxNest, parent);
    }

    private void ParseEnumListBlock<TList>(string line, char notation, int maxNest, Container<Node> parent) where TList : ListItem, new()
    {
      line = line.Substring(1);

      var list = parent.LastChild as TList;

      if (list == null) {
        list = new TList();
        parent.Nodes.Add(list);
      }

      if (0 < --maxNest && 0 < line.Length && line[0] == notation) {
        var item = list.LastChild as ListItem;

        if (item == null) {
          item = new ListItem();
          list.Nodes.Add(item);
        }

        ParseEnumListBlock<TList>(line, notation, maxNest, item);
      }
      else {
        list.Nodes.Add(new ListItem(ParseInline(line)));
      }
    }

    protected virtual Container<Node> ParseHeaderBlock(string line)
    {
      if (line.StartsWith("***", StringComparison.Ordinal))
        return new Header(4, ParseInline(line.Substring(3)));
      else if (line.StartsWith("**", StringComparison.Ordinal))
        return new Header(3, ParseInline(line.Substring(2)));
      else
        return new Header(2, ParseInline(line.Substring(1)));
    }

    protected virtual IEnumerable<Node> ParseInline(string line)
    {
      return ParseInlineCore(line);
    }

    private IEnumerable<Node> ParseInlineCore(string line)
    {
      foreach (var handler in inlineNotationHandlers) {
        var match = handler.Regex.Match(line);

        if (match.Success) {
          var nodes = new List<Node>();

          if (0 < match.Index)
            nodes.AddRange(ParseInlineCore(line.Substring(0, match.Index)));

          nodes.Add(handler.Parser(match));

          if (match.Index + match.Length < line.Length)
            nodes.AddRange(ParseInlineCore(line.Substring(match.Index + match.Length)));

          return nodes;
        }
      }

      // nothing matched
      return new[] {new Text(line)};
    }
  }
}
