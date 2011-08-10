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

using Smdn.Formats.Notations;
using Smdn.Formats.Notations.Dom;
using Smdn.Formats.Notations.Hatena.Dom;

namespace Smdn.Formats.Notations.Hatena {
  public class Parser : WikiLikeNotationParserBase {
    private class TaggedPreformatted : Preformatted {
      public TaggedPreformatted()
        : base(false)
      {
      }
    }

    // regex for 'hoge:arg1:arg2'
    private class AutoLinkRegex : Regex {
      /*
       * https://www.hatena.ne.jp/register
       * アルファベットで始まり、アルファベットか数字で終わる3文字以上､15文字以内の半角英数字
       * http://www.hatena.ne.jp/help/account
       * サブアカウントのはてなIDは、アルファベットで始まり、アルファベットか数字で終わる3文字以上､32文字以内の半角英数字で入力してください。
       */
      private const string hatenaIdRegex = @"(?<hatenaid>[a-zA-Z][a-zA-Z0-9_\-]{1,30}[a-zA-Z0-9])";
      private const string groupIdRegex = @"(?<gruopid>[a-z][a-z0-9_\-]{1,30}[a-z0-9])"; // XXX
      private const string argumentsRegex = @"(\:(?<arg>[^=\:\s]+(=(https?\:)?[^\:\s]+)?))*";

      public AutoLinkRegex(string pattern, bool hasArguments)
        : this(pattern, false, hasArguments)
      {
      }

      protected AutoLinkRegex(string pattern, bool escaped, bool hasArguments)
        : base(GetPattern(pattern, escaped, hasArguments))
      {
      }

      private static string GetPattern(string pattern, bool escaped, bool hasArguments)
      {
        var sb = new System.Text.StringBuilder();

        if (escaped)
          sb.Append(@"\[");

        sb.Append("(?<text>");

        if (pattern.Contains("<$")) {
          pattern = pattern.Replace("<$hatenaid>", hatenaIdRegex);
          pattern = pattern.Replace("<$groupid>", groupIdRegex);
        }

        sb.Append(pattern);

        if (hasArguments)
          sb.Append(argumentsRegex);

        sb.Append(")");

        if (escaped)
          sb.Append(@"\]");

        return sb.ToString();
      }

      public static string GetHatenaIdFrom(Match match)
      {
        return match.Groups["hatenaid"].Value;
      }

      public static string GetGroupIdFrom(Match match)
      {
        return match.Groups["gruopid"].Value;
      }

      public static IEnumerable<Node> GetTextNodeFrom(Match match)
      {
        return new[] {new Text(match.Groups["text"].Value)};
      }

      public static string GetCombinedArgumentsFrom(Match match)
      {
        var args = new System.Text.StringBuilder();

        foreach (Capture arg in match.Groups["arg"].Captures) {
          if (args.Length != 0)
            args.Append(":");
          args.Append(arg.Value);
        }

        return args.ToString();
      }

      public static Dictionary<string, string> GetArgumentsFrom(Match match)
      {
        var args = new Dictionary<string, string>();

        foreach (Capture arg in match.Groups["arg"].Captures) {
          var a = arg.Value;
          var delim = a.IndexOf('=');

          if (0 <= delim)
            args.Add(a.Substring(0, delim), a.Substring(delim + 1));
          else
            args.Add(a, null);
        }

        return args;
      }
    }

    // regex for '[hoge:arg1:arg2]'
    private class EscapedAutoLinkRegex : AutoLinkRegex {
      public EscapedAutoLinkRegex(string pattern, bool hasArguments)
        : base(pattern, true, hasArguments)
      {
      }
    }

    protected delegate Node BlockNotationParser(string line, Match matched, Document document);

    private class BlockNotationHandler {
      public Regex Regex;
      public BlockNotationParser Parser;
    }

    private BlockNotationHandler[] blockNotationHandlers;

    private Preformatted currentPreformattedBlock = null;
    private BlockQuotation currentQuotationBlock = null;

