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
using System.Collections.Generic;

namespace Smdn.Net.Imap4 {
  public sealed class ImapNamespace : ICloneable {
    public ImapNamespaceDesc[] PersonalNamespaces {
      get { return personalNamespaces; }
    }

    public ImapNamespaceDesc[] OtherUsersNamespaces {
      get { return otherUsersNamespaces; }
    }

    public ImapNamespaceDesc[] SharedNamespaces {
      get { return sharedNamespaces; }
    }

    public ImapNamespace()
      : this(new ImapNamespaceDesc[] {}, new ImapNamespaceDesc[] {}, new ImapNamespaceDesc[] {})
    {
    }

    public ImapNamespace(ImapNamespaceDesc[] personalNamespaces, ImapNamespaceDesc[] otherUsersNamespaces, ImapNamespaceDesc[] sharedNamespaces)
    {
      if (personalNamespaces == null)
        throw new ArgumentNullException("personalNamespaces");
      if (otherUsersNamespaces == null)
        throw new ArgumentNullException("otherUsersNamespaces");
      if (sharedNamespaces == null)
        throw new ArgumentNullException("sharedNamespaces");

      this.personalNamespaces = personalNamespaces;
      this.otherUsersNamespaces = otherUsersNamespaces;
      this.sharedNamespaces = sharedNamespaces;
    }

    object ICloneable.Clone()
    {
      return this.Clone();
    }

    public ImapNamespace Clone()
    {
      return new ImapNamespace(Array.ConvertAll<ImapNamespaceDesc, ImapNamespaceDesc>(personalNamespaces, CloneNamespaceDesc),
                               Array.ConvertAll<ImapNamespaceDesc, ImapNamespaceDesc>(otherUsersNamespaces, CloneNamespaceDesc),
                               Array.ConvertAll<ImapNamespaceDesc, ImapNamespaceDesc>(sharedNamespaces, CloneNamespaceDesc));
    }

    private static ImapNamespaceDesc CloneNamespaceDesc(ImapNamespaceDesc desc)
    {
      return desc.Clone();
    }

    public override string ToString()
    {
      return string.Format("{{PersonalNamespaces={0}, OtherUsersNamespaces={1}, SharedNamespaces={2}}}",
                           string.Join(", ", Array.ConvertAll(personalNamespaces,   delegate(ImapNamespaceDesc ns) { return ns.ToString(); })),
                           string.Join(", ", Array.ConvertAll(otherUsersNamespaces, delegate(ImapNamespaceDesc ns) { return ns.ToString(); })),
                           string.Join(", ", Array.ConvertAll(sharedNamespaces,     delegate(ImapNamespaceDesc ns) { return ns.ToString(); })));
    }

    private /*readonly*/ ImapNamespaceDesc[] personalNamespaces;
    private /*readonly*/ ImapNamespaceDesc[] otherUsersNamespaces;
    private /*readonly*/ ImapNamespaceDesc[] sharedNamespaces;
  }
}
