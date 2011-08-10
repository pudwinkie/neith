// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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

using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3.WebClients {
  /*
   * NON STANDARD EXTENSIONS
   */
  internal class ExtendedPopStyleUriParser : PopStyleUriParser {
    /// <summary>returns MSG value</summary>
    public static long GetMsg(Uri uri)
    {
      CheckUriScheme(uri);

      var segments = uri.Segments;

      if (segments.Length <= 1)
        return 0L;

      if (segments[1].StartsWith(";MSG=", StringComparison.OrdinalIgnoreCase)) {
        var msg = segments[1].Substring(5);

        if (msg.EndsWith("/", StringComparison.Ordinal))
          msg = msg.Substring(0, msg.Length - 1);

        return PopTextConverter.ToMessageNumber(msg);
      }

      return 0L;
    }

    /// <summary>returns URI form</summary>
    public static ExtendedPopUriForm GetUriForm(Uri uri)
    {
      CheckUriScheme(uri);

      var segments = uri.Segments;

      if (segments.Length <= 1)
        return ExtendedPopUriForm.Mailbox;

      if (segments[1].StartsWith(";MSG=", StringComparison.OrdinalIgnoreCase))
        return ExtendedPopUriForm.Message;
      else
        return ExtendedPopUriForm.Mailbox;
    }
  }
}
