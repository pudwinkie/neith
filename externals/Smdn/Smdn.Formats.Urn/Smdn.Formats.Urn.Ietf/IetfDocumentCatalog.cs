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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Smdn.IO;

namespace Smdn.Formats.Urn.Ietf {
  public static class IetfDocumentCatalog {
    public static readonly Uri RfcCatalogUrl = new Uri("http://www.ietf.org/download/rfc-index.txt");
    public static readonly Uri InternetDraftCatalogUrl = new Uri("http://www.ietf.org/download/id-abstract.txt");

    public static Dictionary<string, IetfRfcDocument> ReadRfcCatalog()
    {
      return ReadRfcCatalogCore(null /* no caching */, TimeSpan.Zero);
    }

    public static Dictionary<string, IetfRfcDocument> ReadRfcCatalog(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      if (cacheDirectory == null)
        throw new ArgumentNullException("cacheDirectory");

      return ReadRfcCatalogCore(cacheDirectory, cacheExpirationInterval);
    }

    public static Dictionary<string, IetfInternetDraftDocument> ReadIdCatalog()
    {
      return ReadIdCatalogCore(null /* no caching */, TimeSpan.Zero);
    }

    public static Dictionary<string, IetfInternetDraftDocument> ReadIdCatalog(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      if (cacheDirectory == null)
        throw new ArgumentNullException("cacheDirectory");

      return ReadIdCatalogCore(cacheDirectory, cacheExpirationInterval);
    }

    public static Dictionary<string, IetfInformationalDocument> ReadFyiCatalog()
    {
      return ReadFyiCatalogCore(null /* no caching */, TimeSpan.Zero);
    }

    public static Dictionary<string, IetfInformationalDocument> ReadFyiCatalog(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      if (cacheDirectory == null)
        throw new ArgumentNullException("cacheDirectory");

      return ReadFyiCatalogCore(cacheDirectory, cacheExpirationInterval);
    }

    public static Dictionary<string, IetfStandardDocument> ReadStdCatalog()
    {
      return ReadStdCatalogCore(null /* no caching */, TimeSpan.Zero);
    }

    public static Dictionary<string, IetfStandardDocument> ReadStdCatalog(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      if (cacheDirectory == null)
        throw new ArgumentNullException("cacheDirectory");

      return ReadStdCatalogCore(cacheDirectory, cacheExpirationInterval);
    }

    public static Dictionary<string, IetfBestCurrentPracticeDocument> ReadBcpCatalog()
    {
      return ReadBcpCatalogCore(null /* no caching */, TimeSpan.Zero);
    }

    public static Dictionary<string, IetfBestCurrentPracticeDocument> ReadBcpCatalog(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      if (cacheDirectory == null)
        throw new ArgumentNullException("cacheDirectory");

      return ReadBcpCatalogCore(cacheDirectory, cacheExpirationInterval);
    }

    private static Uri ConvertPhraseToUrn(string str)
    {
      str = str.Trim();

      var series = str.Substring(0, 3);
      var number = str.Substring(3);

      switch (series) {
        case "RFC": return IetfRfcDocument.ToUrn(number);
        case "FYI": return IetfInformationalDocument.ToUrn(number);
        case "STD": return IetfStandardDocument.ToUrn(number);
        case "BCP": return IetfBestCurrentPracticeDocument.ToUrn(number);

        default:
          Console.Error.WriteLine("unknown document series: {0}", str);
          return new Uri(string.Format("urn:ietf:{0}", str));
      }
    }