    public Parser(Dictionary<string, string> options)
      : base(options)
    {
      blockNotationHandlers = new[] {
        new BlockNotationHandler {
          // preformatted
          Regex = new Regex(@"^\>\|(?<super>((?<type>[a-zA-Z0-9]*)\|))?$"),
          Parser = ParsePreformattedBlock,
        },
        new BlockNotationHandler {
          // tagged preformatted
          Regex = new Regex(@"^\>\<"),
          Parser = ParseTaggedPreformattedBlock,
        },
        new BlockNotationHandler {
          // quotation
          Regex = new Regex(@"^\>(?<citeuri>.+)?\>$"),
          Parser = ParseQuotationBlock,
        },
      };

#region "hatena auto link notations"
      InlineNotationHandlers.Add("hatena http and mailto notation", new InlineNotationHandler() {
        Priority = InlineNotationPriority.High,
        Regex = new EscapedAutoLinkRegex(@"(?<uri>(https?\:|mailto\:)[^\:]+)", true),
        Parser = ParseHttpAndMailtoNotation,
      });
      InlineNotationHandlers.Add("hatena niconico notation", new InlineNotationHandler() {
        Regex = new EscapedAutoLinkRegex(@"niconico\:(?<id>(sm)\d+)", true),
        // TODO: player and args
        Parser = delegate(Match match) {
          return new Anchor("http://d.hatena.ne.jp/video/niconico/" + match.Groups["id"].Value,
                            EscapedAutoLinkRegex.GetTextNodeFrom(match));
        }
      });
      InlineNotationHandlers.Add("hatena google notation", new InlineNotationHandler() {
        Regex = new EscapedAutoLinkRegex(@"google\:((?<service>(news|image))\:)?(?<query>[^\]]+)", false),
        Parser = delegate(Match match) {
          string baseUri;

          switch (match.Groups["service"].Value) {
            case "image": baseUri = "http://images.google.com/images?ie=utf-8&oe=utf-8&q="; break;
            case "news": baseUri = "http://images.google.com/news?ie=utf-8&oe=utf-8&q="; break;
            default: baseUri = "http://www.google.com/search?ie=utf-8&oe=utf-8&q="; break;
          }

          return new Anchor(baseUri + match.Groups["query"].Value,
                            EscapedAutoLinkRegex.GetTextNodeFrom(match));
        }
      });
      InlineNotationHandlers.Add("hatena map notation", new InlineNotationHandler() {
        Regex = new AutoLinkRegex(@"map\:x(?<longitude>\d+(\.\d+)?)y(?<latitude>\d+(\.\d+)?)", true),
        Parser = delegate(Match match) {
          var args = AutoLinkRegex.GetArgumentsFrom(match);
          var uri = string.Format("http://maps.google.com/maps?ll={0},{1}&z=13", match.Groups["longitude"].Value, match.Groups["latitude"].Value);

          /*
          if (args.ContainsKey("map"))
            uri = uri;
          else*/ if (args.ContainsKey("satellite"))
            uri += "&t=k";
          else if (args.ContainsKey("hybrid"))
            uri += "&t=h";

          var anchor = new Anchor(uri + "&source=embed", new[] {new Text(match.Value)});

#if false
          // XXX
          var iframe = new InlineFrame(new Uri(uri + "&output=embed"));

          iframe.Scrolling = false;
          iframe.FrameBorder = false;
          iframe.Attributes.Add("marginwidth", "0");
          iframe.Attributes.Add("marginheight", "0");

          foreach (var arg in args) {
            if (arg.Key.StartsWith("w"))
              iframe.Width = arg.Key.Substring(1);
            if (arg.Key.StartsWith("h"))
              iframe.Height = arg.Key.Substring(1);
          }

          return new Inline(new Node[] {iframe, anchor});
#else
          return anchor;
#endif
        }
      });
      InlineNotationHandlers.Add("hatena wikipedia notation", new InlineNotationHandler() {
        Regex = new EscapedAutoLinkRegex(@"wikipedia\:((?<lang>[a-z\-]{2,})\:)?(?<query>[^\]]+)", false),
        Parser = delegate(Match match) {
          var lang = match.Groups["lang"].Value;

          if (string.Empty.Equals(lang))
            lang = "ja";

          return new Anchor(string.Format("http://{0}.wikipedia.org/wiki/{1}", lang, match.Groups["query"].Value),
                            EscapedAutoLinkRegex.GetTextNodeFrom(match));
        }
      });
#endregion

#region "hatena internal link notation"
      /*
       * id, antenna, bookmark, diary, fotolife, group, haiku, idea, rss and graph notation
       */
      InlineNotationHandlers.Add("hatena service page link notation", new InlineNotationHandler() {
        Regex = new AutoLinkRegex(@"((?<service>g):<$groupid>(:id:<$hatenaid>)?|(?<service>graph|d|b|a|f|h|i|r):id:<$hatenaid>|(?<!\:)id:<$hatenaid>)", true),
        Parser = ParseHatenaServicePageLinkNotation,
      });
      InlineNotationHandlers.Add("hatena service tag page link notation", new InlineNotationHandler() {
        Regex = new EscapedAutoLinkRegex(@"(?<service>graph|b|f|i)(:id:<$hatenaid>)?:t:(?<tag>[^\]]+)", false),
        Parser = ParseHatenaServicePageLinkNotation,
      });
      InlineNotationHandlers.Add("hatena service keyword page link notation", new InlineNotationHandler() {
        Regex = new EscapedAutoLinkRegex(@"(?<service>b|h):keyword:(?<keyword>[^\]]+)", false),
        Parser = ParseHatenaServicePageLinkNotation,
      });
      /* question notation */
      /* search notation */
      /* fotolife notation */
      /* idea notation */
      /* graph notation */
      /*
       * keyword notation
       */
      InlineNotationHandlers.Add("hatena force link to keyword notation", new InlineNotationHandler() {
        Priority = InlineNotationPriority.High,
        Regex = new Regex(@"\[{2}(?<keyword>[^\]]+)\]{2}"),
        Parser = delegate(Match match) {
          return new KeywordLink(match.Groups["keyword"].Value,
                                 new[] {new Text(match.Groups["keyword"].Value)});
        }
      });
      InlineNotationHandlers.Add("hatena keyword user page notation", new InlineNotationHandler() {
        Regex = new AutoLinkRegex("k:id:<$hatenaid>", false),
        Parser = delegate(Match match) {
          return new Anchor(string.Format("http://k.hatena.ne.jp/{0}/", AutoLinkRegex.GetHatenaIdFrom(match)),
                            EscapedAutoLinkRegex.GetTextNodeFrom(match));
        }
      });
      InlineNotationHandlers.Add("hatena keyword notation", new InlineNotationHandler() {
        Priority = InlineNotationPriority.High,
        Regex = new EscapedAutoLinkRegex(@"(g:<$groupid>:|d:|(?<!\:))keyword:(?<keyword>[^\:\s]+)", true),
        Parser = delegate(Match match) {
          var keyword = match.Groups["keyword"].Value;
          var group = EscapedAutoLinkRegex.GetGroupIdFrom(match);
          var nodes = EscapedAutoLinkRegex.GetTextNodeFrom(match);

          if (group == string.Empty)
            return new KeywordLink(keyword, nodes);
          else
            return new KeywordLink(group, keyword, nodes);
        }
      });
      /* isbn/asin notation */
      /* rakuten notation */
      /* jan/ean notation */
      /* ugomemo notation */
#endregion

      InlineNotationHandlers.Add("hatena autolink cancellation notation", new InlineNotationHandler() {
        Regex = new Regex(@"\[\]"),
        Parser = delegate(Match match) { return new Inline(); }
      });
    }

