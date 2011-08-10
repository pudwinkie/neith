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

namespace Smdn.Formats.Urn.Ietf {
  public class IetfRfcDocument : IetfDocument {
    public Uri[] Obsoletes {
      get; private set;
    }

    public Uri[] ObsoletedBy {
      get; private set;
    }

    public Uri[] Updates {
      get; private set;
    }

    public Uri[] UpdatedBy {
      get; private set;
    }

    public Uri[] Also {
      get; private set;
    }

    public IetfRfcDocumentStatus Status {
      get; private set;
    }

    public string Format {
      get; private set;
    }

    internal IetfRfcDocument(string id, string title, string author, DateTime issued, Uri[] obsoletes, Uri[] obsoletedBy, Uri[] updates, Uri[] updatedBy, Uri[] also, IetfRfcDocumentStatus status, string format) 
      : base(id, title, author, issued)
    {
      this.Obsoletes = obsoletes;
      this.ObsoletedBy = obsoletedBy;
      this.Updates = updates;
      this.UpdatedBy = updatedBy;
      this.Also = also;
      this.Status = status;
      this.Format = format;
    }

    public override Uri GetPlainTextUrl()
    {
      return GetPlainTextUrl(Id);
    }

    public static Uri GetPlainTextUrl(string rfc)
    {
      if (rfc == null)
        throw new ArgumentNullException("rfc");
      else if (rfc.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("rfc");

      return new Uri(string.Format("http://tools.ietf.org/rfc/rfc{0}.txt", rfc));
    }

    public override Uri GetHtmlUrl()
    {
      return GetHtmlUrl(Id);
    }

    public static Uri GetHtmlUrl(string rfc)
    {
      if (rfc == null)
        throw new ArgumentNullException("rfc");
      else if (rfc.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("rfc");

      return new Uri(string.Format("http://tools.ietf.org/html/rfc{0}", rfc));
    }

    public override Uri ToUrn()
    {
      return ToUrn(Id);
    }

    public static Uri ToUrn(string rfc)
    {
      return new Uri(string.Format("urn:ietf:rfc:{0}", rfc));
    }
  }
}
