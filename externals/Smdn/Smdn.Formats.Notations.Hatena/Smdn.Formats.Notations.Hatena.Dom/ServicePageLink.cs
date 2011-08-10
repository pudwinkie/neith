// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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

using Smdn.Formats.Notations.Dom;

namespace Smdn.Formats.Notations.Hatena.Dom {
  public class ServicePageLink : Anchor {
    public string UserId {
      get; private set;
    }

    public string GroupId {
      get; private set;
    }

    public string ServicePrefix {
      get; private set;
    }

    public ServicePageLink(string userId, string directory, IEnumerable<Node> nodes)
      : this("d", null, userId, directory, nodes)
    {
    }

    public ServicePageLink(string groupId, string userId, string directory, IEnumerable<Node> nodes)
      : this("g", groupId, userId, directory, nodes)
    {
    }

    public ServicePageLink(string servicePrefix, string groupId, string userId, string directory, IEnumerable<Node> nodes)
      : base(GetUri(servicePrefix, groupId, userId, directory), nodes)
    {
      this.ServicePrefix = servicePrefix;
      this.GroupId = groupId;
      this.UserId = userId;
    }

    private static string GetUri(string servicePrefix, string groupId, string userId, string directory)
    {
      if (string.IsNullOrEmpty(servicePrefix))
        servicePrefix = "d";

      var isGroup = (servicePrefix == "g");

      if (isGroup) {
        if (string.IsNullOrEmpty(groupId))
          throw new ArgumentNullException("group"); // XXX: is empty
      }
      else {
        if (string.IsNullOrEmpty(directory) && string.IsNullOrEmpty(userId))
          throw new ArgumentException("directory or id must have value");
      }

      if (servicePrefix == "f") {
        if (directory.EndsWith("j", StringComparison.Ordinal)) // jpg
          directory = directory.Substring(0, directory.Length - 1);
        if (directory.EndsWith("p", StringComparison.Ordinal)) // png
          directory = directory.Substring(0, directory.Length - 1);
        if (directory.EndsWith("g", StringComparison.Ordinal)) // gif
          directory = directory.Substring(0, directory.Length - 1);
      }

      if (isGroup) {
        if (string.IsNullOrEmpty(userId))
          return string.Format("http://{0}.g.hatena.ne.jp/{1}", groupId, directory);
        else
          return string.Format("http://{0}.g.hatena.ne.jp/{1}/{2}", groupId, userId, directory);
      }
      else {
        return string.Format("http://{0}.hatena.ne.jp/{1}/{2}", servicePrefix, userId, directory);
      }
    }
  }
}
