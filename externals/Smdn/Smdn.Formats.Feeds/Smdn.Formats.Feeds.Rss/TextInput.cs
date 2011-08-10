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

namespace Smdn.Formats.Feeds.Rss {
  // <textInput> sub-element of <channel>
  // A channel may optionally contain a <textInput> sub-element, which contains four required sub-elements.
  public class TextInput {
    // <title> -- The label of the Submit button in the text input area.
    public string Title {
      get; set;
    }

    // <description> -- Explains the text input area.
    public string Description {
      get; set;
    }

    // <name> -- The name of the text object in the text input area.
    public string Name {
      get; set;
    }

    // <link> -- The URL of the CGI script that processes text input requests.
    public Uri Link {
      get; set;
    }

    public TextInput()
      : this(null, null, null, null)
    {
    }

    public TextInput(string title, string description, string name, Uri link)
    {
      this.Title = title;
      this.Description = description;
      this.Name = name;
      this.Link = link;
    }
  }
}
