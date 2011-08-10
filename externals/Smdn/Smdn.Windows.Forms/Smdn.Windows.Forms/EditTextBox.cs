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
  public class EditTextBox : TextBox {
    public event EventHandler Entered;
    public event EventHandler Canceled;

    protected override bool ProcessCmdKey(ref Message m, Keys keyData)
    {
      if ((WM)m.Msg == WM.KEYDOWN) {
        switch (keyData & Keys.KeyCode) {
          case Keys.Enter: OnEntered(EventArgs.Empty); return true;
          case Keys.Escape: OnCanceled(EventArgs.Empty); return true;
          default:
            break;
        }
      }

      return base.ProcessCmdKey(ref m, keyData);
    }

    protected virtual void OnEntered(EventArgs e)
    {
      var ev = this.Entered;

      if (ev != null)
        ev(this, e);
    }

    protected virtual void OnCanceled(EventArgs e)
    {
      var ev = this.Canceled;

      if (ev != null)
        ev(this, e);
    }
  }
}
