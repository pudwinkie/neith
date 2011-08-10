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
using System.Text.RegularExpressions;

using Smdn.Formats;
using Smdn.Formats.Notations;
using Smdn.Formats.Notations.Dom;
using Smdn.Formats.Notations.PukiWiki.Dom;

namespace Smdn.Formats.Notations.PukiWiki {
  public class Parser : WikiLikeNotationParserBase {
    private static List<string> inlinePluginNames = new List<string>() {
      "t",
      "br",
      "counter",
      "online",
      "version",
      "page",
      "fpage",
      "date",
      "_date",
      "time",
      "_time",
      "now",
      "_now",
      "lastmod",
      "heart",
      "aname",
      // face
      "smile",
      "bigsmile",
      "huh",
      "oh",
      "wink",
      "sad",
      "worried",
    };

    private const string inlinePluginRegexString = @"\&(?<name>[\w\d_]+)(\((?<args>.*?)\))?";
    private Regex blockPluginRegex = new Regex(@"^#[a-zA-Z0-9]+(\([^\)]*\))?");
    private Regex headerAnchorRegex = new Regex(@"\[\#(?<id>[0-9a-z]{8})\]$");

    public bool ConvertLineBreak {
      get; set;
    }

    public bool LimitListNesting {
      get; set;
    }

    public bool EnableSectioning {
      get; set;
    }

    public Parser(Dictionary<string, string> options)
      : base(options)
    {
      this.ConvertLineBreak = options.ContainsKey("convert-line-break");
      this.LimitListNesting = !options.ContainsKey("non-limited-list");
      this.EnableSectioning = options.ContainsKey("enable-sectioning");

      InlineNotationHandlers.Add("inline plugin", new InlineNotationHandler {
        // &name(args);
        Regex = new Regex(inlinePluginRegexString + ";"),
        Parser = delegate(Match match) {
          var name = match.Groups["name"].Value;
          var args = match.Groups["args"].Value;

          if (0 < args.Length) {
            return new InlinePlugin(name, Csv.ToSplitted(args), new Node[] {});
          }
          else {
            if (inlinePluginNames.Contains(name))
              return new InlinePlugin(name, new string[] {});
            else
              // entity (&amp; etc..)
              return new EntityReference(name);
          }
        }
      });

      InlineNotationHandlers.Add("mail address", new InlineNotationHandler {
        // TODO: more strict
        Regex = new Regex(@"\w[a-zA-Z0-9_\-\.]+@[a-zA-Z0-9_\-\.]+\.[a-zA-Z0-9_\-\.]+\w"),
        Parser = delegate(Match match) { return new Anchor(string.Format("mailto:{0}", match.Value), new[] {new Text(match.Value)}); },
      });

      InlineNotationHandlers.Add("page link", new InlineNotationHandler {
        // [[title:href]]
        Regex = new Regex(@"\[{2}(?!\[)(?<title>.+?):(?<href>s?https?://(?:(?!\]\]).)+)\]{2}"),
        Parser = delegate(Match match) {
          return new Anchor(match.Groups["href"].Value,
                            ParseInline(match.Groups["title"].Value));
        }
      });

      InlineNotationHandlers.Add("alias", new InlineNotationHandler {
        // [[name>aliasto]]
        Regex = new Regex(@"\[{2}(?!\[)(?<nameandalias>.+?)\]{2}"),
        Parser = delegate(Match match) {
          var nameAndAlias = match.Groups["nameandalias"].Value;
          var delim = nameAndAlias.LastIndexOf('>');

          if (delim < 0 || nameAndAlias.Length - 1 <= delim)
            return new Alias(nameAndAlias);
          else
            return new Alias(nameAndAlias.Substring(0, delim), nameAndAlias.Substring(delim + 1));
        }
      });
    }

    public override Document Parse(TextReader reader)
    {
      var document = base.Parse(reader);

      if (EnableSectioning)
        return GetSectionizeDocument(document);
      else
        return document;
    }

