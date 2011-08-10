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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Smdn.Formats.Urn.Ietf {
  /*
   * RFC 2648 - A URN Namespace for IETF Documents
   * http://tools.ietf.org/html/rfc2648
   */
  public class IetfUrnResolver : IUrnToUrlResolver {
    public IetfUrnResolver()
      : this(false, null, TimeSpan.Zero)
    {
    }

    public IetfUrnResolver(string cacheDirectory)
      : this(true, cacheDirectory, TimeSpan.FromDays(10.0))
    {
    }

    public IetfUrnResolver(string cacheDirectory, TimeSpan cacheExpirationInterval)
      : this(true, cacheDirectory, cacheExpirationInterval)
    {
    }

    private IetfUrnResolver(bool caching, string cacheDirectory, TimeSpan cacheExpirationInterval)
    {
      if (caching) {
        if (cacheDirectory == null)
          throw new ArgumentNullException("cacheDirectory");
        if (cacheExpirationInterval < TimeSpan.Zero)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("cacheExpirationInterval", cacheExpirationInterval);
      }

      this.cacheDirectory = cacheDirectory;
      this.cacheExpirationInterval = cacheExpirationInterval;
    }

    private static string[] ParseUrn(Uri urn)
    {
      return Smdn.Urn.GetNamespaceSpecificString(urn, Smdn.Urn.NamespaceIetf).Split(':');
    }

#region "IUrnToUrlResolver"
    Uri IUrnToUrlResolver.Resolve(Uri urn)
    {
      return ResolveUrnToUrl(urn);
    }

    public Uri ResolveUrnToUrl(Uri urn)
    {
      var nssArray = ParseUrn(urn);

      switch (nssArray[0].ToLower()) {
        case "rfc": return IetfRfcDocument.GetPlainTextUrl(nssArray[1]);
        case "id":  return IetfInternetDraftDocument.GetPlainTextUrl(nssArray[1]);
        case "fyi": return IetfInformationalDocument.GetPlainTextUrl(nssArray[1]);
        case "std": return IetfStandardDocument.GetPlainTextUrl(nssArray[1]);
        case "bcp": return IetfBestCurrentPracticeDocument.GetPlainTextUrl(nssArray[1]);

        case "mtg":
        default:
          throw new NotSupportedException(string.Format("document series '{0}' is not supported", nssArray[0]));
      }
    }
#endregion

    public IetfDocument ResolveUrn(Uri urn)
    {
      var nssArray = ParseUrn(urn);

      IetfDocument document = null;

      switch (nssArray[0].ToLower()) {
        case "rfc": document = GetRfcDocument(nssArray[1]); break;
        case "id":  document = GetInternetDraftDocument(nssArray[1]); break;
        case "fyi": document = GetInformationalDocument(nssArray[1]); break;
        case "std": document = GetStandardDocument(nssArray[1]); break;
        case "bcp": document = GetBestCurrentPracticeDocument(nssArray[1]); break;

        case "mtg":
        default:
          throw new NotSupportedException(string.Format("document series '{0}' is not supported", nssArray[0]));
      }

      return document;
    }

    private IetfRfcDocument GetRfcDocument(string rfc)
    {
      if (rfcDictionary == null)
        rfcDictionary = IetfDocumentCatalog.ReadRfcCatalog(cacheDirectory, cacheExpirationInterval);

      if (rfcDictionary.ContainsKey(rfc))
        return rfcDictionary[rfc];
      else
        return null;
    }

    private IetfInternetDraftDocument GetInternetDraftDocument(string id)
    {
      if (idDictionary == null)
        idDictionary = IetfDocumentCatalog.ReadIdCatalog(cacheDirectory, cacheExpirationInterval);

      if (idDictionary.ContainsKey(id))
        return idDictionary[id];
      else
        return null;
    }

    private IetfInformationalDocument GetInformationalDocument(string fyi)
    {
      if (fyiDictionary == null)
        fyiDictionary = IetfDocumentCatalog.ReadFyiCatalog(cacheDirectory, cacheExpirationInterval);

      if (fyiDictionary.ContainsKey(fyi))
        return fyiDictionary[fyi];
      else
        return null;
    }

    private IetfStandardDocument GetStandardDocument(string std)
    {
      if (stdDictionary == null)
        stdDictionary = IetfDocumentCatalog.ReadStdCatalog(cacheDirectory, cacheExpirationInterval);

      if (stdDictionary.ContainsKey(std))
        return stdDictionary[std];
      else
        return null;
    }

    private IetfBestCurrentPracticeDocument GetBestCurrentPracticeDocument(string bcp)
    {
      if (bcpDictionary == null)
        bcpDictionary = IetfDocumentCatalog.ReadBcpCatalog(cacheDirectory, cacheExpirationInterval);

      if (bcpDictionary.ContainsKey(bcp))
        return bcpDictionary[bcp];
      else
        return null;
    }

    private string cacheDirectory;
    private TimeSpan cacheExpirationInterval;
    private Dictionary<string, IetfRfcDocument> rfcDictionary = null;
    private Dictionary<string, IetfInternetDraftDocument> idDictionary = null;
    private Dictionary<string, IetfInformationalDocument> fyiDictionary = null;
    private Dictionary<string, IetfStandardDocument> stdDictionary = null;
    private Dictionary<string, IetfBestCurrentPracticeDocument> bcpDictionary = null;
  }
}