    protected override bool TryParseNestableBlock(string line, Document document)
    {
      switch (line[0]) {
        default:
          return base.TryParseNestableBlock(line, document);
      }
    }

    protected override void PreParseLine(string line, Document document)
    {
      if (currentPreformattedBlock != null)
        PreParsePreformattedBlockLine(line, document);
      else if (currentQuotationBlock != null)
        PreParseQuotationBlockLine(line, document);
      else
        base.PreParseLine(line, document);
    }

    private void PreParsePreformattedBlockLine(string line, Document document)
    {
      if (currentPreformattedBlock is TaggedPreformatted && line.EndsWith("><", StringComparison.Ordinal)) {
        // end of tagged blockquote notation
        currentPreformattedBlock.Append(line.Substring(0, line.Length - 1));

        currentPreformattedBlock = null;
      }
      else if (currentPreformattedBlock.NeedEscape && string.Equals(line, "||<", StringComparison.Ordinal)) {
        // end of super-pre, aa, syntax highlight notation
        currentPreformattedBlock = null;
      }
      else if (!currentPreformattedBlock.NeedEscape && line.EndsWith("|<", StringComparison.Ordinal)) {
        // end of 'normal' pre notation
        line = line.Substring(0, line.Length - 2);

        if (currentPreformattedBlock.IsEmpty)
          currentPreformattedBlock.Append(line);
        else
          currentPreformattedBlock.Append(Environment.NewLine, line);

        currentPreformattedBlock = null;
      }
      else {
        // preformatted line
        if (currentPreformattedBlock.IsEmpty)
          currentPreformattedBlock.Append(line);
        else
          currentPreformattedBlock.Append(Environment.NewLine, line);
      }
    }

