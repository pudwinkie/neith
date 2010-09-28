// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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

namespace Smdn.Net.Imap4.WebClients {
  public static class ImapWebRequestMethods {
    // ImapWebRequest methods
    public const string NoOp  = "NOOP";

    // ImapServerWebRequest methods
    public const string Lsub  = "LSUB";
    public const string List  = "LIST";
    public const string XList = "XLIST";

    private static string[] serverMethods = new string[] {
      // must be in sorted order
      List,
      Lsub,
      NoOp,
      XList,
    };

    // ImapMailboxWebRequest methods
    public const string Fetch       = "FETCH";
    public const string Append      = "APPEND";
    public const string Check       = "CHECK";
    public const string Create      = "CREATE";
    public const string Delete      = "DELETE";
    public const string Expunge     = "EXPUNGE";
    public const string Select      = "SELECT";
    public const string Examine     = "EXAMINE";
    public const string Rename      = "RENAME";
    public const string Status      = "STATUS";
    public const string Subscribe   = "SUBSCRIBE";
    public const string Unsubscribe = "UNSUBSCRIBE";

    private static string[] mailboxMethods = new string[] {
      // must be in sorted order
      Append,
      Check,
      Create,
      Delete,
      Examine,
      Expunge,
      Fetch,
      NoOp,
      Rename,
      Select,
      Status,
      Subscribe,
      Unsubscribe,
    };

    // ImapFetchMessageWebRequest methods
    public const string Store   = "STORE";
    public const string Copy    = "COPY";

    private static string[] fetchMessageMethods = new string[] {
      // must be in sorted order
      Copy,
      Expunge,
      Fetch,
      NoOp,
      Store,
    };

    // ImapSearchMessageWebRequest methods
    public const string Search  = "SEARCH";
    public const string Thread  = "THREAD";
    public const string Sort    = "SORT";

    private static string[] searchMessageMethods = new string[] {
      // must be in sorted order
      Copy,
      Expunge,
      NoOp,
      Search,
      Sort,
      Store,
      Thread,
    };

    internal static bool IsSupportedMethod(ImapWebRequest request, string method)
    {
      if (request is ImapServerWebRequest)
        return 0 <= Array.BinarySearch(serverMethods, method);
      else if (request is ImapMailboxWebRequest)
        return 0 <= Array.BinarySearch(mailboxMethods, method);
      else if (request is ImapFetchMessageWebRequest)
        return 0 <= Array.BinarySearch(fetchMessageMethods, method);
      else if (request is ImapSearchMessageWebRequest)
        return 0 <= Array.BinarySearch(searchMessageMethods, method);
      else
        return false;
    }
  }
}
