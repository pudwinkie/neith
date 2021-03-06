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
  public class IetfBestCurrentPracticeDocument : IetfDocument {
    public IetfBestCurrentPracticeDocument(string id, string title, string author, DateTime issued)
      : base(id, title, author, issued)
    {
      throw new NotImplementedException();
    }

    public override Uri GetPlainTextUrl()
    {
      return GetPlainTextUrl(Id);
    }

    public static Uri GetPlainTextUrl(string bcp)
    {
      if (bcp == null)
        throw new ArgumentNullException("bcp");
      else if (bcp.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("bcp");

      return new Uri(string.Format("http://www.rfc-editor.org/bcp/bcp{0}.txt", bcp));
    }

    public override Uri GetHtmlUrl()
    {
      return GetHtmlUrl(Id);
    }

    public static Uri GetHtmlUrl(string bcp)
    {
      if (bcp == null)
        throw new ArgumentNullException("bcp");
      else if (bcp.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("bcp");

      throw new NotImplementedException();
    }

    public override Uri ToUrn()
    {
      return ToUrn(Id);
    }

    public static Uri ToUrn(string bcp)
    {
      return new Uri(string.Format("urn:ietf:bcp:{0}", bcp));
    }
  }
}