    private void PreParseQuotationBlockLine(string line, Document document)
    {
      if (string.Equals(line, "<<", StringComparison.Ordinal)) {
        // end of blockquote notation
        currentQuotationBlock = null;
      }
      else {
        // quotation line
        if (line.Length == 0) {
          currentQuotationBlock.Nodes.Add(new ForcedLineBreak());
        }
        else {
          var parsed = ParseLine(line, document);

          if (parsed != null)
            currentQuotationBlock.Nodes.Add(parsed);
        }
      }
    }

    protected override Node ParseLine(string line, Document document)
    {
      Node parsed = null;

      if (0 == line.Length) {
        parsed = ParseEmptyLine(document);
      }
      else {
        switch (line[0]) {
          case ':': {
            if (0 <= line.IndexOf(':', 1))
              parsed = ParseDefinitionListBlock(line, document);
            else
              parsed = ParseParagraphBlock(line, document);
            break;
          }

          case '|': {
            ParseTableBlock(line, document);
            return null;
          }

          case '>': {
            foreach (var handler in blockNotationHandlers) {
              var matched = handler.Regex.Match(line);

              if (matched.Success) {
                parsed = handler.Parser(line, matched, document);
                break;
              }
            }
            break;
          }

          default:
            parsed = ParseParagraphBlock(line, document);
            break;
        }
      }

      if (parsed == null)
        return base.ParseLine(line, document);
      else
        return parsed;
    }