    private Document GetSectionizeDocument(Document document)
    {
      var nodes = new List<Node>(document.Nodes);

      var rootSection = new Section();
      var currentSection = rootSection;

      for (var i = 0; i < nodes.Count; i++) {
        var node = nodes[i];

        if (node is Header) {
          Section parentSection = null;
          currentSection = rootSection;

          for (var h = 0; h < (node as Header).Level - 1; h++) {
            if (!(currentSection.LastChild is Section))
              currentSection.Nodes.Add(new Section());

            parentSection = currentSection;
            currentSection = currentSection.LastChild as Section;
          }

          if (currentSection.Nodes.Count != 0) {
            currentSection = new Section();
            parentSection.Nodes.Add(currentSection);
          }
        }

        currentSection.Nodes.Add(node);
      }

      var sectionized = new Document();

      sectionized.Nodes.Add(rootSection);

      return sectionized;
    }

    protected Container<Node> GetLastBlockContainable(Document document)
    {
      var blockContainable = (Container<Node>)document;

      for (;;) {
        var containable = GetNestedBlockContainableNodeOf(blockContainable);

        if (containable == null)
          break;
        else
          blockContainable = containable;
      }

      return blockContainable;
    }

    protected virtual Container<Node> GetNestedBlockContainableNodeOf(Container<Node> container)
    {
      if (container.LastChild is DefinitionList)
        return (container.LastChild as DefinitionList).LastChild as DefinitionListItem;
      else if (container.LastChild is DefinitionListItem)
        return container.LastChild as DefinitionListItem;
      else if (container.LastChild is OrderedList)
        return (container.LastChild as OrderedList).LastChild as ListItem;
      else if (container.LastChild is UnorderedList)
        return (container.LastChild as UnorderedList).LastChild as ListItem;
      else
        return null;
    }

    protected override void PreParseLine(string line, Document document)
    {
      var multilineBlockPlugin = GetLastBlockContainable(document).LastChild as MultilineBlockPlugin;

      if (multilineBlockPlugin != null) {
        if (string.Equals(line, multilineBlockPlugin.Delimiter, StringComparison.Ordinal)) {
          // end of multiline plugin
          document.Nodes.Add(new EmptyLine());
        }
        else {
          if (multilineBlockPlugin.IsLastArgumentEmpty)
            multilineBlockPlugin.Append(line);
          else
            multilineBlockPlugin.Append(Environment.NewLine, line);
        }
      }
      else {
        base.PreParseLine(line, document);
      }
    }

    protected override bool TryParseNestableBlock(string line, Document document)
    {
      switch (line[0]) {
        case '-':
        case '+': {
          var list = GetLastBlockContainable(document) as ListItem;

          if (list == null) {
            return base.TryParseNestableBlock(line, document);
          }
          else {
            if (line[0] == '-') {
              var level = 0;

              if (LimitListNesting) {
                if (string.Equals(line, "-", StringComparison.Ordinal))
                  level = 1;
                else if (string.Equals(line, "--", StringComparison.Ordinal))
                  level = 2;
                else if (string.Equals(line, "---", StringComparison.Ordinal))
                  level = 3;
                else
                  return base.TryParseNestableBlock(line, document);
              }
              else {
                for (level = 0; level < line.Length; level++) {
                  if (line[level] != '-')
                    return base.TryParseNestableBlock(line, document);
                }
              }

              ParseEnumListBlockPlaceHolder<UnorderedList>(level, list.Parent as UnorderedList);

              return true;
            }
            else {
              var level = 0;

              if (LimitListNesting) {
                if (string.Equals(line, "+", StringComparison.Ordinal))
                  level = 1;
                else if (string.Equals(line, "++", StringComparison.Ordinal))
                  level = 2;
                else if (string.Equals(line, "+++", StringComparison.Ordinal))
                  level = 3;
                else
                  return base.TryParseNestableBlock(line, document);
              }
              else {
                for (level = 0; level < line.Length; level++) {
                  if (line[level] != '+')
                    return base.TryParseNestableBlock(line, document);
                }
              }

              ParseEnumListBlockPlaceHolder<OrderedList>(level, list.Parent as OrderedList);

              return true;
            }
          }
        }

        case ':':
          if (line.IndexOf('|', 1) < 0)
            return false;

          if (LimitListNesting)
            ParseDefinitionListBlock(line, 3, document);
          else
            ParseDefinitionListBlock(line, int.MaxValue, document);

          return true;

        case '>':
          ParseQuoteBlock(line, document);
          return true;

        default:
          return base.TryParseNestableBlock(line, document);
      }
    }

