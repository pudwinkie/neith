// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
using System.Windows.Forms;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.Forms {
  /// <remarks>this class is based on SantaMarta.Applications.Usamimi.UI.TextBoxes.QueryTextBox</remarks>
  public class QueryTextBox : EditTextBox {
    public bool AutoHighlight {
      get; set;
    }

    public QueryTextBox()
    {
      this.AutoHighlight = true;
    }

    protected override void OnCanceled(EventArgs e)
    {
      Text = string.Empty;

      base.OnCanceled(e);
    }

    protected override void OnEnter(EventArgs e)
    {
      base.OnEnter(e);

      if (AutoHighlight && 0 == SelectionLength)
        SelectAll();
    }

    protected override void OnClick(EventArgs e)
    {
      if (AutoHighlight && 0 == SelectionLength)
        SelectAll();

      base.OnClick(e);
    }
  }
}
