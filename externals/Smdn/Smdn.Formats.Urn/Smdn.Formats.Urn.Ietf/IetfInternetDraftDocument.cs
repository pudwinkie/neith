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
  public class IetfInternetDraftDocument : IetfDocument {
    public string WorkingGroup {
      get; private set;
    }

    public string Index {
      get; private set;
    }

    internal IetfInternetDraftDocument(string id, string workingGroup, string title, string author, DateTime issued, string index)
      : base(id, title, author, issued)
    {
      this.WorkingGroup = workingGroup;
      this.Index = index;
    }

    public override Uri GetPlainTextUrl()
    {
      return GetPlainTextUrl(Id);
    }

    public static Uri GetPlainTextUrl(string id)
    {
      if (id == null)
        throw new ArgumentNullException("id");
      else if (id.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("id");

      return new Uri(string.Format("http://tools.ietf.org/id/draft-{0}.txt", id));
    }

    public override Uri GetHtmlUrl()
    {
      return GetHtmlUrl(Id);
    }

    public static Uri GetHtmlUrl(string id)
    {
      if (id == null)
        throw new ArgumentNullException("id");
      else if (id.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("id");

      return new Uri(string.Format("http://tools.ietf.org/html/draft-{0}", id));
    }

    public override Uri ToUrn()
    {
      return ToUrn(Id);
    }

    public static Uri ToUrn(string id)
    {
      return new Uri(string.Format("urn:ietf:id:{0}", id));
    }
  }
}