    // FIXME:
    private void ParseEnumListBlockPlaceHolder<TList>(int level, TList parent) where TList : ListItem, new()
    {
      for (;0 < level; level--) {
        var li = new ListItem();

        parent.Nodes.Add(li);

        if (1 < level) {
          var el = new TList();

          li.Nodes.Add(el);

          parent = el;
        }
      }
    }

    protected override Node ParseLine(string line, Document document)
    {
      if (line.Length == 0)
        return ParseEmptyLine();

      Node parsed = null;

      // TODO: ブロック要素の入れ子の考慮
      switch (line[0]) {
        case ' ':
          ParsePreformattedBlock(line, document);
          return null;
        case '~':
          ParseParagraphBlock(line, document);
          return null;
        case '|':
          ParseTableBlock(line, document);
          return null;
        case ',':
          parsed = ParseCsvTableBlock(line);
          break;
        case '#':
          if (blockPluginRegex.IsMatch(line))
            ParsePluginBlock(line, document);
          else
            ParseParagraphBlock(line, document);
          return null;
        default:
          if (line.StartsWith("//", StringComparison.Ordinal)) {
            parsed = ParseCommentBlock(line);
          }
          else if (LimitListNesting && line.StartsWith("----", StringComparison.Ordinal)) {
            parsed = new HorizontalRule();
          }
          else if (line.StartsWith("LEFT:", StringComparison.Ordinal)) {
            parsed = new AlignedParagraph("left", ParseInline(line.Substring(5)));
          }
          else if (line.StartsWith("RIGHT:", StringComparison.Ordinal)) {
            parsed = new AlignedParagraph("right", ParseInline(line.Substring(6)));
          }
          else if (line.StartsWith("CENTER:", StringComparison.Ordinal)) {
            parsed = new AlignedParagraph("center", ParseInline(line.Substring(7)));
          }
          else {
            ParseParagraphBlock(line, document);
            return null;
          }
          break;
      }

      if (parsed == null)
        return base.ParseLine(line, document);
      else
        return parsed;
    }

    protected override Container<Node> ParseHeaderBlock(string line)
    {
      var match = headerAnchorRegex.Match(line);
      HeaderAnchor anchor = null;

      if (match.Success) {
        anchor = new HeaderAnchor(match.Groups["id"].Value);

        line = line.Substring(0, match.Index);
      }

      var header = base.ParseHeaderBlock(line);

      if (anchor != null)
        header.Nodes.Add(anchor);

      return header;
    }

    protected override bool TryParseOrderedListBlock (string line, int maxNest, Document document)
    {
      if (LimitListNesting)
        return base.TryParseOrderedListBlock(line, 3, document);
      else
        return base.TryParseOrderedListBlock(line, int.MaxValue, document);
    }

    protected override bool TryParseUnorderedListBlock(string line, int maxNest, Document document)
    {
      if (LimitListNesting) {
        if (line.StartsWith("----", StringComparison.Ordinal))
          return false;
        else
          return base.TryParseUnorderedListBlock(line, 3, document);
      }
      else {
        return base.TryParseUnorderedListBlock(line, int.MaxValue, document);
      }
    }

    private void ParseDefinitionListBlock(string line, int maxNest, Container<Node> parent)
    {
      var list = parent.LastChild as DefinitionList;

      if (list == null) {
        list = new DefinitionList();
        parent.Nodes.Add(list);
      }

      line = line.Substring(1);

      if (0 < --maxNest && 0 < line.Length && line[0] == ':') {
        ParseDefinitionListBlock(line, maxNest, list);
      }
      else {
        // 先にインラインの記法を処理してから分割位置を判断する
        var termNodes = new List<Node>();
        var descNodes = new List<Node>();
        var container = termNodes;

        foreach (var node in ParseInline(line)) {
          if (node is Text) {
            var text = node as Text;
            var delim = text.Value.IndexOf('|');

            // ノードがTextで、termDelimiterを含む場合は、それをtermとdescriptionの
            // 分割位置と判断する
            if (0 <= delim && !(text is Comment)) {
              container.Add(new Text(text.Value.Substring(0, delim)));

              container = descNodes;

              if (delim + 1 < text.Value.Length)
                container.Add(new Text(text.Value.Substring(delim + 1)));
            }
            else {
              container.Add(node);
            }
          }
          else {
            container.Add(node);
          }
        }

        list.Nodes.Add(new DefinitionListItem(termNodes, descNodes));
      }
    }

