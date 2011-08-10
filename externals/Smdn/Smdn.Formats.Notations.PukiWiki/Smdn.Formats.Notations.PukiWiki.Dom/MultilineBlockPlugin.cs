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
using System.Text;

using Smdn.Formats.Notations.Dom;

namespace Smdn.Formats.Notations.PukiWiki.Dom {
  public class MultilineBlockPlugin : BlockPlugin {
    public bool IsLastArgumentEmpty {
      get { return lastArgument.Length == 0; }
    }

    public string LastArgument {
      get { return lastArgument.ToString(); }
    }

    internal string Delimiter {
      get { return delimiter; }
    }

    public MultilineBlockPlugin()
      : base()
    {
      this.delimiter = defaultDelimiter;
    }

    public MultilineBlockPlugin(string name, string[] arguments)
      : this(name, arguments, defaultDelimiter)
    {
    }

    internal MultilineBlockPlugin(string name, string[] arguments, string delimiter)
      : base(name, arguments)
    {
      this.delimiter = delimiter;
    }

    public void Append(params string[] texts)
    {
      foreach (var text in texts) {
        lastArgument.Append(text);
      }
    }

    private const string defaultDelimiter = "}}";

    private StringBuilder lastArgument = new StringBuilder();
    private string delimiter;
  }
}