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
using System.ComponentModel;
using System.Windows.Forms;

using Smdn.Windows.UserInterfaces.Interop;

namespace Smdn.Windows.Forms {
  public class EditableListView : ListView {
    public EditableListView()
    {
      View = View.Details;
      FullRowSelect = true;
      MultiSelect = false;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (components != null)
          components.Dispose();
      }

      base.Dispose(disposing);
    }

    private bool styleChangedLock = false;

    protected override void OnStyleChanged(EventArgs e)
    {
      if (!styleChangedLock) {
        try {
          styleChangedLock = true;

          View = View.Details;
        }
        finally {
          styleChangedLock = false;
        }
      }

      base.OnStyleChanged(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      if ((e.Button & MouseButtons.Left) != MouseButtons.None) {
        if (0 <= e.X && 0 <= e.Y) {
          var hitTestInfo = HitTest(e.Location);

          if (hitTestInfo.Item != null && hitTestInfo.Item.Selected && hitTestInfo.SubItem != null)
            EditSubItem(hitTestInfo.Item, hitTestInfo.SubItem);
        }
      }
      else {
        Focus();
      }

      base.OnMouseClick(e);
    }

    protected virtual void EditSubItem(ListViewItem item, ListViewItem.ListViewSubItem subItem)
    {
      if (doneEditor != null) {
        doneEditor.Dispose();
        components.Remove(doneEditor);

        doneEditor = null;
      }

      activeEditor = CreateSubItemEditor(item, subItem);

      if (activeEditor != null) {
        components.Add(activeEditor);

        activeEditor.EditDone += Editor_EditDone;
        activeEditor.Edit();
      }
    }

    protected virtual ListViewSubItemEditorBase CreateSubItemEditor(ListViewItem item, ListViewItem.ListViewSubItem subItem)
    {
      return new TextBoxListViewSubItemEditor(item, subItem);
    }

    private void Editor_EditDone(object sender, EventArgs e)
    {
      activeEditor = null;

      var editor = sender as ListViewSubItemEditorBase;

      editor.EditDone -= Editor_EditDone;

      // this will cause AccessViolationException, so call Dispose later
      //editor.Dispose();
      //components.Remove(editor);

      doneEditor = editor;

      Focus();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
      if (e.Delta != 0 && activeEditor != null) {
        activeEditor.UpdateEditorBounds();
      }
      else {
        base.OnMouseWheel(e);
      }
    }

    protected override void WndProc(ref Message m)
    {
      base.WndProc(ref m);

      switch ((WM)m.Msg) {
        case WM.HSCROLL:
        case WM.VSCROLL:
          if (activeEditor != null)
            activeEditor.UpdateEditorBounds();
          break;
      }
    }

    private Container components = new Container();
    private ListViewSubItemEditorBase activeEditor = null;
    private ListViewSubItemEditorBase doneEditor = null;
  }
}