    private void ParseQuoteBlock(string line, Container<Node> parent)
    {
      if (line.StartsWith("<", StringComparison.Ordinal))
        throw new NotImplementedException("end-quoting is not implemented");

      line = line.Substring(1);

      var quot = parent.LastChild as BlockQuotation;

      if (quot == null) {
        quot = new BlockQuotation();
        parent.Nodes.Add(quot);
      }

      if (0 < line.Length && line[0] == '>')
        ParseQuoteBlock(line, quot);
      else
        quot.Nodes.AddRange(ParseInline(line));
    }

    private Node ParsePreformattedBlock(string line, Document document)
    {
      var container = GetLastBlockContainable(document);
      var pre = container.LastChild as Preformatted;

      if (pre == null) {
        pre = new Preformatted(line.Substring(1), true);
        container.Nodes.Add(pre);
      }
      else {
        pre.Append(Environment.NewLine, line.Substring(1));
      }

      return pre;
    }

    private Node ParseEmptyLine()
    {
      return new EmptyLine();
    }

    private Container<Node> ParseParagraphBlock(string line, Document document)
    {
      var container = GetLastBlockContainable(document);
      var nodes = line.StartsWith("~", StringComparison.Ordinal)
        ? ParseInline(line.Substring(1))
        : ParseInline(line);

      var p = container.LastChild as Paragraph;

      if (p == null) {
        if (container == document) {
          p = new Paragraph(nodes);
          document.Nodes.Add(p);
        }
        else {
          if (ConvertLineBreak && 0 < container.Nodes.Count)
            container.Nodes.Add(new ForcedLineBreak());

          container.Nodes.AddRange(nodes);
        }
      }
      else {
        if (ConvertLineBreak && 0 < p.Nodes.Count)
          p.Nodes.Add(new ForcedLineBreak());

        p.Nodes.AddRange(nodes);
      }

      return p;
    }

    protected virtual Node ParseTableBlock(string line, Document document)
    {
      if (line.IndexOf('|', 1) < 0)
        return ParseParagraphBlock(line, document);

      line = line.Substring(1);

      // row
      TableRow row = null;

      if (line.EndsWith("|h", StringComparison.Ordinal)) {
        line = line.Substring(0, line.Length - 2);
        row = new TableHeaderRow();
      }
      else if (line.EndsWith("|f", StringComparison.Ordinal)) {
        line = line.Substring(0, line.Length - 2);
        row = new TableFooterRow();
      }
      else {
        if (line.EndsWith("|", StringComparison.Ordinal))
          line = line.Substring(0, line.Length - 1);
        row = new TableRow();
      }

      var table = GetLastOrCreateNewTable(document);

      table.Nodes.Add(row);

      // columns
      foreach (var column in line.Split(new char[] {'|'})) {
        var c = ParseTableColumn(column, row);

        if (c != null)
          row.Nodes.Add(c);
      }

      return row;
    }

    protected Table GetLastOrCreateNewTable(Document document)
    {
      var container = GetLastBlockContainable(document);

      var table = container.LastChild as Table;

      if (table == null) {
        table = new Table();
        container.Nodes.Add(table);
      }

      return table;
    }

    private static readonly Regex regexTableColumnSpecifier
      = new Regex(@"^(?<specifier>(LEFT|CENTER|RIGHT|(BG)?COLOR\((?<color>#[0-9a-fA-F]{3}|#[0-9a-fA-F]{6}|[a-zA-Z]+)\)|SIZE\((?<size>\d+)\)):)*", RegexOptions.Singleline);

