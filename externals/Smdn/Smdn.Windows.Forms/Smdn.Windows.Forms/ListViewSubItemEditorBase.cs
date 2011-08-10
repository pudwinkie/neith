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
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.Forms {
  public abstract class ListViewSubItemEditorBase : Control {
    public EventHandler EditEntered;
    public EventHandler EditCanceled;
    public EventHandler EditLeave;
    public EventHandler EditDone;

    public ListViewItem Item {
      get; private set;
    }

    public ListViewItem.ListViewSubItem SubItem {
      get; private set;
    }

    public ListViewSubItemEditorBase(ListViewItem item, ListViewItem.ListViewSubItem subItem)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (subItem == null)
        throw new ArgumentNullException("subItem");

      this.Item = item;
      this.SubItem = subItem;
    }

    public void Edit()
    {
      Parent = Item.ListView;

      editControl = BeginEdit();

      if (editControl != null)
        editControl.Parent = this;

      // XXX: this causes unexpected scrolling on windows
      // Item.ListView.EnsureVisible(Item.Index);

      UpdateEditorBounds();

      BringToFront();
      Show();

      if (editControl == null)
        Focus();
      else
        editControl.Focus();

      Leave += OnEditLeave;
    }

    protected abstract Control BeginEdit();

    protected virtual void EndEdit()
    {
      Leave -= OnEditLeave; // remove handler first

      if (editControl != null) {
        editControl.Parent = null;
        editControl = null;
      }

      Parent = null;
      Hide();

      OnEditDone(EventArgs.Empty);
    }

    protected internal virtual void UpdateEditorBounds()
    {
      Bounds = SubItem.Bounds;

      if (Item.SubItems.IndexOf(SubItem) == 0)
        Width = Item.ListView.Columns[0].Width;

      if (editControl != null) {
        editControl.Bounds = new Rectangle(Point.Empty, ClientSize);

        if (Height < editControl.Height)
          Height = editControl.Height;
      }
    }

    protected override bool ProcessCmdKey(ref Message m, Keys keyData)
    {
      if ((WM)m.Msg == WM.KEYDOWN) {
        switch (keyData & Keys.KeyCode) {
          case Keys.Enter: OnEditEntered(); return true;
          case Keys.Escape: OnEditCanceled(); return true;
          default:
            break;
        }
      }

      return base.ProcessCmdKey(ref m, keyData);
    }

    private void OnEditEntered()
    {
      OnEditEntered(EventArgs.Empty);

      EndEdit();
    }

    protected virtual void OnEditEntered(EventArgs e)
    {
      var ev = this.EditEntered;

      if (ev != null)
        ev(this, e);
    }

    private void OnEditCanceled()
    {
      OnEditCanceled(EventArgs.Empty);

      EndEdit();
    }

    protected virtual void OnEditCanceled(EventArgs e)
    {
      var ev = this.EditCanceled;

      if (ev != null)
        ev(this, e);
    }

    private void OnEditLeave(object sender, EventArgs e)
    {
      OnEditLeave(EventArgs.Empty);

      EndEdit();
    }

    protected virtual void OnEditLeave(EventArgs e)
    {
      var ev = this.EditLeave;

      if (ev != null)
        ev(this, e);
    }

    protected virtual void OnEditDone(EventArgs e)
    {
      var ev = this.EditDone;

      if (ev != null)
        ev(this, e);
    }

    private Control editControl = null;
  }
}
