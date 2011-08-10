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

namespace Smdn.Net.Pop3.WebClients {
  public static class PopWebRequestMethods {
    // PopWebRequest methods
    public const string NoOp  = "NOOP";

    // PopMailboxWebRequest methods
    public const string List  = "LIST";
    public const string Uidl  = "UIDL";
    public const string Rset  = "RSET";
    public const string Stat  = "STAT";

    private static string[] mailboxMethods = new string[] {
      // must be in sorted order
      List,
      NoOp,
      Rset,
      Stat,
      Uidl,
    };

    // PopMessageWebRequest methods
    public const string Retr  = "RETR";
    public const string Top   = "TOP";
    public const string Dele  = "DELE";

    private static string[] messageMethods = new string[] {
      // must be in sorted order
      Dele,
      List,
      NoOp,
      Retr,
      Top,
      Uidl,
    };

    internal static bool IsSupportedMethod(PopWebRequest request, string method)
    {
      if (request is PopMailboxWebRequest)
        return 0 <= Array.BinarySearch(mailboxMethods, method);
      else if (request is PopMessageWebRequest)
        return 0 <= Array.BinarySearch(messageMethods, method);
      else
        return false;
    }
  }
}
