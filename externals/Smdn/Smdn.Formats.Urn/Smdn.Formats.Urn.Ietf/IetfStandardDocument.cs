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
  public class IetfStandardDocument : IetfDocument {
    public IetfStandardDocument(string id, string title, string author, DateTime issued)
      : base(id, title, author, issued)
    {
      throw new NotImplementedException();
    }

    public override Uri GetPlainTextUrl()
    {
      return GetPlainTextUrl(Id);
    }

    public static Uri GetPlainTextUrl(string std)
    {
      if (std == null)
        throw new ArgumentNullException("std");
      else if (std.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("std");

      return new Uri(string.Format("http://www.rfc-editor.org/std/std{0}.txt", std));
    }

    public override Uri GetHtmlUrl()
    {
      return GetHtmlUrl(Id);
    }

    public static Uri GetHtmlUrl(string std)
    {
      if (std == null)
        throw new ArgumentNullException("std");
      else if (std.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("std");

      throw new NotImplementedException();
    }

    public override Uri ToUrn()
    {
      return ToUrn(Id);
    }

    public static Uri ToUrn(string std)
    {
      return new Uri(string.Format("urn:ietf:std:{0}", std));
    }
  }
}