    private Node ParseDefinitionListBlock(string line, Document document)
    {
      var list = new DefinitionList();

      line = line.Substring(1);

      // 先にインラインの記法を処理してから分割位置を判断する
      var termNodes = new List<Node>();
      var descNodes = new List<Node>();
      var container = termNodes;

      foreach (var node in ParseInline(line)) {
        if (node is Text) {
          var text = node as Text;
          var delim = text.Value.IndexOf(':');

          // ノードがTextで、termDelimiterを含む場合は、それをtermとdescriptionの
          // 分割位置と判断する
          if (0 <= delim && !(text is Comment)) {
            container.Add(new Text(text.Value.Substring(0, delim)));
            container = descNodes;
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

      return list;
    }

    private void ParseTableBlock(string line, Document document)
    {
      line = line.Substring(1);

      // table
      var table = document.LastChild as Table;

      if (table == null) {
        table = new Table();
        document.Nodes.Add(table);
      }

      // row
      if (line.EndsWith("|", StringComparison.Ordinal))
        line = line.Substring(0, line.Length - 1);

      var row = new TableRow();

      table.Nodes.Add(row);

      // columns
      foreach (var column in line.Split(new char[] {'|'})) {
        if (column.StartsWith("*", StringComparison.Ordinal))
          row.Nodes.Add(new TableHeaderColumn(ParseInline(column.Substring(1))));
        else
          row.Nodes.Add(new TableColumn(ParseInline(column)));
      }
    }

    private Node ParsePreformattedBlock(string line, Match matchPreformatted, Document document)
    {
      Preformatted pre = null;

      if (matchPreformatted.Groups["super"].Success) {
        var fileType = matchPreformatted.Groups["type"].Value;

        if (fileType.Length == 0) {
          // super-pre notation
          pre = new Preformatted(true);
        }
        else {
          if (string.Equals(fileType, "aa", StringComparison.Ordinal))
            pre = new AsciiArt();
          else
            pre = new SyntaxHighlightedCode(fileType);
        }
      }
      else {
        // 'normal' pre notation
        pre = new Preformatted(false);
      }

      currentPreformattedBlock = pre;

      return pre;
    }

    private Node ParseTaggedPreformattedBlock(string line, Match matchPreformatted, Document document)
    {
      var pre = new TaggedPreformatted();

      pre.Append(line.Substring(1));

      currentPreformattedBlock = pre;

      return pre;
    }

    private Node ParseQuotationBlock(string line, Match matchQuotation, Document document)
    {
      var quotation = new BlockQuotation();

      if (matchQuotation.Groups["citeuri"].Success)
        quotation.Cite = matchQuotation.Groups["citeuri"].Value;

      currentQuotationBlock = quotation;

      return quotation;
    }

    private Node ParseParagraphBlock(string line, Document document)
    {
      return line.StartsWith(" ", StringComparison.Ordinal)
        ? new Paragraph(ParseInline(line.Substring(1)))
        : new Paragraph(ParseInline(line));;
    }

    private Node ParseEmptyLine(Document document)
    {
      if (document.LastChild is ForcedLineBreak || document.LastChild is EmptyLine)
        return new ForcedLineBreak();
      else
        return new EmptyLine();
    }

    private Node ParseHttpAndMailtoNotation(Match match)
    {
      const string notitle = "\nnotitle";

      try {
        var uri = match.Groups["uri"].Value;
        var args = EscapedAutoLinkRegex.GetArgumentsFrom(match);

        var title = args.ContainsKey("title") ? (args["title"] ?? notitle) : null;
        var image = args.ContainsKey("image") ? (args["image"] ?? uri) : null;
        var barcode = args.ContainsKey("barcode");
        Anchor bookmark = null;

        if (args.ContainsKey("bookmark"))
          bookmark = new Anchor("http://b.hatena.ne.jp/entry/" + uri,
                                new Node[] {new Image("http://b.hatena.ne.jp/entry/image/" + uri, uri)});

        if (image != null) {
          if (title == null || title == notitle)
            title = uri;

          var img = new Image(image, title, title);

          foreach (var key in args.Keys) {
            if (key.StartsWith("w", StringComparison.Ordinal))
              img.Width = key.Substring(1);
            else if (key.StartsWith("h", StringComparison.Ordinal))
              img.Width = key.Substring(1);
          }

          return new Anchor(uri, new[] {img});
        }
        else if (barcode) {
          // http://code.google.com/intl/ja/apis/chart/#qrcodes
          var img = new Image("http://chart.apis.google.com/chart?cht=qr&choe=UTF-8&chs=100x100&chl=" + uri, uri);

          return new Anchor(uri, new[] {img});
        }
        else {
          if (title == null || title == notitle)
            title = uri;

          var anchor = new Anchor(uri, new Node[] {new Text(title)});

          if (bookmark == null)
            return anchor;
          else
            return new Inline(new Node[] {anchor, bookmark});
        }
      }
      catch (UriFormatException) {
        Console.Error.WriteLine("parser error: {0}", match.Value);
        return new Text(match.Value);
      }
    }

    private Node ParseHatenaServicePageLinkNotation(Match match)
    {
      var service = match.Groups["service"].Value;
      var group = AutoLinkRegex.GetGroupIdFrom(match);
      var id = AutoLinkRegex.GetHatenaIdFrom(match);
      var tag = match.Groups["tag"].Value;
      var keyword = match.Groups["keyword"].Value;
      var args = AutoLinkRegex.GetCombinedArgumentsFrom(match);

      string directory;
      IEnumerable<Node> nodes;

      if ((service == string.Empty || service == "d") && (args == "image" || args == "detail")) {
        var imageUri = string.Format("http://www.hatena.ne.jp/users/{0}/{1}/", id.Substring(0, 2), id);
        var idText = string.Format("id:{0}", id);

        if (args == "image")
          nodes = new Node[] {
            new Image(imageUri + "profile.gif", idText, idText),
          };
        else
          nodes = new Node[] {
            new Image(imageUri + "profile_s.gif", idText, idText),
            new Text(idText),
          };

        directory = string.Empty;
      }
      else {
        if (service == "b") {
          if (!string.IsNullOrEmpty(tag)) {
            if (string.IsNullOrEmpty(id))
              directory = "t/" + tag;
            else
              directory = tag;
          }
          else if (!string.IsNullOrEmpty(keyword)) {
            directory = "keyword/" + keyword;
          }
          else {
            directory = string.Empty;
          }
        }
        else {
          if (!string.IsNullOrEmpty(tag))
            directory = "t/" + tag;
          else if (!string.IsNullOrEmpty(keyword))
            directory = "keyword/" + keyword;
          else
            directory = args.Replace(":", "/");
        }

        nodes = AutoLinkRegex.GetTextNodeFrom(match);
      }

      if (string.IsNullOrEmpty(group))
        return new ServicePageLink(service, string.Empty, id, directory, nodes);
      else
        return new ServicePageLink(group, id, directory, nodes);
    }
  }
}