    private static Dictionary<string, IetfRfcDocument> ReadRfcCatalogCore(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      const string catalogSubRegex = @"(?<title>[^\.]+)\.\s+(?<author>[^\)]+)\.\s+(?<issued>[A-Za-z]{3,9}\s+\d{4})\.(\s+\((?<attributes>[^\)]+)\))*";

      var catalog = ReadCatalog(RfcCatalogUrl, cacheDirectory, cacheExpirationInterval, null);
      var ret = new Dictionary<string, IetfRfcDocument>(StringComparer.Ordinal);
      var catalogRegex = new Regex(string.Format(@"^0*(?<rfc>\d{{1,4}}) ((?<title>Not Issued\.)|{0})", catalogSubRegex), RegexOptions.Multiline);
      var whitespaceRegex = new Regex(@"\s{2,}");
      var statusMap = new Dictionary<string, IetfRfcDocumentStatus>(StringComparer.Ordinal) {
        {"STANDARD",               IetfRfcDocumentStatus.Standard},
        {"PROPOSED STANDARD",      IetfRfcDocumentStatus.ProposedStandard},
        {"DRAFT STANDARD",         IetfRfcDocumentStatus.DraftStandard},
        {"BEST CURRENT PRACTICE",  IetfRfcDocumentStatus.BestCurrentPractice},
        {"EXPERIMENTAL",           IetfRfcDocumentStatus.Experimental},
        {"INFORMATIONAL",          IetfRfcDocumentStatus.Informational},
        {"HISTORIC",               IetfRfcDocumentStatus.Historic},
        {"UNKNOWN",                IetfRfcDocumentStatus.Unknown},
      };

      foreach (Match matched in catalogRegex.Matches(catalog)) {
        Uri[] obsoletes = null;
        Uri[] obsoletedBy = null;
        Uri[] updates = null;
        Uri[] updatedBy = null;
        Uri[] also = null;
        string format = null;
        string status = null;

        if (string.Equals(matched.Groups["title"].Value, "Not Issued.", StringComparison.Ordinal))
          continue;

        foreach (Capture captured in matched.Groups["attributes"].Captures) {
          var attr = whitespaceRegex.Replace(captured.Value, " ");

               if (attr.StartsWith("Obsoletes ", StringComparison.Ordinal))    obsoletes   = Array.ConvertAll(attr.Substring(10).Split(','), (Converter<string, Uri>)ConvertPhraseToUrn);
          else if (attr.StartsWith("Obsoleted by ", StringComparison.Ordinal)) obsoletedBy = Array.ConvertAll(attr.Substring(13).Split(','), (Converter<string, Uri>)ConvertPhraseToUrn);
          else if (attr.StartsWith("Updates ", StringComparison.Ordinal))      updates     = Array.ConvertAll(attr.Substring( 8).Split(','), (Converter<string, Uri>)ConvertPhraseToUrn);
          else if (attr.StartsWith("Updated by ", StringComparison.Ordinal))   updatedBy   = Array.ConvertAll(attr.Substring(11).Split(','), (Converter<string, Uri>)ConvertPhraseToUrn);
          else if (attr.StartsWith("Also ", StringComparison.Ordinal))         also        = Array.ConvertAll(attr.Substring( 5).Split(','), (Converter<string, Uri>)ConvertPhraseToUrn);
          else if (attr.StartsWith("Status: ", StringComparison.Ordinal)) status = attr.Substring(8).Trim();
          else if (attr.StartsWith("Format: ", StringComparison.Ordinal)) format = attr.Substring(8).Trim();
          else
            Console.Error.WriteLine("unknown attr {0}", attr);
        }

        ret.Add(matched.Groups["rfc"].Value,
                new IetfRfcDocument(matched.Groups["rfc"].Value,
                                    whitespaceRegex.Replace(matched.Groups["title"].Value, " "),
                                    whitespaceRegex.Replace(matched.Groups["author"].Value, " "),
                                    DateTime.ParseExact(whitespaceRegex.Replace(matched.Groups["issued"].Value, " "), "MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                                    obsoletes ?? new Uri[0],
                                    obsoletedBy ?? new Uri[0],
                                    updates ?? new Uri[0],
                                    updatedBy ?? new Uri[0],
                                    also ?? new Uri[0],
                                    statusMap[status],
                                    format));
      }

      return ret;
    }

    private static Dictionary<string, IetfInternetDraftDocument> ReadIdCatalogCore(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      var catalog = ReadCatalog(InternetDraftCatalogUrl, cacheDirectory, cacheExpirationInterval, null);
      var ret = new Dictionary<string, IetfInternetDraftDocument>(StringComparer.Ordinal);
      var workingGroupRegex = new Regex(@"^(?<wg>.+)\s\-{16,}", RegexOptions.Multiline);
      //var catalogRegex = new Regex(@"^  ""(?<title>[^""]+)"",(\s+(?<author>[^,]+),)+?\s+(?<issued>\d{1,2}-\w{3}-\d{2}),\s+\<(?<index>[^\>]+)\>", RegexOptions.Multiline);
      var catalogRegex = new Regex(@"^  ""(?<title>[^""]+)"",\s+(?<author>[^\<]+?),\s+(?<issued>\d{1,2}-\w{3}-\d{2}),\s+\<(?<index>draft-(?<id>[^\.]+)\.txt)\>", RegexOptions.Multiline);
      var whitespaceRegex = new Regex(@"\s{2,}");

      var lastMatchedWorkingGroup = workingGroupRegex.Match(catalog);

      if (!lastMatchedWorkingGroup.Success)
        return ret;

      for (;;) {
        var workingGroup = lastMatchedWorkingGroup.Groups["wg"].Value;
        var matchedWorkingGroup = lastMatchedWorkingGroup.NextMatch();
        var workingGroupEnd = matchedWorkingGroup.Success ? matchedWorkingGroup.Index : catalog.Length;

        foreach (Match matched in catalogRegex.Matches(catalog, lastMatchedWorkingGroup.Index + lastMatchedWorkingGroup.Length)) {
          if (workingGroupEnd <= matched.Index)
            break;

          /*
          var authors = new List<string>();

          foreach (Capture captured in matched.Groups["author"].Captures) {
            authors.Add(whitespaceRegex.Replace(captured.Value, " "));
          }
          */

          ret.Add(matched.Groups["id"].Value,
                  new IetfInternetDraftDocument(matched.Groups["id"].Value,
                                                workingGroup,
                                                whitespaceRegex.Replace(matched.Groups["title"].Value, " "),
                                                whitespaceRegex.Replace(matched.Groups["author"].Value, " "),
                                                DateTime.ParseExact(matched.Groups["issued"].Value, "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture),
                                                matched.Groups["index"].Value));
        }

        if (matchedWorkingGroup.Success)
          lastMatchedWorkingGroup = matchedWorkingGroup;
        else
          break;
      }

      return ret;
    }

    private static string ReadCatalog(Uri url, string cacheDirectory, TimeSpan cacheExpirationInterval, Encoding encoding)
    {
      if (cacheDirectory == null) {
        using (var client = new WebClient()) {
          using (var stream = new MemoryStream(client.DownloadData(url))) {
            if (encoding == null)
              return (new StreamReader(stream)).ReadToEnd();
            else
              return (new StreamReader(stream, encoding)).ReadToEnd();
          }
        }
      }
      else {
        var segments = url.Segments;
        var cacheFile = Path.Combine(cacheDirectory, segments[segments.Length - 1]);

        if (encoding == null)
          return CachedWebFile.ReadAllText(url, cacheFile, cacheExpirationInterval);
        else
          return CachedWebFile.ReadAllText(url, cacheFile, cacheExpirationInterval, encoding);
      }
    }

    private static Dictionary<string, IetfInformationalDocument> ReadFyiCatalogCore(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      throw new NotImplementedException();
    }

    private static Dictionary<string, IetfStandardDocument> ReadStdCatalogCore(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      throw new NotImplementedException();
    }

    private static Dictionary<string, IetfBestCurrentPracticeDocument> ReadBcpCatalogCore(string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      throw new NotImplementedException();
    }
  }
}