    protected virtual TableColumn ParseTableColumn(string column, TableRow row)
    {
      if (string.Equals(column, "~", StringComparison.Ordinal))
        return new TableColumn(new Node[] {}, 0, -1);
      else if (string.Equals(column, ">", StringComparison.Ordinal))
        return new TableColumn(new Node[] {}, 1, 0);

      string alignment = null;
      string color = null;
      string backgroundColor = null;
      int? size;

      var content = new StringBuilder(regexTableColumnSpecifier.Replace(column, delegate(Match match) {
        var processed = new StringBuilder();
        var lastIndex = 0;

        foreach (Capture capt in match.Groups["specifier"].Captures) {
          if (string.Equals(capt.Value, "LEFT:", StringComparison.Ordinal)) {
            alignment = "left";
          }
          else if (string.Equals(capt.Value, "CENTER:", StringComparison.Ordinal)) {
            alignment = "center";
          }
          else if (string.Equals(capt.Value, "RIGHT:", StringComparison.Ordinal)) {
            alignment = "right";
          }
          else if (capt.Value.StartsWith("COLOR", StringComparison.Ordinal)) {
            color = match.Groups["color"].Value;
          }
          else if (capt.Value.StartsWith("BGCOLOR", StringComparison.Ordinal)) {
            backgroundColor = match.Groups["color"].Value;
          }
          else if (capt.Value.StartsWith("SIZE", StringComparison.Ordinal)) {
            int s;

            if (int.TryParse(match.Groups["size"].Value, out s))
              size = s;
          }
          else {
            // not matched;
            processed.Append(match.Value, lastIndex, capt.Index - lastIndex);
            processed.Append(capt.Value);

            lastIndex = capt.Index + capt.Length;

            continue;
          }

          // matched
          lastIndex = capt.Index + capt.Length;
        }

        processed.Append(match.Value, lastIndex, match.Length - lastIndex);

        return processed.ToString();
      }));

      var isHeader = (0 < content.Length && content[0] == '~');

      if (isHeader)
        content.Remove(0, 1);

      var ret = isHeader
        ? new TableHeaderColumn(ParseInline(content.ToString()))
        : new TableColumn(ParseInline(content.ToString()));

      ret.Alignment = alignment;

      var style = new List<string>();

      if (color != null)
        style.Add(string.Format("color: {0};", color));
      if (backgroundColor != null)
        style.Add(string.Format("background-color: {0};", backgroundColor));
      if (size != null)
        style.Add(string.Format("font-size: {0}px;", size.Value));

      if (0 < style.Count)
#if NET_4_0
        ret.Style = string.Join(" ", style);
#else
        ret.Style = string.Join(" ", style.ToArray());
#endif

      return ret;
    }

    private Node ParseCsvTableBlock(string line)
    {
      throw new NotImplementedException("csv table is not implemented");
    }

    private Node ParsePluginBlock(string line, Document document)
    {
      var container = GetLastBlockContainable(document);
      var multiline = line.EndsWith("{{", StringComparison.Ordinal);
      var argsStarts = line.IndexOf('(');
      Plugin plugin;
      string multilinePluginDelimiter = null;

      if (multiline) {
        if (line.EndsWith("{{{", StringComparison.Ordinal))
          multilinePluginDelimiter = "}}}";
        else
          multilinePluginDelimiter = "}}";
      }

      if (0 < argsStarts) {
        var argsEnds = multiline ? line.LastIndexOf(')') : line.IndexOf(')', argsStarts + 1);
        var name = line.Substring(1, argsStarts - 1);
        var args = (0 <= argsEnds)
          ? line.Substring(argsStarts + 1, argsEnds - argsStarts - 1)
          : line.Substring(argsStarts);

        if (multiline)
          plugin = new MultilineBlockPlugin(name, Csv.ToSplitted(args), multilinePluginDelimiter);
        else
          plugin = new BlockPlugin(name, Csv.ToSplitted(args));
      }
      else {
        if (multiline)
          plugin = new MultilineBlockPlugin(line.Substring(1, line.Length - 1 - multilinePluginDelimiter.Length), new string[] {}, multilinePluginDelimiter);
        else
          plugin = new BlockPlugin(line.Substring(1), new string[] {});
      }

      container.Nodes.Add(plugin);

      return plugin;
    }

    private Node ParseCommentBlock(string line)
    {
      return new Comment(line.Substring(2));
    }

    protected override IEnumerable<Node> ParseInline(string line)
    {
      return ParseBrackets((IList<Node>)base.ParseInline(line)); // XXX
    }

    private delegate Node BracketParser(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch);

