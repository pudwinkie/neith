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

namespace Smdn.Formats.Notations {
  public abstract class Notation {
    public string Name {
      get { return name; }
    }

    protected Notation(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      this.name = name;
    }

    public virtual Parser CreateParser()
    {
      return CreateParser(new Dictionary<string, string>());
    }

    public abstract Parser CreateParser(Dictionary<string, string> options);

    public virtual TextFormatter CreateTextFormatter()
    {
      return CreateTextFormatter(new Dictionary<string, string>());
    }

    public abstract TextFormatter CreateTextFormatter(Dictionary<string, string> options);

    public virtual XhtmlFormatter CreateXhtmlFormatter()
    {
      return CreateXhtmlFormatter(new Dictionary<string, string>());
    }

    public abstract XhtmlFormatter CreateXhtmlFormatter(Dictionary<string, string> options);

    private string name;
  }
}
