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

namespace Smdn.Formats.Notations.Dom {
  public class InlineFrame : Node {
    public Uri SourceUri {
      get; set;
    }

    // TODO: more strict
    public string Width {
      get; set;
    }

    // TODO: more strict
    public string Height {
      get; set;
    }

    // TODO: scrolling="auto"
    public bool Scrolling {
      get; set;
    }

    public bool FrameBorder {
      get; set;
    }

    public Dictionary<string, string> Attributes {
      get { return attributes; }
    }

    public InlineFrame()
      : this(null, null, null)
    {
    }

    public InlineFrame(Uri sourceUri)
      : this(sourceUri, null, null)
    {
    }

    public InlineFrame(Uri sourceUri, string width, string height)
    {
      this.SourceUri = sourceUri;
      this.Width = width;
      this.Height = height;
      this.Scrolling = false;
      this.FrameBorder = false;
    }

    private Dictionary<string, string> attributes = new Dictionary<string, string>();
  }
}