    private class BracketHanlder {
      public Regex OpenBracketRegex = null;
      public Regex CloseBracketRegex = null;
      public BracketParser Parser;
      public bool IsSymmetricBracket = false;
    }

    private bool bracketHandlersInitialzed = false;
    private BracketHanlder[] bracketHandlers = new[] {
      new BracketHanlder() { // annotation
        OpenBracketRegex = new Regex(@"\({2}"),
        CloseBracketRegex   = new Regex(@"\){2}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new Annotation(nodes);
        }
      },
      new BracketHanlder() { // inline plugin with text
        OpenBracketRegex = new Regex(inlinePluginRegexString + "{"),
        CloseBracketRegex   = new Regex(@"};"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new InlinePlugin(openBracketMatch.Groups["name"].Value, Csv.ToSplitted(openBracketMatch.Groups["args"].Value), nodes);
        }
      },
      new BracketHanlder() { // COLOR(){}
        OpenBracketRegex = new Regex(@"COLOR\((?<arg>#\d+)\){"),
        CloseBracketRegex   = new Regex(@"}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new InlinePlugin("color", new[] {openBracketMatch.Groups["arg"].Value}, nodes);
        }
      },
      new BracketHanlder() { // SIZE(){}
        OpenBracketRegex = new Regex(@"SIZE\((?<arg>\d+)\){"),
        CloseBracketRegex   = new Regex(@"}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new InlinePlugin("size", new[] {openBracketMatch.Groups["arg"].Value}, nodes);
        }
      },
      new BracketHanlder() { // inserted text (underline)
        OpenBracketRegex = new Regex(@"\%{3}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new InsertedText(nodes);
        }
      },
      new BracketHanlder() { // deleted text (strike through)
        OpenBracketRegex = new Regex(@"\%{2}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new DeletedText(nodes);
        }
      },
      new BracketHanlder() { // emphasis (italic)
        OpenBracketRegex = new Regex(@"\'{3}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new Emphasis(nodes);
        }
      },
      new BracketHanlder() { // strong emphasis (bold)
        OpenBracketRegex = new Regex(@"\'{2}"),
        Parser = delegate(IEnumerable<Node> nodes, Match openBracketMatch, Match closeBracketMatch) {
          return new StrongEmphasis(nodes);
        }
      },
    };

    private class BracketParsingContext {
      public BracketHanlder Handler;
      public Match OpenBracketMatch;
      public int OpenBracketNodeIndex;
      public Match CloseBracketMatch;
      public int CloseBracketNodeIndex;

      public BracketParsingContext(BracketHanlder handler, Match openBracketMatch, int openBracketNodeIndex)
      {
        this.Handler = handler;
        this.OpenBracketMatch = openBracketMatch;
        this.OpenBracketNodeIndex = openBracketNodeIndex;
      }
    }

    private IEnumerable<Node> ParseBrackets(IList<Node> nodes)
    {
      if (!bracketHandlersInitialzed) {
        foreach (var handler in bracketHandlers) {
          if (handler.CloseBracketRegex == null) {
            handler.IsSymmetricBracket = true;
            handler.CloseBracketRegex = handler.OpenBracketRegex;
          }
        }

        bracketHandlersInitialzed = true;
      }

      for (;;) {
        // search innermost bracket
        var innermost = GetInnermostBracket(nodes);

        if (innermost == null)
          return nodes; // nothing matched(no brackets)

        nodes = ParseBracket(nodes, innermost);

        // reparse
      }
    }

