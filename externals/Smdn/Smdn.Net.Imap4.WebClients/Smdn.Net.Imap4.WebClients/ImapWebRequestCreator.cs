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
using System.Net;

namespace Smdn.Net.Imap4.WebClients {
  public class ImapWebRequestCreator : IWebRequestCreate {
    public static bool RegisterPrefix()
    {
      var inst = new ImapWebRequestCreator();

      return WebRequest.RegisterPrefix(ImapUri.UriSchemeImap, inst) &&
             WebRequest.RegisterPrefix(ImapUri.UriSchemeImaps, inst);

      // TODO: this will broke in windows
      // ImapUri.RegisterParser();
    }

    internal ImapWebRequestCreator()
    {
    }

    WebRequest IWebRequestCreate.Create(Uri uri)
    {
      switch (ImapStyleUriParser.GetUriForm(uri)) {
        case ImapUriForm.Server:          return new ImapServerWebRequest(uri, ImapSessionManager.Instance);
        case ImapUriForm.ListMessages:    return new ImapMailboxWebRequest(uri, ImapSessionManager.Instance);
        case ImapUriForm.FetchMessage:    return new ImapFetchMessageWebRequest(uri, ImapSessionManager.Instance);
        case ImapUriForm.SearchMessages:  return new ImapSearchMessageWebRequest(uri, ImapSessionManager.Instance);
        default: throw new ArgumentException("invalid URI form", "uri");
      }
    }
  }
}
