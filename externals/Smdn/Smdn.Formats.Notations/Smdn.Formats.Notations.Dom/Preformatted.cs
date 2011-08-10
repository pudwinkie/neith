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

namespace Smdn.Formats.Notations.Dom {
  public class Preformatted : Node {
    public bool IsEmpty {
      get { return preformatted.Length == 0; }
    }

    public virtual bool NeedEscape {
      get; private set;
    }

    public Preformatted()
      : this(true)
    {
    }

    public Preformatted(bool needEscape)
    {
      this.NeedEscape = needEscape;
    }

    public Preformatted(string val)
      : this(val, true)
    {
    }

    public Preformatted(string val, bool needEscape)
    {
      if (val == null)
        throw new ArgumentNullException("val");

      this.NeedEscape = needEscape;

      preformatted.Append(val);
    }

    public void Append(params string[] texts)
    {
      foreach (var text in texts) {
        preformatted.Append(text);
      }
    }

    public override string ToString()
    {
      return preformatted.ToString();
    }

    private StringBuilder preformatted = new StringBuilder();
  }
}