    private List<BracketParsingContext> GetInnerBrackets(IList<Node> nodes)
    {
      var openedBracketContexts = new List<BracketParsingContext>();

      foreach (var handler in bracketHandlers) {
        for (var i = 0; i < nodes.Count; i++) {
          var text = nodes[i] as Text;

          if (text == null)
            continue;

          if (handler.IsSymmetricBracket) {
            var firstMatched = handler.OpenBracketRegex.Match(text.Value);

            if (firstMatched.Success) {
              openedBracketContexts.Add(new BracketParsingContext(handler, firstMatched, i));
              break;
            }
          }
          else {
            // TODO: RegexOptions.RightToLeft
            var matches = handler.OpenBracketRegex.Matches(text.Value);

            if (0 < matches.Count) {
              openedBracketContexts.Add(new BracketParsingContext(handler, matches[matches.Count - 1], i));
              break;
            }
          }
        }
      }

      if (openedBracketContexts.Count == 0)
        return null; // no brackets

      var closedBracketContexts = new List<BracketParsingContext>();

      foreach (var opened in openedBracketContexts) {
        for (var i = opened.OpenBracketNodeIndex; i < nodes.Count; i++) {
          var text = nodes[i] as Text;

          if (text == null)
            continue;

          var match = (i == opened.OpenBracketNodeIndex)
            ? opened.Handler.CloseBracketRegex.Match(text.Value, opened.OpenBracketMatch.Index + opened.OpenBracketMatch.Length)
            : opened.Handler.CloseBracketRegex.Match(text.Value);

          if (match.Success) {
            opened.CloseBracketMatch = match;
            opened.CloseBracketNodeIndex = i;

            closedBracketContexts.Add(opened);

            break;
          }
        }
      }

      if (closedBracketContexts.Count == 0)
        return null; // no brackets (probably not closed)
      else
        return closedBracketContexts;
    }

    private BracketParsingContext GetInnermostBracket(IList<Node> nodes)
    {
      var bracketContexts = GetInnerBrackets(nodes);

      if (bracketContexts == null)
        return null;
      else if (bracketContexts.Count == 1)
        return bracketContexts[0];

      // find innermost (=shortest) bracket
      bracketContexts.Sort(delegate(BracketParsingContext x, BracketParsingContext y) {
        if (x == y)
          return 0;

        var openComparison = x.OpenBracketNodeIndex - y.OpenBracketNodeIndex;

        if (openComparison == 0)
          openComparison = x.OpenBracketMatch.Index - y.OpenBracketMatch.Index;

        var closeComparison = x.CloseBracketNodeIndex - y.CloseBracketNodeIndex;

        if (closeComparison == 0)
          closeComparison = (x.CloseBracketMatch.Index + x.CloseBracketMatch.Length) - (y.CloseBracketMatch.Index + y.CloseBracketMatch.Length);

        if (openComparison < 0 && 0 < closeComparison)
          return 1;
        else
          return -1;
      });

      return bracketContexts[0];
    }

    private IList<Node> ParseBracket(IList<Node> nodes, BracketParsingContext context)
    {
      var leadingNodes = new List<Node>();
      var parsingNodes = new List<Node>();
      var trailingNodes = new List<Node>();

      for (var i = 0; i < context.OpenBracketNodeIndex; i++) {
        leadingNodes.Add(nodes[i]);
      }

      var startText = nodes[context.OpenBracketNodeIndex] as Text;

      if (0 < context.OpenBracketMatch.Index)
        leadingNodes.Add(new Text(startText.Value.Substring(0, context.OpenBracketMatch.Index)));

      {
        var start = context.OpenBracketMatch.Index + context.OpenBracketMatch.Length;

        if (context.OpenBracketNodeIndex == context.CloseBracketNodeIndex)
          parsingNodes.Add(new Text(startText.Value.Substring(start, context.CloseBracketMatch.Index - start)));
        else
          parsingNodes.Add(new Text(startText.Value.Substring(start)));
      }

      for (var i = context.OpenBracketNodeIndex + 1; i < context.CloseBracketNodeIndex; i++) {
        parsingNodes.Add(nodes[i]);
      }

      var endText = nodes[context.CloseBracketNodeIndex] as Text;

      {
        var end = context.CloseBracketMatch.Index + context.CloseBracketMatch.Length;

        if (context.OpenBracketNodeIndex != context.CloseBracketNodeIndex && 0 < context.CloseBracketMatch.Index)
          parsingNodes.Add(new Text(endText.Value.Substring(0, context.CloseBracketMatch.Index)));

        if (end < endText.Value.Length)
          trailingNodes.Add(new Text(endText.Value.Substring(end)));
      }

      for (var i = context.CloseBracketNodeIndex + 1; i < nodes.Count; i++) {
        trailingNodes.Add(nodes[i]);
      }

      var ret = new List<Node>();

      ret.AddRange(leadingNodes);
      ret.Add(context.Handler.Parser(parsingNodes, context.OpenBracketMatch, context.CloseBracketMatch));
      ret.AddRange(trailingNodes);

      return ret;
    }
  }
}
